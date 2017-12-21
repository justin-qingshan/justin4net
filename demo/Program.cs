using System;
using System.IO;
using just4net.io;
using System.Collections.Generic;
using just4net.collection;
using just4net.timer;
using System.Threading;
using just4net.util;

namespace demo
{
    class Program
    {
        static void Main(string[] args)
        {
            //new TaskPoolTest().Run();
            new TaskBoxTest().Run();
            
        }


        static void TestConfigUtil()
        {
            string value1 = ConfigUtil.Settings["key1"];
            Console.WriteLine($"key1 = {value1}");

            Console.Write($"Please input value: ");
            string str = Console.ReadLine();
            ConfigUtil util = new ConfigUtil(ConfigType.EXE);
            util.AddAppSetting("key1", str);
            util.Save();
            value1 = util.AppSettings["key1"];
            Console.WriteLine($"key1 = {value1}");
            value1 = ConfigUtil.Settings["key1"];
            Console.WriteLine($"key1 = {value1}");
        }


        static void TestTimer()
        {
            TaskTimer timer = TaskTimer.GetInstance();
            Console.WriteLine("START");
            Thread.Sleep(5000);
            Console.WriteLine("ADD TIMER");
            timer.Add(new PrintTask());
        }



        static void TestPath()
        {
            string path = @"D:\GIT\just4net1\just4net\io";
            string name = Path.GetFileName(path);
            string nameWithoutEx = Path.GetFileNameWithoutExtension(path);
            string dirName = Path.GetDirectoryName(path);

            Console.WriteLine(path);
            Console.WriteLine();
            Console.WriteLine(name);
            Console.WriteLine(nameWithoutEx);
            Console.WriteLine(dirName);
        }
        
        static void TestMD5()
        {
            string path = @"D:\GIT\justin4net\README.md";
            string md5 = FileUtil.MD5(path);
            string sha1 = FileUtil.SHA1(path);

            Console.WriteLine(path);
            Console.WriteLine("MD5: {0}", md5);
            Console.WriteLine("SHA1: {0}", sha1);
        }


        static void TestCollection()
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            dic.Add(1, "first");
            dic.Add(2, "second");
            dic.Add(3, "third");

            Console.WriteLine("Keys: ");
            dic.TraveralKeys(key => { Console.WriteLine(key); });
            Console.WriteLine("Values: ");
            dic.TraveralValues(value => { Console.WriteLine(value); });
        }

    }

    public class PrintTask : ITask
    {
        public override string Name
        {
            get
            {
                return "PRINT";
            }
        }

        public override DateTime GenerateLastTime()
        {
            return default(DateTime);
        }

        public override DateTime GenerateNextTime(DateTime lastTime)
        {
            DateTime now = DateTime.Now;
            DateTime time = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 10);
            if (time < now)
                return time.AddMinutes(1);
            else
                return time;
        }

        public override void Run(DateTime now)
        {
            Console.WriteLine("PRINT: " + now);
        }
    }
}
