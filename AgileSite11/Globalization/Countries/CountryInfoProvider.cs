using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;

namespace CMS.Globalization
{
    using TypedDataSet = InfoDataSet<CountryInfo>;

    /// <summary>
    /// Class providing management of the countries.
    /// </summary>
    public class CountryInfoProvider : AbstractInfoProvider<CountryInfo, CountryInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CountryInfoProvider()
            : base(CountryInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Name = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }

        #endregion


        #region "Variables & Properties"

        /// <summary>
        /// Countries indexed by two letter country code.
        /// </summary>
        private static CMSStatic<ProviderInfoDictionary<string>> mCountryInfosByTwoLetterCode = new CMSStatic<ProviderInfoDictionary<string>>();

        /// <summary>
        /// Countries indexed by three letter country code.
        /// </summary>
        private static CMSStatic<ProviderInfoDictionary<string>> mCountryInfosByThreeLetterCode = new CMSStatic<ProviderInfoDictionary<string>>();

        /// <summary>
        /// Table lock for loading.
        /// </summary>
        private static object tableLock = new object();


        /// <summary>
        /// Countries indexed by two-letter country code.
        /// </summary>
        private static ProviderInfoDictionary<string> CountryInfosByTwoLetterCode
        {
            get
            {
                return mCountryInfosByTwoLetterCode;
            }
            set
            {
                mCountryInfosByTwoLetterCode.Value = value;
            }
        }


