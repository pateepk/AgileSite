namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Context manager interface.
    /// </summary>
    public interface IControlContextManager
    {
        /// <summary>
        /// Gets the current ControlContext object.
        /// </summary>
        ControlContext ControlContext
        {
            get;
        }
    }
}