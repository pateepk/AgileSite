using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.SocialMarketing.Web.UI
{
    /// <summary>
    /// Base class for social marketing auto post controls.
    /// </summary>
    public abstract class SocialMarketingAutoPostControl : FormEngineUserControl
    {
        #region "Private fields"

        private TreeNode mDocument;
        private SiteInfoIdentifier mSiteIdentifier;
        private bool? mIsUnderWorkflow;
        private bool? mIsFeatureAvailable;
        private bool? mHasUserReadPermission;
        private bool? mHasUserModifyPermission;

        #endregion


        #region "Protected properties"

        /// <summary>
        /// Gets the document that is edited by the form.
        /// </summary>
        protected TreeNode Document
        {
            get
            {
                if ((mDocument == null) && (Form != null) && (Form.EditedObject is TreeNode))
                {
                    mDocument = (TreeNode)Form.EditedObject;
                }

                return mDocument;
            }
        }


        /// <summary>
        /// Gets site identefier that specifies context used when creating or editing social marketing post.
        /// </summary>
        protected SiteInfoIdentifier SiteIdentifier
        {
            get
            {
                if (mSiteIdentifier == null)
                {
                    mSiteIdentifier = Document != null ? Document.OriginalNodeSiteID : SiteContext.CurrentSiteID;
                }
                return mSiteIdentifier;
            }
        }


        /// <summary>
        /// Indicates whether the Document is under workflow or not. 
        /// </summary>
        protected bool IsUnderWorkflow
        {
            get
            {
                if (!mIsUnderWorkflow.HasValue)
                {
                    mIsUnderWorkflow = ((Document != null) && (Document.GetWorkflow() != null));
                }
                return mIsUnderWorkflow.Value;
            }
        }


        /// <summary>
        /// Indicates whether Social marketing features are available for current domain's license or not.
        /// </summary>
        protected bool IsFeatureAvailable
        {
            get
            {
                if (!mIsFeatureAvailable.HasValue)
                {
                    mIsFeatureAvailable = LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.SocialMarketing);
                }
                return mIsFeatureAvailable.Value;
            }
        }


        /// <summary>
        /// Indicates whether current user has read permission on Social marketing posts.
        /// </summary>
        protected bool HasUserReadPermission
        {
            get
            {
                if (!mHasUserReadPermission.HasValue)
                {
                    mHasUserReadPermission = UserInfoProvider.IsAuthorizedPerResource(ModuleName.SOCIALMARKETING, "Read", SiteIdentifier, MembershipContext.AuthenticatedUser, false);
                }
                return mHasUserReadPermission.Value;
            }
        }


        /// <summary>
        /// Indicates whether current user has modify permission on Social marketing posts.
        /// </summary>
        protected bool HasUserModifyPermission
        {
            get
            {
                if (!mHasUserModifyPermission.HasValue)
                {
                    mHasUserModifyPermission = UserInfoProvider.IsAuthorizedPerResource(ModuleName.SOCIALMARKETING, "Modify", SiteIdentifier, MembershipContext.AuthenticatedUser, false)
                || UserInfoProvider.IsAuthorizedPerResource(ModuleName.SOCIALMARKETING, "ModifyPosts", SiteIdentifier, MembershipContext.AuthenticatedUser, false);
                }
                return mHasUserModifyPermission.Value;
            }
        }

        #endregion
    }
}