using System;
using System.Linq;

using CMS.Core;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Calculation step providing calculation of the other payments such as gift cards. 
    /// </summary>
    public class OtherPaymentsCalculator : IShoppingCartCalculator
    {
        /// <summary>
        /// Evaluates other payments applications.
        /// </summary>
        /// <param name="calculationData">Cart calculation data</param>
        public void Calculate(CalculatorData calculationData)
        {
            var result = calculationData.Result;

            var otherPayments = Service.Resolve<IGiftCardSource>()
                .GetGiftCards(calculationData, result.Total)
                .ToList();

            result.OtherPayments = Math.Min(otherPayments.Sum(p => p.PaymentValue), result.Total);

            otherPayments.ForEach(payment =>
            {
                result.OtherPaymentsApplications.Sum(payment.PaymentName, payment.PaymentValue);

                result.AppliedCouponCodes.Add(new CouponCode(payment.AppliedCode, CouponCodeApplicationStatusEnum.AppliedInCart, payment, payment.PaymentValueInMainCurrency));

                if (payment.PaymentCorrectionInMainCurrency != 0)
                {
                    result.AppliedCouponCodes.Add(new CouponCode(payment.AppliedCode, CouponCodeApplicationStatusEnum.GiftCardCorrection, payment, payment.PaymentCorrectionInMainCurrency));
                }
            });
        }
    }
}
