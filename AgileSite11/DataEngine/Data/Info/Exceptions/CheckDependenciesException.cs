using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Exception to report attempt to delete an object which has required dependencies.
    /// </summary>
    public class CheckDependenciesException : InfoObjectException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="obj">Object to which the exception relates</param>
        public CheckDependenciesException(GeneralizedInfo obj)
            : this(obj, null)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="obj">Object to which the exception relates</param>
        /// <param name="message">Message</param>
        public CheckDependenciesException(GeneralizedInfo obj, string message)
            : base(obj, message)
        {
            
        }
    }
}