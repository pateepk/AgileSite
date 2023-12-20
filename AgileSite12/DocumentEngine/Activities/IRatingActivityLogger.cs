using System;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides possibility to log rating activity.
    /// </summary>
    public interface IRatingActivityLogger
    {
        /// <summary>
        /// Logs rating activity.
        /// </summary>
        /// <param name="value">Rating value</param>
        /// <param name="currentDocument">Rated document</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="currentDocument"/> is <c>null</c>.</exception>
        void LogRating(double value, TreeNode currentDocument);
    }
}