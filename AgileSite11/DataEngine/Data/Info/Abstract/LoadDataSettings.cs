using System;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Defines the settings for loading data into objects
    /// </summary>
    public class LoadDataSettings
    {
        /// <summary>
        /// Object type used by the factory to create a specific type of the object.
        /// </summary>
        public string ObjectType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Object data. When object type is not specified, data may provide necessary information to select correct object type in case there are multiple candidates.
        /// </summary>
        public IDataContainer Data
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the method throws an exception in case the object type was not found. Default false
        /// </summary>
        public bool ThrowIfNotFound
        {
            get;
            set;
        }

        
        /// <summary>
        /// Determines if the provided data is external data which may not be complete.
        /// If true, the loaded object loads default data before loading this data, and does not overwrite default values with null from external data.
        /// </summary>
        public bool DataIsExternal
        {
            get;
            set;
        } 


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Source data</param>
        /// <param name="objectType">Object type</param>
        public LoadDataSettings(IDataContainer data, string objectType = null)
        {
            Data = data;
            ObjectType = objectType;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dr">Source DataRow</param>
        /// <param name="objectType">Object type</param>
        public LoadDataSettings(DataRow dr, string objectType = null)
        {
            Data = dr.AsDataContainer();
            ObjectType = objectType;
        }
    }
}
