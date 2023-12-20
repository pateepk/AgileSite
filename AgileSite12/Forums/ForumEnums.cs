
namespace CMS.Forums
{
    /// <summary>
    /// Enumeration of forum state.
    /// </summary>
    public enum ForumStateEnum
    {
        /// <summary> 
        /// Forums.
        /// </summary>
        Forums,

        /// <summary> 
        /// Threads.
        /// </summary>
        Threads,

        /// <summary> 
        /// Thread.
        /// </summary>
        Thread,

        /// <summary> 
        /// New thread.
        /// </summary>
        NewThread,

        /// <summary> 
        /// Reply to post.
        /// </summary>
        ReplyToPost,

        /// <summary> 
        /// New subscription.
        /// </summary>
        NewSubscription,

        /// <summary>
        /// Subscription to post.
        /// </summary>
        SubscribeToPost,

        /// <summary>
        /// Edit post.
        /// </summary>
        EditPost,

        /// <summary>
        /// Attachment page.
        /// </summary>
        Attachments,

        /// <summary>
        /// Access denied.
        /// </summary>
        AccessDenied,

        /// <summary>
        /// Search.
        /// </summary>
        Search,

        /// <summary>
        /// Topic move.
        /// </summary>
        TopicMove,

        /// <summary> 
        /// Unknown.
        /// </summary>
        Unknown
    }


    /// <summary>
    /// Forum modes.
    /// </summary>
    public enum ForumMode
    {
        /// <summary> 
        /// Edit.
        /// </summary>
        Edit,

        /// <summary> 
        /// Attachment.
        /// </summary>
        Attachment,

        /// <summary> 
        /// Quote.
        /// </summary>
        Quote,

        /// <summary> 
        /// Topic move.
        /// </summary>
        TopicMove,

        /// <summary> 
        /// Unknown.
        /// </summary>
        Unknown
    }


    /// <summary>
    /// Forum action type.
    /// </summary>
    public enum ForumActionType
    {
        /// <summary>
        /// NewThread.
        /// </summary>
        NewThread,

        /// <summary>
        /// Delete.
        /// </summary>
        Delete,

        /// <summary>
        /// Edit.
        /// </summary>
        Edit,

        /// <summary>
        /// Reply.
        /// </summary>
        Reply,

        /// <summary>
        /// Quote.
        /// </summary>
        Quote,

        /// <summary>
        /// SubscribeToPost.
        /// </summary>
        SubscribeToPost,

        /// <summary>
        /// 
        /// </summary>
        SubscribeToForum,

        /// <summary>
        /// Attachment.
        /// </summary>
        Attachment,

        /// <summary>
        /// Appprove.
        /// </summary>
        Appprove,

        /// <summary>
        /// ApproveAll.
        /// </summary>
        ApproveAll,

        /// <summary>
        /// Reject.
        /// </summary>
        Reject,

        /// <summary>
        /// RejectAll.
        /// </summary>
        RejectAll,

        /// <summary>
        /// LockForum.
        /// </summary>
        LockForum,

        /// <summary>
        /// UnlockForum.
        /// </summary>
        UnlockForum,

        /// <summary>
        /// LockThread.
        /// </summary>
        LockThread,

        /// <summary>
        /// UnlockThread.
        /// </summary>
        UnlockThread,

        /// <summary>
        /// StickThread.
        /// </summary>
        StickThread,

        /// <summary>
        /// UnstickThread.
        /// </summary>
        UnstickThread,

        /// <summary>
        /// MoveStickyThreadUp.
        /// </summary>
        MoveStickyThreadUp,

        /// <summary>
        /// MoveStickyThreadDown.
        /// </summary>
        MoveStickyThreadDown,

        /// <summary>
        /// IsAnswer.
        /// </summary>
        IsAnswer,

        /// <summary>
        /// IsNotAnswer.
        /// </summary>
        IsNotAnswer,

        /// <summary>
        /// Forum.
        /// </summary>
        Forum,

        /// <summary>
        /// Thread.
        /// </summary>
        Thread,

        /// <summary>
        /// SplitThread.
        /// </summary>
        SplitThread,

        /// <summary>
        /// MoveToTheOtherForum.
        /// </summary>
        MoveToTheOtherForum,

        /// <summary>
        /// AddPostToFavorites.
        /// </summary>
        AddPostToFavorites,

        /// <summary>
        /// AddForumToFavorites.
        /// </summary>
        AddForumToFavorites,

        /// <summary>
        /// Badge.
        /// </summary>
        Badge,

        /// <summary>
        /// Forum Group.
        /// </summary>
        ForumGroup,

        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown
    }


    /// <summary>
    /// ShowMode enum.
    /// </summary>
    public enum ShowModeEnum : int
    {
        /// <summary> 
        /// Tree mode.
        /// </summary>
        TreeMode = 0,

        /// <summary> 
        /// Detail mode.
        /// </summary>
        DetailMode = 1,

        ///<summary> Dynamic detail mode </summary>
        DynamicDetailMode = 2,
    }


    /// <summary>
    /// FlatMode enum.
    /// </summary>
    public enum FlatModeEnum : int
    {
        /// <summary> 
        /// Tree mode.
        /// </summary>
        Threaded = 0,

        /// <summary> 
        /// Detail mode.
        /// </summary>
        NewestToOldest = 1,

        ///<summary> Dynamic detail mode </summary>
        OldestToNewest = 2,
    }
}