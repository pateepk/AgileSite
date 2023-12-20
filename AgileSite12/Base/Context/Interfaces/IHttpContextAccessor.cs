namespace CMS.Base
{
    /// <summary>
    /// Encapsulates the access to <see cref="IHttpContext"/> instance.
    /// </summary>
    public interface IHttpContextAccessor
    {
        /// <summary>
        /// Gets the instance of <see cref="IHttpContext"/> of current web request.
        /// </summary>
        IHttpContext HttpContext { get; }
    }
}