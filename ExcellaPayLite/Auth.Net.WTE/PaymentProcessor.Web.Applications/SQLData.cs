using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data.SqlClient;
    using System.Data;
    using System.Xml.Linq;
    using System.IO;
    using System.Xml;
    using System.Data.SqlTypes;

    public static class SQLData
    {

        public static DRspWTE_excellalite_invoices_notes_GetAllNotes spWTE_excellalite_invoices_notes_GetAllNotes(int UserID, int invoiceID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWTE_excellalite_invoices_notes_GetAllNotes);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@invoiceID", SqlDbType.Int).Value = invoiceID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspWTE_excellalite_invoices_notes_GetAllNotes(aps);
        }

        public static DRspWTE_excellalite_invoices_notes_InsertItem spWTE_excellalite_invoices_notes_InsertItem(int invoiceID, int invoiceItemID, int NoteUserID, string invoiceNote)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWTE_excellalite_invoices_notes_InsertItem);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@invoiceID", SqlDbType.Int).Value = invoiceID;
            command.Parameters.Add("@invoiceItemID", SqlDbType.Int).Value = invoiceItemID;
            command.Parameters.Add("@NoteUserID", SqlDbType.Int).Value = NoteUserID;
            command.Parameters.Add("@invoiceNote", SqlDbType.VarChar).Value = invoiceNote;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspWTE_excellalite_invoices_notes_InsertItem(aps);
        }

        public static DRspWTE_excellalite_invoices_items_GetByInvoiceItemID spWTE_excellalite_invoices_items_GetByInvoiceItemID(int UserID, int InvoiceItemID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWTE_excellalite_invoices_items_GetByInvoiceItemID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@InvoiceItemID", SqlDbType.Int).Value = InvoiceItemID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspWTE_excellalite_invoices_items_GetByInvoiceItemID(aps);
        }


        public static DRspUsers_GetByCustNum spUsers_GetByCustNum(string CustNum)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUsers_GetByCustNum);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@CustNum", SqlDbType.VarChar).Value = CustNum;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUsers_GetByCustNum(aps);
        }


        public static DRspUsers_GetAdministrators spUsers_GetAdministrators()
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUsers_GetAdministrators);
            command.CommandType = CommandType.StoredProcedure;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUsers_GetAdministrators(aps);
        }

        public static DRspWTE_excellalite_invoices_items_Fail spWTE_excellalite_invoices_items_Fail(int InvoiceItemID, int UserID, string PaidNote)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWTE_excellalite_invoices_items_Fail);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@InvoiceItemID", SqlDbType.Int).Value = InvoiceItemID;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@PaidNote", SqlDbType.VarChar).Value = PaidNote;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspWTE_excellalite_invoices_items_Fail(aps);
        }

        public static DRspWTE_excellalite_invoices_GetCustNum spWTE_excellalite_invoices_GetCustNum(int InvoiceStatusID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWTE_excellalite_invoices_GetCustNum);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@InvoiceStatusID", SqlDbType.Int).Value = InvoiceStatusID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspWTE_excellalite_invoices_GetCustNum(aps);
        }


        public static DRspWTE_excellalite_invoices_Archive spWTE_excellalite_invoices_Archive(int UserID, int invoiceID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWTE_excellalite_invoices_Archive);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@invoiceID", SqlDbType.Int).Value = invoiceID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspWTE_excellalite_invoices_Archive(aps);
        }


        public static DRspWTE_excellalite_invoices_items_UpdateToPaid spWTE_excellalite_invoices_items_UpdateToPaid(int UserID, int InvoiceItemID, string PaidNote)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWTE_excellalite_invoices_items_UpdateToPaid);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@InvoiceItemID", SqlDbType.Int).Value = InvoiceItemID;
            command.Parameters.Add("@PaidNote", SqlDbType.VarChar).Value = PaidNote;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspWTE_excellalite_invoices_items_UpdateToPaid(aps);
        }


        public static DRspWTE_excellalite_invoices_items_GetByInvoiceID spWTE_excellalite_invoices_items_GetByInvoiceID(int UserID, int InvoiceID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWTE_excellalite_invoices_items_GetByInvoiceID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@InvoiceID", SqlDbType.Int).Value = InvoiceID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspWTE_excellalite_invoices_items_GetByInvoiceID(aps);
        }


        public static DRspWTE_excellalite_invoices_InsertItem spWTE_excellalite_invoices_InsertItem(int UserID, int invoiceID, decimal Amount, string invoiceNote)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWTE_excellalite_invoices_InsertItem);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@invoiceID", SqlDbType.Int).Value = invoiceID;
            command.Parameters.Add("@Amount", SqlDbType.Money).Value = Amount;
            command.Parameters.Add("@invoiceNote", SqlDbType.VarChar).Value = invoiceNote;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspWTE_excellalite_invoices_InsertItem(aps);
        }

        public static DRspWTE_excellalite_invoices_GetByUserID spWTE_excellalite_invoices_GetByUserID(int UserID, int InvoiceID)
        {
            return spWTE_excellalite_invoices_GetByUserID(UserID, InvoiceID, invoiceStatusIDs.NA, "");
        }

        public static DRspWTE_excellalite_invoices_GetByUserID spWTE_excellalite_invoices_GetByUserID(int UserID, int InvoiceID, invoiceStatusIDs InvoiceStatusID, string CustNum)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWTE_excellalite_invoices_GetByUserID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@InvoiceID", SqlDbType.Int).Value = InvoiceID;
            if (InvoiceStatusID != invoiceStatusIDs.NA)
            {
                command.Parameters.Add("@InvoiceStatusID", SqlDbType.Int).Value = (int)InvoiceStatusID;
            }
            if (CustNum.Length > 0)
            {
                command.Parameters.Add("@CustNum", SqlDbType.NVarChar).Value = CustNum;
            }
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspWTE_excellalite_invoices_GetByUserID(aps);
        }

        public static DRspRequestLogs_Insert spRequestLogs_Insert(TimeSpan timeSpan, string IPAddress, string ScriptName)
        {
            return new DRspRequestLogs_Insert(new DataSet());
        }

        public static DRspUSStates_Select spUSStates_Select()
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUSStates_Select);
            command.CommandType = CommandType.StoredProcedure;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUSStates_Select(aps);
        }

        public static DRspWebErrorLogs_Insert spWebErrorLogs_Insert(
              string WebErrorMessage
            , string WebEnvironment
            , string WebServer
            , string IPAddress
            , string WebUser
            , string WebPage
            , HttpMethods HttpMethod
            )
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWebErrorLogs_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@WebErrorMessage", SqlDbType.VarChar).Value = WebErrorMessage;
            command.Parameters.Add("@WebEnvironment", SqlDbType.VarChar).Value = WebEnvironment;
            command.Parameters.Add("@WebServer", SqlDbType.VarChar).Value = WebServer;
            command.Parameters.Add("@IPAddress", SqlDbType.VarChar).Value = IPAddress;
            command.Parameters.Add("@WebUser", SqlDbType.VarChar).Value = WebUser;
            command.Parameters.Add("@WebPage", SqlDbType.VarChar).Value = WebPage;
            command.Parameters.Add("@HttpMethod", SqlDbType.Int).Value = (int)HttpMethod;
            DataSet aps = DB.ExecuteDataSet(command, false); // do not recursive error log if happens
            return new DRspWebErrorLogs_Insert(aps);
        }


        public static DRspUsers_GetByLoginID spUsers_GetByLoginID(string LoginID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUsers_GetByLoginID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@LoginID", SqlDbType.VarChar).Value = LoginID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUsers_GetByLoginID(aps);
        }

        public static DRspAppSettings_GetByKeywords spAppSettings_GetByKeywords(string Keywords)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spAppSettings_GetByKeywords);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Keywords", SqlDbType.VarChar).Value = Keywords;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspAppSettings_GetByKeywords(aps);
        }

        public static DRspAppSettings_Insert spAppSettings_Insert(string Name, string Value)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spAppSettings_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Name", SqlDbType.VarChar).Value = Name;
            command.Parameters.Add("@Value", SqlDbType.VarChar).Value = Value;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspAppSettings_Insert(aps);
        }

        public static DRspAppSettings_GetByID spAppSettings_GetByID(string Name)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spAppSettings_GetByID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Name", SqlDbType.VarChar).Value = Name;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspAppSettings_GetByID(aps);
        }

        public static DRspAppSettings_Update spAppSettings_Update(int UserID, string Name, string Value)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spAppSettings_Update);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@Name", SqlDbType.VarChar).Value = Name;
            command.Parameters.Add("@Value", SqlDbType.VarChar).Value = Value;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspAppSettings_Update(aps);
        }

        public static DRspWebsites_GetByID spWebsites_GetByID(websiteIDs WebsiteID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWebsites_GetByID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@WebsiteID", SqlDbType.Int).Value = (int)WebsiteID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspWebsites_GetByID(aps);
        }

        public static DRspWebsites_SetUpgrade updateWebsitesStartUpgrade(int UserID, websiteIDs WebsiteID)
        {
            return spWebsites_SetUpgrade(UserID, WebsiteID, true);
        }

        public static DRspWebsites_SetUpgrade updateWebsitesFinishUpgrade(int UserID, websiteIDs WebsiteID)
        {
            return spWebsites_SetUpgrade(UserID, WebsiteID, false);
        }

        private static DRspWebsites_SetUpgrade spWebsites_SetUpgrade(int UserID, websiteIDs WebsiteID, bool IsUpgrading)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWebsites_SetUpgrade);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@WebsiteID", SqlDbType.Int).Value = (int)WebsiteID;
            command.Parameters.Add("@IsUpgrading", SqlDbType.Bit).Value = IsUpgrading ? 1 : 0;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspWebsites_SetUpgrade(aps);
        }

        public static DRspWebsitesRestart_Insert spWebsitesRestart_Insert(websiteIDs WebsiteID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWebsitesRestart_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@WebsiteID", SqlDbType.Int).Value = (int)WebsiteID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspWebsitesRestart_Insert(aps);
        }

        public static DRspWebErrorLogs_Select spWebErrorLogs_Select(int MaxResult, Int64 LastWebErrorID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWebErrorLogs_Select);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@MaxResult", SqlDbType.Int).Value = MaxResult;
            command.Parameters.Add("@LastWebErrorID", SqlDbType.BigInt).Value = LastWebErrorID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspWebErrorLogs_Select(aps);
        }

        public static DRspAppTask_Get spAppTask_Get()
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spAppTask_Get);
            command.CommandType = CommandType.StoredProcedure;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspAppTask_Get(aps);
        }

        public static DRspAppTask_UpdateLastExecuted spAppTask_UpdateLastExecuted(int AppTaskID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spAppTask_UpdateLastExecuted);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@AppTaskID", SqlDbType.Int).Value = AppTaskID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspAppTask_UpdateLastExecuted(aps);
        }

        public static DRspReportAccess_GetByUserID spReportAccess_GetByUserID(int UserID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spReportAccess_GetByUserID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspReportAccess_GetByUserID(aps);
        }

        public static DRspEmailLog_Insert spEmailLog_Insert(string EmailFrom, string EmailTo, string Subject, string Body, bool Result)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spEmailLog_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@EmailFrom", SqlDbType.VarChar).Value = EmailFrom;
            command.Parameters.Add("@EmailTo", SqlDbType.VarChar).Value = EmailTo;
            command.Parameters.Add("@Subject", SqlDbType.VarChar).Value = Subject;
            command.Parameters.Add("@Body", SqlDbType.Text).Value = String.IsNullOrEmpty(Body) ? String.Empty : Body;
            command.Parameters.Add("@Result", SqlDbType.Bit).Value = Result ? 1 : 0;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspEmailLog_Insert(aps);
        }

        /// <summary>
        /// Get Unique Users by RoleIDs (separated by commas) 
        /// </summary>
        /// <param name="RoleIDs"></param>
        /// <returns></returns>
        public static DRspUsers_GetByRole spUsers_GetByRole(string RoleIDs)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUsers_GetByRole);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@RoleIDs", SqlDbType.VarChar).Value = RoleIDs;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUsers_GetByRole(aps);
        }

        public static DRspUploadFile_Insert spUploadFile_Insert(int UserID, string FileDescription, string FileName)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUploadFile_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@FileDescription", SqlDbType.VarChar).Value = FileDescription;
            command.Parameters.Add("@FileName", SqlDbType.VarChar).Value = FileName;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUploadFile_Insert(aps);
        }

        public static DRspUploadFile_UpdateStatus spUploadFile_UpdateStatus(int UploadFileID, int UserID, bool FileStatus)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUploadFile_UpdateStatus);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UploadFileID", SqlDbType.Int).Value = UploadFileID;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@FileStatus", SqlDbType.Bit).Value = FileStatus ? 1 : 0;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUploadFile_UpdateStatus(aps);
        }

        public static DRspUploadFile_GetByKeywords spUploadFile_GetByKeywords(int MaxResult, string Keywords)
        {
            return spUploadFile_GetByKeywords(MaxResult, Keywords, String.Empty);
        }

        public static DRspUploadFile_GetByKeywords spUploadFile_GetByKeywords(int MaxResult, string Keywords, string Extension)
        {
            // Extension no dot
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUploadFile_GetByKeywords);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@MaxResult", SqlDbType.Int).Value = MaxResult;
            command.Parameters.Add("@Keywords", SqlDbType.VarChar).Value = Keywords;
            command.Parameters.Add("@Extension", SqlDbType.VarChar).Value = Extension;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUploadFile_GetByKeywords(aps);
        }

        public static DRspAnnouncement_Insert spAnnouncement_Insert(int UserID, bool IsSentEmail, string Subject, string Messages, string IsForRoles, string UploadFileIDs)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spAnnouncement_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@IsSentEmail", SqlDbType.Bit).Value = IsSentEmail ? 1 : 0;
            command.Parameters.Add("@Subject", SqlDbType.VarChar).Value = Subject;
            command.Parameters.Add("@Messages", SqlDbType.Text).Value = String.IsNullOrEmpty(Messages) ? "" : Messages;
            command.Parameters.Add("@IsForRoles", SqlDbType.VarChar).Value = IsForRoles;
            command.Parameters.Add("@UploadFileIDs", SqlDbType.VarChar).Value = UploadFileIDs;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspAnnouncement_Insert(aps);
        }

        public static DRspAnnouncement_Delete spAnnouncement_Delete(int UserID, int AnnouncementID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spAnnouncement_Delete);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@AnnouncementID", SqlDbType.Int).Value = AnnouncementID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspAnnouncement_Delete(aps);
        }

        public static DRspUploadFile_GetByID spUploadFile_GetByID(int UploadFileID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUploadFile_GetByID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UploadFileID", SqlDbType.Int).Value = UploadFileID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUploadFile_GetByID(aps);
        }

        public static DRspAppTask_Select spAppTask_Select()
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spAppTask_Select);
            command.CommandType = CommandType.StoredProcedure;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspAppTask_Select(aps);
        }

        public static DRspAnnouncement_GetByKeywords spAnnouncement_GetByKeywords(int MaxResult, string Keywords)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spAnnouncement_GetByKeywords);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@MaxResult", SqlDbType.Int).Value = MaxResult;
            command.Parameters.Add("@Keywords", SqlDbType.VarChar).Value = Keywords;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspAnnouncement_GetByKeywords(aps);
        }

        public static DRspWTE_excellalite_invoices_items_Delete spWTE_excellalite_invoices_items_Delete(int InvoiceItemID, int UserID, string invoiceNote)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spWTE_excellalite_invoices_items_Delete);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@InvoiceItemID", SqlDbType.Int).Value = InvoiceItemID;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@invoiceNote", SqlDbType.VarChar).Value = invoiceNote;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspWTE_excellalite_invoices_items_Delete(aps);
        }


        public static DRspUser_GetByGUID spUser_GetByGUID(string BackdoorGUID)
        {
            Guid sq;
            Guid.TryParse(BackdoorGUID, out sq);
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUser_GetByGUID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@BackdoorGUID", SqlDbType.UniqueIdentifier).Value = sq;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUser_GetByGUID(aps);
        }

        public static DRspUsers_Select spUsers_Select(string Keywords, string IncludeRoles)
        {
            return spUsers_GetByKeywords(Keywords, IncludeRoles);
        }


        public static DRspUsers_Select spUsers_GetByKeywords(string Keywords, string IncludeRoles)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUsers_GetByKeywords);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Keywords", SqlDbType.VarChar).Value = Keywords;
            command.Parameters.Add("@IncludeRoles", SqlDbType.VarChar).Value = IncludeRoles;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUsers_Select(aps);
        }

        public static DRspUser_GetByID spUser_GetByID(int UserID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUser_GetByID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUser_GetByID(aps);
        }

        public static DRspUser_Update spUser_Update(int ModifierID, int UserID, string FullName, string LoginID, string Email, bool IsActive, bool IsPasswordUpdate, string ExternalPassword, string Phone, string CustNum, string BackdoorGUID)
        {
            Guid BackGuid;
            if (!Guid.TryParse(BackdoorGUID, out BackGuid))
            {
                BackGuid = Guid.NewGuid();
            }
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUser_Update);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@ModifierID", SqlDbType.Int).Value = ModifierID;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@FullName", SqlDbType.VarChar).Value = FullName;
            command.Parameters.Add("@LoginID", SqlDbType.VarChar).Value = LoginID;
            command.Parameters.Add("@Email", SqlDbType.VarChar).Value = Email;
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = IsActive ? 1 : 0;
            command.Parameters.Add("@IsPasswordUpdate", SqlDbType.Bit).Value = IsPasswordUpdate ? 1 : 0;
            command.Parameters.Add("@ExternalPassword", SqlDbType.VarChar).Value = ExternalPassword;
            command.Parameters.Add("@Phone", SqlDbType.VarChar).Value = Phone;
            command.Parameters.Add("@CustNum", SqlDbType.VarChar).Value = CustNum;
            command.Parameters.Add("@BackdoorGUID", SqlDbType.UniqueIdentifier).Value = BackGuid;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUser_Update(aps);
        }


        /// <summary>
        /// Pass external password no encrypted. This method will encrypt before saving to the database.
        /// </summary>
        /// <param name="CreatorID"></param>
        /// <param name="FullName"></param>
        /// <param name="LoginID"></param>
        /// <param name="Email"></param>
        /// <param name="IsActive"></param>
        /// <param name="ExternalPassword"></param>
        /// <returns></returns>
        public static DRspUser_Insert spUser_Insert(int CreatorID, string FullName, string LoginID, string Email, string Phone, bool IsActive, string ExternalPassword, string CustNum, bool IsAdmin)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUser_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@CreatorID", SqlDbType.Int).Value = CreatorID;
            command.Parameters.Add("@FullName", SqlDbType.VarChar).Value = FullName;
            command.Parameters.Add("@LoginID", SqlDbType.VarChar).Value = LoginID;
            command.Parameters.Add("@Email", SqlDbType.VarChar).Value = Email;
            command.Parameters.Add("@Phone", SqlDbType.VarChar).Value = Phone;
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = IsActive ? 1 : 0;
            command.Parameters.Add("@ExternalPassword", SqlDbType.VarChar).Value = String.IsNullOrEmpty(ExternalPassword) ? String.Empty : EncryptionManager.EncryptExternalPassword(ExternalPassword);
            command.Parameters.Add("@CustNum", SqlDbType.VarChar).Value = CustNum;
            command.Parameters.Add("@IsAdmin", SqlDbType.Bit).Value = IsAdmin ? 1 : 0;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUser_Insert(aps);
        }

        public static DRspUsersRoles_InsertByLoginID spUsersRoles_InsertByLoginID(string LoginID, int RoleID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUsersRoles_InsertByLoginID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@LoginID", SqlDbType.VarChar).Value = LoginID;
            command.Parameters.Add("@RoleID", SqlDbType.Int).Value = RoleID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUsersRoles_InsertByLoginID(aps);
        }

        public static DRspUsersRoles_DeleteByLoginID spUsersRoles_DeleteByLoginID(string LoginID, int RoleID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUsersRoles_DeleteByLoginID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@LoginID", SqlDbType.VarChar).Value = LoginID;
            command.Parameters.Add("@RoleID", SqlDbType.Int).Value = RoleID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUsersRoles_DeleteByLoginID(aps);
        }

        public static DRspActivityHistory_Select spActivityHistory_Select(int MaxResult)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spActivityHistory_Select);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@MaxResult", SqlDbType.Int).Value = MaxResult;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspActivityHistory_Select(aps);
        }

        public static DRspActivityHistory_Insert spActivityHistory_Insert(activityIDs ActivityID, int UserID, long? UniqueID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spActivityHistory_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@ActivityID", SqlDbType.Int).Value = ActivityID;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@UniqueID", SqlDbType.BigInt).Value = UniqueID == null ? null : UniqueID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspActivityHistory_Insert(aps);
        }


        public static DRspReportParameter_GetByReportID spReportParameter_GetByReportID(int ReportID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spReportParameter_GetByReportID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@ReportID", SqlDbType.Int).Value = ReportID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspReportParameter_GetByReportID(aps);
        }

        /// <summary>
        /// This method and getParametersFromSP have to be compatible in data return
        /// One for simple report, the other one for custom report
        /// </summary>
        /// <param name="ReportID"></param>
        /// <returns></returns>
        public static List<DataParameter> getParametersFromReportParameter(int ReportID)
        {
            List<DataParameter> parameters = new List<DataParameter>();
            DRspReportParameter_GetByReportID prs = spReportParameter_GetByReportID(ReportID);
            if (prs.Count > 0)
            {
                for (int i = 0; i < prs.Count; i++)
                {
                    DataParameter nd = new DataParameter()
                    {
                        label = prs.ParamLabel(i),
                        name = prs.ParamName(i),
                        type = prs.ParamType(i),
                        value = prs.ParamDefaultValue(i)
                    };
                    parameters.Add(nd);
                }
            }
            return parameters;
        }

        public static DRspUserSettings_DeleteByUserIDAndName spUserSettings_DeleteByUserIDAndName(int UserID, string Name)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUserSettings_DeleteByUserIDAndName);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@Name", SqlDbType.VarChar).Value = Name;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUserSettings_DeleteByUserIDAndName(aps);
        }


        public static DRspUserSettings_UpdateByUserID spUserSettings_UpdateByUserID(int UserID, string Name, string Value)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUserSettings_UpdateByUserID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@Name", SqlDbType.VarChar).Value = Name;
            command.Parameters.Add("@Value", SqlDbType.VarChar).Value = Value;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUserSettings_UpdateByUserID(aps);
        }


        public static DRspUserSettings_GetByUserID spUserSettings_GetByUserID(int UserID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUserSettings_GetByUserID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUserSettings_GetByUserID(aps);
        }


        public static DRspUserSettings_Insert spUserSettings_Insert(int UserID, string Name, string Value)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUserSettings_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@Name", SqlDbType.VarChar).Value = Name;
            command.Parameters.Add("@Value", SqlDbType.VarChar).Value = Value;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUserSettings_Insert(aps);
        }


        public static DRspUserSettings_GetAllDefaults spUserSettings_GetAllDefaults()
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUserSettings_GetAllDefaults);
            command.CommandType = CommandType.StoredProcedure;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUserSettings_GetAllDefaults(aps);
        }

        public static DRspReports_GetReports spReports_GetReports()
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spReports_GetReports);
            command.CommandType = CommandType.StoredProcedure;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspReports_GetReports(aps);
        }

        public static DRspActivityHistory_GetByID spActivityHistory_GetByID(Int64 ActivityHistoryID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spActivityHistory_GetByID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@ActivityHistoryID", SqlDbType.BigInt).Value = ActivityHistoryID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspActivityHistory_GetByID(aps);
        }

        public static DRspReports_GetByKeywords spReports_GetByKeywords(string Keywords)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spReports_GetByKeywords);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Keywords", SqlDbType.VarChar).Value = Keywords;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspReports_GetByKeywords(aps);
        }

        public static DRspReportAccess_GetByReportID spReportAccess_GetByReportID(int ReportID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spReportAccess_GetByReportID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@ReportID", SqlDbType.Int).Value = ReportID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspReportAccess_GetByReportID(aps);
        }

        public static DRspReport_GetByReportID spReport_GetByReportID(int ReportID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spReport_GetByReportID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@ReportID", SqlDbType.Int).Value = ReportID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspReport_GetByReportID(aps);
        }

        public static DRspReport_Update spReport_Update(int UserID, int ReportID, bool Enable, string Title, bool IsDefaultDenied)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spReport_Update);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@ReportID", SqlDbType.Int).Value = ReportID;
            command.Parameters.Add("@Enable", SqlDbType.Bit).Value = Enable ? 1 : 0;
            command.Parameters.Add("@Title", SqlDbType.VarChar).Value = Title;
            command.Parameters.Add("@IsDefaultDenied", SqlDbType.Bit).Value = IsDefaultDenied ? 1 : 0;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspReport_Update(aps);
        }

        public static DRspReportAccess_Update spReportAccess_Update(int UserID, int Action, int ReportID, bool IsRole, int ExceptionRoleOrUserID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spReportAccess_Update);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@Action", SqlDbType.Int).Value = Action;
            command.Parameters.Add("@ReportID", SqlDbType.Int).Value = ReportID;
            command.Parameters.Add("@IsRole", SqlDbType.Bit).Value = IsRole ? 1 : 0;
            command.Parameters.Add("@ExceptionRoleOrUserID", SqlDbType.Int).Value = ExceptionRoleOrUserID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspReportAccess_Update(aps);
        }

        public static DRspTool_Select spTool_Select()
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spTool_Select);
            command.CommandType = CommandType.StoredProcedure;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTool_Select(aps);
        }

        public static DRspTool_GetByID spTool_GetByID(int ToolID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spTool_GetByID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@ToolID", SqlDbType.Int).Value = ToolID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTool_GetByID(aps);
        }

        public static DRspCheckIfSPNameCanBeExecuted spCheckIfSPNameCanBeExecuted(string StoredProcedureName)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spCheckIfSPNameCanBeExecuted);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@StoredProcedureName", SqlDbType.VarChar).Value = StoredProcedureName;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspCheckIfSPNameCanBeExecuted(aps);
        }

        public static DRspToolLog_SelectByToolID spToolLog_SelectByToolID(int MaxResult, int ToolID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spToolLog_SelectByToolID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@MaxResult", SqlDbType.Int).Value = MaxResult;
            command.Parameters.Add("@ToolID", SqlDbType.Int).Value = ToolID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspToolLog_SelectByToolID(aps);
        }

        public static DRspToolLog_Insert spToolLog_Insert(int ToolID, int UserID, XDocument ReturnSet)
        {
            byte[] buffer = System.Text.UTF8Encoding.UTF8.GetBytes(ReturnSet.ToString());
            MemoryStream ms = new MemoryStream(buffer);
            XmlTextReader xr = new XmlTextReader(ms);
            xr.DtdProcessing = DtdProcessing.Ignore;

            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spToolLog_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@ToolID", SqlDbType.Int).Value = ToolID;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@XMLDataSet", SqlDbType.Xml).Value = new SqlXml(xr);
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspToolLog_Insert(aps);
        }

        /// <summary>
        /// This method and getParametersFromReportParameter have to be compatible in data return
        /// One for simple report, the other one for custom report
        /// </summary>
        /// <param name="spName"></param>
        /// <returns></returns>
        public static List<DataParameter> getParametersFromSP(string spName)
        {
            List<DataParameter> parameters = new List<DataParameter>();
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.sp_help);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@objname", SqlDbType.VarChar).Value = String.Format("[dbo].[{0}]", spName);
            DataSet aps = DB.ExecuteDataSet(command);
            int lt = aps.Tables.Count;
            if (lt > 1)
            {
                DataTable dt = aps.Tables[lt - 1];
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string prname = dt.Rows[i]["Parameter_name"].ToString();
                        if (prname.StartsWith("@")) // delete @, it has to be same with name from getParametersFromReportParameter
                        {
                            prname = prname.Substring(1);
                        }
                        DataParameter nd = new DataParameter()
                        {
                            label = prname.Replace("_", " "), // for lable in input parameter use under score to be replaced by space
                            name = prname,
                            type = dt.Rows[i]["Type"].ToString(),   // it has to be same with type from ReportParameter
                            value = string.Empty // default value. we can not have it in simple report. move to custom report if need it
                        };

                        parameters.Add(nd);
                    }
                }
            }
            return parameters;
        }

        public static DRspDocumentUserComment_GetByDocumentID spDocumentUserComment_GetByDocumentID(Int64 DocumentID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spDocumentUserComment_GetByDocumentID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@DocumentID", SqlDbType.BigInt).Value = DocumentID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspDocumentUserComment_GetByDocumentID(aps);
        }

        public static DRspDocumentIdentification_GetByKeyword spDocumentIdentification_GetByKeyword(int MaxResult, string Keyword)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spDocumentIdentification_GetByKeyword);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@MaxResult", SqlDbType.Int).Value = MaxResult;
            command.Parameters.Add("@Keyword", SqlDbType.VarChar).Value = Keyword;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspDocumentIdentification_GetByKeyword(aps);
        }

        public static DRspDocumentUserComment_InsertByClaimID spDocumentUserComment_InsertByClaimID(int UserID, int ClaimID, string Comment)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spDocumentUserComment_InsertByClaimID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@ClaimID", SqlDbType.Int).Value = ClaimID;
            command.Parameters.Add("@Comment", SqlDbType.Text).Value = Comment;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspDocumentUserComment_InsertByClaimID(aps);
        }

        public static DRspDocumentUserComment_InsertByDocumentID spDocumentUserComment_InsertByDocumentID(int UserID, Int64 DocumentID, string Comment)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spDocumentUserComment_InsertByDocumentID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@DocumentID", SqlDbType.BigInt).Value = DocumentID;
            command.Parameters.Add("@Comment", SqlDbType.Text).Value = String.IsNullOrEmpty(Comment) ? null : Comment;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspDocumentUserComment_InsertByDocumentID(aps);
        }

        public static DRspUser_AuthorizeExternalLogin spUser_AuthorizeExternalLogin(string LoginID, string ExternalPassword, string BackdoorGUID)
        {
            Guid BackGuid;
            if (!Guid.TryParse(BackdoorGUID, out BackGuid))
            {
                BackGuid = Guid.NewGuid();
            }
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUser_AuthorizeExternalLogin);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@LoginID", SqlDbType.VarChar).Value = LoginID;
            command.Parameters.Add("@ExternalPassword", SqlDbType.VarChar).Value = ExternalPassword == null ? "" : ExternalPassword;
            command.Parameters.Add("@BackdoorGUID", SqlDbType.UniqueIdentifier).Value = BackGuid;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUser_AuthorizeExternalLogin(aps);
        }

        public static DRspForgotPasswordRequest_Insert spForgotPasswordRequest_Insert(string Email, Guid KeyID, int UserID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spForgotPasswordRequest_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Email", SqlDbType.VarChar).Value = Email;
            command.Parameters.Add("@KeyID", SqlDbType.UniqueIdentifier).Value = KeyID;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspForgotPasswordRequest_Insert(aps);
        }

        public static DRspForgotPasswordRequest_GetByKeys spForgotPasswordRequest_GetByKeys(Guid KeyID, int UserID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spForgotPasswordRequest_GetByKeys);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@KeyID", SqlDbType.UniqueIdentifier).Value = KeyID;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspForgotPasswordRequest_GetByKeys(aps);
        }

        public static DRspUser_GetByEmail spUser_GetByEmail(string Email)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUser_GetByEmail);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Email", SqlDbType.VarChar).Value = Email;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUser_GetByEmail(aps);
        }

        public static DRspUser_UpdatePassword spUser_UpdatePassword(int UserID, string NewExternalPassword)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spUser_UpdatePassword);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@NewExternalPassword", SqlDbType.VarChar).Value = NewExternalPassword;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspUser_UpdatePassword(aps);
        }

        public static DRspFTPLog_Insert spFTPLog_Insert(int UserID, string FilePath, string FolderDestination, Int64 DocumentID)
        {
            DataAccess DB = new DataAccess(databaseServer.DefaultDB);
            SqlCommand command = new SqlCommand(SV.SP.spFTPLog_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@FilePath", SqlDbType.VarChar).Value = FilePath;
            command.Parameters.Add("@FolderDestination", SqlDbType.VarChar).Value = FolderDestination;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@DocumentID", SqlDbType.BigInt).Value = DocumentID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspFTPLog_Insert(aps);
        }

    }

}
