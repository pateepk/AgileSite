using System;
using System.Web;

using CMS;
using CMS.Activities;
using CMS.Blogs;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;

[assembly: RegisterImplementation(typeof(IBlogsActivityLogger), typeof(BlogsActivityLogger), Priority = RegistrationPriority.SystemDefault, Lifestyle = Lifestyle.Singleton)]

namespace CMS.Blogs
{
    /// <summary>
    /// Provides methods for logging blogs activities.
    /// </summary>
    public class BlogsActivityLogger : IBlogsActivityLogger
    {
		private readonly IActivityLogService mActivityLogService = Service.Resolve<IActivityLogService>();


        /// <summary>
        /// Logs Blog comment activity.
        /// </summary>
        /// <param name="blogCommentInfo"><see cref="BlogCommentInfo"/> to log</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="blogCommentInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when blog post or blog of <paramref name="blogCommentInfo"/> is <c>null</c>.</exception>
        public void LogBlogCommentActivity(BlogCommentInfo blogCommentInfo)
		{
            if (blogCommentInfo == null)
            {
                throw new ArgumentNullException("blogCommentInfo");
            }

            var blogPost = GetBlogPost(blogCommentInfo.CommentPostDocumentID);
            if (blogPost == null)
            {
                throw new ArgumentException("[BlogsActivityLogger.LogBlogCommentActivity]: Can't find blog post for the given comment", "blogCommentInfo");
            }

            var blogNode = GetParentBlog(blogCommentInfo.CommentPostDocumentID);
            if (blogNode == null)
            {
                throw new ArgumentException("[BlogsActivityLogger.LogBlogCommentActivity]: Can't find blog for the given comment", "blogCommentInfo");
            }

            var activityInitializer = new BlogCommentActivityInitializer(blogNode, blogPost, blogCommentInfo);
			LogActivityIfEnabled(activityInitializer, blogPost.GetBooleanValue("BlogLogActivity", false));
		}


        /// <summary>
        /// Logs Blog post subscription.
        /// </summary>
        /// <param name="blogPostSubscriptionInfo"><see cref="BlogPostSubscriptionInfo"/> to log</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="blogPostSubscriptionInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when blog post or blog of <paramref name="blogPostSubscriptionInfo"/> is <c>null</c>.</exception>
        public void LogBlogPostSubscriptionActivity(BlogPostSubscriptionInfo blogPostSubscriptionInfo)
        {
            if (blogPostSubscriptionInfo == null)
            {
                throw new ArgumentNullException("blogPostSubscriptionInfo");
            }

            if (blogPostSubscriptionInfo.SubscriptionPostDocumentID > 0)
            {
                var blogPost = GetBlogPost(blogPostSubscriptionInfo.SubscriptionPostDocumentID);
                if (blogPost == null)
                {
                    throw new ArgumentException("[BlogsActivityLogger.LogBlogPostSubscriptionActivity]: Can't find blog post for the given subscription", "blogPostSubscriptionInfo");
                }

                var blogNode = GetParentBlog(blogPostSubscriptionInfo.SubscriptionPostDocumentID);
                if (blogNode == null)
                {
                    throw new ArgumentException("[BlogsActivityLogger.LogBlogPostSubscriptionActivity]: Can't find blog for the given subscription", "blogPostSubscriptionInfo");
                }

                var activityInializer = new BlogPostSubscriptionActivityInitializer(blogNode, blogPost, blogPostSubscriptionInfo);
                LogActivityIfEnabled(activityInializer, blogPost.GetBooleanValue("BlogLogActivity", false));
            }
        }


        /// <summary>
        /// Logs Blog post subscription.
        /// </summary>
        /// <param name="blogPostSubscriptionInfo"><see cref="BlogPostSubscriptionInfo"/> to log</param>
        /// <param name="contactId">Contact to log activity for</param>
        /// <param name="siteId">Site to log activity for</param>
        /// <param name="campaign">Campaign to log activity for</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="blogPostSubscriptionInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when blog post or blog of <paramref name="blogPostSubscriptionInfo"/> is <c>null</c>.</exception>
        public void LogBlogPostSubscriptionActivity(BlogPostSubscriptionInfo blogPostSubscriptionInfo, int contactId, int siteId, string campaign)
        {
            if (blogPostSubscriptionInfo == null)
            {
                throw new ArgumentNullException("blogPostSubscriptionInfo");
            }

            if (blogPostSubscriptionInfo.SubscriptionPostDocumentID > 0)
            {
                var blogPost = GetBlogPost(blogPostSubscriptionInfo.SubscriptionPostDocumentID);
                if (blogPost == null)
                {
                    throw new ArgumentException("[BlogsActivityLogger.LogBlogPostSubscriptionActivity]: Can't find blog post for the given subscription", "blogPostSubscriptionInfo");
                }

                var blogNode = GetParentBlog(blogPostSubscriptionInfo.SubscriptionPostDocumentID);
                if (blogNode == null)
                {
                    throw new ArgumentException("[BlogsActivityLogger.LogBlogPostSubscriptionActivity]: Can't find blog for the given subscription", "blogPostSubscriptionInfo");
                }

                var activityInializer = new BlogPostSubscriptionActivityInitializer(blogNode, blogPost, blogPostSubscriptionInfo)
                    .WithContactId(contactId)
                    .WithSiteId(siteId)
                    .WithCampaign(campaign);
                LogActivityIfEnabled(activityInializer, blogPost.GetBooleanValue("BlogLogActivity", false));
            }
        }


        /// <summary>
        /// Gets parent blog of the specified document.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns></returns>
        protected virtual TreeNode GetParentBlog(int documentId)
        {
            return BlogHelper.GetParentBlog(documentId, false);
        }


        /// <summary>
        /// Gets current document version of the specified document.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns></returns>
        protected virtual TreeNode GetBlogPost(int documentId)
        {
            return DocumentHelper.GetDocument(documentId, new TreeProvider());
        }


        /// <summary>
        /// Returns current request.
        /// </summary>
        /// <returns>Current request.</returns>
        protected virtual HttpRequestBase GetCurrentRequest()
		{
			return CMSHttpContext.Current.Request;
		}


        /// <summary>
        /// Calls <see cref="IActivityLogService"/> and logs the activity using the given <paramref name="activityInitializer"/> if logging of on-line marketing activities is enabled on given blog post.
        /// </summary>
        /// <param name="activityInitializer">Activity initializer used to initialize logged activity</param>
        /// <param name="logActivity">Flag from given blog post specifying whether the logging of on-line marketing activities is enabled</param>
        private void LogActivityIfEnabled(IActivityInitializer activityInitializer, bool logActivity)
		{
			if (logActivity)
			{
				mActivityLogService.Log(activityInitializer, GetCurrentRequest());
			}
		}
	}
}
