using System;
using System.Collections.Generic;
using System.Data;

using CMS.Helpers;
using CMS.Base;

namespace CMS.Globalization
{
    using StringList = List<string>;

    /// <summary>
    /// Wrapper class to provide Countries enumeration in the MacroEngine.
    /// </summary>
    public class CountriesMacroContainer : IDataContainer
    {
        #region "Variables"

        /// <summary>
        /// Names of the countries in the system.
        /// </summary>
        private static CMSStatic<StringList> mCountryNames = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Names of the countries in the system.
        /// </summary>
        private static List<string> CountryNames
        {
            get
            {
                if (mCountryNames == null)
                {
                    var names = new StringList();

                    // Get the countries
                    DataSet ds = CountryInfoProvider.GetCountries();
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            names.Add(dr["CountryName"].ToString());
                        }
                    }

                    mCountryNames = new CMSStatic<StringList>();
                    mCountryNames.Value = names;
                }

                return mCountryNames;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets a country of specified name if exists. Setter is not implemented.
        /// </summary>
        /// <param name="columnName">Column name</param>
        object ISimpleDataContainer.this[string columnName]
        {
            get
            {
                object value = null;
                TryGetValue(columnName, out value);
                return value;
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Returns country of specified name if exists.
        /// </summary>
        /// <param name="columnName">Column name</param>
        object ISimpleDataContainer.GetValue(string columnName)
        {
            object value = null;
            TryGetValue(columnName, out value);
            return value;
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        bool ISimpleDataContainer.SetValue(string columnName, object value)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns list of all countries.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                return CountryNames;
            }
        }


        /// <summary>
        /// Returns country of specified name if exists.
        /// </summary>
        /// <param name="countryName">Name of the country</param>
        /// <param name="value">CountryInfo will be returned if exists</param>
        public bool TryGetValue(string countryName, out object value)
        {
            value = CountryInfoProvider.GetCountryInfo(countryName);
            return (value != null);
        }


        /// <summary>
        /// Returns true if country of specified name exists.
        /// </summary>
        public bool ContainsColumn(string columnName)
        {
            return CountryNames.Contains(columnName);
        }

        #endregion
    }
}