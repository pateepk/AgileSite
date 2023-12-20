using CMS.Activities;
using CMS.DocumentEngine;
using CMS.Helpers;

namespace CMS.Blogs
{
    /// <summary>
    /// Provides initialization for Blog post subscription activity.
    /// </summary>
    internal class BlogPostSubscriptionActivityInitializer : IActivityInitializer
    {
        private readonly TreeNode mBlog;
        private readonly TreeNode mBlogPost;
        private readonly BlogPostSubscriptionInfo mBlogPostSubscriptionInfo;
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Initializes new instance of <see cref="BlogPostSubscriptionActivityInitializer"/>
        /// </summary>
        /// <param name="blog">Blog node</param>
        /// <param name="blogPost">Blog post node</param>
        /// <param name="blogPostSubscriptionInfo">Blog post subscription info</param>
        public BlogPostSubscriptionActivityInitializer(TreeNode blog, TreeNode blogPost, BlogPostSubscriptionInfo blogPostSubscriptionInfo)
        {
            mBlog = blog;
            mBlogPost = blogPost;
            mBlogPostSubscriptionInfo = blogPostSubscriptionInfo;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mBlogPostSubscriptionInfo.SubscriptionEmail);
            activity.ActivityItemDetailID = mBlog.NodeID;
            activity.ActivityItemID = mBlogPostSubscriptionInfo.SubscriptionID;
            activity.ActivityCulture = mBlogPost.DocumentCulture;
            activity.ActivityNodeID = mBlogPost.NodeID;
            activity.ActivityValue = TextHelper.LimitLength(mBlog.DocumentName, 250);
        }


        /// <summary>
        /// Activity type.
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.SUBSCRIPTION_BLOG_POST;
            }
        }


        /// <summary>
        /// Settings key name.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMBlogPostSubscription";
            }
        }        
    }
}
