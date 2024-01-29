using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using PaymentProcessor.Web.Applications;
using System.Text;
using System.IO;

namespace ExcellaLite.Invoices
{
    public partial class CheckMissingDocuments : BasePage
    {



        protected void Page_Load()
        {
            this.EnableViewState = false;


            if (user.isLogin)
            {

                if (user.isUserAdministrator)
                {
                    string FilePath = Utils.getMapFile(SV.ApplicationPath.AppInvoiceFiles);
                    DRspWTE_excellalite_invoices_GetByUserID getallopen = SQLData.spWTE_excellalite_invoices_GetByUserID(user.userID, 0, invoiceStatusIDs.Open, String.Empty);
                    if (getallopen.Count > 0)
                    {
                        DataTable dt = new DataTable();
                        for (int i = 0; i < getallopen.Count; i++)
                        {
                            string FileName = Utils.getMapFile(SV.ApplicationPath.AppInvoiceFiles) + getallopen.docname(0);
                            if (!File.Exists(FilePath))
                            {

                            }
                        }
                    }
                }
            }
            else
            {
                Utils.responseRedirect("Default.aspx");
            }
        }

    }
}