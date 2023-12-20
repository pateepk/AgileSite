using System;

namespace CMS.DocumentEngine.PageBuilder
{
    /// <summary>
    /// Propagates widgets and template configuration from temporary data to page data.
    /// </summary>
    public interface ITempPageBuilderWidgetsPropagator
    {
        /// <summary>
        /// Propagates widgets and template configuration from temporary data to page data.
        /// </summary>
        /// <param name="page">Page to propagate data into.</param>
        /// <param name="instanceGuid">Instance GUID of editing.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        void Propagate(TreeNode page, Guid instanceGuid);


        /// <summary>
        /// Deletes widgets configuration from temporary data.
        /// </summary>
        /// <param name="instanceGuid">Instance GUID of editing.</param>
        void Delete(Guid instanceGuid);
    }
}
