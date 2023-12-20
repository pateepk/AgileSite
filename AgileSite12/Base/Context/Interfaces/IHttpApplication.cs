namespace CMS.Base
{
    /// <summary>
    /// Defines methods for request pipeline processing.
    /// </summary>
    public interface IHttpApplication
    {
        /// <summary>
        /// Completes the processed request.
        /// </summary>
        void CompleteRequest();
    }
}
