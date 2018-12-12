using just4net.reflect.aop;
using System;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace demo.aop
{
    /// <summary>
    /// A sample exception aspect attribute to record the exception's detail information.
    /// </summary>
    public sealed class ExceptionRecordAttribute : AbstractExceptionAspectAttribute
    {
        public ExceptionRecordAttribute(object defaultReturn) : base(defaultReturn)
        {
        }

        public override void OnException(IMethodCallMessage msg, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < msg.InArgCount; i++)
                sb.Append(msg.GetArgName(i)).Append("=").Append(msg.GetArg(i)).Append(",");

            Console.WriteLine($"Exception Record, {msg.MethodName}, {sb.ToString().TrimEnd()}");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }

    /// <summary>
    /// A sample used to annouce two defferent type of exception aspect attribute at the same time.
    /// </summary>
    public sealed class ExceptionAlertAttribute : AbstractExceptionAspectAttribute
    {
        public ExceptionAlertAttribute(object defaultReturn) : base(defaultReturn)
        {
        }

        public override void OnException(IMethodCallMessage msg, Exception ex)
        {
            Console.WriteLine("Exception Occurred: " + ex.Message);
        }
    }
}
