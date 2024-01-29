using System;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Globalization;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class for setting location of contact according to the given IP address.
    /// </summary>
    public sealed class GeoLocationContactHelper
    {
        #region "Variables"

        private readonly ContactInfo mContact;
        private readonly string mSiteName;
        private readonly string mIPAddress;
        private string mSuffix;

        #endregion


        #region "Public methods and constructors"

        /// <summary>
        /// Constructor initializing objects with current context data.
        /// </summary>
        public GeoLocationContactHelper()
            : this(ContactManagementContext.CurrentContact, SiteContext.CurrentSiteName)
        {
        }


        /// <summary>
        /// Constructor initializing objects with parameters.
        /// </summary>
        /// <param name="contact">Contact to be updated</param>
        /// <param name="siteName">Site name used for settings check</param>
        public GeoLocationContactHelper(ContactInfo contact, string siteName)
        {
            mContact = contact;
            mSiteName = siteName;
            mIPAddress = RequestContext.UserHostAddress;
        }


        /// <summary>
        /// Update contact profile based on geolocation data from the IP address.
        /// </summary>
        /// <param name="saveContact">If true, the contact is saved after its location is updated</param>
        public void UpdateContactLocation(bool saveContact = true)
        {
            if (!String.IsNullOrEmpty(mIPAddress) && SettingsKeyInfoProvider.GetBoolValue("CMSCMEnableGeolocation", mSiteName) && HasSufficentLicense())
            {
                try
                {
                    UpdateLocation();
                    UpdateOrganization();

                    if (saveContact)
                    {
                        ContactInfoProvider.SetContactInfo(mContact);
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("Contact", "Geolocation", ex.InnerException ?? ex);
                }
            }
        }

        #endregion


        #region "Private methods"

        private bool HasSufficentLicense()
        {
            return LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.FullContactManagement);
        }


        /// <summary>
        /// Update contact's columns where location should be stored.
        /// </summary>
        private void UpdateLocation()
        {
            GeoLocation location = GeoIPHelper.GetLocationByIp(mIPAddress);
            if (location != null)
            {
                UpdateContactColumn("CMSCMGeoCountry", location.CountryCode);
                UpdateContactColumn("CMSCMGeoState", location.RegionCode);
                UpdateContactColumn("CMSCMGeoCity", location.City);
                UpdateContactColumn("CMSCMGeoPostal", location.PostalCode);
                UpdateContactColumn("CMSCMGeoArea", location.AreaCode);
                UpdateContactColumn("CMSCMGeoMetro", location.MetroCode);
                UpdateContactColumn("CMSCMGeoDMA", location.DMACode);
                UpdateContactColumn("CMSCMGeoLatitude", location.Latitude);
                UpdateContactColumn("CMSCMGeoLongitude", location.Longitude);
            }
        }


        /// <summary>
        /// Update contact's column where organization should be stored.
        /// </summary>
        private void UpdateOrganization()
        {
            string contactColumn = SettingsKeyInfoProvider.GetValue(mSiteName + ".CMSCMGeoOrganization");
            if (!String.IsNullOrEmpty(contactColumn) && String.IsNullOrEmpty(mContact.GetStringValue(contactColumn, null)))
            {
                string organization = GeoIPHelper.GetOrganizationByIp(mIPAddress);
                if (!String.IsNullOrEmpty(organization))
                {
                    mContact[contactColumn] = AppendSuffix(contactColumn, organization);
                }
            }
        }


        /// <summary>
        /// Update value in contact's column only if empty.
        /// </summary>
        /// <param name="geoIPsetting">GEO setting name</param>
        /// <param name="value">GEO value</param>
        private void UpdateContactColumn(string geoIPsetting, object value)
        {
            if (value != null)
            {
                string contactColumn = SettingsKeyInfoProvider.GetValue(mSiteName + "." + geoIPsetting);
                if (!String.IsNullOrEmpty(contactColumn) && String.IsNullOrEmpty(mContact.GetStringValue(contactColumn, null)))
                {
                    if (geoIPsetting == "CMSCMGeoCountry")
                    {
                        InsertCountryValue(contactColumn, value);
                    }
                    else if (geoIPsetting == "CMSCMGeoState")
                    {
                        InsertStateValue(contactColumn, value);
                    }
                    else
                    {
                        InsertValue(contactColumn, value);
                    }
                }
            }
        }


        /// <summary>
        /// Update country display name or ID.
        /// </summary>
        private void InsertCountryValue(string contactColumn, object value)
        {
            CountryInfo country = CountryInfoProvider.GetCountryInfoByCode((string)value);
            if (country != null)
            {
                if (mContact.Generalized.GetColumnType(contactColumn) == typeof(string))
                {
                    mContact[contactColumn] = AppendSuffix(contactColumn, country.CountryDisplayName);
                }
                else
                {
                    mContact[contactColumn] = country.CountryID;
                }
            }
        }


        /// <summary>
        /// Update state display name or ID.
        /// </summary>
        private void InsertStateValue(string contactColumn, object value)
        {
            StateInfo state = StateInfoProvider.GetStateInfoByCode((string)value);
            if ((state != null) && (state.CountryID == mContact.ContactCountryID))
            {
                if (mContact.Generalized.GetColumnType(contactColumn) == typeof(string))
                {
                    mContact[contactColumn] = AppendSuffix(contactColumn, state.StateDisplayName);
                }
                else
                {
                    mContact[contactColumn] = state.StateID;
                }
            }
        }


        /// <summary>
        /// Insert value.
        /// </summary>
        private void InsertValue(string contactColumn, object value)
        {
            if ((value is int) && (ValidationHelper.GetInteger(value, 0) > 0) || !(value is int))
            {
                mContact[contactColumn] = AppendSuffix(contactColumn, value);
            }
        }


        /// <summary>
        /// Append suffix into string column.
        /// </summary>
        private object AppendSuffix(string contactColumn, object value)
        {
            if (mContact.Generalized.GetColumnType(contactColumn) == typeof(string))
            {
                if (mSuffix == null)
                {
                    mSuffix = SettingsKeyInfoProvider.GetValue(mSiteName + ".CMSCMGeoSuffix");
                    if (mSuffix != String.Empty)
                    {
                        mSuffix = " " + mSuffix;
                    }
                }

                return value + mSuffix;
            }

            return value;
        }

        #endregion
    }
}