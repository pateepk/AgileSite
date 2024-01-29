using System.Web;

namespace CMS.Base
{
    /// <summary>
    /// Automation handler
    /// </summary>
    public class GetVaryByCustomStringHandler : SimpleHandler<GetVaryByCustomStringHandler, GetVaryByCustomStringEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        public GetVaryByCustomStringEventArgs StartEvent(HttpContext context, string custom)
        {
            var e = new GetVaryByCustomStringEventArgs
                {
                    Context = context,
                    Custom = custom
                };

            return StartEvent(e);
        }
    }
}