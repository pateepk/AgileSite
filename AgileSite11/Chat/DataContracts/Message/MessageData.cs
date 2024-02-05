using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.Chat
{
    /// <summary>
    /// Chat message.
    /// </summary>
    [DataContract]
    public class MessageData : IChatIncrementalCacheable
    {
        #region "Private fields"

        private string messageText;

        #endregion


        #region "Data members"

        /// <summary>
        /// MessageID.
        /// </summary>
        [DataMember]
        public int MessageID { get; set; }


        /// <summary>
        /// Nickname of a sender.
        /// </summary>
        [DataMember]
        public string Nickname { get; set; }


        /// <summary>
        /// Nickname of a recipient.
        /// </summary>
        [DataMember]
        public string Recipient { get; set; }


        /// <summary>
        /// Posted datetime.
        /// </summary>
        [DataMember]
        public DateTime PostedTime { get; set; }


        /// <summary>
        /// Text of a message. It is autmatically resolved if it is system message.
        /// </summary>
        [DataMember]
        public string MessageText 
        {
            get
            {
                // Do not return text if message is rejected
                if (IsRejected)
                {
                    return "";
                }

                if (SystemMessageType.IsSystemMessage())
                {
                    return MacroResolver.Resolve(messageText);
                }
                return messageText;
            }
            set
            {
                messageText = value;
            }
        }


        /// <summary>
        /// Date time of last modification of this message.
        /// </summary>
        [DataMember]
        public DateTime LastModified { get; set; }


        /// <summary>
        /// Type of this message.
        /// </summary>
        [DataMember]
        public ChatMessageTypeEnum SystemMessageType { get; set; }


        /// <summary>
        /// True if message is rejected.
        /// </summary>
        [DataMember]
        public bool IsRejected { get; set; }


        /// <summary>
        /// Author's ID.
        /// </summary>
        [DataMember]
        public int? AuthorID { get; set; }


        /// <summary>
        /// Recepient of this message. Null if it is a system message.
        /// </summary>
        [DataMember]
        public int? RecipientID { get; set; }

        #endregion


        #region IChatIncrementalCacheable Members

        /// <summary>
        /// Change time of this object.
        /// </summary>
        public DateTime ChangeTime
        {
            get 
            { 
                return LastModified; 
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns string representation of this message
        /// </summary>
        public override string ToString()
        {
            string messageText = MessageText;

            return string.Format("ID: {0}, Author: {1}, Last modified: {2}, Is rejected: {3}, Text (first 5): {4}",
                MessageID,
                Nickname,
                LastModified,
                IsRejected,
                messageText.Substring(0, Math.Min(messageText.Length, 5)));
        }

        #endregion
    }
}
