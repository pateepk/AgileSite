using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.SocialMarketing;

[assembly: RegisterObjectType(typeof(InsightInfo), InsightInfo.OBJECT_TYPE)]

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents an insight for social marketing.
    /// </summary>
    public class InsightInfo : AbstractInfo<InsightInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "sm.insight";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(InsightInfoProvider), OBJECT_TYPE, "SM.Insight", "InsightID", null, null, null, "InsightCodeName", null, null, null, null)
        {
            TouchCacheDependencies = true,
            SupportsCloning = false,
            SynchronizationSettings = 
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None,
            },
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None },
            Feature = FeatureEnum.SocialMarketingInsights
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the code name of the Insight.
        /// </summary>
        [DatabaseField]
        public virtual string InsightCodeName
        {
            get
            {
                return GetStringValue("InsightCodeName", String.Empty);
            }
            set
            {
                SetValue("InsightCodeName", value);
            }
        }


        /// <summary>
        /// Gets or sets the external ID of the Insight.
        /// </summary>
        [DatabaseField]
        public virtual string InsightExternalID
        {
            get
            {
                return GetStringValue("InsightExternalID", String.Empty);
            }
            set
            {
                SetValue("InsightExternalID", value);
            }
        }


        /// <summary>
        /// Gets or sets the value name of the Insight.
        /// </summary>
        [DatabaseField]
        public virtual string InsightValueName
        {
            get
            {
                return GetStringValue("InsightValueName", String.Empty);
            }
            set
            {
                SetValue("InsightValueName", value);
            }
        }


        /// <summary>
        /// Gets or sets the period typeof the Insight.
        /// </summary>
        [DatabaseField]
        public virtual string InsightPeriodType
        {
            get
            {
                return GetStringValue("InsightPeriodType", String.Empty);
            }
            set
            {
                SetValue("InsightPeriodType", value);
            }
        }

        #endregion


        #region "System properties"

        /// <summary>
        /// Gets or sets the identifier of the Insight.
        /// </summary>
        [DatabaseField]
        public virtual int InsightID
        {
            get
            {
                return GetIntegerValue("InsightID", 0);
            }
            set
            {
                SetValue("InsightID", value);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the InsightInfo class.
        /// </summary>
        public InsightInfo() : base(TYPEINFO)
        {

        }


        /// <summary>
        /// Initializes a new instance of the InsightInfo class with the specified data.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public InsightInfo(DataRow dr) : base(TYPEINFO, dr)
        {

        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Updates this object.
        /// </summary>
        protected override void SetObject()
        {
            InsightInfoProvider.SetInsight(this);
        }

        #endregion
    }
}
