using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace CMS.Newsletters.Web.UI.Internal
{
    /// <summary>
    /// Represents view model for <see cref="CMS.Newsletters.EmailTemplateInfo"/>.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TemplateViewModel
    {
        /// <summary>
        /// Email template id (<see cref="EmailTemplateInfo.TemplateID"/>). 
        /// </summary>
        [Required]
        [JsonProperty("id")]
        public int Id
        {
            get;
            set;
        }


        /// <summary>
        /// Email template display name (<see cref="EmailTemplateInfo.TemplateDisplayName"/>).
        /// </summary>
        [Required]
        [JsonProperty("displayName")]
        public string DisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// Email template type.
        /// </summary>
        [Required]
        [JsonProperty("type")]
        public string Type
        {
            get;
            set;
        }
    }
}