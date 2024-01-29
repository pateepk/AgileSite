namespace CMS.Helpers
{
    /// <summary>
    /// Cookie settings
    /// </summary>
    public class CookieSettings
    {
        /// <summary>
        /// Cookie level
        /// </summary>
        public int Level 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Default value
        /// </summary>
        public string DefaultValue 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="level">Cookie level</param>
        /// <param name="defaultValue">Default value</param>
        public CookieSettings(int level, string defaultValue)
        {
            Level = level;
            DefaultValue = defaultValue;
        }
    }
}
