using System;
using System.Linq;
using System.Text;

namespace CMS.SharePoint
{
    /// <summary>
    /// Specifies common SharePoint service properties.
    /// </summary>
    public interface ISharePointService
    {
        /// <summary>
        /// Tells you whether connection parameters can be used by instantiated SharePoint service implementation (eg. the authentication mode might not be supported).
        /// </summary>
        /// <returns>True if connection parameters can be used by current implementation, false otherwise.</returns>
        bool IsConnectionSupported();
    }
}
