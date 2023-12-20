using System;
using System.Linq;
using System.Text;

namespace CMS.MessageBoards
{
    /// <summary>
    /// Type of message board according to owner.
    /// </summary>
    public enum BoardOwnerTypeEnum
    {
        /// <summary>
        /// Messageboard belongs to document.
        /// </summary>
        Document = 0,

        /// <summary>
        /// Messageboard belongs to user.
        /// </summary>
        User = 1,

        /// <summary>
        /// Messageboard belongs to group.
        /// </summary>
        Group = 2
    }
}
