using System;
using System.Collections;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Abstract class that all properties controls should inherit from.
    /// </summary>
    public abstract class ItemProperties : CMSUserControl
    {
        #region "Private variables"

        private DialogConfiguration mConfig = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Hashtable of custom parameters passed to the properties.
        /// </summary>
        public Hashtable Parameters
        {
            get
            {
                return (Hashtable)ViewState["Parameters"];
            }
            set
            {
                ViewState["Parameters"] = value;
            }
        }


        /// <summary>
        /// Selected source type.
        /// </summary>
        public MediaSourceEnum SourceType
        {
            get
            {
                string srcType = ValidationHelper.GetString(ViewState["SourceType"], "");
                return CMSDialogHelper.GetMediaSource(srcType);
            }
            set
            {
                ViewState["SourceType"] = CMSDialogHelper.GetMediaSource(value);
            }
        }


        /// <summary>
        /// Site item is related to.
        /// </summary>
        public string SiteDomainName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["SiteDomainName"], "");
            }
            set
            {
                ViewState["SiteDomainName"] = value;
            }
        }


        /// <summary>
        /// History ID of item object.
        /// </summary>
        public int HistoryID
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["HistoryID"], 0);
            }
            set
            {
                ViewState["HistoryID"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the text which is displayed when no item is loaded in the properties.
        /// </summary>
        public string NoSelectionText
        {
            get
            {
                return ValidationHelper.GetString(ViewState["NoSelectionText"], CMSDialogHelper.GetSelectItemMessage(Config, SourceType));
            }
            set
            {
                ViewState["NoSelectionText"] = value;
            }
        }


        /// <summary>
        /// Indicates whether the item properties are displayed for is coming from within the system or not.
        /// </summary>
        public bool ItemNotSystem
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ItemNotSystem"], false);
            }
            set
            {
                ViewState["ItemNotSystem"] = value;
            }
        }


        /// <summary>
        /// Client ID of editor where new item should be inserted.
        /// </summary>
        public string EditorClientID
        {
            get
            {
                return ValidationHelper.GetString(ViewState["Editor_ClientID"], null);
            }
            set
            {
                ViewState["Editor_ClientID"] = value;
            }
        }


        /// <summary>
        /// Gets or sets current dialog configuration.
        /// </summary>
        public DialogConfiguration Config
        {
            get
            {
                if (mConfig == null)
                {
                    mConfig = DialogConfiguration.GetDialogConfiguration();
                }
                return mConfig;
            }
            set
            {
                mConfig = value;
            }
        }

        #endregion


        #region "Virtual methods"

        /// <summary>
        /// Loads selected item into the child controls.
        /// </summary>
        /// <param name="item">Item to load</param>
        /// <param name="properties">Item properties to load</param>
        public virtual void LoadSelectedItems(MediaItem item, Hashtable properties)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Loads selected item into the child controls and ensures displaying correct controls.
        /// </summary>
        /// <param name="properties">Properties collection</param>
        public virtual void LoadItemProperties(Hashtable properties)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Loads selected properties into the child controls and ensures displaying correct values.
        /// </summary>
        /// <param name="properties">Properties collection</param>
        public virtual void LoadProperties(Hashtable properties)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns all parameters of the selected item as name – value collection.
        /// </summary>
        public virtual Hashtable GetItemProperties()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Validates the user input.
        /// </summary>
        public virtual bool Validate()
        {
            return true;
        }


        /// <summary>
        /// Clears the form with properties.
        /// </summary>
        /// <param name="hideProperties">Indicates whether to hide the properties and show the message</param>
        public virtual void ClearProperties(bool hideProperties)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Clears the form with properties.
        /// </summary>
        public virtual void ClearProperties()
        {
            ClearProperties(true);
        }

        #endregion
    }
}