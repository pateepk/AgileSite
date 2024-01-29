using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using CMS.Base;

namespace CMS.ModuleUsageTracking
{
    /// <summary>
    /// Statistical data about module with metadata.
    /// </summary>
    internal class ModuleUsageData : ISerializable
    {
        #region "Properties"

        /// <summary>
        /// Date and time when these statistics were collected.
        /// </summary>
        internal DateTime DateTime
        {
            get;
            set;
        }


        /// <summary>
        /// Identity of this installation.
        /// </summary>
        internal string Identity
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the data source component relevant to data.
        /// </summary>
        internal string DataSourceName
        {
            get;
            set;
        }


        /// <summary>
        /// Current product version.
        /// </summary>
        internal string ProductVersion
        {
            get;
            set;
        }


        /// <summary>
        /// Duration of module data retrieval in milliseconds.
        /// </summary>
        internal long DataCollectionTime
        {
            get;
            set;
        }


        /// <summary>
        /// Statistical data about module.
        /// </summary>
        internal IModuleUsageDataCollection Data
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Default model constructor.
        /// </summary>
        /// <param name="dateTime">Date and time when these statistics were collected.</param>
        /// <param name="identity">Identity of this installation.</param>
        /// <param name="dataSourceName">Name of the data source component relevant to data.</param>
        /// <param name="productVersion">Current product version.></param>
        /// <param name="dataCollectionTime">Duration of module data retrieval in milliseconds.</param>
        /// <param name="data">Statistical data about module.</param>
        internal ModuleUsageData(DateTime dateTime, string identity, string dataSourceName, string productVersion, long dataCollectionTime, IModuleUsageDataCollection data)
        {
            DateTime = dateTime;
            Identity = identity;
            DataSourceName = dataSourceName;
            ProductVersion = productVersion;
            DataCollectionTime = dataCollectionTime;
            Data = data;
        }

        #endregion


        #region "Serialization methods"

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("DateTime", DateTime);
            info.AddValue("Identity", Identity);
            info.AddValue("DataSourceName", DataSourceName);
            info.AddValue("ProductVersion", ProductVersion);
            info.AddValue("DataCollectionTime", DataCollectionTime);

            // Serialize Data property using the ISerializable interface manually. Data property implements IEnumerable and
            // Json.NET has IEnumerable fallback placed before ISerializable and tries to serialize the property into 
            // Json array and then deserialize it into List. 
            Data.GetObjectData(info, context);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        public ModuleUsageData(SerializationInfo info, StreamingContext context)
        {
            DateTime = info.GetDateTime("DateTime");
            Identity = info.GetString("Identity");
            DataSourceName = info.GetString("DataSourceName");
            ProductVersion = info.GetString("ProductVersion");
            DataCollectionTime = info.GetInt64("DataCollectionTime");

            // De-serialize Data property using the ISerializable interface manually. Data property implements IEnumerable and
            // Json.NET has IEnumerable fallback placed before ISerializable and tries to serialize the property into 
            // Json array and then deserialize it into List. 
            Data = new ModuleUsageDataCollection(info, context);
        }

        #endregion
    }
}
