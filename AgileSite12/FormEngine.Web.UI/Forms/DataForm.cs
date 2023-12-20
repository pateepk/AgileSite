using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Common form control to edit the data.
    /// </summary>
    public class DataForm : BasicForm
    {
        #region "Delegates"

        /// <summary>
        /// Represents the method that will handle the file processing event.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="editedObject">Depends on edited object - could be AbstractInfo or IDataClass type</param>
        /// <param name="uploadedFiles">Array with IDs of uploaders where a file was uploaded</param>
        /// <param name="deletedFiles">Array with IDs of uploaders where a file was deleted</param>
        public delegate void OnFileProcessingEventHandler(object sender, object editedObject, List<string> uploadedFiles, List<string> deletedFiles);

        #endregion


        #region "Events"

        /// <summary>
        /// Occurs during saving when uploaded or deleted files should be processed.
        /// </summary>
        public event OnFileProcessingEventHandler OnFileProcessing;

        #endregion


        #region "Constants"

        /// <summary>
        /// Prefix for the default validation error message.
        /// Concatenated with ".errorvalidationerror" resource string.
        /// If not found, general prefix is used.
        /// </summary>
        private const string mErrorMessagePrefix = "dataform";

        #endregion


        #region "Variables"

        // Indicates whether local MessagesPlaceholder control should be added.
        private bool useLocalMessagesPlaceholder;

        // messages placeholder instance
        private MessagesPlaceHolder mMessagesPlaceHolder;

        /// <summary>
        /// List of files (controls) to upload files.
        /// </summary>
        protected List<string> uploadFiles = new List<string>();


        /// <summary>
        /// List of files (controls) to delete files.
        /// </summary>
        protected List<string> deleteFiles = new List<string>();


        /// <summary>
        /// Info object.
        /// </summary>
        protected BaseInfo mInfo;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the messages placeholder
        /// </summary>
        private MessagesPlaceHolder LocalMessagesPlaceHolder 
        {
            get
            {
                if (mMessagesPlaceHolder == null)
                {
                    mMessagesPlaceHolder = CreateMessagesPlaceHolder();
                }
                return mMessagesPlaceHolder;
            }
        }


        /// <summary>
        /// Class name.
        /// </summary>
        [Category("Data"), Description("Class name.")]
        public string ClassName
        {
            get
            {
                return Convert.ToString(ViewState["ClassName"]);
            }
            set
            {
                ViewState["ClassName"] = value;
            }
        }


        /// <summary>
        /// ID value of the edited record. When value is greater than zero, FormMode is switched to update mode.
        /// </summary>
        [Category("Data"), Description("ID value of the edited record. When value is greater than zero, FormMode is switched to update mode.")]
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
        /// Info object, usable instead of ClassName and ItemID.
        /// </summary>
        public BaseInfo Info
        {
            get
            {
                return mInfo;
            }
            set
            {
                mInfo = value;
            }
        }


        /// <summary>
        /// Determines whether form values have to be cleared after successful saving of the content.
        /// </summary>
        [Category("Behavior"), Description("Determines whether form values have to be cleared after successful saving of the content.")]
        public bool ClearAfterSave
        {
            get
            {
                return Convert.ToBoolean(ViewState["ClearAfterSave"]);
            }
            set
            {
                ViewState["ClearAfterSave"] = value;
            }
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

        #endregion


        #region "Life-cycle methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public DataForm()
        {
            OnUploadFile += DataForm_OnUploadFile;
            OnDeleteFile += DataForm_OnDeleteFile;
        }


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
                output.Write("[DataForm: {0}]", ID);
            }
            else
            {
                base.Render(output);
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Saves basicform content to database.
        /// </summary>
        public bool Save()
        {
            return SaveData(null);
        }


        /// <summary>
        /// Reloads the form.
        /// </summary>
        public override void ReloadData()
        {
            InitForm();

            base.ReloadData();
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Initializes the form.
        /// </summary>
        protected override void InitFormInternal()
        {
            base.InitFormInternal();

            InitForm();
        }


        /// <summary>
        /// Initializes the form properties.
        /// </summary>
        protected void InitForm()
        {
            // Do not load if data not specified
            if ((Info == null) && string.IsNullOrEmpty(ClassName))
            {
                StopProcessing = true;
                return;
            }

            if (Info != null)
            {
                // Get dataclass info from info object
                ClassInfo = DataClassInfoProvider.GetDataClassInfo(Info.TypeInfo.ObjectClassName);

                // Switch control to update mode, when ObjectID property contains value greater than zero
                // Default form mode is 'insert'
                if (Info.Generalized.ObjectID > 0)
                {
                    Mode = FormModeEnum.Update;
                }
            }
            else
            {
                ClassInfo = DataClassInfoProvider.GetDataClassInfo(ClassName);

                // Switch control to update mode, when ItemID property contains value greater than zero
                if (ItemID > 0)
                {
                    Mode = FormModeEnum.Update;
                }
            }

            if (ClassInfo == null)
            {
                StopProcessing = true;
                return;
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

            // Check if any FormInfo found
            if (FormInformation == null)
            {
                StopProcessing = true;
                return;
            }

            // Set up basic form
            ApplyVisibility = ApplyVisibility && (ClassInfo.ClassName.EqualsCSafe("cms.user", true));

            // Set edited object (AbstractInfo or DataClassInfo)
            EditedObject = Info ?? ClassInfo;

            Data = GetData();
        }


        /// <summary>
        /// Returns datarow with the help of ClassName and ItemID or from Info object.
        /// </summary>
        protected IDataContainer GetData()
        {
            IDataContainer data;

            // Get datarow from info object
            if (Info != null)
            {
                data = Info;
            }
            // Get datarow of class specified by ClassName and ItemID
            else
            {
                IDataClass formItem;

                // Load form content.
                switch (Mode)
                {
                    case FormModeEnum.Insert:
                        // Load empty data row.
                        formItem = DataClassFactory.NewDataClass(ClassName);
                        data = formItem;
                        break;

                    case FormModeEnum.Update:
                        // Load existing entry.
                        formItem = DataClassFactory.NewDataClass(ClassName, ItemID);
                        if (!formItem.IsEmpty())
                        {
                            data = formItem;
                        }
                        else
                        {
                            ShowError("Form content which is intended to update does not exist.");
                            return null;
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            return data;
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


        private void DataForm_OnUploadFile(object sender, EventArgs e)
        {
            Uploader ctrlUploader = (Uploader)sender;
            if (!uploadFiles.Contains(ctrlUploader.ID))
            {
                uploadFiles.Add(ctrlUploader.ID);
            }
        }


        private void DataForm_OnDeleteFile(object sender, EventArgs e)
        {
            Uploader ctrlUploader = (Uploader)sender;
            if (!deleteFiles.Contains(ctrlUploader.ID))
            {
                // Add control name to List<string> of deleted files
                deleteFiles.Add(ctrlUploader.ID);
                // Clear CurrentFileName and CurrentFileUrl in uploader
                ctrlUploader.Clear();
            }
        }


        /// <summary>
        /// Saves data to database. It's called after form data are retrieved and validated.
        /// </summary>
        protected override bool SaveDataInternal()
        {
            base.SaveDataInternal();

            MessagesPlaceHolder.ClearLabels();

            try
            {
                IDataClass content = null;

                if (Info == null)
                {
                    // Get previous form content
                    switch (Mode)
                    {
                        case FormModeEnum.Insert:
                            content = DataClassFactory.NewDataClass(ClassName);
                            break;

                        case FormModeEnum.Update:
                            content = DataClassFactory.NewDataClass(ClassName, ItemID);

                            // Throw an exception when previous content does not exist
                            if (content.IsEmpty())
                            {
                                ShowError("Form content which is intended to update does not exist.");
                                return false;
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

                // Get current form content
                if (Info != null)
                {
                    // Info object
                    foreach (string columnName in Data.ColumnNames)
                    {
                        Info.SetValue(columnName, Data.GetValue(columnName));
                    }
                }
                else
                {
                    // Other content
                    foreach (string columnName in Data.ColumnNames)
                    {
                        content.SetValue(columnName, Data.GetValue(columnName));
                    }
                }

                // Process files
                // Uploaded and deleted files have to be processed by parent page/control
                if (OnFileProcessing != null)
                {
                    if (Info != null)
                    {
                        OnFileProcessing(this, Info, uploadFiles, deleteFiles);
                    }
                    else
                    {
                        OnFileProcessing(this, content, uploadFiles, deleteFiles);
                    }
                }

                if (Info != null)
                {
                    // Update the info object
                    Info.Generalized.SetObject();
                }
                else
                {
                    if (Mode == FormModeEnum.Insert)
                    {
                        // Insert a new content
                        content.Insert();
                    }
                    else
                    {
                        // Update existing content
                        content.Update();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                EventLogProvider.LogException("DataForm", "Saving failed", ex);

                // Show error message
                ShowError(ResHelper.GetString("General.ErrorDuringSave"));
            }

            return false;
        }


        /// <summary>
        /// Executes additional actions after successful save.
        /// </summary>
        /// <param name="redirectUrlAfterSave">Target URL for the final redirection</param>
        /// <param name="showChangesSaved">Indicates if info message should be displayed (redirect URL should be empty)</param>
        protected override void AfterSaveActionsInternal(string redirectUrlAfterSave, bool showChangesSaved)
        {
            if (!ClearAfterSave)
            {
                Data = GetData();
            }
            else
            {
                // Clear form fields
                if (Info != null)
                {
                    var genInfo = Info.Generalized;

                    genInfo.ObjectID = 0;
                    genInfo.ObjectGUID = Guid.Empty;
                    genInfo.ObjectLastModified = DateTimeHelper.ZERO_TIME;
                }
                else
                {
                    ItemID = 0;
                }

                // Set insert mode and reload the form
                Mode = FormModeEnum.Insert;
                ReloadData();
            }

            base.AfterSaveActionsInternal(redirectUrlAfterSave, showChangesSaved);
        }

        #endregion
    }
}