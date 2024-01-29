using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Base.Web.UI;
using CMS.FormEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.IO;
using CMS.MacroEngine;
using CMS.Protection;
using CMS.SiteProvider;

using Attachment = System.Net.Mail.Attachment;

namespace CMS.OnlineForms.Web.UI
{
    /// <summary>
    /// Basic bizform engine independent on CMS.
    /// </summary>
    [ToolboxData("<{0}:BizForm runat=\"server\"></{0}:BizForm>")]
    public class BizForm : BasicForm
    {
        #region "Constants"

        /// <summary>
        /// Prefix for the default validation error message.
        /// Concatenated with ".errorvalidationerror" resource string.
        /// If not found, general prefix is used.
        /// </summary>
        private const string mErrorMessagePrefix = "bizform";

        #endregion


        #region "Variables"

        /// <summary>
        /// List of files (controls) to upload files.
        /// </summary>
        protected List<string> uploadFiles = new List<string>();


        /// <summary>
        /// List of files (controls) to delete files.
        /// </summary>
        protected List<string> deleteFiles = new List<string>();


        /// <summary>
        /// Regular expression for macros in e-mail body.
        /// </summary>
        private static Regex mRegExEmailMacro;


        /// <summary>
        /// Indicates if the form should be hidden.
        /// </summary>
        protected bool hideForm;


        /// <summary>
        /// Indicates if notification e-mail should be sent.
        /// </summary>
        protected bool mEnableNotificationEmail = true;


        /// <summary>
        /// Indicates if autoresponder e-mail should be sent.
        /// </summary>
        protected bool mEnableAutoresponder = true;


        /// <summary>
        /// Indicates whether original file name should be displayed instead of the file GUID in 'Uploader' controls.
        /// </summary>
        private bool mDisplayOriginalFileName = true;


        /// <summary>
        /// Indicates whether local MessagesPlaceholder control should be added.
        /// </summary>
        private bool mUseLocalMessagesPlaceholder;


        /// <summary>
        /// Local messages placeholder.
        /// </summary>
        private MessagesPlaceHolder mLocalMessagesPlaceHolder;


        /// <summary>
        /// Bizform ID of currently loaded bizform.
        /// </summary>
        protected int mBizFormID;


        /// <summary>
        /// BizForm info object.
        /// </summary>
        private BizFormInfo bizFormInfo;


        /// <summary>
        /// Bizform item.
        /// </summary>
        protected BizFormItem content;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the local messages placeholder.
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
        /// Regular expression for macros in e-mail body, macros are in form $$type:fieldname$$.
        /// </summary>
        protected static Regex RegExEmailMacro
        {
            get
            {
                return mRegExEmailMacro ?? (mRegExEmailMacro = RegexHelper.GetRegex("\\$\\$\\w+:(?:[A-Za-z]|_[A-Za-z])\\w*\\$\\$"));
            }
            set
            {
                mRegExEmailMacro = value;
            }
        }


        /// <summary>
        /// BizForm name.
        /// </summary>
        [Category("Data"), Description("BizForm name.")]
        public string FormName
        {
            get
            {
                return Convert.ToString(ViewState["FormName"]);
            }
            set
            {
                ViewState["FormName"] = value;
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
        /// Indicates whether to show private fields or not.
        /// </summary>
        public override bool ShowPrivateFields
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowPrivateFields"], false);
            }
            set
            {
                ViewState["ShowPrivateFields"] = value;
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
        /// Indicates if notification and confirmation e-mails should be encoded, default value is 'false'.
        /// </summary>
        public bool EncodeEmails
        {
            get;
            set;
        }


        /// <summary>
        /// If TRUE then notification e-mail will be send. The notification e-mail has to be defined. Default value is TRUE.
        /// </summary>
        public bool EnableNotificationEmail
        {
            get
            {
                return mEnableNotificationEmail;
            }
            set
            {
                mEnableNotificationEmail = value;
            }
        }


        /// <summary>
        /// If TRUE then autoresponder e-mail will be send. The autoresponder has to be defined. Default value is TRUE.
        /// </summary>
        public bool EnableAutoresponder
        {
            get
            {
                return mEnableAutoresponder;
            }
            set
            {
                mEnableAutoresponder = value;
            }
        }


        /// <summary>
        /// If set, overrides settings in BizFormInfo.FormRedirectToUrl.
        /// </summary>
        public string FormRedirectToUrl
        {
            get;
            set;
        }


        /// <summary>
        /// If set, overrides settings in BizFormInfo.FormDisplayText.
        /// </summary>
        public string FormDisplayText
        {
            get;
            set;
        }


        /// <summary>
        /// If set, overrides settings in BizFormInfo.FormClearAfterSave.
        /// </summary>
        public bool? FormClearAfterSave
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether original file name should be displayed instead of the file GUID in 'Uploader' controls.
        /// </summary>
        public bool DisplayOriginalFileName
        {
            get
            {
                return mDisplayOriginalFileName;
            }
            set
            {
                mDisplayOriginalFileName = value;
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
                        mUseLocalMessagesPlaceholder = true;
                    }
                }

                return base.MessagesPlaceHolder;
            }
            set
            {
                base.MessagesPlaceHolder = value;
            }
        }

