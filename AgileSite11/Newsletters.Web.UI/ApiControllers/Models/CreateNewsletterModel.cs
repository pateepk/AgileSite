using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace CMS.Newsletters.Web.UI.Internal
{
    /// <summary>
    /// Represents view model for <see cref="NewsletterInfo"/>.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CreateNewsletterModel
    {
        /// <summary>
        /// Newsletter display name <see cref="NewsletterInfo.NewsletterDisplayName"/>.
        /// </summary>
        [Required]
        [JsonProperty("displayName")]
        public string DisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// Newsletter sender name <see cref="NewsletterInfo.NewsletterSenderName"/>
        /// </summary>
        [Required]
        [JsonProperty("senderName")]
        public string SenderName
        {
            get;
            set;
        }


        /// <summary>
        /// Newsletter sender email <see cref="NewsletterInfo.NewsletterSenderEmail"/>.
        /// </summary>
        [Required]
        [CMSEmail]
        [JsonProperty("senderEmail")]
        public string SenderEmail
        {
            get;
            set;
        }


        /// <summary>
        /// Newsletter template type.
        /// </summary>
        [Required]
        [JsonProperty("unsubscriptionTemplateId")]
        public int UnsubscriptionTemplateId
        {
            get;
            set;
        }


        /// <summary>
        /// Identifiers of issue type templates.
        /// </summary>
        [Required]
        [JsonProperty("issueTemplateId")]
        public int IssueTemplateId
        {
            get;
            set;
        }
    }
}
