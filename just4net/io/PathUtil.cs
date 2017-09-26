using System.IO;

namespace just4net.io
{
    public class PathUtil
    {

        public static PathDelNum Delete(string path)
        {
            PathDelNum pathdel = new PathDelNum();
            if (Directory.Exists(path))
                pathdel.Add(DeleteFolder(path));
            if (File.Exists(path))
                pathdel.Add(DeleteFile(path));

            return pathdel;
        }


        public static PathDelNum DeleteFolder(string folderPath)
        {
            PathDelNum pathdel = new PathDelNum();
            if (!Directory.Exists(folderPath))
                return pathdel;

            string[] folders = Directory.GetDirectories(folderPath);
            foreach(string folder in folders)
                pathdel.Add(DeleteFolder(folder));

            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files)
                pathdel.Add(DeleteFile(file));
            
            pathdel.FolderNum++;
            Directory.Delete(folderPath);

            return pathdel;
        }

        public static PathDelNum DeleteFile(string filePath)
        {
            PathDelNum pathdel = new PathDelNum();

            if (!File.Exists(filePath))
                return pathdel;

            FileInfo info = new FileInfo(filePath);
            pathdel.FileSize += info.Length;
            pathdel.FileNum++;
            File.Delete(filePath);
            return pathdel;
        }
        
    }

}
