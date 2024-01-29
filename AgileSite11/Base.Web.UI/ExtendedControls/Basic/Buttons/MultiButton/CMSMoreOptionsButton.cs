using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Button allows to perform the first action as well as to perform other actions which are hidden in the dropdown menu.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSMoreOptionsButton : CMSMultiButtonBase
    {
        /// <summary>
        /// Screen reader description of more actions button. This description is used for accessibility.
        /// </summary>
        public string ScreenReaderDescription
        {
            get;
            set;
        }


        /// <summary>
        /// Sets css classes in addition to performing default initialization.
        /// </summary>
        /// <param name="actions">Actions of the button</param>
        protected override void InitChildControlsForMoreThanOneActions(List<CMSButtonAction> actions)
        {
            this.AddCssClass("btn-group");
            this.AddCssClass("btn-group-split");

            base.InitChildControlsForMoreThanOneActions(actions);
        }


        /// <summary>
        /// Creates two buttons. When first button is clicked, the action is performed. Second button opens dropdown menu.
        /// </summary>
        /// <param name="firstAction">First action</param>
        /// <returns>Button opening dropdown</returns>
        protected override WebControl InitOpeningButtonControls(CMSButtonAction firstAction)
        {
            var button = new CMSButton
            {
                ButtonStyle = ButtonStyle.Default,
                Text = firstAction.Text,
                Enabled = Enabled && firstAction.Enabled,
                ToolTip = firstAction.ToolTip,
                UseSubmitBehavior = false,
                OnClientClick = GetActionOnClientClick(firstAction),
            };

            Controls.Add(button);

            var openOptionsButton = new CMSAccessibleButton
            {
                Enabled = Enabled && Actions.Skip(1).All(action => action.Enabled),
                IconCssClass = "icon-ellipsis",
                ScreenReaderDescription = ScreenReaderDescription ?? this.GetString("multibutton.moreoptions"),
                UseSubmitBehavior = false,
                OnClientClick = "return false;"
            };
            openOptionsButton.AddCssClass("dropdown-toggle");
            openOptionsButton.Attributes.Add("data-toggle", "dropdown");

            // Register bootstrap script for dropdown button
            ScriptHelper.RegisterBootstrapScripts(Page);

            Controls.Add(openOptionsButton);

            return openOptionsButton;
        }
    }
}
