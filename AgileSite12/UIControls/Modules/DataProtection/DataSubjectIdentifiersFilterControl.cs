using System.Collections.Generic;

using CMS.Base.Web.UI;
using CMS.DataProtection;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for filter control for specification of data subject identifiers.
    /// </summary>
    public abstract class DataSubjectIdentifiersFilterControl : CMSUserControl
    {
        /// <summary>
        /// Gets filter for <see cref="IIdentityCollector"/>.
        /// </summary>
        /// <param name="filter">Initialized filter dictionary.</param>
        /// <returns>Filter dictionary populated by values from the filter control.</returns>
        public abstract IDictionary<string, object> GetFilter(IDictionary<string, object> filter);


        /// <summary>
        /// Returns <c>true</c> if entered data is valid. If data is invalid, it returns <c>false</c> and displays an error message.
        /// </summary>
        /// <seealso cref="AbstractUserControl.AddError(string, string)"/>
        public abstract bool IsValid();
    }
}
