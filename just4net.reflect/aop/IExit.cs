using System.Runtime.Remoting.Messaging;

namespace just4net.reflect.aop
{
    /// <summary>
    /// Interface used after method was proceed (not matter succeed or failed).
    /// </summary>
    public interface IExit
    {
        void OnExit(IMessage callMsg, IMessage returnMsg);
    }
}
