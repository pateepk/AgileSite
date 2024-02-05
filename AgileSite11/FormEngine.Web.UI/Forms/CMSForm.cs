using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;

using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Form engine specific for CMS. It displays the given form.
    /// </summary>
    [ToolboxData("<{0}:CMSForm runat=server></{0}:CMSForm>")]
    public class CMSForm : BasicForm
    {
        #region "Constants"

        /// <summary>
        /// Prefix for the default validation error message.
        /// Concatenated with ".errorvalidationerror" resource string.
        /// If not found, general prefix is used.
        /// </summary>
        private const string mErrorMessagePrefix = "cmsform";

        #endregion


        #region "Variables"

        /// <summary>
        /// Default page template ID.
        /// </summary>
        protected int mDefaultPageTemplateID;


        /// <summary>
        /// Owner ID.
        /// </summary>
        protected int mOwnerID;


        /// <summary>
        /// Indicates if the form has been loaded.
        /// </summary>
        private bool mFormLoaded;


        /// <summary>
        /// Messages placeholder
        /// </summary>
        protected MessagesPlaceHolder mMessagesPlaceHolder;


        // Indicates whether local MessagesPlaceholder control should be added.
        private bool useLocalMessagesPlaceholder;


        // Local messages placeholder instance
        private MessagesPlaceHolder mLocalMessagesPlaceHolder;


        /// <summary>
        /// Document manager.
        /// </summary>
        protected ICMSDocumentManager mDocumentManager;


        /// <summary>
        /// Indicates whether DocumentManager should be added to controls collection.
        /// </summary>
        bool addDocumentManager;

        #endregion


        #region "Properties"

        /// <summary>
        /// Component name.
        /// </summary>
        public string ComponentName
        {
            get
            {
                return DocumentManager.ComponentName;
            }
            set
            {
                DocumentManager.ComponentName = value;
            }
        }


        /// <summary>
        /// Indicates if button OK should be displayed.
        /// </summary>
        [Browsable(true), DefaultValue(false), Description("Indicates if button OK should be displayed.")]
        public bool ShowOkButton
        {
            get
            {
                if (ViewState["ShowOkButton"] == null)
                {
                    ViewState["ShowOkButton"] = false;
                }
                return Convert.ToBoolean(ViewState["ShowOkButton"]);
            }
            set
            {
                ViewState["ShowOkButton"] = value;
            }
        }


        /// <summary>
        /// Optional DocumentId of the document that should be used as a template with default values.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("Optional DocumentId of the document that should be used as a template with default values.")]
        public int CopyDefaultDataFromDocumentId
        {
            get
            {
                int copyDefaultDataFromDocumentId = ValidationHelper.GetInteger(ViewState["CopyDefaultDataFromDocumentId"], 0);
                if (copyDefaultDataFromDocumentId != 0)
                {
                    DocumentManager.SourceDocumentID = copyDefaultDataFromDocumentId;
                }

                return DocumentManager.SourceDocumentID;
            }
            set
            {
                ViewState["CopyDefaultDataFromDocumentId"] = value;
                DocumentManager.SourceDocumentID = value;
            }
        }


        /// <summary>
        /// Specifies together with CultureCode edited node.
        /// </summary>
        [Category("Data"), Description("NodeID.")]
        public int NodeID
        {
            get
            {
                return DocumentManager.NodeID;
            }
            set
            {
                DocumentManager.NodeID = value;
            }
        }


        /// <summary>
        /// Specifies together with CultureCode edited node.
        /// </summary>
        [Category("Data"), Description("DocumentID.")]
        public int DocumentID
        {
            get
            {
                int documentId = ValidationHelper.GetInteger(ViewState["DocumentID"], 0);
                if (documentId != 0)
                {
                    DocumentManager.DocumentID = documentId;
                }

                return DocumentManager.DocumentID;
            }
            set
            {
                ViewState["DocumentID"] = value;
                DocumentManager.DocumentID = value;
            }
        }


        /// <summary>
        /// Parent node ID. Indicates parent node for document insertion.
        /// </summary>
        public int ParentNodeID
        {
            get
            {
                return DocumentManager.ParentNodeID;
            }
            set
            {
                DocumentManager.ParentNodeID = value;
            }
        }


        /// <summary>
        /// Document node.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TreeNode Node
        {
            get
            {
                return DocumentManager.Node;
            }
        }


        /// <summary>
        /// Parent node of currently edited/inserted node
        /// </summary>
        public new TreeNode ParentObject
        {
            get
            {
                if (DocumentManager.ParentNode != null)
                {
                    return DocumentManager.ParentNode;
                }

                if (base.ParentObject != null)
                {
                    return (TreeNode)base.ParentObject;
                }

                return null;
            }
            set
            {
                DocumentManager.ParentNode = value;
                base.ParentObject = value;
            }
        }


        /// <summary>
        /// Culture code.
        /// </summary>
        [Category("Data"), Description("Culture code.")]
        public string CultureCode
        {
            get
            {
                if ((FormMode == FormModeEnum.Insert) || (FormMode == FormModeEnum.InsertNewCultureVersion))
                {
                    return DocumentManager.NewNodeCultureCode;
                }
                else
                {
                    return DocumentManager.CultureCode;
                }
            }
            set
            {
                if ((FormMode == FormModeEnum.Insert) || (FormMode == FormModeEnum.InsertNewCultureVersion))
                {
                    DocumentManager.NewNodeCultureCode = value;
                }
                else
                {
                    DocumentManager.CultureCode = value;
                }
            }
        }


        /// <summary>
        /// Form name in form application.class.form.
        /// </summary>
        [Category("Data"), Description("Form name in form application.class.form.")]
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
        /// Tree provider instance. If it's not provided, it's created automatically.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TreeProvider TreeProvider
        {
            get
            {
                return DocumentManager.Tree;
            }
            set
            {
                DocumentManager.Tree = value;
            }
        }


        /// <summary>
        /// Form mode - insert or update.
        /// </summary>
        [Category("Behavior"), Description("Form mode - insert or update.")]
        public FormModeEnum FormMode
        {
            get
            {
                return DocumentManager.Mode;
            }
            set
            {
                DocumentManager.Mode = value;
            }
        }


        /// <summary>
        /// Default page template ID.
        /// </summary>
        public int DefaultPageTemplateID
        {
            get
            {
                return mDefaultPageTemplateID;
            }
            set
            {
                mDefaultPageTemplateID = value;
            }
        }


        /// <summary>
        /// Owner ID.
        /// </summary>
        public int OwnerID
        {
            get
            {
                return mOwnerID;
            }
            set
            {
                mOwnerID = value;
            }
        }


        /// <summary>
        /// Group ID.
        /// </summary>
        public int GroupID
        {
            get
            {
                return DocumentManager.GroupID;
            }
            set
            {
                if (Page != null)
                {
                    DocumentManager.GroupID = value;
                }
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
        /// Form name prefix. If set, form with name in format [FormPrefix][form name] is searched instead of [form name].
        /// </summary>
        public string FormPrefix
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
        /// Document manager control
        /// </summary>
        public virtual ICMSDocumentManager DocumentManager
        {
            get
            {
                if (mDocumentManager == null)
                {
                    // Get page
                    ICMSPage page = Page as ICMSPage;
                    mDocumentManager = page?.DocumentManager;

                    // Try to get manager from parent controls
                    mDocumentManager = ControlsHelper.GetParentProperty<AbstractUserControl, ICMSDocumentManager>(this, s => s.DocumentManager, mDocumentManager);

                    // Ensure local manager
                    if (mDocumentManager == null)
                    {
                        // Add manager to the collection of controls
                        addDocumentManager = true;
                        mDocumentManager = new CMSDocumentManager
                        {
                            ID = "dM",
                            IsLiveSite = IsLiveSite,
                            LocalMessagesPlaceHolder = MessagesPlaceHolder,
                            Mode = FormModeEnum.Insert
                        };
                    }
                }

                return mDocumentManager;
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
        /// Messages placeholder.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override MessagesPlaceHolder MessagesPlaceHolder
        {
            get
            {
                if (mMessagesPlaceHolder == null)
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
                                    mMessagesPlaceHolder = mess;
                                    return mMessagesPlaceHolder;
                                }
                            }
                        }

                        // Get page placeholder as default one
                        ICMSPage page = Page as ICMSPage;
                        if (page != null)
                        {
                            mMessagesPlaceHolder = page.MessagesPlaceHolder;
                        }

                        // Try to get placeholder from parent controls
                        mMessagesPlaceHolder = ControlsHelper.GetParentProperty<AbstractUserControl, MessagesPlaceHolder>(this, s => s.MessagesPlaceHolder, mMessagesPlaceHolder);
                    }

                    // Ensure local placeholder
                    if (mMessagesPlaceHolder == null)
                    {
                        mMessagesPlaceHolder = LocalMessagesPlaceHolder;
                        useLocalMessagesPlaceholder = true;
                    }
                }

                return mMessagesPlaceHolder;
            }
            set
            {
                mMessagesPlaceHolder = value;
            }
        }

        #endregion


        #region "Life-cycle methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSForm()
        {
            // Allow spell-check for doc.types
            AllowSpellCheck = true;
        }


        /// <summary>
        /// OnInit event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Document manager events
            DocumentManager.OnSaveData += DocumentManager_OnSaveData;
            DocumentManager.OnValidateData += DocumentManager_OnValidateData;
            DocumentManager.OnCheckPermissions += DocumentManager_OnCheckPermissions;
            DocumentManager.OnAfterAction += DocumentManager_OnAfterAction;

            // Form events
            OnAfterDataLoad += CMSForm_OnAfterDataLoad;
        }


        /// <summary>
        /// OnPreRender event
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

            // Process disabled fields if form is in insert mode or data is not consistent, to not loose user changes when the form is disabled (published step for example)
            ProcessDisabledFields = IsInsertMode || !DocumentManager.DataConsistent;
        }


        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (Context == null)
            {
                output.Write("[CMSForm: {0}]", ID);
            }
            else
            {
                base.Render(output);
            }
        }

        #endregion


        #region "DocumentManager event handlers"

        private void DocumentManager_OnAfterAction(object sender, DocumentManagerEventArgs e)
        {
            TreeNode node = e.Node;

            switch (e.Mode)
            {
                case FormModeEnum.Insert:
                    // Update form
                    {
                        DocumentID = node.DocumentID;
                        FormMode = FormModeEnum.Update;
                        Mode = FormModeEnum.Update;
                        EditedObject = node;
                        LoadData(node);
                    }
                    break;

                case FormModeEnum.InsertNewCultureVersion:
                    // Update form
                    {
                        DocumentID = node.DocumentID;
                        FormMode = FormModeEnum.Update;
                        Mode = FormModeEnum.Update;
                        EditedObject = node;
                    }
                    break;

                case FormModeEnum.Update:
                    EditedObject = node;
                    break;
            }
        }


        private void DocumentManager_OnCheckPermissions(object sender, SimpleDocumentManagerEventArgs e)
        {
            RaiseOnCheckPermissions();
        }


        private void DocumentManager_OnValidateData(object sender, DocumentManagerEventArgs e)
        {
            e.IsValid = ValidateData();
        }


        private void DocumentManager_OnSaveData(object sender, DocumentManagerEventArgs e)
        {
            e.UpdateDocument = false;
            SaveData(null);

            if (!IsLiveSite)
            {
                // Refresh breadcrumbs
                ScriptHelper.RefreshTabHeader(Page, e.Node.DocumentName);
            }
        }

        #endregion


        #region "Basic form event handlers"

        /// <summary>
        /// OnAfterDataReload event handler.
        /// </summary>
        protected void CMSForm_OnAfterDataLoad(object sender, EventArgs e)
        {
            // Set visibility of Submit button
            SubmitButton.Visible = ShowOkButton;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Loads the child controls at run-time.
        /// </summary>
        /// <param name="forceReload">Forces nested BasicForm to reload if true</param>
        public void LoadForm(bool forceReload)
        {
            if (StopProcessing || (mFormLoaded && !forceReload))
            {
                return;
            }

            // Clear edited node
            if (forceReload)
            {
                DocumentManager.ClearNode();
            }

            try
            {
                string className = GetClassName();

                // Do not show anything if form not defined
                if (string.IsNullOrEmpty(className))
                {
                    StopProcessing = true;
                    return;
                }

                // Get the node data
                switch (FormMode)
                {
                    case FormModeEnum.Insert:
                        DocumentManager.NewNodeClassName = className;
                        break;

                    case FormModeEnum.Update:
                        if (string.IsNullOrEmpty(FormName))
                        {
                            FormName = Node.NodeClassName + ".default";
                        }
                        break;
                }

                ClassInfo = DataClassInfoProvider.GetDataClassInfo(className);
                if (ClassInfo == null)
                {
                    throw new Exception("CMSForm.LoadForm(): DataClassInfo '" + className + "' not found.");
                }

                // Check the license for blog classes
                switch (ClassInfo.ClassName.ToLowerInvariant())
                {
                    case "cms.blog":
                    case "cms.blogmonth":
                    case "cms.blogpost":
                        if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
                        {
                            LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Blogs);
                        }
                        break;
                }

                // Get form definition (and alt.form definition if defined)
                // Prepare alt.form name
                string altFormName = AlternativeFormFullName;
                // Get alternative form info if preset
                if (string.IsNullOrEmpty(altFormName))
                {
                    // Priority of alternative forms:
                    // 1) FormPrefix + insert/update/newculture
                    // 2) FormPrefix
                    // 3) insert/update/newculture (default alternative form)

                    // Get default alternative form name
                    string defaultForm = GetDefaultAlternativeFormName(FormMode);

                    // Alternative forms to be searched
                    string forms = string.Format("{0}{1};{0}", FormPrefix, defaultForm);

                    // Get first existing alternative form or default one
                    altFormName = GetFirstExistingFormFullName(className, forms, defaultForm);
                }

                // Get merged form info if found
                FormInfo fi = FormHelper.GetFormInfo(altFormName, true);
                if (fi == null)
                {
                    StopProcessing = true;
                    return;
                }

                FormInformation = AddDefaultColumns(fi, ClassInfo);
                AltFormInformation = AlternativeFormInfoProvider.GetAlternativeFormInfo(altFormName);

                // Setup basic form
                ID = "f";
                Mode = FormMode;
                FormCssClass += " " + className.ToLowerInvariant().Replace(".", "_");

                // Set edited object and parent object if it's insert mode
                EditedObject = Data = Node;
                if (FormMode == FormModeEnum.Insert)
                {
                    ParentObject = DocumentManager.ParentNode;
                }

                // Preset dialog parameters which are used in HTML editor dialogs
                if (GroupID > 0)
                {
                    DialogParameters = "groupid=" + GroupID;
                }

                // If new culture version is based on existing, it copies temporary attachments, they are then converted to regular in save phase
                if ((FormMode == FormModeEnum.InsertNewCultureVersion) && (CopyDefaultDataFromDocumentId > 0))
                {
                    EnsureTemporaryAttachmentsForNewCulture();
                }

                // Set flag that the form is loaded
                mFormLoaded = true;

                // Reload data if required
                if (forceReload)
                {
                    ReloadData();
                }
            }
            catch (Exception ex)
            {
                ShowError(ResHelper.GetString("cmsform.notinitializederror"));

                // Log the error
                EventLogProvider.LogException("Content", FormMode.ToString(), ex);
            }
        }


        private void EnsureTemporaryAttachmentsForNewCulture()
        {
            // Check if temporary attachments have been already created
            var temporaryAttachmentsExist =
                AttachmentInfoProvider.GetAttachments()
                    .WhereEquals("AttachmentFormGUID", FormGUID)
                    .Count > 0;

            if (!temporaryAttachmentsExist)
            {
                // Copy attachments
                DocumentHelper.CopyAttachmentsAsTemporary(DocumentManager.SourceNode, FormGUID, SiteName);
            }
        }


        /// <summary>
        /// Saves document data.
        /// </summary>
        public void Save()
        {
            EnsureChildControls();

            DocumentManager.SaveDocument();
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Initializes the form.
        /// </summary>
        protected override void InitFormInternal()
        {
            base.InitFormInternal();

            LoadForm(false);
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
        /// Allows to add additional components after the form.
        /// </summary>
        protected override void AddControlsAfterInternal()
        {
            base.AddControlsAfterInternal();

            if (addDocumentManager)
            {
                // Add document manager
                Controls.Add((CMSDocumentManager)mDocumentManager);
            }
        }


        /// <summary>
        /// Initializes macro resolver data sources.
        /// </summary>
        protected override void InitResolver()
        {
            base.InitResolver();

            ContextResolver.SetNamedSourceDataCallback("ParentObject", r => ParentObject);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns name of the default alternative form for the specified form mode 
        /// (Insert, Convert modes -> "insert", Update mode -> "update", Insert new culture version mode -> "newculture").
        /// </summary>
        /// <param name="mode">Form mode</param>        
        private string GetDefaultAlternativeFormName(FormModeEnum mode)
        {
            switch (mode)
            {
                case FormModeEnum.Insert:
                    return "insert";

                case FormModeEnum.Update:
                    return "update";

                case FormModeEnum.InsertNewCultureVersion:
                    return "newculture";

                default:
                    return "update";
            }
        }


        /// <summary>
        /// Returns full name of the first existing alternative form. If no alternative form exists, default form full name is returned.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="formNames">Names of alternative forms to be searched separated by semicolon</param>
        /// <param name="defaultForm">Name of the default form</param>        
        private string GetFirstExistingFormFullName(string className, string formNames, string defaultForm)
        {
            if (!string.IsNullOrEmpty(formNames))
            {
                // Get list of form names
                string[] list = formNames.Split(';');

                // Return full name of first existing form
                foreach (string formName in list)
                {
                    if (!string.IsNullOrEmpty(formName))
                    {
                        // Get form full name
                        string fullName = className + "." + formName;

                        AlternativeFormInfo form = AlternativeFormInfoProvider.GetAlternativeFormInfo(fullName);
                        if (form != null)
                        {
                            return fullName;
                        }
                    }
                }
            }

            // Return full name of default form
            return className + "." + defaultForm;
        }


        /// <summary>
        /// Add default columns.
        /// </summary>
        private FormInfo AddDefaultColumns(FormInfo fi, DataClassInfo ci)
        {
            // Do not add default columns
            if ((fi == null) || (ci == null))
            {
                return fi;
            }

            FormFieldInfo ffi;

            // Add DocumentName form field
            if ((fi.GetFormField("DocumentName") == null) && (String.IsNullOrEmpty(ci.ClassNodeNameSource)))
            {
                ffi = new FormFieldInfo();
                ffi.Name = "DocumentName";
                ffi.Size = TreePathUtils.GetMaxNameLength(ci.ClassName);
                ffi.Visible = true;
                ffi.DataType = FieldDataType.Text;
                ffi.FieldType = FormHelper.GetFormFieldControlType("textbox");
                ffi.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, ResHelper.GetString("CMSForm.NodeNameDescription"));

                // Turn on automatic trimming
                ffi.Settings["trim"] = true;

                // Emptiness
                if (ci.ClassName.Equals(SystemDocumentTypes.Root, StringComparison.InvariantCultureIgnoreCase))
                {
                    ffi.AllowEmpty = true;
                    ffi.Enabled = false;
                }
                else
                {
                    ffi.AllowEmpty = false;
                }

                // Caption
                ffi.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, ResHelper.GetString(ci.ClassIsProduct ? "com.sku.name" : "general.documentname"));

                fi.AddFormItem(ffi, 0);
            }

            if (ci.ClassUsePublishFromTo)
            {
                // Add publishing category if more categories already available in form
                if (fi.GetCategoryNames().Count > 0)
                {
                    FormCategoryInfo fci = new FormCategoryInfo();
                    fci.SetPropertyValue(FormCategoryPropertyEnum.Caption, ResHelper.GetString("general.publishing"));
                    fci.SetPropertyValue(FormCategoryPropertyEnum.Visible, "true");
                    fi.AddFormCategory(fci);
                }

                // Add PublishFrom field
                if (!fi.FieldExists("DocumentPublishFrom"))
                {
                    ffi = new FormFieldInfo();
                    ffi.Name = "DocumentPublishFrom";
                    ffi.Visible = true;
                    ffi.DataType = FieldDataType.DateTime;
                    ffi.FieldType = FormHelper.GetFormFieldControlType("calendar");
                    ffi.AllowEmpty = true;
                    ffi.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, ResHelper.GetString("CMSForm.PublishFromCaption"));
                    ffi.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, ResHelper.GetString("CMSForm.PublishFromDescription"));
                    fi.AddFormItem(ffi);
                }

                // Add PublishTo field
                if (!fi.FieldExists("DocumentPublishTo"))
                {
                    ffi = new FormFieldInfo();
                    ffi.Name = "DocumentPublishTo";
                    ffi.Visible = true;
                    ffi.DataType = FieldDataType.DateTime;
                    ffi.FieldType = FormHelper.GetFormFieldControlType("calendar");
                    ffi.AllowEmpty = true;
                    ffi.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, ResHelper.GetString("CMSForm.PublishToCaption"));
                    ffi.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, ResHelper.GetString("CMSForm.PublishToDescription"));
                    fi.AddFormItem(ffi);
                }
            }

            return fi;
        }


        /// <summary>
        /// Saves data to database. It's called after mBasicForm.SaveData().
        /// </summary>
        protected override bool SaveDataInternal()
        {
            base.SaveDataInternal();

            MessagesPlaceHolder.ClearLabels();

            if (Data != null)
            {
                // Get new node data
                TreeNode nodeData = TreeNode.New<TreeNode>(Node.NodeClassName, Data, TreeProvider);
                var settings = new CopyNodeDataSettings(true, null)
                {
                    CopySystemTreeData = false,
                    CopySystemDocumentData = false,
                    CopySKUData = false,
                    ResetChanges = Mode != FormModeEnum.Update
                };
                DocumentHelper.CopyNodeData(nodeData, Node, settings);
            }

            // Map the document name
            Node.MapDocumentName();

            switch (FormMode)
            {
                case FormModeEnum.Insert:
                    {
                        // Set culture
                        Node.SetValue("DocumentCulture", CultureCode);

                        // Set default document template
                        if (Node.GetUsedPageTemplateId() <= 0)
                        {
                            Node.SetDefaultPageTemplateID(DefaultPageTemplateID);
                        }

                        // Set document owner
                        if (OwnerID > 0)
                        {
                            Node.SetValue("NodeOwner", OwnerID);
                        }

                        // Set document group
                        if (GroupID > 0)
                        {
                            Node.SetValue("NodeGroupID", GroupID);
                        }

                        // Ensures documents consistency (blog post hierarchy etc.)
                        DocumentManager.EnsureDocumentsConsistency();

                        // Create a new node using parent
                        var parent = DocumentHelper.GetDocument(ParentNodeID, TreeProvider.ALL_CULTURES, TreeProvider);
                        DocumentHelper.InsertDocument(Node, parent, TreeProvider);

                        // Ensure temporary attachments conversion
                        var updateRequired = DocumentHelper.SaveTemporaryAttachments(Node, FormGUID, SiteName);

                        // Update node if required
                        if (updateRequired)
                        {
                            using (new DocumentActionContext { SendNotifications = false })
                            {
                                DocumentManager.UpdateDocument(true);
                            }
                        }
                    }
                    break;

                case FormModeEnum.Update:
                    {
                        // Update node                         
                        DocumentManager.UpdateDocument(true);
                    }
                    break;

                case FormModeEnum.InsertNewCultureVersion:
                    {
                        // Ensures documents consistency (blog post hierarchy etc.)
                        DocumentManager.EnsureDocumentsConsistency();

                        var settings = new NewCultureDocumentSettings(Node, CultureCode, TreeProvider)
                        {
                            // Turn off clearing of attachment fields because of ensuring own attachments at the target culture (values are overwritten later)
                            ClearAttachmentFields = false
                        };

                        // Insert as new culture version
                        DocumentHelper.InsertNewCultureVersion(settings);

                        using (new DocumentActionContext { SendNotifications = false })
                        {
                            // Ensure standard attachments
                            var updateRequired = false;

                            if (DocumentManager.SourceNode != null)
                            {
                                // When creating new culture version from existing, we need to copy the field attachments at this point, because of original GUIDs of attachments in single attachment fields
                                // Other attachments (grouped, unsorted) are already copied as temporary attachments from load phase
                                var attachmentsCopier = new NewCultureVersionAttachmentsCopier(DocumentManager.SourceNode, Node, true);
                                updateRequired = attachmentsCopier.CopyFieldAttachments();
                            }

                            // Ensure temporary attachments conversion, this handles grouped attachments, unsorted attachments
                            updateRequired |= DocumentHelper.SaveTemporaryAttachments(Node, FormGUID, SiteName);

                            if (CopyDefaultDataFromDocumentId > 0)
                            {
                                // Process the categories
                                DocumentHelper.CopyDocumentCategories(CopyDefaultDataFromDocumentId, Node.DocumentID);
                            }

                            // Update node if required
                            if (updateRequired)
                            {
                                DocumentManager.UpdateDocument(true);
                            }
                        }
                    }
                    break;
            }

            return true;
        }


        /// <summary>
        /// Extracts class name from the form name.
        /// </summary>
        protected string GetClassName()
        {
            string className = null;
            if (string.IsNullOrEmpty(FormName))
            {
                // Get class name and form name
                switch (FormMode)
                {
                    case FormModeEnum.Insert:
                        className = DocumentManager.NewNodeClassName;
                        break;

                    case FormModeEnum.InsertNewCultureVersion:
                        TreeNode classNode = DocumentManager.InvariantNode;
                        if (classNode != null)
                        {
                            className = classNode.NodeClassName;
                        }
                        break;

                    case FormModeEnum.Update:
                        if (Node != null)
                        {
                            className = Node.NodeClassName;
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(className))
                {
                    FormName = className + ".default";
                }
            }
            else
            {
                var formParts = FormName.Split('.');
                className = formParts[0] + "." + formParts[1];
            }

            return className;
        }

        #endregion


        #region "Info Messages methods"

        /// <summary>
        /// Creates local messages placeholder.
        /// </summary>
        protected override MessagesPlaceHolder CreateMessagesPlaceHolder()
        {
            MessagesPlaceHolder msgs = base.CreateMessagesPlaceHolder();

            msgs.ErrorBasicCssClass = "EditingFormError";
            msgs.InfoBasicCssClass = "EditingFormInfo";
            msgs.WarningBasicCssClass = "EditingFormInfo";
            msgs.ConfirmationBasicCssClass = "EditingFormInfo";

            return msgs;
        }

        #endregion
    }
}