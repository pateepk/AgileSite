using System.Collections.Generic;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IMultiBuyDiscountsService), typeof(MultiBuyDiscountsService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines the contract for classes providing multibuy discount evaluation.
    /// </summary>
    public interface IMultiBuyDiscountsService
    {
        /// <summary>
        /// Evaluates multibuy discounts specified by the <paramref name="parameters"/> on the given <paramref name="items"/> collection 
        /// using the <paramref name="applicator"/>.
        /// </summary>
        /// <param name="items">Items to be evaluated.</param>
        /// <param name="parameters">Parameter of the discounts.</param>
        /// <param name="applicator">Applicator to be used to apply results of the evaluation.</param>
        void EvaluateDiscounts(IEnumerable<MultiBuyItem> items, DiscountsParameters parameters, IMultiBuyDiscountsApplicator applicator);
    }
}