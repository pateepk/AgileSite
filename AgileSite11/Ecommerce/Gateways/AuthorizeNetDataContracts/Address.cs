using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Information about address.
    /// </summary>
    [DataContract(Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public class Address
    {
        /// <summary>
        /// Personal name.
        /// </summary>
        [DataMember(Name = "firstName", Order = 0, EmitDefaultValue = false)]
        public string FirstName
        {
            get;
            set;
        }


        /// <summary>
        /// Address.
        /// </summary>
        [DataMember(Name = "address", Order = 1, EmitDefaultValue = false)]
        public string AddressLine
        {
            get;
            set;
        }


        /// <summary>
        /// City.
        /// </summary>
        [DataMember(Name = "city", Order = 2, EmitDefaultValue = false)]
        public string City
        {
            get;
            set;
        }


        /// <summary>
        /// State.
        /// </summary>
        [DataMember(Name = "state", Order = 3, EmitDefaultValue = false)]
        public string State
        {
            get;
            set;
        }


        /// <summary>
        /// ZIP.
        /// </summary>
        [DataMember(Name = "zip", Order = 4, EmitDefaultValue = false)]
        public string Zip
        {
            get;
            set;
        }


        /// <summary>
        /// Country.
        /// </summary>
        [DataMember(Name = "country", Order = 5, EmitDefaultValue = false)]
        public string Country
        {
            get;
            set;
        }
    }
}
