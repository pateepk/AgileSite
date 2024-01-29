using System.Collections.Generic;

using CMS.DataProtection;
using CMS.Base.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Represents base class for user control that gets the configuration which data is to be deleted by <see cref="IPersonalDataEraser"/>s.
    /// </summary>
    /// <seealso cref="PersonalDataEraserRegister"/>
    public abstract class ErasureConfigurationControl : CMSUserControl
    {
        /// <summary>
        /// Gets configuration for <see cref="IPersonalDataEraser"/>s.
        /// </summary>
        /// <param name="configuration">Initialized configuration dictionary.</param>
        /// <returns>Configuration dictionary for <see cref="IPersonalDataEraser"/>s.</returns>
        public abstract IDictionary<string, object> GetConfiguration(IDictionary<string, object> configuration);


        /// <summary>
        /// Returns <c>true</c> if entered data is valid. If data is invalid, it returns <c>false</c> and displays an error message.
        /// </summary>
        /// <seealso cref="AbstractUserControl.AddError(string, string)"/>
        public abstract bool IsValid();
    }
}