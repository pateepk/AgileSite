using System;


namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents the type of customer from taxes point of view.
    /// </summary>
    public abstract class CustomerTaxClass
    {
        /// <summary>
        /// Predefined customer tax class for taxable customers.
        /// </summary>
        public static readonly CustomerTaxClass Taxable = new SimpleCustomerTaxClass(_ => false);


        /// <summary>
        /// Predefined customer tax class for tax exempt customers.
        /// </summary>
        public static readonly CustomerTaxClass Exempt = new SimpleCustomerTaxClass(tax => tax.TaxClassZeroIfIDSupplied);


        /// <summary>
        /// Returns true if this class of customer is exempt from given tax class.
        /// </summary>
        /// <param name="taxClass">Tax class which exemption is examined.</param>
        public abstract bool IsTaxExempt(TaxClassInfo taxClass);


        private class SimpleCustomerTaxClass : CustomerTaxClass
        {
            private readonly Predicate<TaxClassInfo> mPredicate;


            public SimpleCustomerTaxClass(Predicate<TaxClassInfo> predicate)
            {
                mPredicate = predicate;
            }


            public override bool IsTaxExempt(TaxClassInfo taxClass)
            {
                return mPredicate.Invoke(taxClass);
            }
        }
    }
}
