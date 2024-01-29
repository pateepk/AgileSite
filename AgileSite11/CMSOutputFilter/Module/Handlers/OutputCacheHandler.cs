using CMS.Base;
using CMS.PortalEngine;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Handler used to define events for editing output from the output cache.
    /// </summary>
    public class OutputCacheHandler : AdvancedHandler<OutputCacheHandler, OutputCacheEventArgs>
    {
        /// <summary>
        /// Initiates event processing.
        /// </summary>
        /// <param name="output">Output retrieved from the cache</param>
        /// <param name="viewMode">View mode</param>
        public OutputCacheHandler StartEvent(CachedOutput output, ViewModeOnDemand viewMode = null)
        {
            var e = new OutputCacheEventArgs
                {
                    ViewMode = viewMode ?? new ViewModeOnDemand(),
                    Output = output,
                };

            return StartEvent(e);
        }
    }
}