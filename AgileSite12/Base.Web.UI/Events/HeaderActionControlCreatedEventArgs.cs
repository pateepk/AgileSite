using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI.ActionsConfig;


namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Event arguments for when a header action control is being constructed
    /// </summary>
    public class HeaderActionControlCreatedEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Gets or sets the header action control.
        /// Is null in Before phase. 
        /// Is provided in the After phase.
        /// Can be overwritten and changed in the After phase.
        /// Can be provided in the Before phase if event is then canceled.
        /// </summary>
        public Control ActionControl
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the header action object on which the action control is based.
        /// </summary>
        public HeaderAction Action
        {
            get;
            set;
        }
    }
}
