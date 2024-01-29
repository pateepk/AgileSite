using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.PortalEngine.Web.UI;
using CMS.DocumentEngine.Web.UI;
using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.CustomTables;

namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class CUCustomTableDeleteItem : CMSAbstractWebPart
    {
        #region "Properties"
        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string CustomTable
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("CustomTable"), "");
            }
            set
            {
                this.SetValue("CustomTable", value);
            }
        }
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!RequestHelper.IsPostBack())
            {
                if (QueryHelper.GetString("action", "").ToLower() == "delete")
                {
                    //Item ID
                    int key = 0;
                    string customTable = CustomTable; //Code name of custom table
                    key = QueryHelper.GetInteger("RecordKey", 0); //Get query parameter from URL

                    if (key > 0)
                    {
                        //delete the custom table item

                        //Definition of custom table in this case
                        DataClassInfo dataClassInfo = DataClassInfoProvider.GetDataClassInfo(customTable);

                        if (dataClassInfo != null)
                        {
                            CustomTableItem item = CustomTableItemProvider.GetItem(key, customTable);
                            item.Delete();
                            lblMsg.Text = "Record has been deleted.<br />";
                            lblMsg.Style["color"] = "Green";
                        }
                    }
                }
            }
        }
    }
}