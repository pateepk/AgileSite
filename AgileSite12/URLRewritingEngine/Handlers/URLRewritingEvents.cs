namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// URL rewriting events
    /// </summary>
    public class URLRewritingEvents
    {
        /// <summary>
        /// Fires when the URL rewriting result is processed
        /// </summary>
        public static URLRewritingHandler ProcessRewritingResult = new URLRewritingHandler { Name = "URLRewritingEvents.ProcessRewritingResult" };


        /// <summary>
        /// Fires when the URL rewriting processing page not found request
        /// </summary>
        public static URLRewriterPageNotFoundHandler PageNotFound = new URLRewriterPageNotFoundHandler { Name = "URLRewritingEvents.PageNotFound" };


        /// <summary>
        /// Fires when PageInfo potentially using A/B test is required
        /// </summary>
        public static ProcessABTestHandler ProcessABTest = new ProcessABTestHandler { Name = "URLRewritingEvents.ProcessABTest" };
    }
}
