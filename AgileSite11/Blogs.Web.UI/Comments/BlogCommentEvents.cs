namespace CMS.Blogs.Web.UI
{
    /// <summary>
    /// Fired when comment action (edit, delete, reject, approve, ..) is performed.
    /// </summary>
    /// <param name="actionName">Action name</param>
    /// <param name="actionArgument">Action argument</param>
    public delegate void OnCommentActionEventHandler(string actionName, object actionArgument);


    /// <summary>
    /// Fired before the comment is saved.
    /// </summary>
    public delegate void OnBeforeCommentSavedEventHandler();


    /// <summary>
    /// Fired after the comment is saved.
    /// </summary>
    /// <param name="commentObj">Comment data</param>
    public delegate void OnAfterCommentSavedEventHandler(BlogCommentInfo commentObj);
}