namespace just4net.io
{
    /// <summary>
    /// Justin
    /// 2017-3-22
    /// </summary>
    public class PathDelNum
    {
        public int FolderNum;
        public int FileNum;
        public long FileSize;

        public PathDelNum()
        {
            FolderNum = 0;
            FileNum = 0;
            FileSize = 0L;
        }

        public PathDelNum Add(PathDelNum num)
        {
            FolderNum += num.FolderNum;
            FileNum += num.FileNum;
            FileSize += num.FileSize;
            return this;
        }
    }
}
