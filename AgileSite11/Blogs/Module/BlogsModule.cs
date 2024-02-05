using CMS;
using CMS.Blogs;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Base;

[assembly: RegisterModule(typeof(BlogsModule))]

namespace CMS.Blogs
{
    /// <summary>
    /// Represents the Blogs module.
    /// </summary>
    public class BlogsModule : Module
    {
        /// <summary>
        /// Name of email template type for blog.
        /// </summary>
        public const string BLOG_EMAIL_TEMPLATE_TYPE_NAME = "blog";


        /// <summary>
        /// Name of email template type for blog subscription.
        /// </summary>
        public const string BLOG_SUBSCRIPTION_EMAIL_TEMPLATE_TYPE_NAME = "blogsubscription";


        /// <summary>
        /// Default constructor
        /// </summary>
        public BlogsModule()
            : base(new BlogsModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            InitImportExport();
            InitMacros();

            BlogHandlers.Init();
        }


        /// <summary>
        /// Initializes blog macros
        /// </summary>
        private static void InitMacros()
        {
            ExtendList<MacroResolverStorage, MacroResolver>.With("BlogResolver").WithLazyInitialization(() => BlogResolvers.BlogResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("BlogSubscriptionResolver").WithLazyInitialization(() => BlogResolvers.BlogSubscriptionResolver);
        }


        /// <summary>
        /// Initializes blog import/export actions
        /// </summary>
        private static void InitImportExport()
        {
            ImportSpecialActions.Init();

            BlogExport.Init();
            BlogImport.Init();
        }
    }
}