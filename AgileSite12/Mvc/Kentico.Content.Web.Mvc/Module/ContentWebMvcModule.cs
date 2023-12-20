using CMS;
using CMS.DataEngine;

using Kentico.Content.Web.Mvc;
using Kentico.Forms.Web.Mvc;

[assembly: RegisterModule(typeof(ContentWebMvcModule))]

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Content.Web.Mvc module.
    /// </summary>
    internal class ContentWebMvcModule : Module
    {
        /// <summary>
        /// Identifier of <see cref="ContentWebMvcModule"/>.
        /// </summary>
        public const string MODULE_NAME = "Kentico.Content.Web.Mvc";


        /// <summary>
        /// Initializes a new instance of the <see cref="ContentWebMvcModule"/> class.
        /// </summary>
        public ContentWebMvcModule()
            : base(MODULE_NAME)
        {
        }


        /// <summary>
        /// Initializes the Content.Web.Mvc module by registering bundles and processing of the virtual context.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            VirtualContextUrlRewriter.Initialize();
            VirtualContextPrincipalAssigner.Initialize();
            VirtualContextLinksClickjackingProtection.Initialize();
            CookiePolicyDetection.Initialize();

            FormBuilderFeature.Initialize();

            ContentGlobalFilters.Register();
        }
    }
}
