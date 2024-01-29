using System;
using System.Web.UI;

using CMS.Base;
using CMS.DocumentEngine;
using CMS.Globalization;
using CMS.IO;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;

using TimeZoneInfo = CMS.Globalization.TimeZoneInfo;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for the template pages.
    /// </summary>
    public abstract class TemplateMasterPage : MasterPage, ITimeZoneManager
    {
        #region "Private variables"

        private TimeZoneTypeEnum mTimeZoneType = TimeZoneTypeEnum.WebSite;
        private TimeZoneInfo mCustomTimeZone = null;

        private Control mLogsContainer = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Template page.
        /// </summary>
        public virtual TemplatePage TemplatePage
        {
            get
            {
                return (TemplatePage)Page;
            }
        }


        /// <summary>
        /// Document base.
        /// </summary>
        public virtual DocumentBase DocumentBase
        {
            get
            {
                return TemplatePage.DocumentBase;
            }
        }


        /// <summary>
        /// Current site.
        /// </summary>
        public virtual SiteInfo CurrentSite
        {
            get
            {
                return TemplatePage.CurrentSite;
            }
        }


        /// <summary>
        /// Current user.
        /// </summary>
        public virtual CurrentUserInfo CurrentUser
        {
            get
            {
                return TemplatePage.CurrentUser;
            }
        }


        /// <summary>
        /// Current page info.
        /// </summary>
        public virtual PageInfo CurrentPage
        {
            get
            {
                return TemplatePage.CurrentPage;
            }
        }


        /// <summary>
        /// Page manager.
        /// </summary>
        public virtual IPageManager PageManager
        {
            get
            {
                return TemplatePage.PageManager;
            }
            set
            {
                TemplatePage.PageManager = value;
            }
        }


        /// <summary>
        /// DocType.
        /// </summary>
        public virtual string DocType
        {
            get
            {
                return TemplatePage.DocType;
            }
            set
            {
                TemplatePage.DocType = value;
            }
        }


        /// <summary>
        /// Body parameters.
        /// </summary>
        public virtual string BodyParameters
        {
            get
            {
                return TemplatePage.BodyParameters;
            }
            set
            {
                TemplatePage.BodyParameters = value;
            }
        }


        /// <summary>
        /// Top HTML body node for custom HTML code.
        /// </summary>
        public virtual string BodyScripts
        {
            get
            {
                return TemplatePage.BodyScripts;
            }
            set
            {
                TemplatePage.BodyScripts = value;
            }
        }


        /// <summary>
        /// Css file.
        /// </summary>
        public virtual string CssFile
        {
            get
            {
                return TemplatePage.CssFile;
            }
            set
            {
                TemplatePage.CssFile = value;
            }
        }


        /// <summary>
        /// Extended tags.
        /// </summary>
        public virtual string ExtendedTags
        {
            get
            {
                return TemplatePage.ExtendedTags;
            }
            set
            {
                TemplatePage.ExtendedTags = value;
            }
        }


        /// <summary>
        /// Header tags.
        /// </summary>
        public virtual string HeaderTags
        {
            get
            {
                return TemplatePage.HeaderTags;
            }
        }


        /// <summary>
        /// Body class.
        /// </summary>
        public virtual string BodyClass
        {
            get
            {
                return TemplatePage.BodyClass;
            }
            set
            {
                TemplatePage.BodyClass = value;
            }
        }


        /// <summary>
        /// Additional XML namespace to HTML tag.
        /// </summary>
        public virtual string XmlNamespace
        {
            get
            {
                return TemplatePage.XmlNamespace;
            }
            set
            {
                TemplatePage.XmlNamespace = value;
            }
        }


        /// <summary>
        /// Description.
        /// </summary>
        public virtual string Description
        {
            get
            {
                return TemplatePage.Description;
            }
            set
            {
                TemplatePage.Description = value;
            }
        }


        /// <summary>
        /// Key words.
        /// </summary>
        public virtual string KeyWords
        {
            get
            {
                return TemplatePage.KeyWords;
            }
            set
            {
                TemplatePage.KeyWords = value;
            }
        }


        /// <summary>
        /// Title.
        /// </summary>
        public virtual string Title
        {
            get
            {
                return TemplatePage.PageTitle;
            }
            set
            {
                TemplatePage.PageTitle = value;
            }
        }


        /// <summary>
        /// Container for the debug logs.
        /// </summary>
        public virtual Control LogsContainer
        {
            get
            {
                if (mLogsContainer == null)
                {
                    // Try to use form as the logs container
                    mLogsContainer = Page.Form;
                    if (mLogsContainer == null)
                    {
                        mLogsContainer = this;
                    }
                }

                return mLogsContainer;
            }
            set
            {
                mLogsContainer = value;
            }
        }

        #endregion


        #region "ITimeZoneManager Members"

        /// <summary>
        /// Gets or sets time zone.
        /// </summary>
        public virtual TimeZoneTypeEnum TimeZoneType
        {
            get
            {
                return mTimeZoneType;
            }
            set
            {
                mTimeZoneType = value;
            }
        }


        /// <summary>
        /// Gets or sets user defined time zone if time zone type is "custom".
        /// </summary>
        public virtual TimeZoneInfo CustomTimeZone
        {
            get
            {
                return mCustomTimeZone;
            }
            set
            {
                mCustomTimeZone = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// PreInit event handler.
        /// </summary>
        public virtual void PreInit()
        {
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            // Backwards compatibility
            PreInit();

            base.OnInit(e);

            // Init the debug functionality
            InitDebug();
        }


        /// <summary>
        /// Initializes the debug controls.
        /// </summary>
        public Control InitDebug()
        {
            // Include debug if enabled
            if (DebugHelper.AnyLiveDebugEnabled && (LogsContainer != null))
            {
                Control debug = this.LoadUserControl("~/CMSAdminControls/Debug/Debug.ascx");
                debug.ID = "dbg";

                LogsContainer.Controls.Add(debug);
                return debug;
            }

            return null;
        }


        /// <summary>
        /// Loads the user control based on the given path
        /// </summary>
        /// <param name="controlPath">Control path</param>
        public Control LoadUserControl(string controlPath)
        {
            VirtualPathLog.Log(controlPath);
            return LoadControl(controlPath);
        }

        #endregion
    }
}