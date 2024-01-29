using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Globalization;

[assembly: RegisterObjectType(typeof(StateInfo), StateInfo.OBJECT_TYPE)]

namespace CMS.Globalization
{
    /// <summary>
    /// StateInfo data container class.
    /// </summary>
    public class StateInfo : AbstractInfo<StateInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.state";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(StateInfoProvider), OBJECT_TYPE, "CMS.State", "StateID", "StateLastModified", "StateGUID", "StateName", "StateDisplayName", null, null, "CountryID", CountryInfo.OBJECT_TYPE)
        {
            LogEvents = true,
            TouchCacheDependencies = true,
            CheckDependenciesOnDelete = true,
            CheckPermissions = false,
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.None
            },
            ContinuousIntegrationSettings = 
            {
                Enabled = true
            },
            DefaultData = new DefaultDataSettings()
        };

        #endregion


        #region "Public properties"

        /// <summary>
        /// State code name.
        /// </summary>
        [DatabaseField]
        public virtual string StateName
        {
            get
            {
                return GetStringValue("StateName", "");
            }
            set
            {
                SetValue("StateName", value);
            }
        }


        /// <summary>
        /// State short code.
        /// </summary>
        [DatabaseField]
        public virtual string StateCode
        {
            get
            {
                return GetStringValue("StateCode", "");
            }
            set
            {
                SetValue("StateCode", value);
            }
        }


        /// <summary>
        /// State display name.
        /// </summary>
        [DatabaseField]
        public virtual string StateDisplayName
        {
            get
            {
                return GetStringValue("StateDisplayName", "");
            }
            set
            {
                SetValue("StateDisplayName", value);
            }
        }


        /// <summary>
        /// Country ID.
        /// </summary>
        [DatabaseField]
        public virtual int CountryID
        {
            get
            {
                return GetIntegerValue("CountryID", 0);
            }
            set
            {
                SetValue("CountryID", value);
            }
        }


        /// <summary>
        /// State ID.
        /// </summary>
        [DatabaseField]
        public virtual int StateID
        {
            get
            {
                return GetIntegerValue("StateID", 0);
            }
            set
            {
                SetValue("StateID", value);
            }
        }


        /// <summary>
        /// State GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid StateGUID
        {
            get
            {
                return GetGuidValue("StateGUID", Guid.Empty);
            }
            set
            {
                SetValue("StateGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime StateLastModified
        {
            get
            {
                return GetDateTimeValue("StateLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("StateLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            StateInfoProvider.DeleteStateInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            StateInfoProvider.SetStateInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty StateInfo object.
        /// </summary>
        public StateInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new StateInfo object from the given DataRow.
        /// </summary>
        public StateInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}