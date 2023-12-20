using System.ComponentModel;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.UIControls.Internal
{
    /// <summary>
    /// Helper methods and options used for testing purposes.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HotfixTestHelper
    {
        /// <summary>
        /// Indicates whether hotfix is executed for testing purposes
        /// </summary>
        /// <remarks>
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        /// <exclude />
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static bool IsRunningInTestMode
        {
            get;
            set;
        }


        /// <summary>
        /// Applies hotfix procedure.
        /// </summary>
        /// <remarks>
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        /// <exclude />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ApplyHotfix()
        {
            IsRunningInTestMode = true;

            CacheHelper.ClearCache();
            ModuleManager.ClearHashtables();

            HotfixProcedure.Hotfix();
        }
    }
}
