using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Abstract class for the editable controls (controls that needs to store the content within the document content).
    /// </summary>
    public abstract class CMSAbstractEditableControl : WebControl, ICMSEditableControl
    {
        #region "Variables"

        // Storage key used for editable control.
        private const string EDITABLE_CONTROL_STORAGE_KEY = "EditableControl";

        /// Parent page manager.
        private IPageManager mPageManager;

        private bool? mCheckPermissions;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the source page info
        /// </summary>
        protected PageInfo SourcePageInfo
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets or sets the value that indicates whether the permissions are checked.
        /// </summary>
        public bool CheckPermissions
        {
            get
            {
                if (mCheckPermissions == null)
                {
                    mCheckPermissions = PageManager.CheckPermissions;
                }
                return mCheckPermissions.Value;
            }
            set
            {
                mCheckPermissions = value;
            }
        }


        /// <summary>
        /// Page manager control.
        /// </summary>
        public virtual IPageManager PageManager
        {
            get
            {
                if (mPageManager == null)
                {
                    mPageManager = PortalContext.CurrentPageManager;
                }
                return mPageManager;
            }
            set
            {
                mPageManager = value;
            }
        }


        /// <summary>
        /// Control view mode.
        /// </summary>
        public virtual ViewModeEnum ViewMode
        {
            get
            {
                if (ViewState["ViewMode"] != null)
                {
                    return (ViewModeEnum)ViewState["ViewMode"];
                }
                else
                {
                    return PageManager.ViewMode;
                }
            }
            set
            {
                ViewState["ViewMode"] = value;
            }
        }


        /// <summary>
        /// If set, the region uses the content inheritance.
        /// If the region content of the current document is empty then the content of the first non-empty parent is used.
        /// Note: The editable region does not combine content of the current document with the content of the parent documents.
        /// </summary>
        public virtual bool InheritContent
        {
            get;
            set;
        }


        /// <summary>
        /// If set, only published content is displayed on a live site.
        /// </summary>
        public virtual bool SelectOnlyPublished
        {
            get;
            set;
        }


        /// <summary>
        /// If set, the instance guid is used as a key for the editable control, making the content available on other documents which use the same page template with this control.
        /// </summary>
        public virtual string InstanceGUID
        {
            get;
            set;
        }


        /// <summary>
        /// Edit page url which will be used for editing of the editable control. Used in On-site editing.
        /// </summary>
        protected virtual string EditPageUrl
        {
            get
            {
                return String.Empty;
            }
        }


        /// <summary>
        /// Edit dialog width which will be opened for editing of the editable control. Used in On-site editing.
        /// </summary>
        protected virtual string EditDialogWidth
        {
            get
            {
                return String.Empty;
            }
        }


        /// <summary>
        /// Indicates whether the control contains any editable content
        /// </summary>
        protected virtual bool EmptyContent
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        protected CMSAbstractEditableControl()
        {
            PortalHelper.RegisterEditableControl(this);
        }


        /// <summary>
        /// Loads the control content.
        /// </summary>
        /// <param name="content">Dynamic content to load to the web part</param>
        /// <param name="forceReload">If true, the content is forced to reload</param>
        public virtual void LoadContent(string content, bool forceReload = false)
        {
            throw new Exception("[CMSAbstractEditableControl.LoadContent]: This method must be overridden by the inherited class.");
        }


        /// <summary>
        /// Load the content to the region, applies the InheritContent.
        /// </summary>
        /// <param name="pageInfo">PageInfo with the content data</param>
        /// <param name="forceReload">If true, the content is forced to reload</param>
        public virtual void LoadContent(PageInfo pageInfo, bool forceReload = false)
        {
            if (pageInfo == null)
            {
                return;
            }

            string id = GetContentID();

            // Get the content
            string content = pageInfo.EditableRegions[id];


            // Get the content
            SourcePageInfo = pageInfo;

            // Mark that the editable content is empty
            EmptyContent = (content == null) || string.IsNullOrEmpty(content.Trim());

            // If content not found and inheritance allowed, try to load from parent
            if (EmptyContent && InheritContent && !pageInfo.IsRoot() && !ViewMode.IsOneOf(ViewModeEnum.Edit, ViewModeEnum.EditDisabled))
            {
                PageInfo parentInfo = pageInfo.ParentPageInfo;

                if (parentInfo != null)
                {
                    LoadContent(parentInfo, forceReload);
                    return;
                }
            }

            LoadContent(content, forceReload);
        }


        /// <summary>
        /// Returns the current web part content.
        /// </summary>
        public virtual string GetContent()
        {
            throw new Exception("[CMSAbstractEditableControl.GetContent]: This method must be overridden by the inherited class.");
        }


        /// <summary>
        /// Saves the control content.
        /// </summary>
        public virtual void SaveContent(PageInfo pageInfo)
        {
            if (pageInfo == null)
            {
                return;
            }

            // Get the content
            string content = GetContent();

            // If content == null, not allowed to change
            if (content != null)
            {
                // Save the value
                string id = GetContentID();
                pageInfo.EditableRegions[id] = content;
            }
        }


        /// <summary>
        /// Returns the list of the field IDs (Client IDs of the inner controls) that should be spell checked.
        /// </summary>
        public virtual List<string> GetSpellCheckFields()
        {
            return null;
        }


        /// <summary>
        /// Returns true if entered data is valid. If data is invalid, it returns false and displays an error message.
        /// </summary>
        public virtual bool IsValid()
        {
            return true;
        }


        /// <summary>
        /// Gets the custom dialog parameters used in the On-site editing when opening the modal edit dialog.
        /// The url parameters are in the following format: "name=value"
        /// </summary>
        public virtual string[] GetEditDialogParameters()
        {
            return null;
        }


        /// <summary>
        /// Returns content identification for storage
        /// </summary>
        protected virtual string GetContentID()
        {
            if (InstanceGUID != null)
            {
                return ID.ToLowerCSafe() + ";" + InstanceGUID.ToLowerCSafe();
            }
            return ID.ToLowerCSafe();
        }


        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            ViewModeEnum vm = PortalContext.ViewMode;
            bool renderOnSiteEditSpans = (vm.IsEditLive()) && (DocumentContext.CurrentPageInfo != null);

            // On-site editing is active => render enclosing span tags
            if (renderOnSiteEditSpans)
            {
                OnSiteEditStartTagConfiguration tagConfig = GetOnSiteTagConfiguration();

                // Get the opening span tag
                string startSpan = OnSiteEditHelper.GetOnSiteEditStartTag(tagConfig);

                if (!string.IsNullOrEmpty(startSpan))
                {
                    writer.Write(startSpan);
                }
                else
                {
                    // Ensure that the end span will not be rendered when the start span was not rendered neither
                    renderOnSiteEditSpans = false;
                }

                if (EmptyContent)
                {
                    // Render "Empty editable control" text
                    writer.Write(OnSiteEditHelper.GetEmptyEditableWebPartTag(ID));
                }
            }

            RenderChildren(writer);

            if (renderOnSiteEditSpans)
            {
                // Render closing span tag
                writer.Write(OnSiteEditHelper.GetOnSiteEditEndTag());
            }
        }


        /// <summary>
        /// Creates tag configuration for on-site editting.
        /// </summary>
        protected virtual OnSiteEditStartTagConfiguration GetOnSiteTagConfiguration()
        {
            return new OnSiteEditStartTagConfiguration
            {
                Page = DocumentContext.CurrentPageInfo,
                EditUrl = EditPageUrl,
                DialogWidth = Unit.Parse(EditDialogWidth),
                ControlObject = this,
                CurrentUser = MembershipContext.AuthenticatedUser,
                WebPartIsEditable = true,
            };
        }


        /// <summary>
        /// Requests the view mode for the editable control
        /// </summary>
        /// <param name="viewMode">View mode to request</param>
        /// <param name="id">Control ID</param>
        public static bool RequestEditViewMode(ViewModeEnum viewMode, string id)
        {
            bool result = true;

            if (PortalContext.ViewMode.IsEditLive() && (viewMode == ViewModeEnum.EditDisabled))
            {
                // Do not render editable container for disabled editable controls in On-site editing (main window)
                return false;
            }

            if (!String.IsNullOrEmpty(id) && (viewMode.IsEdit()))
            {
                if (RequestStockHelper.Contains(EDITABLE_CONTROL_STORAGE_KEY, id))
                {
                    result = false;
                }
                RequestStockHelper.AddToStorage(EDITABLE_CONTROL_STORAGE_KEY, id, true, true);
            }

            return result;
        }


        /// <summary>
        /// Resolves the macros within current context.
        /// </summary>
        /// <param name="inputText">Input text to resolve</param>
        /// <param name="settings">Macro context object with specific options</param>
        protected virtual string ResolveMacros(string inputText, MacroSettings settings = null)
        {
            if (settings == null)
            {
                // Ensure current culture in evaluation context
                settings = new MacroSettings();
                settings.Culture = Thread.CurrentThread.CurrentCulture.ToString();
            }

            return MacroResolver.Resolve(inputText, settings);
        }

        #endregion
    }
}