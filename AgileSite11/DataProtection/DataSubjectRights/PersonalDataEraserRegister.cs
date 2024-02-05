using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.DataProtection
{
    /// <summary>
    /// Contains personal data erasers.
    /// </summary>
    /// <seealso cref="IPersonalDataEraser"/>
    public class PersonalDataEraserRegister
    {
        private readonly ConcurrentQueue<IPersonalDataEraser> mRegister = new ConcurrentQueue<IPersonalDataEraser>();
        private static readonly Lazy<PersonalDataEraserRegister> mInstance = new Lazy<PersonalDataEraserRegister>(() => new PersonalDataEraserRegister());


        /// <summary>
        /// Initializes the register.
        /// </summary>
        internal PersonalDataEraserRegister() { }


        /// <summary>
        /// Gets the <see cref="PersonalDataEraserRegister"/> instance.
        /// </summary>
        public static PersonalDataEraserRegister Instance => mInstance.Value;


        /// <summary>
        /// Gets the number of registered <see cref="IPersonalDataEraser"/>s in the register.
        /// </summary>
        public int Count => mRegister.Count;


        /// <summary>
        /// Adds personal data eraser to the register.
        /// </summary>
        /// <param name="personalDataEraser">Instance of <see cref="IPersonalDataEraser"/> responsible for erasing personal data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="personalDataEraser"/> is null.</exception>
        public void Add(IPersonalDataEraser personalDataEraser)
        {
            if (personalDataEraser == null)
            {
                throw new ArgumentNullException(nameof(personalDataEraser));
            }

            mRegister.Enqueue(personalDataEraser);
        }


        /// <summary>
        /// Erases all personal data of data subject's <paramref name="identities"/> filtered by <paramref name="configuration"/> by invoking registered erasers.
        /// </summary>
        /// <param name="identities">Collection of identities representing a data subject.</param>
        /// <param name="configuration">Defines which personal data will be erased.</param>
        /// <seealso cref="IPersonalDataEraser.Erase(IEnumerable{BaseInfo}, IDictionary{string, object})"/>
        /// <exception cref="LicenseException">Thrown if insufficient license found.</exception>
        public void EraseData(IEnumerable<BaseInfo> identities, IDictionary<string, object> configuration)
        {
            if (identities == null)
            {
                throw new ArgumentNullException(nameof(identities));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            InternalLicenseHelper.ThrowIfInsufficientLicense();

            foreach (var eraser in mRegister)
            {
                eraser.Erase(identities, configuration);
            }
        }
    }
}