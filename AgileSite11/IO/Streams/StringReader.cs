namespace CMS.IO
{
    /// <summary>
    /// CMS.IO StringReader wrapper for System.IO.StringReader.
    /// </summary>
    public class StringReader : System.IO.StringReader
    {
        #region "Constructors"

        /// <summary>
        /// Creates new instance of string reader.
        /// </summary>
        /// <param name="s">Input string</param>        
        public static StringReader New(string s)
        {
            return new StringReader(s);
        }


        /// <summary>
        /// Creates new instance of string reader. 
        /// </summary>
        /// <param name="s">Input string</param>
        public StringReader(string s)
            : base(s)
        {
        }

        #endregion
    }
}