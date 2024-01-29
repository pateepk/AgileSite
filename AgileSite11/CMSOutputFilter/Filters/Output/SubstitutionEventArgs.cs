using System;

using CMS.Base;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Substitution event arguments
    /// </summary>
    public class SubstitutionEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Expression
        /// </summary>
        public string Expression
        {
            get;
            set;
        }


        /// <summary>
        /// Result of the substitution
        /// </summary>
        public string Result
        {
            get;
            set;
        }


        /// <summary>
        /// Returning true if the substitution was matched
        /// </summary>
        public bool Match
        {
            get;
            set;
        }
    }
}