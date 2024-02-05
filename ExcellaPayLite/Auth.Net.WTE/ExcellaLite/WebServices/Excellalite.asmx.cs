using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using PaymentProcessor.Web.Applications;
using System.Xml;
using WTE.Communication;

namespace ExcellaLite.WebServices
{
    /// <summary>
    /// Summary description for Excellalite
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Excellalite : System.Web.Services.WebService
    {

        #region DataContainer
        public class InvoicePayResultData
        {
            public bool success { get; set; }
            public int InvoiceID { get; set; }
            public int InvoiceItemID { get; set; }
            public string ErrorMessage { get; set; }
            public string InvoiceStatus { get; set; }
            public decimal InvoiceTotal { get; set; }
            public decimal AmountInTransit { get; set; }
            public decimal AmountPaid { get; set; }
            public decimal AmountDue { get; set; }
            public string InvoiceTotalDollar { get; set; }
            public string AmountInTransitDollar { get; set; }
            public string AmountPaidDollar { get; set; }
            public string AmountDueDollar { get; set; }
        }

        public class XMLDataResult
        {
            public bool success { get; set; }
            public string xml { get; set; }
        }

        public class InvoiceItemDeleteResultData
        {
            public bool success { get; set; }
            public int InvoiceItemID { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class InvoiceItemFailResultData
        {
            public bool success { get; set; }
            public int InvoiceItemID { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class InvoiceItemPaidResultData
        {
            public bool success { get; set; }
            public int InvoiceItemID { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class InvoiceArchiveResultData
        {
            public bool success { get; set; }
            public int InvoiceID { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class InvoiceNoteInsertResultData
        {
            public bool success { get; set; }
            public int InvoiceID { get; set; }
            public int InvoiceItemID { get; set; }
            public Int64 InvoiceNoteID { get; set; }
            public string ErrorMessage { get; set; }
        }
        
        #endregion

        protected UserManager user = null;
        protected RequestManager request = null;

        #region Constructor
        public Excellalite()
        {
            AppManager.PagePreInit(out user, out request);
        }

        public Excellalite(UserManager User, RequestManager Request)
        {
            user = User;
            request = Request;
        }
        #endregion

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }


        private string ComposeInvoiceInfo(int InvoiceID, string BackdoorGUID)
        {
            string body = "";
            DRspWTE_excellalite_invoices_GetByUserID invoice = SQLData.spWTE_excellalite_invoices_GetByUserID(user.userID, InvoiceID, 0, String.Empty);
            if (invoice.Count > 0)
            {
                body = Utils.getAppSettings(SV.AppSettings.EmailTemplate_InvoiceInfo).ToString();
                //Do not re-number the format
                //Invoice Number: {0}
                //CustNum: {1}
                //Status: {2}
                //Project: {3}
                //Due Date: {4}
                //Terms: {5}
                //Document: {6}
                //Invoice Date: {7}
                //Invoice Total: {8}
                //Amount Paid: {9}
                //Amount In Transit: {10}
                //Amount Due: {11}
                // For detail invoice, please visit {12}

                string invoiceLink = "";

                if (BackdoorGUID.Length > 0)
                {
                    invoiceLink = Utils.ResolveFullPathURL("/Account/Login/") + "?guid=" + BackdoorGUID + "&" + SV.Common.URLReturn + "=" + Utils.UrlEncode(Utils.ResolveFullPathURL("/Invoices/MyInvoices/?InvoiceID=" + InvoiceID));
                }
                else
                {
                    invoiceLink = Utils.ResolveFullPathURL("/") + "?" + SV.Common.URLReturn + "=" + Utils.UrlEncode(Utils.ResolveFullPathURL("/Invoices/MyInvoices/?InvoiceID=" + InvoiceID));
                }

                try
                {
                    body = String.Format(body
                    , invoice.Invoice(0)
                    , invoice.CustNum(0)
                    , invoice.InvoiceStatus(0)
                    , invoice.Project(0)
                    , invoice.DateDue(0).ToString(SV.Common.SimpleDateExpression)
                    , invoice.Terms(0)
                    , invoice.docname(0)
                    , invoice.invoiceDate(0).ToString(SV.Common.SimpleDateExpression)
                    , invoice.InvoiceTotal(0).ToString(SV.Common.CurrencyExpression)
                    , invoice.AmountPaid(0).ToString(SV.Common.CurrencyExpression)
                    , invoice.AmountInTransit(0).ToString(SV.Common.CurrencyExpression)
                    , invoice.AmountDue(0).ToString(SV.Common.CurrencyExpression)
                    , invoiceLink
                    );
                }
                catch (Exception ex)
                {
                    ErrorManager.logError(String.Format(SV.ErrorMessages.Application_ErrorFormatingEmail, SV.AppSettings.EmailTemplate_InvoiceInfo), ex);
                    body = "- error formating email - ";

                }



            }

            return body;
        }

        [WebMethod(Description = "Change status invoice to archive, only if it is paid", EnableSession = true)]
        public string InvoiceArchive(string InvoiceID)
        {
            InvoiceArchiveResultData rs = new InvoiceArchiveResultData()
            {
                success = false,
                ErrorMessage = "",
                InvoiceID = 0
            };
            int intInvoiceID = 0;
            int.TryParse(InvoiceID, out intInvoiceID);

            DRspWTE_excellalite_invoices_Archive archive = SQLData.spWTE_excellalite_invoices_Archive(user.userID, intInvoiceID);
            if (archive.Count > 0 && archive.ErrorMessage.Length == 0 && archive.RowUpdated(0) > 0)
            {
                rs.success = true;
                rs.InvoiceID = intInvoiceID;
                rs.ErrorMessage = "";
            }
            else
            {
                rs.InvoiceID = intInvoiceID;
                if (archive.Count > 0)
                {
                    rs.ErrorMessage = archive.ErrorMessage;
                }
            }

            return Utils.ObjectToJSON(rs);
        }


        [WebMethod(Description = "Change status item to be failed", EnableSession = true)]
        public string InvoiceItemFail(string InvoiceItemID, string PaidNote)
        {
            InvoiceItemFailResultData rs = new InvoiceItemFailResultData()
            {
                success = false,
                ErrorMessage = "",
                InvoiceItemID = 0
            };

            int intInvoiceItemID = 0;
            int.TryParse(InvoiceItemID, out intInvoiceItemID);

            if ((intInvoiceItemID > 0) && (user.isLogin) && user.isUserAdministrator)
            {
                DRspWTE_excellalite_invoices_items_Fail fail = SQLData.spWTE_excellalite_invoices_items_Fail(intInvoiceItemID, user.userID, PaidNote);
                if (fail.Count > 0 && fail.ErrorMessage.Length == 0 && fail.RowUpdated(0) > 0)
                {
                    rs.success = true;
                    rs.InvoiceItemID = intInvoiceItemID;
                    rs.ErrorMessage = "";

                    string fromEmail = Utils.getAppSettings(SV.AppSettings.BillingEmail).ToString();
                    string header = Utils.getAppSettings(SV.AppSettings.EmailTemplate_PaymentDecline).ToString();
                    string subject = String.Format("Payment declined (ID:{0})", intInvoiceItemID);


                    DRspWTE_excellalite_invoices_items_GetByInvoiceItemID item = SQLData.spWTE_excellalite_invoices_items_GetByInvoiceItemID(user.userID, intInvoiceItemID);


                    //Your payment for invoice: {0}
                    //Payment Item:
                    //Item ID: {1}
                    //Amount: {2}
                    //Payment Date: {3}
                    //Note: {4}

                    //has been DECLINED for some reasons. Please contact billing department.

                    //Below is the information about the invoice:


                    try
                    {
                        if (item.Count > 0)
                        {
                            header = String.Format(header,
                                 item.Invoice(0)
                                , intInvoiceItemID
                                , item.Amount(0).ToString(SV.Common.CurrencyExpression)
                                , item.PaidDate(0).ToString(SV.Common.SimpleDateExpression)
                                , item.PaidNote(0).Length > 0 ? item.PaidNote(0) : "-"
                                );
                        }

                    }
                    catch (Exception ex)
                    {
                        ErrorManager.logError(String.Format(SV.ErrorMessages.Application_ErrorFormatingEmail, SV.AppSettings.EmailTemplate_PaymentDecline), ex);
                        header = "- error formating email - ";
                    }

                    // send to admin
                    DRspUsers_GetAdministrators administrators = SQLData.spUsers_GetAdministrators();
                    for (int i = 0; i < administrators.Count; i++)
                    {
                        string body = header + ComposeInvoiceInfo(fail.InvoiceID(0), administrators.BackdoorGUID(i));
                        // pull information about Invoice and its status
                        EmailManager email = new EmailManager();
                        email.SendEmail(fromEmail, administrators.Email(i), subject + String.Format(" (Admin Copy: {0})", administrators.LoginID(i)), body);
                    }

                    // send to customers
                    DRspUsers_GetByCustNum customers = SQLData.spUsers_GetByCustNum(fail.CustNum(0));
                    for (int i = 0; i < customers.Count; i++)
                    {
                        string body = header + ComposeInvoiceInfo(fail.InvoiceID(0), customers.BackdoorGUID(i));
                        // pull information about Invoice and its status
                        EmailManager email = new EmailManager();
                        email.SendEmail(fromEmail, customers.Email(i), subject + String.Format(" (Customer Copy: {0})", fail.CustNum(0)), body);
                    }


                }
                else
                {
                    rs.InvoiceItemID = intInvoiceItemID;
                    if (fail.Count > 0)
                    {
                        rs.ErrorMessage = fail.ErrorMessage;
                    }
                }
            }

            return Utils.ObjectToJSON(rs);
        }

        

        [WebMethod(Description = "Change status item to be paid", EnableSession = true)]
        public string InvoiceItemPaid(string InvoiceItemID, string PaidNote)
        {
            InvoiceItemPaidResultData rs = new InvoiceItemPaidResultData()
            {
                success = false,
                ErrorMessage = "",
                InvoiceItemID = 0
            };

            int intInvoiceItemID = 0;
            int.TryParse(InvoiceItemID, out intInvoiceItemID);

            if ((intInvoiceItemID > 0) && (user.isLogin))
            {
                DRspWTE_excellalite_invoices_items_UpdateToPaid paid = SQLData.spWTE_excellalite_invoices_items_UpdateToPaid(user.userID, intInvoiceItemID, PaidNote);
                if (paid.Count > 0 && paid.ErrorMessage.Length == 0 && paid.RowUpdated(0) > 0)
                {
                    rs.success = true;
                    rs.InvoiceItemID = intInvoiceItemID;
                    rs.ErrorMessage = "";


                    // we have to send email individually ! because each link has backdoor atach to the userid
                    
                    string fromEmail = Utils.getAppSettings(SV.AppSettings.BillingEmail).ToString();
                    string header = Utils.getAppSettings(SV.AppSettings.EmailTemplate_PaymentPaid).ToString();
                    string subject = String.Format("Payment completed (ID:{0})", intInvoiceItemID);

                    DRspWTE_excellalite_invoices_items_GetByInvoiceItemID item = SQLData.spWTE_excellalite_invoices_items_GetByInvoiceItemID(user.userID, intInvoiceItemID);

                    //Your payment for invoice: {0}
                    //Payment Item:
                    //Item ID: {1}
                    //Amount: {2}
                    //Payment Date: {3}

                    //has been COMPLETED. Thank you!

                    //Below is the information about the invoice:


                    try
                    {
                        if (item.Count > 0)
                        {
                            header = String.Format(header,
                                 item.Invoice(0)
                                , intInvoiceItemID
                                , item.Amount(0).ToString(SV.Common.CurrencyExpression)
                                , item.PaidDate(0).ToString(SV.Common.SimpleDateExpression)
                                , item.PaidNote(0).Length > 0 ? item.PaidNote(0) : "-"
                                );
                        }

                    }
                    catch (Exception ex)
                    {
                        ErrorManager.logError(String.Format(SV.ErrorMessages.Application_ErrorFormatingEmail, SV.AppSettings.EmailTemplate_PaymentPaid), ex);
                        header = "- error formating email - ";
                    }

                    // send to admin
                    DRspUsers_GetAdministrators administrators = SQLData.spUsers_GetAdministrators();
                    for (int i = 0; i < administrators.Count; i++)
                    {
                        string body = header + ComposeInvoiceInfo(paid.InvoiceID(0), administrators.BackdoorGUID(i));
                        // pull information about Invoice and its status
                        EmailManager email = new EmailManager();
                        email.SendEmail(fromEmail, administrators.Email(i), subject + String.Format(" (Admin Copy: {0})", administrators.LoginID(i)), body);
                    }

                    // send to customers
                    DRspUsers_GetByCustNum customers = SQLData.spUsers_GetByCustNum(paid.CustNum(0));
                    for (int i = 0; i < customers.Count; i++)
                    {
                        string body = header + ComposeInvoiceInfo(paid.InvoiceID(0), customers.BackdoorGUID(i));
                        // pull information about Invoice and its status
                        EmailManager email = new EmailManager();
                        email.SendEmail(fromEmail, customers.Email(i), subject + String.Format(" (Customer Copy: {0})", paid.CustNum(0)), body);
                    }



                }
                else
                {
                    rs.InvoiceItemID = intInvoiceItemID;
                    if (paid.Count > 0)
                    {
                        rs.ErrorMessage = paid.ErrorMessage;
                    }
                }
            }

            return Utils.ObjectToJSON(rs);
        }


        [WebMethod(Description = "Get Data XML from Activity History, Admin only.", EnableSession = true)]
        public string GetDataXML(string ActivityHistoryID)
        {
            XMLDataResult rs = new XMLDataResult();
            rs.success = false;

            if ((user.isLogin) && (user.isUserAdministrator))
            {
                rs.success = true;
                rs.xml = string.Empty;
                Int64 id = 0;
                Int64.TryParse(ActivityHistoryID, out id);
                DRspActivityHistory_GetByID ah = SQLData.spActivityHistory_GetByID(id);
                if (ah.Count > 0)
                {
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(ah.DataXml(0));
                    rs.xml = Utils.XMLToHTML(xdoc);
                }
            }
            return Utils.ObjectToJSON(rs);
        }

        [WebMethod(Description = "Delete item in an invoice", EnableSession = true)]
        public string InvoiceItemDelete(string InvoiceItemID, string InvoiceNote)
        {
            InvoiceItemDeleteResultData rs = new InvoiceItemDeleteResultData()
            {
                success = false,
                ErrorMessage = "",
                InvoiceItemID = 0
            };

            int intInvoiceItemID = 0;
            int.TryParse(InvoiceItemID, out intInvoiceItemID);

            if ((intInvoiceItemID > 0) && (user.isLogin))
            {
                // we check authority of deleting in the SQL
                DRspWTE_excellalite_invoices_items_Delete del = SQLData.spWTE_excellalite_invoices_items_Delete(intInvoiceItemID, user.userID, InvoiceNote);
                if (del.Count > 0 && del.ErrorMessage.Length == 0 && del.RowDeleted(0) > 0)
                {
                    rs.success = true;
                    rs.InvoiceItemID = intInvoiceItemID;
                    rs.ErrorMessage = "";
                }
                else
                {
                    rs.InvoiceItemID = intInvoiceItemID;
                    if (del.Count > 0)
                    {
                        rs.ErrorMessage = del.ErrorMessage;
                    }
                }

            }
            return Utils.ObjectToJSON(rs);
        }


        [WebMethod(Description = "Insert a payment on invoice", EnableSession = true)]
        public string InvoicePay(string InvoiceID, string Amount, string InvoiceNote)
        {
            InvoicePayResultData rs = new InvoicePayResultData()
            {
                success = false,
                InvoiceID = 0,
                InvoiceItemID = 0,
                ErrorMessage = "",
                AmountDue = 0,
                AmountInTransit = 0,
                AmountPaid = 0,
                InvoiceStatus = "",
                InvoiceTotal = 0,
                AmountDueDollar = "",
                AmountInTransitDollar = "",
                AmountPaidDollar = "",
                InvoiceTotalDollar = ""                 
            };

            int intInvoiceID = 0;
            int.TryParse(InvoiceID, out intInvoiceID);
            decimal decAmount = 0;
            decimal.TryParse(Amount, out decAmount);

            rs.success = true;
            rs.InvoiceID = intInvoiceID;

            if ((intInvoiceID > 0) && (user.userID > 0) && (decAmount > 0))
            {
                DRspWTE_excellalite_invoices_InsertItem insertPay = SQLData.spWTE_excellalite_invoices_InsertItem(user.userID, intInvoiceID, decAmount, InvoiceNote);
                if (insertPay.Count > 0)
                {
                    DRspWTE_excellalite_invoices_GetByUserID invoiceData = SQLData.spWTE_excellalite_invoices_GetByUserID(user.userID, intInvoiceID);
                    if (invoiceData.Count > 0)
                    {
                        rs.InvoiceTotal = invoiceData.InvoiceTotal(0);
                        rs.AmountInTransit = invoiceData.AmountInTransit(0);
                        rs.AmountDue = invoiceData.AmountDue(0);
                        rs.AmountPaid = invoiceData.AmountPaid(0);
                        rs.InvoiceStatus = invoiceData.InvoiceStatus(0);
                        rs.AmountInTransitDollar = String.Format(SV.Common.CurrencyFormat, invoiceData.AmountInTransit(0));
                        rs.AmountDueDollar = String.Format(SV.Common.CurrencyFormat, invoiceData.AmountDue(0));
                        rs.AmountPaidDollar = String.Format(SV.Common.CurrencyFormat, invoiceData.AmountPaid(0));
                        rs.InvoiceTotalDollar = String.Format(SV.Common.CurrencyFormat, invoiceData.InvoiceTotal(0));
                    }

                    rs.InvoiceItemID = insertPay.invoiceItemID(0);
                    if (rs.InvoiceItemID == 0)
                    {
                        rs.ErrorMessage = insertPay.ErrorMessage;
                    }
                    // send email
                    string fromEmail = Utils.getAppSettings(SV.AppSettings.BillingEmail).ToString();
                    string header = Utils.getAppSettings(SV.AppSettings.EmailTemplate_NewInTransitPayment).ToString();

                    DRspWTE_excellalite_invoices_items_GetByInvoiceItemID item = SQLData.spWTE_excellalite_invoices_items_GetByInvoiceItemID(user.userID, insertPay.invoiceItemID(0));

                    //There is a new payment coming for invoice: {0}
                    //Payment Item:
                    //Item ID: {1}
                    //Amount: {2}
                    //Payment Date: {3}
                    //Requested by User: {4}
                    //Email: {5}
                    //Requested from IP address: {6}

                    try
                    {
                        if (item.Count > 0)
                        {
                            header = String.Format(header,
                                 item.Invoice(0)
                                , insertPay.invoiceItemID(0)
                                , item.Amount(0).ToString(SV.Common.CurrencyExpression)
                                , item.CreatedDate(0).ToString(SV.Common.SimpleDateExpression)
                                , user.FullName
                                , user.Email
                                , Utils.getClientIPAddress()
                                );
                        }

                    }
                    catch (Exception ex)
                    {
                        ErrorManager.logError(String.Format(SV.ErrorMessages.Application_ErrorFormatingEmail, SV.AppSettings.EmailTemplate_NewInTransitPayment), ex);
                        header = "- error formating email - ";
                    }
                   

                    string subject = String.Format("New payment (ID:{0})", insertPay.invoiceItemID(0));

                    // send to admin only
                    DRspUsers_GetAdministrators administrators = SQLData.spUsers_GetAdministrators();
                    for (int i = 0; i < administrators.Count; i++)
                    {
                        string body = header + ComposeInvoiceInfo(intInvoiceID, administrators.BackdoorGUID(i));
                        // pull information about Invoice and its status
                        EmailManager email = new EmailManager();
                        email.SendEmail(fromEmail, administrators.Email(i), subject + String.Format(" (Admin Copy: {0})", administrators.LoginID(i)), body);
                    }

                }
                else
                {
                    rs.ErrorMessage = insertPay.ErrorMessage;
                }
            }


            return Utils.ObjectToJSON(rs);

        }

        [WebMethod(Description = "Insert a note on Invoice or InvoiceItem. One of parameters (InvoiceID, InvoiceItemID can be 0)", EnableSession = true)]
        public string InvoiceNoteInsert(string InvoiceID, string InvoiceItemID, string InvoiceNote)
        {
            InvoiceNoteInsertResultData rs = new InvoiceNoteInsertResultData()
            {
                ErrorMessage = "",
                InvoiceID = 0,
                InvoiceItemID = 0,
                InvoiceNoteID = 0,
                success = false
            };

            int intInvoiceID = 0;
            int intInvoiceItemID = 0;
            int.TryParse(InvoiceID, out intInvoiceID);
            int.TryParse(InvoiceItemID, out intInvoiceItemID);

            if ((intInvoiceID > 0 || intInvoiceItemID > 0) && (user.userID > 0))
            {
                DRspWTE_excellalite_invoices_notes_InsertItem insert = SQLData.spWTE_excellalite_invoices_notes_InsertItem(intInvoiceID, intInvoiceItemID, user.userID, InvoiceNote);
                if (insert.Count > 0 && insert.invoiceNoteID(0) > 0)
                {
                    rs.success = true;
                    rs.InvoiceID = insert.invoiceID(0);
                    rs.InvoiceItemID = insert.invoiceItemID(0);
                    rs.InvoiceNoteID = insert.invoiceNoteID(0);
                } else
                {
                    rs.ErrorMessage = insert.ErrorMessage;
                }

            }

            return Utils.ObjectToJSON(rs);

        }

    }
}
