using System.Diagnostics;

namespace just4net.doc
{
    public abstract class IOfficeUtil
    {
        public abstract bool Check(bool createIfNotExist);

        protected abstract string ProcessName { get; }

        public bool Kill()
        {
            return Kill(ProcessName);
        }

        public static bool Kill(string processName)
        {
            try
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.CreateNoWindow = true;
                start.UseShellExecute = false;
                start.FileName = "taskkill";

                start.Arguments = string.Format("/IM \"{0}\" /T /F", processName);
                Process.Start(start).WaitForExit();
                return true;
            }
            catch { return false; }
        }
    }
}
