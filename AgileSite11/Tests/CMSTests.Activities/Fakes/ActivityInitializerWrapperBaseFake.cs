using CMS.Activities;

namespace Tests.Activities
{
    /// <summary>
    /// Fake class for <see cref="ActivityInitializerWrapperBase"/>. Should be used only in tests.
    /// </summary>
    public class ActivityInitializerWrapperBaseFake : ActivityInitializerWrapperBase
    {
        /// <summary>
        /// Constructs new instance of <see cref="ActivityInitializerWrapperBase"/>.
        /// </summary>
        /// <param name="originalInitializer">Original activity initializer</param>
        public ActivityInitializerWrapperBaseFake(IActivityInitializer originalInitializer)
            : base(originalInitializer)
        {
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public override void Initialize(IActivityInfo activity)
        {
            OriginalInitializer.Initialize(activity);
        }
    }
}