using System.ComponentModel;

namespace CMS.ApplicationDashboard.Web.UI.Internal
{    
    /// <summary>
    /// Data class containing information needed to display the welcome tile.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class WelcomeTileModel
    {
        /// <summary>
        /// Gets or sets whether the welcome tile is visible on the dashboard.
        /// </summary>
        public bool Visible
        {
            get;
            set;
        }


        /// <summary>
        /// Header text of the tile.
        /// </summary>
        public string Header
        {
            get;
            set;
        }


        /// <summary>
        /// Description of the tile.
        /// </summary>
        public string Description
        {
            get;
            set;
        }


        /// <summary>
        /// Link leading to the application list text.
        /// </summary>
        public string BrowseApplicationsText
        {
            get;
            set;
        }


        /// <summary>
        /// Link leading to the context help text.
        /// </summary>
        public string OpenHelpText
        {
            get;
            set;
        }
    }
}