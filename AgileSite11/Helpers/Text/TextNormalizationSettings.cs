namespace CMS.Helpers
{
    /// <summary>
    /// Normalization settings for text comparison
    /// </summary>
    public class TextNormalizationSettings
    {
        private bool mNormalizeLineEndings = true;
        private bool mNormalizeWhiteSpaces = true;


        /// <summary>
        /// If true, line endings in the given text are normalized
        /// </summary>
        public bool NormalizeLineEndings
        {
            get
            {
                return mNormalizeLineEndings;
            }
            set
            {
                mNormalizeLineEndings = value;
            }
        }


        /// <summary>
        /// If true, all whitespaces in the given text are normalized
        /// </summary>
        public bool NormalizeWhiteSpaces
        {
            get
            {
                return mNormalizeWhiteSpaces;
            }
            set
            {
                mNormalizeWhiteSpaces = value;
            }
        }
    }
}
