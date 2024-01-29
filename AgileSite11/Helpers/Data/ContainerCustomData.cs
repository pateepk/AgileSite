using System;
using System.Runtime.Serialization;
using System.Security;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Custom data container for data container.
    /// </summary>    
    [Serializable]
    public class ContainerCustomData : CustomData
    {
        #region "Variables"

        private string mColumnName = null;
        private IDataContainer mContainer = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="key">Column name</param>
        public override object this[string key]
        {
            get
            {
                return base[key];
            }
            set
            {
                base[key] = value;
                SaveToDataContainer();
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public override bool SetValue(string columnName, object value)
        {
            bool result = base.SetValue(columnName, value);
            SaveToDataContainer();
            return result;
        }


        /// <summary>
        /// Loads the XML to the content table.
        /// </summary>
        /// <param name="data">Content XML to load</param>
        public override void LoadData(string data)
        {
            base.LoadData(data);
            SaveToDataContainer();
        }


        /// <summary>
        /// Removes element with specified key from collection
        /// </summary>
        /// <param name="key">Key to remove</param>
        public override void Remove(string key)
        {
            base.Remove(key);
            SaveToDataContainer();
        }


        /// <summary>
        /// Sets value to data container.
        /// </summary>
        private void SaveToDataContainer()
        {
            if (mContainer != null)
            {
                mContainer.SetValue(mColumnName, GetData());
            }
        }

        #endregion


        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize base class
            base.GetObjectData(info, context);

            info.AddValue("ColumnName", mColumnName);
        }


        /// <summary>
        /// Constructor - Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="ctxt">Streaming context</param>
        public ContainerCustomData(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
            // Get the values from info and assign them to the appropriate properties
            mColumnName = info.GetString("ColumnName");
        }


        /// <summary>
        /// Constructor - creates empty ContainerCustomData object.
        /// Object created by this constructor works only as in-memory storage and does not reflects changes to DataClass and DB. 
        /// </summary>
        public ContainerCustomData()
        {
        }


        /// <summary>
        /// Constructor - creates empty ContainerCustomData object.
        /// </summary>
        /// <param name="container">Related data container</param>
        /// <param name="columnName">Related column name</param>
        public ContainerCustomData(IDataContainer container, string columnName)
        {
            mContainer = container;
            mColumnName = columnName;
            LoadData(ValidationHelper.GetString(mContainer.GetValue(mColumnName), string.Empty));
        }
    }
}