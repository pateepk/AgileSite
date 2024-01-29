using System;
using System.Linq;
using System.Text;
using System.Web.UI;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// 
    /// </summary>
    public enum GridIconStyle
    {
        /// <summary>
        /// Default icon style
        /// </summary>
        [EnumStringRepresentation("default")]
        Default,

        /// <summary>
        /// Style of icon used to start a positive action such as allow, approve, add, run, subscribe.
        /// </summary>
        [EnumStringRepresentation("allow")]
        Allow,

        /// <summary>
        /// Style of icon that indicates warning or problem.
        /// </summary>
        [EnumStringRepresentation("warning")]
        Warning,

        /// <summary>
        /// Style of icon used to start a negative action such as cancel, remove, delete, stop.
        /// </summary>
        [EnumStringRepresentation("critical")]
        Critical,
    }


    /// <summary>
    /// Button that is used to render unigrid action
    /// </summary>
    public class CMSGridActionButton : CMSAccessibleButton
    {
        #region "Properties"

        /// <summary>
        /// If true, the button is the only-image button. The button is then without a button-like background and only image is dispayed. I.e. it adds "btn-icon" class.
        /// </summary>
        public override bool IconOnly
        {
            get
            {
                return true;
            }
            set
            {
                throw new NotSupportedException("CMSGridActionButton is always in icon only mode. This property cannot be changed");
            }
        }


        /// <summary>
        /// When true button is render as type="submit". Otherwise type is type="button".
        /// </summary>
        public override bool UseSubmitBehavior
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException("CMSGridActionButton is never using submit mode. This property cannot be changed");
            }
        }


        /// <summary>
        /// Gets or sets the style of the icon.
        /// </summary>
        public GridIconStyle IconStyle
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Renders the button.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            this.AddCssClass("btn-unigrid-action");

            string iconStyleClass = null;

            switch (IconStyle)
            {
                case GridIconStyle.Allow:
                    iconStyleClass = "icon-style-allow";
                    break;
                case GridIconStyle.Critical:
                    iconStyleClass = "icon-style-critical";
                    break;
                case GridIconStyle.Warning:
                    iconStyleClass = "icon-style-warning";
                    break;
            }

            if (iconStyleClass != null)
            {
                this.AddCssClass(iconStyleClass);
            }

            base.Render(writer);
        }

        #endregion
    }
}
