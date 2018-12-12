using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace just4net.socket.basic
{
    public interface IRootConfig
    {
        int MaxWorkingThreads { get; }

        int MinWorkingThreads { get; }

        int MaxCompletionPortThreads { get; }

        int MinCompletionPortThreads { get; }
    }
}
