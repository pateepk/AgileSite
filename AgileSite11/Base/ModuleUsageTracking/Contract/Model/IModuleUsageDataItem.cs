using System;
using System.Linq;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Represents one fragment of module usage data
    /// </summary>
    public interface IModuleUsageDataItem 
    {
        /// <summary>
        /// Key representing the item. Must be unique within one data source.
        /// </summary>
        string Key
        {
            get;
        }


        /// <summary>
        /// Item value
        /// </summary>
        object Value
        {
            get;
        }
    }
}
