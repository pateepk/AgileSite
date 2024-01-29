using CMS.Base;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// GetMediaData event handler.
    /// </summary>
    public class GetMediaDataHandler : AdvancedHandler<GetMediaDataHandler, GetMediaDataEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="url">URL of media object</param>
        /// <param name="siteName">Site name</param>
        public GetMediaDataHandler StartEvent(string url, string siteName)
        {
            var e = new GetMediaDataEventArgs
            {
                Url = url,
                SiteName = siteName,
            };

            return StartEvent(e);
        }
    }
}
