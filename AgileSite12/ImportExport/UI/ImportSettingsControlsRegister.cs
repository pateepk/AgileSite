using System;
using System.Linq;
using System.Text;

using CMS.Base;


namespace CMS.CMSImportExport
{
    /// <summary>
    /// Registers settings controls for import process. The settings controls allow for extending the import UI for particular object types.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public static class ImportSettingsControlsRegister
    {
        #region "Fields"

        private static readonly SafeDictionary<string, string> mSettingsControls = new SafeDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        private static readonly SafeDictionary<string, string> mSiteSettingsControls = new SafeDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        #endregion


        #region "Public methods"

        /// <summary>
        /// Registers settings control for given <paramref name="objectType"/>. The control is loaded when importing objects of specified type
        /// using the UI wizard.
        /// </summary>
        /// <param name="objectType">Object type for which the control is designed.</param>
        /// <param name="controlVirtualPath">Virtual path of the control.</param>
        /// <exception cref="ArgumentException">Thrown when any of the parameters is null or empty string.</exception>
        /// <seealso cref="RegisterSiteSettingsControl"/>
        public static void RegisterSettingsControl(string objectType, string controlVirtualPath)
        {
            SettingsControlsRegister.RegisterSettingsControl(objectType, controlVirtualPath, mSettingsControls);
        }


        /// <summary>
        /// Registers settings control for given site <paramref name="objectType"/>. The control is loaded when importing site objects of specified type
        /// using the UI wizard.
        /// </summary>
        /// <param name="objectType">Object type for which the control is designed.</param>
        /// <param name="controlVirtualPath">Virtual path of the control.</param>
        /// <exception cref="ArgumentException">Thrown when any of the parameters is null or empty string.</exception>
        /// <seealso cref="RegisterSettingsControl"/>
        public static void RegisterSiteSettingsControl(string objectType, string controlVirtualPath)
        {
            SettingsControlsRegister.RegisterSettingsControl(objectType, controlVirtualPath, mSiteSettingsControls);
        }


        /// <summary>
        /// Gets settings control registered for <paramref name="objectType"/>.
        /// </summary>
        /// <param name="objectType">Object type for which the control is designed.</param>
        /// <returns>Virtual path of the control, if any is registered, or null.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="objectType"/> is null or empty string.</exception>
        /// <seealso cref="GetSiteSettingsControl"/>
        public static string GetSettingsControl(string objectType)
        {
            return SettingsControlsRegister.GetSettingsControl(objectType, mSettingsControls);
        }


        /// <summary>
        /// Gets settings control registered for site <paramref name="objectType"/>.
        /// </summary>
        /// <param name="objectType">Object type for which the control is designed.</param>
        /// <returns>Virtual path of the control, if any is registered, or null.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="objectType"/> is null or empty string.</exception>
        /// <seealso cref="GetSettingsControl"/>
        public static string GetSiteSettingsControl(string objectType)
        {
            return SettingsControlsRegister.GetSettingsControl(objectType, mSiteSettingsControls);
        }

        #endregion
    }
}
