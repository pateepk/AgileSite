using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using CMS.Core;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(CampaignObjectiveInfo), CampaignObjectiveInfo.OBJECT_TYPE)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// CampaignObjectiveInfo data container class.
    /// </summary>
	[Serializable]
    public class CampaignObjectiveInfo : AbstractInfo<CampaignObjectiveInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "analytics.campaignobjective";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CampaignObjectiveInfoProvider), OBJECT_TYPE, "Analytics.CampaignObjective", "CampaignObjectiveID", "CampaignObjectiveLastModified", "CampaignObjectiveGuid", null, null, null, null, "CampaignObjectiveCampaignID", CampaignInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.WEBANALYTICS,

            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("CampaignObjectiveCampaignConversionID", CampaignConversionInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization
            },

            ContinuousIntegrationSettings =
            {
                Enabled = true,
            },

            Feature = FeatureEnum.CampaignAndConversions,
            SupportsCloning = false,
            ContainsMacros = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Campaign objective ID
        /// </summary>
        [DatabaseField]
        public virtual int CampaignObjectiveID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignObjectiveID"), 0);
            }
            set
            {
                SetValue("CampaignObjectiveID", value);
            }
        }


        /// <summary>
        /// Campaign objective campaign ID references <see cref="CampaignInfo"/> object.
        /// </summary>
        [DatabaseField]
        public virtual int CampaignObjectiveCampaignID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignObjectiveCampaignID"), 0);
            }
            set
            {
                SetValue("CampaignObjectiveCampaignID", value);
            }
        }


        /// <summary>
        /// Campaign objective campaign conversion ID references <see cref="CampaignConversionInfo"/> object.
        /// </summary>
        [DatabaseField]
        public virtual int CampaignObjectiveCampaignConversionID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignObjectiveCampaignConversionID"), 0);
            }
            set
            {
                SetValue("CampaignObjectiveCampaignConversionID", value);
            }
        }


        /// <summary>
        /// Campaign objective value
        /// </summary>
        [DatabaseField]
        public virtual int CampaignObjectiveValue
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignObjectiveValue"), 0);
            }
            set
            {
                SetValue("CampaignObjectiveValue", value, 0);
            }
        }


        /// <summary>
        /// Campaign objective guid
        /// </summary>
        [DatabaseField]
        public virtual Guid CampaignObjectiveGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("CampaignObjectiveGuid"), Guid.Empty);
            }
            set
            {
                SetValue("CampaignObjectiveGuid", value);
            }
        }


        /// <summary>
        /// Campaign objective last modified
        /// </summary>
        [DatabaseField]
        public virtual DateTime CampaignObjectiveLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("CampaignObjectiveLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CampaignObjectiveLastModified", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CampaignObjectiveInfoProvider.DeleteCampaignObjectiveInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CampaignObjectiveInfoProvider.SetCampaignObjectiveInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected CampaignObjectiveInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty CampaignObjectiveInfo object.
        /// </summary>
        public CampaignObjectiveInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CampaignObjectiveInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public CampaignObjectiveInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}