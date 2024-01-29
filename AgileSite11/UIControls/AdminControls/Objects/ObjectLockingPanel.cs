using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.FormEngine.Web.UI;
using CMS.Membership;
using CMS.Synchronization;

namespace CMS.UIControls
{
    /// <summary>
    /// Server control which represents container which handles object locking.
    /// </summary>
    [ToolboxData("<{0}:ObjectLockingPanel runat=server></{0}:ObjectLockingPanel>")]
    public class ObjectLockingPanel : Panel
    {
        #region "Variables"

        /// <summary>
        /// Indicates if the object is locked.
        /// </summary>
        protected bool? mIsLocked = null;

        /// <summary>
        /// ObjectManager control.
        /// </summary>
        protected CMSObjectManager mObjectManager = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the object manager control if present on the page.
        /// </summary>
        public CMSObjectManager ObjectManager
        {
            get
            {
                return mObjectManager ?? (mObjectManager = CMSObjectManager.GetCurrent(this));
            }
        }


        /// <summary>
        /// Control's UI context
        /// </summary>
        private UIContext UIContext
        {
            get
            {
                return UIContextHelper.GetUIContext(this);
            }
        }


        /// <summary>
        /// Returns true if the object is locked for editing for current user.
        /// </summary>
        public bool IsLocked
        {
            get
            {
                if (ObjectManager == null)
                {
                    return false;
                }

                if (mIsLocked != null)
                {
                    return mIsLocked.Value;
                }

                if (!SynchronizationHelper.UseCheckinCheckout)
                {
                    mIsLocked = false;
                    return false;
                }

                if (UIContext.EditedObject != null)
                {
                    mIsLocked = IsObjectLocked();
                    return mIsLocked.Value;
                }

                return false;
            }
        }


        /// <summary>
        /// Indicates if processing of the UI panel should be stopped.
        /// </summary>
        public bool StopProcessing
        {
            get;
            set;
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            PageContext.InitComplete += PageHelper_InitComplete;
        }


        /// <summary>
        /// Init complete event handler
        /// </summary>
        void PageHelper_InitComplete(object sender, EventArgs e)
        {
            if (ObjectManager != null)
            {
                ObjectManager.ShowPanel = true;
            }
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.Enabled = !IsLocked;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the object is locked for editing for current user.
        /// </summary>
        public bool IsObjectLocked()
        {
            if (!SynchronizationHelper.UseCheckinCheckout)
            {
                return false;
            }

            if (UIContext.EditedObject != null)
            {
                GeneralizedInfo info = null;
                if (UIContext.EditedObject is BaseInfo)
                {
                    info = ((BaseInfo)UIContext.EditedObject).Generalized;
                }
                else if (UIContext.EditedObject is GeneralizedInfo)
                {
                    info = (GeneralizedInfo)UIContext.EditedObject;
                }
                if (info != null)
                {
                    return info.TypeInfo.SupportsLocking && !info.IsCheckedOutByUser(MembershipContext.AuthenticatedUser);
                }
            }

            return false;
        }

        #endregion
    }
}