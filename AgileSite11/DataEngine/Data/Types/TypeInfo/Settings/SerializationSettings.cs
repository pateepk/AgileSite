using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.DataEngine
{
    /// <summary>
    /// Determines how the system serializes object fields from the database into XML files. Represents serialization settings in the type information of objects.
    /// </summary>
    /// <remarks>
    /// Use in the <see cref="ObjectTypeInfo.SerializationSettings"/> property of ObjectTypeInfo.
    /// </remarks>
    public class SerializationSettings
    {
        #region "Private variables"

        // Private search-optimized excluded field collection
        private readonly HashSet<string> mExcludedFieldNames;

        // Private search-optimized structured field collection
        private Dictionary<string, IStructuredField> mStructuredFields;

        /// <summary>
        /// Collection of delegates of columns that are excluded by default when using <see cref="SerializationSettings"/>.
        /// </summary>
        /// <remarks>This collection is not meant for public use.</remarks>
        internal static readonly IEnumerable<Func<ObjectTypeInfo, string>> DEFAULT_EXCLUDED_COLUMNS_DELEGATES = new Func<ObjectTypeInfo, string>[]
        {
            typeInfo => typeInfo.IDColumn,
            typeInfo => typeInfo.TimeStampColumn,
            typeInfo => typeInfo.VersionGUIDColumn,
            typeInfo => typeInfo.BinaryColumn,
            typeInfo => typeInfo.SearchContentColumn,
            typeInfo => typeInfo.ObjectLevelColumn,
            typeInfo => typeInfo.ObjectNamePathColumn,
            typeInfo => typeInfo.ObjectIDPathColumn
        };

        #endregion


        #region "Public Fields"

        /// <summary>
        /// Lists fields that contain structured data and cannot directly be serialized as inner text values of XML elements.
        /// Maps an IStructuredField implementation to every listed field, which defines how the serialization occurs.
        /// </summary>
        public IEnumerable<IStructuredField> StructuredFields
        {
            set
            {
                mStructuredFields = (value ?? Enumerable.Empty<IStructuredField>())
                    .ToDictionary(field => field.FieldName, field => field, StringComparer.InvariantCultureIgnoreCase);
            }
        }


        /// <summary>
        /// List of field names that are excluded when serializing objects.
        /// </summary>
        /// <remarks>
        /// By default, the system automatically excludes the object type's IDColumn, TimeStampColumn, VersionGUIDColumn and BinaryColumn.
        /// </remarks>
        /// <remarks>Collection is supposed to contain case insensitive values – when checking field name presence, preferably use <see cref="IsExcludedField"/> method.</remarks>
        public ICollection<string> ExcludedFieldNames
        {
            get
            {
                return mExcludedFieldNames;
            }
        }


        /// <summary>
        /// Predicate for excluded field selection.
        /// </summary>
        /// <remarks>
        /// In case the list in <see cref="ExcludedFieldNames"/> is not sufficient and additional exclusion logic is required.
        /// </remarks>
        public Func<string, bool> AdditionalFieldFilter
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="typeInfo">Related type information</param>
        public SerializationSettings(ObjectTypeInfo typeInfo)
        {
            mStructuredFields = new Dictionary<string, IStructuredField>(StringComparer.InvariantCultureIgnoreCase);
            mExcludedFieldNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            InitDefaultExcludedColumns(typeInfo);
            ComposeFromComponents(typeInfo);
        }


        private void InitDefaultExcludedColumns(ObjectTypeInfo typeInfo)
        {
            // Read default columns from provided typeInfo, removing unknown columns
            var defaultExcludedColumnNames = DEFAULT_EXCLUDED_COLUMNS_DELEGATES
                .Select(columnDelegate => columnDelegate(typeInfo))
                .Where(columnName => !String.Equals(ObjectTypeInfo.COLUMN_NAME_UNKNOWN, columnName, StringComparison.Ordinal));

            mExcludedFieldNames.AddRange(defaultExcludedColumnNames);
        }


        private void ComposeFromComponents(ObjectTypeInfo typeInfo)
        {
            var componentTypeInfos = GetComponentTypeInfos(typeInfo).ToList();

            ComposeExcludedFieldNames(componentTypeInfos);
            ComposeStructuredFields(componentTypeInfos);
        }


        private void ComposeStructuredFields(IEnumerable<ObjectTypeInfo> componentTypeInfos)
        {
            var structuredFields = componentTypeInfos.SelectMany(typeInfo => typeInfo.SerializationSettings.mStructuredFields);

            foreach (var structuredField in structuredFields)
            {
                mStructuredFields.Add(structuredField.Key, structuredField.Value);
            }
        }


        private void ComposeExcludedFieldNames(IEnumerable<ObjectTypeInfo> componentTypeInfos)
        {
            mExcludedFieldNames.AddRange(componentTypeInfos
                .SelectMany(componentTypeInfo => componentTypeInfo.SerializationSettings.ExcludedFieldNames));
        }


        private static IEnumerable<ObjectTypeInfo> GetComponentTypeInfos(ObjectTypeInfo typeInfo)
        {
            if (!typeInfo.IsComposite)
            {
                return Enumerable.Empty<ObjectTypeInfo>();
            }

            return typeInfo.ConsistsOf
                .Where(objectType => typeInfo.ObjectType != objectType)
                .Select(component => ObjectTypeManager.GetTypeInfo(component));
        }


        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Copy source</param>
        internal SerializationSettings(SerializationSettings source)
        {
            mExcludedFieldNames = new HashSet<string>(source.ExcludedFieldNames, StringComparer.InvariantCultureIgnoreCase);

            if (source.mStructuredFields != null)
            {
                mStructuredFields = new Dictionary<string, IStructuredField>(source.mStructuredFields, StringComparer.InvariantCultureIgnoreCase);
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets the structured field information for the field with the given name or null if the given field is not structured.
        /// </summary>
        /// <param name="fieldName">Info field name</param>
        public IStructuredField GetStructuredField(string fieldName)
        {
            return (mStructuredFields == null) || !mStructuredFields.ContainsKey(fieldName)
                ? null
                : mStructuredFields[fieldName];
        }


        /// <summary>
        /// Returns true if the given object field name is excluded from the serialization.
        /// </summary>
        /// <param name="fieldName">Info field name</param>
        /// <remarks>Method is, by the nature of the values in <see cref="ExcludedFieldNames"/>, case insensitive.</remarks>
        public bool IsExcludedField(string fieldName)
        {
            return ExcludedFieldNames.Contains(fieldName)
                || ((AdditionalFieldFilter != null) && AdditionalFieldFilter(fieldName));
        }

        #endregion
    }
}
