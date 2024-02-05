    using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using PaymentProcessor.Web.Applications;
using System.IO;

namespace ExcellaLite.Invoices
{
    /// <summary>
    /// Summary description for DownloadInvoicePDF
    /// </summary>
    public class DownloadInvoicePDF : IHttpHandler, IRequiresSessionState
    {

        protected UserManager user = null;
        protected RequestManager request = null;

        public void ProcessRequest(HttpContext context)
        {
            AppManager.PagePreInit(out user, out request);
            Utils.DisablePageCaching();
            int InvoiceID = Utils.getQueryString("InvoiceID").ToInt();
            bool IsAttachment = Utils.getQueryString("IsAttachment").ToBool();

            context.Response.ContentType = SV.Common.PDFContentType;
            string FilePath = "";
            DRspWTE_excellalite_invoices_GetByUserID invoice = SQLData.spWTE_excellalite_invoices_GetByUserID(user.userID, InvoiceID);
            if (invoice.Count > 0)
            {
                FilePath = Utils.getMapFile(SV.ApplicationPath.AppInvoiceFiles + invoice.docname(0));
                if(!File.Exists(FilePath))
                {
                    FilePath = Utils.getMapFile(SV.Common.InvoiceNotFound);
                    long WebErrorID = ErrorManager.logMessage(String.Format(SV.ErrorMessages.PDFNotFoundInFolder, invoice.docname(0)));
                    DRspActivityHistory_Insert insert = SQLData.spActivityHistory_Insert(activityIDs.InvoicePDFnotfound, user.userID, WebErrorID);
                }
            } else
            {
                // dummy PDF
                FilePath = Utils.getMapFile(SV.Common.InvoiceNotFound);
                ErrorManager.logMessage(String.Format(SV.ErrorMessages.PDFNotAuthInDB,  InvoiceID));
            }
            if (IsAttachment)
            {
                context.Response.AddHeader(SV.Common.ContentDisposition, String.Format("attachment;filename={0}", Utils.getFileNameOnlyFromPath(FilePath)));
            }
            context.Response.WriteFile(FilePath);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}