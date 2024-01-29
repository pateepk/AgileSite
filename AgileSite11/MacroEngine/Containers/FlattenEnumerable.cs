using System.Collections;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Container 
    /// </summary>
    public class FlattenEnumerable : IEnumerable
    {
        private IEnumerable mItems = null;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Items to flatten</param>
        public FlattenEnumerable(IEnumerable items)
        {
            mItems = items;
        }


        /// <summary>
        /// Gets the enumerator of the items
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            if (mItems != null)
            {
                // Recursively iterate items
                foreach (var p in GetItems(mItems))
                {
                    yield return p;
                }
            }
        }


        /// <summary>
        /// Gets the items for the given item
        /// </summary>
        /// <param name="item">Item to process</param>
        private static IEnumerable GetItems(object item)
        {
            if (item != null)
            {
                if (item is string)
                {
                    yield return item;
                }
                else
                {
                    // Try to iterate further
                    var en = item as IEnumerable;
                    if (en != null)
                    {
                        foreach (var subItem in en)
                        {
                            foreach (var subItemChild in GetItems(subItem))
                            {
                                yield return subItemChild;
                            }
                        }
                    }
                    else
                    {
                        yield return item;
                    }
                }
            }
        }
    }
}
