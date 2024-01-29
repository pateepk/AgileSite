using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Macro event arguments
    /// </summary>
    public class MacroEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Sender (active macro resolver)
        /// </summary>
        public MacroResolver Resolver
        {
            get;
            set;
        }


        /// <summary>
        /// Expression to resolve
        /// </summary>
        public string Expression
        {
            get;
            set;
        }


        /// <summary>
        /// Full expression including parameters
        /// </summary>
        public string FullExpression
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the macro matches (was resolved)
        /// </summary>
        public bool Match
        {
            get;
            set;
        }


        /// <summary>
        /// Result of the macro
        /// </summary>
        public object Result
        {
            get;
            set;
        }
    }
}