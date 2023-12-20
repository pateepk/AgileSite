using System;
using System.Linq;
using System.Text;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Data context for specific site, contains only site related items
    /// </summary>
    public class CMSSiteDataContext : CMSDataContextBase<CMSSiteDataContext>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CMSSiteDataContext()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public CMSSiteDataContext(string siteName)
            : base(siteName)
        {
        }
    }
}
