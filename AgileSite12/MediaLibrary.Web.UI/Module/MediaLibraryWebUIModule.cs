using CMS;
using CMS.DataEngine;
using CMS.MediaLibrary.Web.UI;

[assembly: RegisterModule(typeof(MediaLibraryWebUIModule))]

namespace CMS.MediaLibrary.Web.UI
{
    /// <summary>
    /// Represents the MediaLibrary.Web.UI module.
    /// </summary>
    internal class MediaLibraryWebUIModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MediaLibraryWebUIModule()
            : base(new MediaLibraryWebUIModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
 
            // Dialogs handlers
            MediaLibraryHandlers.Init();
        }
    }
}
