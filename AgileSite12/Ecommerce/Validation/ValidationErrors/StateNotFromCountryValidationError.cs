using System;

using CMS.Base;
using CMS.Globalization;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error resulting from assigning a state not belonging to a certain country.
    /// </summary>
    /// <seealso cref="ShoppingService"/>
    public class StateNotFromCountryValidationError : IValidationError
    {
        /// <summary>
        /// Country whose state is invalid.
        /// </summary>
        public CountryInfo Country { get; }


        /// <summary>
        /// State which is invalid for given <see cref="Country"/>.
        /// </summary>
        public StateInfo State { get; }


        /// <summary>
        /// Gets a key which can be used to retrieve a localized error message.
        /// </summary>
        public string MessageKey => "ecommerce.validation.statenotfromcountry";


        /// <summary>
        /// Returns an array containing country and state display names.
        /// </summary>
        public object[] MessageParameters => new string[] { Country.CountryDisplayName, State.StateDisplayName };


        /// <summary>
        /// Initializes a new instance of the <see cref="StateNotFromCountryValidationError"/> class.
        /// </summary>
        /// <param name="country">Country whose state is invalid.</param>
        /// <param name="state">State which is invalid for given <paramref name="country"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="country"/> or <paramref name="state"/> is null.</exception>
        public StateNotFromCountryValidationError(CountryInfo country, StateInfo state)
        {
            Country = country ?? throw new ArgumentNullException(nameof(country));
            State = state ?? throw new ArgumentNullException(nameof(state));
        }
    }
}