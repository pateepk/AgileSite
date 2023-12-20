using CMS;
using CMS.DataEngine;
using CMS.WebDAV;

using ITHit.WebDAV.Logger;

[assembly: RegisterModule(typeof(WebDAVModule))]

namespace CMS.WebDAV
{
    /// <summary>
    /// Represents the WebDAV module.
    /// </summary>
    internal class WebDAVModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WebDAVModule()
            : base(new WebDAVModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            WebDAVHandlers.Init();

            // Set File Logger settings
            FileLogger.LogFile = WebDAVHandler.LogFilePath;
            FileLogger.Level = WebDAVHandler.LogLevel;
        }
    }
}