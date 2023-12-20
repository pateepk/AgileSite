namespace CMS.Base
{
    /// <summary>
    /// Describes implementation that contain methods for setting cache-specific HTTP headers and for controlling the web page output cache.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Sets the <see langword="Cache-Control" /> header to the specified <see cref="T:System.Web.HttpCacheability" /> value.
        /// </summary>
        /// <param name="cacheability">The <see cref="HttpCacheability" /> enumeration value to set the header to.</param>
        void SetCacheability(HttpCacheability cacheability);
    }
}
