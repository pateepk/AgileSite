using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace CMS.Newsletters.Web.UI.Internal
{
    /// <summary>
    /// Represents view model for <see cref="IssueInfo"/>.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IssueViewModel
    {
        /// <summary>
        /// Issue id <see cref="IssueInfo.IssueID"/>.
        /// </summary>
        [Required]
        [JsonProperty("id")]
        public int Id
        {
            get;
            set;
        }


        /// <summary>
        /// Newsletter id <see cref="IssueInfo.IssueID"/>.
        /// </summary>
        [Required]
        [JsonProperty("newsletterId")]
        public int NewsletterId
        {
            get;
            set;
        }


        /// <summary>
        /// Email subject <see cref="IssueInfo.IssueSubject"/>.
        /// </summary>
        [Required]
        [JsonProperty("subject")]
        public string Subject
        {
            get;
            set;
        }
    }
}