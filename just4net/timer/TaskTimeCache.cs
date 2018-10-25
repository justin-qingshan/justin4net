using just4net.serialize;
using System;
using System.Collections.Generic;
using System.IO;

namespace just4net.timer
{
    public class TaskTimeCache
    {
        private const string cacheFileName = "task_time.cache.json";

        private static object locker = new object();
        private static TaskTimeCache instance;

        private Dictionary<string, TaskTime> dic = new Dictionary<string, TaskTime>();
        private static string cacheFolder;


        /// <summary>
        /// Get the singleton instance.
        /// </summary>
        /// <returns></returns>
        public static TaskTimeCache GetInstance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                        instance = new TaskTimeCache();
                }
            }

            return instance;
        }


        /// <summary>
        /// Update the time.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lastTime"></param>
        /// <param name="nextTime"></param>
        public void Update(string name, DateTime lastTime, DateTime nextTime)
        {
            lock (locker)
            {
                if (dic.ContainsKey(name))
                {
                    TaskTime time = dic[name];
                    time.LastTime = lastTime;
                    time.NextTime = nextTime;
                }
                else
                {
                    dic.Add(name, new TaskTime { Name = name, LastTime = lastTime, NextTime = nextTime });
                }
                Save();
            }
        }


        /// <summary>
        /// Get <see cref="TaskTime"/> By its name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TaskTime this[string name]
        {
            get
            {
                if (dic.ContainsKey(name))
                    return dic[name];
                return null;
            }
        }


        /// <summary>
        /// Setter and getter of Cache folder.
        /// <para></para>
        /// Setter must be called firstly when using TaskTimeCache.
        /// </summary>
        public static string CacheFolder
        {
            get { return cacheFolder; }
            set { cacheFolder = value; }
        }

        
        private TaskTimeCache()
        {
            Read();
        }


        private void Save()
        {
            if (string.IsNullOrEmpty(CacheFolder))
                return;

            if (!Directory.Exists(cacheFolder))
                return;

            string file = Path.Combine(cacheFolder, cacheFileName);
            JsonUtil.SerializeIntoFile(dic.Values, file);
        }


        private void Read()
        {
            dic.Clear();
            if (string.IsNullOrEmpty(CacheFolder))
                return;

            string file = Path.Combine(cacheFolder, cacheFileName);
            if (!File.Exists(file))
                return;

            List<TaskTime> list = null;
            try
            {
                list = JsonUtil.DeserializeFromFile<List<TaskTime>>(file);
            }
            catch { }

            if (list == null)
                return;

            foreach (TaskTime task in list)
                dic.Add(task.Name, task);
        }
        
    }


    public class TaskTime
    {
        public string Name;
        public DateTime LastTime;
        public DateTime NextTime;
    }
}
