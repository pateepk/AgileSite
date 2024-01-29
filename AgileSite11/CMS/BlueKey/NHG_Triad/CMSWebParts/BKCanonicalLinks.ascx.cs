using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.PortalEngine.Web.UI;

namespace NHG_T
{
    public partial class BlueKey_CMSWebParts_BKCanonicalLinks : CMSAbstractWebPart
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.StopProcessing)
            {
                // do nothing
            }
            else
            {
                string nodeAliasPath = CMS.DocumentEngine.DocumentContext.CurrentDocument.NodeAliasPath.ToLower();
                string currentUrlPath = CMS.Helpers.RequestContext.CurrentURL.ToLower();
                if (nodeAliasPath != currentUrlPath)
                {
                    this.Page.Header.Controls.Add(new LiteralControl("<link rel=\"canonical\" href=\"" + HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + CMS.DocumentEngine.DocumentContext.CurrentDocument.NodeAliasPath + ".aspx\" />" + Environment.NewLine));

                }
            }
        }
    }
}