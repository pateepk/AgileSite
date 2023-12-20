
using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine.CollectionExtensions;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Represents the settings for the node data copying.
    /// </summary>
    public class CopyNodeDataSettings
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets a value that indicates if the CMS_Tree data should be copied.
        /// </summary>
        public bool CopyTreeData
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value that indicates if the CMS_Document data should be copied.
        /// </summary>
        public bool CopyDocumentData
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value that indicates if the coupled data should be copied.
        /// </summary>
        public bool CopyCoupledData
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value that indicates if the versioned data should be copied.
        /// </summary>
        public bool CopyVersionedData
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value that indicates if the non-versioned data should be copied.
        /// </summary>
        public bool CopyNonVersionedData
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value that indicates if the system tree data should be copied.
        /// </summary>
        public bool CopySystemTreeData
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value that indicates if the system document data should be copied.
        /// </summary>
        public bool CopySystemDocumentData
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value that indicates if the SKU data should be copied.
        /// </summary>
        public bool CopySKUData
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the columns to be excluded from the copying.
        /// </summary>
        public ISet<string> ExcludeColumns
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates if the changes made to the node instance should be reset.
        /// </summary>
        public bool ResetChanges
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a default settings object.
        /// </summary>
        public CopyNodeDataSettings()
        {
            ExcludeColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Creates a settings object with the properties initialized to the specified values.
        /// </summary>
        /// <param name="copyTreeData">Indicates if the CMS_Tree data should be copied</param>
        /// <param name="copyDocumentData">Indicates if the CMS_Document data should be copied</param>
        /// <param name="copyCoupledData">Indicates if the coupled data should be copied</param>
        /// <param name="copyVersionedData">Indicates if the versioned data should be copied</param>
        /// <param name="copyNonVersionedData">Indicates if the non-versioned data should be copied</param>
        /// <param name="copySystemTreeData">Indicates if the system tree data should be copied</param>
        /// <param name="copySystemDocumentData">Indicates if the system document data should be copied</param>
        /// <param name="copySkuData">Indicates if the SKU data should be copied</param>
        /// <param name="excludeColumns">Columns to be excluded from the copying</param>
        public CopyNodeDataSettings(bool copyTreeData, bool copyDocumentData, bool copyCoupledData, bool copyVersionedData, bool copyNonVersionedData, bool copySystemTreeData, bool copySystemDocumentData, bool copySkuData, IEnumerable<string> excludeColumns)
            : this()
        {
            CopyTreeData = copyTreeData;
            CopyDocumentData = copyDocumentData;
            CopyCoupledData = copyCoupledData;
            CopyVersionedData = copyVersionedData;
            CopyNonVersionedData = copyNonVersionedData;
            CopySystemTreeData = copySystemTreeData;
            CopySystemDocumentData = copySystemDocumentData;
            CopySKUData = copySkuData;

            ExcludeColumns.AddRangeToSet(excludeColumns ?? Enumerable.Empty<string>());
        }


        /// <summary>
        /// Creates a settings object with the properties initialized to the specified values.
        /// </summary>
        /// <param name="copyFlagsValue">Common copy flags value</param>
        /// <param name="excludeColumns">Columns to be excluded from the copying</param>
        public CopyNodeDataSettings(bool copyFlagsValue, IEnumerable<string> excludeColumns)
            : this(copyFlagsValue, copyFlagsValue, copyFlagsValue, copyFlagsValue, copyFlagsValue, copyFlagsValue, copyFlagsValue, copyFlagsValue, excludeColumns)
        {
        }

        #endregion
    }
}
