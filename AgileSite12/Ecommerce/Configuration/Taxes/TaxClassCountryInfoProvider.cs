using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing TaxClassCountryInfo management.
    /// </summary>
    public class TaxClassCountryInfoProvider : AbstractInfoProvider<TaxClassCountryInfo, TaxClassCountryInfoProvider>, IFullNameInfoProvider
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TaxClassCountryInfoProvider()
            : base(TaxClassCountryInfo.TYPEINFO, new HashtableSettings
            {
                FullName = true,
                Load = LoadHashtableEnum.All,
                UseWeakReferences = true
            })
        {
        }


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all relationships between tax classes and countries.
        /// </summary>
        public static ObjectQuery<TaxClassCountryInfo> GetTaxClassCountries()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns relationship between specified tax class and country.
        /// </summary>
        /// <param name="countryId">Country ID</param>
        /// <param name="taxClassId">Tax class ID</param>
        public static TaxClassCountryInfo GetTaxClassCountryInfo(int countryId, int taxClassId)
        {
            return ProviderObject.GetTaxClassCountryInfoInternal(countryId, taxClassId);
        }


        /// <summary>
        /// Sets relationship between specified tax class and country.
        /// </summary>
        /// <param name="infoObj">Tax class-country relationship to be set</param>
        public static void SetTaxClassCountryInfo(TaxClassCountryInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified tax class and country.
        /// </summary>
        /// <param name="infoObj">Tax class-country relationship to be deleted</param>
        public static void DeleteTaxClassCountryInfo(TaxClassCountryInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Sets tax value defined for specified country. 
        /// </summary>
        /// <param name="countryId">Country ID</param>
        /// <param name="taxClassId">Tax class ID</param>
        /// <param name="value">Tax rate</param>
        public static void SetCountryTaxValue(int countryId, int taxClassId, decimal value)
        {
            var info = GetTaxClassCountryInfo(countryId, taxClassId);
            info = info ?? new TaxClassCountryInfo {CountryID = countryId, TaxClassID = taxClassId};

            info.TaxValue = value;

            SetTaxClassCountryInfo(info);
        }


        /// <summary>
        /// Removes tax value defined for specified country. 
        /// </summary>
        /// <param name="countryId">Country ID</param>
        /// <param name="taxClassId">Tax class ID</param>
        public static void RemoveCountryTaxValue(int countryId, int taxClassId)
        {
            var infoObj = GetTaxClassCountryInfo(countryId, taxClassId);
            DeleteTaxClassCountryInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns relationship between specified tax class and country.
        /// </summary>
        /// <param name="countryId">Country ID</param>
        /// <param name="taxClassId">Tax class ID</param>
        protected virtual TaxClassCountryInfo GetTaxClassCountryInfoInternal(int countryId, int taxClassId)
        {
            return GetInfoByFullName(ObjectHelper.BuildFullName(taxClassId.ToString(), countryId.ToString()));
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Creates new dictionary for caching the objects by full name.
        /// </summary>
        public ProviderInfoDictionary<string> GetFullNameDictionary()
        {
            return new ProviderInfoDictionary<string>(TaxClassCountryInfo.OBJECT_TYPE, "TaxClassID;CountryID");
        }


        /// <summary>
        /// Returns where condition for TaxClassCountry selection using full name (in format TaxClassID.CountryID)
        /// </summary>
        /// <param name="fullName">TaxClassCoountry full name</param>
        public string GetFullNameWhereCondition(string fullName)
        {
            string taxClassID;
            string countryID;

            // Parse the full name
            if (ObjectHelper.ParseFullName(fullName, out taxClassID, out countryID))
            {
                return new WhereCondition()
                    .WhereEquals("TaxClassID", ValidationHelper.GetInteger(taxClassID, 0))
                    .WhereEquals("CountryID", ValidationHelper.GetInteger(countryID, 0))
                    .ToString(true);
            }

            return null;
        }

        #endregion
    }
}