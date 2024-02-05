using System;

namespace CMS.Helpers
{
    /// <summary>
    /// Specifies the order for an enum field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class EnumOrderAttribute : Attribute
    {
        private readonly int mOrder;


        /// <summary>
        /// Gets the order.
        /// </summary>
        public int Order
        {
            get
            {
                return mOrder;
            }
        }


        /// <summary>
        /// Specifies the order for an enum field.
        /// </summary>
        /// <param name="order">Order of the attributed enum field</param>
        public EnumOrderAttribute(int order)
        {
            mOrder = order;
        }
    }
}
