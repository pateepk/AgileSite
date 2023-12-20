using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineMarketing;

[assembly: RegisterModule(typeof(ContentPersonalizationModule))]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Represents the Content personalization module.
    /// </summary>
    internal class ContentPersonalizationModule : Module
    {
        /// <summary>
        /// Initializes a new instance of the ContentPersonalizationModule class.
        /// </summary>
        public ContentPersonalizationModule() : base(new ContentPersonalizationModuleMetadata())
        {

        }
    }
}