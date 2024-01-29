using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Globalization;

[assembly: RegisterObjectType(typeof(CountryInfo), CountryInfo.OBJECT_TYPE)]

namespace CMS.Globalization
{
    /// <summary>
    /// CountryInfo data container class.
    /// </summary>
    public class CountryInfo : AbstractInfo<CountryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.country";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CountryInfoProvider), OBJECT_TYPE, "CMS.Country", "CountryID", "CountryLastModified", "CountryGUID", "CountryName", "CountryDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                }
            },

            CheckDependenciesOnDelete = true,

            LogEvents = true,
            TouchCacheDependencies = true,
            CheckPermissions = false,
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                },
            },
            ContinuousIntegrationSettings = 
            {
                Enabled = true
            },
            DefaultData = new DefaultDataSettings()
        };

        #endregion


        #region "Properties"

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
        /// Country display name.
        /// </summary>
        [DatabaseField]
        public virtual string CountryDisplayName
        {
            get
            {
                return GetStringValue("CountryDisplayName", "");
            }
            set
            {
                SetValue("CountryDisplayName", value);
            }
        }


        /// <summary>
        /// Country code name.
        /// </summary>
        [DatabaseField]
        public virtual string CountryName
        {
            get
            {
                return GetStringValue("CountryName", "");
            }
            set
            {
                SetValue("CountryName", value);
            }
        }


        /// <summary>
        /// Country two-letter code.
        /// </summary>
        [DatabaseField]
        public virtual string CountryTwoLetterCode
        {
            get
            {
                return GetStringValue("CountryTwoLetterCode", "");
            }
            set
            {
                SetValue("CountryTwoLetterCode", value);
            }
        }


        /// <summary>
        /// Country three-letter code.
        /// </summary>
        [DatabaseField]
        public virtual string CountryThreeLetterCode
        {
            get
            {
                return GetStringValue("CountryThreeLetterCode", "");
            }
            set
            {
                SetValue("CountryThreeLetterCode", value);
            }
        }


        /// <summary>
        /// Country GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid CountryGUID
        {
            get
            {
                return GetGuidValue("CountryGUID", Guid.Empty);
            }
            set
            {
                SetValue("CountryGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CountryLastModified
        {
            get
            {
                return GetDateTimeValue("CountryLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CountryLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CountryInfoProvider.DeleteCountryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CountryInfoProvider.SetCountryInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty CountryInfo object.
        /// </summary>
        public CountryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CountryInfo object from the given DataRow.
        /// </summary>
        public CountryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                this.CountryTwoLetterCode = ValidationHelper.GetString(p["cms.country" + ".twolettercode"], "");
                this.CountryThreeLetterCode = ValidationHelper.GetString(p["cms.country" + ".threelettercode"], "");
            }

            this.Insert();
        }

        #endregion
    }
}