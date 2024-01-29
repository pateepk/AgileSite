using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Info object with no methods implemented. Serves as a base info for partially implemented info objects.
    /// </summary>
    public class NotImplementedInfo : BaseInfo
    {
        #region "Constructors"

        /// <summary>
        /// Empty constructor
        /// </summary>
        internal NotImplementedInfo()
            : base(InfoHelper.UNKNOWN_TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="typeInfo">Type info to use for initialization</param>
        protected NotImplementedInfo(ObjectTypeInfo typeInfo)
            : base(typeInfo)
        {
        }

        #endregion


        #region "Not implemented abstract properties"

        /// <summary>
        /// Dictionary with the methods for clearing the internal object cache [columnName] => [clearCacheAction]
        /// </summary>
        protected internal override IDictionary<string, Action<BaseInfo>> ClearCacheMethods
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Column names.
        /// </summary>
        public override List<string> ColumnNames
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Returns true if the object changed.
        /// </summary>
        public override bool HasChanged
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion


        #region "Not implemented abstract methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Creates the clone of the object.
        /// </summary>
        /// <param name="clear">If true, the object is cleared to be able to create new object</param>
        public override BaseInfo CloneObject(bool clear = false)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Sets the object value.
        /// </summary>
        public override bool SetValue(string columnName, object value)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetValue(string columnName, out object value)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Locks the object as a read-only
        /// </summary>
        protected internal override void SetReadOnly()
        {
            throw new NotImplementedException();
        }                     


        /// <summary>
        /// Returns true if the object contains given column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override bool ContainsColumn(string columnName)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            throw new NotImplementedException();
        }
        

        /// <summary>
        /// Loads the object from the given data container
        /// </summary>
        /// <param name="settings">Data settings</param>
        protected internal override void LoadData(LoadDataSettings settings)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Creates new object of the given class
        /// </summary>
        /// <param name="settings">Data settings</param>
        protected override BaseInfo NewObject(LoadDataSettings settings)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Executes the given action using original data of the object
        /// </summary>
        /// <param name="action">Action to execute</param>
        public override void ExecuteWithOriginalData(Action action)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Reverts the object changes to the original values.
        /// </summary>
        public override void RevertChanges()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Resets the object changes and keeps the new values as unchanged.
        /// </summary>
        public override void ResetChanges()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}