using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Abstract class for the editable web parts (web parts that needs to store the content within the document content).
    /// </summary>
    public abstract class CMSAbstractEditableWebPart : CMSAbstractWebPart, ICMSEditableControl
    {
        #region "Public properties"
        
        /// <summary>
        /// Gets the value that indicates whether panel combination was changed
        /// </summary>
        private static bool MVTCombinationPanelChanged
        {
            get
            {
                if ((HttpContext.Current == null) || !RequestHelper.IsPostBack() || (!SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSMVTEnabled")))
                {
                    return false;
                }

                string eventTarget = ValidationHelper.GetString(HttpContext.Current.Request[Page.postEventSourceID], String.Empty).ToLowerCSafe();
                string eventArgument = ValidationHelper.GetString(HttpContext.Current.Request[Page.postEventArgumentID], String.Empty).ToLowerCSafe();

                return (eventArgument == "combinationchanged") && (eventTarget.Contains("mvt"));
            }
        }


        /// <summary>
        /// Display parent content if not set
        /// </summary>
        public bool UseParentContent
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("UseParentContent"), false);
            }
            set
            {
                SetValue("UseParentContent", value);
            }
        }


        /// <summary>
        /// If set, only published content is displayed on a live site.
        /// </summary>
        public bool SelectOnlyPublished
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("SelectOnlyPublished"), false);
            }
            set
            {
                SetValue("SelectOnlyPublished", value);
            }
        }


        /// <summary>
        /// Gets the url of the page which ensures editing of the web part's editable content in the On-Site editing mode.
        /// </summary>
        public virtual string EditPageUrl
        {
            get
            {
                return String.Empty;
            }
        }


        /// <summary>
        /// Gets the width of the edit dialog in the On-Site editing mode.
        /// </summary>
        public virtual string EditDialogWidth
        {
            get
            {
                return String.Empty;
            }
        }


        /// <summary>
        /// Indicates whether the web part contains any editable content
        /// </summary>
        public bool EmptyContent
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Method that is called when the page content is loaded, override to implement the control initialization after the content has been loaded.
        /// </summary>
        public override void OnContentLoaded()
        {
            // Ensure content loading and saving for invisible editable regions
            if (HideOnCurrentPage)
            {
                PortalContext.EditableControlsHidden = true;
            }
            base.OnContentLoaded();
        }


        /// <summary>
        /// Loads the control content.
        /// </summary>
        /// <param name="content">Dynamic content to load to the web part</param>
        /// <param name="forceReload">If true, the content is forced to reload</param>
        public virtual void LoadContent(string content, bool forceReload = false)
        {
            throw new Exception("[CMSAbstractEditableWebPart.LoadContent]: This method must be overridden by the inherited class.");
        }


        /// <summary>
        /// Loads the control content.
        /// </summary>
        /// <param name="pageInfo">Page info with the web part content</param>
        /// <param name="forceReload">If true, the content is forced to reload</param>
        public virtual void LoadContent(PageInfo pageInfo, bool forceReload = false)
        {
            if (pageInfo == null)
            {
                return;
            }

            string id = GetControlContentID();

            // Get the content
            string content = pageInfo.EditableWebParts[id];

            // Mark that the editable content is empty
            EmptyContent = (content == null) || string.IsNullOrEmpty(content.Trim());

            // If content not found and inheritance allowed, try to load from parent
            if (String.IsNullOrEmpty(content) && UseParentContent && !pageInfo.IsRoot() && !ViewMode.IsOneOf(ViewModeEnum.Edit, ViewModeEnum.EditDisabled))
            {
                int level = pageInfo.NodeLevel;
                pageInfo = PortalManager.HierarchyPageInfo.GetPageLevel(level - 1);

                if (pageInfo != null)
                {
                    LoadContent(pageInfo, forceReload);
                    return;
                }
            }

            // Load default value
            if (EmptyContent && RequestHelper.IsPostBack())
            {
                content = GetContent();
            }

            if (ViewMode != ViewModeEnum.Edit && ViewMode != ViewModeEnum.EditDisabled)
            {
                content = ResolveMacros(content);
            }
            else
            {
                // Do not resolve macros for editable content in page tab
                content = MacroSecurityProcessor.RemoveSecurityParameters(content, false, null);
            }

            LoadContent(content, forceReload | MVTCombinationPanelChanged);
        }


        /// <summary>
        /// Gets the ID of the content stored in the document field
        /// </summary>
        private string GetControlContentID()
        {
            // Prepare the ID
            string id = ID.ToLowerCSafe();
            WebPartInstance pi = PartInstance;

            if (pi != null)
            {
                id = pi.ControlID.ToLowerCSafe();
            }

            if (InstanceGUID != Guid.Empty)
            {
                id += ";" + InstanceGUID.ToString().ToLowerCSafe();
                if (pi != null)
                {
                    if (pi.CurrentVariantInstance != null)
                    {
                        id += "(" + pi.CurrentVariantInstance.ControlID.ToLowerCSafe() + ")";
                    }
                    else if (IsWidget && IsVariant)
                    {
                        id += "(" + pi.ControlID.ToLowerCSafe() + ")";
                    }
                }
            }

            return id;
        }


        /// <summary>
        /// Returns the current web part content.
        /// </summary>
        public virtual string GetContent()
        {
            throw new Exception("[CMSAbstractEditableWebPart.GetContent]: This method must be overridden by the inherited class.");
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
                string id = GetControlContentID();

                if ((PartInstance != null) && (PartInstance.CurrentVariantInstance != null))
                {
                    if (PartInstance.CurrentVariantInstance.ControlID != PartInstance.ControlID)
                    {
                        return;
                    }
                }

                // Save the value
                pageInfo.EditableWebParts[id] = content;
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
        /// Returns the value of web part property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public override object GetValue(string propertyName)
        {
            if (propertyName.ToLowerCSafe() == "content")
            {
                return GetContent();
            }
            else
            {
                return base.GetValue(propertyName);
            }
        }

        #endregion
    }
}