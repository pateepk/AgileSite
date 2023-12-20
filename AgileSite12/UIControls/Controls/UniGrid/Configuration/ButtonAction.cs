using System.Xml.Linq;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls.UniGridConfig
{
    /// <summary>
    /// UniGrid action button.
    /// </summary>
    public class ButtonAction : AbstractAction
    {
        /// <summary>
        /// Gets or sets an enum which represents button type based on which CSS class will be rendered.
        /// </summary>
        /// <remarks>If value is <see cref="ButtonStyle.None"/> no style will be applied.</remarks>
        public ButtonStyle ButtonStyle { get; set; } = ButtonStyle.None;


        /// <summary>
        /// Gets or sets the CSS class for the button rendered by the Web server control on the client.
        /// </summary>
        public string ButtonClass { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="ButtonAction"/>.
        /// </summary>
        public ButtonAction()
        {
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ButtonAction"/> with given <paramref name="name"/>.
        /// </summary>
        public ButtonAction(string name) : base(name)
        {
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ButtonAction"/> with given <paramref name="action"/>.
        /// </summary>
        public ButtonAction(XElement action) : base(action)
        {
            ButtonClass = action.GetAttributeValue("ButtonClass", ButtonClass);
            ButtonStyle = action.GetAttributeValue("ButtonStyle", ButtonStyle.Primary.ToString()).ToEnum<ButtonStyle>();
        }
    }
}
