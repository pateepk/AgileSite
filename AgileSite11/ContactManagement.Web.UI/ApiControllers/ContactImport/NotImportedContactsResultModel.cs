using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Refers to data which were not imported. Result is same as <see cref="RequestModel"/> and contains explanation message for each failed row.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class NotImportedContactsResultModel
    {
        /// <summary>
        /// Names of the contact fields which shall be imported. This list specifies meaning of fields values in <see cref="FieldValues"/> property. 
        /// </summary>
        /// <example>
        /// If FieldsOrder[0] is "ContactEmail", than first item in each row in <see cref="FieldValues"/> property represents contact email.
        /// </example>
        [JsonProperty("fieldsOrder")]
        public List<string> FieldsOrder
        {
            get;
            set;
        }


        /// <summary>
        /// Values of contact fields in order specified by <see cref="FieldsOrder"/> property.
        /// </summary>
        [JsonProperty("fieldValues")]
        public List<List<string>> FieldValues
        {
            get;
            set;
        }


        /// <summary>
        /// Each message is trying to explain why was corresponding row in <see cref="FieldValues"/> rejected.
        /// </summary>
        [JsonProperty("messages")]
        public List<string> Messages
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public NotImportedContactsResultModel()
        {
            Messages = new List<string>();
            FieldsOrder = new List<string>();
            FieldValues = new List<List<string>>();
        }
    }
}