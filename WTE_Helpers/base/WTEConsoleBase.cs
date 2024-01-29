using System;
using WTE.Helpers;

namespace WTE
{
    /// <summary>
    /// Base for WTE object with console support
    /// </summary>
    public class WTEConsoleBase : WTEBase
    {
        #region methods

        #region access to console

        /// <summary>
        /// Output message to console
        /// </summary>
        /// <param name="p_message"></param>
        protected void OutputMessage(string p_message)
        {
            OutputMessage(p_message, false);
        }

        /// <summary>
        /// Output message to console
        /// </summary>
        /// <param name="p_message"></param>
        /// <param name="p_log"></param>
        protected void OutputMessage(string p_message, bool p_log)
        {
            OutputMessage(p_message, p_log, false);
        }

        /// <summary>
        /// Output message to console
        /// </summary>
        /// <param name="p_message"></param>
        /// <param name="p_log"></param>
        /// <param name="p_pause"></param>
        protected void OutputMessage(string p_message, bool p_log, bool p_pause)
        {
            OutputMessage(p_message, p_log, p_pause, String.Empty);
        }

        /// <summary>
        /// Output message to console
        /// </summary>
        /// <param name="p_message"></param>
        /// <param name="p_log"></param>
        /// <param name="p_pause"></param>
        /// <param name="p_pauseMessage"></param>
        protected void OutputMessage(string p_message, bool p_log, bool p_pause, string p_pauseMessage)
        {
            OutputMessage(p_message, p_log, p_pause, p_pauseMessage, true);
        }

        /// <summary>
        /// Log message
        /// </summary>
        /// <param name="p_message"></param>
        protected void LogMessage(string p_message)
        {
            LogMessage(p_message, true);
        }

        /// <summary>
        /// Log message with output
        /// </summary>
        /// <param name="p_message"></param>
        /// <param name="p_output"></param>
        protected void LogMessage(string p_message, bool p_output)
        {
            LogMessage(p_message, p_output, false);
        }

        /// <summary>
        /// Log message
        /// </summary>
        /// <param name="p_message"></param>
        /// <param name="p_output"></param>
        /// <param name="p_pause"></param>
        protected void LogMessage(string p_message, bool p_output, bool p_pause)
        {
            LogMessage(p_message, p_output, p_pause, String.Empty);
        }

        /// <summary>
        /// Log message
        /// </summary>
        /// <param name="p_message"></param>
        protected void LogMessage(string p_message, bool p_output, bool p_pause, string p_pauseMessage)
        {
            OutputMessage(p_message, true, p_pause, p_pauseMessage, p_output);
        }

        /// <summary>
        /// Output message to console
        /// </summary>
        /// <param name="p_message"></param>
        /// <param name="p_log"></param>
        /// <param name="p_pause"></param>
        /// <param name="p_pauseMessage"></param>
        /// <param name="p_output"></param>
        protected virtual void OutputMessage(string p_message, bool p_log, bool p_pause, string p_pauseMessage, bool p_output)
        {
            if (!String.IsNullOrWhiteSpace(p_message))
            {
                if (p_output)
                {
                    Console.WriteLine(p_message);
                }
                if (p_log)
                {
                    WTELogging.LogMessage(p_message, null, false, false, false, false);
                }
            }

            if (p_pause)
            {
                string msg = String.Format("{0}{1}", System.Environment.NewLine, WTEDataHelper.GetString(p_pauseMessage, "Hit any key to continue."));
                WTEConsoleHelper.GetKeyPress(msg);
            }
        }

        #endregion access to console

        #region logging

        /// <summary>
        /// Log exception and exit
        /// </summary>
        /// <param name="ex"></param>
        public void LogExceptionAndExit(Exception ex)
        {
            LogExceptionAndExit(ex, 1);
        }

        /// <summary>
        /// Log exception and exit
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="p_exitCode"></param>
        public void LogExceptionAndExit(Exception ex, int p_exitCode)
        {
            LogException(ex, "\nPress Any Key To Exit", true, p_exitCode);
        }

        /// <summary>
        /// leverage WTE lib get error message and log exception
        /// </summary>
        /// <param name="ex"></param>
        protected void LogException(Exception ex)
        {
            LogException(ex, "\nPress Any Key to Continue");
        }

        /// <summary>
        /// Log exception
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="p_promptMessage"></param>
        protected void LogException(Exception ex, string p_promptMessage)
        {
            LogException(ex, p_promptMessage, false);
        }

        /// <summary>
        /// Log exception
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="p_promptMessage"></param>
        /// <param name="p_exit"></param>
        protected void LogException(Exception ex, string p_promptMessage, bool p_exit)
        {
            LogException(ex, p_promptMessage, p_exit, 1);
        }

        /// <summary>
        /// Log exception
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="p_promptMessage"></param>
        /// <param name="p_exit"></param>
        /// <param name="p_exitCode"></param>
        protected virtual void LogException(Exception ex, string p_promptMessage, bool p_exit, int p_exitCode)
        {
            string message = WTELogging.GetErrorString(ex, false);
            LogMessage(message);

            string prompt = p_promptMessage;
            if (String.IsNullOrWhiteSpace(prompt))
            {
                if (p_exit)
                {
                    prompt = "\nPress Any Key To Continue";
                }
                else
                {
                    prompt = "\nPress Any Key To Exit";
                }
            } 

            WTEConsoleHelper.GetKeyPress(prompt);

            if (p_exit)
            {
                Environment.Exit(p_exitCode);
            }
        }

        #endregion logging

        #endregion methods
    }
}