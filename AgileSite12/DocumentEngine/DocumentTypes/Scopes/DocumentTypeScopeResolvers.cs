using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Membership;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class DocumentTypeScopeResolvers : ResolverDefinition
    {
        #region "Variables"

        private static MacroResolver mDocumentTypeScopeResolver = null;

        #endregion

        
        #region "Properties"

        /// <summary>
        /// Returns document type scope macro resolver with document and current user fields.
        /// </summary>
        public static MacroResolver DocumentTypeScopeResolver
        {
            get
            {
                if (mDocumentTypeScopeResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    resolver.SetNamedSourceData("CurrentUser", ModuleManager.GetReadOnlyObject(UserInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("Document", TreeNode.New(SystemDocumentTypes.Root));

                    mDocumentTypeScopeResolver = resolver;
                }

                return mDocumentTypeScopeResolver;
            }
        }

        #endregion
    }
}