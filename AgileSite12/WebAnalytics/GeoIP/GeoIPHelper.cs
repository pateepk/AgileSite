using System;

using CMS.Base;
using CMS.Globalization;
using CMS.Helpers;
using CMS.IO;

using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides IP address to GEO location conversion.
    /// </summary>
    public class GeoIPHelper : AbstractHelper<GeoIPHelper>, IDisposable
    {
        #region "Variable & constants

        private const string GEOIP_FOLDER = "~/App_Data/CMSModules/WebAnalytics/MaxMind/";
        private const string GEOIP_SERVICE_FILE = "GeoLite2-City.mmdb";
        private const string GEOIP_ASN_SERVICE_FILE = "GeoLite2-ASN.mmdb";
        private const string GEOIP_CACHE_KEY = "maxmind";
        private const string GEOIP_ASN_CACHE_KEY = "maxmind_asn";

        private static IGeoIP2DatabaseReader mIPToLocationService;
        private static IGeoIP2DatabaseReader mIPToAsnService;

        private static readonly object locker = new object();
        private static string mLocationServiceFilePath;
        private static string mAsnServiceFilePath;

        private bool disposed;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns name of city database file.
        /// </summary>
        private static string LocationServiceFileName
        {
            get
            {
                return SettingsHelper.AppSettings["CMSGeoIPLocationFileName"] ?? GEOIP_SERVICE_FILE;
            }
        }


        /// <summary>
        /// Returns physical file path to city database file.
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
        /// Returns name of ASN database file.
        /// </summary>
        private static string AsnServiceFileName
        {
            get
            {
                return SettingsHelper.AppSettings["CMSGeoIPAsnFileName"] ?? GEOIP_ASN_SERVICE_FILE;
            }
        }


        /// <summary>
        /// Returns physical file path to ASN database file.
        /// </summary>
        private static string AsnServiceFilePath
        {
            get
            {
                if (mAsnServiceFilePath == null)
                {
                    mAsnServiceFilePath = URLHelper.GetPhysicalPath(GEOIP_FOLDER + AsnServiceFileName);
                }
                return mAsnServiceFilePath;
            }
        }


        /// <summary>
        /// Database reader for querying the city database.
        /// </summary>
        internal static IGeoIP2DatabaseReader LocationDatabaseReader
        {
            get
            {
                if (mIPToLocationService == null)
                {
                    lock (locker)
                    {
                        if ((mIPToLocationService == null) && File.Exists(LocationServiceFilePath))
                        {
                            mIPToLocationService = new DatabaseReader(LocationServiceFilePath);
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
        /// Database reader for querying the ASN database.
        /// </summary>
        internal static IGeoIP2DatabaseReader AsnDatabaseReader
        {
            get
            {
                if (mIPToAsnService == null)
                {
                    lock (locker)
                    {
                        if ((mIPToAsnService == null) && File.Exists(AsnServiceFilePath))
                        {
                            mIPToAsnService = new DatabaseReader(AsnServiceFilePath);
                        }
                    }
                }

                return mIPToAsnService;
            }
            set
            {
                mIPToAsnService = value;
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
        /// Finds organization/company name associated with registered ASN by IP4 address.
        /// </summary>
        /// <param name="dottedQuadIp">IP4 address</param>
        /// <returns>Organization/company name</returns>
        public static string GetOrganizationByIp(string dottedQuadIp)
        {
            return HelperObject.GetOrganizationByIpInternal(dottedQuadIp);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns geo location according to current IP (RequestContext.UserHostAddress).
        /// </summary>
        protected virtual GeoLocation GetCurrentGeoLocationInternal()
        {
            var location = GetLocation(RequestContext.UserHostAddress);

            // Return empty objet for IntelliSense purposes if cannot be found through the service
            return location ?? new GeoLocation();
        }


        /// <summary>
        /// Finds country name by IP4 address.
        /// </summary>
        /// <param name="ip">IP4 address</param>
        /// <returns>Country name</returns>
        protected virtual string GetCountryByIpInternal(string ip)
        {
            var location = GetLocation(ip);

            return location?.CountryName;
        }


        /// <summary>
        /// Finds ID of country from CMS_Country table by IP4 address.
        /// </summary>
        /// <param name="ip">IP4 address</param>
        /// <returns>ID of country from CMS_Country table</returns>
        protected virtual int GetCountryIDByIpInternal(string ip)
        {
            var location = GetLocation(ip);
            if (location != null)
            {
                return GetCMSCountryIDByCountryCode(location.CountryCode);
            }

            return 0;
        }


        /// <summary>
        /// Finds state name by IP4 address.
        /// </summary>
        /// <param name="ip">IP4 address</param>
        /// <returns>State name compatible with states in CMS_State table</returns>
        protected virtual string GetStateByIpInternal(string ip)
        {
            var location = GetLocation(ip);

            return location?.StateName;
        }


        /// <summary>
        /// Finds ID of state from CMS_State table by IP4 address.
        /// </summary>
        /// <param name="ip">IP4 address</param>
        /// <returns>ID of state from CMS_State table</returns>
        protected virtual int GetStateIDByIpInternal(string ip)
        {
            var location = GetLocation(ip);
            if (location != null)
            {
                // Try to get country ID
                int countryId = GetCMSCountryIDByCountryCode(location.CountryCode);

                // Try to get CMS state by its code
                var state = StateInfoProvider.GetStateInfoByCode(location.RegionCode);
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
        protected virtual GeoLocation GetLocationByIpInternal(string ip)
        {
            return GetLocation(ip);
        }


        /// <summary>
        /// Finds organization/company name associated with registered ASN by IP4 address.
        /// </summary>
        /// <param name="ip">IP4 address</param>
        /// <returns>Organization/company name</returns>
        protected virtual string GetOrganizationByIpInternal(string ip)
        {
            return GetOrganization(ip);
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
            var country = CountryInfoProvider.GetCountryInfoByCode(code);

            return (country != null) ? country.CountryID : 0;
        }


        /// <summary>
        /// Gets location object.
        /// </summary>
        /// <param name="dottedQuadIp">IP4 address</param>
        private static GeoLocation GetLocation(string dottedQuadIp)
        {
            if (LocationDatabaseReader == null)
            {
                return null;
            }
            
            GeoLocation result = null;
            using (var cs = new CachedSection<GeoLocation>(ref result, 1, true, null, GEOIP_CACHE_KEY, dottedQuadIp))
            {
                if (cs.LoadData)
                {
                    if (LocationDatabaseReader.TryCity(dottedQuadIp, out CityResponse response))
                    {
                        result = new GeoLocation(response);
                    }

                    cs.Data = result;
                }
            }

            return result;
        }


        /// <summary>
        /// Gets organization name associated with the ASN for the specified IP address.
        /// </summary>
        /// <param name="dottedQuadIp">IP4 address</param>
        private static string GetOrganization(string dottedQuadIp)
        {
            if (AsnDatabaseReader == null)
            {
                return null;
            }

            string result = null;
            using (var cs = new CachedSection<string>(ref result, 1, true, null, GEOIP_ASN_CACHE_KEY, dottedQuadIp))
            {
                if (cs.LoadData)
                {
                    if (AsnDatabaseReader.TryAsn(dottedQuadIp, out AsnResponse asnResponse))
                    {
                        result = asnResponse.AutonomousSystemOrganization;
                    }

                    cs.Data = result;
                }
            }

            return result;
        }

        #endregion


        #region "IDisposable"

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="GeoIPHelper"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">Indicates whether managed resources are to be released.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    (mIPToLocationService as IDisposable)?.Dispose();
                    (mIPToAsnService as IDisposable)?.Dispose();
                }

                disposed = true;
            }
        }


        /// <summary>
        /// Releases all resources used by the <see cref="GeoIPHelper"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
