using System;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.IO;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Basic Google maps control that automates databinding.
    /// </summary>
    [DefaultProperty("Text"), ToolboxData("<{0}:BasicGoogleMaps runat=server></{0}:BasicGoogleMaps>")]
    public class BasicGoogleMaps : CMSAbstractMap
    {
        #region "Google service constants"

        /// <summary>
        /// Indicates that no errors occurred. The address was successfully parsed and at least one geocode was returned.
        /// </summary>
        public const string OK = "OK";


        /// <summary>
        /// Indicates that you are over your quota.
        /// </summary>
        public const string OVER_QUERY_LIMIT = "OVER_QUERY_LIMIT";


        /// <summary>
        /// Google API URL in the form of protocol-agnostic URL.
        /// </summary>
        private const string GOOGLE_API_URL = "//maps.google.com/maps/api/js";

        #endregion


        #region "Private variables"

        private static BasicGoogleMaps mProviderObject = null;
        private static Regex mRegJsonStatus = null;
        private static Regex mRegJsonCoordinates = null;

        #endregion


        #region "Private properties"

        /// <summary>
        /// Regular expression for parsing Json response to get coordinates.
        /// </summary>
        private static Regex RegJsonCoordinates
        {
            get
            {
                if (mRegJsonCoordinates == null)
                {
                    mRegJsonCoordinates = RegexHelper.GetRegex("\"location\"\\s*:\\s*\\W\\s*\"lat\"\\s*:\\s*([+-]?\\d+\\.\\d+)[\\w\\W]+?\"lng\"\\s*:\\s*([+-]?\\d+\\.\\d+)");
                }
                return mRegJsonCoordinates;
            }
        }


        /// <summary>
        /// Regular expression for parsing Json response to get response status.
        /// </summary>
        private static Regex RegJsonStatus
        {
            get
            {
                if (mRegJsonStatus == null)
                {
                    mRegJsonStatus = RegexHelper.GetRegex("status\"\\s*:\\s*\"(\\w+)");
                }
                return mRegJsonStatus;
            }
        }


        /// <summary>
        /// Provider object.
        /// </summary>
        protected static BasicGoogleMaps ProviderObject
        {
            get
            {
                if (mProviderObject == null)
                {
                    mProviderObject = new BasicGoogleMaps();
                }
                return mProviderObject;
            }
            set
            {
                mProviderObject = value;
            }
        }

        #endregion


        #region "Public Properties"

        /// <summary>
        /// API key for communicating with Google services.
        /// </summary>
        public string ApiKey
        {
            get;
            set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Registers necessary Google javascript files.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="mapId">Map Id</param>
        /// <param name="mainScriptPath">Map script path</param>
        /// <param name="apiKey">API key for communicating with Google services.</param>
        public static void RegisterMapScripts(Page page, string mapId, string mainScriptPath, string apiKey)
        {
            // Google maps API URL
            string query = QueryHelper.BuildQuery("v", "3", "key", apiKey);
            string apiUrl = URLHelper.AppendQuery(GOOGLE_API_URL, query);
            apiUrl = HTMLHelper.EncodeForHtmlAttribute(apiUrl);

            // Register Google API
            ScriptHelper.RegisterClientScriptBlock(page, typeof(string), "googleMapsScriptFile", ScriptHelper.GetScriptTag(apiUrl));

            // Register OnLoad script
            RegisterOnLoadScript(page);

            // Register startup script
            if (!String.IsNullOrEmpty(mapId))
            {
                ScriptHelper.RegisterStartupScript(page, typeof(string), "googleMapsStartup_" + mapId, ScriptHelper.GetScript("addLoadEvent(MapLoad_" + mapId + ");"));
            }

            // Register GoogleMaps helper javascript file
            if (!String.IsNullOrEmpty(mainScriptPath))
            {
                ScriptHelper.RegisterScriptFile(page, mainScriptPath);
            }

            // Register Geocoder object
            ScriptHelper.RegisterClientScriptBlock(page, typeof(string), "googleMapsGeocoder", ScriptHelper.GetScript("var geocoder = new google.maps.Geocoder();"));
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// OnItemDataBound override.
        /// </summary>
        protected override void OnItemDataBound(RepeaterItemEventArgs e)
        {
            base.OnItemDataBound(e);
            // Add each content item to javascript array
            e.Item.Controls.AddAt(0, new LiteralControl("content_" + MapProperties.MapId + ".push({'cont':'##TR##"));
            e.Item.Controls.Add(new LiteralControl("##TRE##'});\n"));
        }


        /// <summary>
        /// OnPreRender override.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!DataHelper.DataSourceIsEmpty(DataSource) || !HideControlForZeroRows || !StopProcessing)
            {
                // Register Google javascript files
                RegisterMapScripts(Page, MapProperties.MapId, MainScriptPath, ApiKey);
            }
        }


        /// <summary>
        /// Render override.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            // Is empty, apply the hiding rules
            if (DataHelper.DataSourceIsEmpty(DataSource))
            {
                // Hide the control when HideControlForZeroRows is set
                if (HideControlForZeroRows)
                {
                    return;
                }
                else if (!string.IsNullOrEmpty(ZeroRowsText))
                {
                    // If the ZeroRowsText is set, display it instead if the control itself
                    writer.Write(ZeroRowsText);
                    return;
                }
            }

            StringBuilder markerArray = new StringBuilder();
            StringWriter sw = new StringWriter(markerArray);
            Html32TextWriter mwriter = new Html32TextWriter(sw);
            base.Render(mwriter);
            sw.Dispose();

            ArrayList contentArray = new ArrayList();

            // Cache coordinates if server processing is enabled
            if (MapProperties.EnableServerProcessing)
            {
                if (!string.IsNullOrEmpty(CacheItemName))
                {
                    CacheItemName += "|" + GetViewMode();
                }

                // Custom cache key name for LiveSite and the other view modes 
                string customCacheItemName = CacheHelper.GetCacheItemName(CacheItemName, MapProperties.MapName, "GoogleMap", GetViewMode());
                using (var cs = new CachedSection<ArrayList>(ref contentArray, CacheMinutes, true, null, customCacheItemName, null))
                {
                    if (cs.LoadData)
                    {
                        contentArray = GetContentArray(DataSource);

                        if (cs.Cached)
                        {
                            cs.CacheDependency = CacheHelper.GetCacheDependency(CacheDependencies);
                        }

                        cs.Data = contentArray;
                    }
                }
            }
            else
            {
                contentArray = GetContentArray(DataSource);
            }

            // Generate map with corrected marker content array
            outputHtml += GenerateMapInternal(MapProperties, RepairInputString(markerArray.ToString()), contentArray);
            // Render Html
            writer.Write(outputHtml);
        }


        /// <summary>
        /// Generates static map.
        /// </summary>
        /// <param name="mp">Map properties</param>
        /// <param name="cacheMinutes">Cache minutes</param>
        /// <param name="cacheItemName">Cache item name</param>
        /// <param name="InstanceGUID">Web part instance guid</param>
        public static string GenerateMap(CMSMapProperties mp, int cacheMinutes, string cacheItemName, Guid InstanceGUID)
        {
            double[] geoResponse = null;

            // Cache marker coordinates 
            if (mp.Latitude == null && mp.Longitude == null && !string.IsNullOrEmpty(mp.Location.Trim()) && mp.EnableServerProcessing)
            {
                string viewMode = GetViewMode();

                if (!string.IsNullOrEmpty(cacheItemName))
                {
                    cacheItemName += "|" + viewMode;
                }

                // Custom cache key name for LiveSite and the other view modes 
                string customCacheItemName = CacheHelper.GetCacheItemName(cacheItemName, mp.Location, "StaticGoogleMap", viewMode);
                using (var cs = new CachedSection<double[]>(ref geoResponse, cacheMinutes, true, null, customCacheItemName, null))
                {
                    if (cs.LoadData)
                    {
                        // Cache response
                        geoResponse = ProviderObject.GetGeoCoordinates(mp.Location);

                        // Flush cache if web part properties are changed
                        if (cs.Cached)
                        {
                            cs.CacheDependency = CacheHelper.GetCacheDependency("webpartinstance|" + InstanceGUID.ToString().ToLowerCSafe());
                        }

                        cs.Data = geoResponse;
                    }
                }
            }

            if (geoResponse != null)
            {
                // Get coordinates from cache
                mp.Latitude = geoResponse[0];
                mp.Longitude = geoResponse[1];
            }

            return ProviderObject.GenerateMapInternal(mp, null, null);
        }


        /// <summary>
        /// Returns ArrayList of content table from DataSource.
        /// </summary>
        /// <param name="dataSource">DataSource object</param>
        private ArrayList GetContentArray(object dataSource)
        {
            ArrayList contentArray = new ArrayList();

            string latitudeField = MapProperties.LatitudeField;
            string longitudeField = MapProperties.LongitudeField;
            string tooltipField = MapProperties.ToolTipField;
            string locationField = MapProperties.LocationField;
            string iconField = MapProperties.IconField;

            if (!DataHelper.DataSourceIsEmpty(dataSource))
            {
                DataTable dt = DataHelper.GetDataTable(dataSource);

                foreach (DataRow dr in dt.Rows)
                {
                    double? lat = null;
                    // Ensure latitude
                    string latTemp = DataHelper.GetNotEmpty(DataHelper.GetDataRowValue(dr, latitudeField), "");
                    if (!string.IsNullOrEmpty(latTemp))
                    {
                        lat = ValidationHelper.GetDouble(latTemp, 0);
                    }
                    double? lng = null;
                    // Ensure longitude    
                    string lngTemp = DataHelper.GetNotEmpty(DataHelper.GetDataRowValue(dr, longitudeField), "");
                    if (!string.IsNullOrEmpty(lngTemp))
                    {
                        lng = ValidationHelper.GetDouble(lngTemp, 0);
                    }
                    string tooltip = DataHelper.GetStringValue(dr, tooltipField).Trim();

                    string location = String.Empty;
                    if (!string.IsNullOrEmpty(locationField))
                    {
                        foreach (string loc in locationField.Split(';'))
                        {
                            location += DataHelper.GetStringValue(dr, loc).Trim() + ",";
                        }

                        location = location.Trim(',');
                    }

                    string iconURL = DataHelper.GetStringValue(dr, iconField).Trim();

                    // Ensure correct coordinates
                    lat = EnsureLatitude(lat);
                    lng = EnsureLongitude(lng);

                    if (lat == null && lng == null && MapProperties.EnableServerProcessing && !string.IsNullOrEmpty(location))
                    {
                        // Get coordinates from Google service
                        double[] geoResult = GetGeoCoordinates(location);
                        lat = geoResult[0];
                        lng = geoResult[1];
                    }
                    contentArray.Add(new ArrayList() { lat, lng, location, tooltip, iconURL });
                }
            }

            return contentArray;
        }


        /// <summary>
        /// Generates complete map code.
        /// </summary>
        /// <param name="mp">Map properties</param>
        /// <param name="markerArray">Marker array</param>
        /// <param name="contentArray">ArrayList of content table</param>
        private string GenerateMapInternal(CMSMapProperties mp, string markerArray, ArrayList contentArray)
        {
            StringBuilder result = new StringBuilder();

            // Initialize local variables from CMSMapProperties
            string mapId = mp.MapId;
            string mapName = mp.MapName;
            int scale = mp.Scale;
            string height = mp.Height;
            string width = mp.Width;
            string location = mp.Location;

            // Marker content - global variable
            result.AppendFormat("var content_{0} = [];\n", mapId);

            // Get the Navigation control type constant
            string navControl = "DEFAULT";
            switch (mp.ZoomControlType)
            {
                case 1:
                    navControl = "SMALL";
                    break;
                case 2:
                    navControl = "LARGE";
                    break;
            }

            // Check if single marker is generated - detail mode
            bool detailMode = false;

            double? lat;
            double? lng;

            // Ensure correct latitude and longitude
            if (contentArray != null)
            {
                // Add marker array
                result.Append(markerArray);

                if (contentArray.Count == 1)
                {
                    detailMode = true;

                    ArrayList coordinates = contentArray[0] as ArrayList;
                    // Latitude                    
                    lat = (double?)coordinates[0];
                    // Longitude
                    lng = (double?)coordinates[1];
                }
                else
                {
                    lat = mp.Latitude;
                    lng = mp.Longitude;
                }
            }
            else
            {
                detailMode = true;
                result.AppendFormat("content_{0}.push({{'cont': '{1}'}});\n", mapId, RepairMarkerContent(mp.Content));

                lat = mp.Latitude;
                lng = mp.Longitude;
            }

            lat = EnsureLatitude(lat);
            lng = EnsureLongitude(lng);

            result.AppendFormat("var {0} = null;\n", mapName);

            // Register map geo code handler for map center
            if (!mp.EnableServerProcessing && !string.IsNullOrEmpty(location.Trim()) && lat == null && lng == null && !detailMode)
            {
                result.AppendFormat(@"
function gmapH_{0}(results, status){{
    if (status == google.maps.GeocoderStatus.OK){{
        {0}.setCenter(new google.maps.LatLng(results[0].geometry.location.lat(), results[0].geometry.location.lng()));
    }}
}}
", mapName);
            }

            // Map definition code
            result.AppendFormat(@"
function MapLoad_{1}(){{
var myOptions = {{
    mapTypeId: google.maps.MapTypeId.{2},
    mapTypeControl: {3},
    scaleControl: {4},
    streetViewControl: {5},
    zoomControl: {6},
    keyboardShortcuts: {7},
    draggable: {8},
    zoomControlOptions: {{
        style: google.maps.ZoomControlStyle.{9} 
    }}
}};  
{0} = new google.maps.Map(document.getElementById(""{0}""), myOptions);
{0}.setZoom({10});
", mapName, mapId, mp.MapType.ToUpperCSafe(), mp.ShowMapTypeControl.ToString().ToLowerCSafe(), mp.ShowScaleControl.ToString().ToLowerCSafe(),
                mp.ShowStreetViewControl.ToString().ToLowerCSafe(), mp.ShowZoomControl.ToString().ToLowerCSafe(), mp.EnableKeyboardShortcuts.ToString().ToLowerCSafe(), mp.EnableMapDragging.ToString().ToLowerCSafe(), navControl, scale);

            // Center of the map
            if (!detailMode)
            {
                // Map center according to default lat and lng
                if (lat != null || lng != null || (string.IsNullOrEmpty(location.Trim())))
                {
                    // If latitude or longitude is not specified, set to zero
                    lat = lat ?? 0;
                    lng = lng ?? 0;
                    result.AppendFormat("{0}.setCenter(new google.maps.LatLng({1}, {2}));\n", mapName, lat.ToString().Replace(",", "."), lng.ToString().Replace(",", "."));
                }
                else if (mp.EnableServerProcessing)
                {
                    // Get coordinates from Google service for map center
                    double[] latLon = GetGeoCoordinates(mp.Location.Trim());
                    // Server processing according to location
                    result.AppendFormat("{0}.setCenter(new google.maps.LatLng({1}, {2}));\n", mapName, latLon[0], latLon[1]);
                }
                else
                {
                    // Client processing according to location
                    result.AppendFormat("geocoder.geocode({{ 'address': {1} }}, gmapH_{0});\n", mapName, ScriptHelper.GetString(location));
                }
            }

            // Generate markers from ArrayList
            if (contentArray != null)
            {
                // Marker array index
                int index = 0;
                foreach (ArrayList item in contentArray)
                {
                    //item => [0] = latitude, [1] = longitude, [2] = location, [3] = tooltip, [4] = iconURL
                    double? iLatitude = (double?)item[0];
                    double? iLongitude = (double?)item[1];
                    string iLocation = ValidationHelper.GetString(item[2], "");
                    string iTooltip = ValidationHelper.GetString(item[3], "");
                    string iIconURL = ValidationHelper.GetString(item[4], "");

                    // Generate marker
                    result.Append(GenerateMapMarker(iLatitude, iLongitude, iLocation, iTooltip, index, mp, detailMode, iIconURL));
                    index++;
                }
            }
            else
            {
                // Generate single marker with same map center
                result.Append(GenerateMapMarker(lat, lng, mp.Location, mp.ToolTip, 0, mp, true, mp.IconURL));
            }

            //End of main map function
            result.Append("}");


            // Ensure width height definition
            if (ValidationHelper.IsInteger(height))
            {
                height = height + "px";
            }
            if (ValidationHelper.IsInteger(width))
            {
                width = width + "px";
            }

            StringBuilder mapHolder = new StringBuilder();

            // Add map holder
            mapHolder.AppendFormat($"<div style=\"{GetHolderStyle(height, width)}\" id=\"{mapName}\"></div>");

            // Return complete map code with javascript tags and add map holder to result
            return ScriptHelper.GetScript(result.ToString()) + mapHolder;
        }


        private static string GetHolderStyle(string height, string width)
        {
            var style = new StringBuilder();
            style.Append("position:relative;");
            if (!string.IsNullOrEmpty(width))
            {
                style.Append($"width:{width};");
            }
            if (!string.IsNullOrEmpty(height))
            {
                style.Append($"height:{height};");
            }

            return style.ToString();
        }


        /// <summary>
        /// Generates Google map marker.
        /// </summary>
        /// <param name="lat">Marker latitude</param>
        /// <param name="lng">Marker longitude</param>
        /// <param name="location">Marker location</param>
        /// <param name="toolTip">Marker tooltip</param>
        /// <param name="markerIndex">Marker array index</param>
        /// <param name="mp">Map Properties</param>
        /// <param name="setMapCenter">Indicates if map center is same as single marker</param>
        /// <param name="iconURL">Custom marker URL</param>
        private string GenerateMapMarker(double? lat, double? lng, string location, string toolTip, int markerIndex, CMSMapProperties mp, bool setMapCenter, string iconURL)
        {
            StringBuilder result = new StringBuilder();

            // Marker content array reference
            string markerRef = "content_" + mp.MapId + "[" + markerIndex + "]['cont']";

            // Custom marker icon
            if (!string.IsNullOrEmpty(iconURL))
            {
                iconURL = ResolveUrl(iconURL);
            }

            if ((lat == null) && (lng == null) && !mp.EnableServerProcessing && !string.IsNullOrEmpty(location.Trim()))
            {
                // Get coordinates on client side and add marker to map
                result.AppendFormat(@"
geocoder.geocode({{ 'address': {3} }}, gH_{1}_{2});
function gH_{1}_{2}(results, status){{
            if (status == google.maps.GeocoderStatus.OK){{" + (setMapCenter ? @"
                {0}.setCenter(new google.maps.LatLng(results[0].geometry.location.lat(), results[0].geometry.location.lng()));" : "") + @"
                addGoogleMarker({0}, results[0].geometry.location.lat(), results[0].geometry.location.lng(), {4}, {5}, {6}, {7});
            }}
}}
", mp.MapName, mp.MapId, markerIndex, ScriptHelper.GetString(location), ScriptHelper.GetString(toolTip.Trim()), markerRef.Trim(), mp.ZoomScale, ScriptHelper.GetString(iconURL));
            }

            else if ((lat != null) || (lng != null))
            {
                // If latitude or longitude is not specified, set to zero
                lat = lat ?? 0;
                lng = lng ?? 0;

                // Add marker to map without server/client geo processing
                result.AppendFormat((setMapCenter ? "{0}.setCenter(new google.maps.LatLng({1}, {2}));\n" : "") +
                                    "addGoogleMarker({0}, {1}, {2}, {3}, {4}, {5}, {6});\n",
                                    mp.MapName, lat.ToString().Replace(",", "."), lng.ToString().Replace(",", "."), ScriptHelper.GetString(toolTip.Trim()), markerRef.Trim(), mp.ZoomScale, ScriptHelper.GetString(iconURL));
            }
            else if (setMapCenter)
            {
                // Set default map focus
                result.AppendFormat("{0}.setCenter(new google.maps.LatLng(0, 0));\n", mp.MapName);
            }

            return result.ToString();
        }


        /// <summary>
        /// Returns coordinates according to location address from Google service.
        /// </summary>
        /// <param name="location">Location</param>
        public double[] GetGeoCoordinates(string location)
        {
            double[] result = { 0, 0 };

            try
            {
                bool repeatRequest;
                int interval = 150;
                int trials = 0;

                // Create WebClient object
                WebClient client = new WebClient();

                do
                {
                    client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    // Create url
                    string url = URLHelper.AddParameterToUrl("http://maps.googleapis.com/maps/api/geocode/json", "address", location);

                    // JSON response from Google service
                    string jsonResponse = client.DownloadString(url);
                    client.Dispose();

                    // Get response status according to regular expression
                    Match matchStatus = RegJsonStatus.Match(jsonResponse);

                    if (matchStatus.Success)
                    {
                        string statusCode = matchStatus.Groups[1].Value;

                        if (statusCode.EqualsCSafe(OK, true))
                        {
                            // Get coordinates from response according to regular expression
                            Match mtchCoordinates = RegJsonCoordinates.Match(jsonResponse);
                            if (mtchCoordinates.Success)
                            {
                                result[0] = ValidationHelper.GetDouble(mtchCoordinates.Groups[1].Value, 0);
                                result[1] = ValidationHelper.GetDouble(mtchCoordinates.Groups[2].Value, 0);
                            }
                            repeatRequest = false;
                        }
                        else if (statusCode.EqualsCSafe(OVER_QUERY_LIMIT, true))
                        {
                            repeatRequest = true;
                            // Delay next request
                            Thread.Sleep(interval);
                            // Increase interval
                            interval *= 2;
                            trials++;
                        }
                        else
                        {
                            repeatRequest = false;
                        }
                    }
                    else
                    {
                        repeatRequest = false;
                    }
                } while (repeatRequest && trials < 5);

                // Switch to client processing
                if (trials >= 4)
                {
                    MapProperties.EnableServerProcessing = false;
                }
            }
            catch
            {
            }

            return result;
        }

        #endregion
    }
}
