using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Multi button which displays first action only as a label which opens dropdown list with other actions.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSToggleButton : CMSMultiButtonBase
    {
        /// <summary>
        /// Sets class class.
        /// </summary>
        /// <param name="e">Args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.AddCssClass("btn-dropdown");
        }


        /// <summary>
        /// Creates first button which will open dropdown with other actions. This button does not perform action itself.
        /// </summary>
        /// <param name="firstAction">Text of this action will be displayed</param>
        /// <returns>Created button</returns>
        protected override WebControl InitOpeningButtonControls(CMSButtonAction firstAction)
        {
            var button = new CMSButton()
            {
                ButtonStyle = ButtonStyle.Default,
                Text = firstAction.Text + CMSButton.ICON_PLACEMENT_MACRO,
                Enabled = Enabled && firstAction.Enabled,
                ToolTip = firstAction.ToolTip,
                UseSubmitBehavior = false,
                IconCssClass = "caret",
                OnClientClick = "return false;",
            };
            button.AddCssClass("dropdown-toggle");
            button.Attributes.Add("data-toggle", "dropdown");

            Controls.Add(button);

            return button;
        }
    }
}