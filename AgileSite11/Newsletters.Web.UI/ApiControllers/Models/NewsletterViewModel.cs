using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using CMS.WebApi;

using Newtonsoft.Json;

namespace CMS.Newsletters.Web.UI.Internal
{
    /// <summary>
    /// Represents view model for <see cref="NewsletterInfo"/>.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class NewsletterViewModel
    {
        /// <summary>
        /// Newsletter id <see cref="NewsletterInfo.NewsletterID"/>.
        /// </summary>
        [JsonProperty("id")]
        public int Id
        {
            get;
            set;
        }


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
        [ContainsKey("unsubscription")]
        [JsonProperty("templates")]
        public Dictionary<string, int> Templates
        {
            get;
            set;
        }


        /// <summary>
        /// Identifiers of issue type templates.
        /// </summary>
        [JsonProperty("issueTemplates")]
        public List<TemplateViewModel> IssueTemplates
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public NewsletterViewModel()
        {
            Templates = new Dictionary<string, int>();
            IssueTemplates = new List<TemplateViewModel>();
        }
    }
}