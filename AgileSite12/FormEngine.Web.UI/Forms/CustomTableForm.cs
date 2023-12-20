using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Activities;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.CustomTables;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Control to edit custom table record.
    /// </summary>
    public class CustomTableForm : BasicForm
    {
        #region "Constants"

        /// <summary>
        /// Prefix for the default validation error message.
        /// Concatenated with ".errorvalidationerror" resource string.
        /// If not found, general prefix is used.
        /// </summary>
        private const string mErrorMessagePrefix = "customtable";

        #endregion


        #region "Variables"

        // Indicates whether local MessagesPlaceholder control should be added.
        private bool useLocalMessagesPlaceholder;

        // Local messages placeholder instance
        private MessagesPlaceHolder mLocalMessagesPlaceHolder;

        #endregion


        #region "Properties"

        /// <summary>
        /// Edited object.
        /// </summary>
        public new CustomTableItem EditedObject
        {
            get
            {
                if (Data != null)
                {
                    return (CustomTableItem)Data;
                }
                else if (base.EditedObject != null)
                {
                    return (CustomTableItem)base.EditedObject;
                }

                return null;
            }
            set
            {
                Data = value;
                UIContext.EditedObject = value;
                base.EditedObject = value;
            }
        }


        /// <summary>
        /// Custom table id.
        /// </summary>
        [Category("Data"), Description("Custom table id.")]
        public int CustomTableId
        {
            get
            {
                return Convert.ToInt32(ViewState["CustomTableId"]);
            }
            set
            {
                ViewState["CustomTableId"] = value;
            }
        }


        /// <summary>
        /// Primary key value of the item being edited.
        /// </summary>
        public int ItemID
        {
            get
            {
                return Convert.ToInt32(ViewState["ItemID"]);
            }
            set
            {
                ViewState["ItemID"] = value;
            }
        }


        /// <summary>
        /// Alternative form full name (ClassName.AlternativeFormName).
        /// </summary>
        public string AlternativeFormFullName
        {
            get;
            set;
        }


        /// <summary>
        /// Default validation error message.
        /// </summary>
        protected override string DefaultValidationErrorMessage
        {
            get
            {
                return ResHelper.GetString(mErrorMessagePrefix + ".errorvalidationerror|general.errorvalidationerror");
            }
        }


        /// <summary>
        /// Information label.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Label InfoLabel
        {
            get
            {
                return MessagesPlaceHolder.InfoLabel;
            }
        }


        /// <summary>
        /// Label for the errors.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Label ErrorLabel
        {
            get
            {
                return MessagesPlaceHolder.ErrorLabel;
            }
        }


        /// <summary>
        /// Gets the local messages placeholder
        /// </summary>
        private MessagesPlaceHolder LocalMessagesPlaceHolder
        {
            get
            {
                if (mLocalMessagesPlaceHolder == null)
                {
                    mLocalMessagesPlaceHolder = CreateMessagesPlaceHolder();
                    mLocalMessagesPlaceHolder.BasicStyles = IsLiveSite;
                }
                return mLocalMessagesPlaceHolder;
            }
        }


        /// <summary>
        /// Messages placeholder
        /// </summary>
        public override MessagesPlaceHolder MessagesPlaceHolder
        {
            get
            {
                if (base.MessagesPlaceHolder == null)
                {
                    // Use page placeholder only if not used on a live site
                    if (!IsLiveSite)
                    {
                        // Try to get placeholder from siblings
                        if (Parent != null)
                        {
                            ControlCollection siblings = Parent.Controls;
                            foreach (var sib in siblings)
                            {
                                MessagesPlaceHolder mess = sib as MessagesPlaceHolder;
                                if (mess != null)
                                {
                                    base.MessagesPlaceHolder = mess;
                                    return base.MessagesPlaceHolder;
                                }
                            }
                        }

                        // Get page placeholder as default one
                        ICMSPage page = Page as ICMSPage;
                        if (page != null)
                        {
                            base.MessagesPlaceHolder = page.MessagesPlaceHolder;
                        }

                        // Try to get placeholder from parent controls
                        base.MessagesPlaceHolder = ControlsHelper.GetParentProperty<AbstractUserControl, MessagesPlaceHolder>(this, s => s.MessagesPlaceHolder, base.MessagesPlaceHolder);
                    }

                    // Ensure local placeholder
                    if (base.MessagesPlaceHolder == null)
                    {
                        base.MessagesPlaceHolder = LocalMessagesPlaceHolder;
                        useLocalMessagesPlaceholder = true;
                    }
                }

                return base.MessagesPlaceHolder;
            }
            set
            {
                base.MessagesPlaceHolder = value;
            }
        }

        #endregion //Properties


        #region "Life-cycle methods"

        /// <summary>
        /// OnPreRender event handler
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPreRender(EventArgs e)
        {
            // Hide local messages placeholder if is not used and has no text
            if ((StopProcessing || !useLocalMessagesPlaceholder) && !LocalMessagesPlaceHolder.HasText)
            {
                LocalMessagesPlaceHolder.Visible = false;
            }

            base.OnPreRender(e);
        }


        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (Context == null)
            {
                output.Write("[CustomTableForm: {0}]", ID);
            }
            else
            {
                base.Render(output);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the form.
        /// </summary>
        protected override void InitFormInternal()
        {
            base.InitFormInternal();

            // If no custom table defined, do not render anything
            if (CustomTableId <= 0)
            {
                StopProcessing = true;
                return;
            }

            // Get Custom table class            
            ClassInfo = DataClassInfoProvider.GetDataClassInfo(CustomTableId);
            if (ClassInfo == null)
            {
                ShowError(ResHelper.GetString("customtable.selectedtable.noexists"));
                return;
            }

            // Switch control to update mode, when ItemID property contains value greater than zero.
            // Default form mode is 'insert'.
            if (ItemID > 0)
            {
                Mode = FormModeEnum.Update;
            }

            // Get form definition (and alt.form definition if defined)
            // Prepare alt.form name
            string altFormName = AlternativeFormFullName;
            if (string.IsNullOrEmpty(altFormName))
            {
                altFormName = ClassInfo.ClassName;

                // Try to get insert/update alternative forms if defined
                switch (Mode)
                {
                    case FormModeEnum.Insert:
                        altFormName += ".insert";
                        break;

                    case FormModeEnum.Update:
                        altFormName += ".update";
                        break;
                }
            }

            // Get merged form info if found
            FormInformation = FormHelper.GetFormInfo(altFormName, true);
            AltFormInformation = AlternativeFormInfoProvider.GetAlternativeFormInfo(altFormName);

            if (FormInformation == null)
            {
                StopProcessing = true;
                return;
            }

            CustomTableItem item;

            // Load custom table content
            switch (Mode)
            {
                case FormModeEnum.Insert:
                    item = CustomTableItem.New(ClassInfo.ClassName);
                    break;

                case FormModeEnum.Update:
                    // Load existing entry.
                    item = CustomTableItemProvider.GetItem(ItemID, ClassInfo.ClassName);

                    if (item == null)
                    {
                        // Item doesn't exist
                        ShowError(ResHelper.GetString("customtable.selecteditem.noexists"));
                        StopProcessing = true;
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            // Set edited object
            EditedObject = item;

            // Set up basic form
            HtmlAreaToolbar = "BizForm";
        }


        /// <summary>
        /// Allows to add additional components before the form.
        /// </summary>
        protected override void AddControlsBeforeInternal()
        {
            base.AddControlsBeforeInternal();

            // Add messages placeholder
            Controls.Add(LocalMessagesPlaceHolder);
        }


        /// <summary>
        /// Saves data to database.
        /// </summary>
        protected override bool SaveDataInternal()
        {
            base.SaveDataInternal();

            MessagesPlaceHolder.ClearLabels();

            try
            {
                // Get custom table class
                ClassInfo = DataClassInfoProvider.GetDataClassInfo(CustomTableId);
                if (ClassInfo == null)
                {
                    throw new Exception(ResHelper.GetString("customtable.selectedtable.noexists"));
                }

                string className = ClassInfo.ClassName;
                CustomTableItem content;

                // Get previous form content.
                switch (Mode)
                {
                    case FormModeEnum.Insert:
                        content = CustomTableItem.New(className);
                        break;

                    case FormModeEnum.Update:
                        content = CustomTableItemProvider.GetItem(ItemID, className);

                        // Throw an exception when previous content does not exist.
                        if (content == null)
                        {
                            throw new Exception(ResHelper.GetString("customtable.selecteditem.noexists"));
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }

                // Get current form content
                foreach (string columnName in Data.ColumnNames)
                {
                    // Set unspecific fields
                    content.SetValue(columnName, Data.GetValue(columnName));
                }

                // Process files - not implemented yet
                //ProcessFiles(content);

                string activityTitle;

                if (Mode == FormModeEnum.Insert)
                {
                    // Ensure correct order
                    if (content.OrderEnabled)
                    {
                        content.ItemOrder = CustomTableItemProvider.GetLastItemOrder(className) + 1;
                    }

                    // Insert a new content.
                    content.Insert();

                    activityTitle = ResHelper.GetString("customtable.activitynew");

                    // Set new item ID to the form
                    ItemID = content.ItemID;
                    Data.SetValue("ItemID", ItemID);
                }
                else
                {
                    // Update existing content.
                    content.Update();

                    activityTitle = ResHelper.GetString("customtable.activityedit");
                }

                LogActivity(content.ItemID, activityTitle);

                // Update the edited object in context is saved successfully
                EditedObject = content;

                return true;
            }
            catch (Exception ex)
            {
                ShowError(ResHelper.GetString("CustomTable.ErrorDuringSave"), ex.Message);
            }

            return false;
        }


        /// <summary>
        /// Logs activity
        /// </summary>
        /// <param name="itemID">Custom table item identifier</param>
        /// <param name="activityTitle">Activity title</param>
        private void LogActivity(int itemID, string activityTitle)
        {
            var activityInitializer = new CustomTableFormSubmitActivityInitializer(ClassInfo, itemID, activityTitle);
            Service.Resolve<IActivityLogService>().Log(activityInitializer, CMSHttpContext.Current.Request);
        }

        #endregion
    }
}