using System;
using System.Web.Http;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.WebApi;

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
}