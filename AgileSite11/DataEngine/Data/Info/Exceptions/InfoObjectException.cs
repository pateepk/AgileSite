using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Base class for the exceptions on the info objects.
    /// </summary>
    public class InfoObjectException : Exception
    {
        /// <summary>
        /// Object to which the exception relates.
        /// </summary>
        public GeneralizedInfo Object
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="obj">Object to which the exception relates</param>
        /// <param name="message">Message</param>
        public InfoObjectException(GeneralizedInfo obj, string message)
            : base(message)
        {
            Object = obj;
        }
    }
}