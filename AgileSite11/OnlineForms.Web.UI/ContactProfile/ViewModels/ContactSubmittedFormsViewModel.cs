using System;

namespace CMS.OnlineForms.Web.UI
{
    /// <summary>
    /// Represents view model for the submitted forms of contact component.
    /// </summary>
    public class ContactSubmittedFormsViewModel
    {
        /// <summary>
        /// Gets or sets the display name of the submitted form.
        /// </summary>
        public string FormDisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the submission date of the submitted form.
        /// </summary>
        public DateTime FormSubmissionDate
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the URL to forms tab.
        /// </summary>
        public string FormUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the site display name of forms location.
        /// </summary>
        public string SiteDisplayName
        {
            get;
            set;
        }
    }
}