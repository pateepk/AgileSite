using System.Web.UI.WebControls;

using CMS.DocumentEngine;
using CMS.Membership;


namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Encapsulates configuration properties needed to render an opening tag of On-Site edit envelope.
    /// </summary>
    public sealed class OnSiteEditStartTagConfiguration
    {
        /// <summary>
        /// The edit page URL used for edit dialog.
        /// </summary>
        public string EditUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Width of the edit dialog.
        /// </summary>
        public Unit DialogWidth
        {
            get;
            set;
        }


        /// <summary>
        /// The web part title.
        /// </summary>
        public string WebPartTitle
        {
            get;
            set;
        }


        /// <summary>
        /// The web part instance.
        /// </summary>
        public WebPartInstance WebPartInstance
        {
            get;
            set;
        }


        /// <summary>
        /// The page being edited.
        /// </summary>
        public PageInfo Page
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the web part is editable.
        /// </summary>
        public bool WebPartIsEditable
        {
            get;
            set;
        }


        /// <summary>
        /// The current control object (must be defined in ASPX mode).
        /// </summary>
        public object ControlObject
        {
            get;
            set;
        }


        /// <summary>
        /// Represents currently authenticated user.
        /// </summary>
        public CurrentUserInfo CurrentUser
        {
            get;
            set;
        }
    }
}
