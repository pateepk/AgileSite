using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.Forums
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class ForumsResolvers : ResolverDefinition
    {
        #region "Variables"

        private static MacroResolver mForumsResolver = null;
        private static MacroResolver mForumsSubscribeResolver = null;

        #endregion


        /// <summary>
        /// Forums e-mail template macro resolver.
        /// </summary>
        public static MacroResolver ForumResolver
        {
            get
            {
                if (mForumsResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    resolver.SetNamedSourceData("ForumPost", ModuleManager.GetReadOnlyObject(ForumPostInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("Forum", ModuleManager.GetReadOnlyObject(ForumInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("ForumGroup", ModuleManager.GetReadOnlyObject(ForumGroupInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("Subscriber", ModuleManager.GetReadOnlyObject("forums.forumsubscription"));

                    RegisterStringValues(resolver, new[]
                    {
                        "ForumDisplayName", "PostSubject", "Link", "ForumName", "PostText", "PostUsername",
                        "PostTime", "GroupDisplayname", "GroupName", "GroupDescription", "ForumDescription", "UnsubscribeLink"
                    });

                    mForumsResolver = resolver;
                }

                return mForumsResolver;
            }
        }


        /// <summary>
        /// Forums subscription e-mail template macro resolver.
        /// </summary>
        public static MacroResolver ForumSubscribtionResolver
        {
            get
            {
                if (mForumsSubscribeResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    // Register objects
                    resolver.SetNamedSourceData("ForumPost", ModuleManager.GetReadOnlyObject(ForumPostInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("Forum", ModuleManager.GetReadOnlyObject(ForumInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("ForumGroup", ModuleManager.GetReadOnlyObject(ForumGroupInfo.OBJECT_TYPE));

                    // Register flat values
                    RegisterStringValues(resolver, new [] { "ForumDisplayName", "Subject", "Link", "UnsubscribeLink", "SubscribeLink", "Separator", "OptInInterval" });

                    // Save the resolver for future use
                    mForumsSubscribeResolver = resolver;
                }
                return mForumsSubscribeResolver;
            }
        }
    }
}