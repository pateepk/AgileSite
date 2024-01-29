using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Class providing access to document custom fields.
    /// </summary>
    /// <remarks>
    /// This class is intended for internal usage only.
    /// </remarks>
    public class DocumentFieldsInfo : AbstractInfoBase<DocumentFieldsInfo>
    {
        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            DocumentFieldsInfoProvider.DeleteDocumentFieldsInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            DocumentFieldsInfoProvider.SetDocumentFieldsInfo(this);
        }


        /// <summary>
        /// Creates a clone of document fields
        /// </summary>
        public override DocumentFieldsInfo Clone()
        {
            // Get new instance
            DocumentFieldsInfo fieldsInfo = New(ClassName);

            fieldsInfo.LoadData(new LoadDataSettings(this));
            fieldsInfo.IsClone = true;

            return fieldsInfo;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates an instance of <see cref="DocumentFieldsInfo"/>.
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly.")]
        public DocumentFieldsInfo()
            : this(null)
        {
        }


        /// <summary>
        /// Creates an instance of <see cref="DocumentFieldsInfo"/> based on give class name.
        /// </summary>
        /// <param name="className">Class name of the document fields.</param>
        public DocumentFieldsInfo(string className)
        {
            if (!String.IsNullOrEmpty(className))
            {
                TypeInfo = DocumentFieldsInfoProvider.GetTypeInfo(className);
            }
        }


        /// <summary>
        /// Creates an instance of <see cref="DocumentFieldsInfo"/> based on give class name and data.
        /// </summary>
        /// <param name="className">Class name of the document fields.</param>
        /// <param name="dr">DataRow with the object data.</param>
        internal DocumentFieldsInfo(string className, DataRow dr)
            : this(className)
        {
            if (dr != null)
            {
                InitFromDataContainer(dr.AsDataContainer());
            }
        }


        /// <summary>
        /// Initializes the data from the Data container, can be called only after calling the empty constructor.
        /// </summary>
        /// <param name="dc">Data container with the data.</param>
        internal void InitFromDataContainer(IDataContainer dc)
        {
            LoadData(new LoadDataSettings(dc));
        }


        /// <summary>
        /// Loads the object using the given settings.
        /// </summary>
        /// <param name="settings">Data settings.</param>
        protected override void LoadData(LoadDataSettings settings)
        {
            base.LoadData(settings);

            if (ObjectID != 0)
            {
                return;
            }

            if (settings.Data == null)
            {
                return;
            }

            // Set at least ID value if data doesn't contain coupled data
            var documentForeignKeyValue = ValidationHelper.GetInteger(settings.Data.GetValue("DocumentForeignKeyValue"), 0);
            if (documentForeignKeyValue <= 0)
            {
                return;
            }

            ExecuteWithOriginalData(() =>
            {
                ObjectID = documentForeignKeyValue;
            });
        }

        #endregion


        #region "New methods"

        /// <summary>
        /// Creates a new object from the given DataRow.
        /// </summary>
        /// <param name="settings">Data settings.</param>
        protected override BaseInfo NewObject(LoadDataSettings settings)
        {
            // Transform object type back to class name and create object by class name
            var className = DocumentFieldsInfoProvider.GetClassName(settings.ObjectType);

            return New(className, settings.Data);
        }


        /// <summary>
        /// Creates new DocumentFieldsInfo instance which must inherit the DocumentFieldsInfo class.
        /// </summary>
        /// <param name="className">Class name in format application.class.</param>
        /// <param name="dataRow">Data row containing both tree node and coupled table.</param>
        internal static DocumentFieldsInfo New(string className, DataRow dataRow = null)
        {
            DocumentFieldsInfo result = NewInternal();

            // If type not provided load default type
            if (result != null)
            {
                result.Initialize(className, dataRow);
            }

            return result;
        }


        /// <summary>
        /// Creates new document fields instance.
        /// </summary>
        /// <param name="className">Class name in format application.class.</param>
        /// <param name="data">Data container containing both tree node and coupled table.</param>
        internal static DocumentFieldsInfo New(string className, IDataContainer data)
        {
            DocumentFieldsInfo result = NewInternal();

            // If type not provided load default type
            if (result != null)
            {
                result.Initialize(className, data);
            }

            return result;
        }


        /// <summary>
        /// Initializes the object created with default constructor. Use it to load existing document fields from data row.
        /// </summary>
        /// <param name="className">Document type name in format application.type.</param>
        /// <param name="dr">Data row containing all document data.</param>
        private void Initialize(string className, DataRow dr)
        {
            TypeInfo = DocumentFieldsInfoProvider.GetTypeInfo(className);
            LoadData(new LoadDataSettings(dr));
        }


        /// <summary>
        /// Initializes the object created with default constructor. Use it to load existing document fields from data row.
        /// </summary>
        /// <param name="className">Document type name in format application.type.</param>
        /// <param name="data">Data container containing all document data.</param>
        private void Initialize(string className, IDataContainer data)
        {
            TypeInfo = DocumentFieldsInfoProvider.GetTypeInfo(className);
            LoadData(new LoadDataSettings(data));
        }

        #endregion
    }
}