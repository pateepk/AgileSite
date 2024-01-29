using System.Web;


using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.URLRewritingEngine;
using CMS.Base.Web.UI;

[assembly: RegisterModule(typeof(URLRewritingModule))]

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Represents the URL Rewriting module.
    /// </summary>
    public class URLRewritingModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public URLRewritingModule()
            : base(new URLRewritingModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            URLRewritingMacros.Init();
            URLRewritingHandlers.Init();

            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                RouteTableSynchronization.Init();
                RegisterContext<URLRewritingContext>();
            }
        }
    }
}