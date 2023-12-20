using System;

using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Helper methods for product UI
    /// </summary>
    public class ProductUIHelper
    {
        /// <summary>
        /// Gets URL for new document language version page.
        /// </summary>
        /// <param name="appendCurrentQuery">If true, current query string is appended</param>
        public static string GetNewCultureVersionPageUrl(bool appendCurrentQuery = true)
        {
            string url = "~/CMSModules/Ecommerce/Pages/Tools/Products/NewCultureVersion.aspx";
            if (appendCurrentQuery)
            {
                url += RequestContext.CurrentQueryString;
            }

            return url;
        }


        /// <summary>
        /// Gets UI page URL for document action.
        /// </summary>
        /// <param name="settings">URL configuration object</param>
        public static string GetProductActionPageUrl(UIPageURLSettings settings)
        {
            string result = null;
            switch (settings.Action)
            {
                case "content":
                    {
                        result = "Product_List.aspx";

                        // Check if products are to be displayed in tree
                        if (ECommerceSettings.DisplayProductsInSectionsTree(SiteContext.CurrentSiteName))
                        {
                            var tree = new TreeProvider();
                            var node = tree.SelectSingleNode(settings.NodeID);

                            if ((node != null) && node.HasSKU)
                            {
                                // Check if edited node belongs to product document type
                                var ci = DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(node.NodeClassName);
                                if ((ci != null) && ci.ClassIsProduct)
                                {
                                    result = GetProductEditUrl();

                                    result = URLHelper.AppendQuery(result, "?sectionId=" + settings.NodeID);
                                }
                            }
                        }

                        // Get usable document ID
                        result = URLHelper.AddParameterToUrl(result, "nodeid", settings.NodeID.ToString());
                        result = URLHelper.AddParameterToUrl(result, "culture", settings.Culture);
                    }
                    break;

                case "delete":
                    {
                        result = "Product_Section.aspx";
                        result = URLHelper.AddParameterToUrl(result, "action", "delete");
                        result = URLHelper.AddParameterToUrl(result, "nodeid", settings.NodeID.ToString());
                    }
                    break;

                case "new":
                    {
                        result = "Product_Section.aspx";
                        result = URLHelper.AddParameterToUrl(result, "action", "new");
                        result = URLHelper.AddParameterToUrl(result, "parentnodeid", settings.NodeID.ToString());
                        result = URLHelper.AddParameterToUrl(result, "parentculture", settings.Culture);
                    }
                    break;
            }

            // Append additional query when result URL found
            if (!String.IsNullOrEmpty(result))
            {
                result = URLHelper.AppendQuery(result, settings.AdditionalQuery);
                result = URLHelper.AddParameterToUrl(result, "hash", QueryHelper.GetHash(result));
            }

            return result ?? DocumentUIHelper.GetDocumentActionPageUrl(settings);
        }


        /// <summary>
        /// Gets the product edit URL
        /// </summary>
        public static string GetProductEditUrl()
        {
            return UIContextHelper.GetElementUrl(ModuleName.ECOMMERCE, "Products.Properties", false);
        }


        /// <summary>
        /// Gets UI page URL for URL configuration object.
        /// </summary>
        /// <param name="settings">URL configuration object</param>
        public static string GetProductPageUrl(UIPageURLSettings settings)
        {
            switch (settings.Action)
            {
                case "copy":
                case "move":
                case "link":
                case "delete":
                    if (settings.NodeID <= 0)
                    {
                        return null;
                    }
                    break;
            }

            string result = null;

            var tree = new TreeProvider();

            switch (settings.Mode)
            {
                // Bypass full listing mode for product nodes
                case "listing":

                // Bypass section properties for product nodes
                case "sectionedit":
                    TreeNode node = tree.SelectSingleNode(settings.NodeID);
                    if ((node != null) && node.HasSKU)
                    {
                        settings.Mode = "edit";
                        settings.Action = "content";
                    }

                    break;
            }

            if (settings.Mode == "listing")
            {
                // Show full listing of section
                result = "Product_List.aspx";

                result = URLHelper.AddParameterToUrl(result, "nodeid", settings.NodeID.ToString());
                result = URLHelper.AddParameterToUrl(result, "culture", settings.Culture);
                result = URLHelper.AddParameterToUrl(result, "showSections", "1");
            }

            if (settings.Mode == "sectionedit")
            {
                bool isSplitMode = PortalUIHelper.DisplaySplitMode;

                result = UrlResolver.ResolveUrl("~/CMSModules/Content/CMSDesk/Edit/edit.aspx");

                // Check (and ensure) the proper content culture
                if (!CMSPage.CheckPreferredCulture())
                {
                    CMSPage.RefreshParentWindow();
                }

                // Check if node is available in given culture
                TreeNode node = tree.SelectSingleNode(settings.NodeID, settings.Culture, false, false);
                if (node == null)
                {
                    // Document does not exist -> redirect to new culture version creation dialog
                    result = UrlResolver.ResolveUrl(GetNewCultureVersionPageUrl(false));
                    result = URLHelper.AddParameterToUrl(result, "culture", settings.Culture);

                    // Bypass split mode
                    isSplitMode = false;
                }
                else
                {
                    // Get data class 
                    DataClassInfo ci = DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(node.NodeClassName);

                    // Check if view page URL is set
                    if ((ci != null) && !String.IsNullOrEmpty(ci.ClassEditingPageURL))
                    {
                        result = UrlResolver.ResolveUrl(ci.ClassEditingPageURL);
                    }

                    result = URLHelper.AddParameterToUrl(result, "siteId", "0");
                }

                result = URLHelper.AddParameterToUrl(result, "mode", "productssection");
                result = URLHelper.AddParameterToUrl(result, "nodeid", settings.NodeID.ToString());


                if (settings.TransformToCompareUrl)
                {
                    PortalUIHelper.SplitModeCultureCode = settings.Culture;
                    result = DocumentUIHelper.TransformToCompareUrl(result);
                }
                else
                {
                    if (isSplitMode)
                    {
                        result = DocumentUIHelper.GetSplitViewUrl(result);
                    }
                }
            }

            return result ?? DocumentUIHelper.GetDocumentPageUrl(settings, GetProductActionPageUrl);
        }


        /// <summary>
        /// Creates product breadcrumbs.
        /// </summary>
        /// <param name="breadcrumbs">Breadcrumbs control.</param>
        /// <param name="productText">New object text which is displayed in breadcrumbs.</param>
        /// <param name="isSection"> Object is section.</param>
        /// <param name="targetParent">Add Target _parent to parent breadcrumb</param>
        /// <param name="displaySuffix"> Display breadcrumbs suffix.</param>
        public static void EnsureProductBreadcrumbs(Breadcrumbs breadcrumbs, string productText, bool isSection = false, bool targetParent = true, bool displaySuffix = true)
        {
            var url = UIContextHelper.GetElementUrl(ModuleName.ECOMMERCE, "Products");

            breadcrumbs.AddBreadcrumb(new BreadcrumbItem
            {
                Text = ResHelper.GetString("com.ui.products"),
                Target = (targetParent) ? "_parent" : "",
                RedirectUrl = url
            });

            breadcrumbs.AddBreadcrumb(new BreadcrumbItem
            {
                Text = ResHelper.GetString(HTMLHelper.HTMLEncode(productText))
            });

            var typeCodeName = (isSection) ? "com.productsection" : "objecttype.com_sku";

            if (displaySuffix)
            {
                // Register breadcrumbsSuffix to client application for Breadcrumbs.js module
                UIHelper.SetBreadcrumbsSuffix(ResHelper.GetString(typeCodeName, CultureHelper.PreferredUICultureCode));
            }
        }
    }
}