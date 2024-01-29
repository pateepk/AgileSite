using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;

namespace CMS.Blogs
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class BlogResolvers : ResolverDefinition
    {
        #region "Variables"

        private static MacroResolver mBlogResolver;
        private static MacroResolver mBlogSubscribeResolver;

        #endregion

        /// <summary>
        /// Blog e-mail template macro resolver.
        /// </summary>
        public static MacroResolver BlogResolver
        {
            get
            {
                if (mBlogResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    // Blog document type doesn't exists, fill with the root doc. type
                    if (DataHelper.DataSourceIsEmpty(DataClassInfoProvider.GetDataClassInfo("cms.blog")))
                    {
                        resolver.SetNamedSourceData("Blog", TreeNode.New(SystemDocumentTypes.Root));
                    }
                    else
                    {
                        resolver.SetNamedSourceData("Blog", TreeNode.New("CMS.Blog"));
                    }

                    // BlogPost document type doesn't exists, fill with the root doc. type
                    if (DataHelper.DataSourceIsEmpty(DataClassInfoProvider.GetDataClassInfo("cms.blogpost")))
                    {
                        resolver.SetNamedSourceData("BlogPost", TreeNode.New(SystemDocumentTypes.Root));
                    }
                    else
                    {
                        resolver.SetNamedSourceData("BlogPost", TreeNode.New("CMS.BlogPost"));
                    }
                    resolver.SetNamedSourceData("Comment", ModuleManager.GetReadOnlyObject(BlogCommentInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("CommentUser", ModuleManager.GetReadOnlyObject(UserInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("CommentUserSettings", ModuleManager.GetReadOnlyObject(UserSettingsInfo.OBJECT_TYPE));

                    // Register flat values
                    RegisterStringValues(resolver, new[] { "UserFullName", "CommentUrl", "Comments", "CommentDate", "BlogPostTitle", "BlogLink", "BlogPostLink", "UnsubscriptionLink" });

                    // Set BlogResolver
                    mBlogResolver = resolver;
                }

                return mBlogResolver;
            }
        }


        /// <summary>
        /// Blog e-mail template macro resolver.
        /// </summary>
        public static MacroResolver BlogSubscriptionResolver
        {
            get
            {
                if (mBlogSubscribeResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    // Blog document type doesn't exists, fill with the root doc. type
                    if (DataHelper.DataSourceIsEmpty(DataClassInfoProvider.GetDataClassInfo("cms.blog")))
                    {
                        resolver.SetNamedSourceData("Blog", TreeNode.New(SystemDocumentTypes.Root));
                    }
                    else
                    {
                        resolver.SetNamedSourceData("Blog", TreeNode.New("CMS.Blog"));
                    }

                    // BlogPost document type doesn't exists, fill with the root doc. type
                    if (DataHelper.DataSourceIsEmpty(DataClassInfoProvider.GetDataClassInfo("cms.blogpost")))
                    {
                        resolver.SetNamedSourceData("BlogPost", TreeNode.New(SystemDocumentTypes.Root));
                    }
                    else
                    {
                        resolver.SetNamedSourceData("BlogPost", TreeNode.New("CMS.BlogPost"));
                    }

                    // Register flat values
                    RegisterStringValues(resolver, new[] { "UserFullName", "BlogPostTitle", "BlogLink", "BlogPostLink", "UnsubscriptionLink", "SubscriptionLink", "OptInInterval" });

                    mBlogSubscribeResolver = resolver;
                }

                return mBlogSubscribeResolver;
            }
        }
    }
}