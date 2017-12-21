using System.Threading.Tasks;

namespace just4net.thread
{
    public abstract class IPoolTask
    {
        protected Task myTask;
        protected string key;
        

        protected IPoolTask(string key)
        {
            this.key = key;
        }


        public void AttachThread(Task attachThread)
        {
            myTask = attachThread;
        }


        public string Key { get { return key; } }


        /// <summary>
        /// Will be called while task fetched.
        /// </summary>
        public abstract void PreRun();


        /// <summary>
        /// The main content of task's running.
        /// </summary>
        public abstract void Run();


        /// <summary>
        /// Called when the task is canceled.
        /// </summary>
        public abstract void Cancel();
    }


    public interface IPoolTaskGenerator<out T> where T : IPoolTask
    {
        T Generate();
    }
}
