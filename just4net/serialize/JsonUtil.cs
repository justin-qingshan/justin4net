using Newtonsoft.Json;
using System.IO;

namespace just4net.serialize
{
    public class JsonUtil
    {
        public static T DeserializeFromString<T>(string jsonStr)
        {
            return JsonConvert.DeserializeObject<T>(jsonStr);
        }

        public static T DeserializeFromFile<T>(string filePath)
        {
            StreamReader reader = new StreamReader(filePath);
            string jsonStr = reader.ReadToEnd();
            reader.Close();

            return DeserializeFromString<T>(jsonStr);
        }


        public static string Serialize<T>(T t)
        {
            return JsonConvert.SerializeObject(t);
        }


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
