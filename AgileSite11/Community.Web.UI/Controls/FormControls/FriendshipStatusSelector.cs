using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.Community.Web.UI
{
    /// <summary>
    /// Form control for the friendship status selection.
    /// </summary>
    [ToolboxData("<{0}:FriendshipStatusSelector runat=server></{0}:FriendshipStatusSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class FriendshipStatusSelector : FormControl
    {
        #region "Public properties"

        /// <summary>
        /// Gets or sets the FriendshipStatus ID.
        /// </summary>
        public int FriendshipStatusID
        {
            get
            {
                return ValidationHelper.GetInteger(Value, 0);
            }
            set
            {
                Value = value;
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public FriendshipStatusSelector()
        {
            FormControlName = "FriendshipStatusSelector";
        }
    }
}