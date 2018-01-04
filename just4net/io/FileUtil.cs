using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace just4net.io
{
    public class FileUtil
    {
        /// <summary>
        /// Copy files in directory.
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
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


        /// <summary>
        /// Move file.
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="dstFile"></param>
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


        /// <summary>
        /// Detect the encoding of text files.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="text"></param>
        /// <param name="taster"></param>
        /// <returns></returns>
        public static Encoding DetectTextEncoding(string filename, out string text, int taster = 1000)
        {
            byte[] b = File.ReadAllBytes(filename);

            // First check the low hanging fruit by checking if a BOM/signature exists 
            // (sourced from http://www.unicode.org/faq/utf_bom.html#bom4)
            if (b.Length >= 4 && b[0] == 0x00 && b[1] == 0x00 && b[2] == 0xFE && b[3] == 0xFF)
            {
                // UTF-32, big-endian 
                text = Encoding.GetEncoding("utf-32BE").GetString(b, 4, b.Length - 4);
                return Encoding.GetEncoding("utf-32BE");
            }
            else if (b.Length >= 4 && b[0] == 0xFF && b[1] == 0xFE && b[2] == 0x00 && b[3] == 0x00)
            {
                // UTF-32, little-endian
                text = Encoding.UTF32.GetString(b, 4, b.Length - 4);
                return Encoding.UTF32;
            }
            else if (b.Length >= 2 && b[0] == 0xFE && b[1] == 0xFF)
            {
                // UTF-16, big-endian
                text = Encoding.BigEndianUnicode.GetString(b, 2, b.Length - 2);
                return Encoding.BigEndianUnicode;
            } 
            else if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xFE)
            {
                // UTF-16, little-endian
                text = Encoding.Unicode.GetString(b, 2, b.Length - 2);
                return Encoding.Unicode;
            }
            else if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF)
            {
                // UTF-8
                text = Encoding.UTF8.GetString(b, 3, b.Length - 3);
                return Encoding.UTF8;
            }
            else if (b.Length >= 3 && b[0] == 0x2b && b[1] == 0x2f && b[2] == 0x76)
            {
                // UTF-7
                text = Encoding.UTF7.GetString(b, 3, b.Length - 3);
                return Encoding.UTF7;
            }


            // If the code reaches here, no BOM/signature was found, so now
            // we need to 'taste' the file to see if can manually discover
            // the encoding. A high taster value is desired for UTF-8

            // Taster size can't be bigger than the filesize obviously.
            if (taster == 0 || taster > b.Length)
                taster = b.Length;

            // Some text files are encoded in UTF8, but have no BOM/signature. Hence
            // the below manually checks for a UTF8 pattern. This code is based off
            // the top answer at: https://stackoverflow.com/questions/6555015/check-for-invalid-utf8
            // For our purposes, an unnecessarily strict (and terser/slower)
            // implementation is shown at: https://stackoverflow.com/questions/1031645/how-to-detect-utf-8-in-plain-c
            // For the below, false positives should be exceedingly rare (and would
            // be either slightly malformed UTF-8 (which would suit our purposes
            // anyway) or 8-bit extended ASCII/UTF-16/32 at a vanishingly long shot).
            int i = 0;
            bool utf8 = false;
            while (i < taster - 4)
            {
                if (b[i] <= 0x7F)
                {
                    // If all characters are below 0x80, then it is valid UTF8, 
                    // but UTF8 is not 'required' (and therefore the text is more 
                    // desirable to be treated as the default codepage of the computer). 
                    // Hence, there's no "utf8 = true;" code unlike the next three checks.
                    i += 1;
                    continue;
                }     
                if (b[i] >= 0xC2 && b[i] <= 0xDF && b[i + 1] >= 0x80 && b[i + 1] < 0xC0)
                {
                    i += 2;
                    utf8 = true;
                    continue;
                }
                if (b[i] >= 0xE0 && b[i] <= 0xF0 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0)
                {
                    i += 3;
                    utf8 = true;
                    continue;
                }
                if (b[i] >= 0xF0 && b[i] <= 0xF4 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0 && b[i + 3] >= 0x80 && b[i + 3] < 0xC0)
                {
                    i += 4;
                    utf8 = true;
                    continue;
                }
                utf8 = false; break;
            }

            if (utf8 == true)
            {
                text = Encoding.UTF8.GetString(b);
                return Encoding.UTF8;
            }


            // The next check is a heuristic attempt to detect UTF-16 without a BOM.
            // We simply look for zeroes in odd or even byte places, and if a certain
            // threshold is reached, the code is 'probably' UF-16.          
            double threshold = 0.1; // proportion of chars step 2 which must be zeroed to be diagnosed as utf-16. 0.1 = 10%
            int count = 0;
            for (int n = 0; n < taster; n += 2) if (b[n] == 0) count++;
            if (((double)count) / taster > threshold) { text = Encoding.BigEndianUnicode.GetString(b); return Encoding.BigEndianUnicode; }
            count = 0;
            for (int n = 1; n < taster; n += 2) if (b[n] == 0) count++;
            if (((double)count) / taster > threshold) { text = Encoding.Unicode.GetString(b); return Encoding.Unicode; } // (little-endian)


            // Finally, a long shot - let's see if we can find "charset=xyz" or
            // "encoding=xyz" to identify the encoding:
            for (int n = 0; n < taster - 9; n++)
            {
                if (((b[n + 0] == 'c' || b[n + 0] == 'C') 
                        && (b[n + 1] == 'h' || b[n + 1] == 'H') 
                        && (b[n + 2] == 'a' || b[n + 2] == 'A') 
                        && (b[n + 3] == 'r' || b[n + 3] == 'R') 
                        && (b[n + 4] == 's' || b[n + 4] == 'S') 
                        && (b[n + 5] == 'e' || b[n + 5] == 'E') 
                        && (b[n + 6] == 't' || b[n + 6] == 'T') 
                        && (b[n + 7] == '=')) 
                    || ((b[n + 0] == 'e' || b[n + 0] == 'E') 
                        && (b[n + 1] == 'n' || b[n + 1] == 'N') 
                        && (b[n + 2] == 'c' || b[n + 2] == 'C') 
                        && (b[n + 3] == 'o' || b[n + 3] == 'O') 
                        && (b[n + 4] == 'd' || b[n + 4] == 'D') 
                        && (b[n + 5] == 'i' || b[n + 5] == 'I') 
                        && (b[n + 6] == 'n' || b[n + 6] == 'N') 
                        && (b[n + 7] == 'g' || b[n + 7] == 'G') 
                        && (b[n + 8] == '=')))
                {
                    if (b[n + 0] == 'c' || b[n + 0] == 'C') n += 8; else n += 9;
                    if (b[n] == '"' || b[n] == '\'') n++;
                    int oldn = n;
                    while (n < taster && (b[n] == '_' || b[n] == '-' || (b[n] >= '0' && b[n] <= '9') || (b[n] >= 'a' && b[n] <= 'z') || (b[n] >= 'A' && b[n] <= 'Z')))
                    { n++; }
                    byte[] nb = new byte[n - oldn];
                    Array.Copy(b, oldn, nb, 0, n - oldn);
                    try
                    {
                        string internalEnc = Encoding.ASCII.GetString(nb);
                        text = Encoding.GetEncoding(internalEnc).GetString(b);
                        return Encoding.GetEncoding(internalEnc);
                    }
                    catch { break; }    // If C# doesn't recognize the name of the encoding, break.
                }
            }


            // If all else fails, the encoding is probably (though certainly not
            // definitely) the user's local codepage! One might present to the user a
            // list of alternative encodings as shown here: https://stackoverflow.com/questions/8509339/what-is-the-most-common-encoding-of-each-language
            // A full list can be found using Encoding.GetEncodings();
            text = Encoding.Default.GetString(b);
            return Encoding.Default;
        }
    }
}
