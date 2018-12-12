using System;
using System.Runtime.Remoting.Messaging;

namespace just4net.reflect.aop
{
    /// <summary>
    /// Abstract attribute to be derived from.
    /// <para>
    /// The custom attributes derived from <see cref="AbstractBoundaryAspectAttribute"/> can be applied to 
    /// methods in classes which was applied with <see cref="AOPContextAttribute"/>.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public abstract class AbstractBoundaryAspectAttribute : Attribute, IEntry, ISuccess, IExit
    {
        /// <summary>
        /// Operate will be called before the applied method was proceed.
        /// </summary>
        /// <param name="msg">
        /// The <see cref="IMethodMessage"/> that delivered to the applied method.
        /// </param>
        public abstract void OnEntry(IMethodMessage msg);

        /// <summary>
        /// Operate will be called after the applied method was successfully proceed.
        /// </summary>
        /// <param name="msg">
        /// The <see cref="IMethodReturnMessage"/> returned by the applied method.
        /// </param>
        public abstract void OnSuccess(IMethodReturnMessage msg);

        /// <summary>
        /// Operate will be called finally.
        /// </summary>
        /// <param name="callMsg"></param>
        /// <param name="returnMsg"></param>
        public abstract void OnExit(IMessage callMsg, IMessage returnMsg);
    }
}
