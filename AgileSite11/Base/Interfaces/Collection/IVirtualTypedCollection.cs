using System;
using System.Linq;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Interface to provide result type over collection
    /// </summary>
    public interface IVirtualTypedCollection : ITypedCollection
    {
        /// <summary>
        /// Gets the virtual empty object representing the collection item
        /// </summary>
        object GetEmptyObject();
    }
}
