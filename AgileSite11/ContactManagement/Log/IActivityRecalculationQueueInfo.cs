namespace CMS.ContactManagement
{
    /// <summary>
    /// Specifies contract for <see cref="ActivityRecalculationQueueInfo"/>. 
    /// </summary>
    public interface IActivityRecalculationQueueInfo
    {
        /// <summary>
        /// Activity recalculation queue ID.
        /// </summary>
        int ActivityRecalculationQueueID
        {
            get;
            set;
        }


        /// <summary>
        /// Activity recalculation queue activity ID.
        /// </summary>
        int ActivityRecalculationQueueActivityID
        {
            get;
            set;
        }
    }
}