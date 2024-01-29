using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WebAnalytics;
using CMS.Core;

[assembly: RegisterObjectType(typeof(CampaignConversionHitsInfo), CampaignConversionHitsInfo.OBJECT_TYPE)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// CampaignConversionHitsInfo data container class.
    /// </summary>
	[Serializable]
    public class CampaignConversionHitsInfo : AbstractInfo<CampaignConversionHitsInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "analytics.campaignconversionhits";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CampaignConversionHitsInfoProvider), OBJECT_TYPE, "Analytics.CampaignConversionHits", "CampaignConversionHitsID", null, null, null, null, null, null, "CampaignConversionHitsConversionID", CampaignConversionInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.WEBANALYTICS,
            SupportsVersioning = false,

            ImportExportSettings = {
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                LogExport = false
            },

            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },

            Feature = FeatureEnum.CampaignAndConversions,
            SupportsCloning = false,
            LogEvents = false,
            AllowRestore = false,
            ContainsMacros = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Campaign conversion hits ID
        /// </summary>
        [DatabaseField]
        public virtual int CampaignConversionHitsID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignConversionHitsID"), 0);
            }
            set
            {
                SetValue("CampaignConversionHitsID", value);
            }
        }


        /// <summary>
        /// Campaign conversion hits conversion ID
        /// </summary>
        [DatabaseField]
        public virtual int CampaignConversionHitsConversionID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignConversionHitsConversionID"), 0);
            }
            set
            {
                SetValue("CampaignConversionHitsConversionID", value);
            }
        }


        /// <summary>
        /// Campaign conversion hits count
        /// </summary>
        [DatabaseField]
        public virtual int CampaignConversionHitsCount
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignConversionHitsCount"), 0);
            }
            set
            {
                SetValue("CampaignConversionHitsCount", value);
            }
        }


        /// <summary>
        /// Campaign conversion hits source name
        /// </summary>
        [DatabaseField]
        public virtual string CampaignConversionHitsSourceName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CampaignConversionHitsSourceName"), String.Empty);
            }
            set
            {
                SetValue("CampaignConversionHitsSourceName", value);
            }
        }


        /// <summary>
        /// Campaign conversion hits content name
        /// </summary>
        [DatabaseField]
        public virtual string CampaignConversionHitsContentName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CampaignConversionHitsContentName"), String.Empty);
            }
            set
            {
                SetValue("CampaignConversionHitsContentName", value, !String.IsNullOrEmpty(value));
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CampaignConversionHitsInfoProvider.DeleteCampaignConversionHitsInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CampaignConversionHitsInfoProvider.SetCampaignConversionHitsInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected CampaignConversionHitsInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty CampaignConversionHitsInfo object.
        /// </summary>
        public CampaignConversionHitsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CampaignConversionHitsInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public CampaignConversionHitsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
