using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Net;
using System.Text;

namespace PaymentProcessor.Web.Applications
{
    /// <summary>
    /// 1. TA_DirectTransactions is single transaction using credit card. Not using Customer ID, and do not use it.
    /// 2. TA_RecurringTransactions is recurring transactions. Right now, there is no way to pull history/single transactions on recurring. CUSTID has to change on every recurr id.
    /// 3. TA_CheckTransactions is single transaction using Check. Not using Customer ID and do not use it
    ///     
    /// </summary>
    public class PayTraceServices
    {
        public string PayTraceURL = "";
        public const string DirectTransactionsInvoiceFormat = "DT{0}";          // format with DirectTransactionID from database
        public const string DirectTransactionsEventsInvoiceFormat = "DE{0}";          // format with DirectTransactionID from database
        public const string RecurringTransactionsCustIDFormat = "ID{0}_RC{1}";  // format with UserGUID, RecurringTransactionID 
        public const string CheckTransactionsInvoiceFormat = "CH{0}";           // format with CheckTransactionID from database

        public struct WebClientMETHODS
        {
            public const string POST = "POST";
            public const string GET = "GET";

        }

        public struct METHODS
        {
            public const string CreateCustomer = "CreateCustomer";      // to create customer profile, only for recurring payment
            public const string ExportCustomers = "ExportCustomers";    // to export customer profile    
            public const string DeleteCustomer = "DeleteCustomer";      // to delete customer profile
            public const string UpdateCustomer = "UpdateCustomer";      // to update customer profile, not being used
            public const string ProcessTranx = "ProcessTranx";          // for direct payment using credit card 
            public const string CreateRecur = "CreateRecur";            // to create recurring payment
            public const string ExportRecur = "ExportRecur";            // to pull information on a recurring payment 
            public const string ExportTranx = "ExportTranx";            // to export transactions from StartDate - EndDate
            public const string DeleteRecur = "DeleteRecur";            // to delete recurring payment. we delete in PayTrace, but only mark IsDeleted in out database
            public const string UpdateRecur = "UpdateRecur";            // to update recurring
            public const string ProcessCheck = "ProcessCheck";
        }

        public struct FREQUENCY
        {
            public const string Annually = "1";
            public const string Quarterly = "2";
            public const string Monthly = "3";
            public const string BiWeekly = "4";
            public const string Weekly = "5";
            public const string Daily = "6";
            public const string FirstAnd15th = "7";
            public const string SemiAnnually = "8";
            public const string BiMonthly = "9";
            public const string Trimesterly = "A";
        }

        public struct TERMS
        {
            public const string Y = "Y";
        }

        public struct CUSTRECEIPT
        {
            public const string Y = "Y";
            public const string N = "N";
        }

        public struct TRANXTYPE
        {
            public const string Sale = "Sale";
            public const string Authorization = "Authorization";
            public const string Refund = "Refund";
            public const string Void = "Void";
            public const string Force = "Force";
            public const string Capture = "Capture";
            public const string StrFWD = "Str/FWD";
        }

        public struct CHECKTYPE
        {
            public const string Sale = "Sale";
        }

        private string _username = "";  // paytrace username
        private string _password = "";  // paytrace password
        private int _siteid = 0; // agile siteid
        private bool _istestmode = false;

