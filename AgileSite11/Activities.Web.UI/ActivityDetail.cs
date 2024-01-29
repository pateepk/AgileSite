using System;
using System.Web.UI.WebControls;

using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.UIControls;

using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.Activities.Web.UI
{
    /// <summary>
    /// Activity detail user control.
    /// </summary>
    public class ActivityDetail : CMSUserControl
    {
        /// <summary>
        /// Loads data to control. Returns false if data not found.
        /// </summary>
        /// <param name="ai">Activity info object</param>
        public virtual bool LoadData(ActivityInfo ai)
        {
            return false;
        }


        /// <summary>
        /// Encodes and assigns text to label.
        /// </summary>
        /// <param name="lbl">Label</param>
        /// <param name="text">Text</param>
        protected void EncodeLabel(Label lbl, string text)
        {
            lbl.Text = HTMLHelper.HTMLEncode(text);
        }


        /// <summary>
        /// Returns document for the given document node ID.
        /// </summary>
        /// <param name="nodeId">Document node ID</param>
        /// <param name="culture">Culture</param>
        protected TreeNode GetDocumentByNodeID(int nodeId, string culture)
        {
            if (nodeId <= 0)
            {
                return null;
            }

            TreeProvider tp = new TreeProvider(MembershipContext.AuthenticatedUser);
            TreeNode tn = DocumentHelper.GetDocument(nodeId, culture, true, tp);
            return tn;
        }


        /// <summary>
        /// Returns URL to administration for the given document.
        /// </summary>
        /// <param name="node">TreeNode of the Site</param>
        protected string GetURL(TreeNode node)
        {
            if (node == null)
            {
                return null;
            }

            int siteID = node.NodeSiteID;
            SiteInfo site = SiteInfoProvider.GetSiteInfo(siteID);
            if (site != null)
            {
                string url = URLHelper.GetAbsoluteUrl("~/Admin/cmsadministration.aspx?action=edit", site.DomainName);

                int nodeID = node.NodeID;
                url = URLHelper.AddParameterToUrl(url, "nodeid", nodeID.ToString());

                string culture = node.DocumentCulture;
                url = URLHelper.AddParameterToUrl(url, "culture", culture + ApplicationUrlHelper.GetApplicationHash("cms.content", "content"));

                return url;
            }

            return null;
        }


        /// <summary>
        /// Returns link.
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="content">Link text (will be HTML encoded)</param>
        public static string GetLink(string url, string content)
        {
            if (String.IsNullOrEmpty(url))
            {
                return ResHelper.GetString("general.na");
            }

            return "<a target=\"_blank\" href=\'" + url + "'\" >" + HTMLHelper.HTMLEncode(content) + "</a>";
        }


        /// <summary>
        /// Returns link for given document node ID to administration.
        /// </summary>
        /// <param name="nodeId">Document node ID</param>
        /// <param name="culture">Document culture</param>
        protected string GetLinkForDocument(int nodeId, string culture)
        {
            string url = null;
            string docName = null;

            TreeNode tn = GetDocumentByNodeID(nodeId, culture);
            if (tn != null)
            {
                url = GetURL(tn);
                docName = tn.GetDocumentName();
            }

            return GetLink(url, docName);
        }
    }
}