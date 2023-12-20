using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Localization;
using CMS.Modules;
using CMS.UIControls.UniGridConfig;
using CMS.DataEngine.CollectionExtensions;

using Action = CMS.UIControls.UniGridConfig.Action;

namespace CMS.UIControls
{
    /// <summary>
    /// Creates template field in grid view dynamically.
    /// </summary>
    [ToolboxItem(false)]
    public class GridViewTemplate : WebControl, ITemplate
    {
        #region "Delegates and events"

        /// <summary>
        /// Event raised when external source data required.
        /// </summary>
        public event OnExternalDataBoundEventHandler OnExternalDataBound;

        #endregion


        #region "Variables"

        private readonly ListItemType templateType = ListItemType.Item;

        private readonly Control mHeaderControl;
        private readonly Control mItemControl;
        private readonly UniGridActions mActions;
        private UniGrid mUniGridControl;

        private readonly string mColumnName;
        private readonly string mImageDirectoryPath;
        private readonly string mDefaultImageDirectoryPath;

        private readonly string[] mActionParameters;
        private string mObjectType;
        private HashSettings mHashSettings;
        private Dictionary<string, bool> mActionsAuthorization;

        #endregion


        #region "Properties"

        /// <summary>
        /// Hash settings for generating hashes for this control.
        /// </summary>
        public HashSettings HashSettings => mHashSettings ?? (mHashSettings = new HashSettings(UniGridControl.ClientID));


        /// <summary>
        /// Object type.
        /// </summary>
        private string ObjectType => mObjectType ?? (mObjectType = GetObjectType());


        /// <summary>
        /// Parent UniGrid control.
        /// </summary>
        public UniGrid UniGridControl
        {
            get
            {
                // Set the unigrid control
                if (mUniGridControl == null)
                {
                    throw new Exception("[GridViewTemplate.UniGridControl]: The UniGridControl property must be initialized.");
                }

                return mUniGridControl;
            }
            set => mUniGridControl = value;
        }


        /// <summary>
        /// Control which is the parent for the context menu (optional).
        /// </summary>
        public Control ContextMenuParent
        {
            get;
            set;
        }


        /// <summary>
        /// If true, relative ancestor div is checked.
        /// </summary>
        public bool CheckRelative
        {
            get;
            set;
        }


        /// <summary>
        /// Cache for UniGrid actions authorization.
        /// </summary>
        private Dictionary<string, bool> ActionsAuthorization => mActionsAuthorization ?? (mActionsAuthorization = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase));

        #endregion


        #region "Constructors"

        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="type">Type of item</param>
        /// <param name="uniGrid">Parent grid control</param>
        protected GridViewTemplate(ListItemType type, UniGrid uniGrid)
        {
            CheckRelative = false;
            templateType = type;
            UniGridControl = uniGrid;
        }


        /// <summary>
        /// Basic constructor.
        /// </summary>
        public GridViewTemplate(ListItemType type, UniGrid uniGrid, Control control)
            : this(type, uniGrid)
        {
            switch (type)
            {
                case ListItemType.Header:
                    mHeaderControl = control;
                    break;

                case ListItemType.Item:
                    mItemControl = control;
                    break;
            }
        }


