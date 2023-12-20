using System;

using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Visitor methods - wrapping methods for macro resolver.
    /// </summary>
    public class VisitorMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns current distance (in kilometers) from specified location (based on Geo IP).
        /// </summary>
        /// <param name="parameters">
        /// Latitude of the place;
        /// Longitude of the place;
        /// </param>
        [MacroMethod(typeof(double), "Returns current distance (in kilometers) from specified location (based on Geo IP).", 2)]
        [MacroMethodParam(0, "latitude", typeof(double), "Latitude of the place.")]
        [MacroMethodParam(1, "longitude", typeof(double), "Longitude of the place.")]
        public static object GetCurrentDistance(params object[] parameters)
        {
            if (parameters.Length == 2)
            {
                double lat = ValidationHelper.GetDouble(parameters[0], 0, CultureHelper.EnglishCulture.Name);
                double lon = ValidationHelper.GetDouble(parameters[1], 0, CultureHelper.EnglishCulture.Name);

                return GeoIPHelper.GetCurrentGeoLocation().Distance(lat, lon);
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Returns true if current visitor is returning.
        /// </summary>
        /// <param name="parameters">Parameters</param>
        [MacroMethod(typeof(bool), "Returns true if current visitor is returning.", 0)]
        public static object IsReturningVisitor(params object[] parameters)
        {
            return WebAnalyticsFunctions.IsReturningVisitor();
        }


        /// <summary>
        /// Returns true if current visitor comes to the website for the first time.
        /// </summary>
        /// <param name="parameters">Parameters</param>
        [MacroMethod(typeof(bool), "Returns true if current visitor comes to the website for the first time.", 0)]
        public static object IsFirstTimeVisitor(params object[] parameters)
        {
            return WebAnalyticsFunctions.IsFirstTimeVisitor();
        }


        /// <summary>
        /// Returns search keywords from search engine visitor came from.
        /// </summary>
        /// <param name="parameters">Parameters</param>
        [MacroMethod(typeof(string), "Returns search keywords from search engine visitor came from.", 0)]
        public static object GetSearchEngineKeyword(params object[] parameters)
        {
            return WebAnalyticsFunctions.GetSearchEngineKeyword();
        }


        /// <summary>
        /// Returns search engine visitor came from.
        /// </summary>
        /// <param name="parameters">Parameters</param>
        [MacroMethod(typeof(string), "Returns search engine visitor came from.", 0)]
        public static object GetSearchEngine(params object[] parameters)
        {
            return WebAnalyticsFunctions.GetSearchEngine();
        }


        /// <summary>
        /// Returns absolute URI of the URLRefferer from current HTTP context.
        /// </summary>
        /// <param name="parameters">Parameters</param>
        [MacroMethod(typeof(string), "Returns absolute URI of the URLRefferer from current HTTP context.", 0)]
        public static object GetUrlReferrer(params object[] parameters)
        {
            return WebAnalyticsFunctions.GetUrlReferrer();
        }


        /// <summary>
        /// Returns value of specified URLReferrer query string parameter.
        /// </summary>
        /// <param name="parameters">Query string parameter name</param>
        [MacroMethod(typeof(string), "Returns value of specified URLReferrer query string parameter.", 1)]
        [MacroMethodParam(0, "parameterName", typeof(string), "Query string parameter name.")]
        public static object GetUrlReferrerParameter(params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return WebAnalyticsFunctions.GetUrlReferrerParameter(ValidationHelper.GetString(parameters[0], ""));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}