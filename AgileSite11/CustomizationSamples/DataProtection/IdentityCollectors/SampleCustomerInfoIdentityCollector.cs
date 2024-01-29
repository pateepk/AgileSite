using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.DataProtection;
using CMS.Ecommerce;

namespace DataProtection
{
    /// <summary>
    /// Sample implementation of <see cref="IIdentityCollector"/> for collecting <see cref="CustomerInfo"/>s by an email address.
    /// </summary>
    public class SampleCustomerInfoIdentityCollector : IIdentityCollector
    {
        /// <summary>
        /// Collects all the <see cref="CustomerInfo"/>s and adds them to the <paramref name="identities"/> collection.
        /// </summary>
        /// <remarks>
        /// Customers are collected by their email address.
        /// Duplicate customers are not added.
        /// </remarks>
        /// <param name="dataSubjectIdentifiersFilter">Key value collection containing data subject's information that identifies it.</param>
        /// <param name="identities">List of already collected identities.</param>
        public void Collect(IDictionary<string, object> dataSubjectIdentifiersFilter, List<BaseInfo> identities)
        {
            if (!dataSubjectIdentifiersFilter.ContainsKey("email"))
            {
                return;
            }

            var email = dataSubjectIdentifiersFilter["email"] as string;
            if (String.IsNullOrWhiteSpace(email))
            {
                return;
            }

            // Find customers that used the same email and distinct them
            var customers = CustomerInfoProvider.GetCustomers()
                                                .WhereEquals(nameof(CustomerInfo.CustomerEmail), email)
                                                .ToList();

            identities.AddRange(customers);
        }
    }
}
