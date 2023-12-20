using System;
using System.ComponentModel;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// CMSObjectManagementButtons class.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSObjectManagementButtons : Control
    {
        #region "Variables"

        private string mEditText;
        private string mDeleteText;
        private string mCulture;
        private string mObjectKeyName = "objectid";
        private bool mCheckPermissions = true;
        private string mSiteName;
        private BaseInfo mCurrentObject;
        private bool mIsLiveSite = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if control is used on a live site.
        /// </summary>
        public bool IsLiveSite
        {
            get
            {
                return mIsLiveSite;
            }
            set
            {
                mIsLiveSite = value;
            }
        }


        /// <summary>
        /// Returns current UI culture.
        /// </summary>
        private string Culture
        {
            get
            {
                if (mCulture == null)
                {
                    CurrentUserInfo ui = MembershipContext.AuthenticatedUser;
                    mCulture = IsLiveSite ? LocalizationContext.PreferredCultureCode : ui.PreferredUICultureCode;
                }

                return mCulture;
            }
        }


        /// <summary>
        /// Custom edit caption.
        /// </summary>
        public string EditText
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return mEditText;
                }

                if (String.IsNullOrEmpty(mEditText))
                {
                    mEditText = ResHelper.GetString("general.edit", Culture);
                }
                return mEditText;
            }
            set
            {
                mEditText = value;
            }
        }



        /// <summary>
        /// Custom delete caption.
        /// </summary>
        public string DeleteText
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return mDeleteText;
                }

                if (String.IsNullOrEmpty(mDeleteText))
                {
                    mDeleteText = ResHelper.GetString("general.delete", Culture);
                }

                return mDeleteText;
            }
            set
            {
                mDeleteText = value;
            }
        }


        /// <summary>
        /// Object type
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Object identifier
        /// </summary>
        public int ObjectID
        {
            get;
            set;
        }


        /// <summary>
        /// Redirect URL to edit control.
        /// </summary>
        public string RedirectUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Redirect URL to items listing.
        /// </summary>
        public string RedirectListUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Query string key name for object identifier.
        /// </summary>
        public string ObjectKeyName
        {
            get
            {
                return mObjectKeyName;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    mObjectKeyName = value;
                }
            }
        }


        /// <summary>
        /// Gets or sets if buttons visibility is checked by permissions.
        /// </summary>
        public bool CheckPermissions
        {
            get
            {
                return mCheckPermissions;
            }
            set
            {
                mCheckPermissions = value;
            }
        }


        /// <summary>
        /// Gets or sets if permissions should be checked on object level.
        /// </summary>
        public bool CheckObjectPermissions
        {
            get;
            set;
        }


        /// <summary>
        /// Site name
        /// </summary>
        public string SiteName
        {
            get
            {
                if (mSiteName == null)
                {
                    // Object level permissions should be checked
                    if (CheckObjectPermissions && (CurrentObject != null))
                    {
                        // Get object site name
                        mSiteName = (CurrentObject.Generalized.ObjectSiteID > 0) ? CurrentObject.Generalized.ObjectSiteName : SiteContext.CurrentSiteName;
                    }
                    else
                    {
                        // Use current site
                        mSiteName = SiteContext.CurrentSiteName;
                    }
                }

                return mSiteName;
            }
            set
            {
                mSiteName = value;
            }
        }


        /// <summary>
        /// Current object
        /// </summary>
        private BaseInfo CurrentObject
        {
            get
            {
                if ((mCurrentObject == null) && !String.IsNullOrEmpty(ObjectType))
                {
                    // Due to performance, don't get info object if the object permissions shouldn't be checked
                    mCurrentObject = !CheckObjectPermissions ? ModuleManager.GetReadOnlyObject(ObjectType) : ProviderHelper.GetInfoById(ObjectType, ValidationHelper.GetInteger(ObjectID, 0));
                }

                return mCurrentObject;
            }
        }

        #endregion


        #region "Methods & Events"

        /// <summary>
        /// OnLoad event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SetupControl();
        }


        /// <summary>
        /// Build hyperlink.
        /// </summary>
        /// <returns>Returns hyperlink strings</returns>
        public void SetupControl()
        {
            // Object not found
            if (CurrentObject == null)
            {
                return;
            }

            var render = false;
            var pnlButtons = new CMSPanel
            {
                CssClass = "cms-bootstrap"
            };

            if (!CheckPermissions || CurrentObject.CheckPermissions(PermissionsEnum.Modify, SiteName, MembershipContext.AuthenticatedUser))
            {
                var btnEdit = new CMSButton();
                btnEdit.Text = EditText;
                btnEdit.ButtonStyle = ButtonStyle.Default;
                btnEdit.Click += btnEdit_Click;

                pnlButtons.Controls.Add(btnEdit);
                render = true;
            }

            if (!CheckPermissions || CurrentObject.CheckPermissions(PermissionsEnum.Delete, SiteName, MembershipContext.AuthenticatedUser))
            {
                var btnDelete = new CMSButton();
                btnDelete.Text = DeleteText;
                btnDelete.ButtonStyle = ButtonStyle.Default;
                btnDelete.Click += btnDelete_Click;
                btnDelete.OnClientClick = "if (confirm(\"" + ResHelper.GetString("general.deleteconfirmation") + "\") == false) {return false;}";

                pnlButtons.Controls.Add(btnDelete);
                render = true;
            }

            if (render)
            {
                Controls.Add(pnlButtons);
                Controls.Add(new LiteralControl("<div class=\"CMSEditModeButtonClear\"></div>"));
            }
        }


        /// <summary>
        /// btnEdit event handler.
        /// </summary>
        protected void btnEdit_Click(object sender, EventArgs e)
        {
            URLHelper.Redirect(UrlResolver.ResolveUrl(URLHelper.AddParameterToUrl(!String.IsNullOrEmpty(RedirectUrl) ? RedirectUrl : RequestContext.CurrentURL, ObjectKeyName, ObjectID.ToString())));
        }


        /// <summary>
        /// btnDelete event handler.
        /// </summary>
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!CheckPermissions || CurrentObject.CheckPermissions(PermissionsEnum.Delete, SiteName, MembershipContext.AuthenticatedUser))
            {
                var info = CurrentObject;
                if (!CheckObjectPermissions)
                {
                    // Get object itself
                    info = ProviderHelper.GetInfoById(ObjectType, ValidationHelper.GetInteger(ObjectID, 0));
                }

                if (info != null)
                {
                    info.Delete();
                }

                URLHelper.Redirect(UrlResolver.ResolveUrl(!String.IsNullOrEmpty(RedirectListUrl) ? RedirectListUrl : URLHelper.RemoveParameterFromUrl(RequestContext.CurrentURL, ObjectKeyName)));
            }
            else
            {
                // Add messages placeholder
                var messagesPlaceholder = new MessagesPlaceHolder
                    {
                        ID = "plcM",
                        IsLiveSite = true
                    };
                Controls.AddAt(0, messagesPlaceholder);

                messagesPlaceholder.ShowError(ResHelper.GetString("general.nopermission"));
            }
        }

        #endregion
    }
}