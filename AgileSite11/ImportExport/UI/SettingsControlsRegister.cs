using System;
using System.Linq;
using System.Text;

using CMS.Base;


namespace CMS.CMSImportExport
{
    /// <summary>
    /// Serves as a base register for <see cref="ExportSettingsControlsRegister"/> and <see cref="ImportSettingsControlsRegister"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    internal static class SettingsControlsRegister
    {
        /// <summary>
        /// Registers settings control for given <paramref name="objectType"/> in <paramref name="register"/>. The control is loaded when importing or exporting objects of specified type
        /// using the UI wizard.
        /// </summary>
        /// <param name="objectType">Object type for which the control is designed.</param>
        /// <param name="controlVirtualPath">Virtual path of the control.</param>
        /// <param name="register">Register for virtual paths.</param>
        /// <exception cref="ArgumentException">Thrown when any of the parameters is null or empty string.</exception>
        public static void RegisterSettingsControl(string objectType, string controlVirtualPath, SafeDictionary<string, string> register)
        {
            if (String.IsNullOrEmpty(objectType))
            {
                throw new ArgumentException("Object type can not be null or empty string.", "objectType");
            }
            if (String.IsNullOrEmpty(controlVirtualPath))
            {
                throw new ArgumentException("Virtual path of the control can not be null or empty string.", "controlVirtualPath");
            }

            register.Add(objectType, controlVirtualPath);
        }


        /// <summary>
        /// Gets settings control registered for <paramref name="objectType"/> in <paramref name="register"/>.
        /// </summary>
        /// <param name="objectType">Object type for which the control is designed.</param>
        /// <param name="register">Register for virtual paths.</param>
        /// <returns></returns>
        public static string GetSettingsControl(string objectType, SafeDictionary<string, string> register)
        {
            if (String.IsNullOrEmpty(objectType))
            {
                throw new ArgumentException("Object type can not be null or empty string.", "objectType");
            }

            return register[objectType];
        }
    }
}
