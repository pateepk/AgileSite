using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Core;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(ExitPageInfo), ExitPageInfo.OBJECT_TYPE)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// ExitPageInfo data container class.
    /// </summary>
    public class ExitPageInfo : AbstractInfo<ExitPageInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "analytics.exitpage";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ExitPageInfoProvider), OBJECT_TYPE, "analytics.exitpage", null, null, null, null, null, null, null, null, null)
        {
            ModuleName = ModuleName.WEBANALYTICS,
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false, IsExportable = false },
            ContainsMacros = false,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Session identificator.
        /// </summary>
        public virtual string SessionIdentificator
        {
            get
            {
                return GetStringValue("SessionIdentificator", "");
            }
            set
            {
                SetValue("SessionIdentificator", value);
            }
        }


        /// <summary>
        /// Exit page node ID.
        /// </summary>
        public virtual int ExitPageNodeID
        {
            get
            {
                return GetIntegerValue("ExitPageNodeID", 0);
            }
            set
            {
                SetValue("ExitPageNodeID", value);
            }
        }


        /// <summary>
        /// Exit page last modified.
        /// </summary>
        public virtual DateTime ExitPageLastModified
        {
            get
            {
                return GetDateTimeValue("ExitPageLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ExitPageLastModified", value);
            }
        }


        /// <summary>
        /// Exit page site ID.
        /// </summary>
        public virtual int ExitPageSiteID
        {
            get
            {
                return GetIntegerValue("ExitPageSiteID", 0);
            }
            set
            {
                SetValue("ExitPageSiteID", value);
            }
        }


        /// <summary>
        /// Exit page culture.
        /// </summary>
        public virtual string ExitPageCulture
        {
            get
            {
                return GetStringValue("ExitPageCulture", "");
            }
            set
            {
                SetValue("ExitPageCulture", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ExitPageInfoProvider.DeleteExitPageInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ExitPageInfoProvider.SetExitPageInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ExitPageInfo object.
        /// </summary>
        public ExitPageInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ExitPageInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ExitPageInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}