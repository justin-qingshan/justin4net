using System;

namespace just4net.timer
{
    public abstract class ITask
    {
        protected static object locker = new object();

        private bool enable = false;
        private DateTime nextTime = default(DateTime);
        private DateTime lastTime = default(DateTime);


        public ITask(DateTime lastTime = default(DateTime), DateTime nextTime = default(DateTime), bool enable = true)
        {
            this.enable = enable;
            this.lastTime = lastTime == default(DateTime) ? GenerateLastTime() : lastTime;
            this.nextTime = nextTime == default(DateTime) ? GenerateNextTime(this.lastTime) : nextTime;
        }


        /// <summary>
        /// Getter for the unique name(as identity) of this task.
        /// </summary>
        public abstract string Name { get; }


        /// <summary>
        /// The main part of this task to execute(every task will run in a new thread).
        /// </summary>
        /// <param name="now"></param>
        public abstract void Run(DateTime now);


        /// <summary>
        /// Generate next time to run.
        /// </summary>
        /// <param name="lastTime">Value of GenerateLastTime() when initial or actually last running time.</param>
        /// <returns></returns>
        public abstract DateTime GenerateNextTime(DateTime lastTime);


        /// <summary>
        /// To generate last time of running(Called when there is no record).
        /// </summary>
        /// <returns></returns>
        public abstract DateTime GenerateLastTime();


        public DateTime LastTime
        {
            get { lock (locker) { return lastTime; } }
            set { lock (locker) { lastTime = value; } }
        }

        
        public DateTime NextTime
        {
            get { lock (locker) { return nextTime; } }
            set { lock (locker) { nextTime = value; } }
        }


        public bool Enable
        {
            get { lock (locker) { return enable; } }
            set { lock(locker) { enable = value; } }
        }
    }
}
