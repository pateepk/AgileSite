using System.Collections.Generic;
using CMS.Globalization;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using MaxMindGeoIP;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class representing location of specific IP.
    /// </summary>
    public class GeoLocation : AbstractDataContainer<GeoLocation>
    {
        #region "Variables"

        private string mStateCode;

        private string mStateName;

        private StateInfo mState;

        #endregion


        #region "Properties"

        /// <summary>
        /// Country code
        /// </summary>
        public string CountryCode
        {
            get;
            set;
        }


        /// <summary>
        /// Country name
        /// </summary>
        public string CountryName
        {
            get;
            set;
        }


        /// <summary>
        /// State code
        /// </summary>
        public string StateCode
        {
            get
            {
                if (string.IsNullOrEmpty(mStateCode))
                {
                    if (State != null)
                    {
                        // Set state code
                        mStateCode = State.StateCode;
                    }
                }

                return mStateCode;
            }
            set
            {
                mStateCode = value;
            }
        }


        /// <summary>
        /// State name
        /// </summary>
        public string StateName
        {
            get
            {
                if (string.IsNullOrEmpty(mStateName))
                {
                    if (State != null)
                    {
                        // Set state code
                        mStateName = State.StateDisplayName;
                    }
                }

                return mStateName;
            }
            set
            {
                mStateName = value;
            }
        }


        /// <summary>
        /// State object
        /// </summary>
        private StateInfo State
        {
            get
            {
                if (mState == null)
                {
                    // Get state object based on retion/state code
                    mState = StateInfoProvider.GetStateInfoByCode(RegionCode);
                }

                return mState;
            }
        }


        /// <summary>
        /// Region/state code
        /// </summary>
        public string RegionCode
        {
            get;
            set;
        }


        /// <summary>
        /// Region/state name
        /// </summary>
        public string RegionName
        {
            get;
            set;
        }


        /// <summary>
        /// City name
        /// </summary>
        public string City
        {
            get;
            set;
        }


        /// <summary>
        /// Latitude
        /// </summary>
        public double Latitude
        {
            get;
            set;
        }


        /// <summary>
        /// Longitude
        /// </summary>
        public double Longitude
        {
            get;
            set;
        }


        /// <summary>
        /// Area code
        /// </summary>
        public int AreaCode
        {
            get;
            set;
        }


        /// <summary>
        /// Postal code
        /// </summary>
        public string PostalCode
        {
            get;
            set;
        }


        /// <summary>
        /// DMA code
        /// </summary>
        public int DMACode
        {
            get;
            set;
        }


        /// <summary>
        /// Metro code
        /// </summary>
        public int MetroCode
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public GeoLocation()
        {
        }


        /// <summary>
        /// Basic constructor.
        /// </summary>
        public GeoLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }


        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="coutryCode">Country code</param>
        /// <param name="countryName">Country name</param>
        /// <param name="regionCode">Region/state code</param>
        /// <param name="regionName">Region/state name</param>
        /// <param name="city">City name</param>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <param name="areaCode">Area code</param>
        /// <param name="postalCode">Postal code</param>
        /// <param name="dmaCode">DMA code</param>
        /// <param name="metroCode">Metro code</param>
        public GeoLocation(string coutryCode, string countryName, string regionCode, string regionName, string city, double latitude, double longitude, int areaCode, string postalCode, int dmaCode, int metroCode)
        {
            CountryCode = coutryCode;
            CountryName = countryName;
            RegionCode = regionCode;
            RegionName = regionName;
            City = city;
            Latitude = latitude;
            Longitude = longitude;
            AreaCode = areaCode;
            PostalCode = postalCode;
            DMACode = dmaCode;
            MetroCode = metroCode;
        }


        /// <summary>
        /// Constructor initialized by MaxMind location object.
        /// </summary>
        /// <param name="location">Location object</param>
        internal GeoLocation(Location location)
        {
            CountryCode = location.countryCode;

            // Try get CMS country based on given country code
            CountryInfo ci = CountryInfoProvider.GetCountryInfoByCode(location.countryCode);
            if (ci != null)
            {
                CountryName = ci.CountryDisplayName;
            }
            else
            {
                CountryName = location.countryName;
            }

            RegionCode = location.region;
            RegionName = location.regionName;
            City = location.city;
            Latitude = location.latitude;
            Longitude = location.longitude;
            AreaCode = location.area_code;
            PostalCode = location.postalCode;
            DMACode = location.dma_code;
            MetroCode = location.metro_code;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns distance in kilometers between current location and location specified by coordinates.
        /// </summary>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        public double Distance(double latitude, double longitude)
        {
            // Create location objects based on current and specified position
            Location currentLoc = new Location() { latitude = Latitude, longitude = Longitude };
            Location specifiedLoc = new Location() { latitude = latitude, longitude = longitude };

            // Return distance between two locations
            return currentLoc.distance(specifiedLoc);
        }


        /// <summary>
        /// Registers columns of the GeoLocation object.
        /// </summary>
        protected override void RegisterColumns()
        {
            base.RegisterColumns();

            RegisterColumn("Country", m => GetCountry(m));
            RegisterColumn("State", m => (m.State != null) ? m.State : new StateInfo());
            RegisterColumn("RegionCode", m => m.RegionCode);
            RegisterColumn("RegionName", m => m.RegionName);
            RegisterColumn("City", m => m.City);
            RegisterColumn("Latitude", m => m.Latitude);
            RegisterColumn("Longitude", m => m.Longitude);
            RegisterColumn("Areacode", m => m.AreaCode);
            RegisterColumn("Postalcode", m => m.PostalCode);
            RegisterColumn("DMACode", m => m.DMACode);
            RegisterColumn("MetroCode", m => m.MetroCode);
        }


        /// <summary>
        /// Returns the country info object.
        /// </summary>
        /// <param name="location">GeoLocation object to get the data (CountryCode) from</param>
        private static object GetCountry(GeoLocation location)
        {
            CountryInfo ci = CountryInfoProvider.GetCountryInfoByCode(location.CountryCode);
            return (ci != null) ? ci : new CountryInfo();
        }

        #endregion
    }
}
