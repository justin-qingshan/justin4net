using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace just4net.thread
{
    public class TaskPool
    {
        private static TaskPool instance;
        private static ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private AutoResetEvent run = new AutoResetEvent(true);

        private TaskPoolState currentState; //Indicate the current state of pool.
        private TaskPoolState toState; //Indicate whether pool will accept new task to run in next step.

        private int maxThreadCount;

        private TaskFetchThread taskFetch;
        private Thread mainThread;

        private Dictionary<string, IPoolTask> running = new Dictionary<string, IPoolTask>();


        public static TaskPool GetInstance(IPoolTaskGenerator<IPoolTask> taskGenerator, int maxThreadCount)
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                        instance = new TaskPool(taskGenerator, maxThreadCount);
                }
            }

            return instance;
        }


        private TaskPool(IPoolTaskGenerator<IPoolTask> taskGenerator, int maxThreadCount)
        {
            toState = TaskPoolState.RUN;
            currentState = TaskPoolState.PAUSE;
            this.maxThreadCount = maxThreadCount;

            /// Start a thread to fetch <see cref="IPoolTask"/>.
            taskFetch = new TaskFetchThread(taskGenerator);
            Task thread = new Task(taskFetch.Run);
            thread.Start();

            mainThread = new Thread(Main);
            mainThread.Start();
        }



        public bool Pause(int waitInterval = 1000)
        {
            locker.EnterWriteLock();
            if (toState == TaskPoolState.RUN)
                toState = TaskPoolState.PAUSE;
            locker.ExitWriteLock();

            TaskPoolState state;
            while (true)
            {
                locker.EnterReadLock();
                state = currentState;
                locker.ExitReadLock();
                if (state != TaskPoolState.RUN)
                    return true;

                Thread.Sleep(waitInterval);
            }
        }


        public Task<bool> PauseAysnc(int waitInterval = 1000)
        {
            return Task.Run(() =>
            {
                return Pause(waitInterval);
            });
        }


        /// <summary>
        /// Resume the task pool to fetch new tasks and run.
        /// </summary>
        public void Resume()
        {
            locker.EnterWriteLock();
            toState = TaskPoolState.RUN;
            locker.ExitWriteLock();
            run.Set();
        }


        /// <summary>
        /// Then main running part of main thread.
        /// <para></para>
        /// Main thread creates thread to run tasks, and manage the threads' count.
        /// </summary>
        private void Main()
        {
            while(toState != TaskPoolState.STOP)
            {
                run.WaitOne();
                int count = 0;
                while (true)
                {
                    locker.EnterReadLock();
                    count = running.Count;
                    locker.ExitReadLock();

                    if (count >= maxThreadCount || toState != TaskPoolState.RUN)
                        break;

                    if (!AddThread())
                        break;
                }

                locker.EnterWriteLock();
                if (count == 0)
                    currentState = TaskPoolState.PAUSE;
                else
                    currentState = TaskPoolState.RUN;
                locker.ExitWriteLock();
            }
        }


        /// <summary>
        /// Create a thread, and fetch a task to run in it.
        /// </summary>
        /// <returns></returns>
        private bool AddThread()
        {
            if (toState != TaskPoolState.RUN)
                return false;
            
            IPoolTask task = taskFetch.FetchNewTask();
            if (task == null)
                return false;

            Task thread = null;
            thread = new Task(() =>
            {
                while (true)
                {
                    task.Run();

                    /// When current task is finished, remove it from <see cref="running"/>.
                    locker.EnterWriteLock();
                    running.Remove(task.Key);
                    locker.ExitWriteLock();

                    /// If pool will run, then fetch a new task and add it to <see cref="running"/>
                    if (toState == TaskPoolState.RUN)
                    {
                        task = taskFetch.FetchNewTask();
                        if (task != null)
                        {
                            locker.EnterWriteLock();
                            task.AttachThread(thread);
                            running.Add(task.Key, task);
                            locker.ExitWriteLock();
                        }
                    }
                    else
                    {
                        task = null;
                    }

                    /// When this thread are running to end, get count of running tasks.
                    /// If count equals 0, it means there is no running task.
                    /// Then set current state to <see cref="TaskPoolState.PAUSE"/>
                    if (task == null)
                    {
                        int count = 0;
                        locker.EnterUpgradeableReadLock();
                        count = running.Count;
                        if (count == 0)
                        {
                            locker.EnterWriteLock();
                            currentState = TaskPoolState.PAUSE;
                            locker.ExitWriteLock();
                        }
                        locker.ExitUpgradeableReadLock();
                        break;
                    }
                }
            });
            task.AttachThread(thread);

            locker.EnterWriteLock();
            running.Add(task.Key, task);
            locker.ExitWriteLock();
            thread.Start();
            return true;
        }
    }
    
}
