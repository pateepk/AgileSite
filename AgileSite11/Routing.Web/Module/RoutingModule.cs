using System.Web.Routing;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.Routing.Web;

[assembly: RegisterModule(typeof(RoutingModule))]

namespace CMS.Routing.Web
{
    /// <summary>
    /// Represents the Routing module.
    /// </summary>
    internal class RoutingModule : Module
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingModule" /> class.
        /// </summary>
        public RoutingModule() : base(new RoutingModuleMetadata())
        {

        }

        /// <summary>
        /// Initializes this module.
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                ApplicationEvents.PreInitialized.Execute += RegisterRoutes;
            }
        }

        
        private void RegisterRoutes(object sender, System.EventArgs arguments)
        {
            var routes = RouteTable.Routes;
            using (routes.GetWriteLock())
            {
                foreach (var route in HttpHandlerRouteTable.Default.GetRoutes())
                {
                    routes.Add(route);
                }
            }
        }
    }
}
