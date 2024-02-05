using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using PaymentProcessor.Web.Applications;
using System.Text;

namespace ExcellaLite.Invoices
{
    public partial class MyInvoices : BasePage
    {

        public struct Columns
        {
            public const string PartialPayButton = "PartialPayButton";
            public const string FullPayButton = "FullPayButton";
            public const string InvoiceDetailLink = "InvoiceDetailLink";
            public const string InsertNoteButton = "InsertNoteButton";
        }

        public struct ItemsColumns
        {
            public const string ItemDeleteLink = "ItemDeleteLink";
            public const string ItemProcessLink = "ItemProcessLink";
        }

        protected string WebServices_InvoiceArchiveURL = Utils.ResolveURL("webservices/Excellalite.asmx/InvoiceArchive");
        protected string WebServices_PayPartialURL = Utils.ResolveURL("webservices/Excellalite.asmx/InvoicePay");
        protected string WebServices_InvoiceItemDeleteURL = Utils.ResolveURL("webservices/Excellalite.asmx/InvoiceItemDelete");
        protected string WebServices_InvoiceItemPaidURL = Utils.ResolveURL("webservices/Excellalite.asmx/InvoiceItemPaid");
        protected string WebServices_InvoiceItemFailURL = Utils.ResolveURL("webservices/Excellalite.asmx/InvoiceItemFail");
        protected string WebServices_InvoiceNoteInsertURL = Utils.ResolveURL("webservices/Excellalite.asmx/InvoiceNoteInsert");
        
        protected DRspWTE_excellalite_invoices_GetByUserID invoices = null;
        protected DRspWTE_excellalite_invoices_items_GetByInvoiceID items = null;
        protected DRspWTE_excellalite_invoices_notes_GetAllNotes notes = null;
    
        protected int TotalInvoices = 0;
        protected string AllInvoiceIDs = "";
        protected decimal TotalItemsAmount = 0;
        protected decimal TotalInTransitAmount = 0;
        protected decimal TotalPaidAmount = 0;
        protected decimal TotalDueAmount = 0;

        protected int InvoiceID = 0;
        protected int InvoiceItemID = 0;
        protected int InvoiceStatusID = 0;
        protected string CustNum = ""; // if admin, CustNum can be a filter, if not admin just pass UserID to Stored Procedure

