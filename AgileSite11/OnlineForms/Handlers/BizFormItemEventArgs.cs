using System;

using CMS.Base;

namespace CMS.OnlineForms
{
    /// <summary>
    /// BizForm item event arguments
    /// </summary>
    public class BizFormItemEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Processed BizForm item
        /// </summary>
        public BizFormItem Item
        {
            get;
            set;
        }
    }
}