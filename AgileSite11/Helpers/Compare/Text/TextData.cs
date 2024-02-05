namespace CMS.Helpers
{
    /// <summary>
    /// Class for storing Text data for comparison.
    /// </summary>
    public class TextData
    {
        #region "Variables"

        // String text part
        private string mText;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Relevant string value.
        /// </summary>
        public string Text
        {
            get
            {
                return mText;
            }
        }


        /// <summary>
        /// Length of stored string value.
        /// </summary>
        public int Length
        {
            get
            {
                return Text.Length;
            }
        }

        #endregion


        #region "Operators"

        /// <summary>
        /// Index operation specification.
        /// </summary>
        /// <param name="index">Index to character</param>
        /// <returns>Particular character on specified index</returns>
        public char this[int index]
        {
            get
            {
                return Text[index];
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="str">Relevant string value</param>
        public TextData(string str)
        {
            mText = str;
        }

        #endregion
    }
}