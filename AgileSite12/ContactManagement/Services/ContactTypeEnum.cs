using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Specifies possible types of contacts. This value determines which component will be used for displaying contact detail.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContactTypeEnum
    {
        /// <summary>
        /// Determines the contact is available in basic license versions.
        /// </summary>
        Simple,


        /// <summary>
        /// Determines the contact is available in all license versions.
        /// </summary>
        Full
    }
}