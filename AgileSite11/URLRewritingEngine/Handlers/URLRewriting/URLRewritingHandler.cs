using CMS.Base;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// URLRewriting handler
    /// </summary>
    public class URLRewritingHandler : AdvancedHandler<URLRewritingHandler, URLRewritingEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="parameters">URL Rewriting parameters</param>
        public URLRewritingHandler StartEvent(URLRewritingParams parameters)
        {
            var e = new URLRewritingEventArgs
            {
                Parameters = parameters
            };

            var h = StartEvent(e);

            return h;
        }
    }
}