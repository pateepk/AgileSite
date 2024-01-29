using System;
using System.Linq;
using System.Text;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Enumeration used for web part actions (Up/Down etc...)
    /// </summary>
    internal enum WebPartActionEnum
    {
        /// <summary>
        /// Up
        /// </summary>
        Up = 0,
        
        /// <summary>
        /// Down
        /// </summary>
        Down = 1,
        
        /// <summary>
        /// Top
        /// </summary>
        Top = 2,
        
        /// <summary>
        /// Bottom
        /// </summary>
        Bottom = 3,
        
        /// <summary>
        /// Remove
        /// </summary>
        Remove = 4
    }
}
