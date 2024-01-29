using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;

[assembly: RegisterObjectType(typeof(CultureInfo), CultureInfo.OBJECT_TYPE)]

namespace CMS.Localization
{
    /// <summary>
    /// Culture info data container class.
    /// </summary>
    public class CultureInfo : AbstractInfo<CultureInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.culture";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CultureInfoProvider), OBJECT_TYPE, "CMS.Culture", "CultureID", "CultureLastModified", "CultureGUID", "CultureCode", "CultureName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                },
            },
            DefaultData = new DefaultDataSettings
            {
                ExcludedColumns = new List<string>{ "CultureIsUICulture", "CultureAlias" }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets culture ID.
        /// </summary>
        public int CultureID
        {
            get
            {
                return GetIntegerValue("CultureID", 0);
            }
            set
            {
                SetValue("CultureID", value);
            }
        }


        /// <summary>
        /// Gets or sets culture name.
        /// </summary>
        public string CultureName
        {
            get
            {
                return GetStringValue("CultureName", "");
            }
            set
            {
                SetValue("CultureName", value);
            }
        }


        /// <summary>
        /// Gets or sets culture code.
        /// </summary>
        public string CultureCode
        {
            get
            {
                return GetStringValue("CultureCode", "");
            }
            set
            {
                SetValue("CultureCode", value);
            }
        }


        /// <summary>
        /// Gets or sets culture short name.
        /// </summary>
        public string CultureShortName
        {
            get
            {
                return GetStringValue("CultureShortName", "");
            }
            set
            {
                SetValue("CultureShortName", value);
            }
        }


        /// <summary>
        /// Gets or sets culture alias.
        /// </summary>
        public string CultureAlias
        {
            get
            {
                return GetStringValue("CultureAlias", "");
            }
            set
            {
                SetValue("CultureAlias", value);
            }
        }


        /// <summary>
        /// Indicates that current culture is UI Culture
        /// </summary>
        public bool CultureIsUICulture
        {
            get
            {
                return GetBooleanValue("CultureIsUICulture", false);
            }
            set
            {
                SetValue("CultureIsUICulture", value);
            }
        }


        /// <summary>
        /// Culture GUID.
        /// </summary>
        public virtual Guid CultureGUID
        {
            get
            {
                return GetGuidValue("CultureGUID", Guid.Empty);
            }
            set
            {
                SetValue("CultureGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime CultureLastModified
        {
            get
            {
                return GetDateTimeValue("CultureLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CultureLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CultureInfoProvider.DeleteCultureInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CultureInfoProvider.SetCultureInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty CultureInfo structure.
        /// </summary>
        public CultureInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates the CultureInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Data row with the culture info data</param>
        public CultureInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the where condition to filter out the default installation data
        /// </summary>
        /// <param name="recursive">Indicates whether where condition should contain further dependency conditions.</param>
        /// <param name="globalOnly">Indicates whether only objects with null in their site ID column should be included.</param>
        /// <param name="excludedNames">Objects with display names and code names starting with these expressions are filtered out.</param>
        protected override string GetDefaultDataWhereCondition(bool recursive = true, bool globalOnly = true, IEnumerable<string> excludedNames = null)
        {
            return AddColumnPrefixesWhereCondition(base.GetDefaultDataWhereCondition(recursive, globalOnly, excludedNames), "CultureShortName", excludedNames);
        }


        /// <summary>
        /// Returns the default object installation data
        /// </summary>
        /// <remarks>
        /// Only en-US culture is marked as UI culture for the default data.
        /// </remarks>
        /// <param name="excludedNames">Objects with display names and code names starting with these expressions are filtered out.</param>
        protected override DataSet GetDefaultData(IEnumerable<string> excludedNames = null)
        {
            var data = base.GetDefaultData(excludedNames);

            if (!DataHelper.DataSourceIsEmpty(data))
            {
                var table = data.Tables[0];

                const string UI_COLUMN_NAME = "CultureIsUICulture";

                DataHelper.EnsureColumn(table, UI_COLUMN_NAME, typeof(bool));

                foreach (DataRow row in table.Rows)
                {
                    if (row["CultureCode"].Equals("en-US"))
                    {
                        // UI is only localized to en-US out of the box.
                        row[UI_COLUMN_NAME] = true;
                        break;
                    }
                }
            }

            return data;
        }

        #endregion
    }
}