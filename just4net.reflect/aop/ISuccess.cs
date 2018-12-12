using System.Runtime.Remoting.Messaging;

namespace just4net.reflect.aop
{
    /// <summary>
    /// Interface used after method was successfully proceed.
    /// </summary>
    public interface ISuccess
    {
        void OnSuccess(IMethodReturnMessage msg);
    }
}
