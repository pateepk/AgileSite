using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;

namespace NHG_T
{
    public partial class BlueKey_CMSWebParts_ViewBuildersFloorPlan : CMSAbstractWebPart
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string href = string.Empty;
            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

            CMS.DocumentEngine.TreeNode node = tree.SelectSingleNode(DocumentContext.CurrentDocument.DocumentNodeID);
            if (node != null)
            {
                href = ValidationHelper.GetString(node.GetValue("DeveloperFloorPlanUrl"), string.Empty);
            }

            if (!String.IsNullOrEmpty(href))
            {
                //ltlHref.Text = href;
                ltlHref.HRef = href;
                divViewBuildersFloorPlan.Visible = true;
            }
        }
    }
}