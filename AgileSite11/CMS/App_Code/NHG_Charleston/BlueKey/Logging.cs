using System;

using CMS.EventLog;

namespace NHG_C.BlueKey
{
    public static class Log
    {
        private const string Info = "I";
        private const string Warn = "W";
        private const string Err = "E";

        private const string BlueKeyEventCode = "BlueKeyEvent";
        private const string BlueKeyEventType = "BlueKey";

        public static void Debug(string message)
        {
            if (Configuration.LogDebug)
            {
                LogEvent(Info, "Debug", message);
            }
        }

        public static void Warning(string message)
        {
            Warning(message, BlueKeyEventCode);
        }

        public static void Warning(string message, string eventCode)
        {
            LogEvent(Warn, eventCode, message);
        }

        public static void Information(string message)
        {
            Information(message, BlueKeyEventCode);
        }

        public static void Information(string message, string eventCode)
        {
            LogEvent(Info, eventCode, message);
        }

        public static void Error(string message)
        {
            Error(message, BlueKeyEventCode);
        }

        public static void Error(string message, string eventCode)
        {
            LogEvent(Err, eventCode, message);
        }

        public static void Error(Exception ex)
        {
            Error(ex, BlueKeyEventCode);
        }

        public static void Error(Exception ex, string eventCode)
        {
            EventLogInfo eli = new EventLogInfo();
            eli.EventCode = eventCode;

            EventLogProvider.LogEvent(eli);
        }

        private static void LogEvent(string eventType, string eventCode, string message)
        {
            EventLogInfo eli = new EventLogInfo();
            eli.EventType = eventType;
            eli.EventCode = eventCode;
            eli.EventDescription = message;

            EventLogProvider.LogEvent(eli);
        }

    }
}