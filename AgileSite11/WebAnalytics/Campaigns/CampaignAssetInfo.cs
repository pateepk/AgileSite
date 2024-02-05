using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(CampaignAssetInfo), CampaignAssetInfo.OBJECT_TYPE)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// CampaignAssetInfo data container class.
    /// </summary>
    [Serializable]
    public class CampaignAssetInfo : AbstractInfo<CampaignAssetInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "analytics.campaignasset";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CampaignAssetInfoProvider), OBJECT_TYPE, "Analytics.CampaignAsset", "CampaignAssetID", "CampaignAssetLastModified", "CampaignAssetGuid", null, null, null, null, "CampaignAssetCampaignID", CampaignInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.WEBANALYTICS,
            TouchCacheDependencies = true,

            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization
            },

            ContinuousIntegrationSettings =
            {
                Enabled = true
            },

            Feature = FeatureEnum.CampaignAndConversions,
            ContainsMacros = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Campaign asset ID
        /// </summary>
        [DatabaseField]
        public virtual int CampaignAssetID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignAssetID"), 0);
            }
            set
            {
                SetValue("CampaignAssetID", value);
            }
        }


        /// <summary>
        /// Campaign asset guid
        /// </summary>
        [DatabaseField]
        public virtual Guid CampaignAssetGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("CampaignAssetGuid"), Guid.Empty);
            }
            set
            {
                SetValue("CampaignAssetGuid", value);
            }
        }


        /// <summary>
        /// Campaign asset last modified
        /// </summary>
        [DatabaseField]
        public virtual DateTime CampaignAssetLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("CampaignAssetLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CampaignAssetLastModified", value);
            }
        }


        /// <summary>
        /// Campaign asset type
        /// </summary>
        [DatabaseField]
        public virtual string CampaignAssetType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CampaignAssetType"), String.Empty);
            }
            set
            {
                SetValue("CampaignAssetType", value);
            }
        }


        /// <summary>
        /// Guid of specific asset (e.g. email or form)
        /// </summary>
        [DatabaseField]
        public virtual Guid CampaignAssetAssetGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("CampaignAssetAssetGuid"), Guid.Empty);
            }
            set
            {
                SetValue("CampaignAssetAssetGuid", value);
            }
        }


        /// <summary>
        /// Campaign asset campaign ID
        /// </summary>
        [DatabaseField]
        public virtual int CampaignAssetCampaignID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignAssetCampaignID"), 0);
            }
            set
            {
                SetValue("CampaignAssetCampaignID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CampaignAssetInfoProvider.DeleteCampaignAssetInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CampaignAssetInfoProvider.SetCampaignAssetInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public CampaignAssetInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty CampaignAssetInfo object.
        /// </summary>
        public CampaignAssetInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CampaignAssetInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public CampaignAssetInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}