        protected void Page_Load()
        {
            this.EnableViewState = false;
            InvoiceID = Utils.getQueryString("InvoiceID").ToInt();
            InvoiceItemID = Utils.getQueryString("InvoiceItemID").ToInt();
            InvoiceStatusID = Utils.getQueryString("InvoiceStatusID").ToInt();
            CustNum = Utils.getQueryString("CustNum").ToString();

            invoiceStatusIDs EnumInvoiceStatusID = (invoiceStatusIDs)Enum.Parse(typeof(invoiceStatusIDs), InvoiceStatusID.ToString(), true);

            if (EnumInvoiceStatusID == 0)
            {
                EnumInvoiceStatusID =  invoiceStatusIDs.VirtualOpenAndInTransit; // virtual 0+1
            }

            Repeater1.DataSource = null;
            Repeater2.DataSource = null;

            if (user.isLogin)
            {

                if ((user.isUserAdministrator)&&(InvoiceID == 0)&&(InvoiceStatusID==0))
                {
                    DRspWTE_excellalite_invoices_GetCustNum custnums = SQLData.spWTE_excellalite_invoices_GetCustNum(InvoiceStatusID);
                    StringBuilder sb = new StringBuilder();
                    if (custnums.Count > 1)
                    {
                        if (CustNum.Length > 0)
                        {
                            sb.Append("<a href=\"?\">-all-</a> | ");
                        }
                        for (int i = 0; i < custnums.Count; i++)
                        {
                            if (custnums.CustNum(i) == CustNum)
                            {
                                sb.AppendFormat("<b>{0}</b>", custnums.CustNum(i));
                            }
                            else
                            {
                                sb.AppendFormat("<a href=\"?CustNum={1}\">{0}</a>", custnums.CustNum(i), Utils.UrlEncode(custnums.CustNum(i)));
                            }
                            if (i < custnums.Count - 1)
                            {
                                sb.Append(" | ");
                            }
                        }
                    }
                    CustNumLiteral.Text = sb.ToString();
                }
                else
                {
                    CustNumLiteral.Text = "";
                }

                invoices = SQLData.spWTE_excellalite_invoices_GetByUserID(user.userID, InvoiceID, EnumInvoiceStatusID, CustNum);
                if (invoices.Count > 0)
                {
                    TotalInvoices = invoices.Count;
                    DataTable dt = invoices.DataSource;
                    dt.Columns.Add(Columns.PartialPayButton);
                    dt.Columns.Add(Columns.FullPayButton);
                    dt.Columns.Add(Columns.InvoiceDetailLink);
                    dt.Columns.Add(Columns.InsertNoteButton);
                    string SelectedCustNum = "";

                    for (int i = 0; i < invoices.Count; i++)
                    {
                        dt.Rows[i][Columns.InvoiceDetailLink] = String.Format("<a title=\"Click for details\" href=\"/Invoices/MyInvoices/?InvoiceID={0}\">{1}</a>", invoices.invoiceID(i), invoices.Invoice(i));
                        if ((invoices.InvoiceStatusID(i) == (int)invoiceStatusIDs.Open) || ((invoices.InvoiceStatusID(i) == (int)invoiceStatusIDs.InTransit) && (invoices.InvoiceTotal(i) - invoices.AmountPaid(0) - invoices.AmountInTransit(i) > 0))) // if on in transit check any partial left
                        {
                            dt.Rows[i][Columns.PartialPayButton] = String.Format("<input type=\"button\" class=\"ui-button ui-widget ui-corner-all\" id=\"PayPartialButton_{0}\" onclick=\"PayPartial({0},{1},{2},{3},{4});\" value=\"Pay Partial\">", invoices.invoiceID(i), invoices.InvoiceTotal(i), invoices.AmountInTransit(i), invoices.AmountPaid(i), invoices.AmountDue(i));
                            dt.Rows[i][Columns.FullPayButton] = String.Format("<input type=\"button\" class=\"ui-button ui-widget ui-corner-all\"  id=\"PayFullButton_{0}\" onclick=\"PayFull({0},{1},{2},{3},{4});\" value=\"Pay Full\">", invoices.invoiceID(i), invoices.InvoiceTotal(i), invoices.AmountInTransit(i), invoices.AmountPaid(i), invoices.AmountDue(i));
                            // dt.Rows[i][Columns.InsertNoteButton] = String.Format("<input type=\"button\" class=\"ui-button ui-widget ui-corner-all\" id=\"InsertNoteButton_{0}\" onclick=\"InsertNote({0});\" value=\"Add Note\"><input type=\"text\" id=\"InvoiceNote_{0}\">", invoices.invoiceID(i));
                        } else if (invoices.InvoiceStatusID(i) == (int)invoiceStatusIDs.InTransit)
                        {
                            dt.Rows[i][Columns.PartialPayButton] = SV.Common.WaitingForProcess; // there is full in transit payment waiting to be processed
                        } else if (invoices.InvoiceStatusID(i) == (int)invoiceStatusIDs.Paid)
                        {
                            dt.Rows[i][Columns.PartialPayButton] = String.Format("<input type=\"button\" class=\"ui-button ui-widget ui-corner-all\" id=\"ArchivelButton_{0}\" onclick=\"ArchiveInvoice({0});\" value=\"Archive\">", invoices.invoiceID(i));
                        }
                        else
                        {
                            dt.Rows[i][Columns.PartialPayButton] = "";
                            dt.Rows[i][Columns.FullPayButton] = "";
                            dt.Rows[i][Columns.InsertNoteButton] = "";
                        }
                        AllInvoiceIDs += invoices.invoiceID(i) + ",";

                        if (InvoiceID > 0 && invoices.invoiceID(i) == InvoiceID)
                        {
                            TotalItemsAmount = invoices.InvoiceTotal(i);
                            TotalPaidAmount = invoices.AmountPaid(i);
                            TotalInTransitAmount = invoices.AmountInTransit(i);
                            TotalDueAmount = invoices.AmountDue(i);
                            SelectedCustNum = invoices.CustNum(i);
                            InvoiceStatusID = invoices.InvoiceStatusID(i);
                        }
                    }
                    PagedDataSource objPds1 = new PagedDataSource();
                    objPds1.DataSource = invoices.DataSource.DefaultView;
                    //objPds1.AllowPaging = true;
                    //objPds1.PageSize = 5;
                    //objPds1.CurrentPageIndex = 1;
                    Repeater1.DataSource = objPds1;

                    if (InvoiceID > 0)
                    {
                        items = SQLData.spWTE_excellalite_invoices_items_GetByInvoiceID(user.userID, InvoiceID);
                        notes = SQLData.spWTE_excellalite_invoices_notes_GetAllNotes(user.userID, InvoiceID);

                        DataTable dt2 = items.DataSource;
                        dt2.Columns.Add(ItemsColumns.ItemDeleteLink);
                        dt2.Columns.Add(ItemsColumns.ItemProcessLink);
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (items.InvoiceItemStatusID(i) < (int)invoiceItemStatusIDs.Paid)
                            {
                                if (user.CustNum == SelectedCustNum)
                                {
                                    dt2.Rows[i][ItemsColumns.ItemDeleteLink] = String.Format("<a title=\"Click To delete\" href=\"javascript:void(0);\" onclick=\"PaymentItemDelete({0})\">DEL</a>", items.invoiceItemID(i));
                                }
                                else
                                {
                                    dt2.Rows[i][ItemsColumns.ItemDeleteLink] = String.Format("<a title=\"Only user/customer can delete this item. Admin can only reject/fail the payment.\" href=\"javascript:void(0);\"><strike>DEL</strike></a>");
                                }
                            }
                            else
                            {
                                dt2.Rows[i][ItemsColumns.ItemDeleteLink] = String.Format("<a title=\"Only in transit payment can be deleted.\" href=\"javascript:void(0);\"><strike>DEL</strike></a>");
                            }
                            if ((items.PaidUserID(i) == 0) &&(user.isUserAdministrator))
                            {
                                dt2.Rows[i][ItemsColumns.ItemProcessLink] = String.Format("<input id=\"ChkFailProcess_{0}\" type=\"checkbox\"> Failed&nbsp;<input title=\"You see this button because you are an administrator\" type=\"button\" class=\"ui-button ui-widget ui-corner-all\" value=\"Process Now\" onclick=\"ProcessItemNow({0},'${1:#0.00}')\" />", items.invoiceItemID(i), items.Amount(i));
                            }
                            else
                            {
                                dt2.Rows[i][ItemsColumns.ItemProcessLink] = "";
                            }
                        }
                        PagedDataSource objPds2 = new PagedDataSource();
                        objPds2.DataSource = items.DataSource.DefaultView;
                        //objPds1.AllowPaging = true;
                        //objPds1.PageSize = 5;
                        //objPds1.CurrentPageIndex = 1;
                        Repeater2.DataSource = objPds2;

                        if (notes.Count > 0)
                        {
                            PagedDataSource objPds3 = new PagedDataSource();
                            objPds3.DataSource = notes.DataSource.DefaultView;
                            Repeater3.DataSource = objPds3;
                        } else
                        {
                            Repeater3.DataSource = null;
                        }
                    }


                }

                Repeater1.DataBind();
                Repeater2.DataBind();
                Repeater3.DataBind();

            }
            else
            {
                Utils.responseRedirect("Default.aspx");
            }
        }

    }
}