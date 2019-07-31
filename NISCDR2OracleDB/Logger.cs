using System;
using System.Text;
using System.Diagnostics;

namespace NISCDR2OracleDB
{
    public class Logger
    {
        public static void Log(Exception exception)
        {
            StringBuilder sbExceptionMessage = new StringBuilder();

            do
            {
                sbExceptionMessage.Append("Exception Type" + Environment.NewLine);
                sbExceptionMessage.Append(exception.GetType().Name);
                sbExceptionMessage.Append(Environment.NewLine + Environment.NewLine);
                sbExceptionMessage.Append("Message" + Environment.NewLine);
                sbExceptionMessage.Append(exception.Message + Environment.NewLine + Environment.NewLine);
                sbExceptionMessage.Append("Stack Trace" + Environment.NewLine);
                sbExceptionMessage.Append(exception.StackTrace + Environment.NewLine + Environment.NewLine);

                exception = exception.InnerException;
            }
            while (exception != null);

            if (EventLog.SourceExists("Hello"))
            {
                EventLog log = new EventLog("Hello");
                log.Source = "Hello";
                log.WriteEntry(sbExceptionMessage.ToString(), EventLogEntryType.Error);
            }
        }

    }
}
