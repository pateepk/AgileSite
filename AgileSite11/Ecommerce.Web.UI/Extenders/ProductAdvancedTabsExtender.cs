using CMS;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;
using CMS.UIControls;

[assembly: RegisterCustomClass("ProductAdvancedTabsExtender", typeof(CMS.Ecommerce.Web.UI.ProductAdvancedTabsExtender))]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Extender for Advanced tab of product detail
    /// </summary>
    public class ProductAdvancedTabsExtender : UITabsExtender
    {
        private SKUInfo sku;
        private int siteId;
        private ICMSDocumentManager documentManager;

        /// <summary>
        /// Initializes the extender
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();

            siteId = QueryHelper.GetInteger("siteId", SiteContext.CurrentSiteID);

            // Get SKUID from querystring or from edited document
            var skuId = QueryHelper.GetInteger("productId", 0);
            if ((skuId <= 0) && (documentManager.Node != null))
            {
                skuId = documentManager.Node.NodeSKUID;
            }

            // Get product info
            if (skuId > 0)
            {
                sku = SKUInfoProvider.GetSKUInfo(skuId);
            }
        }


        /// <summary>
        /// Initialization of tabs.
        /// </summary>
        public override void OnInitTabs()
        {
            var page = (CMSPage)Control.Page;

            Control.OnTabCreated += OnTabCreated;

            // Setup the document manager
            documentManager = page.DocumentManager;
            ScriptHelper.RegisterClientScriptBlock(page, typeof(string), "refreshtree", ScriptHelper.GetScript("function RefreshTree(expandNodeId, selectNodeId) { parent.RefreshTree(expandNodeId, selectNodeId); } function SelectNode(selectNodeId) { parent.SelectNode(selectNodeId); }"));
        }

        /// <summary>
        /// OnTabCreated event handler.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">The TabCreatedEventArgs instance containing the event data.</param>
        protected void OnTabCreated(object sender, TabCreatedEventArgs e)
        {
            if (e.Tab == null)
            {
                return;
            }

            var tab = e.Tab;
            var element = e.UIElement;
            var node = documentManager.Node;

            bool splitViewSupported = false;
            string lowerElementName = element.ElementName.ToLowerCSafe();

            // Skip some elements if editing just SKU without document binding
            switch (lowerElementName)
            {
                case "products.relatedproducts":
                    if (node == null)
                    {
                        e.Tab = null;
                        return;
                    }

                    break;

                case "products.documents":
                    var displayTree = ECommerceSettings.ProductsTree(siteId) == ProductsTreeModeEnum.Sections;
                    if ((node == null) && displayTree)
                    {
                        if (!MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || (sku == null) || !sku.IsGlobal)
                        {
                            e.Tab = null;
                            return;
                        }
                    }
                    break;

                case "products.workflow":
                case "products.versions":
                    splitViewSupported = true;
                    if ((node == null) || (documentManager.Workflow == null))
                    {
                        e.Tab = null;
                        return;
                    }
                    break;
            }

            // Add SiteId parameter to each tab
            if (!string.IsNullOrEmpty(tab.RedirectUrl) && (siteId != SiteContext.CurrentSiteID))
            {
                tab.RedirectUrl = URLHelper.AddParameterToUrl(tab.RedirectUrl, "siteId", siteId.ToString());
            }

            // Ensure split view mode
            if (splitViewSupported && PortalUIHelper.DisplaySplitMode)
            {
                tab.RedirectUrl = DocumentUIHelper.GetSplitViewUrl(tab.RedirectUrl);
            }
        }
    }
}