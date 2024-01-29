namespace CMS.Base
{
    /// <summary>
    /// Simple thread handler
    /// </summary>
    public class URLHandler : SimpleHandler<URLHandler, URLEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="url">Handled URL</param>
        public URLEventArgs StartEvent(string url)
        {
            var e = new URLEventArgs
            {
                URL = url
            };

            var h = StartEvent(e);

            return h;
        }
    }
}