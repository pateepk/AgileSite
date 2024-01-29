using System;

namespace CMS
{
    /// <summary>
    /// Marks the automatically generated classes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class GeneratedFromAttribute : Attribute
    {
        /// <summary>
        /// Path to the file within the solution from which the marked class has been generated.
        /// </summary>
        public string SourceFile
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">Path to the file within the solution from which the marked class has been generated.</param>
        public GeneratedFromAttribute(string filePath)
        {
            SourceFile = filePath;
        }
    }
}
