using System;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
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
    /// Basic Bing maps control that automates data-binding.
    /// </summary>
    [DefaultProperty("Text"), ToolboxData("<{0}:BasicBingMaps runat=server></{0}:BasicBingMaps>")]
    public class BasicBingMaps : CMSAbstractMap
    {
        /// <summary>
        /// Protocol-agnostic URL of Bing maps Locations API.
        /// </summary>
        private const string BING_LOCATIONS_API_URL = "//dev.virtualearth.net/REST/v1/Locations";

        #region "Private variables"

        private static BasicBingMaps mProviderObject = null;
        private static Regex mRegJsonStatus = null;
        private static Regex mRegJsonCoordinates = null;

        #endregion


        #region "Private properties"

        /// <summary>
        /// Regular expression for parsing JSON response to get coordinates.
        /// </summary>
        private static Regex RegJsonCoordinates
        {
            get
            {
                if (mRegJsonCoordinates == null)
                {
                    // The RegEx accepts strings like: coordinates" 12.234134 -20.65213
                    mRegJsonCoordinates = RegexHelper.GetRegex("coordinates\"[\\w\\W]+?(-?\\d+\\.\\d+)[\\w\\W]+?(-?\\d+\\.\\d+)");
                }
                return mRegJsonCoordinates;
            }
        }


        /// <summary>
        /// Regular expression for parsing JSON response to get response status.
        /// </summary>
        private static Regex RegJsonStatus
        {
            get
            {
                if (mRegJsonStatus == null)
                {
                    mRegJsonStatus = RegexHelper.GetRegex("statusCode\":(\\d+)");
                }
                return mRegJsonStatus;
            }
        }


        /// <summary>
        /// Provider object.
        /// </summary>
        protected static BasicBingMaps ProviderObject
        {
            get
            {
                if (mProviderObject == null)
                {
                    mProviderObject = new BasicBingMaps();
                }
                return mProviderObject;
            }
            set
            {
                mProviderObject = value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Registers necessary Bing Javascript files.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="mapId">Map Id</param>
        /// <param name="mainScriptPath">Map script path</param>
        public static void RegisterMapScripts(Page page, string mapId, string mainScriptPath)
        {
            string bingScriptUrl = "//www.bing.com/api/maps/mapcontrol?callback=loadMaps";
            string script = ScriptHelper.GetScriptTag(bingScriptUrl, false, ScriptExecutionModeEnum.Deferred);

            // Register Bing API
            ScriptHelper.RegisterClientScriptBlock(page, typeof(string), "bingMapsScriptFile", script);

            // Register BingMaps helper javascript file
            RegisterMapFileScript(page, mainScriptPath);
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
                string customCacheItemName = CacheHelper.GetCacheItemName(cacheItemName, mp.Location, "StaticBingMap", viewMode);
                using (var cs = new CachedSection<double[]>(ref geoResponse, cacheMinutes, true, null, customCacheItemName, null))
                {
                    if (cs.LoadData)
                    {
                        // Cache response
                        geoResponse = GetGeoCoordinates(mp.Location, mp.MapKey);

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

        #endregion


        #region "Private methods"

        /// <summary>
        /// OnItemDataBound override.
        /// </summary>
        protected override void OnItemDataBound(RepeaterItemEventArgs e)
        {
            base.OnItemDataBound(e);

            // Add each content item to javascript array
            e.Item.Controls.AddAt(0, new LiteralControl($"content_{MapProperties.MapId}.push({{'cont':'{TR}"));
            e.Item.Controls.Add(new LiteralControl($"{TRE}'}});\n"));
        }


        /// <summary>
        /// OnPreRender override.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!DataHelper.DataSourceIsEmpty(DataSource) || !HideControlForZeroRows || !StopProcessing)
            {
                // Register Bing javascript files
                RegisterMapScripts(Page, MapProperties.MapId, MainScriptPath);
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
                string viewMode = GetViewMode();

                if (!string.IsNullOrEmpty(CacheItemName))
                {
                    CacheItemName += "|" + viewMode;
                }

                // Custom cache key name for LiveSite and the other view modes 
                string customCacheItemName = CacheHelper.GetCacheItemName(CacheItemName, MapProperties.MapName, "BingMap", viewMode);
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

            // Ensure Page presence for client geocoding
            if (MapProperties.Page == null)
            {
                MapProperties.Page = Page;
            }

            // Generate map with corrected marker content array
            outputHtml += GenerateMapInternal(MapProperties, RepairInputString(markerArray.ToString()), contentArray);
            // Render Html
            writer.Write(outputHtml);
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
            string mapKey = MapProperties.MapKey;

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
                        // Get coordinates from Bing service
                        double[] geoResult = GetGeoCoordinates(location, mapKey);
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
            string mapType = mp.MapType;
            string mapKey = mp.MapKey;

            // Ensure correct map type
            if (string.IsNullOrEmpty(mapType))
            {
                mapType = "road";
            }

            // Marker content - global variable
            result.AppendLine($@"
var content_{mapId} = [];
var {mapName} = null;");

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
                result.AppendLine($"content_{mapId}.push({{'cont': '{RepairMarkerContent(mp.Content)}'}});");

                lat = mp.Latitude;
                lng = mp.Longitude;
            }

            lat = EnsureLatitude(lat);
            lng = EnsureLongitude(lng);

            // Register map geo code handler for map center
            if (!mp.EnableServerProcessing && !string.IsNullOrEmpty(mp.Location.Trim()) && lat == null && lng == null && !detailMode)
            {
                result.AppendLine($@"
function callback_{mp.MapName}(res) 
{{
    var centerLocation = new Microsoft.Maps.Location(0, 0);
    if (res && res.resourceSets && res.resourceSets.length > 0 && res.resourceSets[0].resources && res.resourceSets[0].resources.length > 0) {{
        centerLocation = new Microsoft.Maps.Location(res.resourceSets[0].resources[0].point.coordinates[0], res.resourceSets[0].resources[0].point.coordinates[1]);
    }}
        
    {mp.MapName}.setView({{ zoom: {scale}, center: centerLocation }});
}}");
            }

            result.AppendLine($@"
function infoBox_{mapId}(e) {{
    showInfo({mapName}, e.target, {mp.ZoomScale});
}}");

            result.AppendLine($@"
function MapLoad_{mapId}() {{
if ({mapName} != null) {{
    {mapName} = null;
}}
{mapName} = new Microsoft.Maps.Map('#{mapName}',
{{
    credentials: '{HTMLHelper.HTMLEncode(mapKey)}',
    enableSearchLogo: enableSearchLogo,
    mapTypeId: Microsoft.Maps.MapTypeId.{mapType},
    showDashboard: {mp.ShowNavigationControl.ToString().ToLowerInvariant()},
    disablePanning: {(!mp.EnableMapDragging).ToString().ToLowerInvariant()},
    showScalebar: {mp.ShowScaleControl.ToString().ToLowerInvariant()}"
);

            bool clientProcMapFocus = false;
            // Center of the map
            if (!detailMode)
            {
                // Map center according to default lat and lng
                if (lat != null || lng != null || (string.IsNullOrEmpty(location.Trim())))
                {
                    // If latitude or longitude is not specified, set to zero
                    lat = lat ?? 0;
                    lng = lng ?? 0;

                    result.AppendLine($@",
    zoom: {scale},
    center: new Microsoft.Maps.Location({lat.ToString().Replace(",", ".")}, {lng.ToString().Replace(",", ".")})"
);
                }
                else if (mp.EnableServerProcessing)
                {
                    // Get coordinates from Bing service for map center
                    double[] latLon = GetGeoCoordinates(location.Trim(), mp.MapKey.Trim());
                    // Server processing according to location
                    result.AppendLine($@",
    zoom: {scale}, 
    center: new Microsoft.Maps.Location({latLon[0]}, {latLon[1]})"
);
                }
                else
                {
                    clientProcMapFocus = true;
                }
            }

            result.AppendLine("});");

            if (clientProcMapFocus)
            {
                // Client processing according to location for default map focus
                result.AppendLine($@"
function bingRequest_{mp.MapId}(credentials) {{
    var url = '{BING_LOCATIONS_API_URL}/{HttpUtility.UrlEncode(location.Trim())}?output=json&jsonp=callback_{mp.MapName}&key=' + credentials;
    callBingService(url);
}}                    
{mp.MapName}.getCredentials(bingRequest_{mp.MapId});"
);
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
                result.Append(GenerateMapMarker(lat, lng, location, mp.ToolTip, 0, mp, true, mp.IconURL));
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

            // register map initialization
            result.AppendLine($@"
registerMapInitializer(MapLoad_{mapId});
");

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
        /// Generates Bing map marker.
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
            string markerRef = $"content_{mp.MapId}[{markerIndex}]['cont']";

            // Custom marker icon
            if (!string.IsNullOrEmpty(iconURL))
            {
                iconURL = ResolveUrl(iconURL);
            }

            if ((lat == null) && (lng == null) && !mp.EnableServerProcessing && !string.IsNullOrEmpty(location.Trim()))
            {
                // Get coordinates on client side and add marker to map
                result.AppendLine($@"
{mp.MapName}.getCredentials(bingRequest_{mp.MapId}_{markerIndex});
function bingRequest_{mp.MapId}_{markerIndex}(credentials)
{{
    var url = '{BING_LOCATIONS_API_URL}/{HttpUtility.UrlEncode(location.Trim())}?output=json&jsonp=callback_{mp.MapName}_{markerIndex}&key=' + credentials;
    callBingService(url);
}}"
);

                StringBuilder geoMarkerScript = new StringBuilder();
                geoMarkerScript.AppendLine($@"
function callback_{mp.MapName}_{markerIndex}(res) 
{{
    if (res && res.resourceSets && res.resourceSets.length > 0 && res.resourceSets[0].resources && res.resourceSets[0].resources.length > 0) {{
        var loc = new Microsoft.Maps.Location(res.resourceSets[0].resources[0].point.coordinates[0], res.resourceSets[0].resources[0].point.coordinates[1]);
        {(setMapCenter ? $"{mp.MapName}.setView({{ zoom: {mp.Scale}, center: new Microsoft.Maps.Location(loc.latitude, loc.longitude) }});\n" : "")}
        Microsoft.Maps.Events.addHandler(addBingMarker({mp.MapName}, loc.latitude, loc.longitude, {ScriptHelper.GetString(toolTip.Trim())}, {markerRef.Trim()}, {ScriptHelper.GetString(iconURL)}), 'click', infoBox_{mp.MapId});
    }}
}}"
);

                ScriptHelper.RegisterStartupScript(mp.Page, typeof(string), $"bingGeoMarkerScript_{mp.MapName}_{markerIndex}", ScriptHelper.GetScript(geoMarkerScript.ToString()));
            }
            else if ((lat != null) || (lng != null))
            {
                // If latitude or longitude is not specified, set to zero
                lat = lat ?? 0;
                lng = lng ?? 0;

                string latString = lat.ToString().Replace(",", ".");
                string lngString = lng.ToString().Replace(",", ".");

                // Add marker to map without server/client geo processing
                if (setMapCenter)
                {
                    result.AppendLine($"{mp.MapName}.setView({{ zoom: {mp.Scale}, center: new Microsoft.Maps.Location({latString}, {lngString}) }});");
                }

                result.AppendLine($@"Microsoft.Maps.Events.addHandler(addBingMarker({mp.MapName}, {latString}, {lngString}, {ScriptHelper.GetString(toolTip.Trim())}, {markerRef.Trim()}, {ScriptHelper.GetString(iconURL)}), 'click', infoBox_{mp.MapId});");

            }
            return result.ToString();
        }


        /// <summary>
        /// Returns coordinates according to location address from Bing service.
        /// </summary>
        /// <param name="location">Location</param>
        /// <param name="mapKey">Bing map key</param>
        private static double[] GetGeoCoordinates(string location, string mapKey)
        {
            const int MAX_TRIALS = 4;
            const int SLEEP_INTERVAL = 150;
            double[] result = { 0, 0 };

            try
            {
                bool repeatRequest;
                int interval = SLEEP_INTERVAL;
                int trials = 0;

                do
                {
                    string jsonResponse = String.Empty;

                    using (var client = new WebClient())
                    {
                        client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                        // Create URL
                        string url = URLHelper.AddParameterToUrl($"https:{BING_LOCATIONS_API_URL}", "query", HttpUtility.UrlEncode(location));
                        url = URLHelper.AddParameterToUrl(url, "key", mapKey);

                        // JSON response from Bing service
                        jsonResponse = client.DownloadString(url);
                    }

                    // Get response status according to regular expression
                    Match matchStatus = RegJsonStatus.Match(jsonResponse);

                    if (matchStatus.Success)
                    {
                        int statusCode = ValidationHelper.GetInteger(matchStatus.Groups[1].Value, 0);

                        if (statusCode == (int)HttpStatusCode.OK)
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
                        else if ((statusCode == (int)HttpStatusCode.ServiceUnavailable) || (statusCode == (int)HttpStatusCode.InternalServerError))
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
                } while (repeatRequest && trials < MAX_TRIALS);
            }
            catch
            {
            }

            return result;
        }

        #endregion
    }
}
