using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a group of discounts applied on the same base price.
    /// </summary>
    /// <threadsafety static="false" instance="false"/>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class DiscountCollection : IEnumerable<IDiscount>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly List<IDiscount> mDiscounts = new List<IDiscount>();


        /// <summary>
        /// Initializes a new instance of the <see cref="DiscountCollection"/> class that
        /// contains <see cref="IDiscount"/> elements copied from the specified collection.
        /// </summary>
        /// <param name="discounts">The collection whose discounts are copied to the new list.</param>
        public DiscountCollection(IEnumerable<IDiscount> discounts)
        {
            if (discounts == null)
            {
                throw new ArgumentNullException(nameof(discounts));
            }

            mDiscounts.AddRange(discounts);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DiscountCollection"/> class that is empty.
        /// </summary>
        public DiscountCollection()
        {
        }


        /// <summary>
        /// Gets the number of discounts contained in the <see cref="DiscountCollection"/>.
        /// </summary>
        public int Count => mDiscounts.Count;


        /// <summary>
        /// Adds a discount to the collection.
        /// </summary>
        /// <param name="discount">The discount to be added.</param>
        public void Add(IDiscount discount)
        {
            if (discount == null)
            {
                throw new ArgumentNullException(nameof(discount));
            }

            mDiscounts.Add(discount);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection
        /// </summary>
        public IEnumerator<IDiscount> GetEnumerator()
        {
            return mDiscounts.GetEnumerator();
        }
    }
}