using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WebAnalytics;
using CMS.Core;
using CMS.SiteProvider;

[assembly: RegisterObjectType(typeof(CampaignConversionInfo), CampaignConversionInfo.OBJECT_TYPE)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// CampaignConversionInfo data container class.
    /// </summary>
	[Serializable]
    public class CampaignConversionInfo : AbstractInfo<CampaignConversionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "analytics.campaignconversion";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CampaignConversionInfoProvider), OBJECT_TYPE, "Analytics.CampaignConversion", "CampaignConversionID", "CampaignConversionLastModified", "CampaignConversionGuid", "CampaignConversionName", "CampaignConversionDisplayName", null, null, "CampaignConversionCampaignID", CampaignInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.WEBANALYTICS,
            TouchCacheDependencies = true,

            LogEvents = true,
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ExcludedStagingColumns = new List<string>
                {
                    "CampaignConversionHits"
                }
            },

            ContinuousIntegrationSettings =
            {
                Enabled = true,
            },

            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "CampaignConversionHits"
                }
            },

            Feature = FeatureEnum.CampaignAndConversions,
            OrderColumn = "CampaignConversionOrder",
            SupportsCloning = false,
            ContainsMacros = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Campaign conversion ID
        /// </summary>
        [DatabaseField]
        public virtual int CampaignConversionID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignConversionID"), 0);
            }
            set
            {
                SetValue("CampaignConversionID", value);
            }
        }


        /// <summary>
        /// Campaign conversion guid
        /// </summary>
        [DatabaseField]
        public virtual Guid CampaignConversionGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("CampaignConversionGuid"), Guid.Empty);
            }
            set
            {
                SetValue("CampaignConversionGuid", value);
            }
        }


        /// <summary>
        /// Campaign conversion last modified
        /// </summary>
        [DatabaseField]
        public virtual DateTime CampaignConversionLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("CampaignConversionLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CampaignConversionLastModified", value);
            }
        }


        /// <summary>
        /// Campaign conversion display name
        /// </summary>
        [DatabaseField]
        public virtual string CampaignConversionDisplayName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CampaignConversionDisplayName"), String.Empty);
            }
            set
            {
                SetValue("CampaignConversionDisplayName", value);
            }
        }


        /// <summary>
        /// Campaign conversion page visit URL. Used if a campaign is running on content only site <see cref="SiteInfo.SiteIsContentOnly"/>.
        /// </summary>
        [DatabaseField]
        public virtual string CampaignConversionURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CampaignConversionURL"), null);
            }
            set
            {
                SetValue("CampaignConversionURL", value, !string.IsNullOrEmpty(value));
            }
        }


        /// <summary>
        /// Campaign conversion name
        /// </summary>
        [DatabaseField]
        public virtual string CampaignConversionName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CampaignConversionName"), String.Empty);
            }
            set
            {
                SetValue("CampaignConversionName", value);
            }
        }


        /// <summary>
        /// Campaign conversion activity type code name
        /// </summary>
        [DatabaseField]
        public virtual string CampaignConversionActivityType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CampaignConversionActivityType"), String.Empty);
            }
            set
            {
                SetValue("CampaignConversionActivityType", value);
            }
        }


        /// <summary>
        /// Campaign conversion detail item ID. For example nodeID for <c>pagevisit</c> <see cref="CampaignConversionActivityType"/>.
        /// </summary>
        [DatabaseField]
        public virtual int CampaignConversionItemID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignConversionItemID"), 0);
            }
            set
            {
                SetValue("CampaignConversionItemID", value, value > 0);
            }
        }


        /// <summary>
        /// Campaign conversion campaign ID
        /// </summary>
        [DatabaseField]
        public virtual int CampaignConversionCampaignID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignConversionCampaignID"), 0);
            }
            set
            {
                SetValue("CampaignConversionCampaignID", value);
            }
        }

        
        /// <summary>
        /// Campaign conversion represents step in campaign journey, i.e. conversion required to reach desired conversion.
        /// </summary>
        [DatabaseField]
        public virtual bool CampaignConversionIsFunnelStep
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("CampaignConversionIsFunnelStep"), false);
            }
            set
            {
                SetValue("CampaignConversionIsFunnelStep", value);
            }
        }


        /// <summary>
        /// Campaign conversion order
        /// </summary>
        [DatabaseField]
        public virtual int CampaignConversionOrder
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignConversionOrder"), 0);
            }
            set
            {
                SetValue("CampaignConversionOrder", value);
            }
        }


        /// <summary>
        /// Campaign conversion hits
        /// </summary>
        [DatabaseField]
        public virtual int CampaignConversionHits
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignConversionHits"), 0);
            }
            set
            {
                SetValue("CampaignConversionHits", value);
            }
        }


        /// <summary>
        /// Campaign conversion value
        /// </summary>
        [DatabaseField]
        public virtual double CampaignConversionValue
        {
            get
            {
                return ValidationHelper.GetDouble(GetValue("CampaignConversionValue"), 0);
            }
            set
            {
                SetValue("CampaignConversionValue", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CampaignConversionInfoProvider.DeleteCampaignConversionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CampaignConversionInfoProvider.SetCampaignConversionInfo(this);
        }


        /// <summary>
        /// Loads the default object data
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            CampaignConversionHits = 0;
            CampaignConversionValue = 0;
            CampaignConversionIsFunnelStep = false;
        }


        /// <summary>
        /// Creates where condition according CampaignConversionIsFunnelStep field.
        /// Used to separate order for main conversions and funnel steps.
        /// </summary>
        protected override WhereCondition GetSiblingsWhereCondition()
        {
            return base.GetSiblingsWhereCondition().WhereEquals("CampaignConversionIsFunnelStep", CampaignConversionIsFunnelStep);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public CampaignConversionInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty CampaignConversionInfo object.
        /// </summary>
        public CampaignConversionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CampaignConversionInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public CampaignConversionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
