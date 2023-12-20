namespace CMS.Base
{
    /// <summary>
    /// Defines implementation capable of processing web requests.
    /// </summary>
    public interface IServer
    {
        /// <summary>
        /// Gets or sets the request time-out value in seconds.
        /// </summary>
        int ScriptTimeout { get; set; }


        /// <summary>
        /// Returns the physical file path that corresponds to the specified virtual path on the Web server.
        /// </summary>
        /// <param name="path">The virtual path to get the physical path for.</param>
        /// <returns>The physical file path that corresponds to <paramref name="path" />.</returns>
        string MapPath(string path);


        /// <summary>
        /// URL-encodes the path section of a URL string.
        /// </summary>
        /// <param name="requestUrl">The string to URL-encode.</param>
        /// <returns>The URL-encoded text.</returns>
        string UrlPathEncode(string requestUrl);


        /// <summary>
        /// Terminates execution of the current process and starts execution of a page or handler that is specified with a URL.
        /// </summary>
        /// <param name="path">The URL of the page or handler to execute.</param>
        void Transfer(string path);
    }
}
