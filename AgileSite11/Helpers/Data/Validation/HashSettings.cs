namespace CMS.Helpers
{
    /// <summary>
    /// Hash settings container
    /// </summary>
    public class HashSettings
    {
        private bool mRedirect = true;
        

        /// <summary>
        /// Whether or not should method redirect.
        /// </summary>
        public bool Redirect
        {
            get
            {
                return mRedirect;
            }
            set
            {
                mRedirect = value;
            }
        }


        /// <summary>
        /// Custom salt string. If not specified then default salt <see cref="ValidationHelper.HashStringSalt"/> is used. 
        /// </summary>
        public string CustomSalt
        {
            get;
            set;
        }


        /// <summary>
        /// Additional hash salt for specific context.
        /// </summary>
        public string HashSalt
        {
            get;
            set;
        }


        /// <summary>
        /// Clones the settings
        /// </summary>
        public HashSettings Clone()
        {
            return new HashSettings
            {
                Redirect = Redirect,
                HashSalt = HashSalt,
                CustomSalt = CustomSalt
            };
        }
    }
}