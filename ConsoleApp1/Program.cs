using System;
using System.IO;

/// <summary>
/// After an exception is thrown, the runtime checks the current statement to see whether it is within a try block.
/// If it is, any catch blocks associated with the try block are checked to see whether they can catch the exception. 
/// Catch blocks typically specify exception types; if the type of the catch block is the same type as the exception, or a base class of the exception, 
/// the catch block can handle the method.
///
/// If the statement that throws an exception is not within a try block or if the try block that encloses it has no matching catch block, the runtime checks the calling method for a try statement and catch blocks. 
/// The runtime continues up the calling stack, searching for a compatible catch block. 
/// After the catch block is found and executed, control is passed to the next statement after that catch block.
/// A try statement can contain more than one catch block.The first catch statement that can handle the exception is executed; 
/// any following catch statements, even if they are compatible, are ignored.
/// Therefore, catch blocks should always be ordered from most specific (or most-derived) to least specific.
/// 
/// If no catch block is found, one of 3 things happen
/// a) If there is a finalizer, the finalizer is aborted and the base finalizer is called
/// b) If the call stack contains a static constructor, or a static field initializer, a TypeInitializationException is thrown
/// c) If the start of a thread is reached, the thread is terminated
/// 
/// Throwing Exceptions is expensive do don't do it as a means to control logic flow
/// 
/// </summary>
namespace ConsoleApp1
{
    class Program
    {
        static double SafeDivision(double x, double y)
        {
            if (y == 0)
                throw new DivideByZeroException();
            return (x / y);
        }

        static void Main(string[] args)
        {
            double a = 98, b = 0;
            double result = 0;

            // So, this first block catches and handles any divide by zero errors.
            // If this try block wasn't here, you'd get an unhandled exception thrown
            // INTERESTING NOTE: dividing by 0 for a float/double type doesn't natively throw a divide by zero exception - it just returns NaN/Infinity
            try
            {
                result = SafeDivision(a, b);
                Console.WriteLine($"{a} divided by {b} = {result}");
            }
            catch(DivideByZeroException e)
            {
                Console.WriteLine($"Attempted divide by zero.");
            }

            // Now lets try out that Custom Exception we created below
            CustomException custex = new CustomException("Custom Exception");
            // throw custex;

            try
            {
                Console.WriteLine($"Do something");
            }
            catch(CustomException custExc) when (custExc.Status == 500)
            {
                Console.WriteLine($"Handle something very specific");
            }
            catch(CustomException custExcGeneric)
            {
                Console.WriteLine($"Handle a generic error");
            }

            // Always order your catch blocks from most specific to least specific
            try
            {
                using (var sw = new StreamWriter(@"C:\test\test.txt"))
                {
                    sw.WriteLine("Hello");
                }
            }
            // Put the more specific exceptions first.
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine(ex);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex);
            }
            // Put the least specific exception last.
            catch (IOException ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Done");

            // Now lets test the Finally block
            TestFinally();

            // Trick! This actually won't work because .Net Core! You can do GC.Collect() but that only works in Release mode anyway
            var myTest = new TestingFinalziers();
            var myTest2 = new TestingFinalziers();

            // Nesting exceptions
            try
            {
                try
                {
                    // So, this one will hit this inner most exception
                    // throw new DivideByZeroException();

                    // This one will hit the more generic out outside
                    throw new ArithmeticException();
                }
                catch(DivideByZeroException e)
                {
                    Console.WriteLine("Oops, divide by zero");
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Something bad happened");
            }
        }

        static void TestFinally()
        {
            System.IO.FileStream file = null;
            //Change the path to something that works on your machine.
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(@"C:\file.txt");

            try
            {
                file = fileInfo.OpenWrite();
                // So, even if this failed - the finally block would close the file to allow the subsequent try to run
                file.WriteByte(0xF);
            }
            finally
            {
                // Closing the file allows you to reopen it immediately - otherwise IOException is thrown.
                if (file != null)
                {
                    file.Close();
                }
            }

            try
            {
                file = fileInfo.OpenWrite();
                System.Console.WriteLine("OpenWrite() succeeded");
            }
            catch (System.IO.IOException)
            {
                System.Console.WriteLine("OpenWrite() failed");
            }
        }
    }

    public class CustomException : Exception
    {
        public CustomException() : base() { }
        public CustomException(string customMessage) : base(customMessage) { }
        public CustomException(string customMessage, Exception inner) : base(customMessage, inner) { }

        public int Status { get; set; }
    }

    public class TestingFinalziers
    {
        public TestingFinalziers()
        {
            Console.WriteLine("Starting...");
        }
        ~TestingFinalziers()
        {
            Console.WriteLine("Ending...");
        }
    }
}
