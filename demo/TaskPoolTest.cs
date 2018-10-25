using just4net.thread;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace demo
{
    public class TaskPoolTest
    {

        public void Run()
        {
            TaskPool pool = TaskPool.GetInstance(new Generator(), 5);
            while (true)
            {
                string str = Console.ReadLine();
                if (str == "p")
                    pool.Pause();
                else if (str == "r")
                    pool.Resume();
                else if (str == "stop")
                    break;
                else if (str == "pa")
                {
                    Task<bool> task = pool.PauseAysnc();
                    Console.WriteLine("PAUSE ASYNC");
                    bool result = task.Result;
                    Console.WriteLine("PAUSED");
                }
                else
                {
                    try
                    {
                        int n = Convert.ToInt32(str);
                        Generator.MAX = n;
                    }
                    catch { }
                }
            }
        }
    }

    public class MyTask : IPoolTask
    {
        public static object obj = new object();

        public MyTask(string key) : base(key)
        {
        }


        public override void Cancel()
        {
            Console.WriteLine($"Canceled - {Thread.CurrentThread.ManagedThreadId}: {key}");
        }


        public override void PreRun()
        {
            Console.WriteLine($"PRE - {Thread.CurrentThread.ManagedThreadId}: {key}");
        }


        public override void PostRun()
        {

        }


        public override void Run()
        {

            int top;
            string str;
            lock (obj)
            {
                Console.WriteLine();
                top = Console.CursorTop;
                str = $"RUN - {key}: ";
                Console.SetCursorPosition(0, top);
                Console.WriteLine(str);
            }
            
            for (int i = 0; i < 110; i++)
            {
                if (i % 10 == 0)
                {
                    lock (obj)
                    {
                        Console.SetCursorPosition(str.Length, top);
                        Console.Write($"{i}%");
                    }
                }
                Thread.Sleep(100);
            }
            Console.WriteLine();

        }
    }

    public class Generator : IPoolTaskGenerator<MyTask>
    {
        private static int id = 0;
        public static int MAX = 0;

        public MyTask Generate()
        {
            if (id < MAX)
                return new MyTask(Convert.ToString(id++));
            else
                return null;
        }
    }
}
