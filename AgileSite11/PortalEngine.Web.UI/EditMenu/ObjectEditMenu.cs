using CMS.DataEngine;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Base class for object edit menu control.
    /// </summary>
    public abstract class ObjectEditMenu : BaseEditMenu
    {
        #region "Variables"

        private bool? mAllowCheckOut;
        private bool? mAllowUndoCheckOut;
        private bool? mAllowCheckIn;
        private bool? mAllowSave;
        private bool? mShowSave;
        private bool? mShowCheckOut;
        private bool? mShowUndoCheckOut;
        private bool? mShowCheckIn;

        #endregion


        #region "Button display options"

        /// <summary>
        /// Show the Save button.
        /// </summary>
        public bool ShowSave
        {
            get
            {
                if (mShowSave == null)
                {
                    mShowSave = true;
                }
                return mShowSave.Value;
            }
            set
            {
                mShowSave = value;
            }
        }


        /// <summary>
        /// Show the Check Out button.
        /// </summary>
        public bool ShowCheckOut
        {
            get
            {
                if (mShowCheckOut == null)
                {
                    mShowCheckOut = (InfoObject != null) && InfoObject.TypeInfo.SupportsLocking && (InfoObject.Generalized.ObjectID > 0) && !InfoObject.Generalized.IsCheckedOut;
                }
                return mShowCheckOut.Value;
            }
            set
            {
                mShowCheckOut = value;
            }
        }


        /// <summary>
        /// Show the Check In button.
        /// </summary>
        public bool ShowCheckIn
        {
            get
            {
                if (mShowCheckIn == null)
                {
                    mShowCheckIn = (InfoObject != null) && InfoObject.TypeInfo.SupportsLocking && (InfoObject.Generalized.ObjectID > 0) && InfoObject.Generalized.IsCheckedOut;
                }
                return mShowCheckIn.Value;
            }
            set
            {
                mShowCheckIn = value;
            }
        }


        /// <summary>
        /// Show the Undo Check Out button.
        /// </summary>
        public bool ShowUndoCheckOut
        {
            get
            {
                if (mShowUndoCheckOut == null)
                {
                    mShowUndoCheckOut = (InfoObject != null) && InfoObject.TypeInfo.SupportsLocking && (InfoObject.Generalized.ObjectID > 0) && InfoObject.Generalized.IsCheckedOut;
                }
                return mShowUndoCheckOut.Value;
            }
            set
            {
                mShowUndoCheckOut = value;
            }
        }


        /// <summary>
        /// Allow the Check Out button.
        /// </summary>
        public bool AllowCheckOut
        {
            get
            {
                if (mAllowCheckOut == null)
                {
                    if (InfoObject != null)
                    {
                        mAllowCheckOut = InfoObject.TypeInfo.SupportsLocking && InfoObject.CheckPermissions(PermissionsEnum.Modify, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
                    }
                    else
                    {
                        mAllowCheckOut = false;
                    }
                }
                return mAllowCheckOut.Value;
            }
            set
            {
                mAllowCheckOut = value;
            }
        }


        /// <summary>
        /// Allow the Check In button.
        /// </summary>
        public bool AllowCheckIn
        {
            get
            {
                if (mAllowCheckIn == null)
                {
                    if (InfoObject != null)
                    {
                        mAllowCheckIn = InfoObject.TypeInfo.SupportsLocking && InfoObject.CheckPermissions(PermissionsEnum.Modify, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
                    }
                    else
                    {
                        mAllowCheckIn = false;
                    }
                }
                return mAllowCheckIn.Value;
            }
            set
            {
                mAllowCheckIn = value;
            }
        }


        /// <summary>
        /// Allow the Undo Check Out button.
        /// </summary>
        public bool AllowUndoCheckOut
        {
            get
            {
                if (mAllowUndoCheckOut == null)
                {
                    if (InfoObject != null)
                    {
                        mAllowUndoCheckOut = InfoObject.TypeInfo.SupportsLocking && InfoObject.CheckPermissions(PermissionsEnum.Modify, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
                    }
                    else
                    {
                        mAllowUndoCheckOut = false;
                    }
                }
                return mAllowUndoCheckOut.Value;
            }
            set
            {
                mAllowUndoCheckOut = value;
            }
        }


        /// <summary>
        /// Allow the Save button.
        /// </summary>
        public override bool AllowSave
        {
            get
            {
                if (mAllowSave == null)
                {
                    if (InfoObject != null)
                    {
                        mAllowSave = InfoObject.CheckPermissions(PermissionsEnum.Modify, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
                    }
                    else
                    {
                        mAllowSave = false;
                    }
                }
                return mAllowSave.Value;
            }
            set
            {
                mAllowSave = value;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Object the menu is working with.
        /// </summary>
        public virtual BaseInfo InfoObject
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets or sets if the menu should be rendered with styles for PreviewControl.
        /// </summary>
        public bool PreviewMode
        {
            get;
            set;
        }

        #endregion
    }
}