using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.DataEngine
{
    /// <summary>
    /// Determines how the object behaves within continuous integration. Represents continuous integration settings in the type information of objects.
    /// </summary>
    /// <remarks>
    /// Use in the <see cref="ObjectTypeInfo.ContinuousIntegrationSettings"/> property of ObjectTypeInfo.
    /// </remarks>
    public class ContinuousIntegrationSettings
    {
        #region "Private variables"

        // Private search-optimized object file name fields collection
        private readonly HashSet<string> mObjectFileNameFields;

        // Private search-optimized separated field collection
        private Dictionary<string, SeparatedField> mSeparatedFields;

        // Private search-optimized collection of dependency columns
        private readonly HashSet<string> mDependencyColumns;

        // Private search-optimized collection of additional filter dependencies
        private readonly HashSet<ObjectReference> mFilterDependencies;

        #endregion
        

        #region "Public properties"

        /// <summary>
        /// Indicates whether the object type supports continuous integration.
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the collection of field names to get object name for file name of the serialized object. If not specified code name or GUID of the object is used.
        /// </summary>
        public ICollection<string> ObjectFileNameFields
        {
            get
            {
                return mObjectFileNameFields;
            }
        }


        /// <summary>
        /// Field used for object identification in a repository, when default code name and/or GUID are not suitable.
        /// </summary>
        /// <remarks>
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        public string IdentificationField
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets folder name used to create object type folder. If not specified object type of the object is used.
        /// </summary>
        public string ObjectTypeFolderName
        {
            get;
            set;
        }


        /// <summary>
        /// List of separated fields. Separated field's content is stored in extra file when using continuous integration.
        /// This collection contains info's binary data field by default.
        /// </summary>
        public IEnumerable<SeparatedField> SeparatedFields
        {
            get
            {
                return mSeparatedFields.Select(x => x.Value);
            }
            set
            {
                var valueDictionary = (value ?? Enumerable.Empty<SeparatedField>()).ToDictionary(field => field.FieldName, field => field);
                mSeparatedFields = new Dictionary<string, SeparatedField>(valueDictionary, StringComparer.InvariantCultureIgnoreCase);
            }
        }


        /// <summary>
        /// Collection of additional columns whose change may affect object's code name, GUID, group, site or parent. 
        /// </summary>
        /// <remarks>
        /// For example when you override <see cref="BaseInfo.ObjectCodeName"/> and you compose the code name from other fields than <see cref="ObjectTypeInfo.CodeNameColumn"/> then these fields must be defined in this collection.
        /// </remarks>
        public ICollection<string> DependencyColumns
        {
            get
            {
                return mDependencyColumns;
            }
        }
        

        /// <summary>
        /// Where condition for object filtering. Only objects that meet this condition will be handled by continuous integration.
        /// </summary>
        public IWhereCondition FilterCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Column name used for object filtering. By default the codename column is used.
        /// </summary>
        /// <remarks>
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        public string FilterColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Collection of additional dependencies which may affect object's inclusion in repository.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Dependent objects are checked in similar way as parent objects and can filter out the processed object.
        /// </para>
        /// <para>
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </para>
        /// </remarks>
        internal ICollection<ObjectReference> FilterDependencies
        {
            get
            {
                return mFilterDependencies;
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="typeInfo">Related type information</param>
        public ContinuousIntegrationSettings(ObjectTypeInfo typeInfo)
        {
            mSeparatedFields = new Dictionary<string, SeparatedField>(StringComparer.InvariantCultureIgnoreCase);
            mObjectFileNameFields = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            mDependencyColumns = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            mFilterDependencies = new HashSet<ObjectReference>();

            if (!String.IsNullOrEmpty(typeInfo.BinaryColumn) && typeInfo.BinaryColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                mSeparatedFields.Add(typeInfo.BinaryColumn, new SeparatedField(typeInfo.BinaryColumn)
                {
                    FileExtensionFieldName = typeInfo.ExtensionColumn,
                    IsBinaryField = true
                });
            }

            // Objects are filtered by codename by default
            FilterColumn = typeInfo.CodeNameColumn;
        }
    }
}
