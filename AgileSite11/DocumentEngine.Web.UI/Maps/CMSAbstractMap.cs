using System;
using System.Text;
using System.Web;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Base map class.
    /// </summary>
    public abstract class CMSAbstractMap : BasicRepeater
    {
        #region "Constants"

        /// <summary>
        /// Marker that indicates the beginning of map InfoBox content.
        /// </summary>
        protected const string TR = "##TR##";


        /// <summary>
        /// Marker that indicates the end of map InfoBox content.
        /// </summary>
        protected const string TRE = "##TRE##";

        #endregion

        #region "Private variables"

        private string mCacheItemName = "";
        private string mCacheDependencies = "";
        private string mMainScriptPath = "";


        /// <summary>
        /// Map output Html code.
        /// </summary>
        protected string outputHtml = "";

        /// <summary>
        /// Map properties.
        /// </summary>
        protected CMSMapProperties mMapProperties;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets map properties.
        /// </summary>
        public CMSMapProperties MapProperties
        {
            get
            {
                if (mMapProperties == null)
                {
                    mMapProperties = new CMSMapProperties();
                }
                return mMapProperties;
            }
            set
            {
                mMapProperties = value;
            }
        }


        /// <summary>
        /// Cache minutes.
        /// </summary>
        public int CacheMinutes
        {
            get;
            set;
        }


        /// <summary>
        /// Cache item name.
        /// </summary>
        public string CacheItemName
        {
            get
            {
                return mCacheItemName;
            }
            set
            {
                mCacheItemName = value;
            }
        }


        /// <summary>
        /// Cache dependencies, each cache dependency on a new line.
        /// </summary>
        public string CacheDependencies
        {
            get
            {
                return mCacheDependencies;
            }
            set
            {
                mCacheDependencies = value;
            }
        }


        /// <summary>
        /// Stop control processing.
        /// </summary>
        public bool StopProcessing
        {
            get;
            set;
        }


        /// <summary>
        /// Map script path.
        /// </summary>
        public string MainScriptPath
        {
            get
            {
                return mMainScriptPath;
            }
            set
            {
                mMainScriptPath = value;
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Ensures correct range of latitude.
        /// </summary>
        /// <param name="lat">Latitude</param>
        protected static double? EnsureLatitude(double? lat)
        {
            if (!lat.HasValue)
            {
                return null;
            }
            else if ((lat > 90) || (lat < -90))
            {
                lat = 0;
            }
            return lat;
        }


        /// <summary>
        /// Ensures correct range of longitude.
        /// </summary>
        /// <param name="lng">Longitude</param>
        protected static double? EnsureLongitude(double? lng)
        {
            if (!lng.HasValue)
            {
                return null;
            }
            else if ((lng > 180) || (lng < -180))
            {
                lng = 0;
            }
            return lng;
        }


        /// <summary>
        /// Returns custom view mode key used as part of cache key.
        /// </summary>
        protected static string GetViewMode()
        {
            return PortalContext.ViewMode.IsLiveSite() ? "LiveSite" : "Edit";
        }


        /// <summary>
        /// Repairs input string (remove forbidden characters between macros '##TR##' and '##TRE##').
        /// </summary>
        /// <param name="input">Input text</param>
        protected static string RepairInputString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            int TRE_length = TRE.Length;

            StringBuilder output = new StringBuilder();
            bool firstPart = true;

            // Split the input string by ##TR## macro.
            string[] parts = input.Split(new[] { TR }, StringSplitOptions.None);

            // Try to find closing macro ##TRE## and remove forbidden characters between macro strings
            foreach (string part in parts)
            {
                if (firstPart)
                {
                    // Always append first split part
                    output.Append(part);
                    firstPart = false;
                    continue;
                }

                // Find the closing macro string
                var endIndex = part.IndexOfCSafe(TRE);
                var innerText = string.Empty;

                // Get the inner text contained in the ##TR##/##TRE## macros
                if (endIndex >= 0)
                {
                    innerText = part.Substring(0, endIndex);

                    // Remove forbidden characters
                    innerText = innerText.Replace("\"", "\\" + "\"").Replace("\n", " ").Replace("\r", "").Replace(TR, "").Replace(TRE, "").Replace(@"'", @"\'").Trim();
                }

                output.Append(innerText);

                // Append the rest of the string (after the closing macro)
                if (endIndex != -1)
                {
                    output.Append(part.Substring(endIndex + TRE_length));
                }
            }

            return output.ToString();
        }


        /// <summary>
        /// Repairs marker content string.
        /// </summary>
        /// <param name="input">Input text</param>
        protected static string RepairMarkerContent(string input)
        {
            input = HTMLHelper.ResolveUrls(input, HttpRuntime.AppDomainAppVirtualPath);
            return input.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\"", "\\" + "\"").Replace("\n", " ").Replace("\r", "").Trim();
        }


        /// <summary>
        /// Registers OnLoad script.
        /// </summary>
        /// <param name="page">Page</param>
        protected static void RegisterOnLoadScript(Page page)
        {
            StringBuilder onLoad = new StringBuilder();
            onLoad.Append(@"
function addLoadEvent(func) {
    var oldonload = window.onload;
    if (typeof window.onload != 'function') {
        window.onload = func;
    } else {
        window.onload = function () {
            oldonload();
            func();
        }
    }
}
");
            ScriptHelper.RegisterClientScriptBlock(page, typeof(string), "mapsOnLoad", ScriptHelper.GetScript(onLoad.ToString()));
        }


        /// <summary>
        /// Registers OnLoad script.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="url">Script Url</param>
        protected static void RegisterMapFileScript(Page page, string url)
        {
            if (page != null && !string.IsNullOrEmpty(url))
            {
                if (page.Form != null)
                {
                    const string KEY = "bingMapsFileScript";
                    // Normal registration through the form
                    if (!ScriptHelper.IsClientScriptBlockRegistered(KEY))
                    {
                        ScriptHelper.RegisterClientScriptInclude(page, typeof(string), KEY, url);
                        ScriptHelper.AddToRegisteredClientScripts(KEY);
                    }
                }
                else
                {
                    // Add to the header in case the page doesn't have Form
                    if (page.Header != null)
                    {
                        page.Header.Controls.Add(new LiteralControl(ScriptHelper.GetScriptTag(url)));
                    }
                }
            }
        }

        #endregion
    }
}
