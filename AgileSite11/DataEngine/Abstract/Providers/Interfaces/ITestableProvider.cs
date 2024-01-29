using System.ComponentModel;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Adds testing support to info provider.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ITestableProvider : ICustomizableProvider
    {
        /// <summary>
        /// Data source for the provider
        /// </summary>
        DataQuerySource DataSource
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the current provider instance
        /// </summary>
        ITestableProvider GetCurrentProvider();


        /// <summary>
        /// Resets the provider to default implementation.
        /// </summary>
        void ResetToDefault();
    }
}