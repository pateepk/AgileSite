namespace CMS.Helpers
{
    /// <summary>
    /// Defines contract for cache item container.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    /// <exclude />
    public interface ICacheItemContainer
    {
        /// <summary>
        /// Data item.
        /// </summary>
        object Data { get; }
    }
}
