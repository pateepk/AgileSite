using CMS.DataEngine;
using CMS.MacroEngine;


namespace CMS.Newsletters
{
    /// <summary>
    /// Provides macro resolvers for newsletters.
    /// </summary>
    internal static class NewsletterResolvers
    {
        /// <summary>
        /// Registers newsletter resolvers to the resolvers storage to be accessible via names.
        /// </summary>
        public static void Register()
        {
            MacroResolverStorage.RegisterResolver("NewsletterResolver", GetVirtualResolver);
        }


        /// <summary>
        /// Gets resolver with fake objects used for macro editor intellisense.
        /// </summary>
        public static MacroResolver GetVirtualResolver()
        {
            var macroResolverSettings = new EmailContentMacroResolverSettings
            {
                Newsletter = (NewsletterInfo)ModuleManager.GetReadOnlyObject(NewsletterInfo.OBJECT_TYPE),
                Issue = (IssueInfo)ModuleManager.GetReadOnlyObject(IssueInfo.OBJECT_TYPE),
                UseFakeData = true
            };

            var resolver = new EmailContentMacroResolver(macroResolverSettings);
            return resolver.GetResolver();
        }
    }
}
