namespace CMS.Base
{
    /// <summary>
    /// Describes implementation capable to gather information about the capabilities of the browser that made the current request using request headers.
    /// </summary>
    public interface IBrowser
    {
        /// <summary>
        /// Gets the browser string (if any) that was sent by the browser in the <see langword="User-Agent" /> request header.
        /// </summary>
        string Browser { get; }


        /// <summary>
        /// Gets a value that indicates whether the browser is a search-engine Web crawler.
        /// </summary>
        bool Crawler { get; }


        /// <summary>
        /// Gets the major (integer) version number of the browser.
        /// </summary>
        int MajorVersion { get; }


        /// <summary>
        /// Gets the minor (double) version number of the browser.
        /// </summary>
        double MinorVersion { get; }


        /// <summary>
        /// Gets the full version number (integer and decimal) of the browser as a string.
        /// </summary>
        string Version { get; }


        /// <summary>
        /// Gets a value that indicates whether the browser is a recognized mobile device.
        /// </summary>
        bool IsMobileDevice { get; }


        /// <summary>
        /// Gets a value that indicates whether the client is a Win32-based computer.
        /// </summary>
        bool Win32 { get; }


        /// <summary>
        /// Gets the value of the specified browser capability.
        /// </summary>
        string this[string key] { get; }
    }
}
