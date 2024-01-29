using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.WebServices;

[assembly: RegisterModule(typeof(WebServicesModule))]

namespace CMS.WebServices
{
    /// <summary>
    /// Represents the WebDAV module.
    /// </summary>
    public class WebServicesModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WebServicesModule()
            : base("CMS.WebServices")
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            WebServicesHandlers.Init();
        }
    }
}