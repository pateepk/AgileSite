using CMS;
using CMS.DataEngine;
using CMS.SocialMarketing;

[assembly: RegisterModule(typeof(SocialMarketingModule))]

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents the Social marketing module.
    /// </summary>
    public class SocialMarketingModule : Module
    {
        internal const string SOCIALMARKETING = "##SOCIALMARKETING##";


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the SocialMarketingModule class.
        /// </summary>
        public SocialMarketingModule() : base(new SocialMarketingModuleMetadata())
        {

        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Initializes social marketing module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            SocialMarketingHandlers.Init();
        }

        #endregion
    }
}