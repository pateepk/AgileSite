using System;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Option that may provide modification or customization of an item.
    /// </summary>
    public class CalculationRequestItemOption
    {
        /// <summary>
        /// Identifier of the corresponding cart item option.
        /// </summary>
        public Guid OptionGuid
        {
            get;
            set;
        }


        /// <summary>
        /// Item option's SKU
        /// </summary>
        public SKUInfo SKU
        {
            get;
            set;
        }
    }
}