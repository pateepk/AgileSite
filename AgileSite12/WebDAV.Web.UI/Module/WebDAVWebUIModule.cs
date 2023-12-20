using CMS;
using CMS.DataEngine;
using CMS.UIControls;
using CMS.WebDAV.Web.UI;

[assembly: RegisterModule(typeof(WebDAVWebUIModule))]

namespace CMS.WebDAV.Web.UI
{
    /// <summary>
    /// Represents the WebDAV.Web.UI module.
    /// </summary>
    internal class WebDAVWebUIModule : Module
    {
        /// <summary>
        /// Javascript file providing WebDAV handling
        /// </summary>
        public const string WEBDAV_JS = "WebDAV.js";


        /// <summary>
        /// Default constructor
        /// </summary>
        public WebDAVWebUIModule() 
            : base(new WebDAVWebUIModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            RegisterExternalEditControls();
        }


        private static void RegisterExternalEditControls()
        {
            ExternalEditHelper.RegisterControl(FileTypeEnum.Attachment, typeof(AttachmentEditControl), WebDAVHelper.IsWebDAVExtensionEnabled);
            ExternalEditHelper.RegisterControl(FileTypeEnum.MediaFile, typeof(MediaFileEditControl), WebDAVHelper.IsWebDAVExtensionEnabled);
            ExternalEditHelper.RegisterControl(FileTypeEnum.MetaFile, typeof(MetaFileEditControl), WebDAVHelper.IsWebDAVExtensionEnabled);

            ExternalEditHelper.RegisterScript(WEBDAV_JS);
        }
    }
}