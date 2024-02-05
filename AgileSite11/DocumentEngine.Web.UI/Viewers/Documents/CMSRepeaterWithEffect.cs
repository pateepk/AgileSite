using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Repeater with effect control that can be bounded to the CMS content.
    /// </summary>
    [ToolboxData("<{0}:CMSRepeaterWithEffect runat=server></{0}:CMSRepeaterWithEffect>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    [ParseChildren(true)]
    [PersistChildren(true)]
    public class CMSRepeaterWithEffect : CMSRepeater, INamingContainer
    {
        #region "Variables"

        private bool renderEnvelope = true;

        #endregion 


        #region "Effect properties"

        /// <summary>
        /// Gets or sets the html code which is inserted before the generated items.
        /// </summary>
        [Category("Layout"), DefaultValue(""), Description("HTML code inserted before the generated items.")]
        public string RepeaterHTMLBefore
        {
            get
            {
                return ValidationHelper.GetString(ViewState["RepeaterHTMLBefore"], string.Empty);
            }
            set
            {
                ViewState["RepeaterHTMLBefore"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the html code which is inserted after the generated items.
        /// </summary>
        [Category("Layout"), DefaultValue(""), Description("HTML code inserted after the generated items.")]
        public string RepeaterHTMLAfter
        {
            get
            {
                return ValidationHelper.GetString(ViewState["RepeaterHTMLAfter"], string.Empty);
            }
            set
            {
                ViewState["RepeaterHTMLAfter"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the html code which is inserted before each of the item.
        /// </summary>
        [Category("Layout"), DefaultValue(""), Description("HTML code inserted before each of the item.")]
        public string ItemHTMLBefore
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ItemHTMLBefore"], string.Empty);
            }
            set
            {
                ViewState["ItemHTMLBefore"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the html code which is inserted after each of the item.
        /// </summary>
        [Category("Layout"), DefaultValue(""), Description("HTML code inserted after each of the item.")]
        public string ItemHTMLAfter
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ItemHTMLAfter"], string.Empty);
            }
            set
            {
                ViewState["ItemHTMLAfter"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the item separator between displayed records.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Item separator between displayed records.")]
        public string ItemHTMLSeparator
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ItemHTMLSeparator"], string.Empty);
            }
            set
            {
                ViewState["ItemHTMLSeparator"] = value;
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether to hide layout (Content before, Content after) when no data found.
        /// </summary>
        public bool HideLayoutForZeroRows
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["HideLayoutForZeroRows"], false);
            }
            set
            {
                ViewState["HideLayoutForZeroRows"] = value;
            }
        }


        /// <summary>
        /// Gets or sets a list of additional script files which should  be included to the page. One line per file.
        /// </summary>
        [Category("Effect settings"), DefaultValue(""), Description("List of additional script files to include to the page. One line per file.")]
        public string ScriptFiles
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ScriptFiles"], string.Empty);
            }
            set
            {
                ViewState["ScriptFiles"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the script that initializes the effect applied on the items.
        /// </summary>
        [Category("Effect settings"), DefaultValue(""), Description("Script that initializes the effect applied on the items.")]
        public string InitScript
        {
            get
            {
                return ValidationHelper.GetString(ViewState["InitScript"], string.Empty);
            }
            set
            {
                ViewState["InitScript"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the additional CSS files that should be linked to the page.
        /// </summary>
        [Category("Styles"), DefaultValue(""), Description("Additional CSS files that should be linked to the page.")]
        public string CSSFiles
        {
            get
            {
                return ValidationHelper.GetString(ViewState["CSSFiles"], string.Empty);
            }
            set
            {
                ViewState["CSSFiles"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the additional inline CSS styles that will be placed into the page header.
        /// </summary>
        [Category("Styles"), DefaultValue(""), Description("Additional inline CSS styles that will be placed into the page header.")]
        public string InlineCSS
        {
            get
            {
                return ValidationHelper.GetString(ViewState["InlineCSS"], string.Empty);
            }
            set
            {
                ViewState["InlineCSS"] = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSRepeaterWithEffect()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="treeProvider">Tree provider instance to be used for retrieving data</param>
        public CMSRepeaterWithEffect(TreeProvider treeProvider)
            : base(treeProvider)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (Context == null)
            {
                return;
            }

            if (!StopProcessing)
            {
                // Register scripts
                string resolvedScriptFiles = MacroResolver.Resolve(ScriptFiles);
                string[] scripts = resolvedScriptFiles.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string script in scripts)
                {
                    // Register the script file
                    string sfile = script.Trim();
                    if (!String.IsNullOrEmpty(sfile))
                    {
                        ScriptHelper.RegisterScriptFile(Page, sfile);
                    }
                }

                // Add init script
                string resolvedInitScript = MacroResolver.Resolve(InitScript);
                if (!string.IsNullOrEmpty(resolvedInitScript))
                {
                    ScriptHelper.RegisterStartupScript(this, typeof(string), ClientID + "_Init", ScriptHelper.GetScript(resolvedInitScript));
                }

                // Register CSS files
                string resolvedCSSFiles = MacroResolver.Resolve(CSSFiles);
                string[] cssFiles = resolvedCSSFiles.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string cssFile in cssFiles)
                {
                    CssRegistration.RegisterCssLink(Page, cssFile.Trim());
                }

                // Add inline CSS
                string resolvedInlineCSS = MacroResolver.Resolve(InlineCSS);
                if (!string.IsNullOrEmpty(resolvedInlineCSS))
                {
                    // Add css to page header
                    CssRegistration.RegisterCssBlock(Page, "repeaterWithEffectInlineCss_" + ClientID, resolvedInlineCSS);
                }
            }

            base.OnLoad(e);

            // Load the data and set the item separator
            ReloadData(false);

            // Apply the effect only if the item is not selected (or SelectedItem transformation is not set)
            renderEnvelope = !IsSelected || string.IsNullOrEmpty(SelectedItemTransformationName);
        }


        /// <summary>
        /// Loads data from the according to the current values of properties.
        /// </summary>
        public override void ReloadData(bool forceReload)
        {
            if (!StopProcessing)
            {
                // If already loaded, exit
                if (!mDataLoaded || forceReload)
                {
                    // Load Separator text
                    if (!String.IsNullOrEmpty(ItemHTMLSeparator))
                    {
                        SeparatorTemplate = new GeneralTemplateClass(ItemHTMLSeparator);
                    }
                }

                base.ReloadData(forceReload);
            }
        }


        /// <summary>
        /// Raises the <see cref="E:Repeater.ItemDataBound"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:RepeaterItemEventArgs"/> object that contains the event data</param>
        protected override void OnItemDataBound(RepeaterItemEventArgs e)
        {
            // Apply the effect only if the item is not selected
            if (renderEnvelope)
            {
                if ((e.Item.ItemType == ListItemType.Item)
                    || (e.Item.ItemType == ListItemType.AlternatingItem)
                    || (e.Item.ItemType == ListItemType.EditItem)
                    || (e.Item.ItemType == ListItemType.SelectedItem))
                {
                    // Insert a layout before the item
                    if (!string.IsNullOrEmpty(ItemHTMLBefore))
                    {
                        LiteralControl ltrBefore = new LiteralControl(ItemHTMLBefore);
                        e.Item.Controls.AddAt(0, ltrBefore);
                    }

                    // Add a layout after the item
                    if (!string.IsNullOrEmpty(ItemHTMLAfter))
                    {
                        LiteralControl ltrAfter = new LiteralControl(ItemHTMLAfter);
                        e.Item.Controls.Add(ltrAfter);
                    }
                }
            }

            base.OnItemDataBound(e);
        }


        /// <summary>
        /// Render event handler.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (Context == null)
            {
                output.Write("[CMSRepeaterWithEffect: " + ClientID + " ]");
                return;
            }

            if (!StopProcessing)
            {
                // Apply the effect only if the item is not selected
                if (renderEnvelope && (!HideLayoutForZeroRows || !NoData))
                {
                    output.Write(RepeaterHTMLBefore);
                }

                base.Render(output);

                // Apply the effect only if the item is not selected
                if (renderEnvelope && (!HideLayoutForZeroRows || !NoData))
                {
                    output.Write(RepeaterHTMLAfter);
                }
            }
        }

        #endregion
    }
}
