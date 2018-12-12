using just4net.reflect.aop;
using System;
using System.Threading;

namespace demo.aop
{
    public class AOPTest
    {
        public void Test()
        {
            Console.WriteLine();
            AOPCalc calc = new AOPCalc();
            // Test normal track.
            calc.Divide(5, 2);
            Console.WriteLine();
            Console.WriteLine();

            // Test exception catch.
            calc.Divide(1, 0);
            Console.WriteLine();
            Console.WriteLine();

            // Extra test.
            calc.Divide(2, 1);
        }
    }

    [AOPContext]
    public class AOPCalc : ContextBoundObject
    {
        [Track, ExceptionRecord(0), ExceptionAlert(0)]
        public int Divide(int a, int b)
        {
            Thread.Sleep(100);
            return a / b;
        }
    }
}
