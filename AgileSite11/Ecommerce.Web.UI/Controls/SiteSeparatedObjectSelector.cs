using System;

using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;
using CMS.DataEngine;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Abstract class providing common functionality for selectors offering site-separated objects.
    /// </summary>
    public abstract class SiteSeparatedObjectSelector : BaseObjectSelector
    {
        #region "Variables"

        private bool? mDisplayGlobalItems;
        private bool? mDisplaySiteItems;
        private int mSiteId = -1;

        #endregion


        #region "Site/global objects handling properties"

        /// <summary>
        /// Indicates whether global objects are to be offered.
        /// </summary>
        public virtual bool IncludeGlobalItems
        {
            get
            {
                int siteId = (SiteID > 0) ? SiteID : CurrentSite.SiteID;
                return ECommerceSettings.AllowGlobalObjects(siteId, ObjectType);
            }
        }


        /// <summary>
        /// Indicates whether site records are to be offered. Default value is based on IncludeGlobalItems property.
        /// </summary>
        public virtual bool DisplaySiteItems
        {
            get
            {
                if (!mDisplaySiteItems.HasValue)
                {
                    mDisplaySiteItems = AllowCombineSiteAndGlobal || !IncludeGlobalItems;
                }

                return mDisplaySiteItems.Value;
            }
            set
            {
                mDisplaySiteItems = value;
            }
        }


        /// <summary>
        /// Indicates whether site records are to be offered. Default value is based on IncludeGlobalItems property.
        /// </summary>
        public virtual bool DisplayGlobalItems
        {
            get
            {
                if (!mDisplayGlobalItems.HasValue)
                {
                    mDisplayGlobalItems = IncludeGlobalItems;
                }

                return mDisplayGlobalItems.Value;
            }
            set
            {
                mDisplayGlobalItems = value;
            }
        }


        /// <summary>
        /// Allows to display records only for specified site ID. Use 0 for global objects. Default value is current site ID.
        /// </summary>
        public override int SiteID
        {
            get
            {
                if (mSiteId >= 0)
                {
                    return mSiteId;
                }

                // Try to get the value from form data
                if (Form != null)
                {
                    // Special case for SKUTreeNode
                    if (Form.Data.ContainsColumn("SKUSiteID"))
                    {
                        int siteId = ValidationHelper.GetInteger(Form.Data.GetValue("SKUSiteID"), 0);
                        if (siteId >= 0)
                        {
                            return siteId;
                        }
                    }

                    // Get ID of the site from editing form
                    UIForm uiForm = Form as UIForm;
                    BaseInfo info = Form.Data as BaseInfo;
                    if (uiForm != null)
                    {
                        if ((info != null) && (info.TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
                        {
                            return ValidationHelper.GetInteger(uiForm.GetFieldValue(info.TypeInfo.SiteIDColumn), 0);
                        }

                        return uiForm.ObjectSiteID;
                    }

                    // Get ID of the site from edited object
                    if ((info != null) && (info.TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
                    {
                        mSiteId = info.Generalized.ObjectSiteID;
                        return mSiteId;
                    }

                    // Get site from SiteName field - usable for webpart properties
                    if (Form.Data.ColumnNames.Contains("SiteName"))
                    {
                        string siteName = ValidationHelper.GetString(Form.GetFieldValue("SiteName"), null);
                        if (!String.IsNullOrEmpty(siteName))
                        {
                            SiteInfo siteObj = SiteInfoProvider.GetSiteInfo(siteName);
                            if (siteObj != null)
                            {
                                mSiteId = siteObj.SiteID;
                                return mSiteId;
                            }
                        }
                    }
                }

                // Use current site ID
                mSiteId = SiteContext.CurrentSiteID;
                return mSiteId;
            }
            set
            {
                mSiteId = value;
                mDisplayGlobalItems = null;
                mDisplaySiteItems = null;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the selector.
        /// </summary>
        protected override void InitSelector()
        {
            base.InitSelector();

            // Ensure new global object create 
            if (AllowCreate && (SiteID <= 0) && IncludeGlobalItems)
            {
                UniSelector.NewItemPageUrl = GetNewItemUrl();
            }
        }

        #endregion


        #region "Where condition generation"

        /// <summary>
        /// Appends restrictive part of where condition to given where condition.
        /// </summary>
        protected override string AppendExclusiveWhere(string where)
        {
            // Restrict by site and/or global flag
            where = AppendSiteWhere(where);

            return base.AppendExclusiveWhere(where);
        }


        /// <summary>
        /// Appends site where to given where condition.
        /// </summary>
        /// <param name="where">Original where condition to append site where to.</param>
        protected virtual string AppendSiteWhere(string where)
        {
            var siteWhere = new WhereCondition();

            if (SiteID > 0)
            {
                // Filter site items
                if (DisplaySiteItems)
                {
                    siteWhere.WhereEquals(SiteIDColumn, SiteID);
                }

                // Filter global items
                if (DisplayGlobalItems)
                {
                    siteWhere.Or(w => w.WhereNull(SiteIDColumn));
                }

                // Select nothing when nothing requested
                if (!DisplaySiteItems && !DisplayGlobalItems)
                {
                    siteWhere.NoResults();
                }
            }
            else
            {
                // Show global items when no site specified
                siteWhere.WhereNull(SiteIDColumn);
            }

            return new WhereCondition(where).Where(siteWhere).ToString(true);
        }

        #endregion
    }
}
