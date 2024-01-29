using CMS.Base.Web.UI;
using CMS.Membership;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for smart tip control.
    /// </summary>
    public abstract class SmartTipControl : CMSUserControl
    {
        /// <summary>
        /// Manager for dismissing smart tip.
        /// </summary>
        protected readonly UserSmartTipDismissalManager UserSmartTipManager = new UserSmartTipDismissalManager(MembershipContext.AuthenticatedUser);


        /// <summary>
        /// Gets or sets the identifier of the smart tip used for storing the collapsed state. If multiple smart tips with the same
        /// identifier are created, closing one will result in closing all of them.
        /// </summary>
        public string CollapsedStateIdentifier
        {
            get;
            set;
        }


        /// <summary>
        /// Sets the expanded header of the smart tip.
        /// Use plain text.
        /// </summary>
        public string ExpandedHeader
        {
            get;
            set;
        }


        /// <summary>
        /// Sets the collapsed header of the smart tip.
        /// Use plain text.
        /// </summary>
        public string CollapsedHeader
        {
            get;
            set;
        }


        /// <summary>
        /// Sets the content of the smart tip.
        /// Use HTML.
        /// </summary>
        public string Content
        {
            get;
            set;
        }
    }
}
