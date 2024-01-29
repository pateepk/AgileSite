using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.Localization;

[assembly: RegisterObjectType(typeof(ResourceStringInfo), ResourceStringInfo.OBJECT_TYPE)]

namespace CMS.Localization
{
    /// <summary>
    /// Resource info data container class.
    /// </summary>
    public class ResourceStringInfo : ResourceStringInfoBase<ResourceStringInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.resourcestring";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ResourceStringInfoProvider), OBJECT_TYPE, "CMS.ResourceString", "StringID", null, "StringGUID", "StringKey", ObjectTypeInfo.COLUMN_NAME_UNKNOWN, null, null, null, null)
        {
            SynchronizationSettings = 
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            AllowRestore = false,
            SupportsCloning = false,
            ImportExportSettings =
            {
                LogExport = true,
                AllowSingleExport = false,
                IsExportable = true,
                WhereCondition = "StringIsCustom = 1",
                OrderBy = "StringKey",
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
            },
            ContainsMacros = false,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion

        
        #region "Properties"
        
        /// <summary>
        /// The translation text.
        /// </summary>
        public string TranslationText
        {
            get;
            set;
        }


        /// <summary>
        /// The UICulture code.
        /// </summary>
        public string CultureCode
        {
            get;
            set;
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Retrieves value of the specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Retrieved column value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetValue(string columnName, out object value)
        {
            bool result = true;

            switch (columnName.ToLowerInvariant())
            {
                case "translationtext":
                    value = TranslationText;
                    break;

                case "culturecode":
                    value = CultureCode;
                    break;

                default:
                    result = base.TryGetValue(columnName, out value);
                    break;
            }

            return result;
        }


        /// <summary>
        /// Sets value to the specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public override bool SetValue(string columnName, object value)
        {
            bool result = true;

            switch (columnName.ToLowerInvariant())
            {
                case "translationtext":
                    TranslationText = value.ToString(null);
                    break;

                case "culturecode":
                    CultureCode = value.ToString(null);
                    break;

                default:
                    result = base.SetValue(columnName, value);
                    break;
            }

            return result;
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ResourceStringInfoProvider.DeleteResourceStringInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ResourceStringInfoProvider.SetResourceStringInfo(this);
        }


        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            // Build the list of column names
            var names = new List<string>(base.GetColumnNames());

            names.AddRange(new[] { "TranslationText", "CultureCode" });

            return names;
        }


        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            StringIsCustom = false;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty ResourceStringInfo structure.
        /// </summary>
        public ResourceStringInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates an ResourceStringInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Data row with the class info data</param>
        public ResourceStringInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
        

        /// <summary>
        /// Loads the object data from given data container.
        /// </summary>
        /// <param name="settings">Data settings</param>
        protected override void LoadData(LoadDataSettings settings)
        {
            var data = settings.Data;

            base.LoadData(settings);

            // If no data given, create blank class info
            if (data != null)
            {
                if (data.ContainsColumn("TranslationText"))
                {
                    TranslationText = data.GetValue("TranslationText").ToString();
                }
                if (data.ContainsColumn("CultureCode"))
                {
                    CultureCode = data.GetValue("CultureCode").ToString();
                }
            }
        }
        
        #endregion
    }
}