using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Globalization;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class for customer address validation.
    /// </summary>
    public class CustomerAddressValidator : IValidator
    {
        private List<IValidationError> errors;


        /// <summary>
        /// Gets an enumeration of validation errors associated with this validator. An empty enumeration is returned if validation succeeded.
        /// </summary>
        /// <seealso cref="CountryNotSetValidationError"/>
        /// <seealso cref="StateNotFromCountryValidationError"/>
        public IEnumerable<IValidationError> Errors => errors ?? Enumerable.Empty<IValidationError>();


        /// <summary>
        /// Gets a value indicating whether validation succeeded.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="Validate"/> has not been called yet.</exception>
        public bool IsValid => errors != null ? errors.Count == 0 : throw new InvalidOperationException($"The {nameof(Validate)}() method must be called prior to accessing the {nameof(IsValid)} property.");



        /// <summary>
        /// Gets address this validator was initialized for.
        /// </summary>
        public AddressInfo Address
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerAddressValidator"/> class.
        /// </summary>
        /// <param name="address"><see cref="AddressInfo"/> object representing a customer address that is validated.</param>
        public CustomerAddressValidator(AddressInfo address)
        {
            Address = address;
        }


        /// <summary>
        /// Validates the address.
        /// </summary>
        /// <returns>Returns a value indicating whether validation succeeded.</returns>
        /// <seealso cref="CountryNotSetValidationError"/>
        /// <seealso cref="StateNotFromCountryValidationError"/>
        public bool Validate()
        {
            errors = new List<IValidationError>();

            if (Address.AddressCountryID == 0)
            {
                errors.Add(new CountryNotSetValidationError());
            }
            else
            {
                var state = StateInfoProvider.GetStateInfo(Address.AddressStateID);
                if ((state != null) && (state.CountryID != Address.AddressCountryID))
                {
                    errors.Add(new StateNotFromCountryValidationError(CountryInfoProvider.GetCountryInfo(Address.AddressCountryID), state));
                }
            }

            return IsValid;
        }
    }
}
