using System.Collections.Generic;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IMultiBuyDiscountsEvaluator), typeof(MultiBuyDiscountsEvaluator), Priority = CMS.Core.RegistrationPriority.Fallback, Lifestyle = CMS.Core.Lifestyle.Transient)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface for classes that handle application of multi buy discounts on set of cart items.
    /// </summary>
    public interface IMultiBuyDiscountsEvaluator
    {
        /// <summary>
        /// Evaluates given discounts and applies matching ones to corresponding cart items.
        /// </summary>
        /// <param name="discounts">Discounts to be evaluated.</param>
        /// <param name="cartItems">Cart items to be evaluated.</param>
        /// <param name="applicator">Applicator to be used to apply results of the evaluation.</param>
        void EvaluateDiscounts(IEnumerable<IMultiBuyDiscount> discounts, IEnumerable<MultiBuyItem> cartItems, IMultiBuyDiscountsApplicator applicator);
    }
}
