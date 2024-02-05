using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using PaymentProcessor.Web.Applications;
using System.Text;

namespace ExcellaLite.Invoices
{
    public partial class InvoiceInIFrame : BasePage
    {

        

        protected int InvoiceID = 0;
        protected int InvoiceStatusID = 0;

        protected void Page_Load()
        {
            this.EnableViewState = false;
            InvoiceID = Utils.getQueryString("InvoiceID").ToInt();
            if (user.isLogin)
            {
                DRspWTE_excellalite_invoices_GetByUserID inv = SQLData.spWTE_excellalite_invoices_GetByUserID(user.userID, InvoiceID);
                if (inv.Count > 0)
                {
                    InvoiceStatusID = inv.InvoiceStatusID(0);
                }
                else
                {
                    ErrorManager.logMessage(String.Format(SV.ErrorMessages.PDFNotAuthInDB, InvoiceID));
                    Utils.responseRedirect("Default.aspx");
                }
            }
            else
            {
                Utils.responseRedirect("Default.aspx");
            }
        }

    }
}