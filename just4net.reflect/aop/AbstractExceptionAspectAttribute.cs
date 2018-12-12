using System;
using System.Runtime.Remoting.Messaging;

namespace just4net.reflect.aop
{
    /// <summary>
    /// Abstract attribute to be derived from.
    /// <para>
    /// The custom attributes derived from <see cref="AbstractExceptionAspectAttribute"/> can be applied to 
    /// methods in classes which was applied with <see cref="AOPContextAttribute"/> to catch exceptions
    /// occurred in method.
    /// </para>
    /// <para>
    /// If a method was applied by custom attributes derived from <see cref="AbstractExceptionAspectAttribute"/>,
    ///  then the exception in <see cref="IMethodReturnMessage"/> will be erased and given a default value by
    ///  the first custom attributes' <see cref="AbstractExceptionAspectAttribute.GetDefaultReturn"/>, which was
    ///  delived from constructor.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public abstract class AbstractExceptionAspectAttribute : Attribute, IException
    {
        protected object _defaultReturn;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="defaultReturn">
        /// Value to be used to fill in <see cref="IMethodReturnMessage"/>.
        /// </param>
        public AbstractExceptionAspectAttribute(object defaultReturn)
        {
            _defaultReturn = defaultReturn;
        }

        /// <summary>
        /// Operate that will be called when there is an exception occurred when applied method was proceed 
        /// (the exception is not null in <see cref="IMethodReturnMessage"/>).
        /// </summary>
        /// <param name="msg">the <see cref="IMethodCallMessage"/> 
        /// that was delivered to applied method.
        /// </param>
        /// <param name="ex">
        /// the exception will be handled in <see cref="IMethodReturnMessage"/>.
        /// </param>
        public abstract void OnException(IMethodCallMessage msg, Exception ex);

        /// <summary>
        /// Get the default return value.
        /// <para>
        /// Need the value to write to <see cref="IMethodReturnMessage"/> when the exception was erased.
        /// </para>
        /// </summary>
        /// <returns>
        /// the default return value used when exception was erased.
        /// </returns>
        public object GetDefaultReturn()
        {
            return _defaultReturn;
        }
    }
}
