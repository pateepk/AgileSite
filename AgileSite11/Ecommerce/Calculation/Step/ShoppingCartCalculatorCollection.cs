using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using CMS.Core;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents the ordered collection of calculation pipeline actions performing shopping cart calculation.
    /// </summary>
    public sealed class ShoppingCartCalculatorCollection : IShoppingCartCalculator
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly IEnumerable<IShoppingCartCalculator> mCalculators;

        private readonly ICalculatorLoggingService mLoggingService = Service.Resolve<ICalculatorLoggingService>();


        /// <summary>
        /// Instantiates a new <see cref="ShoppingCartCalculatorCollection"/> with given particular calculators.
        /// </summary>
        public ShoppingCartCalculatorCollection(IEnumerable<IShoppingCartCalculator> calculators)
        {
            if (calculators == null)
            {
                throw new ArgumentNullException(nameof(calculators));
            }

            mCalculators = calculators.ToArray();
        }


        /// <summary>
        /// Runs shopping cart calculation on given data.
        /// </summary>
        /// <param name="calculationData">All calculation related data.</param>
        public void Calculate(CalculatorData calculationData)
        {
            if (calculationData == null)
            {
                throw new ArgumentNullException(nameof(calculationData));
            }

            foreach (var step in mCalculators)
            {
                step.Calculate(calculationData);

                mLoggingService.Log(calculationData);
            }
        }
    }
}
