using System;
using System.Threading.Tasks;

namespace just4net.socket.basic
{
    public static class Async
    {
        public static Task AsyncRun(this ILoggerProvider log, Action task)
        {
            return AsyncRun(log, task, TaskCreationOptions.None, null);
        }

        public static Task AsyncRun(this ILoggerProvider log, Action task,
            TaskCreationOptions taskOption)
        {
            return AsyncRun(log, task, taskOption, null);
        }

        public static Task AsyncRun(this ILoggerProvider log, Action task,
            Action<Exception> exceptionHandler)
        {
            return AsyncRun(log, task, TaskCreationOptions.None, exceptionHandler);
        }

        public static Task AsyncRun(this ILoggerProvider log, Action task,
            TaskCreationOptions taskOption, Action<Exception> exceptionHandler)
        {
            return Task.Factory.StartNew(task, taskOption).ContinueWith(t =>
            {
                if (exceptionHandler != null)
                    exceptionHandler(t.Exception);
                else
                {
                    if (log.Logger.IsErrorEnabled)
                    {
                        for (int i = 0; i < t.Exception.InnerExceptions.Count; i++)
                            log.Logger.Error(t.Exception.InnerExceptions[i]);
                    }
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
