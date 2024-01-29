using System.Collections.Generic;
using System.Linq;

using CMS.Core;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of <see cref="IMultiBuyDiscountsService"/>.
    /// </summary>
    internal class MultiBuyDiscountsService : IMultiBuyDiscountsService
    {
        private readonly IMultiBuyDiscountSource mDiscountSource;


        public MultiBuyDiscountsService(IMultiBuyDiscountSource discountSource)
        {
            mDiscountSource = discountSource;
        }


        /// <summary>
        /// Evaluates multibuy discounts specified by the <paramref name="parameters"/> on the given <paramref name="items"/> collection 
        /// using the <paramref name="applicator"/>.
        /// </summary>
        /// <param name="items">Items to be evaluated.</param>
        /// <param name="parameters">Parameter of the discounts.</param>
        /// <param name="applicator">Applicator to be used to apply results of the evaluation.</param>
        public void EvaluateDiscounts(IEnumerable<MultiBuyItem> items, DiscountsParameters parameters, IMultiBuyDiscountsApplicator applicator)
        {
            var allItems = items.ToList();

            var discounts = mDiscountSource.GetDiscounts(parameters);

            // Pre-filter discounts by products for evaluator
            var applicableByProducts = discounts.Where(d => allItems.Any(d.IsBasedOn)).ToList();

            var evaluator = Service.Resolve<IMultiBuyDiscountsEvaluator>();
            evaluator.EvaluateDiscounts(applicableByProducts, allItems, applicator);
        }
    }
}