using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Piece of main information.
    /// </summary>
    [DataContract(Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class MainMessage
    {
        /// <summary>
        /// Messasge code.
        /// </summary>
        [DataMember(Name = "code", Order = 0, IsRequired = true)]
        public string Code
        {
            get;
            set;
        }


        /// <summary>
        /// Message text.
        /// </summary>
        [DataMember(Name = "text", Order = 1, IsRequired = true)]
        public string Text
        {
            get;
            set;
        }
    }
}
