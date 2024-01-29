using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Routing;

using CMS;
using CMS.DataEngine;
using CMS.WebApi;
using CMS.Base;

[assembly: RegisterModule(typeof(WebApiModule))]

namespace CMS.WebApi
{
    /// <summary>
    /// Represents the Web API module.
    /// </summary>
    internal class WebApiModule : Module
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiModule"/> class.
        /// </summary>
        public WebApiModule() : base(new WebApiModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                ApplicationEvents.Initialized.Execute += MapCmsApiRoutes;
            }
        }


        /// <summary>
        /// Maps routes for Kentico CMS API controllers.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void MapCmsApiRoutes(object sender, EventArgs e)
        {
            HttpControllerRouteTable.Instance.MapRoutes();
            GlobalConfiguration.Configuration.Filters.Add(new ThreadUICultureActionFilter());
        }
    }


    /// <summary>
    /// Stores configuration for CMS API controller
    /// </summary>
    internal class CMSApiControllerConfiguration
    {
        /// <summary>
        /// Gets or sets controller type.
        /// </summary>
        public Type ControllerType { get; set; }


        /// <summary>
        /// Gets or sets flag indicating whether session state is required.
        /// </summary>
        public bool RequiresSessionState { get; set; }
    }


    /// <summary>
    /// Stores the URL routes served by Kentico CMS API controllers.
    /// </summary>
    internal sealed class HttpControllerRouteTable
    {
        private readonly ConcurrentDictionary<string, CMSApiControllerConfiguration> mControllerConfigurations =
            new ConcurrentDictionary<string, CMSApiControllerConfiguration>(4, 4 * 16, StringComparer.OrdinalIgnoreCase);

        private static readonly Lazy<HttpControllerRouteTable> mInstance = new Lazy<HttpControllerRouteTable>(() => new HttpControllerRouteTable(), LazyThreadSafetyMode.PublicationOnly);


        /// <summary>
        /// Gets default instance of <see cref="HttpControllerRouteTable"/>.
        /// </summary>
        public static HttpControllerRouteTable Instance
        {
            get
            {
                return mInstance.Value;
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        private HttpControllerRouteTable()
        {
        }


        /// <summary>
        /// Registers a route served by Kentico CMS API controller.
        /// </summary>
        /// <param name="controllerType">Kentico CMS API controller type.</param>
        /// <param name="requiresSessionState">Indicates whether session state is required for given controller.</param>
        /// <exception cref="ArgumentNullException"><paramref name="controllerType"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">A controller of given name is already registered.</exception>
        public void Register(Type controllerType, bool requiresSessionState)
        {
            if (controllerType == null)
            {
                throw new ArgumentNullException(nameof(controllerType));
            }

            var controllerConfiguration = new CMSApiControllerConfiguration { ControllerType = controllerType, RequiresSessionState = requiresSessionState };

            if (!mControllerConfigurations.TryAdd(controllerType.Name, controllerConfiguration))
            {
                var alreadyRegisteredControllerType = mControllerConfigurations[controllerType.Name].ControllerType;

                throw new ArgumentException("A controller of given name is already registered. Controller names must be unique. " +
                    $"Already registered controller type FullName: '{alreadyRegisteredControllerType.FullName}', " +
                    $"Registering controller type FullName: '{controllerType.FullName}'.", nameof(controllerType));
            }
        }


        /// <summary>
        /// Maps HTTP routes served by all registered Kentico CMS API controllers.
        /// </summary>
        public void MapRoutes()
        {
            var routes = RouteTable.Routes;
            using (routes.GetWriteLock())
            {
                foreach (var controllerConfiguration in mControllerConfigurations.Values)
                {
                    HttpControllerRouteMapper.MapRoute(routes, controllerConfiguration);
                }
            }
        }
    }


    /// <summary>
    /// Maps routes served by Kentico CMS API controllers.
    /// </summary>
    internal static class HttpControllerRouteMapper
    {
        /// <summary>
        /// Maps a route served by CMS API controller into <paramref name="routes"/>.
        /// </summary>
        /// <param name="routes">Route collection to map route in.</param>
        /// <param name="controllerConfiguration">Controller configuration.</param>
        public static void MapRoute(RouteCollection routes, CMSApiControllerConfiguration controllerConfiguration)
        {
            string controllerName = GetControllerName(controllerConfiguration.ControllerType.Name);

            if (String.IsNullOrEmpty(controllerName) || IsRouteMapped(routes, controllerName))
            {
                return;
            }

            var defaults = new { controller = controllerName, action = RouteParameter.Optional };
            var messageHandler = controllerConfiguration.RequiresSessionState ? GetSessionHttpMessageHandler() : null;

            var route = routes.MapHttpRoute(controllerName, $"cmsapi/{controllerName}/{{action}}/", defaults, null, messageHandler);
            route.RouteHandler = controllerConfiguration.RequiresSessionState ? new RequiredSessionStateRouteHandler() : route.RouteHandler;
        }


        /// <summary>
        /// Gets name of Controller without <c>"Controller"</c> suffix.
        /// </summary>
        /// <param name="controllerTypeName">Controller type name.</param>
        /// <returns>Controller type name without <c>"Controller"</c> suffix or empty string, if <paramref name="controllerTypeName"/> does not ends with <c>"Controller"</c>.</returns>
        private static string GetControllerName(string controllerTypeName)
        {
            const string controllerSuffix = "Controller";

            if (!controllerTypeName.EndsWith(controllerSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return String.Empty;
            }

            var controllerNameIdex = controllerTypeName.LastIndexOf(controllerSuffix, StringComparison.OrdinalIgnoreCase);

            return controllerTypeName.Remove(controllerNameIdex);
        }


        /// <summary>
        /// Returns <c>true</c> if a route served by <paramref name="controllerName"/> is mapped in <paramref name="routes"/>.
        /// </summary>
        /// <param name="routes">Routes.</param>
        /// <param name="controllerName">Controller name.</param>
        private static bool IsRouteMapped(RouteCollection routes, string controllerName)
        {
            return routes[controllerName] != null;
        }


        /// <summary>
        /// Gets instance of <see cref="HttpMessageHandler"/> that ensures the session is available for the Web API requests.
        /// </summary>
        /// <returns><see cref="HttpMessageHandler"/> ensuring the session is available.</returns>
        private static HttpMessageHandler GetSessionHttpMessageHandler()
        {
            return HttpClientFactory.CreatePipeline(
                new HttpControllerDispatcher(GlobalConfiguration.Configuration),
                new DelegatingHandler[]
                {
                    new EnsureSessionMessageHandler()
                });
        }
    }


    /// <summary>
    /// Registers route given CMS API controller.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterCMSApiControllerAttribute : Attribute, IPreInitAttribute
    {
        /// <summary>
        /// Type of the registered API controller.
        /// </summary>
        public Type MarkedType { get; }


        /// <summary>
        /// Gets or sets value that defines if the target API controller requires read and write access to session-state values.
        /// Default value is <c>true</c>.
        /// </summary>
        public bool RequiresSessionState { get; set; } = true;


        /// <summary>
        /// Creates new instance of <see cref="RegisterCMSApiControllerAttribute"/>
        /// </summary>
        /// <param name="markedType"></param>
        public RegisterCMSApiControllerAttribute(Type markedType)
        {
            MarkedType = markedType;
        }


        /// <summary>
        /// Registers instance of this attribute to <see cref="HttpControllerRouteTable"/>.
        /// </summary>
        public void PreInit()
        {
            HttpControllerRouteTable.Instance.Register(MarkedType, RequiresSessionState);
        }
    }
}