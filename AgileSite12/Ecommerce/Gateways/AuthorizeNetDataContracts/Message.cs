using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Message.
    /// </summary>
    [DataContract(Name = "message", Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class Message
    {
        /// <summary>
        /// Message code.
        /// </summary>
        [DataMember(Name = "code", EmitDefaultValue = false)]
        public string Code
        {
            get;
            set;
        }


        /// <summary>
        /// Message description.
        /// </summary>
        [DataMember(Name = "description", EmitDefaultValue = false)]
        public string Description
        {
            get;
            set;
        }
    }
}