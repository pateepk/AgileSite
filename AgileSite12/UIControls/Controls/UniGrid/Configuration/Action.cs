using System.Xml.Linq;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls.UniGridConfig
{
    /// <summary>
    /// UniGrid icon action button.
    /// </summary>
    public class Action : AbstractAction
    {
        /// <summary>
        /// Name of the image that should be used as the icon of the action. The image must be located in the folder defined by the ImageDirectoryPath property of the UniGrid.
        /// Sample value: "delete.png"
        /// </summary>
        public string Icon
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the CSS class that serves as icon for the button.
        /// </summary>
        public string FontIconClass
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the style of the font icon.
        /// </summary>
        public GridIconStyle FontIconStyle
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public Action()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Action name</param>
        public Action(string name) : base(name)
        {
            InitializeActionForObjectMenu();
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="action">XML element with the action definition</param>
        public Action(XElement action) : base(action)
        {
            FontIconClass = action.GetAttributeValue("fonticonclass", FontIconClass);
            FontIconStyle = action.GetAttributeValue("fonticonstyle", "default").ToEnum<GridIconStyle>();
            Icon = action.GetAttributeValue("icon", Icon);
        }


        private void InitializeActionForObjectMenu()
        {
            // Built-in action for object menu
            switch (Name)
            {
                case "#objectmenu":
                    Caption = "$General.OtherActions$";
                    FontIconClass = "icon-ellipsis";
                    ContextMenu = "~/CMSAdminControls/UI/UniGrid/Controls/ObjectMenu.ascx";
                    MenuParameter = "new Array('{objecttype}', '{0}')";
                    break;

                case "#clone":
                    Caption = "$General.Clone$";
                    FontIconClass = "icon-doc-copy";
                    break;
            }
        }
    }
}