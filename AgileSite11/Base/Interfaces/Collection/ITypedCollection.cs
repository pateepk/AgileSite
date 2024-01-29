using System;
using System.Linq;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Interface to provide result type over collection
    /// </summary>
    public interface ITypedCollection
    {
        /// <summary>
        /// Result type
        /// </summary>
        Type ItemType
        {
            get;
        }
    }
}
