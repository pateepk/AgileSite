namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data.SqlClient;

    public static class ErrorManager
    {
        public static void logErrorSQLCommand(SqlCommand command, string otherMessages, Exception lastError)
        {
            string strMsg = string.Empty;
            strMsg += String.Format("Msg: {0}\r\n Last Error: {1}\r\n", otherMessages, (lastError != null) ? lastError.Message : "");
            string strCmd = string.Empty;
            if (command != null)
            {
                strCmd += String.Format("type: {0}, text: {1} ", command.CommandType.ToString(), command.CommandText);
                if ((command.Parameters != null) && (command.Parameters.Count > 0))
                {
                    for (int i = 0; i < command.Parameters.Count; i++)
                    {
                        strCmd += String.Format("P{0} {1} {2} = {3} ,"
                            , i
                            , command.Parameters[i].ParameterName
                            , command.Parameters[i].SqlDbType.ToString()
                            , command.Parameters[i].Value.ToString()
                            );
                    }
                }
            }
            strMsg += String.Format("Cmd : {0}", strCmd);
            logMessage(strMsg, true, String.Empty);
        }

        public static void logError(string otherMessages, Exception lastError)
        {
            string strMsg = String.Format("Msg: {0}\r\n Last Error: {1}\r\n Stack Trace: {2}\r\n", otherMessages, (lastError != null) ? lastError.Message : "", (lastError != null) ? lastError.StackTrace : "");
            logMessage(strMsg, true, String.Empty);
        }

        public static long logError(string otherMessages, Exception lastError, bool isRedirect)
        {
            string strMsg = String.Format("Msg: {0}\r\n Last Error: {1}\r\n Stack Trace: {2}\r\n", otherMessages, (lastError != null) ? lastError.Message : "", (lastError != null) ? lastError.StackTrace : "");
            return logMessage(strMsg, isRedirect, String.Empty);
        }

        public static long logMessage(string message)
        {
            return logMessage(String.Format("Msg: {0}\r\n", message), false, String.Empty);
        }

        public static long logMessageFromSilverlight(string message, string loginID)
        {
            return logMessage(message, false, loginID);
        }


        private static long logMessage(string message, bool isRedirect, string SLloginID)
        {
            // do not try to call other object that can generate errors (loopback)
            long WebErrorID = 0;
            string env = AppSettings.EnvironmentCode.ToString();
            string mname = AppSettings.MachineName.ToString();
            string uname = string.Empty;
            string ip = Utils.getClientIPAddress();
            string sname = Utils.getScriptName();
            HttpMethods method = Utils.getHttpMethod();
            bool isUserAdmin = false;

            message += String.Format("Query : {0}\r\n", Utils.getQueryStringForLogs());
            message += String.Format("Form : {0}\r\n", Utils.getFormForLogs());

            object obj = Utils.getPageItem(SV.PageItems.UserManager);
            if (obj != null)
            {
                try
                {
                    UserManager user = (UserManager)obj;
                    uname = user.loginID;
                    isUserAdmin = user.isUserAdministrator;
                }
                catch { }
            }

            DRspWebErrorLogs_Insert er = SQLData.spWebErrorLogs_Insert(
                 message
                , env
                , mname
                , ip
                , String.IsNullOrEmpty(SLloginID) ? uname : SLloginID
                , sname
                , method
                );


            string errorNumber = "";
            if (er.Count > 0)
            {
                WebErrorID = er.WebErrorID(0);
                errorNumber = er.WebErrorID(0).ToString();
            }

            if (AppSettings.EnvironmentCode == environmentCodes.Production)
            {
                if (errorNumber == "")
                {
                    EmailManager email = new EmailManager();
                    try
                    {
                        email.SendEmailToWebsiteAdministrator("Error and could not save to SQL", message);
                    }
                    catch { }
                }
                message = string.Empty; // not error message on production, only number
            }
            else
            {
                if (isUserAdmin)
                {
                    message = "Please see error logs.";
                }
                else
                {
                    message = string.Empty;
                }
            }
            // no redirect on asmx, and ashx. only aspx request
            if ((Utils.getScriptName().EndsWith(".aspx", StringComparison.OrdinalIgnoreCase)) && (isRedirect))
            {
                Utils.responseRedirect(String.Format(Utils.getAppSettings(SV.AppSettings.ErrorPageURL).ToString(), errorNumber, Utils.UrlEncode(message)));
            }

            return WebErrorID;
        }

    }
}
