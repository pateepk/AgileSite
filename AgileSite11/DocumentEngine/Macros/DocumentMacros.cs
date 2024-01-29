using CMS.MacroEngine;
using CMS.Base;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document macros implementation
    /// </summary>
    internal class DocumentMacros
    {
        /// <summary>
        /// Initializes the macro engine with DocumentEngine parts
        /// </summary>
        public static void Init()
        {
            // Register resolvers
            ExtendList<MacroResolverStorage, MacroResolver>.With("WorkflowBaseDocumentResolver").WithLazyInitialization(() => WorkflowResolvers.WorkflowBaseDocumentResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("WorkflowResolver").WithLazyInitialization(() => WorkflowResolvers.WorkflowResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("WorkflowSimpleDocumentResolver").WithLazyInitialization(() => WorkflowResolvers.WorkflowSimpleDocumentResolver);

            ExtendList<MacroResolverStorage, MacroResolver>.With("DocumentTypeScopeResolver").WithLazyInitialization(() => DocumentTypeScopeResolvers.DocumentTypeScopeResolver);

            var r = MacroContext.GlobalResolver;

            r.AddSourceAlias("Document", "DocumentContext.CurrentDocument");

            r.AddAnonymousSourceData(new MacroField("Current", x => CMSDataContext.Current));
            r.AddAnonymousSourceData(new MacroField("CurrentDocument", x => DocumentContext.CurrentDocument));

            r.SetNamedSourceDataCallback("CurrentPath", x => DocumentContext.OriginalAliasPath, false);
            r.SetNamedSourceDataCallback("CurrentTemplate", x => (DocumentContext.CurrentPageInfo != null ? DocumentContext.CurrentPageInfo.UsedPageTemplateInfo : null), false);
            r.SetNamedSourceDataCallback("ClassName", x => (DocumentContext.CurrentPageInfo != null ? DocumentContext.CurrentPageInfo.ClassName : null), false);

            // Backward compatibility with BrowserInfo which was renamed to CurrentBrowser
            r.SetHiddenNamedSourceData("BrowserInfo", x => CMSDataContext.Current.BrowserHelper);

            // Register methods
            Extend<SiteInfo>.WithProperty<CMSSiteDataContext>("DataContext").WithLazyInitialization(s => new CMSSiteDataContext(((SiteInfo)s).SiteName));

            MacroResolver.RegisterObjectValueByNameHandler(GetObjectValue);
            MacroSecurityProcessor.OnCheckObjectPermissions += MacroSecurityProcessor_OnCheckObjectPermissions;
        }


        /// <summary>
        /// Adds permission check for TreeNode collections.
        /// </summary>
        /// <param name="sender">MacroResolver which resolves the macro</param>
        /// <param name="e">Permission check event args</param>
        private static void MacroSecurityProcessor_OnCheckObjectPermissions(object sender, MacroSecurityEventArgs e)
        {
            var collection = e.ObjectToCheck as TreeNodeCollection;
            if (collection == null)
            {
                return;
            }

            // Based on collection settings the context of the current user to check the permissions can be already provided 
            if (collection.User != null)
            {
                return;
            }

            // Set the context of permissions check to the user who signed the macro
            var identityInfo = MacroIdentityInfoProvider.GetMacroIdentityInfo(e.Context.IdentityName);

            collection.User = identityInfo != null ? UserInfoProvider.GetUserInfo(identityInfo.MacroIdentityEffectiveUserID) : UserInfoProvider.GetUserInfo(e.Context.UserName);
        }


        /// <summary>
        /// Gets the object value.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="columnName">Column name</param>
        /// <param name="result">Returning the result in case of match</param>
        private static bool GetObjectValue(object obj, string columnName, ref object result)
        {
            if (MacroStaticSettings.AllowContextMacros)
            {
                if (obj is IContext)
                {
                    result = ((IContext)obj).GetProperty(columnName);
                    if (result != null)
                    {
                        return true;
                    }
                }
                else if (obj is TreeNode)
                {
                    // TreeNode editable fields
                    TreeNode node = (TreeNode)obj;
                    if (node.DocumentContent[columnName] != null)
                    {
                        result = node.DocumentContent[columnName];
                        return true;
                    }
                }
                else if (obj is PageInfo)
                {
                    // Page info editable content
                    PageInfo pi = (PageInfo)obj;
                    if (pi.EditableItems[columnName] != null)
                    {
                        result = pi.EditableItems[columnName];
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
