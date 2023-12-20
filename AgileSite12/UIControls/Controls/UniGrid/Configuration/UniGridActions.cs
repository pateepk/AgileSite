using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Xml.Linq;

using CMS.Helpers;

namespace CMS.UIControls.UniGridConfig
{
    /// <summary>
    /// UniGrid actions definition.
    /// </summary>
    [ParseChildren(typeof(AbstractAction), DefaultProperty = "Actions", ChildrenAsProperties = true)]
    public class UniGridActions : AbstractConfiguration
    {
        #region "Properties"

        /// <summary>
        /// Specifies the name of the CSS class from the stylesheet to be used for the actions column.
        /// </summary>
        public new string CssClass
        {
            get;
            set;
        }


        /// <summary>
        /// A list of columns used as parameters in the onclick or menuparameter attributes of child action elements separated by semicolons.
        /// Sample value: "AttachmentGUID;AttachmentFormGUID"
        /// </summary>
        public string Parameters
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the header of the actions column should be displayed. The default value is true.
        /// </summary>
        public bool ShowHeader
        {
            get;
            set;
        }


        /// <summary>
        /// Determines the width of the actions column in the UniGrid.
        /// Sample values: "30%", "100px"
        /// </summary>
        public new string Width
        {
            get;
            set;
        }


        /// <summary>
        /// List of the actions.
        /// </summary>
        public List<AbstractAction> Actions
        {
            get;
            set;
        }


        #region "Context menu properties"

        /// <summary>
        /// Specifies the resource string used as the tooltip of the context menu arrow icon. Must begin and end with the $ character.
        /// Sample value: "$General.Delete$"
        /// </summary>
        public string Caption
        {
            get;
            set;
        }


        /// <summary>
        /// The relative path to a control (.ascx file) that implements a context menu for the action column of header. Controls created for this purpose must inherit from the CMS.Base.Web.UI.CMSContextMenuControl class.
        /// Sample value: "~/CMSAdminControls/UI/UniGrid/Controls/ObjectMenu.ascx"
        /// </summary>
        public string ContextMenu
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the actions allow export of the grid data
        /// </summary>
        internal bool AllowExport
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the reset action is allowed
        /// </summary>
        internal bool AllowReset
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the show filter action is allowed
        /// </summary>
        internal bool AllowShowFilter
        {
            get;
            set;
        }

        #endregion


        #endregion


        #region "Methods"

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public UniGridActions()
        {
            ShowHeader = true;
            Actions = new List<AbstractAction>();
            CssClass = "unigrid-actions";
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="actions">Actions XML element</param>
        public UniGridActions(XElement actions)
            : this()
        {
            Width = actions.GetAttributeStringValue("width");
            ShowHeader = actions.GetAttributeValue("showheader", true);

            string attributeCssClass = actions.GetAttributeStringValue("cssclass");
            if (!String.IsNullOrEmpty(attributeCssClass))
            {
                CssClass += " " + attributeCssClass;
            }

            Parameters = actions.GetAttributeStringValue("parameters");
            Caption = actions.GetAttributeStringValue("caption");
            ContextMenu = actions.GetAttributeStringValue("contextmenu");

            foreach (var action in actions.Elements())
            {
                if(action.Name.LocalName.Equals("action", StringComparison.OrdinalIgnoreCase))
                {
                    Actions.Add(new Action(action));
                }

                if (action.Name.LocalName.Equals("buttonaction", StringComparison.OrdinalIgnoreCase))
                {
                    Actions.Add(new ButtonAction(action));
                }
            }
        }


        /// <summary>
        /// Returns UniGrid action with specified name.
        /// </summary>
        /// <param name="name">Name of the action to be returned</param>
        [Obsolete("Use UniGridActions.GetActionByName instead.")]
        public Action GetAction(string name)
        {
            // Search for the action
            return Actions
                .OfType<Action>()
                .FirstOrDefault(action => action.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }


        /// <summary>
        /// Returns UniGrid action with specified name.
        /// </summary>
        /// <param name="name">Name of the action to be returned</param>
        public AbstractAction GetActionByName(string name)
        {
            // Search for the action
            return Actions
                .FirstOrDefault(action => !(action is EmptyAction) && action.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}