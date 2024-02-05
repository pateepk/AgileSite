namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Common interface for content rating controls.
    /// </summary>
    public abstract class AbstractRatingControl : AbstractUserControl
    {
        /// <summary>
        /// Rating event handler.
        /// </summary>
        public delegate void OnRatingEventHandler(AbstractRatingControl sender);

        /// <summary>
        /// Event is raised when user voted.
        /// </summary>
        public event OnRatingEventHandler RatingEvent;


        /// <summary>
        /// Max value of a rating scale.
        /// </summary>
        public int MaxRating { get; set; } = 10;


        /// <summary>
        /// Gets or sets current rating (float between 0 and 1).
        /// </summary>
        public double CurrentRating { get; set; }


        /// <summary>
        /// Enables/disables rating scale
        /// </summary>
        public abstract bool Enabled { get; set; }


        /// <summary>
        /// Gets or sets external management flag. If true external (parent) control
        /// manages rating value on its own (i.e. no submit button is shown).
        /// </summary>
        public bool ExternalManagement { get; set; }


        /// <summary>
        /// Reloads rating control data.
        /// </summary>
        public abstract void ReloadData();


        /// <summary>
        /// Gets current rating value.
        /// </summary>
        public virtual double GetCurrentRating()
        {
            return CurrentRating;
        }


        /// <summary>
        /// Raises RatingEvent event.
        /// </summary>
        protected void OnRating()
        {
            RatingEvent?.Invoke(this);
        }


        /// <summary>
        /// Returns path to particular rating control.
        /// </summary>
        /// <param name="filename">Filename including extension</param>
        public static string GetRatingControlUrl(string filename)
        {
            return "~/CMSAdminControls/ContentRating/Controls/" + filename;
        }
    }
}