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

    public static class SQLDataPayTrace
    {
        #region
        // before saving Request String to the DB, use this
        // PARMLIST=UN~demo123|PSWD~demo123|TERMS~Y|METHOD~ProcessTranx|TRANXTYPE~Sale|INVOICE~1001|BADDRESS~102 York St|BZIP~27705|CC~4111111111111111|CSC~999|EXPMNTH~12|EXPYR~2016|AMOUNT~1.00
        private static string REQUESTMarking(string REQUEST)
        {
            string MM = "";
            int t1 = REQUEST.IndexOf("|PSWD~", 0);
            if (t1 == -1)
            {
                t1 = REQUEST.IndexOf("PSWD~", 0);
            }
            if (t1 > -1)
            {
                t1 = REQUEST.IndexOf("~", t1);
                int t2 = REQUEST.IndexOf("|", t1);
                if (t2 == -1)
                {
                    t2 = REQUEST.Length;
                }
                MM = REQUEST.Substring(0, t1 + 1) + new String('*', t2 - t1) + REQUEST.Substring(t2);
            }
            else
            {
                MM = REQUEST;
            }
            return MM;
        }

        // before saving credit card number to th DB, use this
        private static string CCMarking(string CC)
        {
            string MCC = "";
            if (CC.Length > 4)
            {
                MCC = new String('*', CC.Length - 4) + CC.Substring(CC.Length - 4, 4);
            }
            else
            {
                MCC = CC;
            }
            return MCC;
        }
        #endregion

        public static DRspTA_custom_egiving_Events_UpdateNumber_of_Tickets spTA_custom_egiving_Events_UpdateNumber_of_Tickets(int SiteID, int Egiving_EventsID, int TotalTicketPurchased)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_custom_egiving_Events_UpdateNumber_of_Tickets);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@SiteID", SqlDbType.Int).Value = SiteID;
            command.Parameters.Add("@Egiving_EventsID", SqlDbType.Int).Value = Egiving_EventsID;
            command.Parameters.Add("@TotalTicketPurchased", SqlDbType.Int).Value = TotalTicketPurchased;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_custom_egiving_Events_UpdateNumber_of_Tickets(aps);
        }


        public static DRspTA_DirectTransactions_Events_UpdateAfterPost spTA_DirectTransactions_Events_UpdateAfterPost(int DirectTransactions_EventsID, string INVOICE, int PayTraceRequestID, string PayTraceTRANSACTIONID, string PayTraceAPPMSG)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_DirectTransactions_Events_UpdateAfterPost);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@DirectTransactions_EventsID", SqlDbType.Int).Value = DirectTransactions_EventsID;
            command.Parameters.Add("@INVOICE", SqlDbType.VarChar).Value = INVOICE;
            command.Parameters.Add("@PayTraceRequestID", SqlDbType.Int).Value = PayTraceRequestID;
            command.Parameters.Add("@PayTraceTRANSACTIONID", SqlDbType.VarChar).Value = PayTraceTRANSACTIONID;
            command.Parameters.Add("@PayTraceAPPMSG", SqlDbType.VarChar).Value = PayTraceAPPMSG;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_DirectTransactions_Events_UpdateAfterPost(aps);
        }


        public static DRspTA_DirectTransactions_Events_GetForThanks spTA_DirectTransactions_Events_GetForThanks(SqlGuid UserGUID, int SiteID, int DirectTransactions_EventsID)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_DirectTransactions_Events_GetForThanks);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserGUID", SqlDbType.UniqueIdentifier).Value = UserGUID;
            command.Parameters.Add("@SiteID", SqlDbType.Int).Value = SiteID;
            command.Parameters.Add("@DirectTransactions_EventsID", SqlDbType.Int).Value = DirectTransactions_EventsID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_DirectTransactions_Events_GetForThanks(aps);
        }


        public static DRspTA_DirectTransactions_Events_GetByUserGUID spTA_DirectTransactions_Events_GetByUserGUID(SqlGuid UserGUID, int SiteID)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_DirectTransactions_Events_GetByUserGUID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserGUID", SqlDbType.UniqueIdentifier).Value = UserGUID;
            command.Parameters.Add("@SiteID", SqlDbType.Int).Value = SiteID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_DirectTransactions_Events_GetByUserGUID(aps);
        }


        public static DRspTA_DirectTransactions_Events_Insert spTA_DirectTransactions_Events_Insert(SqlGuid UserGUID, int SiteID, int Egiving_EventsID, string BNAME, string CC, string EXPMNTH, string EXPYR, string AMOUNT, string CSC, string BADDRESS, string BZIP, string Address2, string City, string State, string Email, string Phone)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_DirectTransactions_Events_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserGUID", SqlDbType.UniqueIdentifier).Value = UserGUID;
            command.Parameters.Add("@SiteID", SqlDbType.Int).Value = SiteID;
            command.Parameters.Add("@Egiving_EventsID", SqlDbType.Int).Value = Egiving_EventsID;
            command.Parameters.Add("@BNAME", SqlDbType.VarChar).Value = BNAME;
            command.Parameters.Add("@CC", SqlDbType.VarChar).Value = CCMarking(CC);
            command.Parameters.Add("@EXPMNTH", SqlDbType.VarChar).Value = EXPMNTH;
            command.Parameters.Add("@EXPYR", SqlDbType.VarChar).Value = EXPYR;
            command.Parameters.Add("@AMOUNT", SqlDbType.VarChar).Value = AMOUNT;
            command.Parameters.Add("@CSC", SqlDbType.VarChar).Value = CSC;
            command.Parameters.Add("@BADDRESS", SqlDbType.VarChar).Value = BADDRESS;
            command.Parameters.Add("@BZIP", SqlDbType.VarChar).Value = BZIP;
            command.Parameters.Add("@Address2", SqlDbType.VarChar).Value = Address2;
            command.Parameters.Add("@City", SqlDbType.VarChar).Value = City;
            command.Parameters.Add("@State", SqlDbType.VarChar).Value = State;
            command.Parameters.Add("@Email", SqlDbType.VarChar).Value = Email;
            command.Parameters.Add("@Phone", SqlDbType.VarChar).Value = Phone;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_DirectTransactions_Events_Insert(aps);
        }


        public static DRspTA_custom_egiving_Events_Get spTA_custom_egiving_Events_Get(int SiteID, int Egiving_EventsID)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_custom_egiving_Events_Get);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@SiteID", SqlDbType.Int).Value = SiteID;
            command.Parameters.Add("@Egiving_EventsID", SqlDbType.Int).Value = Egiving_EventsID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_custom_egiving_Events_Get(aps);
        }

        public static DRspTA_RecurringTransactions_UpdateRecurringInfo spTA_RecurringTransactions_UpdateRecurringInfo(int RecurringTransactionID, DateTime START, string RecurringFrequencyID, string TOTALCOUNT, string AMOUNT)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_RecurringTransactions_UpdateRecurringInfo);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@RecurringTransactionID", SqlDbType.Int).Value = RecurringTransactionID;
            command.Parameters.Add("@START", SqlDbType.DateTime).Value = START;
            command.Parameters.Add("@RecurringFrequencyID", SqlDbType.VarChar).Value = RecurringFrequencyID;
            command.Parameters.Add("@TOTALCOUNT", SqlDbType.VarChar).Value = TOTALCOUNT;
            command.Parameters.Add("@AMOUNT", SqlDbType.VarChar).Value = AMOUNT;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_RecurringTransactions_UpdateRecurringInfo(aps);
        }


        public static DRspTA_RecurringTransactions_UpdateCustomerInfo spTA_RecurringTransactions_UpdateCustomerInfo(int RecurringTransactionID, string BNAME, string CC, string EXPMNTH, string EXPYR, string CSC, string BADDRESS, string BZIP, string Address2, string City, string State, string Email, string Phone)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_RecurringTransactions_UpdateCustomerInfo);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@RecurringTransactionID", SqlDbType.Int).Value = RecurringTransactionID;
            command.Parameters.Add("@BNAME", SqlDbType.VarChar).Value = BNAME;
            command.Parameters.Add("@CC", SqlDbType.VarChar).Value = CCMarking(CC);
            command.Parameters.Add("@EXPMNTH", SqlDbType.VarChar).Value = EXPMNTH;
            command.Parameters.Add("@EXPYR", SqlDbType.VarChar).Value = EXPYR;
            command.Parameters.Add("@CSC", SqlDbType.VarChar).Value = CSC;
            command.Parameters.Add("@BADDRESS", SqlDbType.VarChar).Value = BADDRESS;
            command.Parameters.Add("@BZIP", SqlDbType.VarChar).Value = BZIP;
            command.Parameters.Add("@Address2", SqlDbType.VarChar).Value = Address2;
            command.Parameters.Add("@City", SqlDbType.VarChar).Value = City;
            command.Parameters.Add("@State", SqlDbType.VarChar).Value = State;
            command.Parameters.Add("@Email", SqlDbType.VarChar).Value = Email;
            command.Parameters.Add("@Phone", SqlDbType.VarChar).Value = Phone;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_RecurringTransactions_UpdateCustomerInfo(aps);
        }


        public static DRspTA_RecurringTransactions_GetByUserGUID spTA_RecurringTransactions_GetByUserGUID(SqlGuid UserGUID, int SiteID)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_RecurringTransactions_GetByUserGUID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserGUID", SqlDbType.UniqueIdentifier).Value = UserGUID;
            command.Parameters.Add("@SiteID", SqlDbType.Int).Value = SiteID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_RecurringTransactions_GetByUserGUID(aps);
        }


        public static DRspTA_CheckTransactions_GetByUserGUID spTA_CheckTransactions_GetByUserGUID(SqlGuid UserGUID, int SiteID)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_CheckTransactions_GetByUserGUID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserGUID", SqlDbType.UniqueIdentifier).Value = UserGUID;
            command.Parameters.Add("@SiteID", SqlDbType.Int).Value = SiteID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_CheckTransactions_GetByUserGUID(aps);
        }

        public static DRspTA_DirectTransactions_GetByUserGUID spTA_DirectTransactions_GetByUserGUID(SqlGuid UserGUID, int SiteID)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_DirectTransactions_GetByUserGUID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserGUID", SqlDbType.UniqueIdentifier).Value = UserGUID;
            command.Parameters.Add("@SiteID", SqlDbType.Int).Value = SiteID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_DirectTransactions_GetByUserGUID(aps);
        }

        public static DRspTA_RecurringTransactions_GetForThanks spTA_RecurringTransactions_GetForThanks(SqlGuid UserGUID, int SiteID, int RecurringTransactionID)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_RecurringTransactions_GetForThanks);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserGUID", SqlDbType.UniqueIdentifier).Value = UserGUID;
            command.Parameters.Add("@SiteID", SqlDbType.Int).Value = SiteID;
            command.Parameters.Add("@RecurringTransactionID", SqlDbType.Int).Value = RecurringTransactionID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_RecurringTransactions_GetForThanks(aps);
        }


        public static DRspTA_CheckTransactions_GetForThanks spTA_CheckTransactions_GetForThanks(SqlGuid UserGUID, int SiteID, int CheckTransactionID)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_CheckTransactions_GetForThanks);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserGUID", SqlDbType.UniqueIdentifier).Value = UserGUID;
            command.Parameters.Add("@SiteID", SqlDbType.Int).Value = SiteID;
            command.Parameters.Add("@CheckTransactionID", SqlDbType.Int).Value = CheckTransactionID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_CheckTransactions_GetForThanks(aps);
        }


        public static DRspTA_DirectTransactions_GetForThanks spTA_DirectTransactions_GetForThanks(SqlGuid UserGUID, int SiteID, int DirectTransactionID)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_DirectTransactions_GetForThanks);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserGUID", SqlDbType.UniqueIdentifier).Value = UserGUID;
            command.Parameters.Add("@SiteID", SqlDbType.Int).Value = SiteID;
            command.Parameters.Add("@DirectTransactionID", SqlDbType.Int).Value = DirectTransactionID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_DirectTransactions_GetForThanks(aps);
        }


        public static DRspTA_custom_egivings_Get spTA_custom_egivings_Get(int SiteID, int EgivingsID)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_custom_egivings_Get);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@SiteID", SqlDbType.Int).Value = SiteID;
            command.Parameters.Add("@EgivingsID", SqlDbType.Int).Value = EgivingsID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_custom_egivings_Get(aps);
        }


        public static DRspTA_CheckTransactions_UpdateAfterPost spTA_CheckTransactions_UpdateAfterPost(int CheckTransactionID, string INVOICE, int PayTraceRequestID, string PayTraceCHECKIDENTIFIER, string PayTraceRESPONSE)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_CheckTransactions_UpdateAfterPost);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@CheckTransactionID", SqlDbType.Int).Value = CheckTransactionID;
            command.Parameters.Add("@INVOICE", SqlDbType.VarChar).Value = INVOICE;
            command.Parameters.Add("@PayTraceRequestID", SqlDbType.Int).Value = PayTraceRequestID;
            command.Parameters.Add("@PayTraceCHECKIDENTIFIER", SqlDbType.VarChar).Value = PayTraceCHECKIDENTIFIER;
            command.Parameters.Add("@PayTraceRESPONSE", SqlDbType.VarChar).Value = PayTraceRESPONSE;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_CheckTransactions_UpdateAfterPost(aps);
        }

        public static DRspTA_CheckTransactions_Insert spTA_CheckTransactions_Insert(SqlGuid UserGUID, int SiteID, int EgivingsID, string BNAME, string DDA, string TR, string AMOUNT, string BADDRESS, string BZIP, string Address2, string City, string State, string Email, string Phone)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_CheckTransactions_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserGUID", SqlDbType.UniqueIdentifier).Value = UserGUID;
            command.Parameters.Add("@SiteID", SqlDbType.Int).Value = SiteID;
            command.Parameters.Add("@EgivingsID", SqlDbType.Int).Value = EgivingsID;
            command.Parameters.Add("@BNAME", SqlDbType.VarChar).Value = BNAME;
            command.Parameters.Add("@DDA", SqlDbType.VarChar).Value = DDA;
            command.Parameters.Add("@TR", SqlDbType.VarChar).Value = TR;
            command.Parameters.Add("@AMOUNT", SqlDbType.VarChar).Value = AMOUNT;
            command.Parameters.Add("@BADDRESS", SqlDbType.VarChar).Value = BADDRESS;
            command.Parameters.Add("@BZIP", SqlDbType.VarChar).Value = BZIP;
            command.Parameters.Add("@Address2", SqlDbType.VarChar).Value = Address2;
            command.Parameters.Add("@City", SqlDbType.VarChar).Value = City;
            command.Parameters.Add("@State", SqlDbType.VarChar).Value = State;
            command.Parameters.Add("@Email", SqlDbType.VarChar).Value = Email;
            command.Parameters.Add("@Phone", SqlDbType.VarChar).Value = Phone;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_CheckTransactions_Insert(aps);
        }

        public static DRspTA_RecurringTransactions_Delete spTA_RecurringTransactions_Delete(int RecurringTransactionID, SqlGuid UserGUID, int PayTraceDeleteRequestID)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_RecurringTransactions_Delete);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@RecurringTransactionID", SqlDbType.Int).Value = RecurringTransactionID;
            command.Parameters.Add("@UserGUID", SqlDbType.UniqueIdentifier).Value = UserGUID;
            command.Parameters.Add("@PayTraceDeleteRequestID", SqlDbType.Int).Value = PayTraceDeleteRequestID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_RecurringTransactions_Delete(aps);
        }


        public static DRspTA_RecurringTransactions_GetByRecurringTransactionID spTA_RecurringTransactions_GetByRecurringTransactionID(int RecurringTransactionID)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_RecurringTransactions_GetByRecurringTransactionID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@RecurringTransactionID", SqlDbType.Int).Value = RecurringTransactionID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_RecurringTransactions_GetByRecurringTransactionID(aps);
        }


        public static DRspTA_RecurringTransactions_UpdateNext spTA_RecurringTransactions_UpdateNext(int RecurringTransactionID, string PayTraceRECURID, DateTime PayTraceNEXT, string PayTraceTOTALCOUNT, string PayTraceCURRENTCOUNT, string PayTraceREPEAT)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_RecurringTransactions_UpdateNext);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@RecurringTransactionID", SqlDbType.Int).Value = RecurringTransactionID;
            command.Parameters.Add("@PayTraceRECURID", SqlDbType.VarChar).Value = PayTraceRECURID;
            command.Parameters.Add("@PayTraceNEXT", SqlDbType.DateTime).Value = PayTraceNEXT;
            command.Parameters.Add("@PayTraceTOTALCOUNT", SqlDbType.VarChar).Value = PayTraceTOTALCOUNT;
            command.Parameters.Add("@PayTraceCURRENTCOUNT", SqlDbType.VarChar).Value = PayTraceCURRENTCOUNT;
            command.Parameters.Add("@PayTraceREPEAT", SqlDbType.VarChar).Value = PayTraceREPEAT;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_RecurringTransactions_UpdateNext(aps);
        }


        public static DRspTA_RecurringTransactions_UpdateRecurringID spTA_RecurringTransactions_UpdateRecurringID(int RecurringTransactionID, int PayTraceCreateRequestID, string PayTraceRECURID)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_RecurringTransactions_UpdateRecurringID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@RecurringTransactionID", SqlDbType.Int).Value = RecurringTransactionID;
            command.Parameters.Add("@PayTraceCreateRequestID", SqlDbType.Int).Value = PayTraceCreateRequestID;
            command.Parameters.Add("@PayTraceRECURID", SqlDbType.VarChar).Value = PayTraceRECURID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_RecurringTransactions_UpdateRecurringID(aps);
        }


        public static DRspTA_RecurringTransactions_UpdateAfterCustomerProfile spTA_RecurringTransactions_UpdateAfterCustomerProfile(int RecurringTransactionID, int PayTraceProfileRequestID, string CUSTID)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_RecurringTransactions_UpdateAfterCustomerProfile);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@RecurringTransactionID", SqlDbType.Int).Value = RecurringTransactionID;
            command.Parameters.Add("@PayTraceProfileRequestID", SqlDbType.Int).Value = PayTraceProfileRequestID;
            command.Parameters.Add("@CUSTID", SqlDbType.VarChar).Value = CUSTID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_RecurringTransactions_UpdateAfterCustomerProfile(aps);
        }


        public static DRspTA_RecurringTransactions_Insert spTA_RecurringTransactions_Insert(SqlGuid UserGUID, int SiteID, int EgivingsID, string BNAME, string CC, string EXPMNTH, string EXPYR, string AMOUNT, string CSC, string BADDRESS, string BZIP, DateTime START, string RecurringFrequencyID, string TOTALCOUNT, string TRANXTYPE, string CUSTRECEIPT, string Address2, string City, string State, string Email, string Phone)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_RecurringTransactions_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserGUID", SqlDbType.UniqueIdentifier).Value = UserGUID;
            command.Parameters.Add("@SiteID", SqlDbType.Int).Value = SiteID;
            command.Parameters.Add("@EgivingsID", SqlDbType.Int).Value = EgivingsID;
            command.Parameters.Add("@BNAME", SqlDbType.VarChar).Value = BNAME;
            command.Parameters.Add("@CC", SqlDbType.VarChar).Value = CCMarking(CC);
            command.Parameters.Add("@EXPMNTH", SqlDbType.VarChar).Value = EXPMNTH;
            command.Parameters.Add("@EXPYR", SqlDbType.VarChar).Value = EXPYR;
            command.Parameters.Add("@AMOUNT", SqlDbType.VarChar).Value = AMOUNT;
            command.Parameters.Add("@CSC", SqlDbType.VarChar).Value = CSC;
            command.Parameters.Add("@BADDRESS", SqlDbType.VarChar).Value = BADDRESS;
            command.Parameters.Add("@BZIP", SqlDbType.VarChar).Value = BZIP;
            command.Parameters.Add("@START", SqlDbType.DateTime).Value = START;
            command.Parameters.Add("@RecurringFrequencyID", SqlDbType.VarChar).Value = RecurringFrequencyID;
            command.Parameters.Add("@TOTALCOUNT", SqlDbType.VarChar).Value = TOTALCOUNT;
            command.Parameters.Add("@TRANXTYPE", SqlDbType.VarChar).Value = TRANXTYPE;
            command.Parameters.Add("@CUSTRECEIPT", SqlDbType.VarChar).Value = CUSTRECEIPT;
            command.Parameters.Add("@Address2", SqlDbType.VarChar).Value = Address2;
            command.Parameters.Add("@City", SqlDbType.VarChar).Value = City;
            command.Parameters.Add("@State", SqlDbType.VarChar).Value = State;
            command.Parameters.Add("@Email", SqlDbType.VarChar).Value = Email;
            command.Parameters.Add("@Phone", SqlDbType.VarChar).Value = Phone;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_RecurringTransactions_Insert(aps);
        }


        public static DRspTA_DirectTransactions_UpdateAfterPost spTA_DirectTransactions_UpdateAfterPost(int DirectTransactionID, string INVOICE, int PayTraceRequestID, string PayTraceTRANSACTIONID, string PayTraceAPPMSG)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_DirectTransactions_UpdateAfterPost);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@DirectTransactionID", SqlDbType.Int).Value = DirectTransactionID;
            command.Parameters.Add("@INVOICE", SqlDbType.VarChar).Value = INVOICE;
            command.Parameters.Add("@PayTraceRequestID", SqlDbType.Int).Value = PayTraceRequestID;
            command.Parameters.Add("@PayTraceTRANSACTIONID", SqlDbType.VarChar).Value = PayTraceTRANSACTIONID;
            command.Parameters.Add("@PayTraceAPPMSG", SqlDbType.VarChar).Value = PayTraceAPPMSG;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_DirectTransactions_UpdateAfterPost(aps);
        }


        public static DRspTA_DirectTransactions_Insert spTA_DirectTransactions_Insert(SqlGuid UserGUID, int SiteID, int EgivingsID, string BNAME, string CC, string EXPMNTH, string EXPYR, string AMOUNT, string CSC, string BADDRESS, string BZIP, string Address2, string City, string State, string Phone, string Email)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_DirectTransactions_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserGUID", SqlDbType.UniqueIdentifier).Value = UserGUID;
            command.Parameters.Add("@SiteID", SqlDbType.Int).Value = SiteID;
            command.Parameters.Add("@EgivingsID", SqlDbType.Int).Value = EgivingsID;
            command.Parameters.Add("@BNAME", SqlDbType.VarChar).Value = BNAME;
            command.Parameters.Add("@CC", SqlDbType.VarChar).Value = CCMarking(CC);
            command.Parameters.Add("@EXPMNTH", SqlDbType.VarChar).Value = EXPMNTH;
            command.Parameters.Add("@EXPYR", SqlDbType.VarChar).Value = EXPYR;
            command.Parameters.Add("@AMOUNT", SqlDbType.VarChar).Value = AMOUNT;
            command.Parameters.Add("@CSC", SqlDbType.VarChar).Value = CSC;
            command.Parameters.Add("@BADDRESS", SqlDbType.VarChar).Value = BADDRESS;
            command.Parameters.Add("@BZIP", SqlDbType.VarChar).Value = BZIP;
            command.Parameters.Add("@Address2", SqlDbType.VarChar).Value = Address2;
            command.Parameters.Add("@City", SqlDbType.VarChar).Value = City;
            command.Parameters.Add("@State", SqlDbType.VarChar).Value = State;
            command.Parameters.Add("@Email", SqlDbType.VarChar).Value = Email;
            command.Parameters.Add("@Phone", SqlDbType.VarChar).Value = Phone;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_DirectTransactions_Insert(aps);
        }


        public static DRspTA_CMS_User_GetByUserGUID spTA_CMS_User_GetByUserGUID(SqlGuid UserGUID)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_CMS_User_GetByUserGUID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserGUID", SqlDbType.UniqueIdentifier).Value = UserGUID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_CMS_User_GetByUserGUID(aps);
        }


        public static void TA_PayTraceCustomerExport_BulkInsert(PayTraceCustomerDataArray darr)
        {
            if (darr.data != null && darr.data.Length > 0)
            {
                SqlGuid ExportGroupGUID = Guid.NewGuid();
                for (int i = 0; i < darr.data.Length; i++)
                {
                    DateTime cwhen;
                    DateTime.TryParse(darr.data[i].WHEN, out cwhen);

                    DRspTA_PayTraceCustomerExport_Insert ins = spTA_PayTraceCustomerExport_Insert(
                        ExportGroupGUID,
                        darr.data[i].CUSTID,
                        darr.data[i].CUSTOMERID,
                        darr.data[i].CC,
                        darr.data[i].EXPMNTH,
                        darr.data[i].EXPYR,
                        darr.data[i].SNAME,
                        darr.data[i].SADDRESS,
                        darr.data[i].SADDRESS2,
                        darr.data[i].SCITY,
                        darr.data[i].SCOUNTY,
                        darr.data[i].SSTATE,
                        darr.data[i].SZIP,
                        darr.data[i].SCOUNTRY,
                        darr.data[i].BNAME,
                        darr.data[i].BADDRESS,
                        darr.data[i].BADDRESS2,
                        darr.data[i].BCITY,
                        darr.data[i].BSTATE,
                        darr.data[i].BZIP,
                        darr.data[i].BCOUNTRY,
                        darr.data[i].EMAIL,
                        darr.data[i].PHONE,
                        darr.data[i].FAX,
                        cwhen,
                        darr.data[i].USER,
                        darr.data[i].IP
                        );
                }
            }
        }

        public static DRspTA_PayTraceCustomerExport_Insert spTA_PayTraceCustomerExport_Insert(SqlGuid ExportGroupGUID, string CUSTID, string CUSTOMERID, string CC, string EXPMNTH, string EXPYR, string SNAME, string SADDRESS, string SADDRESS2, string SCITY, string SCOUNTY, string SSTATE, string SZIP, string SCOUNTRY, string BNAME, string BADDRESS, string BADDRESS2, string BCITY, string BSTATE, string BZIP, string BCOUNTRY, string EMAIL, string PHONE, string FAX, DateTime CWHEN, string CUSER, string CIP)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_PayTraceCustomerExport_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@ExportGroupGUID", SqlDbType.UniqueIdentifier).Value = ExportGroupGUID;
            command.Parameters.Add("@ExportDate", SqlDbType.DateTime).Value = DateTime.Now;
            command.Parameters.Add("@CUSTID", SqlDbType.VarChar).Value = CUSTID;
            command.Parameters.Add("@CUSTOMERID", SqlDbType.VarChar).Value = CUSTOMERID;
            command.Parameters.Add("@CC", SqlDbType.VarChar).Value = CCMarking(CC);
            command.Parameters.Add("@EXPMNTH", SqlDbType.VarChar).Value = EXPMNTH;
            command.Parameters.Add("@EXPYR", SqlDbType.VarChar).Value = EXPYR;
            command.Parameters.Add("@SNAME", SqlDbType.VarChar).Value = SNAME;
            command.Parameters.Add("@SADDRESS", SqlDbType.VarChar).Value = SADDRESS;
            command.Parameters.Add("@SADDRESS2", SqlDbType.VarChar).Value = SADDRESS2;
            command.Parameters.Add("@SCITY", SqlDbType.VarChar).Value = SCITY;
            command.Parameters.Add("@SCOUNTY", SqlDbType.VarChar).Value = SCOUNTY;
            command.Parameters.Add("@SSTATE", SqlDbType.VarChar).Value = SSTATE;
            command.Parameters.Add("@SZIP", SqlDbType.VarChar).Value = SZIP;
            command.Parameters.Add("@SCOUNTRY", SqlDbType.VarChar).Value = SCOUNTRY;
            command.Parameters.Add("@BNAME", SqlDbType.VarChar).Value = BNAME;
            command.Parameters.Add("@BADDRESS", SqlDbType.VarChar).Value = BADDRESS;
            command.Parameters.Add("@BADDRESS2", SqlDbType.VarChar).Value = BADDRESS2;
            command.Parameters.Add("@BCITY", SqlDbType.VarChar).Value = BCITY;
            command.Parameters.Add("@BSTATE", SqlDbType.VarChar).Value = BSTATE;
            command.Parameters.Add("@BZIP", SqlDbType.VarChar).Value = BZIP;
            command.Parameters.Add("@BCOUNTRY", SqlDbType.VarChar).Value = BCOUNTRY;
            command.Parameters.Add("@EMAIL", SqlDbType.VarChar).Value = EMAIL;
            command.Parameters.Add("@PHONE", SqlDbType.VarChar).Value = PHONE;
            command.Parameters.Add("@FAX", SqlDbType.VarChar).Value = FAX;
            command.Parameters.Add("@CWHEN", SqlDbType.DateTime).Value = CWHEN;
            command.Parameters.Add("@CUSER", SqlDbType.VarChar).Value = CUSER;
            command.Parameters.Add("@CIP", SqlDbType.VarChar).Value = CIP;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_PayTraceCustomerExport_Insert(aps);
        }



        public static DRspTA_PayTraceRequestResponse_InsertRequest spTA_PayTraceRequestResponse_InsertRequest(int RequestTypeID, string RequestString)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_PayTraceRequestResponse_InsertRequest);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@RequestTypeID", SqlDbType.Int).Value = RequestTypeID;
            command.Parameters.Add("@RequestString", SqlDbType.VarChar).Value = REQUESTMarking(RequestString);
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_PayTraceRequestResponse_InsertRequest(aps);
        }

        public static DRspTA_PayTraceRequestResponse_UpdateResponse spTA_PayTraceRequestResponse_UpdateResponse(int PayTraceRequestID, string ResponseString)
        {
            DataAccess DB = new DataAccess(databaseServer.EGivings);
            SqlCommand command = new SqlCommand(SV.SP.EGivings.spTA_PayTraceRequestResponse_UpdateResponse);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@PayTraceRequestID", SqlDbType.Int).Value = PayTraceRequestID;
            command.Parameters.Add("@ResponseString", SqlDbType.VarChar).Value = ResponseString;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTA_PayTraceRequestResponse_UpdateResponse(aps);
        }



    }

}
