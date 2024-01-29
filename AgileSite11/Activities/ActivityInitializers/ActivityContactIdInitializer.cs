namespace CMS.Activities
{
    /// <summary>
    /// Initializes activity with contact id. All other members are delegated to provided initializer.
    /// </summary>
    internal class ActivityContactIdInitializer : ActivityInitializerWrapperBase
    {
        private readonly int mContactId;


        /// <summary>
        /// Initializes new instance of <see cref="ActivityContactIdInitializer"/> with provided <paramref name="contactId"/>.
        /// Other setting that <see cref="IActivityInfo.ActivityContactID"/> of <paramref name="originalInitializer"/> is not changed.
        /// </summary>
        /// <param name="originalInitializer">Original activity initializer</param>
        /// <param name="contactId">Contact id to insert in activity</param>
        public ActivityContactIdInitializer(IActivityInitializer originalInitializer, int contactId)
            : base(originalInitializer)
        {
            mContactId = contactId;
        }


        /// <summary>
        /// Calls method on original initializer and sets contactId.
        /// </summary>
        /// <param name="activity"></param>
        public override void Initialize(IActivityInfo activity)
        {
            OriginalInitializer.Initialize(activity);

            activity.ActivityContactID = mContactId;
        }
    }
}