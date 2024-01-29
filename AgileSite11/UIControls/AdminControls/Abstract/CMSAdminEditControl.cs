using System;

using CMS.Helpers;

namespace CMS.UIControls
{ 
    /// <summary>
    /// Administration edit control
    /// </summary>
    public abstract class CMSAdminEditControl : CMSAdminControl
    {
        #region "Events"

        /// <summary>
        /// Occurs when the edited object is saved.
        /// </summary>
        public event EventHandler OnSaved;

        #endregion


        #region "Properties"

        /// <summary>
        /// Item ID.
        /// </summary>
        public int ItemID
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["ItemID"], 0);
            }
            set
            {
                ViewState["ItemID"] = value;
            }
        }


        /// <summary>
        /// Item GUID.
        /// </summary>
        public Guid ItemGUID
        {
            get
            {
                return ValidationHelper.GetGuid(ViewState["ItemGUID"], Guid.Empty);
            }
            set
            {
                ViewState["ItemGUID"] = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Raises the OnSaved event.
        /// </summary>
        public void RaiseOnSaved()
        {
            if (OnSaved != null)
            {
                OnSaved(this, null);
            }
        }

        #endregion
    }
}