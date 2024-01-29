using System.Globalization;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.UIControls;

namespace CMS.ModuleLicenses.Web.UI
{
    /// <summary>
    /// Extender for displaying list of module license data.
    /// </summary>
    public class ModuleLicenseKeyListExtender : ControlExtender<UniGrid>
    {
        /// <summary>
        /// Register method for customizing list of module licenses.
        /// </summary>
        public override void OnInit()
        {
            Control.OnExternalDataBound += Control_OnExternalDatabound;
        }


        private object Control_OnExternalDatabound(object sender, string sourceName, object parameter)
        {
            // Encode license data into HTML and hide signature in list of module licenses
            if (sourceName == "license")
            {
                string license = ValidationHelper.GetString(parameter, "", CultureInfo.InvariantCulture);
                if (!string.IsNullOrWhiteSpace(license))
                {
                    // Trim license signature and show only license data
                    string licenseData = "";
                    string signature = "";
                    if (ModuleLicensesHelper.ParseModuleLicense(license, out licenseData, out signature))
                    {
                        license = licenseData;
                    }

                    license = HTMLHelper.HTMLEncode(license);

                    return license;
                }
            }

            return parameter;
        }
    }
}