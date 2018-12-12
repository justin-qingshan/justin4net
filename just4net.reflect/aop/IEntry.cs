using System.Runtime.Remoting.Messaging;

namespace just4net.reflect.aop
{
    /// <summary>
    /// Interface used before method was proceed.
    /// </summary>
    public interface IEntry
    {
        void OnEntry(IMethodMessage msg);
    }
}
