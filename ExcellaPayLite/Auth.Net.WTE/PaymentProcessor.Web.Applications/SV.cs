namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    public static class SV
    {

        public static class appSettingData
        {
            public const string NA = "";
            public const string Application = "Application";
            public const string ConnectionString = "ConnectionString";
            public const string Links = "Links";
            public const string Users = "Users";
        }

        public static class UserAgentDetectTokens
        {
            public const string Android = "Android";
            public const string Blackberry = "Blackberry";
            public const string IEMobile = "IEMobile";
            public const string iPhone = "iPhone";
            public const string iPad = "iPad";
            public const string FacebookBot = "facebookexternalhit";
        }

        public static class ApplicationPath
        {
            public const string AppRoot = "~/{0}";
            public const string AppSettings = "~/App_Data/AppSettings/";
            public const string AppFiles = "~/App_Data/Files/";
            public const string AppInvoiceFiles = "~/InvoicesFiles/"; // need to start with ~
        }

        public struct RDLCTokens
        {
            public const string DATA_NAME = "{DATA_NAME}";
            public const string DATA_NAME_SPACE = "{DATA_NAME_SPACE}";
            public const string TEXTBOX_NAME = "{TEXTBOX_NAME}";
            public const string DATA_TYPE = "{DATA_TYPE}";
            public const string FIELD_VALUE_FORMAT = "{FIELD_VALUE_FORMAT}";
            public const string REPORT_TITLE = "{REPORT_TITLE}";
        }

        public static class ReportParameters // And Tool Paramater
        {
            // lower case
            public const string CurrentUserID = "currentuserid";
            public const string UserID = "userid";
            public const string IsRunFromTool = "isrunfromtool"; // Tool Only
        }

        public static class Tasks
        {
            public const string HelloWorld = "HelloWorld";
        }

        public static class UserSettings
        {
        }

        public static class AppSettings
        {
            public const string PayTraceUsername = "PayTraceUsername";
            public const string PayTracePassword = "PayTracePassword";
            public const string PayTraceURL = "PayTraceURL";
            public const string EmailTemplate_BackdoorLink = "EmailTemplate_BackdoorLink";
            public const string IsEmailManagerUsingWTECommunication = "IsEmailManagerUsingWTECommunication";
            public const string CommunicationWSURL = "CommunicationWSURL";
            public const string CommunicationWSUser = "CommunicationWSUser";
            public const string CommunicationWSPassword = "CommunicationWSPassword";
            public const string BillingEmail = "BillingEmail";
            public const string EmailTemplate_InvoiceInfo = "EmailTemplate_InvoiceInfo";
            public const string EmailTemplate_NewInTransitPayment = "EmailTemplate_NewInTransitPayment";
            public const string EmailTemplate_PaymentDecline = "EmailTemplate_PaymentDecline";
            public const string EmailTemplate_PaymentPaid = "EmailTemplate_PaymentPaid";
            public const string IsRecordActivityUserLoginLogout = "IsRecordActivityUserLoginLogout";
            public const string IsTaskExecutorInAppManager = "IsTaskExecutorInAppManager";
            public const string IncomingEmailToFromPublic = "IncomingEmailToFromPublic";
            public const string ApplicationName = "ApplicationName";
            public const string ExternalMode = "ExternalMode";
            public const string Copyright = "CompanyCopyright";
            public const string StagingWebsiteBanner = "StagingWebsiteBanner";
            public const string NoPrivilegeMessageURL = "NoPrivilegeMessageURL";
            public const string NonUserURL = "NonUserURL";
            public const string UnderConstructionPageURL = "UnderConstructionPageURL";
            public const string UpgradingURL = "UpgradingURL";
            public const string SimpleReport_TypeName = "SimpleReport_TypeName";
            public const string SimpleReport_SelectMethod = "SimpleReport_SelectMethod";
            public const string SimpleReport_DataSourceName = "SimpleReport_DataSourceName";
            public const string SimpleReport_RDLCPath = "SimpleReport_RDLCPath";
            public const string SimpleReport_RDLCTemplateXML = "SimpleReport_RDLCTemplateXML";
            public const string ErrorPageURL = "ErrorPageURL";
            public const string TaskTimerInterval = "TaskTimerInterval";
            public const string EnvironmentURL = "EnvironmentURL";
            public const string Announcement_MaxList = "Announcement_MaxList";
            public const string Browse_SafeFileExtensions = "Browse_SafeFileExtensions";
            public const string View_ToolLogMaxResult = "View_ToolLogMaxResult";
            public const string TaskExecutor_Domain = "TaskExecutor_Domain";
            public const string TaskExecutor_Username = "TaskExecutor_Username";
            public const string TaskExecutor_Password = "TaskExecutor_Password";
            public const string TaskExecutor_Timeout = "TaskExecutor_Timeout";
            public const string EmailServer = "EmailServer";
            public const string EmailServerPort = "EmailServerPort";
            public const string Email_NetworkCredential_Username = "Email_NetworkCredential_Username";
            public const string Email_NetworkCredential_Password = "Email_NetworkCredential_Password";
            public const string IsOutgoingEmailLog = "IsOutgoingEmailLog";
            public const string OutgoingEmailFrom = "OutgoingEmailFrom";
            public const string OutgoingEmailFromDisplayName = "OutgoingEmailFromDisplayName";
            public const string DevelopmentEmail = "DevelopmentEmail";
            public const string EmailTemplate_ForgotPassword = "EmailTemplate_ForgotPassword";
        }

        /// <summary>
        /// List all server variables that we use in this application. List it here, so we can keep track them.
        /// </summary>
        public static class ServerVariables
        {
            public const string SCRIPT_NAME = "SCRIPT_NAME";
            public const string SERVER_NAME = "SERVER_NAME";
            public const string HTTP_COOKIE = "HTTP_COOKIE";
            public const string HTTP_HOST = "HTTP_HOST";
            public const string HTTP_USER_AGENT = "HTTP_USER_AGENT";
            public const string REMOTE_ADDR = "REMOTE_ADDR";
            public const string HTTP_REFERER = "HTTP_REFERER";
            public const string QUERY_STRING = "QUERY_STRING";
            public const string SERVER_PORT_SECURE = "SERVER_PORT_SECURE";
            public const string SERVER_PORT = "SERVER_PORT";
        }

        public static class Sessions
        {
            public const string URLReturn = "URLReturn";
            public const string UserManager = "UserManager";
        }

        public static class Applications
        {
            public static class GroupPrefix
            {
                public const string PGNObject = "PGNCache_";
            }

            public static class DataTable
            {
                
            }

            public const string IncomingTournamentsInNC = "IncomingTournamentsInNC_{0}_{1}";
            public const string UploadFileID = "UploadFileID_{0}";
            public const string LoginIDAndPassword = "LoginID_{0}";
        }

        public static class SP
        {
            public static class EGivings
            {
                public const string spTA_custom_egiving_Events_UpdateNumber_of_Tickets = "spTA_custom_egiving_Events_UpdateNumber_of_Tickets";
                public const string spTA_DirectTransactions_Events_UpdateAfterPost = "spTA_DirectTransactions_Events_UpdateAfterPost";
                public const string spTA_DirectTransactions_Events_GetForThanks = "spTA_DirectTransactions_Events_GetForThanks";
                public const string spTA_DirectTransactions_Events_GetByUserGUID = "spTA_DirectTransactions_Events_GetByUserGUID";
                public const string spTA_DirectTransactions_Events_Insert = "spTA_DirectTransactions_Events_Insert";
                public const string spTA_custom_egiving_Events_Get = "spTA_custom_egiving_Events_Get";
                public const string spTA_RecurringTransactions_UpdateRecurringInfo = "spTA_RecurringTransactions_UpdateRecurringInfo";
                public const string spTA_RecurringTransactions_UpdateCustomerInfo = "spTA_RecurringTransactions_UpdateCustomerInfo";
                public const string spTA_RecurringTransactions_GetByUserGUID = "spTA_RecurringTransactions_GetByUserGUID";
                public const string spTA_CheckTransactions_GetByUserGUID = "spTA_CheckTransactions_GetByUserGUID";
                public const string spTA_DirectTransactions_GetByUserGUID = "spTA_DirectTransactions_GetByUserGUID";
                public const string spTA_RecurringTransactions_GetForThanks = "spTA_RecurringTransactions_GetForThanks";
                public const string spTA_CheckTransactions_GetForThanks = "spTA_CheckTransactions_GetForThanks";
                public const string spTA_DirectTransactions_GetForThanks = "spTA_DirectTransactions_GetForThanks";
                public const string spTA_custom_egivings_Get = "spTA_custom_egivings_Get";
                public const string spTA_CheckTransactions_UpdateAfterPost = "spTA_CheckTransactions_UpdateAfterPost";
                public const string spTA_CheckTransactions_Insert = "spTA_CheckTransactions_Insert";
                public const string spTA_RecurringTransactions_Delete = "spTA_RecurringTransactions_Delete";
                public const string spTA_RecurringTransactions_GetByRecurringTransactionID = "spTA_RecurringTransactions_GetByRecurringTransactionID";
                public const string spTA_RecurringTransactions_UpdateNext = "spTA_RecurringTransactions_UpdateNext";
                public const string spTA_RecurringTransactions_UpdateRecurringID = "spTA_RecurringTransactions_UpdateRecurringID";
                public const string spTA_RecurringTransactions_UpdateAfterCustomerProfile = "spTA_RecurringTransactions_UpdateAfterCustomerProfile";
                public const string spTA_RecurringTransactions_Insert = "spTA_RecurringTransactions_Insert";
                public const string spTA_DirectTransactions_UpdateAfterPost = "spTA_DirectTransactions_UpdateAfterPost";
                public const string spTA_DirectTransactions_Insert = "spTA_DirectTransactions_Insert";
                public const string spTA_CMS_User_GetByUserGUID = "spTA_CMS_User_GetByUserGUID";
                public const string spTA_PayTraceCustomerExport_Insert = "spTA_PayTraceCustomerExport_Insert";
                public const string spTA_PayTraceRequestResponse_UpdateResponse = "spTA_PayTraceRequestResponse_UpdateResponse";
                public const string spTA_PayTraceRequestResponse_InsertRequest = "spTA_PayTraceRequestResponse_InsertRequest";
            }

            public const string spWTE_excellalite_invoices_notes_GetAllNotes = "spWTE_excellalite_invoices_notes_GetAllNotes";
            public const string spWTE_excellalite_invoices_notes_InsertItem = "spWTE_excellalite_invoices_notes_InsertItem";
            public const string spWTE_excellalite_invoices_items_GetByInvoiceItemID = "spWTE_excellalite_invoices_items_GetByInvoiceItemID";
            public const string spUsers_GetByCustNum = "spUsers_GetByCustNum";
            public const string spUsers_GetAdministrators = "spUsers_GetAdministrators";
            public const string spWTE_excellalite_invoices_items_Fail = "spWTE_excellalite_invoices_items_Fail";
            public const string spWTE_excellalite_invoices_GetCustNum = "spWTE_excellalite_invoices_GetCustNum";
            public const string spWTE_excellalite_invoices_Archive = "spWTE_excellalite_invoices_Archive";
            public const string spWTE_excellalite_invoices_items_UpdateToPaid = "spWTE_excellalite_invoices_items_UpdateToPaid";
            public const string spWTE_excellalite_invoices_items_Delete = "spWTE_excellalite_invoices_items_Delete";
            public const string spUser_GetByGUID = "spUser_GetByGUID";
            public const string spWTE_excellalite_invoices_items_GetByInvoiceID = "spWTE_excellalite_invoices_items_GetByInvoiceID";
            public const string spWTE_excellalite_invoices_InsertItem = "spWTE_excellalite_invoices_InsertItem";
            public const string spWTE_excellalite_invoices_GetByUserID = "spWTE_excellalite_invoices_GetByUserID";
            public const string spUserSettings_DeleteByUserIDAndName = "spUserSettings_DeleteByUserIDAndName";
            public const string spUserSettings_UpdateByUserID = "spUserSettings_UpdateByUserID";
            public const string spUserSettings_GetByUserID = "spUserSettings_GetByUserID";
            public const string spUserSettings_Insert = "spUserSettings_Insert";
            public const string spUserSettings_GetAllDefaults = "spUserSettings_GetAllDefaults";
            public const string sp_help = "sp_help";
            public const string sp_helptext = "sp_helptext";
            public const string spRequestLogs_Insert = "spRequestLogs_Insert";
            public const string spWebErrorLogs_Insert = "spWebErrorLogs_Insert";
            public const string spWebsites_GetByID = "spWebsites_GetByID";
            public const string spWebsites_SetUpgrade = "spWebsites_SetUpgrade";
            public const string spWebsitesRestart_Insert = "spWebsitesRestart_Insert";
            public const string spWebErrorLogs_Select = "spWebErrorLogs_Select";
            public const string spUsers_GetByLoginID = "spUsers_GetByLoginID";
            public const string spAppSettings_GetByKeywords = "spAppSettings_GetByKeywords";
            public const string spAppSettings_Insert = "spAppSettings_Insert";
            public const string spAppSettings_GetByID = "spAppSettings_GetByID";
            public const string spAppSettings_Update = "spAppSettings_Update";
            public const string spUSStates_Select = "spUSStates_Select";
            public const string spUser_GetByID = "spUser_GetByID";
            public const string spUser_Update = "spUser_Update";
            public const string spUser_Insert = "spUser_Insert";
            public const string spUsersRoles_InsertByLoginID = "spUsersRoles_InsertByLoginID";
            public const string spUsers_GetByKeywords = "spUsers_GetByKeywords";
            public const string spUsersRoles_DeleteByLoginID = "spUsersRoles_DeleteByLoginID";
            public const string spReportAccess_GetByUserID = "spReportAccess_GetByUserID";
            public const string spEmailLog_Insert = "spEmailLog_Insert";
            public const string spUsers_GetByRole = "spUsers_GetByRole";
            public const string spUploadFile_Insert = "spUploadFile_Insert";
            public const string spUploadFile_UpdateStatus = "spUploadFile_UpdateStatus";
            public const string spUploadFile_GetByKeywords = "spUploadFile_GetByKeywords";
            public const string spAnnouncement_Insert = "spAnnouncement_Insert";
            public const string spAnnouncement_Delete = "spAnnouncement_Delete";
            public const string spUploadFile_GetByID = "spUploadFile_GetByID";
            public const string spAnnouncement_GetByKeywords = "spAnnouncement_GetByKeywords";
            public const string spAppTask_Select = "spAppTask_Select";
            public const string spAppTask_Get = "spAppTask_Get";
            public const string spAppTask_UpdateLastExecuted = "spAppTask_UpdateLastExecuted";
            public const string spActivityHistory_Select = "spActivityHistory_Select";
            public const string spActivityHistory_GetByID = "spActivityHistory_GetByID";
            public const string spReportAccess_GetByReportID = "spReportAccess_GetByReportID";
            public const string spReportAccess_Update = "spReportAccess_Update";
            public const string spReports_GetReports = "spReports_GetReports";
            public const string spReportParameter_GetByReportID = "spReportParameter_GetByReportID";
            public const string spReports_GetByKeywords = "spReports_GetByKeywords";
            public const string spReport_GetByReportID = "spReport_GetByReportID";
            public const string spReport_Update = "spReport_Update";
            public const string spTool_Select = "spTool_Select";
            public const string spTool_GetByID = "spTool_GetByID";
            public const string spCheckIfSPNameCanBeExecuted = "spCheckIfSPNameCanBeExecuted";
            public const string spToolLog_SelectByToolID = "spToolLog_SelectByToolID";
            public const string spToolLog_Insert = "spToolLog_Insert";
            public const string spDocumentUserComment_GetByDocumentID = "spDocumentUserComment_GetByDocumentID";
            public const string spDocumentUserComment_InsertByDocumentID = "spDocumentUserComment_InsertByDocumentID";
            public const string spDocumentUserComment_InsertByClaimID = "spDocumentUserComment_InsertByClaimID";
            public const string spDocumentIdentification_GetByKeyword = "spDocumentIdentification_GetByKeyword";
            public const string spUser_AuthorizeExternalLogin = "spUser_AuthorizeExternalLogin";
            public const string spForgotPasswordRequest_Insert = "spForgotPasswordRequest_Insert";
            public const string spForgotPasswordRequest_GetByKeys = "spForgotPasswordRequest_GetByKeys";
            public const string spUser_GetByEmail = "spUser_GetByEmail";
            public const string spUser_UpdatePassword = "spUser_UpdatePassword";
            public const string spActivityHistory_Insert = "spActivityHistory_Insert";
            public const string spFTPLog_Insert = "spFTPLog_Insert";

        }

        public static class PageItems
        {
            public const string RequestManager = "RequestManager";
            public const string UserManager = "UserManager";
            public const string ExecutionTime = "ExecutionTime";
            public const string AnyLiveGames = "AnyLiveGames";
        }

        public static class ErrorMessages
        {
            public const string PDFNotDirectAccess = "PDF Error: Not authorized in the virtual path. Path {0}.";
            public const string PDFNotAuthInDB = "PDF Error: Not found/authorized in the DB. InvoiceID {0}.";
            public const string PDFNotFoundInFolder = "PDF Error: Not found in the folder. File Name {0}.";
            public const string Application_ErrorFormatingEmail = "Application_ErrorFormatingEmail {0}";
            public const string EmailRegistrationError = "EmailRegistrationError {0} ";
            public const string EncryptionError = "EncryptionError. {0} ";
            public const string Application_Error = "Application_Error. ";
            public const string ExecuteDataTable = "ExecuteDataTable. ";
            public const string ExecuteNonQuery = "ExecuteNonQuery. ";
            public const string ExecuteDataSet = "ExecuteDataSet. ";
            public const string LoadingAppSettingXML = "LoadingAppSettingXML. XML:{0}. ";
            public const string LoadingXMLFromLib = "LoadingXMLFromLib. XMLURL:{0}. ";
            public const string DuplicateAppSetting = "Duplicate AppSetting Entry. Key:{0}. ";
            public const string NotFoundAppSetting = "Not Found AppSetting. Key:{0}. ";
            public const string TablesInDataSet = "TablesInDataSet unexpected.";
            public const string NoSessionObject = "NoSessionObject";
            public const string NullSession = "NullSession. {0} ";
            public const string NullApplication = "NullApplication. {0}";
            public const string CastUserManager = "CastUserManager. {0} ";
            public const string NullUserManager = "NullUserManager. {0} ";
            public const string NullRequestManager = "NullRequestManager. {0} ";
            public const string XDocumentParseErrorInWs = "XDocumentParseErrorInWs";
            public const string ErrorSavingAssessment = "ErrorSavingAssessment {0}";
            public const string ErrorCreatingPDF = "ErrorCreatingPDF {0}";
            public const string ErrorJSONSerialize = "ErrorJSONSerialize {0}";
            public const string ErrorJSONDeserialize = "ErrorJSONDeserialize {0}";
            public const string WebRequestURL = "WebRequestURL {0}";
            public const string ErrorLogingTooLog = "ErrorLogingTooLog {0}";
            public const string ErrorSendingEmail = "ErrorSendingEmail {0}";
            public const string ErrorLoggingEmail = "ErrorLoggingEmail {0}";
            public const string ErrorLoadingValueFromFile = "ErrorLoadingValueFromFile {0}";
            public const string ErrorUploadFile = "ErrorUploadFile {0}";
            public const string ErrorReadingFile = "ErrorReadingFile {0}";
            public const string ErrorSaveReviewResult = "ErrorSaveReviewResult {0}";
            public const string FTPUpload = "FTP Upload {0}";
        }

        public static class Common
        {
            public const string WaitingForProcess = "Waiting for process";
            public const string ExternalModeAuto = "AUTO";
            public const string ExternalModeExternal = "EXTERNAL";
            public const string USCountry = "US";
            public const string Draw = "Draw";
            public const string xmls = "*.xml";
            public const string pgns = "*.pgn";
            public const string fens = "*.fen";
            public const string txts = "*.txt";
            public const string PasswordOnConString = "password=";
            public const string ConnectionStringNoShow = "-connection strings are stored in different place -";
            public const string SimpleDateExpression = "M/d/yyyy";
            public const string CurrencyExpression = "$#0.00";
            public const string SimpleDateFormat = "{0:M/d/yyyy}";
            public const string CurrencyFormat = "${0:n}";
            public const string Generic = "GENERIC";
            public const string AutoGenerated = "- auto generated ID -";
            public const string JSONContentType = "plain/text";
            public const string JSONPContentType = "application/javascript";
            public const string JPEGContentType = "image/jpeg";
            public const string TEXTContentType = "text/plain";
            public const string PDFContentType = "application/pdf";
            public const string DBFContentType = "application/x-msdownload";
            public const string POSTMethod = "POST";
            public const string GETMethod = "GET";
            public const string http = "http";
            public const string https = "https";
            public const string httpURL = "http://";
            public const string UserIDForSYSTEM = "1";
            public const string LoginIDForSYSTEM = "system";
            public const string SortDirectionASC = "ASC";
            public const string SortDirectionDESC = "DESC";
            public const string RootTag = "Root";
            public const string True = "True";
            public const string False = "False";
            public const string PasswordMarker = "*******";
            public const string ContentDisposition = "Content-Disposition";
            public const string ASPNETSessionIDCookie = "ASP.NET_SessionId";
            public const string URLReturn = "URLReturn";
            public const string InvoiceNotFound = ApplicationPath.AppFiles + "INVOICE-NOT-FOUND.pdf";

            public struct AppSetting
            {
                public const string XPath = "AppSettings/Data";
                public const string XPathWithName = "AppSettings/Data[Name='{0}']";
                public const string Type = "Type";
                public const string Name = "Name";
                public const string Value = "Value";
                public const string Notes = "Notes";
                public const string WebServer = "WebServer";
                public const string CanBeInJavascript = "CanBeInJavascript";
                public const string CanBeEdited = "CanBeEdited";
                public const string IsXMLData = "IsXMLData";
                public const string IsEncrypted = "IsEncrypted";
                public const string IsFile = "IsFile";
            }



        }

    }

}
