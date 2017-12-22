using just4net.thread;
using System;
using System.Threading.Tasks;

namespace demo
{
    class TaskBoxTest
    {
        static int i = 0;

        public void Run()
        {
            TaskBox box = TaskBox.GetInstance(5);

            while (true)
            {
                string str = Console.ReadLine();
                if (str == "p")
                    box.Pause();
                else if (str == "r")
                    box.Resume();
                else if (str == "stop")
                    break;
                else if (str == "pa")
                {
                    Task<bool> task = box.PauseAysnc();
                    Console.WriteLine("PAUSE ASYNC");
                    bool result = task.Result;
                    Console.WriteLine("PAUSED");
                }
                else
                {
                    box.AddTask(new MyTask(Convert.ToString(i++)));
                }
            }

            return;
        }
    }
}
