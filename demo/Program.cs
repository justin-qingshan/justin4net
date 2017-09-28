using System;
using System.IO;
using just4net.io;
using System.Collections.Generic;
using just4net.collection;

namespace demo
{
    class Program
    {
        static void Main(string[] args)
        {
            TestCollection();
            Console.ReadKey();
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
}
