using System;

using CMS.Base;
using CMS.EventLog;
using CMS.Globalization;
using CMS.Helpers;
using CMS.IO;

using MaxMindGeoIP;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides IP address to GEO location conversion.
    /// </summary>
    public class GeoIPHelper : AbstractHelper<GeoIPHelper>
    {
        #region "Variable & constants

        private const string GEOIP_FOLDER = "~/App_Data/CMSModules/WebAnalytics/MaxMind/";
        private const string GEOIP_LOCATION_SERVICE_FILE = "GeoLiteCity.dat";
        private const string GEOIP_ORGANIZATION_SERVICE_FILE = "GeoIPOrg.dat";
        private const string GEOIP_CACHE_KEY = "maxmind";

        private static LookupService mIPToOrgService = null;
        private static LookupService mIPToLocationService = null;

        private static object locker = new object();
        private static string mLocationServiceFileName = null;
        private static string mLocationServiceFilePath = null;
        private static string mOrgServiceFilePath = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns name of LocationService file.
        /// </summary>
        private static string LocationServiceFileName
        {
            get
            {
                if (mLocationServiceFileName == null)
                {
                    mLocationServiceFileName = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSGeoIPLocationFileName"], GEOIP_LOCATION_SERVICE_FILE);
                }
                return mLocationServiceFileName;
            }
        }


        /// <summary>
        /// Returns physical file path to LocationService file.
        /// </summary>
        private static string LocationServiceFilePath
        {
            get
            {
                if (mLocationServiceFilePath == null)
                {
                    mLocationServiceFilePath = URLHelper.GetPhysicalPath(GEOIP_FOLDER + LocationServiceFileName);
                }
                return mLocationServiceFilePath;
            }
        }


        /// <summary>
        /// Returns physical file path to OrgService file.
        /// </summary>
        private static string OrgServiceFilePath
        {
            get
            {
                if (mOrgServiceFilePath == null)
                {
                    mOrgServiceFilePath = URLHelper.GetPhysicalPath(GEOIP_FOLDER + GEOIP_ORGANIZATION_SERVICE_FILE);
                }
                return mOrgServiceFilePath;
            }
        }


        /// <summary>
        /// Gets lookup service for country, region, city, ...
        /// </summary>
        private static LookupService IPToLocationService
        {
            get
            {
                if (mIPToLocationService == null)
                {
                    lock (locker)
                    {
                        if (File.Exists(LocationServiceFilePath))
                        {
                            mIPToLocationService = new LookupService(LocationServiceFilePath, LookupService.GEOIP_STANDARD);
                        }
                    }
                }

                return mIPToLocationService;
            }
            set
            {
                mIPToLocationService = value;
            }
        }


        /// <summary>
        /// Gets lookup service for organization
        /// </summary>
        private static LookupService IPToOrgService
        {
            get
            {
                if (mIPToOrgService == null)
                {
                    lock (locker)
                    {
                        if (File.Exists(OrgServiceFilePath))
                        {
                            mIPToOrgService = new LookupService(OrgServiceFilePath, LookupService.GEOIP_STANDARD);
                        }
                    }
                }

                return mIPToOrgService;
            }
            set
            {
                mIPToOrgService = value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns geo location according to current IP.
        /// </summary>
        public static GeoLocation GetCurrentGeoLocation()
        {
            return HelperObject.GetCurrentGeoLocationInternal();
        }


        /// <summary>
        /// Finds country name by IP4 address.
        /// </summary>
        /// <param name="dottedQuadIp">IP4 address</param>
        /// <returns>Country name</returns>
        public static string GetCountryByIp(string dottedQuadIp)
        {
            return HelperObject.GetCountryByIpInternal(dottedQuadIp);
        }


        /// <summary>
        /// Finds ID of country from CMS_Country table by IP4 address.
        /// </summary>
        /// <param name="dottedQuadIp">IP4 address</param>
        /// <returns>ID of country from CMS_Country table</returns>
        public static int GetCountryIDByIp(string dottedQuadIp)
        {
            return HelperObject.GetCountryIDByIpInternal(dottedQuadIp);
        }


        /// <summary>
        /// Finds state code name by IP4 address.
        /// </summary>
        /// <param name="dottedQuadIp">IP4 address</param>
        /// <returns>State code name compatible with states in CMS_State table</returns>
        public static string GetStateByIp(string dottedQuadIp)
        {
            return HelperObject.GetStateByIpInternal(dottedQuadIp);
        }


        /// <summary>
        /// Finds ID of state from CMS_State table by IP4 address.
        /// </summary>
        /// <param name="dottedQuadIp">IP4 address</param>
        /// <returns>ID of state from CMS_State table</returns>
        public static int GetStateIDByIp(string dottedQuadIp)
        {
            return HelperObject.GetStateIDByIpInternal(dottedQuadIp);
        }


        /// <summary>
        /// Returns location object according to IP4 address.
        /// </summary>
        /// <param name="dottedQuadIp">IP4 address</param>
        public static GeoLocation GetLocationByIp(string dottedQuadIp)
        {
            return HelperObject.GetLocationByIpInternal(dottedQuadIp);
        }


        /// <summary>
        /// Finds organization/company name by IP4 address.
        /// </summary>
        /// <param name="dottedQuadIp">IP4 address</param>
        /// <returns>Organization/company name</returns>
        public static string GetOrganizationByIp(string dottedQuadIp)
        {
            return HelperObject.GetOrganizationByIpInternal(dottedQuadIp);
        }


        /// <summary>
        /// Initializes the IP to location service with options.
        /// </summary>
        /// <param name="options">The options from LookupService. Like GEOIP_STANDARD, GEOIP_MEMORY_CACHE, etc.</param>
        public static void InitIPToLocationService(int options)
        {
            if (File.Exists(LocationServiceFilePath))
            {
                lock (locker)
                {
                    IPToLocationService = new LookupService(LocationServiceFilePath, options);
                }
            }
        }


        /// <summary>
        /// Initializes the IP to organization service with options.
        /// </summary>
        /// <param name="options">The options from LookupService. Like GEOIP_STANDARD, GEOIP_MEMORY_CACHE, etc.</param>
        public static void InitIPToOrgService(int options)
        {
            if (File.Exists(OrgServiceFilePath))
            {
                lock (locker)
                {
                    IPToOrgService = new LookupService(OrgServiceFilePath, options);
                }
            }
        }


        /// <summary>
        /// Clears MaxMind GeoIP lookup services.
        /// </summary>
        public static void ClearLookUpServices()
        {
            mIPToLocationService = null;
            mIPToOrgService = null;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns geo location according to current IP (RequestContext.UserHostAddress).
        /// </summary>
        protected virtual GeoLocation GetCurrentGeoLocationInternal()
        {
            GeoLocation location = GetLocation(RequestContext.UserHostAddress);
            if (location == null)
            {
                // Return empty objet for IntelliSense purposes if cannot be found through the service.
                location = new GeoLocation();
            }
            return location;
        }


        /// <summary>
        /// Finds country name by IP4 address.
        /// </summary>
        /// <param name="ip">IP4 address</param>
        /// <returns>Country name</returns>
        protected virtual string GetCountryByIpInternal(string ip)
        {
            if (IPToLocationService != null)
            {
                GeoLocation location = GetLocation(ip);
                if (location != null)
                {
                    return location.CountryName;
                }
            }

            return null;
        }


        /// <summary>
        /// Finds ID of country from CMS_Country table by IP4 address.
        /// </summary>
        /// <param name="ip">IP4 address</param>
        /// <returns>ID of country from CMS_Country table</returns>
        protected virtual int GetCountryIDByIpInternal(string ip)
        {
            GeoLocation location = GetLocation(ip);
            if (location != null)
            {
                return GetCMSCountryIDByCountryCode(location.CountryCode);
            }

            return 0;
        }


        /// <summary>
        /// Finds state code name by IP4 address.
        /// </summary>
        /// <param name="ip">IP4 address</param>
        /// <returns>State code name compatible with states in CMS_State table</returns>
        protected virtual string GetStateByIpInternal(string ip)
        {
            GeoLocation location = GetLocation(ip);
            if (location != null)
            {
                return location.StateName;
            }

            return null;
        }


        /// <summary>
        /// Finds ID of state from CMS_State table by IP4 address.
        /// </summary>
        /// <param name="ip">IP4 address</param>
        /// <returns>ID of state from CMS_State table</returns>
        protected virtual int GetStateIDByIpInternal(string ip)
        {
            GeoLocation location = GetLocation(ip);
            if (location != null)
            {
                // Try to get country ID
                int countryId = GetCMSCountryIDByCountryCode(location.CountryCode);

                // Try to get CMS state by its code
                StateInfo state = StateInfoProvider.GetStateInfoByCode(location.RegionCode);
                if ((state != null) && (state.CountryID == countryId))
                {
                    return state.StateID;
                }
            }

            return 0;
        }


        /// <summary>
        /// Returns location object according to IP4 address.
        /// </summary>
        /// <param name="ip">IP4 address</param>
        public virtual GeoLocation GetLocationByIpInternal(string ip)
        {
            return GetLocation(ip);
        }


        /// <summary>
        /// Finds organization/company name by IP4 address.
        /// </summary>
        /// <param name="ip">IP4 address</param>
        /// <returns>Organization/company name</returns>
        protected virtual string GetOrganizationByIpInternal(string ip)
        {
            if (IPToOrgService != null)
            {
                return IPToOrgService.getOrg(ip);
            }

            return null;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns ID of CMS country specified by country code.
        /// </summary>
        /// <param name="code">Country code</param>
        private static int GetCMSCountryIDByCountryCode(string code)
        {
            // Try to get CMS country by country code
            CountryInfo country = CountryInfoProvider.GetCountryInfoByCode(code);
            if (country != null)
            {
                return country.CountryID;
            }

            return 0;
        }


        /// <summary>
        /// Gets location object.
        /// </summary>
        /// <param name="dottedQuadIp">IP4 address</param>
        private static GeoLocation GetLocation(string dottedQuadIp)
        {
            if (IPToLocationService == null)
            {
                return null;
            }

            GeoLocation result = null;
            // Try to get location from cache
            using (var cs = new CachedSection<GeoLocation>(ref result, 1, true, null, GEOIP_CACHE_KEY, dottedQuadIp))
            {
                if (cs.LoadData)
                {
                    // Get location from location service
                    Location loc;
                    try
                    {
                        loc = IPToLocationService.getLocation(dottedQuadIp);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("Geolocation", "getLocation", ex);
                        loc = null;
                    }
                    result = (loc != null) ? new GeoLocation(loc) : null;

                    cs.Data = result;
                }
            }

            return result;
        }

        #endregion
    }
}
