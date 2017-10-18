using System;
using System.IO;
using System.Security.Cryptography;

namespace just4net.io
{
    public class FileUtil
    {
        public static void CopyInDirectory(string sourceDir, string destDir)
        {
            string[] fileName = Directory.GetFiles(sourceDir);
            string filePathTemp;
            foreach (string filePath in fileName)
            {
                filePathTemp = destDir + "\\" + filePath.Substring(sourceDir.Length + 1);
                File.Copy(filePath, filePathTemp, true);
            }
        }


        public static void Move(string sourceFile, string dstFile)
        {
            if (!File.Exists(sourceFile))
                return;

            if (File.Exists(dstFile))
                return;

            File.Move(sourceFile, dstFile);
        }



        /// <summary>
        /// Compute the MD5 value of file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string MD5(string path)
        {
            HashAlgorithm hash = new MD5CryptoServiceProvider();
            return HashLarge(path, hash);
        }


        /// <summary>
        /// Compute the SHA1 value of file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string SHA1(string path)
        {
            HashAlgorithm hash = new SHA1CryptoServiceProvider();
            return HashLarge(path, hash);
        }


        /// <summary>
        /// Compute the SHA256 value of file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string SHA256(string path)
        {
            HashAlgorithm hash = new SHA256CryptoServiceProvider();
            return HashLarge(path, hash);
        }


        /// <summary>
        /// Hash file with specific hash algorithm (for large file).
        /// </summary>
        /// <param name="path"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static string HashLarge(string path, HashAlgorithm hash)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("File path can't be null.");

            if (!File.Exists(path))
                throw new ArgumentException("File doesn't exist: " + path);

            int bufferSize = 1024 * 16;
            byte[] buffer = new byte[bufferSize];

            Stream inputStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            int readLength = 0;
            var output = new byte[bufferSize];
            while ((readLength = inputStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                hash.TransformBlock(buffer, 0, readLength, output, 0);
            }

            hash.TransformFinalBlock(buffer, 0, 0);
            string md5 = BitConverter.ToString(hash.Hash);
            hash.Clear();
            inputStream.Close();
            md5 = md5.Replace("-", "");

            return md5;
        }


        /// <summary>
        /// Hash file with specific hash algorithm(for small file).
        /// </summary>
        /// <param name="path"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static string HashSmall(string path, HashAlgorithm hash)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("File path can't be null.");

            if (!File.Exists(path))
                throw new ArgumentException("File doesn't exist: " + path);

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] buffer = hash.ComputeHash(fs);
            string str = BitConverter.ToString(buffer);
            str = str.Replace("-", "");
            hash.Clear();
            fs.Close();
            return str;
        }
    }
}
