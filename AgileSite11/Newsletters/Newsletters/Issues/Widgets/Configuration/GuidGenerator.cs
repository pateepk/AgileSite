using System;

namespace CMS.Newsletters.Issues.Widgets.Configuration
{
    /// <summary>
    /// Generates a GUID identifier.
    /// </summary>
    internal class GuidGenerator
    {
        /// <summary>
        /// Generates new GUID.
        /// </summary>
        public virtual Guid Generate()
        {
            return Guid.NewGuid();
        }
    }
}
