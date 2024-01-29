using System;
using System.Linq;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Localization;
using CMS.PortalEngine.Web.UI;

namespace CMS.ModuleLicenses.Web.UI
{
    /// <summary>
    /// Extender for validating that new module license is unique for given module.
    /// </summary>
    public class ModuleLicenseKeyExtender : ControlExtender<UIForm>
    {
        /// <summary>
        /// Register methods for editing and validating new module license.
        /// </summary>
        public override void OnInit()
        {
            Control.OnAfterValidate += Control_OnAfterValidate;
            Control.OnGetControlValue += Control_OnGetControlValue;
        }


        /// <summary>
        /// Trim possible whitespaces at the end of new license. Whitespaces at the beginning should not be trimmed, as they can be part of license data.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="formEngineUserControlEventArgs">Event arguments</param>
        private void Control_OnGetControlValue(object sender, FormEngineUserControlEventArgs formEngineUserControlEventArgs)
        {
            if (formEngineUserControlEventArgs.ColumnName.Equals("ModuleLicenseKeyLicense", StringComparison.OrdinalIgnoreCase))
            {
                formEngineUserControlEventArgs.Value = ValidationHelper.GetString(formEngineUserControlEventArgs.Value, String.Empty).TrimEnd();
            }
        }


        /// <summary>
        /// Check whether inserted module license is unique for given module.
        /// </summary>
        private void Control_OnAfterValidate(object sender, EventArgs eventArgs)
        {
            // Get license key data and module
            string newLicense = ValidationHelper.GetString(Control.GetFieldValue("ModuleLicenseKeyLicense"), String.Empty).TrimEnd();
            int moduleID = ValidationHelper.GetInteger(Control.GetFieldValue("ModuleLicenseKeyResourceID"), 0);
            int moduleLicenseKeyID = ValidationHelper.GetInteger(Control.GetFieldValue("ModuleLicenseKeyID"), 0);

            // Inserted license should be unique for given module
            if (ModuleLicenseKeyInfoProvider.GetResourceModuleLicenseKeyInfos(moduleID)
                .Where(l => l.ModuleLicenseKeyID != moduleLicenseKeyID)
                .Count(l => l.ModuleLicenseKeyLicense.Equals(newLicense, StringComparison.Ordinal)) > 0)
            {
                Control.StopProcessing = true;
                Control.ValidationErrorMessage = LocalizationHelper.GetString("modulelicenses.modulelicensekeyextender.notuniquelicense");
                Control.ShowValidationErrorMessage = true;
            }
        }
    }
}
