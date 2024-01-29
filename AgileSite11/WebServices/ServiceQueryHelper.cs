using System;
using System.Collections.Generic;
using System.ServiceModel.Web;

using CMS.Helpers;
using CMS.Base;

namespace CMS.WebServices
{
    /// <summary>
    /// QueryString helper methods for WCF Service.
    /// </summary>
    public class ServiceQueryHelper : IDataContainer
    {
        #region "Variables"

        private static ServiceQueryHelper mInstance = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Singleton Instance of the service query helper.
        /// </summary>
        public static ServiceQueryHelper Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new ServiceQueryHelper();
                }
                return mInstance;
            }
            set
            {
                mInstance = value;
            }
        }

        #endregion


        #region "ISimpleDataContainer Members"

        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                SetValue(columnName, value);
            }
        }


        /// <summary>
        /// Gets the value from QueryString.
        /// </summary>
        /// <param name="key">QueryString key</param>
        public object GetValue(string key)
        {
            object retval = null;
            TryGetValue(key, out retval);
            return retval;
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <param name="value">New value</param>
        public bool SetValue(string key, object value)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region IDataContainer Members

        /// <summary>
        /// Column names.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                return new List<string>(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters.AllKeys);
            }
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public bool TryGetValue(string columnName, out object value)
        {
            value = ValidationHelper.GetString(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters[columnName], null);
            return (value != null);
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            return (WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters[columnName] == null);
        }

        #endregion
    }
}