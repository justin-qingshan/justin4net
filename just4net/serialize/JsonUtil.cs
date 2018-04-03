using Newtonsoft.Json;
using System.IO;

namespace just4net.serialize
{
    public class JsonUtil
    {
        /// <summary>
        /// Deserialize a string to object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static T DeserializeFromString<T>(string jsonStr)
        {
            return JsonConvert.DeserializeObject<T>(jsonStr);
        }


        /// <summary>
        /// Read content from file and deserialize it to object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T DeserializeFromFile<T>(string filePath)
        {
            StreamReader reader = new StreamReader(filePath);
            string jsonStr = reader.ReadToEnd();
            reader.Close();

            return DeserializeFromString<T>(jsonStr);
        }


        /// <summary>
        /// Serialize a object to string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string Serialize<T>(T t)
        {
            return JsonConvert.SerializeObject(t);
        }


        /// <summary>
        /// Serialize a object to string and store it in file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="filePath"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public static bool SerializeIntoFile<T>(T t, string filePath, bool create = true)
        {
            string jsonStr = Serialize(t);

            StreamWriter writer;
            if (File.Exists(filePath))
                writer = new StreamWriter(filePath);
            else if (create)
                writer = new StreamWriter(File.Create(filePath));
            else
                return false;

            writer.Write(jsonStr);
            writer.Flush();
            writer.Close();

            return true;
        }
    }
}
