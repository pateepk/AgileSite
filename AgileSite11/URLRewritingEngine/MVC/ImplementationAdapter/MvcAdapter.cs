using System;
using System.Web;
using System.Web.Routing;

using CMS.Core;

namespace CMS.Mvc
{
    /// <summary>
    /// Class providing a connection for the implementation of the MVC contract
    /// </summary>
    public class MvcAdapter : StaticWrapper<IMvcAdapter>
    {
        #region "Private properties and variables"

        private static IMvcAdapter mMvcAdapterImplementation = null;

        /// <summary>
        /// Gets the MVC implementation object.
        /// </summary>
        private static IMvcAdapter MvcAdapterImplementation
        {
            get
            {
                if (mMvcAdapterImplementation == null)
                {
                    try
                    {
                        mMvcAdapterImplementation = Implementation;
                    }
                    catch
                    {
                        // Do not throw exception when implementation is not found
                    }
                }

                return mMvcAdapterImplementation;
            }
        }

        #endregion


        #region "Interface methods and properties"

        /// <summary>
        /// Gets the MVC handler for the given request context.
        /// </summary>
        /// <param name="context">The request context</param>
        public static IHttpHandler GetMvcHandler(RequestContext context)
        {
            if (MvcAdapterImplementation == null)
            {
                return null;
            }

            return MvcAdapterImplementation.GetMvcHandler(context);
        }


        /// <summary>
        /// Gets the MVC route handler object.
        /// </summary>
        public static IRouteHandler GetMvcRouteHandler()
        {
            if (MvcAdapterImplementation == null)
            {
                return null;
            }

            return MvcAdapterImplementation.GetMvcRouteHandler();
        }


        /// <summary>
        /// Proceeds the default route values.
        /// </summary>
        /// <param name="values">The values</param>
        /// <param name="routeDefaults">The route defaults</param>
        public static void ProceedDefaultRouteValues(RouteValueDictionary values, RouteValueDictionary routeDefaults)
        {
            if (MvcAdapterImplementation != null)
            {
                MvcAdapterImplementation.ProceedDefaultRouteValues(values, routeDefaults);
            }
        }


        /// <summary>
        /// Gets the "Optional URL parameter" type.
        /// </summary>
        public static object OptionalUrlParameter
        {
            get
            {
                if (MvcAdapterImplementation == null)
                {
                    return null;
                }

                return MvcAdapterImplementation.OptionalUrlParameter;
            }
        }


        /// <summary>
        /// Gets the type of the controller.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="controllerName">Name of the controller</param>
        public static Type GetControllerType(RequestContext context, string controllerName)
        {
            if (MvcAdapterImplementation == null)
            {
                return null;
            }

            return MvcAdapterImplementation.GetControllerType(context, controllerName);
        }

        #endregion
    }
}
