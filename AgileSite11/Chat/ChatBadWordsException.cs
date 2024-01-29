using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Exception which is thrown when some user submitted text contains bad words
    /// </summary>
    public class ChatBadWordsException : ChatServiceException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ChatBadWordsException() : base(ChatResponseStatusEnum.BadWordsValidationFailed)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public ChatBadWordsException(string message)
            : base(ChatResponseStatusEnum.BadWordsValidationFailed, message)
        {
        }
    }
}
