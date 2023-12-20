namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Defines control capable of encoding output during rendering.
    /// </summary>
    public interface IOutputEncodingControl
    {
        /// <summary>
        /// Gets or sets the value indicating whether control output is encoded or not.
        /// </summary>
        bool EncodeOutput
        {
            get;
            set;
        }
    }
}