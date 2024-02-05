using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

using SystemIO = System.IO;

namespace CMS.SalesForce
{

    /// <summary>
    /// Contains the credentials used to authenticate SalesForce organization access.
    /// </summary>
    [DataContract]
    public sealed class OrganizationCredentials
    {

        #region "Public properties"

        /// <summary>
        /// Gets or sets the string representing the OAuth refresh token obtained from SalesForce.
        /// </summary>
        [DataMember]
        public string RefreshToken { get; set; }
        
        /// <summary>
        /// Gets or sets the consumer identifier associated with the remote access application in SalesForce.
        /// </summary>
        [DataMember]
        public string ClientId { get; set; }
        
        /// <summary>
        /// Gets or sets the consumer secret associated with the remote access application in SalesForce.
        /// </summary>
        [DataMember]
        public string ClientSecret { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the SalesForce user who authorized the remote access. This information is optional.
        /// </summary>
        [DataMember]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the address of the server hosting the SalesForce organization. This information is optional.
        /// </summary>
        [DataMember]
        public string OrganizationBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the SalesForce organization name. This information is optional.
        /// </summary>
        [DataMember]
        public string OrganizationName { get; set; }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Serializes an instance of the OrganizationCredentials class into a string.
        /// </summary>
        /// <param name="instance">An instance of the OrganizationCredentials class.</param>
        /// <returns>A string representation of the specified instance.</returns>
        public static string Serialize(OrganizationCredentials instance)
        {
            var stream = new SystemIO.MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(OrganizationCredentials));
            serializer.WriteObject(stream, instance);

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        /// <summary>
        /// Reads the specified string representation and returns the deserialized instance of the OrganizationCredentials class.
        /// </summary>
        /// <param name="content">A string representation of the instance of the OrganizationCredentials class.</param>
        /// <returns>An instance of the OrganizationCredentials class deserialized from the specified string representation.</returns>
        public static OrganizationCredentials Deserialize(string content)
        {
            var stream = new SystemIO.MemoryStream(Encoding.UTF8.GetBytes(content));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(OrganizationCredentials));

            return (OrganizationCredentials)serializer.ReadObject(stream);
        }

        #endregion

    }

}