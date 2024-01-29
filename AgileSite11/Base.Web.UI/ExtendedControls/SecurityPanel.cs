using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Security Panel.
    /// </summary>
    [ToolboxItem(false)]
    public class SecurityPanel : CMSPlaceHolder, INamingContainer
    {
        #region "Private variables"

        private string mRoles = null;
        private SecurityAccessEnum mSecurityAccess = SecurityAccessEnum.AllUsers;
        private int mOwnerId = 0;
        private int mCommunityGroupId = 0;
        private bool mIsDirty = false;

        #endregion


        #region "Properties"

        /// <summary>
        /// Semicolon-separated role codenames.
        /// </summary>
        [Category("Panel Control"), Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), RefreshProperties(RefreshProperties.Repaint), NotifyParentProperty(true), DefaultValue(null)]
        public string Roles
        {
            get
            {
                return mRoles;
            }
            set
            {
                mRoles = value;
                mIsDirty = true;
            }
        }


        /// <summary>
        /// Sets or gets SecurityAccessEnum.
        /// </summary>
        [Category("Panel Control"), Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), RefreshProperties(RefreshProperties.Repaint), NotifyParentProperty(true), DefaultValue(SecurityAccessEnum.AllUsers)]
        public SecurityAccessEnum SecurityAccess
        {
            get
            {
                return mSecurityAccess;
            }
            set
            {
                mSecurityAccess = value;
                mIsDirty = true;
            }
        }


        /// <summary>
        /// ID of owner (needed only if SecurityAccess is set to Owner).
        /// </summary>
        [Category("Panel Control"), Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), RefreshProperties(RefreshProperties.Repaint), NotifyParentProperty(true), DefaultValue(0)]
        public int OwnerID
        {
            get
            {
                return mOwnerId;
            }
            set
            {
                mOwnerId = value;
                mIsDirty = true;
            }
        }


        /// <summary>
        /// ID of group (needed only if SecurityAccess is set to GroupAdmin or GroupMembers.
        /// </summary>
        [Category("Panel Control"), Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), RefreshProperties(RefreshProperties.Repaint), NotifyParentProperty(true), DefaultValue(0)]
        public int CommunityGroupID
        {
            get
            {
                return mCommunityGroupId;
            }
            set
            {
                mCommunityGroupId = value;
                mIsDirty = true;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// OnLoad.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            SetVisibility();
        }


        /// <summary>
        /// OnPrerender - Set visibility if dirty bit is sets.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Set visibilty ona preRender only if dirty bit is sets
            if (mIsDirty)
            {
                SetVisibility();
            }
        }


        /// <summary>
        /// Sets the panel visibility with dependence on panel settings.
        /// </summary>
        public void SetVisibility()
        {
            // Clear dirty bit
            mIsDirty = false;

            Visible = false;
            // Set visibility (depends on selected security policy)
            switch (mSecurityAccess)
            {
                case SecurityAccessEnum.Nobody:
                    Visible = false;
                    break;

                case SecurityAccessEnum.AllUsers:
                    Visible = true;
                    break;

                case SecurityAccessEnum.AuthenticatedUsers:
                    Visible = AuthenticationHelper.IsAuthenticated();
                    break;

                case SecurityAccessEnum.AuthorizedRoles:
                    if (!string.IsNullOrEmpty(mRoles))
                    {
                        // Check if user is in one of defined roles
                        foreach (string role in mRoles.Split())
                        {
                            if (MembershipContext.AuthenticatedUser.IsInRole(role, SiteContext.CurrentSiteName))
                            {
                                Visible = true;
                                break;
                            }
                        }
                    }
                    break;

                case SecurityAccessEnum.GroupAdmin:
                    if (mCommunityGroupId != 0)
                    {
                        // Check whether user is group administrator
                        Visible = MembershipContext.AuthenticatedUser.IsGroupAdministrator(mCommunityGroupId);
                    }
                    else
                    {
                        throw new Exception("You have to specify CommunityGroupID if you want to use 'SecurityAccessEnum.GroupAdmin'.");
                    }
                    break;

                case SecurityAccessEnum.GroupMembers:
                    if (mCommunityGroupId != 0)
                    {
                        // Check whether user is member of group
                        Visible = MembershipContext.AuthenticatedUser.IsGroupMember(mCommunityGroupId);
                    }
                    else
                    {
                        throw new Exception("You have to specify CommunityGroupID if you want to use 'SecurityAccessEnum.GroupMembers'.");
                    }
                    break;

                case SecurityAccessEnum.Owner:
                    if (mOwnerId != 0)
                    {
                        // Check if user is owner
                        Visible = (MembershipContext.AuthenticatedUser.UserID == mOwnerId);
                    }
                    else
                    {
                        throw new Exception("You have to specify OwnerID if you want to use 'SecurityAccessEnum.Owner'.");
                    }
                    break;
            }
        }

        #endregion
    }
}