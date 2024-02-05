namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// Represents a view model of the contact journey component.
    /// </summary>
    public class ContactJourneyViewModel
    {
        /// <summary>
        /// Gets or sets value representing how many days did the journey take.
        /// </summary>
        public string JourneyLengthDaysText
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value representing the starting date of the journey.
        /// </summary>
        public string JourneyLengthStartedDate
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value representing how many days ago was the first activity performed.
        /// </summary>
        public string LastActivityDaysAgoText
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value representing the date of the last activity.
        /// </summary>
        public string LastActivityDate
        {
            get;
            set;
        }
    }
}