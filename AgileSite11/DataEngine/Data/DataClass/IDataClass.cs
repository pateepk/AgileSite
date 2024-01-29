using System.Data;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// DataClass interface.
    /// </summary>
    public interface IDataClass : IAdvancedDataContainer
    {
        #region "Properties"

        /// <summary> 
        /// Name of the data class
        /// </summary>
        string ClassName
        {
            get;
        }


        /// <summary>
        /// Object ID.
        /// </summary>
        /// Property can be used only for objects with single <see cref="IDColumn"/>.
        int ID
        {
            get;
        }


        /// <summary>
        /// ID column(s).
        /// </summary>
        /// <remarks>Primary key can be identified by more than one column. In this case columns are separated by ';' character.</remarks>
        string IDColumn
        {
            get;
        }


        /// <summary>
        /// Class structure information.
        /// </summary>
        ClassStructureInfo StructureInfo
        {
            get;
        }


        /// <summary>
        /// Number of the object columns.
        /// </summary>
        int ColumnsCount
        {
            get;
        }


        /// <summary>
        /// If true, the object allows partial updates.
        /// </summary>
        bool AllowPartialUpdate
        {
            get;
            set;
        }


        /// <summary>
        /// If true, original data is used instead of the actual data.
        /// </summary>
        bool UseOriginalData
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether class is in read-only mode.
        /// </summary>
        /// <seealso cref="SetReadOnly"/>
        bool IsReadOnly
        {
            get;
        }

        #endregion


        #region "Methods"

        /// <summary> 
        /// Inserts current record in the database.
        /// </summary>
        /// <param name="initId">If true, ID of the new object is initialized.</param>
        void Insert(bool initId = true);


        /// <summary>
        /// Updates or inserts the current record.
        /// </summary>
        /// <param name="existingWhere">Where condition for the existing object</param>
        void Upsert(WhereCondition existingWhere);


        /// <summary>
        /// Updates current record.
        /// </summary>
        void Update();


        /// <summary>
        /// Deletes current record.
        /// </summary>
        /// <param name="preserveData">If true, object data are preserved (it is possible to manipulate with the object further)</param>
        /// <remarks>The method assumes that the primary key is the first column in the DataRow.</remarks>
        void Delete(bool preserveData = false);


        /// <summary>
        /// Returns true if the object is empty.
        /// </summary>
        bool IsEmpty();


        /// <summary>
        /// Gets the DataSet from the object data.
        /// </summary>
        DataSet GetDataSet();


        /// <summary>
        /// Initializes an instance of data class after created by empty constructor
        /// </summary>
        /// <param name="structureInfo">Class structure info</param>
        void Init(ClassStructureInfo structureInfo);


        /// <summary>
        /// Copies the class data to other data class
        /// </summary>
        void CopyDataTo(IDataClass target);


        /// <summary>
        /// Copies the original class data to other data class
        /// </summary>
        void CopyOriginalDataTo(IDataClass target);


        /// <summary>
        /// Loads the object data from given data container.
        /// </summary>
        /// <param name="data">Source data</param>
        /// <param name="loadNullValues">If true, null values are loaded to the object</param>
        void LoadData(IDataContainer data, bool loadNullValues = true);
        

        /// <summary>
        /// Loads the object data by the given primary key value
        /// </summary>
        /// <param name="primaryKeyValue">Primary key value</param>
        void LoadData(int primaryKeyValue);


        /// <summary>
        /// Locks the data class as a read-only
        /// </summary>
        void SetReadOnly();

        #endregion
    }
}