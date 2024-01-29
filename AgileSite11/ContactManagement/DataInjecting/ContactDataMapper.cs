using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Globalization;
using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Maps data to provided <see cref="ContactInfo"/>.
    /// </summary>
    public class ContactDataMapper : IContactDataMapper
    {
        private readonly string dataClassName;
        private readonly bool allowOverwrite;


        /// <summary>
        /// Creates an instance of <see cref="ContactDataMapper"/>.
        /// </summary>
        /// <param name="dataClassName">Data container class name.</param>
        /// <param name="allowOverwrite">Indicates if the already filled <see cref="ContactInfo"/> properties should be overwritten. Otherwise only empty properties are filled.</param>
        public ContactDataMapper(string dataClassName, bool allowOverwrite)
        {
            this.dataClassName = dataClassName;
            this.allowOverwrite = allowOverwrite;
        }


        /// <summary>
        /// Maps <paramref name="data"/> to provided <paramref name="contact"/>.
        /// </summary>
        /// <param name="data">Source data.</param>
        /// <param name="contact">Contact.</param>
        /// <returns>Returns <c>true</c> if there were any properties to map.</returns>
        /// <remarks>The modification is based on mapping defined for external data source.</remarks>
        public bool Map(ISimpleDataContainer data, ContactInfo contact)
        {
            if (data == null || string.IsNullOrEmpty(dataClassName))
            {
                return false;
            }

            // Get class mapping
            var classInfo = DataClassInfoProvider.GetDataClassInfo(dataClassName);
            if (string.IsNullOrEmpty(classInfo?.ClassContactMapping))
            {
                return false;
            }

            // Prepare form info based on mapping data
            var mapping = new FormInfo(classInfo.ClassContactMapping);
            if (mapping.ItemsList.Count <= 0)
            {
                return false;
            }

            // Get all mapped fields
            var fields = mapping.GetFields(true, true);

            var updated = false;

            // Name property contains a column of contact object
            // and MappedToField property contains form field mapped to the contact column
            foreach (FormFieldInfo ffi in fields)
            {
                var value = data.GetValue(ffi.MappedToField);
                if (value == null)
                {
                    continue;
                }

                if (DataHelper.IsEmpty(value) || !ContactFieldCanBeModified(allowOverwrite, contact, ffi))
                {
                    continue;
                }

                // Country and state data may be returned either as integer or as string
                switch ((ffi.Name ?? "").ToLowerInvariant())
                {
                    case "contactcountryid":
                        value = GetCountryID(value);
                        break;

                    case "contactstateid":
                        value = GetStateID(value);
                        break;

                    case "contactlastname":
                        value = GetTruncatedName(value.ToString());
                        break;

                    case "contactfirstname":
                        value = GetTruncatedName(value.ToString());
                        break;
                }

                updated |= contact.SetValue(ffi.Name, value);
            }

            return updated;
        }


        /// <summary>
        /// Returns name trimmed to 100 chars. If the string is shorter, returns the same string.
        /// </summary>
        private static string GetTruncatedName(string name)
        {
            return name.Length > 100 ? name.Substring(0, 100) : name;
        }


        /// <summary>
        /// Returns true if contact's field can be modified.
        /// </summary>
        private static bool ContactFieldCanBeModified(bool allowOverwrite, ContactInfo contact, FormFieldInfo ffi)
        {
            return string.IsNullOrEmpty(contact.GetStringValue(ffi.Name, null))
                   || allowOverwrite
                   || contact.ContactLastName.StartsWith(ContactHelper.ANONYMOUS, StringComparison.OrdinalIgnoreCase) && CMSString.Equals(ffi.Name, "contactlastname", true);
        }


        /// <summary>
        /// Helper method that returns country ID based on input value.
        /// </summary>
        /// <param name="value">Contains either int or string value (country ID or name)</param>
        private static int? GetCountryID(object value)
        {
            if (value is int)
            {
                var countryID = (int)value;

                // Get country by ID to check its existence
                if (CountryInfoProvider.GetCountryInfo(countryID) != null)
                {
                    return countryID;
                }

                return null;
            }

            if (value is string)
            {
                string countryName = ValidationHelper.GetString(value, string.Empty);
                if (!string.IsNullOrEmpty(countryName) && countryName.Contains(";"))
                {
                    // Get country name if value is in form '<CountryName>;<StateName>'
                    countryName = countryName.Remove(countryName.IndexOf(";", StringComparison.Ordinal));
                }

                // Get country object by code name
                CountryInfo country = CountryInfoProvider.GetCountryInfo(countryName);
                if (country != null)
                {
                    return country.CountryID;
                }
            }

            return null;
        }


        /// <summary>
        /// Helper method that returns state ID based on input value.
        /// </summary>
        /// <param name="value">Contains either int or string value (state ID or name)</param>
        private static int? GetStateID(object value)
        {
            if (value is int)
            {
                var stateID = (int)value;

                // Get state by ID to check its existence
                if (StateInfoProvider.GetStateInfo(stateID) != null)
                {
                    return stateID;
                }

                return null;
            }

            if (value is string)
            {
                string stateName = ValidationHelper.GetString(value, string.Empty);
                if (!string.IsNullOrEmpty(stateName) && stateName.Contains(";"))
                {
                    // Get state name if value is in form '<CountryName>;<StateName>'
                    stateName = stateName.Substring(stateName.IndexOf(";", StringComparison.Ordinal) + 1);
                }

                // Get state object by code name
                StateInfo state = StateInfoProvider.GetStateInfo(stateName);
                if (state != null)
                {
                    return state.StateID;
                }
            }

            return null;
        }
    }
}
