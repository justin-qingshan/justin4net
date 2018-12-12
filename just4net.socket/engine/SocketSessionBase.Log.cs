using just4net.socket.basic;
using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace just4net.socket.engine
{
    public abstract partial class SocketSessionBase : ISocketSession
    {
        private const string m_GeneralErrorMessage = "Unexpected error";
        private const string m_GeneralSocketErrorMessage = "Unexpected socket error: {0}";
        private const string m_CallerInformation = "Caller: {0}, file path: {1}, line number: {2}";

        /// <summary>
        /// Logs the error, skip the ignored exception
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="caller">The caller.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        protected void LogError(Exception exception, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            int socketErrorCode;

            //This exception is ignored, needn't log it
            if (IsIgnorableException(exception, out socketErrorCode))
                return;

            var message = socketErrorCode > 0 ? string.Format(m_GeneralSocketErrorMessage, socketErrorCode) : m_GeneralErrorMessage;

            logger.Error(this
                , message + Environment.NewLine + string.Format(m_CallerInformation, caller, callerFilePath, callerLineNumber)
                , exception);
        }

        /// <summary>
        /// Logs the error, skip the ignored exception
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="caller">The caller.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        protected void LogError(string message, Exception exception, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            int socketErrorCode;

            //This exception is ignored, needn't log it
            if (IsIgnorableException(exception, out socketErrorCode))
                return;

            logger.Error(this
                , message + Environment.NewLine + string.Format(m_CallerInformation, caller, callerFilePath, callerLineNumber)
                , exception);
        }

        /// <summary>
        /// Logs the socket error, skip the ignored error
        /// </summary>
        /// <param name="socketErrorCode">The socket error code.</param>
        /// <param name="caller">The caller.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        protected void LogError(int socketErrorCode, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            
            //This error is ignored, needn't log it
            if (IsIgnorableSocketError(socketErrorCode))
                return;

            logger.Error(this
                , string.Format(m_GeneralSocketErrorMessage, socketErrorCode) + Environment.NewLine + string.Format(m_CallerInformation, caller, callerFilePath, callerLineNumber)
                , new SocketException(socketErrorCode));
        }
    }
}
