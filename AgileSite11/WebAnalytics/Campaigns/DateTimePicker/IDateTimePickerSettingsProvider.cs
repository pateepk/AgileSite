using System;
using System.Globalization;

using CMS.SiteProvider;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides dictionary containing all culture dependent settings needed for initialization of cmsdatepicker javascript component.
    /// </summary>
    public interface IDateTimePickerSettingsProvider
    {
        /// <summary>
        /// Get dictionary containing all culture dependent settings needed by cmsdatepicker component, as well as timezone dependent labels.
        /// </summary>
        /// <param name="culture">Culture for which the settings will be evaluated</param>
        /// <param name="currentSite">Reference to the executing site. This information is needed, because current time for the datepicker is retrieved based on the site time zone</param>
        /// <param name="currentDateTime">Current date time for the site. Should be already shifted by timezone, if necessary</param>
        /// <returns>Settings object suitable for serialization</returns>
        object GetDateTimePickerSettings(CultureInfo culture, SiteInfo currentSite, DateTime currentDateTime);
    }
}