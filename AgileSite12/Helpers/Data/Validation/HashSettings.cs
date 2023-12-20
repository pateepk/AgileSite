namespace CMS.Helpers
{
    /// <summary>
    /// Hash settings container
    /// </summary>
    public class HashSettings
    {
        /// <summary>
        /// Gets or sets value indicating whether or not should hash validation method redirect in case of invalid hash.
        /// </summary>
        public bool Redirect
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Gets or sets custom salt string. If not specified then default salt <see cref="ValidationHelper.HashStringSalt"/> is used. 
        /// </summary>
        public string HashStringSaltOverride
        {
            get;
            set;
        }


        /// <summary>
        /// Gets purpose of the hash.
        /// </summary>
        public string Purpose
        {
            get;
        }


        /// <summary>
        /// Creates new instance of <see cref="HashSettings"/> with hash <paramref name="purpose"/>.
        /// The same purpose must be used in <see cref="HashSettings"/> when validating hash.
        /// </summary>
        /// <param name="purpose">Purpose of the hash.</param>
        public HashSettings(string purpose)
        {
            Purpose = purpose;
        }


        /// <summary>
        /// Clones the settings.
        /// </summary>
        public HashSettings Clone()
        {
            return new HashSettings(Purpose)
            {
                Redirect = Redirect,
                HashStringSaltOverride = HashStringSaltOverride
            };
        }
    }
}