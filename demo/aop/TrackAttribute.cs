using just4net.reflect.aop;
using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace demo.aop
{
    /// <summary>
    /// A sample of <see cref="AbstractBoundaryAspectAttribute"/>, used to track execution interval.
    /// </summary>
    public class TrackAttribute : AbstractBoundaryAspectAttribute
    {
        Stopwatch watch = new Stopwatch();

        public override void OnExit(IMessage callMsg, IMessage returnMsg)
        {
            watch.Stop();
            var msg = callMsg as IMethodCallMessage;
            if (msg == null)
            {
                Console.WriteLine("call message is null.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < msg.InArgCount; i++)
                sb.Append(msg.GetArgName(i)).Append("=").Append(msg.GetArg(i)).Append(",");

            Console.WriteLine($"[{msg.MethodBase.DeclaringType.FullName}: {msg.MethodBase}], {sb.ToString().TrimEnd(',')}, spend {watch.ElapsedMilliseconds}ms.");
        }

        public override void OnSuccess(IMethodReturnMessage msg)
        {
        }

        public override void OnEntry(IMethodMessage msg)
        {
            // Start the stopwatch while entry the method.
            watch.Start();
        }
    }
}
