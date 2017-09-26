using System;
using System.IO;

namespace demo
{
    class Program
    {
        static void Main(string[] args)
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
            Console.ReadKey();
        }

        
    }
}
