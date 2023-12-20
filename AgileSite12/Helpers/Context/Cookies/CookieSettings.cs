namespace CMS.Helpers
{
    /// <summary>
    /// Cookie settings
    /// </summary>
    public class CookieSettings
    {
        /// <summary>
        /// Gets the cookie level.
        /// </summary>
        public int Level 
        { 
            get; 
        }


        /// <summary>
        /// Gets the cookie default value.
        /// </summary>
        public string DefaultValue 
        { 
            get; 
        }


        /// <summary>
        /// Gets the cookie sensitivity from security point of view.
        /// </summary>
        public bool IsSensitive
        {
            get;
        }


        /// <summary>
        /// Creates a new instance of <see cref="CookieSettings"/> with given parameters.
        /// </summary>
        /// <param name="level">Cookie level</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="isSensitive">Cookie sensitivity</param>
        public CookieSettings(int level, string defaultValue, bool isSensitive = false)
        {
            Level = level;
            DefaultValue = defaultValue;
            IsSensitive = isSensitive;
        }
    }
}
