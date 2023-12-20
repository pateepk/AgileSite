using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;


[assembly: RegisterObjectType(typeof(MacroIdentityInfo), MacroIdentityInfo.OBJECT_TYPE)]

namespace CMS.MacroEngine
{
    /// <summary>
    /// MacroIdentityInfo data container class.
    /// </summary>
	[Serializable]
    public class MacroIdentityInfo : AbstractInfo<MacroIdentityInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.macroidentity";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MacroIdentityInfoProvider), OBJECT_TYPE, "CMS.MacroIdentity", "MacroIdentityID", "MacroIdentityLastModified", "MacroIdentityGuid", "MacroIdentityName", null, null, null, null, null)
        {
            ModuleName = "CMS.MacroEngine",
            TouchCacheDependencies = true,
            ContainsMacros = false,
            DefaultData = new DefaultDataSettings(),
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("MacroIdentityEffectiveUserID", PredefinedObjectType.USER, ObjectDependencyEnum.NotRequired),
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            ImportExportSettings =
            {
                LogExport = true,
                AllowSingleExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                }
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                },
                ExcludedStagingColumns = new List<string>
                {
                    "MacroIdentityEffectiveUserID"
                }
            },
            LogEvents = true,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Macro identity ID
        /// </summary>
		[DatabaseField]
        public virtual int MacroIdentityID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MacroIdentityID"), 0);
            }
            set
            {
                SetValue("MacroIdentityID", value);
            }
        }


        /// <summary>
        /// Macro identity name
        /// </summary>
		[DatabaseField]
        public virtual string MacroIdentityName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("MacroIdentityName"), String.Empty);
            }
            set
            {
                SetValue("MacroIdentityName", value);
            }
        }


        /// <summary>
        /// Macro identity guid
        /// </summary>
		[DatabaseField]
        public virtual Guid MacroIdentityGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("MacroIdentityGuid"), Guid.Empty);
            }
            set
            {
                SetValue("MacroIdentityGuid", value);
            }
        }


        /// <summary>
        /// Macro identity effective user ID
        /// </summary>
		[DatabaseField]
        public virtual int MacroIdentityEffectiveUserID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MacroIdentityEffectiveUserID"), 0);
            }
            set
            {
                SetValue("MacroIdentityEffectiveUserID", value, 0);
            }
        }


        /// <summary>
        /// Macro identity last modified
        /// </summary>
		[DatabaseField]
        public virtual DateTime MacroIdentityLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("MacroIdentityLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MacroIdentityLastModified", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MacroIdentityInfoProvider.DeleteMacroIdentityInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MacroIdentityInfoProvider.SetMacroIdentityInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected MacroIdentityInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty MacroIdentityInfo object.
        /// </summary>
        public MacroIdentityInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MacroIdentityInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public MacroIdentityInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}