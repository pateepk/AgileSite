using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Events that serve for communication of the document components within the system.
    /// </summary>
    public class DocumentComponentEvents
    {
        #region "Constants"

        /// <summary>
        /// Validate data.
        /// </summary>
        public const string VALIDATE_DATA = "DocumentValidateData";

        /// <summary>
        /// Save data event - only propagates changes to manager control (doesn't save data to database).
        /// </summary>
        public const string SAVE_DATA = "DocumentSaveData";

        /// <summary>
        /// Load data event - ensures data for control before action.
        /// </summary>
        public const string LOAD_DATA = "DocumentLoadData";

        /// <summary>
        /// Before action event.
        /// </summary>
        public const string BEFORE_ACTION = "DocumentBeforeAction";

        /// <summary>
        /// After action event.
        /// </summary>
        public const string AFTER_ACTION = "DocumentAfterAction";

        /// <summary>
        /// Approve event.
        /// </summary>
        public const string APPROVE = "DocumentApprove";

        /// <summary>
        /// Publish event.
        /// </summary>
        public const string PUBLISH = "DocumentPublish";

        /// <summary>
        /// Archive event.
        /// </summary>
        public const string ARCHIVE = "DocumentArchive";

        /// <summary>
        /// Reject event.
        /// </summary>
        public const string REJECT = "DocumentReject";

        /// <summary>
        /// Check-out event.
        /// </summary>
        public const string CHECKOUT = ComponentEvents.CHECKOUT;

        /// <summary>
        /// Check-in event.
        /// </summary>
        public const string CHECKIN = ComponentEvents.CHECKIN;

        /// <summary>
        /// Undo check-out event.
        /// </summary>
        public const string UNDO_CHECKOUT = ComponentEvents.UNDO_CHECKOUT;

        /// <summary>
        /// Create new version event.
        /// </summary>
        public const string CREATE_VERSION = "DocumentCreateVersion";

        /// <summary>
        /// Apply document workflow event.
        /// </summary>
        public const string APPLY_WORKFLOW = "DocumentApplyWorkflow";
        
        #endregion
    }
}
