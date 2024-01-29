namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Represents the object edit menu control interface.
    /// </summary>
    public interface IObjectEditMenu
    {
        /// <summary>
        /// Gets the associated object manager control.
        /// </summary>
        ICMSObjectManager AbstractObjectManager
        {
            get;
        }


        /// <summary>
        /// Gets or sets a value that indicates if the Save action should be visible.
        /// </summary>
        bool ShowSave
        {
            get;
            set;
        }
    }
}
