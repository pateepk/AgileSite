using System.Device.Location;

using CMS.Base;
using CMS.Globalization;

using MaxMind.GeoIP2.Responses;

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
        private readonly object locker = new object();

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
                    lock (locker)
                    {
                        if (mState == null)
                        {
                            // Get state object based on retion/state code
                            mState = StateInfoProvider.GetStateInfoByCode(RegionCode);
                        }
                    }
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
        public double? Latitude
        {
            get;
            set;
        }


        /// <summary>
        /// Longitude
        /// </summary>
        public double? Longitude
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
        /// Metro code
        /// </summary>
        public int? MetroCode
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
        /// <param name="postalCode">Postal code</param>
        /// <param name="metroCode">Metro code</param>
        public GeoLocation(string coutryCode, string countryName, string regionCode, string regionName, string city, double latitude, double longitude, string postalCode, int metroCode)
        {
            CountryCode = coutryCode;
            CountryName = countryName;
            RegionCode = regionCode;
            RegionName = regionName;
            City = city;
            Latitude = latitude;
            Longitude = longitude;
            PostalCode = postalCode;
            MetroCode = metroCode;
        }


        /// <summary>
        /// Constructor initialized by MaxMind <see cref="CityResponse"/> object.
        /// </summary>
        /// <param name="cityResponse">Record returned from GeoIPCity database</param>
        internal GeoLocation(CityResponse cityResponse)
        {
            CountryCode = cityResponse.Country.IsoCode;

            // Try get CMS country based on given country code
            var ci = CountryInfoProvider.GetCountryInfoByCode(CountryCode);

            CountryName = (ci != null) ? ci.CountryDisplayName : cityResponse.Country.Name;
            RegionCode = cityResponse.MostSpecificSubdivision.IsoCode;
            RegionName = cityResponse.MostSpecificSubdivision.Name;
            City = cityResponse.City.Name;
            Latitude = cityResponse.Location.Latitude;
            Longitude = cityResponse.Location.Longitude;
            PostalCode = cityResponse.Postal.Code;
            MetroCode = cityResponse.Location.MetroCode;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns distance in kilometers between current location and location specified by coordinates.
        /// </summary>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <remarks>The Haversine formula is used to calculate the distance.</remarks>
        public double Distance(double latitude, double longitude)
        {
            if (Latitude == null || Longitude == null)
            {
                return 0;
            }

            GeoCoordinate currentLoc;
            GeoCoordinate specifiedLoc;
            try
            {
                currentLoc = new GeoCoordinate(Latitude.Value, Longitude.Value);
                specifiedLoc = new GeoCoordinate(latitude, longitude);
            }
            catch
            {
                // Coordinates are out of range
                return 0;
            }

            return currentLoc.GetDistanceTo(specifiedLoc) / 1000;
        }


        /// <summary>
        /// Registers columns of the GeoLocation object.
        /// </summary>
        protected override void RegisterColumns()
        {
            base.RegisterColumns();

            RegisterColumn("Country", m => GetCountry(m));
            RegisterColumn("State", m => m.State ?? new StateInfo());
            RegisterColumn("RegionCode", m => m.RegionCode);
            RegisterColumn("RegionName", m => m.RegionName);
            RegisterColumn("City", m => m.City);
            RegisterColumn("Latitude", m => m.Latitude);
            RegisterColumn("Longitude", m => m.Longitude);
            RegisterColumn("Postalcode", m => m.PostalCode);
            RegisterColumn("MetroCode", m => m.MetroCode);
        }


        /// <summary>
        /// Returns the country info object.
        /// </summary>
        /// <param name="location">GeoLocation object to get the data (CountryCode) from</param>
        private static object GetCountry(GeoLocation location)
        {
            var ci = CountryInfoProvider.GetCountryInfoByCode(location.CountryCode);
            return ci ?? new CountryInfo();
        }

        #endregion
    }
}
