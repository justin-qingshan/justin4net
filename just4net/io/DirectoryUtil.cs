using System;
using System.IO;
using System.Threading;

namespace just4net.io
{
    /// <summary>
    /// Tools for directory.
    /// </summary>
    public static class DirectoryUtil
    {
        /// <summary>
        /// Copy the specific directory to destination.
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        public static void Copy(string sourceDir, string destDir)
        {
            if (!Directory.Exists(sourceDir))
                throw new IOException(sourceDir + " is not exists.");

            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            FileUtil.CopyInDirectory(sourceDir, destDir);

            string[] directionName = Directory.GetDirectories(sourceDir);
            foreach (string directionPath in directionName)
            {
                string directionPathTemp = destDir + "\\" + directionPath.Substring(sourceDir.Length + 1);
                Copy(directionPath, directionPathTemp);
            }
        }
        
        /// Judge the two paths is the same or not.
        /// </summary>
        /// <param name="path1">path 1.</param>
        /// <param name="path2">path 2.</param>
        /// <returns></returns>
        public static bool Equals(string path1, string path2)
        {
            if (Directory.Exists(path1) && Directory.Exists(path2))
                return new DirectoryInfo(path1).FullName.Equals(new DirectoryInfo(path2).FullName);
            else if (File.Exists(path1) && File.Exists(path2))
                return new FileInfo(path1).FullName.Equals(new FileInfo(path2).FullName);
            else
                return false;
        }


        /// <summary>
        /// Move directory.
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        public static void Move(string sourceDir, string destDir)
        {
            if (string.IsNullOrEmpty(sourceDir) || string.IsNullOrEmpty(destDir))
                throw new ArgumentNullException();

            if (!Directory.Exists(sourceDir))
                throw new IOException("Source directory doesn't exists：" + sourceDir);

            string[] files = Directory.GetFiles(sourceDir);
            foreach (string file in files)
                FileUtil.Move(file, Path.Combine(destDir, Path.GetFileName(file)));

            string[] dirs = Directory.GetDirectories(sourceDir);
            foreach(string dir in dirs)
            {
                Move(dir, Path.Combine(destDir, Path.GetFileName(dir)));
            }

        }


        /// <summary>
        /// Delete the directory recursively.
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="tillSuccess">try delete untill all deleted.</param>
        public static void Delete(string dir, bool tillSuccess = false)
        {
            if (string.IsNullOrWhiteSpace(dir))
                throw new ArgumentNullException("parameter 'directory' can't be null.");

            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
                while(Directory.Exists(dir) && tillSuccess)
                {
                    Thread.Sleep(100);
                    Directory.Delete(dir, true);
                }
            }
        }


    }
}
