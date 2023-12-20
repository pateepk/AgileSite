using System;
using System.ComponentModel;
using System.Web.UI;

using CMS.DataEngine;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// CMSAbstractBaseProperties class.
    /// </summary>
    public abstract class CMSAbstractBaseProperties : CMSWebControl, ICMSBaseProperties
    {
        #region "Variables"

        /// <summary>
        /// Stop processing this control.
        /// </summary>
        protected bool mStopProcessing = false;

        /// <summary>
        /// Parent control.
        /// </summary>
        protected Control mParent = null;

        /// <summary>
        /// Control context.
        /// </summary>
        protected string mControlContext = null;

        #endregion


        #region "Constructor"

        /// <summary>
        /// BaseProperties constructor.
        /// </summary>
        protected CMSAbstractBaseProperties()
        {
        }


        /// <summary>
        /// Base properties constructor.
        /// </summary>
        /// <param name="tag">Writer tag</param>
        protected CMSAbstractBaseProperties(HtmlTextWriterTag tag)
            : base(tag)
        {
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Parent control.
        /// </summary>
        [Browsable(false)]
        public Control ParentControl
        {
            get
            {
                return mParent;
            }
            set
            {
                mParent = value;
            }
        }


        /// <summary>
        /// Indicates if processing of the control should be stopped and the control should not retrieve or display any data.
        /// </summary>
        /// <remarks>
        /// Stop processing. Use in webparts design mode.
        /// </remarks>
        [Category("Behavior"), DefaultValue(false), Description("Stop processing.")]
        public virtual bool StopProcessing
        {
            get
            {
                return (mStopProcessing & (Context != null));
            }
            set
            {
                mStopProcessing = value;
            }
        }


        /// <summary>
        /// Name of the cache item the control will use.
        /// </summary>
        /// <remarks>
        /// By setting this name dynamically, you can achieve caching based on URL parameter or some other variable - simply put the value of the parameter to the CacheItemName property. If no value is set, the control stores its content to the item named "URL|ControlID".
        /// </remarks>
        [Category("Behavior"), DefaultValue(""), Description("Name of the cache item the control will use.")]
        public virtual string CacheItemName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["CacheItemName"], "");
            }
            set
            {
                ViewState["CacheItemName"] = value;
            }
        }


        /// <summary>
        /// Cache dependencies, each cache dependency on a new line.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Cache dependencies, each cache dependency on a new line.")]
        public virtual string CacheDependencies
        {
            get
            {
                return (string)ViewState["CacheDependencies"];
            }
            set
            {
                ViewState["CacheDependencies"] = value;
            }
        }


        /// <summary>
        /// Number of minutes the retrieved content is cached for. Zero indicates that the content will not be cached.
        /// </summary>
        /// <remarks>
        /// This parameter allows you to set up caching of content so that it's not retrieved from the database each time a user requests the page.
        /// </remarks>
        [Category("Behavior"), DefaultValue(-1), Description("Number of minutes the retrieved content is cached for. Zero indicates that the content will not be cached.")]
        public virtual int CacheMinutes
        {
            get
            {
                // Use cache only for live site
                if (PortalContext.ViewMode != ViewModeEnum.LiveSite)
                {
                    return 0;
                }

                if (ViewState["CacheMinutes"] == null)
                {
                    if (CMSHttpContext.Current == null)
                    {
                        return -1;
                    }
                    ViewState["CacheMinutes"] = SettingsKeyInfoProvider.GetIntValue(SiteName + ".CMSCacheMinutes");
                }

                // If result -1, use global settings
                int minutes = Convert.ToInt32(ViewState["CacheMinutes"]);
                if (minutes == -1)
                {
                    minutes = SettingsKeyInfoProvider.GetIntValue(SiteName + ".CMSCacheMinutes");
                    ViewState["CacheMinutes"] = minutes;
                }

                return minutes;
            }
            set
            {
                ViewState["CacheMinutes"] = value;
            }
        }


        /// <summary>
        /// Site code name.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Property to set and get the SiteName.")]
        public virtual string SiteName
        {
            get
            {
                // Get site name from the view state
                string siteName = (string)ViewState["SiteName"];
                if (siteName == null)
                {
                    // Get site name from the context
                    siteName = SiteContext.CurrentSiteName;
                    ViewState["SiteName"] = siteName;
                }

                return siteName;
            }
            set
            {
                ViewState["SiteName"] = value;
            }
        }


        /// <summary>
        /// WHERE part of the SQL statement.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Where condition.")]
        public virtual string WhereCondition
        {
            get
            {
                return ValidationHelper.GetString(ViewState["WhereCondition"], "");
            }
            set
            {
                ViewState["WhereCondition"] = value;
            }
        }


        /// <summary>
        /// ORDER BY part of the SQL statement.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Order By expression.")]
        public virtual string OrderBy
        {
            get
            {
                return ValidationHelper.GetString(ViewState["OrderBy"], "");
            }
            set
            {
                ViewState["OrderBy"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the columns to be retrieved from database.
        /// </summary>    
        [Category("Behavior"), DefaultValue(""), Description("Gets or sets the columns to be retrieved from database.")]
        public string SelectedColumns
        {
            get
            {
                return ValidationHelper.GetString(ViewState["SelectedColumns"], "");
            }
            set
            {
                ViewState["SelectedColumns"] = value;
            }
        }


        /// <summary>
        /// Select top N rows.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("Select top N rows.")]
        public int TopN
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["TopN"], 0);
            }
            set
            {
                ViewState["TopN"] = value;
            }
        }


        /// <summary>
        /// Overrides the generation of the SPAN tag with custom tag.
        /// </summary>
        public virtual HtmlTextWriterTag ControlTagKey
        {
            get
            {
                if (SettingsKeyInfoProvider.GetValue(SiteName + ".CMSControlElement").ToLowerCSafe().Trim() == "div")
                {
                    return HtmlTextWriterTag.Div;
                }
                return HtmlTextWriterTag.Span;
            }
        }


        /// <summary>
        /// Control context.
        /// </summary>
        public virtual string ControlContext
        {
            get
            {
                return mControlContext;
            }
            set
            {
                mControlContext = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets the control context.
        /// </summary>
        public virtual void SetContext()
        {
            if (mControlContext != null)
            {
                DebugHelper.SetContext(mControlContext);
            }
        }


        /// <summary>
        /// Releases the control context.
        /// </summary>
        public virtual void ReleaseContext()
        {
            if (mControlContext != null)
            {
                DebugHelper.ReleaseContext();
            }
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public virtual string GetDefaultCacheDependencies()
        {
            return null;
        }


        /// <summary>
        /// Gets the cache dependency for the control.
        /// </summary>
        public virtual CMSCacheDependency GetCacheDependency()
        {
            // Process the dependencies
            string dep = CacheHelper.GetCacheDependencies(CacheDependencies, GetDefaultCacheDependencies());

            return CacheHelper.GetCacheDependency(dep);
        }

        #endregion
    }
}