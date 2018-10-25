using System.Threading;
using System.Threading.Tasks;

namespace just4net.thread
{
    public class TaskFetchThread
    {
        private IPoolTask task = null;
        private bool isFetched = false;

        private bool willFetch = false;
        private IPoolTaskGenerator<IPoolTask> taskGenerator;
        private AutoResetEvent run = new AutoResetEvent(false);

        private static object locker = new object();


        public TaskFetchThread(IPoolTaskGenerator<IPoolTask> taskGenerator)
        {
            this.taskGenerator = taskGenerator;
        }


        public void Run()
        {
            while (true)
            {
                run.WaitOne();

                lock (locker)
                {
                    if (willFetch)
                    {
                        task = taskGenerator.Generate();
                        if (task != null)
                            task.PreRun();
                        willFetch = false;
                        isFetched = true;
                    }
                }
            }
        }


        public async Task<IPoolTask> FetchNewTaskAsync()
        {
            return await Task.Run(() =>
            {
                return FetchNewTask();
            });
        }


        public IPoolTask FetchNewTask()
        {
            lock (locker)
            {
                willFetch = true;
            }
            run.Set();

            IPoolTask current;
            while (true)
            {
                lock (locker)
                {
                    if (isFetched)
                    {
                        current = task;
                        task = null;
                        isFetched = false;
                        break;
                    }
                }
                Thread.Sleep(10);
            }

            return current;
        }
    }
}
