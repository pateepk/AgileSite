using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class TranslationServicesResolvers : ResolverDefinition
    {
        #region "Variables"

        private static MacroResolver mTranslationsResolver = null;

        #endregion


        /// <summary>
        /// Returns translation services e-mail template macro resolver.
        /// </summary>
        public static MacroResolver TranslationServicesResolver
        {
            get
            {
                if (mTranslationsResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    resolver.SetNamedSourceData("Submission", ModuleManager.GetReadOnlyObject(TranslationSubmissionInfo.OBJECT_TYPE));

                    mTranslationsResolver = resolver;
                }

                return mTranslationsResolver;
            }
        }
    }
}