using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents the settings key name.
    /// </summary>
    public class SettingsKeyName
    {
        /// <summary>
        /// Key name
        /// </summary>
        public readonly string KeyName;


        /// <summary>
        /// Site name
        /// </summary>
        public readonly string SiteName;


        /// <summary>
        /// Gets a value that indicates if the settings key name is global (i.e. site name is not specified).
        /// </summary>
        public bool IsGlobal
        {
            get
            {
                return string.IsNullOrEmpty(SiteName);
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyName">Key name in format "[site name].[key name]" for site setting or "[key name]" for global setting</param>
        public SettingsKeyName(string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
            {
                return;
            }

            int dotIndex = keyName.LastIndexOfCSafe(".");
            if (dotIndex >= 0)
            {
                SiteName = keyName.Remove(dotIndex);
                KeyName = keyName.Substring(dotIndex + 1);
            }
            else
            {
                SiteName = "";
                KeyName = keyName;
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyName">Key name in format "[key name]"</param>
        /// <param name="siteIdentifier">Site identifier</param>
        public SettingsKeyName(string keyName, SiteInfoIdentifier siteIdentifier)
        {
            if (string.IsNullOrEmpty(keyName))
            {
                return;
            }

            KeyName = keyName;
            SiteName = siteIdentifier;
        }


        /// <summary>
        /// Returns the fully qualified key name in format "[site name].[key name]" or "[key name]" for global setting.
        /// </summary>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(SiteName))
            {
                return KeyName;
            }
            return string.Format("{0}.{1}", SiteName, KeyName);
        }


        /// <summary>
        /// Implicitly converts the specified SettingsKeyName string representation to a SettingsKeyName instance.
        /// </summary>
        /// <param name="keyName">Settings key name string representation</param>
        public static implicit operator SettingsKeyName(string keyName)
        {
            return new SettingsKeyName(keyName);
        }


        /// <summary>
        /// Implicitly converts the specified SettingsKeyName instance to it's string representation.
        /// </summary>
        /// <param name="settingsKeyName">Settings key name</param>
        public static implicit operator string(SettingsKeyName settingsKeyName)
        {
            return settingsKeyName.ToString();
        }
    }
}