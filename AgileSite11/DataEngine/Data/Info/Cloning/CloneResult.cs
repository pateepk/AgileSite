using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class encapsulating cloning result messages.
    /// </summary>
    public sealed class CloneResult
    {
        /// <summary>
        /// Collection of warning messages.
        /// </summary>
        public ICollection<string> Warnings { get; } = new List<string>();


        /// <summary>
        /// Collection of error messages.
        /// </summary>
        public ICollection<string> Errors { get; } = new List<string>();
    }
}