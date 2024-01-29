using System;

using CMS.Base;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for the E-commerce products pages to apply global settings to the pages.
    /// </summary>
    public class CMSProductsPage : CMSEcommerceObjectsPage
    {
        #region "Variables"

        private bool? mDisplayTreeInProducts;
        private string mProductsStartingPath;
        private int mProductId;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether the page is for normal product or for product option.
        /// </summary>
        public bool IsProductOption
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if content tree is displayed in product management UI. Value is based on setting for current site.
        /// </summary>
        public bool DisplayTreeInProducts
        {
            get
            {
                if (!mDisplayTreeInProducts.HasValue)
                {
                    mDisplayTreeInProducts = ECommerceSettings.ProductsTree(CurrentSiteName) == ProductsTreeModeEnum.Sections;
                }

                return mDisplayTreeInProducts.Value;
            }
        }


        /// <summary>
        /// Starting path indicating where the product sub tree starts. Reflects also UserStartingAliasPath property of current user.
        /// </summary>
        public string ProductsStartingPath
        {
            get
            {
                if (mProductsStartingPath == null)
                {
                    // Get more strict starting path
                    mProductsStartingPath = ECommerceSettings.ProductsStartingPath(CurrentSiteName) ?? "";
                    string userStartingPath = CurrentUser.UserStartingAliasPath ?? "";
                    if (userStartingPath.TrimEnd('%').StartsWithCSafe(mProductsStartingPath.TrimEnd('%')))
                    {
                        // Check if users starting path exists on current site
                        TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                        TreeNode rootNode = tree.SelectSingleNode(CurrentSiteName, userStartingPath, TreeProvider.ALL_CULTURES, false, null, false);
                        if (rootNode != null)
                        {
                            mProductsStartingPath = userStartingPath;
                        }
                    }
                }

                return mProductsStartingPath;
            }
        }


        /// <summary>
        /// ID of the product taken from query parameter 'productId' or from node specified by query parameter 'nodeId'.
        /// </summary>
        public int ProductID
        {
            get
            {
                if (mProductId <= 0)
                {
                    mProductId = QueryHelper.GetInteger("productId", 0);
                }

                if ((mProductId <= 0) && (Node != null))
                {
                    mProductId = Node.NodeSKUID;
                }

                return mProductId;
            }
        }

        #endregion


        #region "Page events

        /// <summary>
        /// Page OnPreInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnPreInit(EventArgs e)
        {
            // Ensure document manager only for products with node 
            if ((NodeID > 0) || (QueryHelper.GetInteger("parentNodeId", 0) > 0))
            {
                EnsureDocumentManager = true;
            }

            base.OnPreInit(e);
        }


        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            GlobalObjectsKeyName = IsProductOption ? ECommerceSettings.ALLOW_GLOBAL_PRODUCT_OPTIONS : ECommerceSettings.ALLOW_GLOBAL_PRODUCTS;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Appends nodeId parameters to given URL.
        /// </summary>
        /// <param name="url">URL to append parameters to</param>
        protected string AddNodeIDParameterToUrl(string url)
        {
            if (NodeID >= 0)
            {
                url = URLHelper.AddParameterToUrl(url, "nodeId", NodeID.ToString());
            }

            return url;
        }


        /// <summary>
        /// Appends nodeId and siteId parameters to given URL.
        /// </summary>
        /// <param name="url">URL to append parameters to</param>
        protected string AddProductParametersToUrl(string url)
        {
            url = AddNodeIDParameterToUrl(url);
            url = URLHelper.UpdateParameterInUrl(url, "siteId", SiteID.ToString());

            return url;
        }


        /// <summary>
        /// Checks if site id of edited object corresponds to current site ID and site settings. If it does not, 
        /// user is redirected to 'Object doesn't exist' page.
        /// </summary>
        /// <param name="editedProductSiteId">ID of the site which edited product belongs to</param>
        protected override void CheckEditedObjectSiteID(int editedProductSiteId)
        {
            // An attempt to configure site-specific record which does not belong to current site
            if ((editedProductSiteId != SiteContext.CurrentSiteID) && ((editedProductSiteId != 0) || !AllowGlobalObjects))
            {
                EditedObject = null;
            }
        }


        /// <summary>
        /// Called when requested language version of product section does not exist in split mode.
        /// </summary>
        protected virtual void RequestSectionNewCultureVersion()
        {
            RequestNewCultureVersion("productsection");
        }


        /// <summary>
        /// Called when requested language version of document does not exist in split mode.
        /// </summary>
        /// <param name="mode">Parameter to be supplied in query under 'mode' key.</param>
        protected virtual void RequestNewCultureVersion(string mode)
        {
            var url = ProductUIHelper.GetNewCultureVersionPageUrl();
            url += URLHelper.AddParameterToUrl(url, "mode", mode);

            // Redirect
            URLHelper.Redirect(url);
        }


        /// <summary>
        /// Redirects to new culture page
        /// </summary>
        protected override void RedirectToNewCultureVersionPage()
        {
            RequestNewCultureVersion("product");
        }


        /// <summary>
        /// Validates product price.
        /// </summary>
        /// <returns>Null or validation error.</returns>
        protected override string ValidatePrice(decimal price, CurrencyInfo currency, SKUInfo product)
        {
            if (price < 0)
            {
                return string.Format(GetString("com.skuprice.pricerange"));
            }

            return base.ValidatePrice(price, currency, product);
        }


        /// <summary>
        /// Checks if user has permission explore tree which is needed to create product when products are displayed in tree mode.
        /// </summary>
        /// <returns></returns>
        protected bool CheckExploreTreePermission()
        {
            if (DisplayTreeInProducts)
            {
                // Check 'Explore tree' module permission
                if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.Content", "ExploreTree", SiteContext.CurrentSiteName))
                {
                    RedirectToAccessDenied("CMS.Content", "ExploreTree");
                }
            }

            return true;
        }


        /// <summary>
        /// Creates product breadcrumbs.
        /// </summary>
        /// <param name="breadcrumbs">Breadcrumbs control.</param>
        /// <param name="productText">New object text which is displayed in breadcrumbs.</param>
        /// <param name="isSection">Object is section.</param>
        /// <param name="targetParent">Add Target _parent to parent breadcrumb</param>
        /// <param name="displaySuffix">Display breadcrumbs suffix.</param>
        protected void EnsureProductBreadcrumbs(Breadcrumbs breadcrumbs, string productText, bool isSection = false, bool targetParent = true, bool displaySuffix = true)
        {
            ProductUIHelper.EnsureProductBreadcrumbs(breadcrumbs, productText, isSection, targetParent, displaySuffix);
        }

        #endregion
    }
}