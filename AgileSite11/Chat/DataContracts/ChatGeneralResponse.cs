using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Chat general response without payload with data.
    /// </summary>
    [DataContract]
    public class ChatGeneralResponse
    {
        /// <summary>
        /// Status code.
        /// </summary>
        [DataMember]
        public ChatResponseStatusEnum StatusCode { get; set; }

        
        /// <summary>
        /// Message.
        /// </summary>
        [DataMember]
        public string StatusMessage { get; set; }
    }


    /// <summary>
    /// Chat general response with payload.
    /// </summary>
    /// <typeparam name="TData">Type of payload.</typeparam>
    [DataContract]
    public class ChatGeneralResponse<TData> : ChatGeneralResponse
    {
        /// <summary>
        /// Payload.
        /// </summary>
        [DataMember]
        public TData Data { get; set; }
    }
}
