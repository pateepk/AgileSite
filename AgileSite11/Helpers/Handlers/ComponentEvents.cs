using System;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Events that serve for communication of the components within the system.
    /// </summary>
    public class ComponentEvents
    {
        #region "Constants"

        /// <summary>
        /// Next event.
        /// </summary>
        public const string NEXT = "Next";

        /// <summary>
        /// Previous event.
        /// </summary>
        public const string PREVIOUS = "Previous";

        /// <summary>
        /// Finish event.
        /// </summary>
        public const string FINISH = "Finish";

        /// <summary>
        /// Submit event.
        /// </summary>
        public const string SUBMIT = "Submit";

        /// <summary>
        /// Cancel event.
        /// </summary>
        public const string CANCEL = "Cancel";

        /// <summary>
        /// Load step event.
        /// </summary>
        public const string LOAD_STEP = "LoadStep";

        /// <summary>
        /// Step loaded event.
        /// </summary>
        public const string STEP_LOADED = "StepLoaded";

        /// <summary>
        /// Validate step event.
        /// </summary>
        public const string VALIDATE_STEP = "ValidateStep";

        /// <summary>
        /// Finish step event.
        /// </summary>
        public const string FINISH_STEP = "FinishStep";

        /// <summary>
        /// Step finished event.
        /// </summary>
        public const string STEP_FINISHED = "StepFinished";

        /// <summary>
        /// Cancel step event.
        /// </summary>
        public const string CANCEL_STEP = "CancelStep";

        /// <summary>
        /// Save event (saves data to database).
        /// </summary>
        public const string SAVE = "Save";

        /// <summary>
        /// Save data event - only propagates changes to manager control (doesn't save data to database).
        /// </summary>
        public const string SAVE_DATA = "SaveData";

        /// <summary>
        /// Validate data.
        /// </summary>
        public const string VALIDATE_DATA = "ValidateData";

        /// <summary>
        /// Comment for workflow action.
        /// </summary>
        public const string COMMENT = "Comment";

        /// <summary>
        /// Delete event.
        /// </summary>
        public const string DELETE = "Delete";

        /// <summary>
        /// Move to next automation step event.
        /// </summary>
        public const string AUTOMATION_MOVE_NEXT = "AutomationMoveNext";

        /// <summary>
        /// Move to previous automation step event.
        /// </summary>
        public const string AUTOMATION_MOVE_PREVIOUS = "AutomationMovePrevious";

        /// <summary>
        /// Remove automation process event.
        /// </summary>
        public const string AUTOMATION_REMOVE = "RemoveAutomation";

        /// <summary>
        /// Start automation process event.
        /// </summary>
        public const string AUTOMATION_START = "StartAutomation";

        /// <summary>
        /// Move to specific step event.
        /// </summary>
        public const string AUTOMATION_MOVE_SPEC = "AutomationMoveSpecific";
        
        /// <summary>
        /// Object check-in event.
        /// </summary>
        public const string CHECKIN = "Checkin";

        /// <summary>
        /// Object check-out event.
        /// </summary>
        public const string CHECKOUT = "Checkout";

        /// <summary>
        /// Object undo check-out event.
        /// </summary>
        public const string UNDO_CHECKOUT = "UndoCheckout";

        /// <summary>
        /// Customize event.
        /// </summary>
        public const string CUSTOMIZE = "Customize";

        #endregion


        #region "Variables"

        /// <summary>
        /// Global events.
        /// </summary>
        protected static EventList mGlobalEvents = new EventList { Name = "ComponentEvents.GlobalEvents" };

        #endregion


        #region "Properties"

        /// <summary>
        /// Global events that are fired and survive across requests during the whole application lifetime.
        /// </summary>
        public static EventList GlobalEvents
        {
            get
            {
                return mGlobalEvents;
            }
        }


        /// <summary>
        /// Events that are fired and survive within current request.
        /// </summary>
        public static EventList RequestEvents
        {
            get
            {
                return RequestContext.RequestEvents;
            }
        }

        #endregion
    }
}
