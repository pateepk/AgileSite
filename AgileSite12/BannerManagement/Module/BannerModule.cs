using CMS;
using CMS.BannerManagement;
using CMS.Core;
using CMS.DataEngine;

[assembly: RegisterModule(typeof(BannerModule))]

namespace CMS.BannerManagement
{
    /// <summary>
    /// Represents the Banner module.
    /// </summary>
    public class BannerModule : Module
    {
        internal const string BANNERMANAGEMENT = "##BANNERMANAGEMENT##";

        /// <summary>
        /// Default constructor
        /// </summary>
        public BannerModule()
            : base(new BannerModuleMetadata())
        {
        }
    }
}