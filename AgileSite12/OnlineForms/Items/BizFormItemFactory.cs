using System;

using CMS.Core;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Factory that provides BizForm item objects
    /// </summary>
    public class BizFormItemFactory : ObjectFactory<BizFormItem>
    {
        /// <summary>
        /// BizForm item type
        /// </summary>
        public Type Type 
        { 
            get; 
            protected set; 
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of the BizForm class</param>
        public BizFormItemFactory(Type type)
        {
            Type = type;
        }


        /// <summary>
        /// Creates new BizForm item object
        /// </summary>
        public override object CreateNewObject()
        {
            return Activator.CreateInstance(Type);
        }
    }
}
