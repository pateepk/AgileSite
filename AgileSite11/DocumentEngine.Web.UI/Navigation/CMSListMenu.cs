using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base;
using CMS.Helpers;
using CMS.IO;
using CMS.MacroEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// CMSListmenu class.
    /// </summary>
    [DefaultProperty("Text"), ToolboxData("<{0}:CMSListMenu runat=server></{0}:CMSListMenu>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class CMSListMenu : CMSAbstractMenuProperties
    {
        #region "Variables"

        /// <summary>
        /// UL level indent.
        /// </summary>
        protected int mULlevelIndent;

        /// <summary>
        /// Rendered HTML code.
        /// </summary>
        private string mRenderedHTML = String.Empty;

        /// <summary>
        /// Default list menu css class.
        /// </summary>
        private const string defaultClassName = "CMSListMenu";

        /// <summary>
        /// Filter name.
        /// </summary>
        protected string mFilterName = null;

        /// <summary>
        /// Filter control.
        /// </summary>
        protected CMSAbstractBaseFilterControl mFilterControl = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether Rendered HTML is changed by manual interaction
        /// </summary>
        private bool IsChanged
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or set rendered HTML code.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Get or set rendered HTML code")]
        public string RenderedHTML
        {
            get
            {
                IsChanged = true;

                StringWriter sw = new StringWriter();

                HtmlTextWriter writer = new HtmlTextWriter(sw);
                RenderInternal(writer);

                mRenderedHTML = sw.ToString();

                return mRenderedHTML;
            }
            set
            {
                IsChanged = true;
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
        [Category("Behavior"), DefaultValue(""), Description("Specifies target frame for all URLs.")]
        public string UrlTarget
        {
            get
            {
                return ValidationHelper.GetString(ViewState["UrlTarget"], String.Empty);
            }
            set
            {
                ViewState["UrlTarget"] = value;
            }
        }


        /// <summary>
        /// Indicates if the highlighted item should be displayed as a link.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if the highlighted item should be displayed as a link.")]
        public bool DisplayHighlightedItemAsLink
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["DisplayHighlightedItemAsLink"], false);
            }
            set
            {
                ViewState["DisplayHighlightedItemAsLink"] = value;
            }
        }


        /// <summary>
        /// Specifies whether all submenus should be displayed or just submenu under highlighted (selected) item.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Specifies whether all submenus should be displayed or just submenu under highlighted (selected) item.")]
        public bool DisplayOnlySelectedPath
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["DisplayOnlySelectedPath"], false);
            }
            set
            {
                ViewState["DisplayOnlySelectedPath"] = value;
            }
        }


        /// <summary>
        /// Indicates if CSS classes should be rendered for every element. If set to false, only first and last item of menu level will use CSS class.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if CSS classes should be rendered for every element. If set to false, only first and last item of menu level will use CSS class.")]
        public bool RenderCssClasses
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["RenderCssClasses"], true);
            }
            set
            {
                ViewState["RenderCssClasses"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether menu style should be rendered for link item.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates whether menu style should be rendered for link item.")]
        public bool RenderStyleForItemLink
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["RenderStyleForItemLink"], true);
            }
            set
            {
                ViewState["RenderStyleForItemLink"] = value;
            }
        }


        /// <summary>
        /// Indicates if unique ID should be rendered for every menu item.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if unique ID should be rendered for every menu item.")]
        public bool RenderItemID
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["RenderItemID"], false);
            }
            set
            {
                ViewState["RenderItemID"] = value;
            }
        }


        /// <summary>
        /// Item ID  preffix..
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Item ID  preffix.")]
        public string ItemIdPrefix
        {
            get
            {
                return DataHelper.GetNotEmpty(ViewState["ItemIdPrefix"], ID);
            }
            set
            {
                ViewState["ItemIdPrefix"] = value;
            }
        }


        /// <summary>
        /// OnMouseOutScript script for menu items. You can use macro expressions here.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("OnMouseOutScript script for menu items.")]
        public string OnMouseOutScript
        {
            get
            {
                return ValidationHelper.GetString(ViewState["OnMouseOutScript"], String.Empty);
            }
            set
            {
                ViewState["OnMouseOutScript"] = value;
            }
        }


        /// <summary>
        /// OnMouseOver script for menu items. You can use macro expressions here.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("OnMouseOver script for menu items.")]
        public string OnMouseOverScript
        {
            get
            {
                return ValidationHelper.GetString(ViewState["OnMouseOverScript"], String.Empty);
            }
            set
            {
                ViewState["OnMouseOverScript"] = value;
            }
        }


        /// <summary>
        /// Specifies CSS class for the first item in every menu level.
        /// </summary>    
        [Category("Behavior"), DefaultValue(""), Description("Specifies CSS class for the first item in every menu level.")]
        public string FirstItemCssClass
        {
            get
            {
                return ValidationHelper.GetString(ViewState["FirstItemCssClass"], String.Empty);
            }
            set
            {
                ViewState["FirstItemCssClass"] = value;
            }
        }


        /// <summary>
        /// Specifies CSS class for the last item in every menu level.
        /// </summary>    
        [Category("Behavior"), DefaultValue(""), Description("Specifies CSS class for the last item in every menu level.")]
        public string LastItemCssClass
        {
            get
            {
                return ValidationHelper.GetString(ViewState["LastItemCssClass"], String.Empty);
            }
            set
            {
                ViewState["LastItemCssClass"] = value;
            }
        }


        /// <summary>
        /// Path of the item that will be highlighted like it was selected. The path type must be the same as PathType. If you omit this value, the control automatically uses the current alias path from the "aliaspath" querystring parameter.
        /// </summary>
        [Category("Behavior"), Description("Path of the item that will be highlighted like it was selected. The path type must be the same as PathType. If you omit this value, the control automatically uses the current alias path from the aliaspath querystring parameter.")]
        public string HighlightedNodePath
        {
            get
            {
                if ((ViewState["HighlightedNodePath"] == null))
                {
                    if (!String.IsNullOrEmpty(DocumentContext.OriginalAliasPath))
                    {
                        ViewState["HighlightedNodePath"] = DocumentContext.OriginalAliasPath;
                    }
                    else
                    {
                        ViewState["HighlightedNodePath"] = QueryHelper.GetString("redirectto", String.Empty);
                    }
                }
                return ValidationHelper.GetString(ViewState["HighlightedNodePath"], String.Empty).Trim();
            }
            set
            {
                ViewState["HighlightedNodePath"] = value;
            }
        }


        /// <summary>
        /// Hover CSS class name.
        /// </summary>
        [Category("Behavior"), Description("Hover CSS class name.")]
        public string HoverCSSClassName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["HoverCssClassName"], String.Empty);
            }
            set
            {
                ViewState["HoverCssClassName"] = value;
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
        /// Render the image alt attribute?
        /// </summary>
        [Category("Behavior"), Description("Render the alt attribute within the image.")]
        public bool RenderImageAlt
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["RenderImageAlt"], true);
            }
            set
            {
                ViewState["RenderImageAlt"] = value;
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

        #endregion //public properties


        #region "Methods"

        /// <summary>
        /// Ensures default data binding 
        /// </summary>
        /// <param name="loadPhase">Indicates whether Init is call from Load event</param>
        protected override void InitControl(bool loadPhase)
        {
            if ((LoadDataAutomaticaly) && (!StopProcessing))
            {
                if (FilterControl != null)
                {
                    FilterControl.OnFilterChanged += FilterControl_OnFilterChanged;
                }
                base.InitControl(loadPhase);
            }

            // Do not call base method for LoadDataAutomatically = false
            // base.InitControl(loadPhase);
        }


        /// <summary>
        /// Reload data.
        /// </summary>
        /// <param name="forceLoad">Indicate if data will be loaded whatever datasource is not empty</param>
        public override void ReloadData(bool forceLoad)
        {
            SetContext();

            // Clear rendered HTML code
            mRenderedHTML = String.Empty;
            IsChanged = false;

            // Do not load data if is stop processing set
            if ((StopProcessing) || (Context == null))
            {
                return;
            }

            EnableViewState = false;

            // Load DataSource
            if ((DataSource == null) || (forceLoad))
            {
                if (FilterControl != null)
                {
                    FilterControl.InitDataProperties(this);
                }
                DataSource = GetDataSource();
            }

            if (DataHelper.DataSourceIsEmpty(DataSource))
            {
                return;
            }

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
                    if (!HideControlForZeroRows && !String.IsNullOrEmpty(ZeroRowsText))
                    {
                        // Display no records found text
                        output.Write(ZeroRowsText);
                    }

                    // There is nothing to render
                    return;
                }

                if (!IsChanged)
                {
                    RenderInternal(output);
                }
                else
                {
                    output.Write(mRenderedHTML);
                }
            }
        }


        /// <summary>
        /// Renders CMS list menu to the HTMLTextWriter
        /// </summary>
        /// <param name="writer">HTMLTextWriter</param>
        private void RenderInternal(HtmlTextWriter writer)
        {
            // Encapsulate by new line
            writer.WriteLine();
            writer.Indent++;

            // Use "div" envelope is HoverCSSClassName is selected
            if (!String.IsNullOrEmpty(HoverCSSClassName))
            {
                writer.Indent++;
                writer.AddAttribute(HtmlTextWriterAttribute.Class, HoverCSSClassName);
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.WriteLine();
            }

            if (!DataHelper.DataSourceIsEmpty(DataSource))
            {
                // Get top node level
                int topNodeLevel = ValidationHelper.GetInteger(DataSource.Tables[0].DefaultView[0]["NodeLevel"], 0);
                List<int> displayedParentNodeIDs = new List<int>();

                // Check all  items
                foreach (DataRowView dr in DataSource.Tables[0].DefaultView)
                {
                    if (ValidationHelper.GetInteger(dr["NodeLevel"], 0) == topNodeLevel)
                    {
                        // if NodeParentID isn't in displayedParentNodeIDs then GetItems
                        int nodeParentID = ValidationHelper.GetInteger(dr["NodeParentID"], 0);
                        if (!displayedParentNodeIDs.Contains(nodeParentID))
                        {
                            RenderItemInternal(writer, nodeParentID, 0);
                            displayedParentNodeIDs.Add(nodeParentID);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (!String.IsNullOrEmpty(HoverCSSClassName))
            {
                writer.RenderEndTag();
                writer.Indent--;
            }

            // Encapsulate by new line
            writer.WriteLine();
            writer.Indent--;
        }


        /// <summary>
        /// Gets menu value from datarow view with dependence on highlighted status
        /// </summary>
        /// <param name="dr">Data row view</param>
        /// <param name="columnName">Column name</param>
        /// <param name="highlightedColumnName">Highlighted column name</param>
        /// <param name="isHighlighted">Indicates whether highlighted is preferred</param>
        private string GetMenuValue(DataRowView dr, string columnName, string highlightedColumnName, bool isHighlighted)
        {
            return ValidationHelper.GetString(dr[isHighlighted ? highlightedColumnName : columnName], String.Empty);
        }


        /// <summary>
        /// Generate image HTML
        /// </summary>
        /// <param name="writer">HTML writer</param>
        /// <param name="itemName">Item name</param>
        /// <param name="dr">Datarow view</param>
        /// <param name="columnName">Image column name</param>
        /// <param name="highlightedColumnName">Highlighted image column name</param>
        /// <param name="isHighlighted">Indicates whether highlighted is preferred</param>
        private bool GenerateImage(HtmlTextWriter writer, string itemName, DataRowView dr, string columnName, string highlightedColumnName, bool isHighlighted)
        {
            string url = GetMenuValue(dr, columnName, highlightedColumnName, UseItemImagesForHighlightedItem | isHighlighted);
            if (!String.IsNullOrEmpty(url))
            {
                // src
                writer.AddAttribute(HtmlTextWriterAttribute.Src, ResolveUrl(url));
                // border
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                // alt
                if (RenderImageAlt)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, itemName, true);
                }
                // img tag
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.WriteLine();

                return true;
            }

            return false;
        }


        /// <summary>
        /// Render current level items
        /// </summary>
        /// <param name="writer">HtmlTextWriter</param>
        /// <param name="nodeParentID">Parent node id</param>
        /// <param name="nodeLevel">Node level</param>
        private void RenderItemInternal(HtmlTextWriter writer, int nodeParentID, int nodeLevel)
        {
            // Get items
            var items = GroupedDS.GetGroupView(nodeParentID);
            if (items == null)
            {
                return;
            }

            // Current level css prefix
            string cssPrefix = null;

            #region "<ul>"

            // UL - ID - first level is always Me.ID, others Me.ID_UL_indentLevel
            if (nodeLevel == 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, ItemIdPrefix);
            }
            else if (RenderItemID)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, ItemIdPrefix + "_UL_" + nodeLevel);
            }

            // UL - Class
            if (RenderCssClasses)
            {
                cssPrefix = GetCSSPrefix(nodeLevel);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, cssPrefix + defaultClassName + "UL");
            }

            // <ul>
            writer.RenderBeginTag(HtmlTextWriterTag.Ul);

            #endregion

            int rowIndex = 0;
            var localResolver = ContextResolver.CreateChild();

            // Create level items
            foreach (DataRowView dr in items)
            {
                // Add current DataRow to the resolver
                localResolver.SetAnonymousSourceData(dr.Row);

                #region "Identification of highlighted item"

                // Identify highlighted item
                bool isHighlighted = false;

                // Indicates whether current item is in path of selected element
                bool isInPath;

                // Highlighted path (selected by user or current path)
                string highlightedPath = MacroResolver.ResolveCurrentPath(HighlightedNodePath);
                // Current item path
                string currentItemPath = ValidationHelper.GetString(dr["NodeAliasPath"], String.Empty);

                // Path are equal
                if (highlightedPath.EqualsCSafe(currentItemPath, true))
                {
                    isHighlighted = true;
                    isInPath = true;
                }
                // Current item path is in current path
                else
                {
                    isInPath = highlightedPath.StartsWithCSafe(currentItemPath + "/", true);

                    if (HighlightAllItemsInPath)
                    {
                        isHighlighted = isInPath;
                    }
                }

                #endregion

                // Prepare the item name. Disable encoding. Encoding depends on "EncodeMenuCaption" property
                localResolver.Settings.EncodeResolvedValues = false;
                var caption = localResolver.ResolveMacros(TreePathUtils.GetMenuCaption(ValidationHelper.GetString(dr["DocumentMenuCaption"], String.Empty), ValidationHelper.GetString(dr["DocumentName"], String.Empty)));
                localResolver.Settings.EncodeResolvedValues = true;

                #region "<li>"

                // LI style attribute
                AddStyle(writer, dr, isHighlighted);

                // OnMouseOut attribute
                if (!String.IsNullOrEmpty(OnMouseOutScript))
                {
                    writer.AddAttribute("OnMouseOut", localResolver.ResolveMacros(OnMouseOutScript));
                }

                // OnMouseOver attribute
                if (!String.IsNullOrEmpty(OnMouseOverScript))
                {
                    writer.AddAttribute("OnMouseOver", localResolver.ResolveMacros(OnMouseOverScript));
                }

                // create LI IDs ... format Me.ID_LI_indentLevel_(rowIndex + 1)
                if (RenderItemID)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, ItemIdPrefix + "_LI_" + nodeLevel + "_" + (rowIndex + 1) + "_" + nodeParentID);
                }


                bool applyMenuDesign = false;
                string menuClass = null;
                if (ApplyMenuDesign)
                {
                    if (isHighlighted)
                    {
                        menuClass = ValidationHelper.GetString(dr["DocumentMenuClassHighlighted"], String.Empty);
                        if (!String.IsNullOrEmpty(menuClass))
                        {
                            applyMenuDesign = true;
                        }
                    }
                    else
                    {
                        menuClass = ValidationHelper.GetString(dr["DocumentMenuClass"], String.Empty);
                        if (!String.IsNullOrEmpty(menuClass))
                        {
                            applyMenuDesign = true;
                        }
                    }
                }

                if (applyMenuDesign)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, menuClass);
                }
                else
                {
                    bool renderAsFirstItemClass = (rowIndex == 0) && !String.IsNullOrEmpty(FirstItemCssClass);
                    bool renderAsLastItemClass = (rowIndex == (items.Count - 1)) && !String.IsNullOrEmpty(LastItemCssClass);
                    bool alternativeRow = ((rowIndex % 2) == 1);

                    var className = GenerateCssClassForLi(cssPrefix, isHighlighted, renderAsFirstItemClass, renderAsLastItemClass, alternativeRow);
                    if (!string.IsNullOrEmpty(className))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
                    }
                }

                // <li> tag
                writer.RenderBeginTag(HtmlTextWriterTag.Li);
                writer.WriteLine();
                writer.Indent++;

                #endregion


                #region "<img> left image"

                // Left image
                if (ApplyMenuDesign)
                {
                    GenerateImage(writer, caption, dr, "DocumentMenuItemLeftImage", "DocumentMenuItemLeftImageHighlighted", isHighlighted);
                }

                #endregion


                #region "Main item"

                // Item style link
                if (RenderStyleForItemLink)
                {
                    AddStyle(writer, dr, isHighlighted);
                }

                // Item title
                if (RenderLinkTitle)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, caption);
                }

                // Item css class
                if (RenderCssClasses)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, isHighlighted ? cssPrefix + defaultClassName + "Link" + "Highlighted" : cssPrefix + defaultClassName + "Link");
                }

                // Render SPAN (for inactive item)
                if ((isHighlighted && !DisplayHighlightedItemAsLink) || ValidationHelper.GetBoolean(dr["DocumentMenuItemInactive"], false))
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                }
                //  or A tag
                else
                {
                    // target
                    if (!String.IsNullOrEmpty(UrlTarget))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Target, UrlTarget);
                    }

                    // Current item JavaScript
                    string javaScript = ValidationHelper.GetString(dr["DocumentMenuJavascript"], String.Empty);
                    if (!String.IsNullOrEmpty(javaScript))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Onclick, localResolver.ResolveMacros(javaScript));
                    }

                    // href
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, HttpUtility.UrlPathEncode(DocumentURLProvider.GetNavigationUrl(new DataRowContainer(dr), localResolver)));
                    // <a> tag
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                }

                #region "<img> main image"

                bool isImageGenerated = false;
                // Left image
                if (ApplyMenuDesign)
                {
                    isImageGenerated = GenerateImage(writer, caption, dr, "DocumentMenuItemImage", "DocumentMenuItemImageHighlighted", isHighlighted);
                }

                #endregion


                #region "Item text"

                // Item text (only if main image is not used)
                if (!isImageGenerated)
                {
                    // Encode menu caption if it is required
                    var itemName = EncodeMenuCaption ? HTMLHelper.HTMLEncode(caption) : caption;

                    writer.Write(WordWrap ? itemName : itemName.Replace(" ", "&nbsp;"));
                }

                #endregion

                writer.RenderEndTag();
                writer.WriteLine();

                #endregion


                #region "<img> right image"

                // Left image
                if (ApplyMenuDesign)
                {
                    GenerateImage(writer, caption, dr, "DocumentMenuItemRightImage", "DocumentMenuItemRightImageHighlighted", isHighlighted);
                }

                #endregion


                #region "Sub-Items"

                // if DisplayOnlySelectedPath is set true, then only submenu under highlighted item could be render, otherwise all submenus
                // implication: A => B  <=>  !A v B
                if (!DisplayOnlySelectedPath || isHighlighted || isInPath)
                {
                    int nodeId = Convert.ToInt32(dr["NodeID"]);
                    var childItems = GroupedDS.GetGroupView(nodeId);

                    // if there is any child, create a UL (submenu)
                    if (!DataHelper.DataSourceIsEmpty(childItems))
                    {
                        // SubMenuIndicator image
                        if (!String.IsNullOrEmpty(SubmenuIndicator))
                        {
                            // Alt
                            writer.AddAttribute(HtmlTextWriterAttribute.Alt, caption, true);
                            // border
                            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                            // src
                            writer.AddAttribute(HtmlTextWriterAttribute.Src, ResolveUrl(SubmenuIndicator));
                            // <img tag>
                            writer.RenderBeginTag(HtmlTextWriterTag.Img);
                            writer.RenderEndTag();
                        }

                        RenderItemInternal(writer, nodeId, nodeLevel + 1);
                    }
                }

                #endregion


                #region "</li>"

                writer.Indent--;
                writer.RenderEndTag();
                writer.WriteLine();

                #endregion

                // Increment row index
                rowIndex++;
            }

            #region "</ul>"

            // </ul>
            writer.RenderEndTag();
            writer.WriteLine();

            #endregion
        }


        /// <summary>
        /// Generates the CSS class for LI tag.
        /// </summary>
        /// <param name="cssPrefix">The prefix for CSS class.</param>
        /// <param name="isHighlighted">Determines whether should be LI item highlighted.</param>
        /// <param name="renderAsFirstItemClass">Determines whether should be LI item rendered as first item.</param>
        /// <param name="renderAsLastItemClass">Determines whether should be LI item rendered as last item.</param>
        /// <param name="alternativeRow">Determines whether should be LI item rendered as alternative row.</param>
        private string GenerateCssClassForLi(string cssPrefix, bool isHighlighted, bool renderAsFirstItemClass, bool renderAsLastItemClass, bool alternativeRow)
        {
            var classNamePrefix = new StringBuilder();

            if (RenderCssClasses)
            {
                classNamePrefix.Append(cssPrefix);
                classNamePrefix.Append(defaultClassName);
            }

            if (isHighlighted)
            {
                classNamePrefix.Append("Highlighted");
            }

            if (RenderCssClasses)
            {
                classNamePrefix.Append("LI");
            }

            string classNamePostfix = (UseAlternatingStyles && alternativeRow) ? "Alt" : string.Empty;

            if (renderAsFirstItemClass && renderAsLastItemClass)
            {
                // Insert both (first, last) classes when only one item is created                
                var prefix = classNamePrefix.ToString();
                return string.Format("{0}{1}{3} {0}{2}{3}", prefix, FirstItemCssClass, LastItemCssClass, classNamePostfix);
            }

            if (renderAsFirstItemClass)
            {
                return string.Format("{0}{1}{2}", classNamePrefix, FirstItemCssClass, classNamePostfix);
            }

            if (renderAsLastItemClass)
            {
                return string.Format("{0}{1}{2}", classNamePrefix, LastItemCssClass, classNamePostfix);
            }

            return string.Format("{0}{1}", classNamePrefix, classNamePostfix);
        }


        /// <summary>
        /// Add style from datarow.
        /// </summary>
        private void AddStyle(HtmlTextWriter writer, DataRowView dataRow, bool highlighted)
        {
            if (ApplyMenuDesign)
            {
                if (highlighted)
                {
                    string style = DataHelper.GetStringValue(dataRow.Row, "DocumentMenuStyleHighlighted");
                    if (!String.IsNullOrEmpty(style))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Style, style);
                    }
                }
                else
                {
                    string style = DataHelper.GetStringValue(dataRow.Row, "DocumentMenuStyle");
                    if (!String.IsNullOrEmpty(style))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Style, style);
                    }
                }
            }
        }


        /// <summary>
        /// Data filter control handler.
        /// </summary>
        private void FilterControl_OnFilterChanged()
        {
            ReloadData(true);
        }

        #endregion
    }
}