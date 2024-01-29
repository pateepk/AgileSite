namespace CMS.Helpers
{
    /// <summary>
    /// Encapsulates the site name but doesn't request it until it is demanded by Value.
    /// </summary>
    public class PreferredCultureOnDemand
    {
        private string mValue = null;


        /// <summary>
        /// Value.
        /// </summary>
        public string Value
        {
            get
            {
                // Get site name from context
                if (mValue == null)
                {
                    mValue = CultureHelper.GetPreferredCulture();
                }

                return mValue;
            }
            set
            {
                mValue = value;
            }
        }
    }
}