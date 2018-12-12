using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace just4net.reflect.aop
{
    public sealed class AOPHandler : IMessageSink
    {
        /// <summary>
        /// a reference to the next sink in the sink chain.
        /// </summary>
        private readonly IMessageSink _nextSink;

        private List<IEntry> befores = new List<IEntry>();          // operates before method.
        private List<ISuccess> afters = new List<ISuccess>();       // operates after method succeed.
        private List<IExit> finals = new List<IExit>();             // operates around method.
        private List<IException> exs = new List<IException>();      // operates when exception occurred.

        public IMessageSink NextSink { get { return _nextSink; } }

        public AOPHandler(IMessageSink nextSink)
        {
            _nextSink = nextSink;
        }

        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            return null;
        }

        /// <summary>
        /// Defined by the <see cref="IMessageSink"/> interface. Synchronously process the given message.
        /// </summary>
        /// <param name="msg">the message to process.</param>
        /// <returns>A return <see cref="IMessage"/> in response to the request.</returns>
        public IMessage SyncProcessMessage(IMessage msg)
        {
            IMessage message = null;
            var callMsg = msg as IMethodCallMessage;
            if (callMsg != null)
            {
                Arrange(callMsg);
                PreProceed(callMsg);

                message = _nextSink.SyncProcessMessage(msg);
                var returnMsg = message as IMethodReturnMessage;
                if (returnMsg != null)
                {
                    
                    if (returnMsg.Exception == null )
                        PostProceed(returnMsg);
                    else
                    {
                        // When exception will be handled, then call the handler to handle it.
                        // Than erase exception from return message.
                        if (exs != null && exs.Count != 0)
                        {
                            foreach (IException e in exs)
                                e.OnException(callMsg, returnMsg.Exception);
                            EraseException(returnMsg, exs[0].GetDefaultReturn());
                        }
                    }
                }
                FinalProceed(msg, message);
            }
            else
            {
                msg = _nextSink.SyncProcessMessage(msg);
            }
            return message;
        }

        /// <summary>
        /// Arrange all the aop operates applied to <see cref="IMethodCallMessage"/>.
        /// </summary>
        /// <param name="msg">the message to be arranged.</param>
        private void Arrange(IMethodCallMessage msg)
        {
            MethodInfo info = msg.MethodBase as MethodInfo;
            
            List<AbstractBoundaryAspectAttribute> ars = ReflectUtil.GetAttrs<AbstractBoundaryAspectAttribute>(info);
            List<AbstractExceptionAspectAttribute> es = ReflectUtil.GetAttrs<AbstractExceptionAspectAttribute>(info);

            befores.Clear();
            afters.Clear();
            finals.Clear();
            exs.Clear();
            
            if (ars != null)
            {
                befores.AddRange(ars);
                afters.AddRange(ars);
                finals.AddRange(ars);
            }

            if (es != null)
                exs.AddRange(es);
        }

        /// <summary>
        /// execute the operates before current method is proceed.
        /// </summary>
        /// <param name="msg"></param>
        private void PreProceed(IMethodMessage msg)
        {
            if (befores != null && befores.Count != 0)
            {
                foreach (IEntry before in befores)
                    before.OnEntry(msg);
            }
        }

        /// <summary>
        /// execute the operates after succeed to process current method.
        /// </summary>
        /// <param name="msg"></param>
        private void PostProceed(IMethodReturnMessage msg)
        {
            if (msg != null && afters != null && afters.Count != 0)
            {
                foreach (ISuccess after in afters)
                    after.OnSuccess(msg);
            }
        }

        /// <summary>
        /// execute the operates finally.
        /// </summary>
        /// <param name="callMsg"></param>
        /// <param name="returnMsg"></param>
        private void FinalProceed(IMessage callMsg, IMessage returnMsg)
        {
            if (finals != null && finals.Count != 0)
                foreach (IExit f in finals)
                    f.OnExit(callMsg, returnMsg);
        }

        /// <summary>
        /// When there is exception handle operates, the exception in <see cref="IMethodReturnMessage"/>
        /// need to be erased.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="defaultReturn"></param>
        private void EraseException(IMethodReturnMessage msg, object defaultReturn)
        {
            // WARNING: This uses reflection to wipe out a reference in a private variable.  Since
            // the variable is not documented, it could change in future releases of the .NET
            // Framework.  As much as I wanted to get around having to do this, I could not find
            // any other way at this time.
            Type rmType = Type.GetType("System.Runtime.Remoting.Messaging.ReturnMessage");
            FieldInfo rmException = rmType.GetField("_e", BindingFlags.NonPublic | BindingFlags.Instance);
            if (rmException != null) rmException.SetValue(msg, null);

            // Update the default return value when the exception is swallowed using reflection.
            FieldInfo rmReturnValue = rmType.GetField("_ret", BindingFlags.NonPublic | BindingFlags.Instance);
            if (rmReturnValue != null) rmReturnValue.SetValue(msg, defaultReturn);
        }
    }
}
