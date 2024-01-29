using System;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Page cycle enumeration.
    /// </summary>
    public enum PageCycleEnum : int
    {
        /// <summary>
        /// Item has been created.
        /// </summary>
        Created = 0,

        /// <summary>
        /// Item is just being initialized by the master control (parent).
        /// </summary>
        ParentInitializing = 1,

        /// <summary>
        /// Item is just being initialized (before or inside of Init event).
        /// </summary>
        Initializing = 2,

        /// <summary>
        /// Item has been initialized (fired Init event).
        /// </summary>
        Initialized = 3,

        /// <summary>
        /// Item is just being loaded (before or inside of Load event).
        /// </summary>
        Loading = 4,

        /// <summary>
        /// Item has been loaded (fired Load event).
        /// </summary>
        Loaded = 5,

        /// <summary>
        /// Item is just being prerendered (before or inside of PreRender event).
        /// </summary>
        PreRendering = 6,

        /// <summary>
        /// Item has been prerendered (fired PreRender event).
        /// </summary>
        PreRendered = 7,

        /// <summary>
        /// Item is just being rendered (before or inside of Render event).
        /// </summary>
        Rendering = 8,

        /// <summary>
        /// Item has already been rendered (fired Render event).
        /// </summary>
        Rendered = 9
    }
}