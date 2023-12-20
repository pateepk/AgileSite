using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Base.Web.UI;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Server control which represents container which handles object columns customization.
    /// </summary>
    [ToolboxData("<{0}:ObjectCustomizationPanel runat=server></{0}:ObjectCustomizationPanel>")]
    public class ObjectCustomizationPanel : Panel
    {
        #region "Public properties"

        /// <summary>
        /// List of customized columns.
        /// </summary>
        public string[] Columns
        {
            get;
            set;
        }

        /// <summary>
        /// Messages placeholder to use for the messages.
        /// </summary>
        public MessagesPlaceHolder MessagesPlaceHolder
        {
            get;
            set;
        }


        /// <summary>
        /// Header actions to insert the customization button.
        /// </summary>
        public HeaderActions HeaderActions
        {
            get;
            set;
        }


        /// <summary>
        /// Object edit menu to insert the customization button. This control has higher priority than the HeaderActions control if both initialized.
        /// </summary>
        public ObjectEditMenu ObjectEditMenu
        {
            get;
            set;
        }


        /// <summary>
        /// If true, control does not process the data.
        /// </summary>
        public bool StopProcessing
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if current object has customized columns.
        /// </summary>
        public bool IsObjectCustomized
        {
            get
            {
                if (EditedObject != null)
                {
                    if ((Columns != null) && (Columns.Length > 0))
                    {
                        // Object has CustomizedColumns column
                        return EditedObject.CustomizedColumns.Contains(Columns[0]);
                    }
                    else if (EditedObject.TypeInfo.IsCustomColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        // Object has only IsCustom flag
                        return EditedObject.ObjectIsCustom;
                    }
                }

                return false;
            }
        }

        #endregion


        #region "Private properties"
        
        /// <summary>
        /// Edited object under this panel.
        /// </summary>
        private GeneralizedInfo EditedObject
        {
            get
            {
                BaseInfo editedObject = (BaseInfo)UIContextHelper.GetUIContext(this).EditedObject;
                if (editedObject != null)
                {
                    return editedObject.Generalized;
                }

                return null;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// OnLoad event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (StopProcessing || (EditedObject == null))
            {
                return;
            }

            // Register event for customize action
            ComponentEvents.RequestEvents.RegisterForEvent(ComponentEvents.CUSTOMIZE, CustomizeButton_Click);

            if (IsObjectCustomized)
            {
                MessagesPlaceHolder.ShowInformation(ResHelper.GetString(EditedObject.TypeInfo.ObjectType + ".customization.information|customization.information"));
            }
            else
            {
                MessagesPlaceHolder.ShowInformation(ResHelper.GetString(EditedObject.TypeInfo.ObjectType + ".customization.howtocustomize|customization.howtocustomize"));

                if (ObjectEditMenu != null)
                {
                    ObjectEditMenu.AddExtraAction(new CustomizeAction());
                    ObjectEditMenu.AllowCheckIn = false;
                    ObjectEditMenu.AllowCheckOut = false;
                    ObjectEditMenu.AllowSave = false;
                    ObjectEditMenu.AllowUndoCheckOut = false;
                }
                else if (HeaderActions != null)
                {
                    HeaderActions.AddAction(new CustomizeAction());

                    if (HeaderActions != null)
                    {
                        var saveAction = HeaderActions.ActionsList.Find(a => a is SaveAction);
                        if (saveAction != null)
                        {
                            saveAction.Enabled = false;
                        }
                    }
                }

                Enabled = false;
            }
        }


        /// <summary>
        /// CustomizeButton_Click event handler
        /// </summary>
        protected void CustomizeButton_Click(object sender, EventArgs e)
        {
            if (EditedObject == null)
            {
                return;
            }

            if ((Columns != null) && (Columns.Length > 0))
            {
                var customizedColumns = EditedObject.CustomizedColumns.Union(Columns).ToArray();
                EditedObject.CustomizedColumns = new ReadOnlyCollection<string>(customizedColumns);
            }
            else if (EditedObject.TypeInfo.IsCustomColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                EditedObject.ObjectIsCustom = true;
            }
            else
            {
                // Do nothing
                return;
            }

            // Save changes
            EditedObject.SetObject();

            URLHelper.Redirect(RequestContext.URL.AbsoluteUri);
        }

        #endregion
    }
}