        /// <summary>
        /// Countries indexed by three-letter country code.
        /// </summary>
        private static ProviderInfoDictionary<string> CountryInfosByThreeLetterCode
        {
            get
            {
                return mCountryInfosByThreeLetterCode;
            }
            set
            {
                mCountryInfosByThreeLetterCode.Value = value;
            }
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns the CountryInfo structure for the specified country.
        /// </summary>
        /// <param name="countryId">Country id</param>
        public static CountryInfo GetCountryInfo(int countryId)
        {
            return ProviderObject.GetInfoById(countryId);
        }


        /// <summary>
        /// Returns the CountryInfo structure for the specified country.
        /// </summary>
        /// <param name="countryName">Country name</param>
        public static CountryInfo GetCountryInfo(string countryName)
        {
            return ProviderObject.GetInfoByCodeName(countryName);
        }


        /// <summary>
        ///  Returns the CountryInfo structure for the specified country code.
        /// </summary>
        /// <param name="countryCode">Code of the country. Two- and three- letter codes are supported.</param>
        public static CountryInfo GetCountryInfoByCode(string countryCode)
        {
            return ProviderObject.GetCountryInfoByCodeInternal(countryCode);
        }


        /// <summary>
        /// Sets (updates or inserts) specified country.
        /// </summary>
        /// <param name="country">Country to set</param>
        public static void SetCountryInfo(CountryInfo country)
        {
            ProviderObject.SetCountryInfoInternal(country);
        }


        /// <summary>
        /// Deletes specified country.
        /// </summary>
        /// <param name="countryObj">Country object</param>
        public static void DeleteCountryInfo(CountryInfo countryObj)
        {
            ProviderObject.DeleteCountryInfoInternal(countryObj);
        }


        /// <summary>
        /// Deletes specified country.
        /// </summary>
        /// <param name="countryId">Country id</param>
        public static void DeleteCountryInfo(int countryId)
        {
            CountryInfo countryObj = GetCountryInfo(countryId);
            DeleteCountryInfo(countryObj);
        }


        /// <summary>
        /// Returns countries query.
        /// </summary>
        public static ObjectQuery<CountryInfo> GetCountries()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Gets all countries.
        /// </summary>
        [Obsolete("Use method GetCountries() instead.")]
        public static TypedDataSet GetAllCountries()
        {
            return GetCountries().TypedResult;
        }


        /// <summary>
        /// Returns DataSet with all countries which have some states.
        /// </summary>
        public static TypedDataSet GetCountriesWithStates()
        {
            return ProviderObject.GetCountriesWithStatesInternal();
        }


        /// <summary>
        /// Check dependencies. Returns true if something is dependent.
        /// </summary>
        /// <param name="countryId">Country ID</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public static bool CheckDependencies(int countryId)
        {
            return ProviderObject.CheckDependenciesInternal(countryId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            // Clear countries
            lock (tableLock)
            {
                if (CountryInfosByTwoLetterCode != null)
                {
                    CountryInfosByTwoLetterCode.Clear(logTasks);
                }

                if (CountryInfosByThreeLetterCode != null)
                {
                    CountryInfosByThreeLetterCode.Clear(logTasks);
                }
            }
        }


        /// <summary>
        ///  Returns the CountryInfo structure for the specified country code.
        /// </summary>
        /// <param name="countryCode">Code of the country. Two- and three- letter codes are supported.</param>
        protected virtual CountryInfo GetCountryInfoByCodeInternal(string countryCode)
        {
            CountryInfo result = null;

            if (!string.IsNullOrEmpty(countryCode))
            {
                // Load hashtables
                LoadCountries();

                string column = null;
                ProviderInfoDictionary<string> table = null;

                // Select column and hashtable according to code type
                if (countryCode.Length == 2)
                {
                    column = "CountryTwoLetterCode";
                    table = CountryInfosByTwoLetterCode;
                }
                else if (countryCode.Length == 3)
                {
                    column = "CountryThreeLetterCode";
                    table = CountryInfosByThreeLetterCode;
                }

                if ((column != null) && (table != null))
                {
                    // Try to get country from hashtable
                    result = (CountryInfo)table[countryCode];
                    if (result == null)
                    {
                        // Get country from DB when not found
                        result = GetObjectQuery().WhereEquals(column, countryCode).TopN(1).FirstOrDefault();
                        if (result != null)
                        {
                            // Update hashtables
                            CountryInfosByTwoLetterCode.Update(result.CountryTwoLetterCode, result);
                            CountryInfosByThreeLetterCode.Update(result.CountryThreeLetterCode, result);
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Sets (updates or inserts) specified country.
        /// </summary>
        /// <param name="country">Country to set</param>
        protected virtual void SetCountryInfoInternal(CountryInfo country)
        {
            if (country != null)
            {
                // Load hashtables
                LoadCountries();

                // When editing existing country
                if (country.CountryID > 0)
                {
                    CountryInfo originalCountry = GetInfoById(country.CountryID);

                    // Check if two letter code has changed
                    if (originalCountry.CountryTwoLetterCode != country.CountryTwoLetterCode)
                    {
                        CountryInfosByTwoLetterCode.Delete(originalCountry.CountryTwoLetterCode);
                    }

                    // Check if three letter code has changed
                    if (originalCountry.CountryThreeLetterCode != country.CountryThreeLetterCode)
                    {
                        CountryInfosByThreeLetterCode.Delete(originalCountry.CountryThreeLetterCode);
                    }
                }

                // Save changes
                SetInfo(country);

                // Update hashtables
                CountryInfosByTwoLetterCode.Update(country.CountryTwoLetterCode, country);
                CountryInfosByThreeLetterCode.Update(country.CountryThreeLetterCode, country);
            }
        }


        /// <summary>
        /// Deletes specified country.
        /// </summary>
        /// <param name="countryObj">Country object</param>
        protected virtual void DeleteCountryInfoInternal(CountryInfo countryObj)
        {
            if (countryObj != null)
            {
                // Load hashtables
                LoadCountries();

                // Delete info
                DeleteInfo(countryObj);

                // Update hashtables
                CountryInfosByTwoLetterCode.Delete(countryObj.CountryTwoLetterCode);
                CountryInfosByThreeLetterCode.Delete(countryObj.CountryThreeLetterCode);
            }
        }


        /// <summary>
        /// Returns DataSet with all countries which have some states.
        /// </summary>
        protected virtual TypedDataSet GetCountriesWithStatesInternal()
        {
            var parameters = new QueryDataParameters();
            parameters.EnsureDataSet<CountryInfo>();

            return ConnectionHelper.ExecuteQuery("cms.country.selectcountrieswithstates", parameters).As<CountryInfo>();
        }


        /// <summary>
        /// Check dependencies. Returns true if something is dependent.
        /// </summary>
        /// <param name="countryId">Country ID</param>
        protected virtual bool CheckDependenciesInternal(int countryId)
        {
            var infoObj = GetInfoById(countryId);
            if (infoObj != null)
            {
                return infoObj.Generalized.CheckDependencies();
            }
            return false;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Loads countries to hashtables.
        /// </summary>
        private static void LoadCountries()
        {
            if (ProviderHelper.LoadTables(CountryInfosByTwoLetterCode, CountryInfosByThreeLetterCode))
            {
                lock (tableLock)
                {
                    if (ProviderHelper.LoadTables(CountryInfosByTwoLetterCode, CountryInfosByThreeLetterCode))
                    {
                        // Prepare the tables
                        var tempCountries2 = new ProviderInfoDictionary<string>(CountryInfo.OBJECT_TYPE, "CountryTwoLetterCode");
                        var tempCountries3 = new ProviderInfoDictionary<string>(CountryInfo.OBJECT_TYPE, "CountryThreeLetterCode");

                        if (ProviderHelper.LoadHashTables(CountryInfo.OBJECT_TYPE, LoadHashtableEnum.All) != LoadHashtableEnum.None)
                        {
                            // Add all countries to hashtable
                            var countries = GetCountries();
                            foreach (var country in countries)
                            {
                                tempCountries2[country.CountryTwoLetterCode] = country;
                                tempCountries3[country.CountryThreeLetterCode] = country;
                            }
                        }

                        CountryInfosByTwoLetterCode = tempCountries2;
                        CountryInfosByThreeLetterCode = tempCountries3;
                    }
                }
            }
        }

        #endregion
    }
}