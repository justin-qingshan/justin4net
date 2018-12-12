using System;

namespace just4net.socket.basic
{
    public static class LoggerExtension
    {
        private readonly static string m_SessionInfoTemplate = "Session: {0}/{1}";

        /// <summary>
        /// Logs the error
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="session">The session.</param>
        /// <param name="title">The title.</param>
        /// <param name="e">The e.</param>
        public static void Error(this ILog logger, ISessionBase session, string title, Exception e)
        {
            logger.Error(string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + title, e);
        }

        /// <summary>
        /// Logs the error
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="session">The session.</param>
        /// <param name="message">The message.</param>
        public static void Error(this ILog logger, ISessionBase session, string message)
        {
            logger.Error(string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + message);
        }

        /// <summary>
        /// Logs the information
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="session">The session.</param>
        /// <param name="message">The message.</param>
        public static void Info(this ILog logger, ISessionBase session, string message)
        {
            string info = string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + message;
            logger.Info(info);
        }

        /// <summary>
        /// Logs the debug message
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="session">The session.</param>
        /// <param name="message">The message.</param>
        public static void Debug(this ILog logger, ISessionBase session, string message)
        {
            if (!logger.IsDebugEnabled)
                return;

            logger.Debug(string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + message);
        }

        private const string m_PerfLogName = "Perf";
    }
}
