using CMS.Base;

namespace CMS.FileSystemStorage
{
    /// <summary>
    /// EVent arguments used for <see cref="File.GetFileUrlForPath"/> event.
    /// </summary>
    public class GetFileUrlEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Url to file path result.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets the input file path.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the site name.
        /// </summary>
        public string SiteName { get; }


        /// <summary>
        /// Creates new instance of <see cref="GetFileUrlEventArgs"/>
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="siteName">Site name.</param>
        public GetFileUrlEventArgs(string path, string siteName)
        {
            Path = path;
            SiteName = siteName;
        }
    }
}
