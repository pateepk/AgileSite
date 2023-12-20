using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Operations which are checked for flooding.
    /// </summary>
    public enum FloodOperationEnum
    {
        /// <summary>
        /// Post message operation
        /// </summary>
        PostMessage,

        /// <summary>
        /// Create room operation.
        /// </summary>
        CreateRoom,

        /// <summary>
        /// Join room operation.
        /// </summary>
        JoinRoom,

        /// <summary>
        /// Change nickname operation.
        /// </summary>
        ChangeNickname,
    }
}
