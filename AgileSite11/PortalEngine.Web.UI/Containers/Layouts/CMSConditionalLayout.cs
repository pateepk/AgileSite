using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.DeviceProfiles;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Conditional layout block.
    /// </summary>
    public class CMSConditionalLayout : PlaceHolder
    {
        #region "Variables"

        /// <summary>
        /// List of the inner web part zones.
        /// </summary>
        protected List<CMSWebPartZone> mWebPartZones = null;

        /// <summary>
        /// Parent page placeholder.
        /// </summary>
        protected CMSPagePlaceholder mPagePlaceholder = null;

        /// <summary>
        /// Returns true if the layout is visible.
        /// </summary>
        protected bool? mVisible = null;

        /// <summary>
        /// Returns the state of the visibility of the control within the group
        /// </summary>
        protected bool? mGroupVisible = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Parent page placeholder.
        /// </summary>
        public virtual CMSPagePlaceholder PagePlaceholder
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return null;
                }

                if (mPagePlaceholder == null)
                {
                    mPagePlaceholder = PortalHelper.FindParentPlaceholder(this);
                    if (mPagePlaceholder == null)
                    {
                        throw new Exception("[CMSConditionalLayout.PagePlaceholder]: Parent CMSPagePlaceholder not found.");
                    }
                }
                return mPagePlaceholder;
            }
            set
            {
                mPagePlaceholder = value;
            }
        }


        /// <summary>
        /// If true, the layout behaves actively in design mode of the page, meaning that it evaluates it's display condition
        /// </summary>
        public bool ActiveInDesignMode
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether control should be visible in design mode
        /// </summary>
        private bool DesignVisible
        {
            get
            {
                return !ActiveInDesignMode && PagePlaceholder.ViewMode.IsDesign(true);
            }
        }


        /// <summary>
        /// Returns true if the layout is visible.
        /// </summary>
        public override bool Visible
        {
            get
            {
                if (mVisible == null)
                {
                    mVisible = GetVisible();
                }

                // Check design visibility
                if (DesignVisible)
                {
                    return true;
                }

                // Visible - check group
                bool result = mVisible.Value;
                if (result)
                {
                    if (mGroupVisible == null)
                    {
                        string groupName = GroupName;
                        if (!String.IsNullOrEmpty(groupName))
                        {
                            // Check if some control is already visible
                            string key = PagePlaceholder.ClientID + "_ConditionalLayout_FirstVisible_" + groupName;
                            string firstVisibleId = ValidationHelper.GetString(RequestStockHelper.GetItem(key), "");

                            if (!String.IsNullOrEmpty(firstVisibleId))
                            {
                                // Not first in group - not visible unless the ID matches (already registered as visible)
                                mGroupVisible = (firstVisibleId == this.ClientID);
                            }
                            else
                            {
                                // First in group, visible and set the flag
                                mGroupVisible = true;
                                RequestStockHelper.Add(key, this.ClientID);
                            }
                        }
                        else
                        {
                            mGroupVisible = true;
                        }
                    }

                    return result && mGroupVisible.Value;
                }

                return false;
            }
            set
            {
                base.Visible = value;

                mVisible = value;
                //mGroupVisible = null;
            }
        }


        /// <summary>
        /// If true, the layout is shown only for given document types. The value contains list of document type class names separated by semicolon, e.g. "CMS.Article;CMS.Product"
        /// </summary>
        public string VisibleForDocumentTypes
        {
            get;
            set;
        }


        /// <summary>
        /// List of role names separated by semicolon to which the user must belong in order to display the layout
        /// </summary>
        public string VisibleForRoles
        {
            get;
            set;
        }


        /// <summary>
        /// List of device profile names separated by semicolon in which current device must belong in order to display the layout
        /// </summary>
        public virtual string VisibleForDeviceProfiles
        {
            get;
            set;
        }


        /// <summary>
        /// List of domain names separated by semicolon for which the layout is displayed
        /// </summary>
        public string VisibleForDomains
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the layout group to which the layout belongs. If more layouts use the same group, only the first one that matches the visibility condition is displayed.
        /// </summary>
        public string GroupName
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the list of all the inner web part zones (CMSWebPartZone controls).
        /// </summary>
        public List<CMSWebPartZone> WebPartZones
        {
            get
            {
                if (mWebPartZones == null)
                {
                    EnsureChildControls();

                    // Resolve all properties set in markup (including child elements)
                    DataBind();

                    // Collect the zones
                    mWebPartZones = PortalHelper.CollectWebPartZones(this);
                    if (mWebPartZones == null)
                    {
                        mWebPartZones = new List<CMSWebPartZone>();
                    }
                    else
                    {
                        // Connect the zones to the conditional layout
                        foreach (CMSWebPartZone zone in mWebPartZones)
                        {
                            zone.ConditionalLayout = this;
                        }
                    }
                }

                return mWebPartZones;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks whether the layout is visible or not and returns the state of the layout
        /// </summary>
        public bool CheckLayoutVisible()
        {
            return Visible;
        }


        /// <summary>
        /// Gets the visibility status of the control
        /// </summary>
        private bool GetVisible()
        {
            // Handle the design mode
            if (DesignVisible)
            {
                return true;
            }

            bool result = true;

            // Handle document types settings
            if (!String.IsNullOrEmpty(VisibleForDocumentTypes))
            {
                string docTypes = ";" + VisibleForDocumentTypes + ";";

                if (docTypes.IndexOfCSafe(";" + DocumentContext.CurrentPageInfo.ClassName + ";", true) < 0)
                {
                    result = false;
                }
            }

            // Check display to roles
            if (result && !String.IsNullOrEmpty(VisibleForRoles))
            {
                var currentUser = MembershipContext.AuthenticatedUser;

                bool show = false;
                string[] roles = VisibleForRoles.Split(';');

                string siteName = SiteContext.CurrentSiteName;

                // Check the roles
                foreach (string role in roles)
                {
                    if (((role.ToLowerCSafe() != RoleName.NOTAUTHENTICATED) && currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin)) || currentUser.IsInRole(role, siteName))
                    {
                        show = true;
                        break;
                    }
                }

                if (!show)
                {
                    result = false;
                }
            }

            // Check display for domains
            if (result && !String.IsNullOrEmpty(VisibleForDomains))
            {
                bool show = false;
                string[] domains = VisibleForDomains.Split(';');

                string currentDomain = RequestContext.CurrentDomain;

                // Check the roles
                foreach (string domain in domains)
                {
                    if (URLHelper.DomainMatch(currentDomain, domain, false))
                    {
                        show = true;
                        break;
                    }
                }

                if (!show)
                {
                    result = false;
                }
            }

            // Check display for device profiles
            if (result && !String.IsNullOrEmpty(VisibleForDeviceProfiles))
            {
                bool show = false;
                string[] profiles = VisibleForDeviceProfiles.Split(';');
                string currentProfile = DeviceContext.CurrentDeviceProfileName;

                // Check the profiles
                foreach (string profile in profiles)
                {
                    if (currentProfile.EqualsCSafe(profile, true))
                    {
                        show = true;
                        break;
                    }
                }

                if (!show)
                {
                    result = false;
                }
            }

            return result;
        }


        /// <summary>
        /// Load event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // If not visible, mark the possibility that editable controls are hidden on current page
            if (!this.Visible)
            {
                PortalContext.EditableControlsHidden = true;
            }
        }


        /// <summary>
        /// Binds a data source to the invoked server control and all its child controls.
        /// </summary>
        public override void DataBind()
        {
            // It's not necessary to bind data in design mode. Design mode should display the layout nevertheless what is set in its properties.
            // Layout properties can contain document related expressions which are not present in Template -> Edit -> Design tab.
            if (!DesignVisible)
            {
                // Resolve all properties set in markup (including child elements)
                base.DataBind();
            }
        }

        #endregion
    }
}