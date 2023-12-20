using System;

namespace CMS.Blogs
{
    /// <summary>
    /// Provides possibility to log blogs activities.
    /// </summary>
    public interface IBlogsActivityLogger
    {
        /// <summary>
        /// Logs Blog comment activity.
        /// </summary>
        /// <param name="blogCommentInfo"><see cref="BlogCommentInfo"/> to log</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="blogCommentInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when blog post or blog of <paramref name="blogCommentInfo"/> is <c>null</c>.</exception>
        void LogBlogCommentActivity(BlogCommentInfo blogCommentInfo);


        /// <summary>
        /// Logs Blog post subscription.
        /// </summary>
        /// <param name="blogPostSubscriptionInfo"><see cref="BlogPostSubscriptionInfo"/> to log</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="blogPostSubscriptionInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when blog post or blog of <paramref name="blogPostSubscriptionInfo"/> is <c>null</c>.</exception>
        void LogBlogPostSubscriptionActivity(BlogPostSubscriptionInfo blogPostSubscriptionInfo);


        /// <summary>
        /// Logs Blog post subscription.
        /// </summary>
        /// <param name="blogPostSubscriptionInfo"><see cref="BlogPostSubscriptionInfo"/> to log</param>
        /// <param name="contactId">Contact to log activity for</param>
        /// <param name="siteId">Site to log activity for</param>
        /// <param name="campaign">Campaign to log activity for</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="blogPostSubscriptionInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when blog post or blog of <paramref name="blogPostSubscriptionInfo"/> is <c>null</c>.</exception>
        void LogBlogPostSubscriptionActivity(BlogPostSubscriptionInfo blogPostSubscriptionInfo, int contactId, int siteId, string campaign);
    }
}