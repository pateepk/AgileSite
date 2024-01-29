using System;
using System.Linq;
using System.Text;

namespace CMS.UIControls
{
    /// <summary>
    /// User filter interface
    /// </summary>
    public interface IUserFilter
    {

        /// <summary>
        /// Indicates if guests should be displayed.
        /// </summary>
        bool DisplayGuests
        {
            get;
        }


        /// <summary>
        /// Indicates if 'user enabled' filter should be visible.
        /// </summary>
        bool DisplayUserEnabled
        {
            get;
            set;
        }


        /// <summary>
        /// Selected score.
        /// </summary>
        int SelectedScore
        {
            get;
        }


        /// <summary>
        /// Indicates if checkbox for hiding/displaying hidden users should be visible.
        /// </summary>
        bool EnableDisplayingHiddenUsers
        {
            get;
            set;
        }

        
        /// <summary>
        /// Indicates if filter is working with CMS_Session table instead of CMS_User.
        /// </summary>
        bool SessionInsteadOfUser
        {
            get;
            set;
        }


        /// <summary>
        /// Selected site.
        /// </summary>
        int SelectedSite
        {
            get;
        }


        /// <summary>
        /// Gets or sets filter mode for various type of users list.
        /// </summary>
        string CurrentMode
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if checkbox for displaying guests should be visible.
        /// </summary>
        bool EnableDisplayingGuests
        {
            get;
            set;
        }
    }
}
