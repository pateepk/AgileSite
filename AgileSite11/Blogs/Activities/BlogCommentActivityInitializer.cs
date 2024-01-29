using CMS.Activities;
using CMS.Base;
using CMS.DocumentEngine;

namespace CMS.Blogs
{
    /// <summary>
    /// Provides initialization for Blog comment activity.
    /// </summary>
    internal class BlogCommentActivityInitializer : IActivityInitializer
	{		
        private readonly TreeNode mBlog;
		private readonly ITreeNode mBlogPost;
		private readonly BlogCommentInfo mBlogComment;
		private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Initializes new instance of <see cref="BlogCommentActivityInitializer"/>
        /// </summary>
        /// <param name="blog">Blog node</param>
        /// <param name="blogPost">Blog post node</param>
        /// <param name="blogComment">Blog comment info</param>        
        public BlogCommentActivityInitializer(TreeNode blog, TreeNode blogPost, BlogCommentInfo blogComment)
		{
			mBlog = blog;
			mBlogPost = blogPost;
			mBlogComment = blogComment;
		}


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public void Initialize(IActivityInfo activity)
		{
			activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mBlog.DocumentName);
			activity.ActivityItemID = mBlogComment.CommentID;
			activity.ActivityItemDetailID = mBlog.NodeID;
            activity.ActivityNodeID = mBlogPost.NodeID;
			activity.ActivityCulture = mBlogPost.DocumentCulture;
		}


        /// <summary>
        /// Activity type.
        /// </summary>
        public string ActivityType
		{
			get
			{
				return PredefinedActivityType.BLOG_COMMENT;
			}
		}


        /// <summary>
        /// Settings key name.
        /// </summary>
        public string SettingsKeyName
		{
			get
			{
				return "CMSCMBlogPostComments";
			}
		}
	}
}
