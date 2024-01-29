using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System;
using System.Data;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Base;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Site map control that displays the whole menu structure or it part.
    /// </summary>
    [ToolboxData("<{0}:CMSSiteMap runat=server></{0}:CMSSiteMap>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class CMSSiteMap : CMSAbstractMenuProperties
    {
        #region "Private variables"

        /// <summary>
        /// Rendered HTML code.
        /// </summary>
        private string mRenderedHTML = String.Empty;

        /// <summary>
        /// Filter name.
        /// </summary>
        protected string mFilterName = null;

        /// <summary>
        /// Filter control.
        /// </summary>
        protected CMSAbstractBaseFilterControl mFilterControl = null;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates whether the sitemap should apply menu inactivation flag.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates whether the sitemap should apply menu inactivation flag.")]
        public bool ApplyMenuInactivation
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ApplyMenuInactivation"], true);
            }
            set
            {
                ViewState["ApplyMenuInactivation"] = value;
            }
        }


        /// <summary>
        /// Gets or set rendered HTML code.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Get or set rendered HTML code")]
        public string RenderedHTML
        {
            get
            {
                return mRenderedHTML;
            }
            set
            {
                mRenderedHTML = value;
            }
        }


        /// <summary>
        /// Indicates if data will be loaded automatically.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if data will be loaded automatically.")]
        public bool LoadDataAutomaticaly
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["LoadDataAutomaticaly"], true);
            }
            set
            {
                ViewState["LoadDataAutomaticaly"] = value;
            }
        }


        /// <summary>
        /// Specifies target frame for all URLs.
        /// </summary>
        public string UrlTarget
        {
            get
            {
                return Convert.ToString(ViewState["UrlTarget"]);
            }
            set
            {
                ViewState["UrlTarget"] = value;
            }
        }


        /// <summary>
        /// Render the link title attribute?
        /// </summary>
        [Category("Behavior"), Description("Render the title attribute within the menu links.")]
        public bool RenderLinkTitle
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["RenderLinkTitle"], false);
            }
            set
            {
                ViewState["RenderLinkTitle"] = value;
            }
        }


        /// <summary>
        /// Filter control.
        /// </summary>
        public CMSAbstractBaseFilterControl FilterControl
        {
            get
            {
                if (mFilterControl == null)
                {
                    if (!DataHelper.IsEmpty(FilterName))
                    {
                        mFilterControl = CMSControlsHelper.GetFilter(FilterName) as CMSAbstractBaseFilterControl;
                    }
                }
                return mFilterControl;
            }
            set
            {
                mFilterControl = value;
            }
        }


        /// <summary>
        /// Gets or Set filter name.
        /// </summary>
        public string FilterName
        {
            get
            {
                return mFilterName;
            }
            set
            {
                mFilterName = value;
            }
        }


        /// <summary>
        /// Indicates if menu caption should be HTML encoded.
        /// </summary>
        [Category("Behavior"), Description("Indicates if menu caption should be HTML encoded.")]
        public bool EncodeMenuCaption
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["EncodeMenuCaption"], true);
            }
            set
            {
                ViewState["EncodeMenuCaption"] = value;
            }
        }

        #endregion //Public properties


        #region "Methods"

        /// <summary>
        /// Reload data.
        /// </summary>
        /// <param name="forceReload">If is true data will be always loaded</param>
        public override void ReloadData(bool forceReload)
        {
            SetContext();

            // Clear rendered HTML code
            mRenderedHTML = String.Empty;

            // Do not load data if is stop processing set
            if (StopProcessing)
            {
                return;
            }

            if (Context == null)
            {
                mRenderedHTML = "[ CMSSiteMap \"" + ID + "\"] ";
                return;
            }

            EnableViewState = false;

            if (FilterControl != null)
            {
                FilterControl.InitDataProperties(this);
            }

            mRenderedHTML = GetSiteMap();

            ReleaseContext();
        }


        /// <summary>
        /// Renders the control.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            // Render only if is not stop processing or if is some HTML code generated
            if (!StopProcessing)
            {
                if (LoadDataAutomaticaly)
                {
                    ReloadData(false);
                }

                // Hide control for zero rows or display zero rows text
                if (DataHelper.DataSourceIsEmpty(DataSource))
                {
                    if (HideControlForZeroRows)
                    {
                        return;
                    }
                    else if (!String.IsNullOrEmpty(ZeroRowsText))
                    {
                        output.Write(ZeroRowsText);
                        return;
                    }
                }

                output.Write(mRenderedHTML);
            }
        }


        /// <summary>
        /// OnLoad.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (LoadDataAutomaticaly)
            {
                ReloadData(false);
            }

            base.OnLoad(e);
        }


        /// <summary>
        /// Returns string representation of site map source code.
        /// </summary>
        protected string GetSiteMap()
        {
            // Fill in the items
            DataSource = GetDataSource(true);

            if (DataHelper.DataSourceIsEmpty(DataSource))
            {
                return String.Empty;
            }

            // Get source code of items under the parent node
            int parentId = ValidationHelper.GetInteger(DataSource.Tables[0].DefaultView[0]["NodeParentID"], 0);
            return GetItems(parentId);
        }


        /// <summary>
        /// Returns source code of items under specified parent node.
        /// </summary>
        /// <param name="parentNodeId">Parent Node ID</param>
        protected string GetItems(int parentNodeId)
        {
            // Get items
            var items = GroupedDS.GetGroupView(parentNodeId);
            if (items != null)
            {
                var localResolver = ContextResolver.CreateChild();

                StringBuilder result = new StringBuilder(items.Count * 100);
                result.Append(@"<ul class=""CMSSiteMapList"">");

                foreach (DataRowView dr in items)
                {
                    // Add current datarow to the resolver
                    localResolver.SetAnonymousSourceData(dr.Row);

                    result.Append(@"<li class=""CMSSiteMapListItem"">");

                    string url;
                    bool doNotResolve = false;
                    string onClick = String.Empty;

                    // Get url
                    if (ValidationHelper.GetString(dr["DocumentMenuRedirectURL"], String.Empty) != String.Empty)
                    {
                        url = UrlResolver.ResolveUrl(localResolver.ResolveMacros(ValidationHelper.GetString(dr["DocumentMenuRedirectURL"], String.Empty)));
                    }
                    else if (ValidationHelper.GetString(dr["DocumentMenuJavascript"], String.Empty) != String.Empty)
                    {
                        // Javascript
                        url = localResolver.ResolveMacros(Convert.ToString(dr["DocumentMenuJavascript"]));
                        if (!url.ToLowerCSafe().StartsWithCSafe("javascript:"))
                        {
                            doNotResolve = true;
                            onClick = url;
                            url = "#";
                        }
                    }
                    else
                    {
                        url = DocumentURLProvider.GetNavigationUrl(new DataRowContainer(dr), localResolver);
                    }

                    // Prepare the item name. Disable encoding. Encoding depends on "EncodeMenuCaption" property
                    localResolver.Settings.EncodeResolvedValues = false;
                    string itemName = localResolver.ResolveMacros(TreePathUtils.GetMenuCaption(Convert.ToString(dr["DocumentMenuCaption"]), Convert.ToString(dr["DocumentName"])));
                    localResolver.Settings.EncodeResolvedValues = true;

                    // HTML encode item name
                    if (EncodeMenuCaption)
                    {
                        itemName = HTMLHelper.HTMLEncode(itemName);
                    }

                    // If current item is inactive and 'Apply inactivation' is enabled make inactive item
                    if (ValidationHelper.GetBoolean(dr["DocumentMenuItemInactive"], false) && ApplyMenuInactivation)
                    {
                        result.Append("<span>");
                        result.Append(itemName);
                        result.Append("</span>");
                    }
                    else
                    {
                        result.Append(@"<a href=""");
                        result.Append(doNotResolve ? HttpUtility.UrlPathEncode(url) : ResolveUrl(HttpUtility.UrlPathEncode(url)));
                        result.Append(@""" class=""CMSSiteMapLink"" ");
                        if (UrlTarget != "")
                        {
                            result.Append(@" target=""");
                            result.Append(UrlTarget);
                            result.Append(@"""");
                        }
                        if (onClick != "")
                        {
                            result.Append(@" onclick=""");
                            result.Append(onClick);
                            result.Append(@"""");
                        }

                        // Render link title
                        if (RenderLinkTitle)
                        {
                            result.Append(" title=\"" + HTMLHelper.HTMLEncode(itemName) + "\" ");
                        }

                        result.Append(">");


                        if (!WordWrap)
                        {
                            itemName = itemName.Replace(" ", "&nbsp;");
                        }

                        // get site map node name
                        result.Append(itemName);
                        result.Append("</a>");
                    }

                    // Ensures child nodes
                    result.Append(GetItems(Convert.ToInt32(dr["NodeID"])));
                    result.Append("</li>");
                }

                result.Append("</ul>");

                return result.ToString();
            }

            return "";
        }

        #endregion
    }
}