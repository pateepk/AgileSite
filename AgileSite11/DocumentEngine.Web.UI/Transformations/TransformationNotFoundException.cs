using System;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Exception which is thrown when a transformation is not found
    /// </summary>
    [Serializable]
    public class TransformationNotFoundException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="TransformationNotFoundException" />
        /// </summary>
        /// <param name="message">Exception message</param>
        public TransformationNotFoundException(string message) 
            : base(message)
        {
        }        
    }
}