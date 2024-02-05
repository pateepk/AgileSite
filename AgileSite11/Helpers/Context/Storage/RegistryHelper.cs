using System;

using Microsoft.Win32;

namespace CMS.Helpers
{
    /// <summary>
    /// Registry access methods.
    /// </summary>
    public static class RegistryHelper
    {
        /// <summary>
        /// Gets the string from registry value.
        /// </summary>
        /// <param name="baseKey">Base key, such as Registry.LocalMachine</param>
        /// <param name="path">Path to the key</param>
        /// <param name="valueName">Value name</param>
        public static string GetStringValue(RegistryKey baseKey, string path, string valueName)
        {
            string result = null;

            // Open the path
            using (RegistryKey regKey = OpenRegistryKey(baseKey, path, false))
            {
                if (regKey != null)
                {
                    // Get the value
                    result = Convert.ToString(regKey.GetValue(valueName));
                    if (result == null)
                    {
                        result = "";
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Sets the string in a registry value.
        /// </summary>
        /// <param name="baseKey">Base key, such as Registry.LocalMachine</param>
        /// <param name="path">Path to the key</param>
        /// <param name="valueName">Value name</param>
        /// <param name="newValue">New value</param>
        public static bool SetStringValue(RegistryKey baseKey, string path, string valueName, string newValue)
        {
            // Open the path
            using (RegistryKey regKey = OpenRegistryKey(baseKey, path, true))
            {
                if (regKey != null)
                {
                    // Set the value
                    regKey.SetValue(valueName, newValue);

                    return true;
                }
                return false;
            }
        }


        /// <summary>
        /// Opens the registry key.
        /// </summary>
        /// <param name="baseKey">Base key, such as Registry.LocalMachine</param>
        /// <param name="path">Path to the key</param>
        /// <param name="write">Write to the registry key</param>
        public static RegistryKey OpenRegistryKey(RegistryKey baseKey, string path, bool write)
        {
            // Open the path
            RegistryKey regKey = baseKey;
            if (regKey != null)
            {
                string[] parts = path.Trim('/').Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                {
                    // Open next level
                    string subKey = parts[i];
                    if (write)
                    {
                        regKey = regKey.CreateSubKey(subKey);
                    }
                    else
                    {
                        regKey = regKey.OpenSubKey(subKey);
                    }

                    if (regKey == null)
                    {
                        return null;
                    }
                }

                return regKey;
            }

            return null;
        }
    }
}