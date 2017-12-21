using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace just4net.thread
{
    public class TaskBox
    {
        private static TaskBox instance;

        private static readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private static readonly ReaderWriterLockSlim runningLocker = new ReaderWriterLockSlim();
        private static AutoResetEvent run = new AutoResetEvent(true);

        private Queue<IPoolTask> remains;
        private Dictionary<string, IPoolTask> running;
        private int maxThreadCount;

        private bool isPreRunOne, isPostRunOne;

        private TaskPoolState toState; // the state task box want to go.
        private TaskPoolState currentState; // current state of task box.



        public void AddTask(IPoolTask task)
        {
            locker.EnterWriteLock();
            remains.Enqueue(task);
            locker.ExitWriteLock();
            //run.Set();
        }


        /// <summary>
        /// Get the singleton instance.
        /// </summary>
        /// <param name="maxThreadCount"></param>
        /// <returns></returns>
        public static TaskBox GetInstance(int maxThreadCount, bool isPreRunOne = true, bool isPostRunOne = true)
        {
            if (instance == null)
            {
                locker.EnterWriteLock();
                if (instance == null)
                    instance = new TaskBox(maxThreadCount, isPreRunOne, isPostRunOne);
                locker.ExitWriteLock();
            }

            return instance;
        }


        private TaskBox(int maxThreadCount, bool isPreRunOne, bool isPostRunOne)
        {
            this.maxThreadCount = maxThreadCount;
            this.isPreRunOne = isPreRunOne;
            this.isPostRunOne = isPostRunOne;

            remains = new Queue<IPoolTask>();
            running = new Dictionary<string, IPoolTask>();
            toState = TaskPoolState.RUN;
            currentState = TaskPoolState.PAUSE;

            new Thread(Main).Start();
        }


        /// <summary>
        /// Try to pause task box until no running tasks.
        /// </summary>
        /// <param name="waitInterval"></param>
        /// <returns></returns>
        public bool Pause(int waitInterval = 1000)
        {
            /// set toState to pause.
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
        /// Resume task box to run tasks.
        /// </summary>
        public void Resume()
        {
            locker.EnterWriteLock();
            toState = TaskPoolState.RUN;
            locker.ExitWriteLock();
            run.Reset();
            run.Set();
        }


        /// <summary>
        /// The main thread to dispach task and create threads.
        /// </summary>
        private void Main()
        {
            while (toState != TaskPoolState.STOP)
            {
                run.WaitOne();

                int count = 0;
                while (true)
                {
                    runningLocker.EnterReadLock();
                    count = running.Count;
                    runningLocker.ExitReadLock();

                    if (count >= maxThreadCount)
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
        /// Add a thread to run tasks.
        /// </summary>
        /// <returns></returns>
        private bool AddThread()
        {
            int count = 0;
            IPoolTask task = FetchTask();
            if (task == null)
                return false;

            Task thread = null;

            TaskPoolState m_toState;
            thread = new Task(() =>
            {
                while (true)
                {
                    if (task != null)
                    {
                        locker.EnterReadLock();
                        m_toState = toState;
                        locker.ExitReadLock();

                        if (m_toState == TaskPoolState.RUN)
                        {
                            task.Run();
                            RemoveRunning(task);
                            task = null;
                             
                            /// When finished a task, check <see cref="toState"/>.
                            locker.EnterReadLock();
                            m_toState = toState;
                            locker.ExitReadLock();
                            if (m_toState == TaskPoolState.RUN)
                            {
                                task = FetchTask();
                                if (task != null)
                                    AddRunning(task, thread);
                            }
                        }
                        else
                        {
                            task.Cancel();
                            RemoveRunning(task, false);
                            task = null;
                        }
                    }

                    // When the intent is not to run, then cancel current task, 
                    if (task == null)
                    {
                        runningLocker.EnterReadLock(); 
                        count = running.Count;
                        runningLocker.ExitReadLock();
                        if (count == 0)
                        {
                            locker.EnterWriteLock();
                            currentState = TaskPoolState.PAUSE;
                            locker.ExitWriteLock();
                        }
                        break;
                    }
                }
            });
            AddRunning(task, thread);
            thread.Start();
            return true;
        }


        /// <summary>
        /// Add task to running cache.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="attachThread"></param>
        private void AddRunning(IPoolTask task, Task attachThread)
        {
            runningLocker.EnterWriteLock();
            task.AttachThread(attachThread);
            running.Add(task.Key, task);
            runningLocker.ExitWriteLock();
        }


        /// <summary>
        /// Remove task from running cache.
        /// </summary>
        /// <param name="task"></param>
        private void RemoveRunning(IPoolTask task, bool postRun = true)
        {
            if (!isPostRunOne && postRun)
                task.PostRun();

            runningLocker.EnterWriteLock();
            if (isPostRunOne && postRun)
                task.PostRun();
            running.Remove(task.Key);
            runningLocker.ExitWriteLock();
        }


        private IPoolTask FetchTask()
        {
            IPoolTask task;
            locker.EnterWriteLock();
            if (remains.Count == 0)
                task = null;
            else
                task = remains.Dequeue();

            // While task in not null and only one pre-runs in the same time, pre-run it.
            if (task != null && isPreRunOne)
                task.PreRun();
            locker.ExitWriteLock();

            // If set to more than one can pre-run in the same time,
            // pre-run it outside the locker.
            if (!isPreRunOne && task != null)
                task.PreRun();

            return task;
        }

    }
}