        private void Init()
        {
            string AS_PayTraceURL = Utils.getAppSettings(SV.AppSettings.PayTraceURL + _siteid.ToString()).ToString();
            if ((AS_PayTraceURL.Length > 0) && (!_istestmode))
            {
                PayTraceURL = AS_PayTraceURL;
            }
            else
            {
                PayTraceURL = "https://paytrace.com/api/default.pay";

            }

            string AS_PayTraceUsername = Utils.getAppSettings(SV.AppSettings.PayTraceUsername + _siteid.ToString()).ToString();
            if ((AS_PayTraceUsername.Length > 0) && (!_istestmode))
            {
                _username = AS_PayTraceUsername;
            }
            else
            {
                _username = "demo123";
            }
            string AS_PayTracePassword = Utils.getAppSettings(SV.AppSettings.PayTracePassword + _siteid.ToString()).ToString();
            if ((AS_PayTracePassword.Length > 0) && (!_istestmode))
            {
                _password = AS_PayTracePassword;
            }
            else
            {
                _password = "demo123";
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PayTraceServices(int SiteID)
        {
            _siteid = SiteID;
            Init();
        }

        public bool IsTestMode
        {
            set
            {
                _istestmode = value;
                Init(); // reset password
            }
            get
            {
                return _istestmode;
            }
        }

        public PayTraceResponseData ProcessCheckTransaction(SqlGuid UserGUID, int EgivingsID, string BNAME, string DDA, string TR, string AMOUNT, string BADDRESS, string BZIP, string Address2, string City, string State, string Phone, string Email)
        {
            PayTraceResponseData rdata = new PayTraceResponseData("");
            PayTraceResponseData rsubmit = new PayTraceResponseData("");
            DRspTA_CMS_User_GetByUserGUID user = SQLDataPayTrace.spTA_CMS_User_GetByUserGUID(UserGUID);
            if (user.Count > 0 && user.UserID(0) > 0)
            {

                DRspTA_CheckTransactions_Insert tran = SQLDataPayTrace.spTA_CheckTransactions_Insert(UserGUID, _siteid, EgivingsID, BNAME, DDA, TR, AMOUNT, BADDRESS, BZIP, Address2, City, State, Phone, Email);
                if (tran.Count > 0 && tran.CheckTransactionID(0) > 0)
                {
                    string INVOICE = string.Format(CheckTransactionsInvoiceFormat, tran.CheckTransactionID(0));
                    rsubmit = ProcessCheck(INVOICE, BNAME, DDA, TR, AMOUNT, BADDRESS, BZIP);
                    DRspTA_CheckTransactions_UpdateAfterPost checkData = SQLDataPayTrace.spTA_CheckTransactions_UpdateAfterPost(tran.CheckTransactionID(0), INVOICE, rsubmit.PayTraceRequestID, rsubmit.CHECKIDENTIFIER, rsubmit.RESPONSE);
                    if (checkData.Count > 0)
                    {
                        rsubmit.CheckTransactionID = tran.CheckTransactionID(0);
                    }

                }
                else
                {
                    rdata.ERROR = "ERROR~User Inserting data, could not have INVOICEID.";
                }
            }
            else
            {
                rdata.ERROR = "ERROR~User Not found.";
            }

            if (rdata.ERROR != null && rdata.ERROR.Length > 0)
            {
                DRspTA_PayTraceRequestResponse_InsertRequest insertError = SQLDataPayTrace.spTA_PayTraceRequestResponse_InsertRequest(0, "ProcessCheckTransaction()");
                if (insertError.Count > 0)
                {
                    DRspTA_PayTraceRequestResponse_UpdateResponse updateError = SQLDataPayTrace.spTA_PayTraceRequestResponse_UpdateResponse(insertError.PayTraceRequestID(0), rdata.ERROR);
                }
            }
            else
            {
                rdata = rsubmit;
            }

            return rdata;
        }

        /// <summary>
        /// You can export customer, and save to TempTable (TA_PayTraceCustomerExport) using SQLDataPayTraceTA_PayTraceCustomerExport_BulkInsert
        /// </summary>
        /// <returns></returns>
        public PayTraceCustomerDataArray ExportCustomer()
        {
            //‘format the request string to export all of the customer profiles for the demo account
            //strRequest = “UN~demo123|PSWD~demo123|TERMS~Y|METHOD~ExportCustomers|”
            PayTraceRequestData req = new PayTraceRequestData()
            {
                UN = _username,
                PSWD = _password,
                METHOD = METHODS.ExportCustomers,
                TERMS = TERMS.Y,
            };

            string strRequest = req.BuildStringFor("UN,PSWD,TERMS,METHOD");

            int PayTraceRequestID = 0;
            string rst = UploadData(requestTypeIDs.ExportCustomers, strRequest, WebClientMETHODS.POST, out PayTraceRequestID);
            PayTraceCustomerDataArray result = new PayTraceCustomerDataArray(rst);

            return result;
        }

        public PayTraceResponseData ProcessDirectTransactionForEvents(SqlGuid UserGUID, int Egiving_EventsID, string BNAME, string CC, string EXPMNTH, string EXPYR, string AMOUNT, int TotalTicketPurchased, string CSC, string BADDRESS, string BSTATE, string BZIP, string Address2, string City, string State, string Phone, string Email)
        {
            // check if UserGUID is a customer
            // DO NOT create CustomerID in ThePayTrace
            // insert data to TA_DirectTransactions
            // Generate INVOICE
            // post to PayTrace
            // update invoice table, PayTraceTRANSACTIONID, and PayTraceAPPMSG
            PayTraceResponseData rdata = new PayTraceResponseData("");
            PayTraceResponseData rsubmit = new PayTraceResponseData("");

            DRspTA_CMS_User_GetByUserGUID user = SQLDataPayTrace.spTA_CMS_User_GetByUserGUID(UserGUID);
            if (user.Count > 0 && user.UserID(0) > 0)
            {
                DRspTA_DirectTransactions_Events_Insert tran = SQLDataPayTrace.spTA_DirectTransactions_Events_Insert(UserGUID, _siteid, Egiving_EventsID, BNAME, CC, EXPMNTH, EXPYR, AMOUNT, CSC, BADDRESS, BZIP, Address2, City, State, Phone, Email);
                if (tran.Count > 0 && tran.DirectTransactions_EventsID(0) > 0)
                {
                    string INVOICE = string.Format(DirectTransactionsEventsInvoiceFormat, tran.DirectTransactions_EventsID(0));
                    rsubmit = ProcessTransaction(INVOICE, CC, EXPMNTH, EXPYR, AMOUNT, CSC, BADDRESS, BZIP);
                    // only if no error, and no voided
                    if (rsubmit.IsSuccess && (rsubmit.APPCODE.Length > 0) && rsubmit.APPMSG.IndexOf("VOIDED", StringComparison.CurrentCultureIgnoreCase) == -1) // if there is approval code
                    {
                        DRspTA_DirectTransactions_Events_UpdateAfterPost ok = SQLDataPayTrace.spTA_DirectTransactions_Events_UpdateAfterPost(tran.DirectTransactions_EventsID(0), INVOICE, rsubmit.PayTraceRequestID, rsubmit.TRANSACTIONID, rsubmit.APPMSG);
                        DRspTA_custom_egiving_Events_UpdateNumber_of_Tickets updateTotal = SQLDataPayTrace.spTA_custom_egiving_Events_UpdateNumber_of_Tickets(_siteid, Egiving_EventsID, TotalTicketPurchased);
                    }
                    else
                    {
                        // we do not update TransactionID, and AppMsg
                        DRspTA_DirectTransactions_Events_UpdateAfterPost fail = SQLDataPayTrace.spTA_DirectTransactions_Events_UpdateAfterPost(tran.DirectTransactions_EventsID(0), INVOICE, rsubmit.PayTraceRequestID, "", "");
                    }
                    rsubmit.DirectTransactions_EventsID = tran.DirectTransactions_EventsID(0);
                }
                else
                {
                    rdata.ERROR = "ERROR~User Inserting data, could not have INVOICEID.";
                }
            }
            else
            {
                rdata.ERROR = "ERROR~User Not found.";
            }

            if (rdata.ERROR != null && rdata.ERROR.Length > 0)
            {
                DRspTA_PayTraceRequestResponse_InsertRequest insertError = SQLDataPayTrace.spTA_PayTraceRequestResponse_InsertRequest(0, "ProcessDirectTransaction()");
                if (insertError.Count > 0)
                {
                    DRspTA_PayTraceRequestResponse_UpdateResponse updateError = SQLDataPayTrace.spTA_PayTraceRequestResponse_UpdateResponse(insertError.PayTraceRequestID(0), rdata.ERROR);
                }
            }
            else
            {
                rdata = rsubmit;
            }

            return rdata;

        }

        /// <summary>
        /// For direct payment using Credit Card. No need to create Customer Profile
        /// </summary>
        /// <param name="UserGUID"></param>
        /// <param name="BNAME"></param>
        /// <param name="CC"></param>
        /// <param name="EXPMNTH"></param>
        /// <param name="EXPYR"></param>
        /// <param name="AMOUNT"></param>
        /// <param name="CSC"></param>
        /// <param name="BADDRESS"></param>
        /// <param name="BCITY"></param>
        /// <param name="BSTATE"></param>
        /// <param name="BZIP"></param>
        /// <returns></returns>
        public PayTraceResponseData ProcessDirectTransaction(SqlGuid UserGUID, int EgivingsID, string BNAME, string CC, string EXPMNTH, string EXPYR, string AMOUNT, string CSC, string BADDRESS, string BSTATE, string BZIP, string Address2, string City, string State, string Phone, string Email)
        {
            // check if UserGUID is a customer
            // DO NOT create CustomerID in ThePayTrace
            // insert data to TA_DirectTransactions
            // Generate INVOICE
            // post to PayTrace
            // update invoice table, PayTraceTRANSACTIONID, and PayTraceAPPMSG
            PayTraceResponseData rdata = new PayTraceResponseData("");
            PayTraceResponseData rsubmit = new PayTraceResponseData("");

            DRspTA_CMS_User_GetByUserGUID user = SQLDataPayTrace.spTA_CMS_User_GetByUserGUID(UserGUID);
            if (user.Count > 0 && user.UserID(0) > 0)
            {
                DRspTA_DirectTransactions_Insert tran = SQLDataPayTrace.spTA_DirectTransactions_Insert(UserGUID, _siteid, EgivingsID, BNAME, CC, EXPMNTH, EXPYR, AMOUNT, CSC, BADDRESS, BZIP, Address2, City, State, Phone, Email);
                if (tran.Count > 0 && tran.DirectTransactionID(0) > 0)
                {
                    string INVOICE = string.Format(DirectTransactionsInvoiceFormat, tran.DirectTransactionID(0));
                    rsubmit = ProcessTransaction(INVOICE, CC, EXPMNTH, EXPYR, AMOUNT, CSC, BADDRESS, BZIP);
                    // only if no error, and no voided
                    if (rsubmit.IsSuccess && (rsubmit.APPCODE.Length > 0) && rsubmit.APPMSG.IndexOf("VOIDED", StringComparison.CurrentCultureIgnoreCase) == -1) // if there is approval code
                    {
                        DRspTA_DirectTransactions_UpdateAfterPost ok = SQLDataPayTrace.spTA_DirectTransactions_UpdateAfterPost(tran.DirectTransactionID(0), INVOICE, rsubmit.PayTraceRequestID, rsubmit.TRANSACTIONID, rsubmit.APPMSG);
                    }
                    else
                    {
                        // we do not update TransactionID, and AppMsg
                        DRspTA_DirectTransactions_UpdateAfterPost fail = SQLDataPayTrace.spTA_DirectTransactions_UpdateAfterPost(tran.DirectTransactionID(0), INVOICE, rsubmit.PayTraceRequestID, "", "");
                    }
                    rsubmit.DirectTransactionID = tran.DirectTransactionID(0);
                }
                else
                {
                    rdata.ERROR = "ERROR~User Inserting data, could not have INVOICEID.";
                }
            }
            else
            {
                rdata.ERROR = "ERROR~User Not found.";
            }

            if (rdata.ERROR != null && rdata.ERROR.Length > 0)
            {
                DRspTA_PayTraceRequestResponse_InsertRequest insertError = SQLDataPayTrace.spTA_PayTraceRequestResponse_InsertRequest(0, "ProcessDirectTransaction()");
                if (insertError.Count > 0)
                {
                    DRspTA_PayTraceRequestResponse_UpdateResponse updateError = SQLDataPayTrace.spTA_PayTraceRequestResponse_UpdateResponse(insertError.PayTraceRequestID(0), rdata.ERROR);
                }
            }
            else
            {
                rdata = rsubmit;
            }

            return rdata;

        }

        public PayTraceRecurringPaymentDataArray ExportRecurringTransactions(string CUSTID)
        {
            //‘format the request string to export a customer’s recurring payment
            //strRequest = “un~demo123|pswd~demo123|method~ExportCustomerRecur|terms~Y|"
            //strRequest = strRequest & "custid~testcustomer|"

            PayTraceRequestData req = new PayTraceRequestData()
            {
                UN = _username,
                PSWD = _password,
                METHOD = METHODS.ExportRecur,
                TERMS = TERMS.Y,
                CUSTID = CUSTID
            };

            string strRequest = req.BuildStringFor("UN,PSWD,TERMS,METHOD,CUSTID");

            int PayTraceRequestID = 0;
            PayTraceRecurringPaymentDataArray rdata = new PayTraceRecurringPaymentDataArray(UploadData(requestTypeIDs.CreateCustomer, strRequest, WebClientMETHODS.POST, out PayTraceRequestID)); // create using POST
            rdata.PayTraceRequestID = PayTraceRequestID;

            return rdata;

        }

        public PayTraceResponseData DeleteRecurringByUser(SqlGuid UserGUID, int RecurringTransactionID)
        {
            // check if RecurringTransactionID belong to User, and has not been deleted
            PayTraceResponseData rdata = new PayTraceResponseData("");
            rdata.ERROR = "";
            DRspTA_RecurringTransactions_GetByRecurringTransactionID check = SQLDataPayTrace.spTA_RecurringTransactions_GetByRecurringTransactionID(RecurringTransactionID);
            if (check.Count > 0)
            {
                int PayTraceDeleteRequestID = 0;
                // check if same UserGUID
                if (check.UserGUID(0).Equals(UserGUID))
                {
                    if (!check.IsDeleted(0))
                    {
                        // delete customer profile first
                        if (check.PayTraceRECURID(0).Length > 0)
                        {
                            PayTraceResponseData p1 = DeleteRecurringTransaction(check.PayTraceRECURID(0));
                            if (p1.IsError)
                            {
                                rdata.ERROR += p1.ERROR;
                            }
                            PayTraceDeleteRequestID = p1.PayTraceRequestID;
                        }

                        // delete profile
                        if (check.CUSTID(0).Length > 0)
                        {
                            PayTraceResponseData p2 = DeleteCustomer(check.CUSTID(0));
                            if (p2.IsError)
                            {
                                rdata.ERROR += p2.ERROR;
                            }
                        }

                        // now mark delete
                        DRspTA_RecurringTransactions_Delete delete = SQLDataPayTrace.spTA_RecurringTransactions_Delete(check.RecurringTransactionID(0), UserGUID, PayTraceDeleteRequestID);

                    }
                }
            }

            if (rdata.ERROR != null && rdata.ERROR.Length > 0)
            {
                DRspTA_PayTraceRequestResponse_InsertRequest insertError = SQLDataPayTrace.spTA_PayTraceRequestResponse_InsertRequest(0, "DeleteRecurringByUser()");
                if (insertError.Count > 0)
                {
                    DRspTA_PayTraceRequestResponse_UpdateResponse updateError = SQLDataPayTrace.spTA_PayTraceRequestResponse_UpdateResponse(insertError.PayTraceRequestID(0), rdata.ERROR);
                }
            }

            return rdata;
        }

        private string GetRecurringFrequencyID(string Frequency)
        {
            string RecurringFrequencyID = "";
            switch (Frequency.ToLower())
            {
                case "annually":
                case "1":
                    RecurringFrequencyID = FREQUENCY.Annually;
                    break;
                case "quarterly":
                case "2":
                    RecurringFrequencyID = FREQUENCY.Quarterly;
                    break;
                case "monthly":
                case "3":
                    RecurringFrequencyID = FREQUENCY.Monthly;
                    break;
                case "biweekly":
                case "4":
                    RecurringFrequencyID = FREQUENCY.BiWeekly;
                    break;
                case "weekly":
                case "5":
                    RecurringFrequencyID = FREQUENCY.Weekly;
                    break;
                case "daily":
                case "6":
                    RecurringFrequencyID = FREQUENCY.Daily;
                    break;
                case "firstand15th":
                case "7":
                    RecurringFrequencyID = FREQUENCY.FirstAnd15th;
                    break;
                case "semiannually":
                case "8":
                    RecurringFrequencyID = FREQUENCY.SemiAnnually;
                    break;
                case "bimonthly":
                case "9":
                    RecurringFrequencyID = FREQUENCY.BiMonthly;
                    break;
                case "trimesterly":
                case "A":
                    RecurringFrequencyID = FREQUENCY.Trimesterly;
                    break;
                default:

                    break;
            }
            return RecurringFrequencyID;
        }

        public PayTraceResponseData CreateRecurringByUser(SqlGuid UserGUID, int EgivingsID, DateTime START, string TOTALCOUNT, string Frequency, string BNAME, string CC, string EXPMNTH, string EXPYR, string AMOUNT, string CSC, string BADDRESS, string BCITY, string BSTATE, string BZIP, string Address2, string Phone, string Email)
        {
            string RecurringFrequencyID = GetRecurringFrequencyID(Frequency);

            PayTraceResponseData rdata = new PayTraceResponseData("");
            PayTraceResponseData rsubmit = new PayTraceResponseData("");
            DRspTA_CMS_User_GetByUserGUID user = SQLDataPayTrace.spTA_CMS_User_GetByUserGUID(UserGUID);
            if (user.Count > 0 && user.UserID(0) > 0)
            {
                DRspTA_RecurringTransactions_Insert insert = SQLDataPayTrace.spTA_RecurringTransactions_Insert(UserGUID, _siteid, EgivingsID, BNAME, CC, EXPMNTH, EXPYR, AMOUNT, CSC, BADDRESS, BZIP, START, RecurringFrequencyID, TOTALCOUNT, TRANXTYPE.Sale, CUSTRECEIPT.N, Address2, BCITY, BSTATE, Email, Phone);
                if (insert.Count > 0 && insert.RecurringTransactionID(0) > 0)
                {
                    string CUSTID = string.Format(RecurringTransactionsCustIDFormat, UserGUID, insert.RecurringTransactionID(0));
                    PayTraceResponseData p1 = CreateCustomer(CUSTID, BNAME, BADDRESS, BCITY, BSTATE, BZIP, "", "", "", "", "", CC, EXPMNTH, EXPYR, user.Email(0));
                    if (p1.IsSuccess)
                    {
                        DRspTA_RecurringTransactions_UpdateAfterCustomerProfile cust = SQLDataPayTrace.spTA_RecurringTransactions_UpdateAfterCustomerProfile(insert.RecurringTransactionID(0), p1.PayTraceRequestID, CUSTID);

                        rsubmit = CreateRecurringTransaction(CUSTID, START.ToShortDateString(), AMOUNT, TOTALCOUNT, RecurringFrequencyID);

                        if (rsubmit.IsSuccess && rsubmit.RECURID != null && rsubmit.RECURID.Length > 0)
                        {
                            DRspTA_RecurringTransactions_UpdateRecurringID updateRecurID = SQLDataPayTrace.spTA_RecurringTransactions_UpdateRecurringID(insert.RecurringTransactionID(0), rsubmit.PayTraceRequestID, rsubmit.RECURID);
                            if (updateRecurID.Count > 0 && updateRecurID.RowUpdated(0) > 0)
                            {
                                // now update NEXT Payment, but it is not necessary
                                RecurringTransactions_UpdateNextPaymentDate(insert.RecurringTransactionID(0), CUSTID);
                            }
                            else
                            {
                                rdata.ERROR = "ERROR~Error Updating Recurring ID.";
                            }
                        }
                        rsubmit.RecurringTransactionID = insert.RecurringTransactionID(0);
                    }
                    else
                    {
                        rdata = p1; // transfer the error out
                    }
                }
                else
                {
                    rdata.ERROR = "ERROR~Error adding transaction in the database.";
                }
            }
            else
            {
                rdata.ERROR = "ERROR~Error UserGUID not found.";
            }

            if (rdata.ERROR != null && rdata.ERROR.Length > 0)
            {
                DRspTA_PayTraceRequestResponse_InsertRequest insertError = SQLDataPayTrace.spTA_PayTraceRequestResponse_InsertRequest(0, "CreateRecurringByUser()");
                if (insertError.Count > 0)
                {
                    DRspTA_PayTraceRequestResponse_UpdateResponse updateError = SQLDataPayTrace.spTA_PayTraceRequestResponse_UpdateResponse(insertError.PayTraceRequestID(0), rdata.ERROR);
                }
            }
            else
            {
                rdata = rsubmit;
            }

            return rdata;
        }

        public PayTraceResponseData UpdateRecurringByUser(SqlGuid UserGUID, int RecurringTransactionID, string TOTALCOUNT, string Frequency, DateTime NEXT, string BNAME, string CC, string EXPMNTH, string EXPYR, string AMOUNT, string CSC, string BADDRESS, string BCITY, string BSTATE, string BZIP, string Address2, string City, string Phone, string Email)
        {
            string RecurringFrequencyID = GetRecurringFrequencyID(Frequency);

            PayTraceResponseData rdata = new PayTraceResponseData("");
            DRspTA_RecurringTransactions_GetForThanks recTran = SQLDataPayTrace.spTA_RecurringTransactions_GetForThanks(UserGUID, _siteid, RecurringTransactionID);
            if (recTran.Count > 0)
            {
                string CUSTID = recTran.CUSTID(0);
                PayTraceResponseData p1 = UpdateCustomer(CUSTID, BNAME, BADDRESS, BCITY, BSTATE, BZIP, BNAME, "", "", "", "", CC, EXPMNTH, EXPYR, Email);
                if (p1.IsSuccess)
                {
                    DRspTA_RecurringTransactions_UpdateCustomerInfo updateDB1 = SQLDataPayTrace.spTA_RecurringTransactions_UpdateCustomerInfo(RecurringTransactionID, BNAME, CC, EXPMNTH, EXPYR, CSC, BADDRESS, BZIP, Address2, City, BSTATE, Email, Phone);

                    // now update the recurring
                    PayTraceResponseData p2 = UpdateRecurringTransaction(CUSTID, recTran.PayTraceRECURID(0), NEXT.ToShortDateString(), AMOUNT, TOTALCOUNT, RecurringFrequencyID);
                    if (p2.IsSuccess)
                    {
                        DRspTA_RecurringTransactions_UpdateRecurringInfo updateDB2 = SQLDataPayTrace.spTA_RecurringTransactions_UpdateRecurringInfo(RecurringTransactionID, NEXT, RecurringFrequencyID, TOTALCOUNT, AMOUNT);
                        // now update next
                        RecurringTransactions_UpdateNextPaymentDate(RecurringTransactionID, CUSTID);
                        rdata = p2;
                    }
                    else
                    {
                        rdata.ERROR = p2.ERROR;
                    }
                }
                else
                {
                    rdata.ERROR = p1.ERROR;
                }
            }

            return rdata;
        }

        public void UpdateRecurringNextPaymentByUserGUID(SqlGuid UserGUID, int AfterDays)
        {
            DRspTA_RecurringTransactions_GetByUserGUID recTrans = SQLDataPayTrace.spTA_RecurringTransactions_GetByUserGUID(UserGUID, _siteid);
            for (int i = 0; i < recTrans.Count; i++)
            {
                if ((recTrans.LastCheckDate(i).AddDays(AfterDays) < DateTime.Now) && (recTrans.CUSTID(i).Length > 0))
                {
                    RecurringTransactions_UpdateNextPaymentDate(recTrans.RecurringTransactionID(i), recTrans.CUSTID(i));
                }
            }
        }

        private void RecurringTransactions_UpdateNextPaymentDate(int RecurringTransactionID, string CUSTID)
        {
            PayTraceRecurringPaymentDataArray p2 = ExportRecurringTransactions(CUSTID); // this should be only one recurring because we create unique profile on each recurring
            if (p2.data.Length > 0)
            {
                DateTime pnext;
                DateTime.TryParse(p2.data[0].NEXT, out pnext);
                DRspTA_RecurringTransactions_UpdateNext updateNext = SQLDataPayTrace.spTA_RecurringTransactions_UpdateNext(RecurringTransactionID, p2.data[0].RECURID, pnext, p2.data[0].TOTALCOUNT, p2.data[0].CURRENTCOUNT, p2.data[0].REPEAT);
            }
        }

        #region PrivateCallsToPayTrace

        /// <summary>
        /// Do not call this directly/public. Make sure insert entry to the database first 
        /// </summary>
        /// <param name="INVOICE"></param>
        /// <param name="BNAME"></param>
        /// <param name="DDA"></param>
        /// <param name="TR"></param>
        /// <param name="AMOUNT"></param>
        /// <param name="BADDRESS"></param>
        /// <param name="BZIP"></param>
        /// <returns></returns>
        private PayTraceResponseData ProcessCheck(string INVOICE, string BNAME, string DDA, string TR, string AMOUNT, string BADDRESS, string BZIP)
        {
            //‘format the request string to process a sale for $1.00
            //strRequest = “UN~demo123|PSWD~demo123|TERMS~Y|METHOD~ProcessCheck|”
            //strRequest = strRequest & “CHECKTYPE~Sale|DDA~123456|TR~325070760|”
            //strRequest = strRequest & “AMOUNT~1.00| BADDRESS~1234|BZIP~83852|INVOICE~8888|”

            PayTraceRequestData req = new PayTraceRequestData()
            {
                UN = _username,
                PSWD = _password,
                TERMS = TERMS.Y,
                METHOD = METHODS.ProcessCheck,
                CHECKTYPE = CHECKTYPE.Sale,
                BNAME = BNAME,
                DDA = DDA,
                TR = TR,
                AMOUNT = AMOUNT,
                BADDRESS = BADDRESS,
                BZIP = BZIP,
                INVOICE = INVOICE
            };

            string strRequest = req.BuildStringFor("UN,PSWD,TERMS,METHOD,CHECKTYPE,BNAME,DDA,TR,AMOUNT,BADDRESS,BZIP,INVOICE");

            int PayTraceRequestID = 0;
            PayTraceResponseData rdata = new PayTraceResponseData(UploadData(requestTypeIDs.DeleteCustomer, strRequest, WebClientMETHODS.POST, out PayTraceRequestID));
            rdata.PayTraceRequestID = PayTraceRequestID;
            return rdata;
        }


        /// <summary>
        /// Do not call this directly from outside
        /// </summary>
        /// <param name="SDATE"></param>
        /// <param name="EDATE"></param>
        /// <returns></returns>
        private PayTraceResponseData ExportTransactions(string SDATE, string EDATE)
        {
            //‘format the request string to export all of the transactions for the demo account processed in the May of 2011 
            //strRequest = “UN~demo123|PSWD~demo123|TERMS~Y|METHOD~ExportTranx|”
            //strRequest = strRequest & “SDATE~05/01/2011|EDATE~05/31/2011|”

            PayTraceRequestData req = new PayTraceRequestData()
            {
                UN = _username,
                PSWD = _password,
                METHOD = METHODS.ExportTranx,
                TERMS = TERMS.Y,
                SDATE = SDATE,
                EDATE = EDATE
            };

            string strRequest = req.BuildStringFor("UN,PSWD,TERMS,METHOD,SDATE,EDATE");

            int PayTraceRequestID = 0;
            PayTraceResponseData rdata = new PayTraceResponseData(UploadData(requestTypeIDs.CreateCustomer, strRequest, WebClientMETHODS.POST, out PayTraceRequestID)); // create using POST
            rdata.PayTraceRequestID = PayTraceRequestID;

            return rdata;

        }

        /// <summary>
        /// Not being used now. But do not call this directly. Only for Recurring Payment
        /// </summary>
        /// <param name="CUSTID"></param>
        /// <param name="BNAME"></param>
        /// <param name="BADDRESS"></param>
        /// <param name="BCITY"></param>
        /// <param name="BSTATE"></param>
        /// <param name="BZIP"></param>
        /// <param name="SNAME"></param>
        /// <param name="SADDRESS"></param>
        /// <param name="SCITY"></param>
        /// <param name="SSTATE"></param>
        /// <param name="SZIP"></param>
        /// <param name="CC"></param>
        /// <param name="EXPMNTH"></param>
        /// <param name="EXPYR"></param>
        /// <param name="EMAIL"></param>
        /// <returns></returns>
        private PayTraceResponseData UpdateCustomer(
            string CUSTID,
            string BNAME,
            string BADDRESS,
            string BCITY,
            string BSTATE,
            string BZIP,
            string SNAME,
            string SADDRESS,
            string SCITY,
            string SSTATE,
            string SZIP,
            string CC,
            string EXPMNTH,
            string EXPYR,
            string EMAIL
            )
        {
            //‘format the request string to update customer ID johndoe
            //strRequest = “UN~demo123|PSWD~demo123|TERMS~Y|METHOD~UpdateCustomer|”
            //strRequest = strRequest & “BNAME~John Doe|BADDRESS~1234 Main|BCITY~Spokane|BSTATE~WA|”
            //strRequest = strRequest & “BZIP~99201|SNAME~John Doe|SADDRESS~5678 First|SCITY~Portland|”
            //strRequest = strRequest & “SSTATE~OR|SZIP~97201|CUSTID~johndoe|CC~4012881888818888|”
            //strRequest = strRequest & “EXPMNTH~12|EXPYR~12|EMAIL~support@paytrace.com|”

            PayTraceRequestData req = new PayTraceRequestData()
            {
                UN = _username,
                PSWD = _password,
                METHOD = METHODS.UpdateCustomer,
                TERMS = TERMS.Y,
                BNAME = BNAME,
                BADDRESS = BADDRESS,
                BCITY = BCITY,
                BSTATE = CorrectState(BSTATE),
                BZIP = BZIP,
                SNAME = SNAME,
                SADDRESS = SADDRESS,
                SCITY = SCITY,
                SSTATE = SSTATE,
                SZIP = SZIP,
                CUSTID = CUSTID,
                CC = CC,
                EXPMNTH = EXPMNTH,
                EXPYR = EXPYR,
                EMAIL = EMAIL
            };

            string strRequest = req.BuildStringFor("UN,PSWD,TERMS,METHOD,BNAME,BADDRESS,BCITY,BSTATE,BZIP,SNAME,SADDRESS,SCITY,SSTATE,SZIP,CUSTID,CC,EXPMNTH,EXPYR,EMAIL");
            int PayTraceRequestID = 0;
            PayTraceResponseData rdata = new PayTraceResponseData(UploadData(requestTypeIDs.UpdateCustomer, strRequest, "POST", out PayTraceRequestID)); // create using POST
            rdata.PayTraceRequestID = PayTraceRequestID;
            return rdata;

        }
        /// <summary>
        /// To delete customer. Do not call this directly, only for recurring payment
        /// </summary>
        /// <param name="CUSTID"></param>
        /// <returns></returns>
        private PayTraceResponseData DeleteCustomer(string CUSTID)
        {
            //‘format the request string to delete customer ID johndoe
            //strRequest = “UN~demo123|PSWD~demo123|TERMS~Y|METHOD~DeleteCustomer|”
            //strRequest = strRequest & “CUSTID~johndoe|”

            PayTraceRequestData req = new PayTraceRequestData()
            {
                UN = _username,
                PSWD = _password,
                TERMS = TERMS.Y,
                METHOD = METHODS.DeleteCustomer,
                CUSTID = CUSTID
            };

            string strRequest = req.BuildStringFor("UN,PSWD,TERMS,METHOD,CUSTID");

            int PayTraceRequestID = 0;
            PayTraceResponseData rdata = new PayTraceResponseData(UploadData(requestTypeIDs.DeleteCustomer, strRequest, WebClientMETHODS.POST, out PayTraceRequestID));
            rdata.PayTraceRequestID = PayTraceRequestID;
            return rdata;

        }

        /// <summary>
        /// Creating a customer. Do not call this directly. Only for Recurring Payment
        /// CustomerID (CUSTID) must be unique
        /// </summary>
        private PayTraceResponseData CreateCustomer(
            string CUSTID,
            string BNAME,
            string BADDRESS,
            string BCITY,
            string BSTATE,
            string BZIP,
            string SNAME,
            string SADDRESS,
            string SCITY,
            string SSTATE,
            string SZIP,
            string CC,
            string EXPMNTH,
            string EXPYR,
            string EMAIL
            )
        {
            //‘format the request string to create customer ID johndoe
            //strRequest = “UN~demo123|PSWD~demo123|TERMS~Y|METHOD~CreateCustomer|”
            //strRequest = strRequest & “BNAME~John Doe|BADDRESS~1234 Main|BCITY~Spokane|BSTATE~WA|”
            //strRequest = strRequest & “BZIP~99201|SNAME~John Doe|SADDRESS~5678 First|SCITY~Portland|”
            //strRequest = strRequest & “SSTATE~OR|SZIP~97201|CUSTID~johndoe|CC~4012881888818888|”
            //strRequest = strRequest & “EXPMNTH~12|EXPYR~12|EMAIL~support@paytrace.co

            PayTraceRequestData req = new PayTraceRequestData()
            {
                UN = _username,
                PSWD = _password,
                METHOD = METHODS.CreateCustomer,
                TERMS = TERMS.Y,
                BNAME = BNAME,
                BADDRESS = BADDRESS,
                BCITY = BCITY,
                BSTATE = CorrectState(BSTATE),
                BZIP = BZIP,
                SNAME = SNAME,
                SADDRESS = SADDRESS,
                SCITY = SCITY,
                SSTATE = SSTATE,
                SZIP = SZIP,
                CUSTID = CUSTID,
                CC = CC,
                EXPMNTH = EXPMNTH,
                EXPYR = EXPYR,
                EMAIL = EMAIL
            };

            string strRequest = req.BuildStringFor("UN,PSWD,TERMS,METHOD,BNAME,BADDRESS,BCITY,BSTATE,BZIP,SNAME,SADDRESS,SCITY,SSTATE,SZIP,CUSTID,CC,EXPMNTH,EXPYR,EMAIL");

            int PayTraceRequestID = 0;
            PayTraceResponseData rdata = new PayTraceResponseData(UploadData(requestTypeIDs.CreateCustomer, strRequest, WebClientMETHODS.POST, out PayTraceRequestID)); // create using POST
            rdata.PayTraceRequestID = PayTraceRequestID;

            return rdata;
        }

        private string CorrectState(string BSTATE)
        {
            string[,] array2D = new string[,] { 
                    {"Alabama","AL"},
                    {"Alaska","AK"},
                    {"Arizona","AZ"},
                    {"Arkansas","AR"},
                    {"California","CA"},
                    {"Colorado","CO"},
                    {"Connecticut","CT"},
                    {"Delaware","DE"},
                    {"Florida","FL"},
                    {"Georgia","GA"},
                    {"Hawaii","HI"},
                    {"Idaho","ID"},
                    {"Illinois","IL"},
                    {"Indiana","IN"},
                    {"Iowa","IA"},
                    {"Kansas","KS"},
                    {"Kentucky","KY"},
                    {"Louisiana","LA"},
                    {"Maine","ME"},
                    {"Maryland","MD"},
                    {"Massachusetts","MA"},
                    {"Michigan","MI"},
                    {"Minnesota","MN"},
                    {"Mississippi","MS"},
                    {"Missouri","MO"},
                    {"Montana","MT"},
                    {"Nebraska","NE"},
                    {"Nevada","NV"},
                    {"NewHampshire","NH"},
                    {"NewJersey","NJ"},
                    {"NewMexico","NM"},
                    {"NewYork","NY"},
                    {"NorthCarolina","NC"},
                    {"NorthDakota","ND"},
                    {"Ohio","OH"},
                    {"Oklahoma","OK"},
                    {"Oregon","OR"},
                    {"Pennsylvania","PA"},
                    {"RhodeIsland","RI"},
                    {"SouthCarolina","SC"},
                    {"SouthDakota","SD"},
                    {"Tennessee","TN"},
                    {"Texas","TX"},
                    {"Utah","UT"},
                    {"Vermont","VT"},
                    {"Virginia","VA"},
                    {"Washington","WA"},
                    {"WestVirginia","WV"},
                    {"Wisconsin","WI"},
                    {"Wyoming","WY"},
                    {"DistrictOfColumbia","DC"},
                    };


            string st = BSTATE.Replace("USA", "").Replace(";", "").Replace(" ", "").Trim();
            for (int i = 0; i < 51; i++)
            {
                if (array2D[i, 0].IndexOf(st) > -1)
                {
                    BSTATE = array2D[i, 1];
                }
            }


            return BSTATE;

        }

        /// <summary>
        /// Do not call this function without calling internal database first, use public PayTraceResponseData ProcessTransactionByUser
        /// </summary>
        /// <param name="INVOICE"></param>
        /// <param name="CC"></param>
        /// <param name="EXPMNTH"></param>
        /// <param name="EXPYR"></param>
        /// <param name="AMOUNT"></param>
        /// <param name="CSC"></param>
        /// <param name="BADDRESS"></param>
        /// <param name="BZIP"></param>
        /// <returns></returns>
        private PayTraceResponseData ProcessTransaction(string INVOICE, string CC, string EXPMNTH, string EXPYR, string AMOUNT, string CSC, string BADDRESS, string BZIP)
        {
            //‘format the request string to process a sale for $1.00
            //strRequest = “UN~demo123|PSWD~demo123|TERMS~Y|METHOD~ProcessTranx|”
            //strRequest = strRequest & “TRANXTYPE~Sale|CC~4012881888818888|EXPMNTH~12|EXPYR~12|”
            //strRequest = strRequest & “AMOUNT~1.00|CSC~999|BADDRESS~1234|BZIP~83852|INVOICE~8888|”

            //‘format the request string to process an authorization for $1.00
            //strRequest = “UN~demo123|PSWD~demo123|TERMS~Y|METHOD~ProcessTranx|”
            //strRequest = strRequest & “TRANXTYPE~Authorization|CC~4012881888818888|EXPMNTH~12|”
            //strRequest = strRequest & “EXPYR~12|AMOUNT~1.00|CSC~999|BADDRESS~1234|BZIP~83852|”
            //strRequest = strRequest & “INVOICE~8888|”

            PayTraceRequestData req = new PayTraceRequestData()
            {
                UN = _username,
                PSWD = _password,
                METHOD = METHODS.ProcessTranx,
                TERMS = TERMS.Y,
                TRANXTYPE = TRANXTYPE.Sale,
                INVOICE = INVOICE,
                CC = CC,
                BADDRESS = BADDRESS,
                CSC = CSC,
                BZIP = BZIP,
                AMOUNT = AMOUNT,
                EXPMNTH = EXPMNTH,
                EXPYR = EXPYR
            };

            string strRequest = req.BuildStringFor("UN,PSWD,TERMS,METHOD,TRANXTYPE,INVOICE,BADDRESS,BZIP,CC,CSC,EXPMNTH,EXPYR,AMOUNT");

            int PayTraceRequestID = 0;
            PayTraceResponseData rdata = new PayTraceResponseData(UploadData(requestTypeIDs.CreateCustomer, strRequest, WebClientMETHODS.POST, out PayTraceRequestID)); // create using POST
            rdata.PayTraceRequestID = PayTraceRequestID;
            return rdata;

        }

        public PayTraceResponseData RefundTransaction(string TRANXID)
        {

            //‘format the request string to process a refund for $1.00
            //strRequest = “UN~demo123|PSWD~demo123|TERMS~Y|METHOD~ProcessTranx|”
            //strRequest = strRequest & “TRANXTYPE~Refund|CC~4012881888818888|EXPMNTH~12|”
            //strRequest = strRequest & “EXPYR~12|AMOUNT~1.00|”

            //UN, PSWD, TERMS, METHOD, TRANXTYPE,TRANXID

            PayTraceRequestData req = new PayTraceRequestData()
            {
                UN = _username,
                PSWD = _password,
                METHOD = METHODS.ProcessTranx,
                TERMS = TERMS.Y,
                TRANXTYPE = TRANXTYPE.Refund,
                TRANXID = TRANXID
            };

            string strRequest = req.BuildStringFor("UN,PSWD,TERMS,METHOD,TRANXTYPE,TRANXID");

            int PayTraceRequestID = 0;
            PayTraceResponseData rdata = new PayTraceResponseData(UploadData(requestTypeIDs.CreateCustomer, strRequest, WebClientMETHODS.POST, out PayTraceRequestID)); // create using POST
            rdata.PayTraceRequestID = PayTraceRequestID;
            return rdata;

        }


        /// <summary>
        /// Do not call this directly! w/o marking our database 
        /// </summary>
        /// <param name="RECURID"></param>
        /// <returns></returns>
        private PayTraceResponseData DeleteRecurringTransaction(string RECURID)
        {
            //‘format the request string to delete the recurring transaction for the demo account
            //strRequest = "un~demo123|pswd~demo123|method~DeleteRecur|terms~Y|"
            //strRequest = strRequest & "RecurID~1333|"

            PayTraceRequestData req = new PayTraceRequestData()
            {
                UN = _username,
                PSWD = _password,
                METHOD = METHODS.DeleteRecur,
                TERMS = TERMS.Y,
                RECURID = RECURID
            };

            string strRequest = req.BuildStringFor("UN,PSWD,TERMS,METHOD,RECURID");

            int PayTraceRequestID = 0;
            PayTraceResponseData rdata = new PayTraceResponseData(UploadData(requestTypeIDs.CreateCustomer, strRequest, WebClientMETHODS.POST, out PayTraceRequestID)); // create using POST
            rdata.PayTraceRequestID = PayTraceRequestID;

            return rdata;
        }

        /// <summary>
        /// Do not call this directly! w/o updating database entry
        /// </summary>
        /// <param name="CUSTID"></param>
        /// <param name="START"></param>
        /// <param name="AMOUNT"></param>
        /// <param name="TOTALCOUNT"></param>
        /// <returns></returns>
        private PayTraceResponseData UpdateRecurringTransaction(string CUSTID, string RECURID, string NEXT, string AMOUNT, string TOTALCOUNT, string FREQUENCY)
        {
            //‘format the request string to update the recurring transaction for the demo account
            //strRequest = "un~demo123|pswd~demo123|method~updaterecur|terms~Y|"
            //strRequest = strRequest & "next~3/27/2012|amount~1.00|"
            //strRequest = strRequest & "recurid~8|totalcount~03|frequency~1|"
            //strRequest = strRequest & "custid~testcustomer|tranxtype~Sale|custreceipt~N|"

            PayTraceRequestData req = new PayTraceRequestData()
            {
                UN = _username,
                PSWD = _password,
                METHOD = METHODS.UpdateRecur,
                TERMS = TERMS.Y,
                TRANXTYPE = TRANXTYPE.Sale,
                CUSTRECEIPT = CUSTRECEIPT.N,
                FREQUENCY = FREQUENCY,
                NEXT = NEXT,
                AMOUNT = AMOUNT,
                TOTALCOUNT = TOTALCOUNT,
                CUSTID = CUSTID,
                RECURID = RECURID,
            };

            string strRequest = req.BuildStringFor("UN,PSWD,TERMS,METHOD,FREQUENCY,NEXT,AMOUNT,TOTALCOUNT,CUSTID,TRANXTYPE,CUSTRECEIPT,RECURID");

            int PayTraceRequestID = 0;
            PayTraceResponseData rdata = new PayTraceResponseData(UploadData(requestTypeIDs.CreateCustomer, strRequest, WebClientMETHODS.POST, out PayTraceRequestID)); // create using POST
            rdata.PayTraceRequestID = PayTraceRequestID;

            return rdata;
        }

        /// <summary>
        /// Do not call this directly! w/o adding database entry
        /// </summary>
        /// <param name="CUSTID"></param>
        /// <param name="START"></param>
        /// <param name="AMOUNT"></param>
        /// <param name="TOTALCOUNT"></param>
        /// <returns></returns>
        private PayTraceResponseData CreateRecurringTransaction(string CUSTID, string START, string AMOUNT, string TOTALCOUNT, string FREQUENCY)
        {
            //‘format the request string to create a recurring transaction for the demo account
            //strRequest = "un~demo123|pswd~demo123|method~createrecur|terms~Y|"
            //strRequest = Request & "start~3/26/2012|amount~1.00|totalcount~03|frequency~1|"
            //strRequest = strRequest & "custid~testcustomer|tranxtype~SALE|custreceipt~N|"


            PayTraceRequestData req = new PayTraceRequestData()
            {
                UN = _username,
                PSWD = _password,
                METHOD = METHODS.CreateRecur,
                TERMS = TERMS.Y,
                FREQUENCY = FREQUENCY,
                START = START,
                AMOUNT = AMOUNT,
                TOTALCOUNT = TOTALCOUNT,
                CUSTID = CUSTID,
                TRANXTYPE = TRANXTYPE.Sale,
                CUSTRECEIPT = CUSTRECEIPT.N,
            };

            string strRequest = req.BuildStringFor("UN,PSWD,TERMS,METHOD,FREQUENCY,START,AMOUNT,TOTALCOUNT,CUSTID,TRANXTYPE,CUSTRECEIPT");

            int PayTraceRequestID = 0;
            PayTraceResponseData rdata = new PayTraceResponseData(UploadData(requestTypeIDs.CreateCustomer, strRequest, WebClientMETHODS.POST, out PayTraceRequestID)); // create using POST
            rdata.PayTraceRequestID = PayTraceRequestID;

            return rdata;
        }
        #endregion

        /// <summary>
        /// Do not call this directly. Only private
        /// </summary>
        /// <param name="RequestTypeID"></param>
        /// <param name="strRequest"></param>
        /// <param name="uploadMethod"></param>
        /// <param name="PayTraceRequestID"></param>
        /// <returns></returns>
        private string UploadData(requestTypeIDs RequestTypeID, string strRequest, string uploadMethod, out int PayTraceRequestID)
        {
            // put request in the log
            PayTraceRequestID = 0;
            DRspTA_PayTraceRequestResponse_InsertRequest insert = SQLDataPayTrace.spTA_PayTraceRequestResponse_InsertRequest((int)RequestTypeID, strRequest);
            if (insert.Count > 0 && insert.PayTraceRequestID(0) > 0)
            {
                PayTraceRequestID = insert.PayTraceRequestID(0);
            }


            WebClient wClient = new WebClient();
            wClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            string sResponse = "";

            if (uploadMethod.ToUpper() != WebClientMETHODS.POST)
            {
                sResponse = wClient.UploadString(PayTraceURL + "?" + strRequest, "");
            }
            else
            {
                sResponse = wClient.UploadString(PayTraceURL, strRequest);
            }

            if (PayTraceRequestID > 0)
            {
                DRspTA_PayTraceRequestResponse_UpdateResponse update = SQLDataPayTrace.spTA_PayTraceRequestResponse_UpdateResponse(PayTraceRequestID, sResponse);
            }

            return sResponse;
        }

        #region PublicProperties
        public string UN
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
            }
        }

        public string PSWD
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
            }
        }
        #endregion


    }
}