        /// <summary>
        /// Advanced constructor.
        /// </summary>
        public GridViewTemplate(ListItemType type, UniGrid uniGrid, UniGridActions actionsConfig, string colName, string imageDirectoryPath, string defaultImageDirectoryPath, Page page)
            : this(type, uniGrid)
        {
            mActions = actionsConfig;
            // Get the parameters column names
            if (mActions.Parameters != null)
            {
                mActionParameters = mActions.Parameters.Split(';');
            }
            else
            {
                var infoObj = uniGrid.InfoObject;
                if (infoObj != null)
                {
                    var ti = infoObj.TypeInfo;

                    if (ti.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        mActionParameters = new[]
                        {
                            ti.IDColumn
                        };
                    }
                }
            }

            mColumnName = colName;
            mImageDirectoryPath = imageDirectoryPath;
            mDefaultImageDirectoryPath = defaultImageDirectoryPath;

            Page = page;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns icon file for current theme or from default if current doesn't exist.
        /// </summary>
        /// <param name="iconfile">Icon file name</param>
        private string GetActionImage(string iconfile)
        {
            if (((HttpContext.Current != null) && File.Exists(StorageHelper.GetFullFilePhysicalPath(mImageDirectoryPath + iconfile))) || (mDefaultImageDirectoryPath == null))
            {
                return mImageDirectoryPath + iconfile;
            }

            // Short path to the icon
            if (ControlsExtensions.RenderShortIDs)
            {
                return UIHelper.GetShortImageUrl(UIHelper.UNIGRID_ICONS, iconfile);
            }

            return AdministrationUrlHelper.ResolveImageUrl(mDefaultImageDirectoryPath + iconfile);
        }


        /// <summary>
        /// Handle DataBinding event called in the InstantiateIn method.
        /// </summary>
        private void BindData(object sender, EventArgs e)
        {
            Control ctrl = (Control)sender;
            GridViewRow container = (GridViewRow)ctrl.NamingContainer;
            DataRowView drv = (DataRowView)container.DataItem;

            if (sender is IButtonControl buttonControl)
            {
                var uniGrid = UniGridControl;
                var attributes = ((WebControl)buttonControl).Attributes;

                // Process command argument
                string commandArgument = buttonControl.CommandArgument;
                if (String.IsNullOrEmpty(commandArgument))
                {
                    // First found column
                    commandArgument = drv[0].ToString();
                }
                else
                {
                    string columnName = commandArgument;
                    commandArgument = drv.Row.Table.Columns.Contains(columnName) ? drv[columnName].ToString() : ApplyParameters(commandArgument, container);
                }

                // Use the command argument as the ID for row identification
                if (container.Attributes["data-objectid"] == null)
                {
                    container.Attributes.Add("data-objectid", commandArgument);
                }

                // Add to the allowed actions
                uniGrid.ActionsID.Add(commandArgument);

                buttonControl.CommandArgument = commandArgument;

                // Set default action
                string action = buttonControl.CommandName;

                string onclick = attributes["onclick"];

                if ((onclick == null) || !onclick.StartsWith("return false;", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle edit action URL
                    if (action.Equals("edit", StringComparison.OrdinalIgnoreCase) && (uniGrid.EditActionUrl != null))
                    {
                        string url = UrlResolver.ResolveUrl(uniGrid.EditActionUrl);

                        if (uniGrid.EditInDialog)
                        {
                            // Edit in dialog
                            url = URLHelper.AddParameterToUrl(url, "dialog", "1");
                            url = ApplicationUrlHelper.AppendDialogHash(url);

                            ScriptHelper.RegisterDialogScript(Page);

                            onclick += ScriptHelper.GetModalDialogScript(url, "edit", uniGrid.DialogWidth, uniGrid.DialogHeight);
                        }
                        else
                        {
                            // Edit on the same page
                            onclick += "return " + uniGrid.GetJSModule() + ".redir(" + ScriptHelper.GetString(ScriptHelper.ResolveUrl(url)) + ");";
                        }
                    }
                    else if (action.Equals("#move", StringComparison.OrdinalIgnoreCase))
                    {
                        onclick += "return false;";
                        attributes["onmousedown"] = uniGrid.GetJSModule() + ".initMove(" + ScriptHelper.GetString(commandArgument) + "); return false;";
                    }
                    else
                    {
                        // Perform action command
                        onclick += "return " + uniGrid.GetJSModule() + ".command(" + ScriptHelper.GetString(action) + ", " + ScriptHelper.GetString(commandArgument) + ");";
                    }
                }

                // Apply the parameters
                onclick = ApplyParameters(onclick, container);

                // Set confirmation
                string confirmation = attributes["confirmation"];
                if (confirmation != null)
                {
                    onclick = $"if ({confirmation}) {{ {onclick} }} return false;";
                    attributes.Remove("confirmation");
                }

                // Set onclick attribute
                if (sender is CMSGridActionButton iconButton)
                {
                    iconButton.Attributes.Remove("onclick");
                    iconButton.OnClientClick = onclick;
                }
                else
                {
                    attributes["onclick"] = onclick;
                }

                // External source
                string externalSource = attributes["externalsourcename"];
                if (externalSource != null)
                {
                    OnExternalDataBound(sender, externalSource, container);
                    attributes.Remove("externalsourcename");
                }
            }
            else
            {
                // Init context menu parameter
                if (sender is ContextMenuContainer menu)
                {
                    menu.Parameter = ApplyParameters(menu.Parameter, container);
                }
            }
        }


        /// <summary>
        /// Handle DataBinding event called in the InstantiateIn method.
        /// </summary>
        private void BindCheckData(object sender, EventArgs e)
        {
            CMSCheckBox checkBox = (CMSCheckBox)sender;
            GridViewRow container = (GridViewRow)checkBox.NamingContainer;

            var uniGrid = UniGridControl;

            if (container.RowType == DataControlRowType.DataRow)
            {
                if (container.DataItem != null)
                {
                    var dataItem = (DataRowView)container.DataItem;

                    var selColumn = checkBox.Attributes["selectioncolumn"];

                    string argument = String.IsNullOrEmpty(selColumn) ? dataItem[0].ToString() : dataItem[selColumn].ToString();

                    if (uniGrid != null)
                    {
                        // Add to the allowed selection
                        uniGrid.SelectionsID.Add(argument);

                        checkBox.Page.PreRenderComplete += ((senderObj, eventArgs) => { if (uniGrid.SelectedItems != null) { checkBox.Checked = uniGrid.SelectedItems.Contains(argument, StringComparer.InvariantCultureIgnoreCase); } });

                        string onclick = uniGrid.GetJSModule() + ".select(this);";

                        checkBox.InputAttributes["data-arg"] = argument;
                        checkBox.InputAttributes["data-argHash"] = ValidationHelper.GetHashString(argument, HashSettings);

                        if (!String.IsNullOrEmpty(uniGrid.SelectionJavascript))
                        {
                            onclick += $"{uniGrid.SelectionJavascript}('{ScriptHelper.GetString(argument, false)}', this.checked);";
                        }

                        checkBox.InputAttributes["onclick"] = onclick;
                        checkBox.InputAttributes["onchange"] = uniGrid.GetJSModule() + ".updateHeaderCheckbox();";

                        uniGrid.RaiseExternalDataBound(checkBox, UniGrid.SELECTION_EXTERNAL_DATABOUND, container.DataItem);
                    }

                    checkBox.Attributes.Remove("selectioncolumn");
                }
            }
            else if (container.RowType == DataControlRowType.Header)
            {
                if (uniGrid != null)
                {
                    checkBox.Attributes["onclick"] = uniGrid.GetJSModule() + ".selectAll(this);";
                    checkBox.Checked = false;

                    uniGrid.RaiseExternalDataBound(checkBox, UniGrid.SELECTALL_EXTERNAL_DATABOUND, container.DataItem);
                }
            }
        }


        /// <summary>
        /// Applies the action parameters to the given string.
        /// </summary>
        /// <param name="text">Text to process</param>
        /// <param name="container">Container with the data</param>
        private string ApplyParameters(string text, GridViewRow container)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            DataRowView rowView = ((DataRowView)container.DataItem);

            text = text.Replace("{objecttype}", ObjectType);

            if (mActionParameters != null)
            {
                string result = text;
                int index = 0;

                // Process the parameters columns
                foreach (string param in mActionParameters)
                {
                    result = rowView.Row.Table.Columns.Contains(param) ? result.Replace("{" + index + "}", rowView[param].ToString()) : string.Empty;

                    index += 1;
                }

                return result;
            }

            if (rowView.Row.Table.Columns.Contains(text))
            {
                // Try single column value
                return rowView[text].ToString();
            }

            // Return just the existing parameter
            return text;
        }


        /// <summary>
        /// Gets the object type of the parent grid.
        /// </summary>
        private string GetObjectType()
        {
            // Get the object
            GeneralizedInfo infoObj = UniGridControl.InfoObject;

            // Get object type
            string objectType = "";
            if (infoObj != null)
            {
                var typeInfo = infoObj.TypeInfo;

                objectType = typeInfo.IsListingObjectTypeInfo ? typeInfo.OriginalObjectType : typeInfo.ObjectType;
            }

            return objectType;
        }


        /// <summary>
        /// Creates button for given unigrid action.
        /// </summary>
        /// <param name="action">Unigrid action</param>
        /// <param name="container">Control container</param>
        private void CreateButtonAction(ButtonAction action, Control container)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action), "Unigrid action cannot be null");
            }

            var caption = action.CaptionText;
            var safeName = action.SafeName;

            var button = new CMSAccessibleButtonBase
            {
                CommandName = action.Name,
                CommandArgument = action.CommandArgument,
                ToolTip = caption,
                Text = caption,
                ButtonStyle = action.ButtonStyle,
                CssClass = action.ButtonClass
            };

            container.Controls.Add(GetFinalizedButton(button, action, safeName));
        }


        /// <summary>
        /// Creates font icon button for given unigrid action.
        /// </summary>
        /// <param name="action">Unigrid action</param>
        /// <param name="container">Control container</param>
        private void CreateIconAction(Action action, Control container)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action), "Unigrid action cannot be null");
            }

            var caption = action.CaptionText;
            var safeName = action.SafeName;

            var button = new CMSGridActionButton
            {
                CssClass = "js-unigrid-action js-" + safeName,
                CommandName = action.Name,
                CommandArgument = action.CommandArgument,
                ToolTip = caption,
                ScreenReaderDescription = caption,
                IconCssClass = action.FontIconClass,
                IconStyle = action.FontIconStyle
            };

            container.Controls.Add(GetFinalizedButton(button, action, safeName));
        }


        /// <summary>
        /// Creates image button for given unigrid action.
        /// </summary>
        /// <param name="action">Unigrid action</param>
        /// <param name="container">Control container</param>
        private void CreateImageAction(Action action, Control container)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action), "Unigrid action cannot be null");
            }

            var caption = ControlsLocalization.GetString(UniGridControl, String.IsNullOrEmpty(action.Caption) ? "general.action" : LocalizationHelper.GetResourceName(action.Caption));
            var safeName = action.SafeName;

            ImageButton image = new CMSImageButton
            {
                CssClass = "btn-unigrid-action js-unigrid-action",
                CommandName = action.Name,
                CommandArgument = action.CommandArgument,
                ToolTip = caption,
                AlternateText = caption,
                ImageUrl = GetActionImage(action.Icon)
            };

            image.AddCssClass($"js-{safeName}");
            container.Controls.Add(GetFinalizedButton(image, action, safeName));
        }


        /// <summary>
        /// Set attributes to the action control. These attributes will be processed in data bind event a removed from the control.
        /// </summary>
        /// <param name="control">Action control (CMSAccessibleButton or CMSImageButton)</param>
        /// <param name="action">Unigrid action</param>
        private void SetDataBindAttributes(WebControl control, AbstractAction action)
        {
            // Confirmation
            if (action.Confirmation != null)
            {
                // Identifies action for client side validation
                string actionIdentifier = control.ClientID;
                string confirmString = ControlsLocalization.GetString(UniGridControl, LocalizationHelper.GetResourceName(action.Confirmation));
                string confirmScript = $"function UG_Confirm_{actionIdentifier}() {{ return confirm({ScriptHelper.GetString(confirmString)}); }}";

                ScriptHelper.RegisterClientScriptBlock(Page, typeof(Page), "UG_Confirm_" + actionIdentifier, ScriptHelper.GetScript(confirmScript));

                control.Attributes.Add("confirmation", $"UG_Confirm_{actionIdentifier}()");
            }

            // OnClick
            if (action.OnClick != null)
            {
                // Attribute "onclick"
                control.Attributes.Add("onclick", action.OnClick);
            }

            // External source
            if (action.ExternalSourceName != null)
            {
                control.Attributes.Add("externalsourcename", action.ExternalSourceName);
            }
        }


        /// <summary>
        /// Create context menu for unigrid action.
        /// </summary>
        /// <param name="action">Unigrid action</param>
        /// <param name="control">Action control (CMSAccessibleButton or CMSImageButton)</param>
        /// <returns></returns>
        private ContextMenuContainer CreateActionContextMenu(AbstractAction action, WebControl control)
        {
            string safeName = action.SafeName;

            var menuCont = new ContextMenuContainer();
            if (ContextMenuParent != null)
            {
                menuCont.ContextMenuParent = ContextMenuParent;
            }
            menuCont.MenuControlPath = action.ContextMenu;
            menuCont.MenuID = String.Concat(UniGridControl.ControlGUID, "_m", safeName);
            menuCont.MenuParameter = String.Join(";", UniGridControl.ObjectType, UniGridControl.GroupObject, ShowMoveActionsInContextMenu(UniGridControl.GridActions));

            menuCont.ParentElementClientID = UniGridControl.ClientID;

            menuCont.RenderAsTag = HtmlTextWriterTag.A;
            menuCont.VerticalPosition = VerticalPositionEnum.Bottom;
            menuCont.HorizontalPosition = HorizontalPositionEnum.Left;

            menuCont.CssClass = "unigrid-actionmenu";
            menuCont.ActiveItemCssClass = "unigrid-action-menuactive";
            menuCont.DataBinding += BindData;

            // Initialize the menu parameter
            if (action.MenuParameter != null)
            {
                menuCont.Parameter = action.MenuParameter;
            }

            // Initialize the menu parameter
            if (action.MouseButton != null)
            {
                // Initialize the mouse button
                switch (action.MouseButton.ToLowerInvariant())
                {
                    case "left":
                        menuCont.MouseButton = MouseButtonEnum.Left;
                        control.Attributes.Add("onclick", "return false;");
                        break;

                    case "right":
                        menuCont.MouseButton = MouseButtonEnum.Right;
                        break;

                    default:
                        menuCont.MouseButton = MouseButtonEnum.Both;
                        control.Attributes.Add("onclick", "return false;");
                        break;
                }
            }
            else
            {
                menuCont.MouseButton = MouseButtonEnum.Both;
                control.Attributes.Add("onclick", "return false;");
            }

            menuCont.Controls.Add(control);

            // Hide menu container if the menu is hidden
            menuCont.PreRender += ((sender, eventArgs) =>
            {
                menuCont.Visible = (menuCont.MenuControl.ContextMenuControl != null)
                    && menuCont.MenuControl.ContextMenuControl.Visible;
            });

            return menuCont;
        }


        private Control GetFinalizedButton(WebControl button, AbstractAction action, string safeName)
        {
            button.ID = "a" + safeName;
            button.EnableViewState = false;

            SetDataBindAttributes(button, action);
            button.DataBinding += BindData;

            // Context menu
            if (action.ContextMenu == null)
            {
                // Simple action
                return button;
            }
            else
            {
                // Create the context menu container
                return CreateActionContextMenu(action, button);
            }
        }

        #endregion


        #region "ITemplate members"

        /// <summary>
        /// Instantiates the control.
        /// </summary>
        /// <param name="container">Control container</param>
        public void InstantiateIn(Control container)
        {
            switch (templateType)
            {
                case ListItemType.Header:
                    // Header
                    if (mHeaderControl != null)
                    {
                        // Set column control
                        bool isCheckbox = (mHeaderControl.GetType() == typeof(CMSCheckBox));
                        if (isCheckbox)
                        {
                            mHeaderControl.DataBinding += BindCheckData;
                        }
                        container.Controls.Add(mHeaderControl);

                        if (isCheckbox)
                        {
                            // WAI validation
                            LocalizedLabel lblHeader = new LocalizedLabel
                            {
                                EnableViewState = false,
                                Display = false,
                                AssociatedControlID = mHeaderControl.ID,
                                ResourceString = "general.selectall"
                            };
                            container.Controls.Add(lblHeader);
                            ((WebControl)container).CssClass = "unigrid-selection";
                        }
                    }
                    else
                    {
                        // Create context menu
                        if (!string.IsNullOrEmpty(mActions?.ContextMenu))
                        {
                            // Initialize context menu container
                            var menuCont = new ContextMenuContainer();

                            if (ContextMenuParent != null)
                            {
                                menuCont.ContextMenuParent = ContextMenuParent;
                            }

                            menuCont.MenuControlPath = mActions.ContextMenu;
                            menuCont.MenuID = UniGridControl.ControlGUID + "_m" + ValidationHelper.GetIdentifier(mColumnName);
                            menuCont.ParentElementClientID = UniGridControl.ClientID;
                            menuCont.MenuParameter = mActions.AllowExport + "|" + mActions.AllowReset + "|" + mActions.AllowShowFilter;
                            menuCont.RenderAsTag = HtmlTextWriterTag.A;
                            menuCont.MouseButton = MouseButtonEnum.Both;
                            menuCont.VerticalPosition = VerticalPositionEnum.Bottom;
                            menuCont.HorizontalPosition = HorizontalPositionEnum.Left;
                            menuCont.CheckRelative = CheckRelative;
                            const string cssClass = "unigrid-menu-panel";
                            menuCont.CssClass = cssClass;
                            menuCont.ActiveItemCssClass = cssClass;
                            menuCont.ActiveItemInactiveCssClass = cssClass;

                            // Add menu icon
                            menuCont.Controls.Add(new LiteralControl("<i class=\"icon-menu\" aria-hidden=\"true\"></i>"));

                            if (mActions.Actions.FirstOrDefault() is EmptyAction)
                            {
                                menuCont.ToolTip = ControlsLocalization.GetString(UniGridControl, "unigrid.actions");
                            }
                            else
                            {
                                menuCont.Controls.Add(new LiteralControl(mColumnName));
                                menuCont.ToolTip = ControlsLocalization.GetString(UniGridControl, "General.OtherActions");
                            }

                            container.Controls.Add(menuCont);
                        }
                        else
                        {
                            container.Controls.Add(new LiteralControl(mColumnName));
                        }
                    }
                    break;

                case ListItemType.Item:
                    // Item
                    if (mItemControl == null)
                    {
                        // Process all actions
                        foreach (AbstractAction action in mActions.Actions)
                        {
                            if (action.HideIfNotAuthorized)
                            {
                                if (!ActionsAuthorization.TryGetValue(action.Name, out var actionAuthorized))
                                {
                                    // Action haven't yet been checked for authorization
                                    if ((!action.CheckPermissionsAuthorization()) || (!action.CheckUIElementsAuthorization()))
                                    {
                                        // Action is not authorized - do not render it
                                        ActionsAuthorization.Add(action.Name, false);
                                        continue;
                                    }

                                    ActionsAuthorization.Add(action.Name, true);
                                }
                                else
                                {
                                    // Action has already been checked for authorization - use value from cache
                                    if (!actionAuthorized)
                                    {
                                        continue;
                                    }
                                }
                            }

                            if (action is ButtonAction buttonAction)
                            {
                                CreateButtonAction(buttonAction, container);
                            }
                            else if (action is Action act)
                            {
                                // Action
                                if (!String.IsNullOrEmpty(act.FontIconClass))
                                {
                                    CreateIconAction(act, container);
                                }
                                else
                                {
                                    CreateImageAction(act, container);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Set column control
                        Type itemType = mItemControl.GetType();

                        switch (itemType.Name)
                        {
                            case "CMSCheckBox":
                                CMSCheckBox chkBox = new CMSCheckBox();
                                chkBox.EnableViewState = false;
                                chkBox.ID = "s";

                                // Set selection row argument
                                chkBox.Attributes["selectioncolumn"] = ((CMSCheckBox)mItemControl).Attributes["selectioncolumn"];

                                // Databind event to get item id
                                chkBox.DataBinding += BindCheckData;

                                // Add checkbox to unigrid
                                container.Controls.Add(chkBox);

                                LocalizedLabel lblBox = new LocalizedLabel();
                                lblBox.EnableViewState = false;
                                lblBox.Display = false;
                                lblBox.AssociatedControlID = chkBox.ID;
                                lblBox.ResourceString = "general.select";

                                container.Controls.Add(lblBox);
                                ((WebControl)container).CssClass = "unigrid-selection";
                                break;
                        }
                    }
                    break;
            }
        }


        /// <summary>
        /// Indicates whether move up/down actions should be displayed in the object menu.
        /// </summary>
        /// <remarks>
        /// By default move up/down actions are hidden in the object menu.
        /// If grid actions contain drag&amp;drop action and not the move up/down, move up/down actions will be added to the object menu.
        /// </remarks>
        /// <param name="actions">Current grid actions.</param>
        private static bool ShowMoveActionsInContextMenu(UniGridActions actions)
        {
            if (actions == null)
            {
                return false;
            }

            var actionNames = actions.Actions
                .Where(action => !(action is EmptyAction))
                .Select(action => action.Name)
                .ToHashSetCollection(StringComparer.InvariantCultureIgnoreCase);

            return actionNames.Contains("#move") && !actionNames.Contains("#moveup") && !actionNames.Contains("#movedown");
        }

        #endregion
    }
}