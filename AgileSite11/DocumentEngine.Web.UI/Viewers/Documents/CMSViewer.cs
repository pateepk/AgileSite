using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.PortalEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Control for displaying CMS content transformed using XSLT.
    /// </summary>
    [ToolboxData("<{0}:CMSViewer runat=server></{0}:CMSViewer>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class CMSViewer : CMSAbstractDataProperties, INamingContainer
    {
        #region "Variables"

        private bool formatBuilt = false;

        /// <summary>
        /// When DataSource is empty NoData  = true.
        /// </summary>
        protected bool mNoData = false;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the value that indicates whether dynamic controls should be resolved
        /// </summary>
        public bool ResolveDynamicControls
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ResolveDynamicControls"], true);
            }
            set
            {
                ViewState["ResolveDynamicControls"] = value;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether the control contains any data.
        /// </summary>
        public bool NoData
        {
            get
            {
                return mNoData;
            }
        }


        /// <summary>
        /// Hides the control when no data is loaded. The default value is False.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Hides the control when no data loaded. Default value is False.")]
        public virtual bool HideControlForZeroRows
        {
            get
            {
                if ((Convert.ToString(ViewState["HideControlForZeroRows"]) == null) || (Convert.ToString(ViewState["HideControlForZeroRows"]) == ""))
                {
                    return false;
                }
                else
                {
                    return Convert.ToBoolean(ViewState["HideControlForZeroRows"]);
                }
            }
            set
            {
                ViewState["HideControlForZeroRows"] = value;
            }
        }


        /// <summary>
        /// Text to be shown when the control doesn't display any data.
        /// </summary>        
        [Category("Behavior"), DefaultValue(""), Description("Text to be shown when the control doesn't display any data.")]
        public virtual string ZeroRowsText
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ZeroRowsText"], "");
            }
            set
            {
                ViewState["ZeroRowsText"] = value;
            }
        }


        /// <summary>
        /// Transformation name in format application.class.transformation.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name in format application.class.transformation.")]
        public string TransformationName
        {
            get
            {
                if (Convert.ToString(ViewState["TransformationName"]) == "")
                {
                    ViewState["TransformationName"] = "";
                }
                return Convert.ToString(ViewState["TransformationName"]);
            }
            set
            {
                ViewState["TransformationName"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the columns to be retrieved from database.
        /// </summary>    
        public string Columns
        {
            get
            {
                return SelectedColumns;
            }
            set
            {
                SelectedColumns = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            EnableViewState = false;

            if (Context == null)
            {
                output.Write(" [ CMSViewer : " + ID + " ]");
                return;
            }
            else
            {
                if ((NoData) && (HideControlForZeroRows))
                {
                    return;
                }

                if (NoData)
                {
                    output.Write(ZeroRowsText);
                    return;
                }
            }

            base.Render(output);
        }


        /// <summary>
        /// Reload control data.
        /// </summary>
        /// <param name="forceLoad">Indicates force load</param>
        public override void ReloadData(bool forceLoad)
        {
            if ((!mDataLoaded) || (forceLoad))
            {
                BuildFormat();
            }
        }


        /// <summary>
        /// Build format.
        /// </summary>
        public void BuildFormat()
        {
            if (!formatBuilt)
            {
                formatBuilt = true;

                // Get the content
                string content = GetContent();

                if (!String.IsNullOrEmpty(content))
                {
                    // Resolve dynamic controls
                    Controls.Add(new LiteralControl(content));
                    if (ResolveDynamicControls)
                    {
                        ControlsHelper.ResolveDynamicControls(this);
                    }
                }
                else
                {
                    mNoData = true;
                }
            }
        }


        /// <summary>
        /// Returns content according to the current settings.
        /// </summary>
        private string GetContent()
        {
            if (Context == null)
            {
                return "";
            }

            SetContext();

            // Prepare the content provider
            ContentProvider contentProv = new ContentProvider();
            contentProv.TreeProvider = TreeProvider;
            string result = null;

            // Check if the item is selected item            
            if (!String.IsNullOrEmpty(SelectedItemTransformationName) && !String.IsNullOrEmpty(DocumentContext.OriginalAliasPath))
            {
                // Get the document path
                string nodesPath = Path.TrimEnd('%', '/');
                if (String.IsNullOrEmpty(nodesPath))
                {
                    nodesPath = DocumentContext.OriginalAliasPath;
                }

                // Get the document if path set
                if (!String.IsNullOrEmpty(nodesPath))
                {
                    TreeNode currentDocument = GetDocument(nodesPath);
                    if (currentDocument != null)
                    {
                        // Use the selected transformation for non-menu item documents
                        if (!TreePathUtils.IsMenuItemType(currentDocument.NodeClassName))
                        {
                            TransformationName = SelectedItemTransformationName;
                            Path = nodesPath;
                        }
                    }
                }
            }

            // Load the data
            string path = MacroResolver.ResolveCurrentPath(Path);

            DataSet ds = GetDataSet(path);
            DataSource = ds;

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Process the XSLT transformation
                result = contentProv.Transform(ds.GetXml(), TransformationName);
                result = result.Replace(@" src=""~/", @" src=""" + ResolveUrl("~/"));
                result = result.Replace(@" SRC=""~/", @" SRC=""" + ResolveUrl("~/"));
                result = result.Replace(@" href=""~/", @" href=""" + ResolveUrl("~/"));
                result = result.Replace(@" HREF=""~/", @" HREF=""" + ResolveUrl("~/"));
            }

            ReleaseContext();

            return result;
        }


        /// <summary>
        /// Overrides the generation of the SPAN tag with custom tag.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (SettingsKeyInfoProvider.GetValue(SiteName + ".CMSControlElement").ToLowerCSafe().Trim() == "div")
                {
                    return HtmlTextWriterTag.Div;
                }
                else
                {
                    return HtmlTextWriterTag.Span;
                }
            }
        }


        /// <summary>
        /// OnLoad override, ensure child controls if it wasn't ensured regularly.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnLoad(EventArgs e)
        {
            EnsureChildControls();

            base.OnLoad(e);
        }


        /// <summary>
        /// CreateChildControls() override.
        /// </summary>
        protected override void CreateChildControls()
        {
            if (Context == null)
            {
                return;
            }

            if (!StopProcessing)
            {
                // Register edit mode buttons script
                if ((PortalContext.ViewMode != ViewModeEnum.LiveSite) && (PortalContext.ViewMode != ViewModeEnum.EditLive))
                {
                    ScriptHelper.RegisterClientScriptBlock(this, typeof(string), ScriptHelper.EDIT_DOCUMENT_SCRIPT_KEY, CMSControlsHelper.EditDocumentScript);
                }

                ReloadData(false);
            }

            base.CreateChildControls();
        }

        #endregion
    }
}
