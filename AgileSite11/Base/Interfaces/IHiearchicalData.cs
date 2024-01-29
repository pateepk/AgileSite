using System.Collections;

namespace CMS.Base
{
    /// <summary>
    /// Hierarchical data interface
    /// </summary>
    public interface IGroupedData : IEnumerable
    {
        /// <summary>
        /// Top item in the hierarchy
        /// </summary>
        object TopItem
        {
            get;
        }


        /// <summary>
        /// Gets the items based on the given key
        /// </summary>
        /// <param name="key">Key</param>
        IList GetItems(object key);
    }
}
