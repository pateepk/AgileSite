using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.SharePoint;

[assembly: RegisterModule(typeof(SharePointModule))]
namespace CMS.SharePoint
{
    /// <summary>
    /// Represents the SharePoint module.
    /// </summary>
    public class SharePointModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SharePointModule()
            : base(new SharePointModuleMetadata())
        {

        }
    }
}
