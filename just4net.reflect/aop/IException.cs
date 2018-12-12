using System;
using System.Runtime.Remoting.Messaging;

namespace just4net.reflect.aop
{
    /// <summary>
    /// Interface used when an exception occurred during the method's lifetime.
    /// </summary>
    public interface IException
    {
        void OnException(IMethodCallMessage msg, Exception ex);

        object GetDefaultReturn();
    }
}
