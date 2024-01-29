using System;
using System.Web.Routing;

using CMS.EventLog;
using CMS.Mvc;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Controller factory with additional functionality for MVC controllers to provide site separation
    /// </summary>
    public class CMSControllerFactory 
    {
        /// <summary>
        /// Gets the type of a single controller, returns null if no valid controller found or ambiguous
        /// </summary>
        /// <param name="context">Execution context</param>
        /// <param name="controllerName">Controller name</param>
        public static Type GetSingleControllerType(RequestContext context, string controllerName)
        {
            try
            {
                return MvcAdapter.GetControllerType(context, controllerName);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("CMSControllerFactory", "GETCONTROLLER", ex);
                return null;
            }
        }


        /// <summary>
        /// Gets the type of a controller, returns null if no valid controller found. Throws an exception if multiple controllers were found
        /// </summary>
        /// <param name="context">Execution context</param>
        /// <param name="controllerName">Controller name</param>
        public static Type GetAnyControllerType(RequestContext context, string controllerName)
        {
            return MvcAdapter.GetControllerType(context, controllerName);
        }
    }
}