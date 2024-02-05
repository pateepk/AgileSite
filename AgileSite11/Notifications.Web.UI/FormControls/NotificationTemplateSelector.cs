using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.DataEngine;
using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// Form control for the notification template selector.
    /// </summary>
    [ToolboxData("<{0}:NotificationTemplateSelector runat=server></{0}:NotificationTemplateSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class NotificationTemplateSelector : FormControl
    {
        #region "Private variables"

        private bool mUseTemplateNameForSelection = true;
        private bool mAddGlobalTemplates = true;
        private int mSiteId;
        private CMSDropDownList mDrpList;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Returns ClientID of the dropdown with templates.
        /// </summary>
        public string ValueElementID
        {
            get
            {
                EnsureChildControl(ref mDrpList);
                
                return mDrpList.ClientID;
            }
        }


        /// <summary>
        /// Gets or sets the Template ID.
        /// </summary>
        public int TemplateID
        {
            get
            {
                if (mUseTemplateNameForSelection)
                {
                    BaseInfo info = ProviderHelper.GetInfoByName(PredefinedObjectType.NOTIFICATIONTEMPLATE, ValidationHelper.GetString(Value, String.Empty));
                    if (info != null)
                    {
                        return info.Generalized.ObjectID;
                    }
                    return 0;
                }
                else
                {
                    return ValidationHelper.GetInteger(Value, 0);
                }
            }
            set
            {
                if (mUseTemplateNameForSelection)
                {
                    // Convert ID to name
                    BaseInfo info = ProviderHelper.GetInfoById(PredefinedObjectType.NOTIFICATIONTEMPLATE, value);
                    if (info != null)
                    {
                        Value = info.Generalized.ObjectCodeName;
                    }
                }
                else
                {
                    Value = value;
                }
            }
        }


        /// <summary>
        /// Gets or sets the Template code name.
        /// </summary>
        public string TemplateName
        {
            get
            {
                if (!mUseTemplateNameForSelection)
                {
                    BaseInfo info = ProviderHelper.GetInfoById(PredefinedObjectType.NOTIFICATIONTEMPLATE, ValidationHelper.GetInteger(Value, 0));
                    if (info != null)
                    {
                        return info.Generalized.ObjectCodeName;
                    }
                    return String.Empty;
                }
                else
                {
                    return ValidationHelper.GetString(Value, String.Empty);
                }
            }
            set
            {
                if (!mUseTemplateNameForSelection)
                {
                    // Convert ID to name
                    BaseInfo info = ProviderHelper.GetInfoByName(PredefinedObjectType.NOTIFICATIONTEMPLATE, value);
                    if (info != null)
                    {
                        Value = info.Generalized.ObjectID;
                    }
                }
                else
                {
                    Value = value;
                }
            }
        }


        /// <summary>
        ///  If true, selected value is TemplateName, if false, selected value is TemplateID.
        /// </summary>
        public bool UseTemplateNameForSelection
        {
            get
            {
                return mUseTemplateNameForSelection;
            }
            set
            {
                mUseTemplateNameForSelection = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines, whether to add (global) record to the CMSDropDownList.
        /// </summary>
        public bool AddGlobalTemplates
        {
            get
            {
                return mAddGlobalTemplates;
            }
            set
            {
                mAddGlobalTemplates = value;
            }
        }


        /// <summary>
        /// Gets or sets SiteId value.
        /// </summary>
        public int SiteId
        {
            get
            {
                if (mSiteId == 0)
                {
                    mSiteId = SiteContext.CurrentSite.SiteID;
                }
                return mSiteId;
            }
            set
            {
                mSiteId = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines, whether to add none item record to the CMSDropDownList.
        /// </summary>
        public bool AddNoneRecord
        {
            get
            {
                return GetValue("AllowEmpty", true);
            }
            set
            {
                SetValue("AllowEmpty", value);
            }
        }


        /// <summary>
        /// Gets current uni-selector.
        /// </summary>
        private UniSelector UniSelector
        {
            get
            {
                return (UniSelector)EditingControl;
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public NotificationTemplateSelector()
        {
            FormControlName = "NotificationTemplateSelector";
        }


        #region "Methods"

        /// <summary>
        /// OnLoad event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (UniSelector != null)
            {
                UniSelector.ReturnColumnName = (UseTemplateNameForSelection ? "TemplateName" : "TemplateID");
            }

            if (AddGlobalTemplates)
            {
                SetValue("FilterMode", "notificationtemplateglobal");
            }
            else
            {
                SetValue("FilterMode", "notificationtemplate");
            }
        }

        #endregion
    }
}