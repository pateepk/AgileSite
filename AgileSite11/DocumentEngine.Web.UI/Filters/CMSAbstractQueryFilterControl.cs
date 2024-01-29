using System;

using CMS.DataEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Abstract class for filter controls.
    /// </summary>
    public abstract class CMSAbstractQueryFilterControl : CMSAbstractBaseFilterControl, ICMSQueryProperties
    {
        #region "Variables"

        private int? mPageSize;
        private string mQueryName;
        private bool? mForceCacheMinutes;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether cache minutes must be set manually (Cache minutes value is independent on view mode and cache settings)
        /// </summary>
        public virtual bool ForceCacheMinutes
        {
            get
            {
                return mForceCacheMinutes.Value;
            }
            set
            {
                mForceCacheMinutes = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Query name in format application.class.query.
        /// </summary>
        public virtual string QueryName
        {
            get
            {
                return mQueryName;
            }
            set
            {
                mQueryName = value;
                FilterChanged = true;
            }
        }
        

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public virtual int PageSize
        {
            get
            {
                return mPageSize.Value;
            }
            set
            {
                mPageSize = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Top N rows to select.
        /// </summary>    
        public int SelectTopN
        {
            get
            {
                return TopN;
            }
            set
            {
                TopN = value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Initialize data properties from property object.
        /// </summary>
        /// <param name="properties">Properties object</param>
        public override void InitDataProperties(ICMSBaseProperties properties)
        {
            base.InitDataProperties(properties);

            if (properties is ICMSQueryProperties)
            {
                ICMSQueryProperties queryProperties = (ICMSQueryProperties)properties;

                if (mPageSize != null)
                {
                    queryProperties.PageSize = PageSize;
                }

                if (QueryName != null)
                {
                    queryProperties.QueryName = QueryName;
                }

                if (mForceCacheMinutes != null)
                {
                    queryProperties.ForceCacheMinutes = ForceCacheMinutes;
                }
            }
        }

        #endregion
    }
}