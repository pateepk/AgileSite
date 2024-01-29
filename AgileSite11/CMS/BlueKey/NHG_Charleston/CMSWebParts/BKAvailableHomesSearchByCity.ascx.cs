using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.CustomTables;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;

namespace NHG_C
{
    public partial class BlueKey_CMSWebParts_BKAvailableHomesSearchByCity : CMSAbstractWebPart
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PopulateCity();
        }

        private void PopulateCity()
        {
            DataSet ds = CustomTableItemProvider.GetItems("customlocations.city", "CityVisible = 1", "CityName ASC", 0, "CityName, MAX(ItemID) as ItemID").GroupBy("CityName");

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                ddCity.DataSource = ds.Tables[0];
                ddCity.DataTextField = "CityName";
                ddCity.DataValueField = "ItemID";
                ddCity.DataBind();
            }
        }
    }
}