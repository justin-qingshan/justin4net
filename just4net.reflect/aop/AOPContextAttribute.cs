using System;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;

namespace just4net.reflect.aop
{
    /// <summary>
    /// The <see cref="AOPContextAttribute"/> attribute should be applied to classes which wish
    /// to have aop operates for method calls.
    /// <para>
    /// Only classes that derive from <see cref="ContextBoundObject"/> can use this attribute
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AOPContextAttribute : ContextAttribute, IContributeObjectSink
    {
        public AOPContextAttribute() : base("AOPContextAttribute")
        {
        }

        /// <summary>
        /// Defined by the <see cref="IContributeObjectSink"/>. Get a message sink. In this case,
        /// a new <see cref="AOPHandler"/> was given.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="nextSink"></param>
        /// <returns></returns>
        public IMessageSink GetObjectSink(MarshalByRefObject obj, IMessageSink nextSink)
        {
            return new AOPHandler(nextSink);
        }
        
    }
}
