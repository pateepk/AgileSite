using CMS.Core;

namespace CMS.SocialMarketing
{

    /// <summary>
    /// Represents the Social media module metadata.
    /// </summary>
    public class SocialMarketingModuleMetadata : ModuleMetadata
    {

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the SocialMarketingModuleMetadata class.
        /// </summary>
        public SocialMarketingModuleMetadata() : base(ModuleName.SOCIALMARKETING)
        {
            RootPath = "~/CMSModules/SocialMarketing/";
        }

        #endregion

    }

}