        #endregion


        #region "Lifecycle methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public BizForm()
        {
            DefaultFormLayout = FormLayoutEnum.SingleTable;

            // Register events.
            OnBeforeValidate += BizForm_OnBeforeValidate;
            OnUploadFile += BizForm_OnUploadFile;
            OnDeleteFile += BizForm_OnDeleteFile;
        }


        /// <summary>
        /// OnPreRender event handler.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPreRender(EventArgs e)
        {
            // Hide local messages placeholder if is not used and has no text
            if ((StopProcessing || !mUseLocalMessagesPlaceholder) && !LocalMessagesPlaceHolder.HasText)
            {
                LocalMessagesPlaceHolder.Visible = false;
            }

            base.OnPreRender(e);

            if (hideForm)
            {
                Controls.Clear();
                Controls.Add(MessagesPlaceHolder);
            }
        }


        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (Context == null)
            {
                output.Write("[BizForm: {0}]", ID);
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

            InitForm();
        }


        /// <summary>
        /// Initializes the form properties and prepares the data to load.
        /// </summary>
        protected void InitForm()
        {
            // Do not show anything if form not defined
            if (string.IsNullOrEmpty(FormName))
            {
                StopProcessing = true;
                return;
            }

            // Get BizFormInfo and DataClassInfo
            BizFormInfo bfi = BizFormInfoProvider.GetBizFormInfo(FormName, SiteName);
            if (bfi == null)
            {
                ShowError("Required form '" + HTMLHelper.HTMLEncode(FormName) + "' does not exist.");
                return;
            }
            mBizFormID = bfi.FormID;
            ClassInfo = DataClassInfoProvider.GetDataClassInfo(bfi.FormClassID);

            // Switch control to update mode, when ItemID property contains value greater than zero.
            // Default form mode is 'insert'.
            if (ItemID > 0)
            {
                Mode = FormModeEnum.Update;
            }

            // Get form definition (and alt.form definition if defined)
            // Prepare alt.form name
            string altFormName = AlternativeFormFullName;
            if (String.IsNullOrEmpty(altFormName))
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

            BizFormItem formItem;

            // Load BizForm dataItem
            switch (Mode)
            {
                case FormModeEnum.Insert:
                    // Load empty data row
                    formItem = BizFormItem.New(ClassInfo.ClassName);
                    break;

                case FormModeEnum.Update:
                    // Load existing entry.
                    formItem = BizFormItemProvider.GetItem(ItemID, ClassInfo.ClassName);

                    if (formItem == null)
                    {
                        ShowError("Form dataItem which is intended to update does not exist.");
                        return;
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            // Set edited object and data for form
            EditedObject = Data = formItem;

            // Set up basic form
            HtmlAreaToolbar = "BizForm";

            // Use full URL for all links if the site is not current
            if (SiteName != SiteContext.CurrentSiteName)
            {
                DialogParameters = "fullurl=true";
            }

            // Prepare submit text
            string submitText = null;
            if (!string.IsNullOrEmpty(bfi.FormSubmitButtonText))
            {
                var resolver = ContextResolver.CreateChild();
                resolver.Culture = Thread.CurrentThread.CurrentCulture.ToString();

                submitText = resolver.ResolveMacros(bfi.FormSubmitButtonText);
            }

            // Init submit button
            if (!string.IsNullOrEmpty(bfi.FormSubmitButtonImage))
            {
                // Image button
                SubmitImageButton.ImageUrl = bfi.FormSubmitButtonImage;

                if (submitText != null)
                {
                    SubmitImageButton.AlternateText = submitText;
                    SubmitImageButton.ToolTip = submitText;
                }
            }
            else
            {
                // Standard button
                if (submitText != null)
                {
                    SubmitButton.ResourceString = submitText;
                }
            }
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
        /// On before validate event handler.
        /// </summary>
        protected void BizForm_OnBeforeValidate(object sender, EventArgs e)
        {
            // Check banned ip
            if (!BannedIPInfoProvider.IsAllowed(SiteName, BanControlEnum.AllNonComplete))
            {
                ShowError(ResHelper.GetString("General.BannedIP"));
                StopProcessing = true;
            }
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
                // Get BizFormInfo
                bizFormInfo = BizFormInfoProvider.GetBizFormInfo(FormName, SiteName);
                if (bizFormInfo == null)
                {
                    throw new Exception("Required form '" + FormName + "' does not exist.");
                }

                // Get DataClassInfo
                ClassInfo = DataClassInfoProvider.GetDataClassInfo(bizFormInfo.FormClassID);

                // Get previous form dataItem.
                switch (Mode)
                {
                    case FormModeEnum.Insert:
                        content = BizFormItem.New(ClassInfo.ClassName);
                        break;

                    case FormModeEnum.Update:
                        content = BizFormItemProvider.GetItem(ItemID, ClassInfo.ClassName);

                        // Throw an exception when previous dataItem does not exist.
                        if (content == null)
                        {
                            throw new Exception("Form item which is intended to update does not exist.");
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }

                // Get current form dataItem.
                foreach (string columnName in Data.ColumnNames)
                {
                    content.SetValue(columnName, Data.GetValue(columnName));
                }

                // Process files.
                ProcessFiles(content);

                if (Mode == FormModeEnum.Insert)
                {
                    // Insert a new dataItem.
                    content.Insert();

                    // Set new item ID to the form
                    ItemID = content.ItemID;
                    Data.SetValue(content.TypeInfo.IDColumn, content.ItemID);
                }
                else
                {
                    // Update existing dataItem.
                    content.Update();
                }

                Data.SetValue("FormInserted", content.FormInserted);
                Data.SetValue("FormUpdated", content.FormUpdated);

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                EventLogProvider.LogException("BizForm", "Saving failed", ex);

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
            // Get confirmation sender and recipient email addresses
            object email = content.GetValue(bizFormInfo.FormConfirmationEmailField);
            string sendConfirmEmailTo = ValidationHelper.GetString(email, "");
            string sendConfirmEmailFrom = bizFormInfo.FormConfirmationSendFromEmail;

            // Get sender and recipient email addresses
            string sendFromEmail = bizFormInfo.FormSendFromEmail;
            string sendToEmail = bizFormInfo.FormSendToEmail;

            bool sendEmailtoAdmin = (!String.IsNullOrEmpty(sendFromEmail)) && (!String.IsNullOrEmpty(sendToEmail));
            bool sendEmailtoCustomer = (!String.IsNullOrEmpty(sendConfirmEmailFrom)) && (!String.IsNullOrEmpty(sendConfirmEmailTo));

            if (sendEmailtoAdmin || sendEmailtoCustomer)
            {
                if (sendEmailtoAdmin && EnableNotificationEmail)
                {
                    try
                    {
                        // Send notification e-mail
                        SendNotificationEmail(sendFromEmail, sendToEmail, content, bizFormInfo);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("BizForm", "Sending notification failed", ex);
                    }
                }

                if (sendEmailtoCustomer && EnableAutoresponder)
                {
                    try
                    {
                        // Send confirmation e-mail
                        SendConfirmationEmail(sendConfirmEmailFrom, sendConfirmEmailTo, content, bizFormInfo);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("BizForm", "Sending autoresponder failed", ex);
                    }
                }
            }

            // Reinitialize resolver's named source data to contain values from the current biz form item, not the default ones
            HandleFieldsValues(false);
            
            // Following fields are not initialized from the form controls in the method above, needs to be added manually
            ContextResolver.SetNamedSourceData("formUpdated", content.FormUpdated);
            ContextResolver.SetNamedSourceData("formInserted", content.FormInserted);
            
            // Display predefined text.
            string displayText = FormDisplayText ?? ResHelper.LocalizeString(bizFormInfo.FormDisplayText);
            if (!String.IsNullOrEmpty(displayText))
            {
                // Resolve the macros
                displayText = ContextResolver.ResolveMacros(displayText);
                
                // Show the text
                ShowConfirmation(displayText);

                // This will cause that the form will be hidden on pre-render
                hideForm = true;
                ItemID = 0;
                Mode = FormModeEnum.Insert;

                // Initialize control values
                Data = null;
                LoadControlValues();
                return;
            }

            // Get redirection url
            string redirectUrl = redirectUrlAfterSave ?? FormRedirectToUrl ?? bizFormInfo.FormRedirectToUrl;
    
            // If no redirect, reload data to properly change form mode based on ItemID
            if (String.IsNullOrEmpty(redirectUrl))
            {
                // Clear form fields
                if (FormClearAfterSave ?? bizFormInfo.FormClearAfterSave)
                {
                    ItemID = 0;
                    Mode = FormModeEnum.Insert;
                }
                ReloadData();
            }

            // Handles possible redirect based on property RedirectUrlAfterSave and handles information info
            base.AfterSaveActionsInternal(redirectUrl, showChangesSaved);
        }


        /// <summary>
        /// Reloads the form and its data.
        /// </summary>
        public override void ReloadData()
        {
            // Before reloading the form, data must be loaded depending on ItemID
            InitForm();

            base.ReloadData();
        }

        #endregion


        #region "Attachment methods"

        /// <summary>
        /// Handles file upload
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        protected void BizForm_OnUploadFile(object sender, EventArgs e)
        {
            Uploader ctrlUploader = (Uploader)sender;
            if (!uploadFiles.Contains(ctrlUploader.ID))
            {
                uploadFiles.Add(ctrlUploader.ID);
            }
        }


        /// <summary>
        /// Handles file deletion
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        protected void BizForm_OnDeleteFile(object sender, EventArgs e)
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
        /// Proceeds the file operations on the bizform.
        /// </summary>
        /// <param name="dataItem">Updated BizFrom item</param>
        private void ProcessFiles(BizFormItem dataItem)
        {
            // Path to BizForm files in file system.
            string filesFolderPath = FormHelper.GetBizFormFilesFolderPath(SiteName);
            string fileName;
            string fileNameString;
            string fileExtension;
            string newExtension;
            string inputID;
            FormEngineUserControl control;

            // Delete files which are in List<string> 'deleteFiles'.
            foreach (string deleteColumn in deleteFiles)
            {
                // Find uploader in bizform and clear it.
                control = FieldControls[deleteColumn];
                if (control != null)
                {
                    inputID = control.InputControlID;
                    IUploaderControl ctrlUploader = (IUploaderControl)control.FindControl(inputID);

                    ctrlUploader?.Clear();
                }

                fileNameString = Convert.ToString(dataItem.GetValue(deleteColumn));
                fileName = FormHelper.GetGuidFileName(fileNameString);
                if (!string.IsNullOrEmpty(fileName))
                {
                    // Delete file from file system.
                    DeleteFile(fileName, filesFolderPath, SiteName);

                    // Update DataClass dataItem
                    dataItem.SetValue(deleteColumn, null);
                    Data.SetValue(deleteColumn, null);
                }
            }

            // Create or update files which are in List<string> 'uploadedFiles'.
            foreach (string uploadColumn in uploadFiles)
            {
                // Use transaction when working with file
                using (var tr = new CMSTransactionScope())
                {
                    // Find uploader in bizform.
                    control = FieldControls[uploadColumn];
                    if (control != null)
                    {
                        inputID = control.InputControlID;
                        Uploader uploader = (Uploader)control.FindControl(inputID);

                        if (uploader?.PostedFile != null)
                        {
                            // Get extension from posted file.
                            newExtension = Path.GetExtension(uploader.PostedFile.FileName);

                            // Get fileName from DataClass or create new one (file name = guid + extension).
                            if (!DataHelper.IsEmpty(dataItem.GetValue(uploadColumn)))
                            {
                                fileNameString = Convert.ToString(dataItem.GetValue(uploadColumn));
                                fileName = FormHelper.GetGuidFileName(fileNameString);
                                // Check file extension
                                if (fileName.Contains("."))
                                {
                                    fileExtension = fileName.Substring(fileName.LastIndexOf(".", StringComparison.Ordinal) + 1);
                                    if (!newExtension.Contains(fileExtension))
                                    {
                                        // Delete old file
                                        DeleteFile(fileName, filesFolderPath, SiteName);
                                        
                                        // Replace extensions
                                        fileName = fileName.Substring(0, fileName.LastIndexOf(".", StringComparison.Ordinal));
                                        if (!ValidationHelper.IsGuid(fileName))
                                        {
                                            fileName = Guid.NewGuid().ToString();
                                        }

                                        fileName += "." + newExtension.TrimStart('.');
                                    }
                                }
                                // Existing file name is in wrong format, create new one
                                else
                                {
                                    fileName = GetNewGuidName(newExtension);
                                }
                            }
                            // No file name created yet
                            else
                            {
                                fileName = GetNewGuidName(newExtension);
                            }

                            fileNameString = fileName + "/" + Path.GetFileName(uploader.PostedFile.FileName);

                            // Save file to file system.
                            SaveFileToDisk(uploader.PostedFile, fileName, filesFolderPath, uploader.ResizeToWidth, uploader.ResizeToHeight, uploader.ResizeToMaxSideSize, SiteName);
                            // Update DataClass dataItem
                            dataItem.SetValue(uploadColumn, fileNameString);
                            Data.SetValue(uploadColumn, fileNameString);
                            // Update uploader.
                            uploader.CurrentFileName = GetFileNameForUploader(fileNameString);
                            uploader.CurrentFileUrl = "~/CMSPages/GetBizFormFile.aspx?filename=" + fileName + "&sitename=" + SiteName;

                            // Close and dispose stream
                            uploader.PostedFile.InputStream.Close();
                        }
                    }
                    tr.Commit();
                }
            }
        }


        /// <summary>
        /// Returns GUID.extension for newly created files.
        /// </summary>
        private static string GetNewGuidName(string extension)
        {
            string fileName = Guid.NewGuid().ToString();
            // Add extension to the file name
            if (!string.IsNullOrEmpty(extension))
            {
                fileName = string.Format("{0}.{1}", fileName, extension.TrimStart('.'));
            }

            return fileName;
        }


        /// <summary>
        /// Returns file name that will be displayed by the Uploader control. Return value depends on property <see cref="DisplayOriginalFileName"/>.
        /// </summary>
        /// <param name="fileNameString">File name from database in format "[guid].[extension]/[originalfilename].[extension]"</param>
        public string GetFileNameForUploader(string fileNameString)
        {
            if (DisplayOriginalFileName)
            {
                return FormHelper.GetOriginalFileName(fileNameString);
            }
            return FormHelper.GetGuidFileName(fileNameString);
        }


        /// <summary>
        /// Save uploaded file to file system.
        /// </summary>
        /// <param name="postedFile">Posted file</param>
        /// <param name="fileName">File name</param>        
        /// <param name="filesFolderPath">Directory path where the file is saved to</param>
        /// <param name="widht">Image required widht</param>
        /// <param name="height">Image required height</param>
        /// <param name="maxSideSize">Image required max side size</param>
        /// <param name="siteName">Site name</param>
        private static void SaveFileToDisk(HttpPostedFile postedFile, string fileName, string filesFolderPath, int widht, int height, int maxSideSize, string siteName)
        {
            // Get file size and path
            string filePath = filesFolderPath + fileName;

            // Ensure disk path
            DirectoryHelper.EnsureDiskPath(filePath, SystemContext.WebApplicationPhysicalPath);

            string extension = Path.GetExtension(postedFile.FileName);
            // Images -> save as binary
            if (ImageHelper.IsImage(extension))
            {
                // Resize image if required
                byte[] imageBinary = ImageHelper.GetResizedImageData(postedFile, widht, height, maxSideSize);

                StorageHelper.SaveFileToDisk(filePath, imageBinary);
            }
            // Other files -> save as stream
            else
            {
                StorageHelper.SaveFileToDisk(filePath, postedFile.InputStream, false);
            }

            // Check synchronization max. file size
            if (WebFarmHelper.WebFarmEnabled)
            {
                WebFarmHelper.CreateIOTask(FormTaskType.UpdateBizFormFile, filePath, postedFile.InputStream, "updatebizformfile", siteName, fileName);
            }
        }


        /// <summary>
        /// Delete uploaded file from file system.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="directoryPath">Directory path</param>
        /// <param name="siteName">Site name</param>
        private static void DeleteFile(string fileName, string directoryPath, string siteName)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            DirectoryInfo di = DirectoryInfo.New(directoryPath);

            // Select all files with the same name ( '<fileName>' ).
            FileInfo[] filesInfos = di.GetFiles(fileName);
            if (filesInfos == null)
            {
                return;
            }

            // Delete all selected files.
            foreach (FileInfo file in filesInfos)
            {
                File.Delete(file.FullName);
            }

            WebFarmHelper.CreateTask(FormTaskType.DeleteBizFormFile, "deletebizformfile", siteName, fileName);
        }

        #endregion


        #region "Mailing methods (Notification and autorespond e-mails)"

        /// <summary>
        /// Sends notification email to specified person.
        /// </summary>
        /// <param name="fromEmail">From email address</param>
        /// <param name="recipients">Email address of recipients where notification emails are sent. You can specify multiple addresses separated by semicolons.</param>
        /// <param name="data">Data from BasicForm</param>
        /// <param name="bfi">BizFormInfo</param>
        public virtual void SendNotificationEmail(string fromEmail, string recipients, IDataContainer data, BizFormInfo bfi)
        {
            if ((fromEmail != null) && (recipients != null) && (data != null) && (bfi != null))
            {
                EnsureDataForSending(bfi);
                if (FormInformation == null)
                {
                    return;
                }

                // Prepare the resolver
                MacroResolver resolver = MacroResolver.GetInstance();
                resolver.SetAnonymousSourceData(data);
                resolver.Settings.EncodeResolvedValues = EncodeEmails;

                // Localization macros wouldn't be correctly resolved without setting current culture code
                resolver.Culture = CultureHelper.GetPreferredCulture();

                // Merge email addresses and subject with entered data
                fromEmail = resolver.ResolveMacros(fromEmail);
                recipients = resolver.ResolveMacros(recipients);

                if (!ValidationHelper.AreEmails(recipients))
                {
                    throw new ArgumentException("At least one of the specified recipient's email addresses is not valid.");
                }

                // Set default email subject
                string subject = ResHelper.GetString("BizForm.MessageSubject") + " - " + bfi.FormDisplayName;

                if (!DataHelper.IsEmpty(bfi.FormEmailSubject))
                {
                    // Set predefined email subject
                    subject = resolver.ResolveMacros(bfi.FormEmailSubject);
                }

                string emailBody;
                // Get form info with alternative form changes (if alt.form used)
                FormInfo formInfo = FormInformation;

                // Create default email body layout
                if (string.IsNullOrEmpty(bfi.FormEmailTemplate))
                {
                    var columnNames = formInfo.GetColumnNames();
                    emailBody = CreateMessageWithDefaultLayout(data, new FormFieldInfoProvider(formInfo), resolver, columnNames);
                }
                else
                {
                    // Resolve context macros in custom layout and include form data
                    emailBody = ResolveEmailMessageText(resolver.ResolveMacros(bfi.FormEmailTemplate), new FormFieldInfoProvider(formInfo), data);
                }

                // Set and send email message
                EmailMessage em = new EmailMessage();
                em.EmailFormat = EmailFormatEnum.Html;
                em.From = fromEmail;
                em.Recipients = recipients;
                em.Subject = subject;
                em.Body = URLHelper.MakeLinksAbsolute(emailBody);

                // Attach uploaded documents if true
                if (bfi.FormEmailAttachUploadedDocs)
                {
                    string folderPath = FormHelper.GetBizFormFilesFolderPath(SiteName);

                    foreach (string file in uploadFiles)
                    {
                        var fileNameString = Convert.ToString(data.GetValue(file));
                        if (!String.IsNullOrEmpty(fileNameString))
                        {
                            var filePath = folderPath + FormHelper.GetGuidFileName(fileNameString);

                            if (File.Exists(filePath))
                            {
                                FileStream stream = FileStream.New(filePath, FileMode.Open, FileAccess.Read);

                                // Add attachment
                                Attachment attachment = new Attachment(stream, GetFileNameForUploader(fileNameString));
                                em.Attachments.Add(attachment);
                            }
                        }
                    }
                }

                // Encode email body if required
                if (EncodeEmails)
                {
                    em.Body = HTMLHelper.HTMLEncode(em.Body);
                }

                // Send email
                EmailSender.SendEmail(SiteName, em);
            }
        }


        /// <summary>
        /// Sends confirmation email (autoresponder).
        /// </summary>
        /// <param name="fromEmail">From email address</param>
        /// <param name="toEmail">To email address</param>
        /// <param name="data">Data from basic form</param>
        /// <param name="bfi">BizFormInfo</param>
        public virtual void SendConfirmationEmail(string fromEmail, string toEmail, IDataContainer data, BizFormInfo bfi)
        {
            if ((fromEmail != null) && (toEmail != null) && (data != null) && (bfi != null))
            {
                EnsureDataForSending(bfi);
                if (FormInformation == null)
                {
                    return;
                }

                // Prepare the resolver
                MacroResolver resolver = MacroContext.CurrentResolver.CreateChild();
                resolver.SetAnonymousSourceData(data);
                resolver.Settings.EncodeResolvedValues = EncodeEmails;

                // Localization macros wouldn't be correctly resolved without setting current culture code
                resolver.Culture = CultureHelper.GetPreferredCulture();

                // Merge email addresses and subject with entered data
                fromEmail = resolver.ResolveMacros(fromEmail);
                toEmail = resolver.ResolveMacros(toEmail);

                if (!ValidationHelper.IsEmail(toEmail))
                {
                    throw new ArgumentException("Recipient's email address is not valid.");
                }

                // Set default email subject
                string subject = string.Format("{0} - {1}", ResHelper.GetString("BizForm.ConfirmationMessageSubject"), bfi.FormDisplayName);

                if (!DataHelper.IsEmpty(bfi.FormConfirmationEmailSubject))
                {
                    // Set predefined email subject
                    subject = resolver.ResolveMacros(bfi.FormConfirmationEmailSubject);
                }

                // Prepare email body
                string body = ResolveEmailMessageText(resolver.ResolveMacros(bfi.FormConfirmationTemplate), new FormFieldInfoProvider(FormInformation), data);

                // Set and send email message
                EmailMessage em = new EmailMessage();
                em.EmailFormat = EmailFormatEnum.Html;
                em.From = fromEmail;
                em.Recipients = toEmail;
                em.Subject = subject;
                em.Body = URLHelper.MakeLinksAbsolute(body);

                // Append attachments
                EmailHelper.ResolveMetaFileImages(em, bfi.FormID, BizFormInfo.OBJECT_TYPE, ObjectAttachmentsCategories.FORMLAYOUT);

                // Encode email body if required
                if (EncodeEmails)
                {
                    em.Body = HTMLHelper.HTMLEncode(em.Body);
                }

                // Send the message
                EmailSender.SendEmail(SiteName, em);
            }
        }


        /// <summary>
        /// Resolve confirmation email message text.
        /// </summary>
        /// <param name="emailLayout">Email layout template with macros to resolve</param>
        /// <param name="formFieldInfoProvider">FormFieldInfo provider</param>
        /// <param name="data">Form data</param>
        internal string ResolveEmailMessageText(string emailLayout, IFormFieldInfoProvider formFieldInfoProvider, IDataContainer data)
        {
            if (string.IsNullOrEmpty(emailLayout))
            {
                return String.Empty;
            }

            // Read form layout definition from the beginning to the end
            MatchCollection matchColl = RegExEmailMacro.Matches(emailLayout);

            int actualPos = 0;
            StringBuilder result = new StringBuilder();

            foreach (Match match in matchColl)
            {
                int newPos = match.Index;
                if (actualPos < newPos)
                {
                    // Append non-macro text
                    result.Append(emailLayout.Substring(actualPos, newPos - actualPos));
                }
                actualPos = newPos + match.Length;

                // Read macro value
                string macro = match.Value;

                macro = macro.Replace("$$", string.Empty);
                var colonIndex = macro.IndexOf(":", StringComparison.Ordinal);
                string ffType = macro.Substring(0, colonIndex);
                string ffName = macro.Substring(colonIndex + 1, macro.Length - colonIndex - 1);

                FormFieldInfo ffi = formFieldInfoProvider.GetFormField(ffName);
                if (ffi != null)
                {
                    switch (ffType.ToLowerInvariant())
                    {
                        case "label":
                            // Add field caption
                            result.Append(ResHelper.LocalizeString(ffi.GetDisplayName(ContextResolver)));
                            break;

                        case "value":
                            // Add field value
                            if (data.ContainsColumn(ffName))
                            {
                                var value = GetFieldValueForCustomMail(data, ffName, ffi);
                                result.Append(value);
                            }
                            break;
                    }
                }
            }

            if (actualPos < emailLayout.Length)
            {
                // Append non-macro text
                result.Append(emailLayout.Substring(actualPos, emailLayout.Length - actualPos));
            }

            return result.ToString();
        }


        /// <summary>
        /// Creates message body with default layout of items.
        /// </summary>
        /// <param name="data">Data container with form fields data.</param>
        /// <param name="formFieldInfoInfoProvider">Provides access to form fields.</param>
        /// <param name="resolver">Macro resolver.</param>
        /// <param name="columnNames">Name of columns that should be included in message.</param>
        /// <returns></returns>
        internal string CreateMessageWithDefaultLayout(IDataContainer data, IFormFieldInfoProvider formFieldInfoInfoProvider, IMacroResolver resolver, IEnumerable<string> columnNames)
        {
            StringBuilder message = new StringBuilder();

            // Get column names in correct order

            var columns = from column in columnNames
                          where data.ContainsColumn(column)
                          select column;

            // Compose message body
            foreach (string column in columns)
            {
                // Get field definition
                FormFieldInfo ffi = formFieldInfoInfoProvider.GetFormField(column);
                if (ffi != null)
                {
                    var value = GetFieldValue(data, column, ffi);
                    message.Append(String.Format("{0}:      {1}", ResHelper.LocalizeString(ffi.GetDisplayName(resolver)), value));
                    message.Append("<br /><br />");
                }
            }

            return message.ToString();
        }


        /// <summary>
        /// Returns html code of link to bizform attached file.
        /// </summary>
        /// <param name="fileNameString">BizForm file name - guid + extension</param>
        protected string GetBizFormAttachmentLink(string fileNameString)
        {
            if (!string.IsNullOrEmpty(fileNameString))
            {
                return string.Format("<a href=\"~/CMSPages/GetBizFormFile.aspx?filename={0}&sitename={1}\">{2}</a>", FormHelper.GetGuidFileName(fileNameString), SiteName, GetFileNameForUploader(fileNameString));
            }

            return String.Empty;
        }


        /// <summary>
        /// Gets value of field for message with default layout. Value is transformed according to field type.
        /// </summary>
        /// <param name="data">Data container with form fields data.</param>
        /// <param name="fieldName">Field name</param>
        /// <param name="formFiledInfo">Field info object</param>
        /// <returns>Value for given field, transformed according to field type.</returns>
        private string GetFieldValue(IDataContainer data, string fieldName, FormFieldInfo formFiledInfo)
        {
            var rawValue = data.GetValue(fieldName);
            string value = ValidationHelper.GetString(rawValue, string.Empty);

            if (IsHtmlField(formFiledInfo))
            {
                value = URLHelper.MakeLinksAbsolute(value);
            }
            else if (IsFileField(formFiledInfo))
            {
                value = GetBizFormAttachmentLink(value);
            }
            else if (IsDateField(formFiledInfo))
            {
                value = ConvertDate(rawValue);
            }
            return value;
        }


        /// <summary>
        /// Checks if field is of html type.
        /// </summary>
        /// <param name="ffi">Field info</param>
        /// <returns><c>True</c> if field is of html type.</returns>
        private static bool IsHtmlField(FormFieldInfo ffi)
        {
            return FormHelper.IsFieldOfType(ffi, FormFieldControlTypeEnum.HtmlAreaControl);
        }


        /// <summary>
        /// Checks if field is of file type.
        /// </summary>
        /// <param name="ffi">Field info</param>
        /// <returns><c>True</c> if field is of file type.</returns>
        private static bool IsFileField(FormFieldInfo ffi)
        {
            return FormHelper.IsFieldOfType(ffi, FormFieldControlTypeEnum.UploadControl);
        }


        /// <summary>
        /// Checks if field is of date type.
        /// </summary>
        /// <param name="ffi">Field info</param>
        /// <returns><c>True</c> if field is of date type.</returns>
        private static bool IsDateField(FormFieldInfo ffi)
        {
            return ffi.DataType.Equals(FieldDataType.Date, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Gets date from date time without time information.
        /// </summary>
        private static string ConvertDate(object parameter)
        {
            var dateTime = ValidationHelper.GetDateTime(parameter, DateTimeHelper.ZERO_TIME);
            return dateTime.ToShortDateString();
        }


        /// <summary>
        /// Returns field value for custom mail message. Value is transformed according to field type.
        /// </summary>
        /// <param name="data">Data container with form fields data.</param>
        /// <param name="fieldName">Field name</param>
        /// <param name="formFieldInfo">Form field info</param>
        /// <returns>Value for given field, transformed according to field type.</returns>
        private string GetFieldValueForCustomMail(IDataContainer data, string fieldName, FormFieldInfo formFieldInfo)
        {
            var rawValue = data.GetValue(fieldName);
            string value = ValidationHelper.GetString(rawValue, string.Empty);

            if (FormHelper.IsFieldOfType(formFieldInfo, FormFieldControlTypeEnum.UploadControl))
            {
                value = GetBizFormAttachmentLink(value);
            }
            else if (IsDateField(formFieldInfo))
            {
                value = ConvertDate(rawValue);
            }
            return value;
        }


        /// <summary>
        /// Ensures that essential data is initialized when notification or confirmation emails are send from an external code.
        /// </summary>
        /// <param name="bfi">BizFormInfo object</param>
        private void EnsureDataForSending(BizFormInfo bfi)
        {
            if (ClassInfo == null && bfi != null)
            {
                // Get bizform class
                ClassInfo = DataClassInfoProvider.GetDataClassInfo(bfi.FormClassID);
            }

            if (ClassInfo != null)
            {
                // Ensure form information
                FormInformation = FormHelper.GetFormInfo(ClassInfo.ClassName, true);
            }
        }

        #endregion

        /// <summary>
        /// Provides access to form fields.
        /// </summary>
        internal interface IFormFieldInfoProvider
        {
            FormFieldInfo GetFormField(string fieldName);
        }


        /// <summary>
        /// Reads form field from FormInfo
        /// </summary>
        private class FormFieldInfoProvider : IFormFieldInfoProvider
        {
            private readonly FormInfo mFormInfo;

            public FormFieldInfoProvider(FormInfo formInfo)
            {
                mFormInfo = formInfo;
            }


            public FormFieldInfo GetFormField(string fieldName)
            {
                return mFormInfo.GetFormField(fieldName);
            }
        }
    }
}