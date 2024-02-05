using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using CMS.Core;
using CMS.DataEngine;

namespace CMS.DataProtection
{
    /// <summary>
    /// Contains identity collectors.
    /// </summary>
    /// <seealso cref="IIdentityCollector"/>
    public class IdentityCollectorRegister
    {
        private readonly ConcurrentQueue<IIdentityCollector> mRegister = new ConcurrentQueue<IIdentityCollector>();
        private static readonly Lazy<IdentityCollectorRegister> mInstance = new Lazy<IdentityCollectorRegister>(() => new IdentityCollectorRegister());


        /// <summary>
        /// Initializes the register.
        /// </summary>
        internal IdentityCollectorRegister() { }


        /// <summary>
        /// Gets the <see cref="IdentityCollectorRegister"/> instance.
        /// </summary>
        public static IdentityCollectorRegister Instance => mInstance.Value;


        /// <summary>
        /// Gets the number of registered <see cref="IIdentityCollector"/>s in the register.
        /// </summary>
        public int Count => mRegister.Count;
        

        /// <summary>
        /// Adds identity collector to the register.
        /// </summary>
        /// <param name="identitiesCollector">Instance of <see cref="IIdentityCollector"/> responsible for collecting data subject's identities.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="identitiesCollector"/> is null.</exception>
        public void Add(IIdentityCollector identitiesCollector)
        {
            if (identitiesCollector == null)
            {
                throw new ArgumentNullException(nameof(identitiesCollector));
            }

            mRegister.Enqueue(identitiesCollector);
        }


        /// <summary>
        /// Collects all the data subject's identities by invoking registered collectors and passing them the <paramref name="dataSubjectIdentifiersFilter"/>.
        /// </summary>
        /// <param name="dataSubjectIdentifiersFilter">Key value collection containing data subject's information that identifies it.</param>
        /// <returns>Collection of data subject's identities.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataSubjectIdentifiersFilter"/> is null.</exception>
        /// <exception cref="LicenseException">Thrown if insufficient license found.</exception>
        /// <seealso cref="IIdentityCollector.Collect(IDictionary{string, object}, List{BaseInfo})"/>
        public IEnumerable<BaseInfo> CollectIdentities(IDictionary<string, object> dataSubjectIdentifiersFilter)
        {
            if (dataSubjectIdentifiersFilter == null)
            {
                throw new ArgumentNullException(nameof(dataSubjectIdentifiersFilter));
            }

            InternalLicenseHelper.ThrowIfInsufficientLicense();

            var identities = new List<BaseInfo>();
            foreach (var item in mRegister)
            {
                item.Collect(dataSubjectIdentifiersFilter, identities);
            }

            return identities;
        }
    }
}