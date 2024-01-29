namespace CMS.Membership
{
    /// <summary>
    /// Activity points enumeration.
    /// </summary>
    public enum ActivityPointsEnum
    {
        // Note: On this enum is depened proc_cms_user_updateusercounts
        // Check if your change has no effect to correct functionality!!

        /// <summary>
        /// Forum post.
        /// </summary>
        ForumPost = 0,

        /// <summary>
        /// Messsage board post.
        /// </summary>
        MessageBoardPost = 1,

        /// <summary>
        /// Blog comment post.
        /// </summary>
        BlogCommentPost = 2,

        /// <summary>
        /// Blog posts.
        /// </summary>
        BlogPosts = 3,

        /// <summary>
        /// All.
        /// </summary>
        All = 4
    }
}