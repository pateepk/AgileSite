using System;
using System.Collections.Generic;

using CMS;
using CMS.Core;
using CMS.Core.Internal;
using CMS.Ecommerce;
using CMS.Helpers;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(CustomerMethods), typeof(CustomerInfo))]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Macro methods for <see cref="CustomerInfo"/>.
    /// </summary>
    internal sealed class CustomerMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns true when customer has been registered within the last number of days.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true when customer has been registered within the last number of days.", 2)]
        [MacroMethodParam(0, "customer", typeof(CustomerInfo), "Customer")]
        [MacroMethodParam(1, "numberOfDays", typeof(int), "Number of days")]
        public static object IsCustomerRegisteredWithin(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return IsCustomerRegisteredWithin(parameters);

                default:
                    throw new NotSupportedException();
            }
        }


        private static object IsCustomerRegisteredWithin(IReadOnlyList<object> parameters)
        {
            var customer = parameters[0] as CustomerInfo;
            var numberOfDays = ValidationHelper.GetInteger(parameters[1], 0);

            if (customer == null || numberOfDays <= 0)
            {
                return false;
            }

            if (!customer.CustomerIsRegistered && customer.Orders.Count == 0)
            {
                return ECommerceHelper.IsCustomerRegisteredAfterCheckout(customer);
            }

            var date = GetCreationDate(customer);
            var days = (Service.Resolve<IDateTimeNowService>().GetDateTimeNow() - date).TotalDays;
            return days >= 0 && days <= numberOfDays;
        }


        private static DateTime GetCreationDate(CustomerInfo customer)
        {
            var date = customer.CustomerCreated;

            if (customer.CustomerUser != null && customer.CustomerUser.UserCreated > date)
            {
                date = customer.CustomerUser.UserCreated;
            }

            return date;
        }
    }
}
