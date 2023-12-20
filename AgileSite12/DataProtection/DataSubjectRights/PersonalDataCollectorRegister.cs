using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.DataProtection
{
    /// <summary>
    /// Contains personal data collectors.
    /// </summary>
    /// <seealso cref="IPersonalDataCollector"/>
    public class PersonalDataCollectorRegister
    {
        private readonly ConcurrentQueue<IPersonalDataCollector> mRegister = new ConcurrentQueue<IPersonalDataCollector>();
        private static readonly Lazy<PersonalDataCollectorRegister> mInstance = new Lazy<PersonalDataCollectorRegister>(() => new PersonalDataCollectorRegister());


        /// <summary>
        /// Initializes the register.
        /// </summary>
        internal PersonalDataCollectorRegister() { }


        /// <summary>
        /// Gets the <see cref="PersonalDataCollectorRegister"/> instance.
        /// </summary>
        public static PersonalDataCollectorRegister Instance => mInstance.Value;
        

        /// <summary>
        /// Gets the number of registered <see cref="IPersonalDataCollector"/>s in the register.
        /// </summary>
        public int Count => mRegister.Count;


        /// <summary>
        /// Adds personal data collector to the register.
        /// </summary>
        /// <param name="personalDataCollector">Instance of <see cref="IPersonalDataCollector"/> responsible for collecting personal data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="personalDataCollector"/> is null.</exception>
        public void Add(IPersonalDataCollector personalDataCollector)
        {
            if (personalDataCollector == null)
            {
                throw new ArgumentNullException(nameof(personalDataCollector));
            }

            mRegister.Enqueue(personalDataCollector);
        }


        /// <summary>
        /// Collects all the personal data by invoking registered collectors and passing them the <paramref name="identities"/>.
        /// </summary>
        /// <param name="identities">Collection of identities representing a data subject.</param>
        /// <param name="outputFormat">Defines an output format for the result.</param>
        /// <returns>Returns results of each data collector in an order they were registered.</returns>
        /// <exception cref="LicenseException">Thrown if insufficient license found.</exception>
        /// <seealso cref="IdentityCollectorRegister.CollectIdentities(IDictionary{string, object})"/>
        /// <seealso cref="IPersonalDataCollector.Collect(IEnumerable{BaseInfo}, string)"/>
        public IEnumerable<PersonalDataCollectorResult> CollectData(IEnumerable<BaseInfo> identities, string outputFormat)
        {
            if (identities == null)
            {
                throw new ArgumentNullException(nameof(identities));
            }

            InternalLicenseHelper.ThrowIfInsufficientLicense();

            return mRegister.Select(c => c.Collect(identities, outputFormat));
        }
    }
}
