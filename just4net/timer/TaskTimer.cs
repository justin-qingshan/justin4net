using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace just4net.timer
{
    public class TaskTimer
    {
        private static object locker = new object();
        private static TaskTimer instance;
        private static bool willCacheTime;
        

        private Dictionary<string, ITask> tasks = new Dictionary<string, ITask>();
        private Timer timer;

        
        /// <summary>
        /// Get the singleton instance.
        /// </summary>
        /// <returns></returns>
        public static TaskTimer GetInstance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                        instance = new TaskTimer();
                }
            }

            return instance;
        }


        /// <summary>
        /// Add a task to timer.
        /// </summary>
        /// <param name="task"></param>
        public void Add(ITask task)
        {
            lock (locker)
            {
                tasks.Add(task.Name, task);
            }

            Run(DateTime.Now);
        }


        /// <summary>
        /// Remove a task from timer.
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            lock (locker)
            {
                tasks.Remove(name);
            }
        }


        /// <summary>
        /// Get task by its <see cref="ITask.Name"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ITask Get(string name)
        {
            ITask task = null;
            lock (locker)
            {
                tasks.TryGetValue(name, out task);
            }
            return task;
        }


        /// <summary>
        /// Get task by its <see cref="ITask.Name"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ITask this[string name]
        {
            get { return Get(name); }
        }


        /// <summary>
        /// Setter and getter of whether timer will cache task execute time.
        /// <para></para>
        /// When need to set to true, It's necessary to set <see cref="TaskTimeCache.CacheFolder"/> firstly.
        /// </summary>
        public bool WillCacheTime
        {
            get { return willCacheTime; }
            set { willCacheTime = value; }
        }


        private TaskTimer()
        {
            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(Run);
        }
        

        private void Run(object source, ElapsedEventArgs e)
        {
            Run(e.SignalTime);
        }


        private void Run(DateTime time)
        {
            DateTime now = time;
            // pause time temporarily.
            timer.Enabled = false;

            // run tasks.
            // find the min interval to run next task.
            int minInterval = int.MaxValue;
            lock (locker)
            {
                foreach (ITask task in tasks.Values)
                {
                    // ignore when task isn't enable or next time is illegal or not the time.
                    if (!task.Enable || task.NextTime == default(DateTime))
                        continue;

                    // if it's not the time to run this task, calculat the interval.
                    int interval;
                    if (task.NextTime > now)
                    {
                        interval = (int)(task.NextTime - now).TotalMilliseconds;
                        minInterval = interval < minInterval ? interval : minInterval;
                        continue;
                    }

                    // if run this task, then create a thread to run it and generate time of next running.
                    task.LastTime = now;
                    task.NextTime = task.GenerateNextTime(now);
                    new Task(() =>
                    {
                        task.Run(now);
                    }).Start();

                    // calculate the min interval.
                    interval = (int)(task.NextTime - now).TotalMilliseconds;
                    minInterval = interval < minInterval ? interval : minInterval;

                    if (willCacheTime)
                        TaskTimeCache.GetInstance().Update(task.Name, now, task.NextTime);
                }
            }
            
            timer.Interval = minInterval;
            timer.Enabled = true;
        }
    }
}
