using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace just4net.io
{
    /// <summary>
    /// Tools for file.
    /// </summary>
    public static class FileUtil
    {
        /// Combine the content of specific files.
        /// </summary>
        /// <param name="outputFile">the output file contains combination content of files</param>
        /// <param name="inputFiles">the files to combine content</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static void CombineContent(string outputFile, IEnumerable<string> inputFiles)
        {
            if (string.IsNullOrEmpty(outputFile))
                throw new ArgumentNullException("outputFile cannot be null.");

            if (inputFiles == null)
                throw new ArgumentNullException("inputFiles cannot be null.");

            if (File.Exists(outputFile))
                File.Delete(outputFile);

            StreamWriter writer = new StreamWriter(File.Create(outputFile));

            string str = "";
            foreach (string inputFile in inputFiles)
            {
                if (!File.Exists(inputFile))
                    continue;

                StreamReader reader = new StreamReader(inputFile);
                str = reader.ReadToEnd();
                reader.Close();
                writer.Write(str + Environment.NewLine);
                writer.Flush();
            }

            writer.Close();
        }

        
        /// Copy the files of specific directory to another directory.
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static void CopyInDirectory(string sourceDir, string destDir)
        {
            if (string.IsNullOrEmpty(sourceDir))
                throw new ArgumentNullException("sourceDir cannot be null.");

            if (string.IsNullOrEmpty(destDir))
                throw new ArgumentNullException("destDir cannot be null.");

            if (!Directory.Exists(sourceDir))
                throw new ArgumentException(sourceDir + ", directory not exists");

            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            string[] fileName = Directory.GetFiles(sourceDir);
            string filePathTemp;
            foreach (string filePath in fileName)
            {
                filePathTemp = destDir + "\\" + filePath.Substring(sourceDir.Length + 1);
                File.Copy(filePath, filePathTemp, true);
            }
        }




        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="sourceFile">source file</param>
        /// <param name="destFile">destination file.</param>
        /// <exception cref="IOException"></exception>
        public static void Move(string sourceFile, string destFile)
        {
            if (!File.Exists(sourceFile))
                throw new IOException("Source file doesn't exist: " + sourceFile);

            if (File.Exists(destFile))
                return;

            File.Move(sourceFile, destFile);
        }



        /// <summary>
        /// Read the content of file.
        /// </summary>
        /// <param name="file">file path</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="OutOfMemoryException"></exception>
        /// <returns>file content.</returns>
        public static string ReadContent(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                throw new ArgumentException("file path can not be null.");

            if (!File.Exists(file))
                throw new FileNotFoundException(file + " not exists.");


            StreamReader reader = new StreamReader(file);
            string content = reader.ReadToEnd();
            reader.Close();

            return content;
        }


        /// <summary>
        /// Write content to the file.
        /// </summary>
        /// <param name="file">file path</param>
        /// <param name="content">content</param>
        /// <param name="create">whether to create a new file if file does not exists.</param>
        /// <param name="append">whether to append content to the file's end.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public static void WriteToFile(string file, string content, bool create, bool append)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException("file path can not be null.");


            FileStream fileStream;
            if (!File.Exists(file))
            {
                if (!create)
                    return;

                string dir = Path.GetDirectoryName(file);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                fileStream = File.Create(file);
            }
            else
            {
                if (append)
                    fileStream = File.Open(file, FileMode.Append, FileAccess.ReadWrite);
                else
                    fileStream = File.Open(file, FileMode.Open, FileAccess.ReadWrite);
            }

            StreamWriter writer = new StreamWriter(fileStream);
            writer.Write(content);
            writer.Flush();
            writer.Close();
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
