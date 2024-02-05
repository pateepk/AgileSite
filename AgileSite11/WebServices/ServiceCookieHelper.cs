using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel.Web;

using CMS.Helpers;
using CMS.Base;

namespace CMS.WebServices
{
    /// <summary>
    /// Cookie helper methods for WCF Service.
    /// </summary>
    public class ServiceCookieHelper : IDataContainer
    {
        #region "Variables"

        private static ServiceCookieHelper mInstance = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Singleton Instance of the cookie helper.
        /// </summary>
        public static ServiceCookieHelper Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new ServiceCookieHelper();
                }
                return mInstance;
            }
            set
            {
                mInstance = value;
            }
        }


        /// <summary>
        /// Response cookies collection.
        /// </summary>
        public static Hashtable RequestCookies
        {
            get
            {
                Hashtable cookiesTable = (Hashtable)RequestStockHelper.GetItem("ServiceCookiesTable");
                if (cookiesTable == null)
                {
                    cookiesTable = new Hashtable();

                    // Get the cookie string from the header
                    string cookiesString = WebOperationContext.Current.IncomingRequest.Headers["cookie"];
                    if (!string.IsNullOrEmpty(cookiesString))
                    {
                        // Find the correct cookie and get the encrypted value
                        string[] cookies = cookiesString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string cookie in cookies)
                        {
                            int index = cookie.IndexOf('=');
                            string val = cookie.Substring(index + 1).TrimEnd(';');
                            cookiesTable[cookie.Substring(0, index)] = val;
                        }
                    }

                    RequestStockHelper.Add("ServiceCookiesTable", cookiesTable);
                }

                return cookiesTable;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the value of the given cookie.
        /// </summary>
        /// <param name="cookieName">Name of the cookie</param>
        private object GetValue(string cookieName)
        {
            object retval = null;
            TryGetValue(cookieName, out retval);
            return retval;
        }

        #endregion


        #region "ISimpleDataContainer Members"

        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        object ISimpleDataContainer.this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Gets the value from cookie.
        /// </summary>
        /// <param name="cookieName">Cookie name</param>
        object ISimpleDataContainer.GetValue(string cookieName)
        {
            return GetValue(cookieName);
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="cookieName">Cookie name</param>
        /// <param name="value">New value</param>
        bool ISimpleDataContainer.SetValue(string cookieName, object value)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region IDataContainer Members

        /// <summary>
        /// All the cookie names.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                return TypeHelper.NewList(RequestCookies.Keys);
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
            value = RequestCookies[columnName];
            return (value != null);
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            return (GetValue(columnName) != null);
        }

        #endregion
    }
}