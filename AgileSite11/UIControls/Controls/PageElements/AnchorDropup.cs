using System;
using System.Linq;
using System.Text;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for anchor dropup.
    /// </summary>
    public class AnchorDropup : CMSAdminControl
    {
        #region "Public properties"

        /// <summary>
        /// If amount of anchors is lower than value, control is hidden.
        /// </summary>
        public virtual int MinimalAnchors
        {
            get;
            set;
        }


        /// <summary>
        /// Vertical offset for scrolling to a target;
        /// </summary>
        public virtual int ScrollOffset
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the dropup is opened by default.
        /// </summary>
        public virtual bool IsOpened
        {
            get;
            set;
        }


        /// <summary>
        /// Sets CssClass for inner div, which is add to defaults.
        /// </summary>
        public virtual string CssClass
        {
            get;
            set;
        }

        #endregion
    }
}
