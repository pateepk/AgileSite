using CMS.Base;
using CMS.DataEngine;

namespace CMS.Automation
{
    /// <summary>
    /// Wrapper for starting automation process for given <see cref="InfoObject"/> object with additional data <see cref="AdditionalData"/> collection
    /// </summary>
    /// <typeparam name="InfoType">Info object type</typeparam>
    public class AutomationProcessItem<InfoType> 
        where InfoType : BaseInfo
    {
        /// <summary>
        /// Gets or sets the object for which is automation started
        /// </summary>
        public InfoType InfoObject
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets additional data for start automation
        /// </summary>
        public StringSafeDictionary<object> AdditionalData
        {
            get;
            set;
        }
    }
}
