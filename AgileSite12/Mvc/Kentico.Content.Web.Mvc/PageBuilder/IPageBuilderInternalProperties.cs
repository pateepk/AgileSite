using System;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides internal properties of Page builder feature.
    /// </summary>
    internal interface IPageBuilderInternalProperties
    {
        /// <summary>
        /// The editing instance identifier used for admin user interface.
        /// </summary>
        Guid EditingInstanceIdentifier { get; }


        /// <summary>
        /// The Page builder feature data context.
        /// </summary>
        IPageBuilderDataContext DataContext { get; }
    }
}
