using System;
using System.ComponentModel;
using System.Security;
using System.Threading;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.Synchronization;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Form control for the admin UI.
    /// </summary>
    [ToolboxData("<{0}:UIForm runat=server></{0}:UIForm>")]
    public class UIForm : BasicForm, IObjectTypeDriven
    {
        #region "Constants"

        /// <summary>
        /// Prefix for the default validation error message.
        /// Concatenated with ".errorvalidationerror" resource string.
        /// If not found, general prefix is used.
        /// </summary>
        private const string mErrorMessagePrefix = "uiform";

        #endregion


        #region "Variables"

        private CMSObjectManager mObjectManager;
        private bool mDisplayExternallyModified = true;
        private int? mObjectSiteID;
        private IFormStyleConfiguration mFormDefaultStyle;
        private string fullFormName;

        #endregion


        #region "Events"

        /// <summary>
        /// Event for early initialization
        /// </summary>
        public event EventHandler OnCreate;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether the tabs should be refreshed.
        /// </summary>
        private bool PerformTabsRefresh
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether header should be automatically refreshed when display name is changed.
        /// </summary>
        public bool RefreshHeader
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether display name of object was changed
        /// </summary>
        public bool DisplayNameChanged
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the object manager control if present on the page.
        /// </summary>
        public CMSObjectManager ObjectManager
        {
            get
            {
                return mObjectManager ?? (mObjectManager = CMSObjectManager.GetCurrent(Page));
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
                return base.MessagesPlaceHolder;
            }
            set
            {
                base.MessagesPlaceHolder = value;
            }
        }


        /// <summary>
        /// Security check.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue(null), Browsable(false)]
        public SecurityCheck SecurityCheck
        {
            get;
            set;
        }


        /// <summary>
        /// Object type for editing.
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Edited object.
        /// </summary>
        public new BaseInfo EditedObject
        {
            get
            {
                if (Data != null)
                {
                    return (BaseInfo)Data;
                }
                else if (base.EditedObject != null)
                {
                    return (BaseInfo)base.EditedObject;
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
        /// Edited object site ID.
        /// </summary>
        public int ObjectSiteID
        {
            get
            {
                // Get externally set value
                if (mObjectSiteID.HasValue)
                {
                    return mObjectSiteID.Value;
                }

                // Get parent object site ID in insert mode
                if (IsInsertMode)
                {
                    return (ParentObject != null) ? ParentObject.Generalized.ObjectSiteID : 0;
                }

                return EditedObject.Generalized.ObjectSiteID;
            }
            set
            {
                mObjectSiteID = value;
            }
        }


        /// <summary>
        /// Parent object.
        /// </summary>
        public new BaseInfo ParentObject
        {
            get
            {
                if (UIContext.EditedObjectParent != null)
                {
                    return (BaseInfo)UIContext.EditedObjectParent;
                }

                if (EditedObject != null)
                {
                    return EditedObject.Parent;
                }

                if (base.ParentObject != null)
                {
                    return (BaseInfo)base.ParentObject;
                }

                return null;
            }
            set
            {
                UIContext.EditedObjectParent = value;
                base.ParentObject = value;
            }
        }


        /// <summary>
        /// The URL to which the engine should redirect after creation of the new object.
        /// Set to empty string to disable redirect.
        /// </summary>
        public string RedirectUrlAfterCreate
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value that indicates if the form enabled state is set automatically based on the lock state of the edited object.
        /// </summary>
        public bool EnabledByLockState
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the name of the alternative form to use.
        /// </summary>
        public string AlternativeFormName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the position of the newly created object. This applies only to those object which has OrderColumn defined.
        /// </summary>
        public ObjectOrderEnum NewObjectPosition
        {
            get;
            set;
        }


        /// <summary>
        /// Default form specific style configuration.
        /// </summary>
        protected override IFormStyleConfiguration FormDefaultStyle
        {
            get
            {
                return mFormDefaultStyle ?? (mFormDefaultStyle = new UIFormDefaultStyle());
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

        #endregion


        #region "Lifecycle methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public UIForm()
        {
            DefaultFieldLayout = FieldLayoutEnum.ThreeColumns;

            EnabledByLockState = true;

            OnBeforeValidate += UIForm_OnBeforeValidate;
            OnItemValidation += UIForm_OnItemValidation;
            OnCheckPermissions += UIForm_OnCheckPermissions;

            MarkRequiredFields = true;
        }


        /// <summary>
        /// OnInit method.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (SubmitButton != null)
            {
                SubmitButton.RegisterHeaderAction = true;
            }

            PageContext.InitComplete += PageHelper_InitComplete;
        }


        /// <summary>
        /// Init complete event handler
        /// </summary>
        protected void PageHelper_InitComplete(object sender, EventArgs e)
        {
            if (ObjectManager != null)
            {
                ObjectManager.ShowPanel = true;
            }
        }


        /// <summary>
        /// Onload method.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (StopProcessing)
            {
                return;
            }

            if (ObjectManager != null)
            {
                ObjectManager.OnSaveData += ObjectManager_OnSaveData;
                ObjectManager.OnAfterAction += ObjectManager_OnAfterAction;
                ObjectManager.OnBeforeAction += ObjectManager_OnBeforeAction;
            }

            if (!RequestHelper.IsPostBack())
            {
                // Automatically display changes saved text
                if (QueryHelper.GetBoolean("saved", false))
                {
                    ShowChangesSaved();
                }
            }

            // Disable form if the security check didn't succeed
            if ((SecurityCheck != null) && SecurityCheck.DisableForm && !SecurityCheck.IsAllowed)
            {
                // Disable all editing controls
                Enabled = false;
            }
        }


        /// <summary>
        /// OnPreRender method.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (EnabledByLockState)
            {
                EnableByLockState();
            }

            if (EditedObject != null)
            {
                if (mDisplayExternallyModified && EditedObject.Generalized.IsModifiedExternally())
                {
                    ShowInformation(ResHelper.GetString("general.objectmodifiedexternally"));
                }

                if (PerformTabsRefresh)
                {
                    ScriptHelper.RefreshTabHeader(Page, EditedObject.Generalized.ObjectDisplayName);
                }
            }
        }

        #endregion


        #region "ObjectManager event handlers"

        /// <summary>
        /// Handles the ObjectManager OnSaveData event.
        /// </summary>
        private void ObjectManager_OnSaveData(object sender, SimpleObjectManagerEventArgs e)
        {
            if (!SaveData(null))
            {
                e.IsValid = false;
            }
        }


        /// <summary>
        /// Handles before action events. Validates data before check-in.
        /// </summary>
        private void ObjectManager_OnBeforeAction(object sender, SimpleObjectManagerEventArgs e)
        {
            if (e.ActionName == ComponentEvents.CHECKIN)
            {
                if (!ValidateData())
                {
                    e.IsValid = false;
                }
            }
        }


        /// <summary>
        /// Handles after action events. Reloads the form after Undo checkout action because this action causes changes in the content of the form.
        /// </summary>
        private void ObjectManager_OnAfterAction(object sender, SimpleObjectManagerEventArgs e)
        {
            if ((e.ActionName == ComponentEvents.UNDO_CHECKOUT) || (e.ActionName == ComponentEvents.CHECKOUT) || (e.ActionName == ComponentEvents.CHECKIN))
            {
                // Get data
                Data = (IDataContainer)UIContext.EditedObject;

                // Load current values
                LoadControlValues();
            }
        }


        /// <summary>
        /// Enables or disables form according to current checkout state with respect to current user.
        /// </summary>
        public void EnableByLockState()
        {
            if (SynchronizationHelper.UseCheckinCheckout && Mode == FormModeEnum.Update
                && ObjectManager?.InfoObject != null && ObjectManager.InfoObject.TypeInfo.SupportsLocking)
            {
                // Disable the form if the object is checked out by different user
                // Disable also when the object is not checked out by anybody to force object locking when UseCheckinCheckout is enabled
                Enabled = ObjectManager.InfoObject.Generalized.IsCheckedOutByUser(MembershipContext.AuthenticatedUser);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the form information.
        /// </summary>
        protected override void InitFormInternal()
        {
            base.InitFormInternal();

            OnCreate?.Invoke(this, null);

            if (StopProcessing)
            {
                return;
            }

            if (!EnsureObjectType())
            {
                return;
            }

            // Load the edited object or create new one
            Data = (IDataContainer)UIContext.EditedObject;

            string formName;
            if (Data == null)
            {
                // Synchronize edited object within context
                EditedObject = ModuleManager.GetObject(ObjectType);

                Mode = FormModeEnum.Insert;
                formName = AlternativeFormName ?? "insert";
            }
            else
            {
                base.EditedObject = Data;

                Mode = FormModeEnum.Update;
                formName = AlternativeFormName ?? "update";
            }

            // Prepare full form name from object class name (object type name) and alternative form name
            fullFormName = string.Format("{0}.{1}", (Data != null) ? ((BaseInfo)Data).TypeInfo.ObjectClassName : ObjectType, formName);

            // Get form definition (and alt.form definition if defined)
            FormInformation = FormHelper.GetFormInfo(fullFormName, true);
            AltFormInformation = AlternativeFormInfoProvider.GetAlternativeFormInfo(fullFormName);

            if (Mode == FormModeEnum.Insert)
            {
                // Load default values
                FormInformation?.LoadDefaultValues(Data, FormResolveTypeEnum.AllFields);
            }

            // Set form CSS class as object type (e.g. cms_user)
            var infoObj = Data as BaseInfo;
            if (infoObj != null)
            {
                ClassInfo = DataClassInfoProvider.GetDataClassInfo(infoObj.TypeInfo.ObjectClassName);
                FormCssClass += " " + infoObj.TypeInfo.ObjectType.ToLowerInvariant().Replace(".", "_");
            }
        }


        private bool EnsureObjectType()
        {
            // Ensure the object type
            if (String.IsNullOrEmpty(ObjectType))
            {
                var editedObj = UIContext.EditedObject as BaseInfo;
                if (editedObj != null)
                {
                    ObjectType = editedObj.TypeInfo.ObjectType;
                }
            }

            // Validate that the object type is set
            if (String.IsNullOrEmpty(ObjectType))
            {
                ShowError(ResHelper.GetString("ui.objecttypenotset"));
                StopProcessing = true;
                return false;
            }

            return true;
        }


        /// <summary>
        /// Initializes macro resolver data sources.
        /// </summary>
        protected override void InitResolver()
        {
            base.InitResolver();

            // Replace form data with UI form data
            var resolver = ContextResolver;

            resolver.SetNamedSourceDataCallback("Form", r => new UIFormMacroContainer(this));

            resolver.SetNamedSourceDataCallback("EditedObject", r => EditedObject);
            resolver.SetNamedSourceDataCallback("ParentObject", r => ParentObject);
        }


        /// <summary>
        /// Allows to add additional components after the form.
        /// </summary>
        protected override void AddControlsAfterInternal()
        {
            base.AddControlsAfterInternal();

            // Create quick edit link
            if (SystemContext.DevelopmentMode && (ClassInfo != null))
            {
                string link = (AltFormInformation == null) ? PortalUIHelper.GetClassFieldsLink(ClassInfo.ClassID, fullFormName) : PortalUIHelper.GetClassAlternativeFormsLink(ClassInfo.ClassID, AltFormInformation.FormName);

                Controls.Add(new LiteralControl(link));
            }
        }


        private void UIForm_OnBeforeValidate(object sender, EventArgs e)
        {
            if (Data == null)
            {
                return;
            }

            // Clone the object for editing
            BaseInfo obj = (BaseInfo)Data;
            obj = obj.Generalized.Clone();

            // Ensure object site ID
            if (!DataHelper.IsValidID(obj.Generalized.ObjectSiteID) && (obj.TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                obj.Generalized.ObjectSiteID = ObjectSiteID;
            }

            // Ensure the parent identifier
            if (!DataHelper.IsValidID(obj.Generalized.ObjectParentID) && (ParentObject != null))
            {
                obj.Generalized.ObjectParentID = ParentObject.Generalized.ObjectID;
            }

            Data = obj;
        }


        /// <summary>
        /// Check permissions event handler.
        /// </summary>
        protected void UIForm_OnCheckPermissions(object sender, EventArgs e)
        {
            if ((SecurityCheck != null) && !SecurityCheck.IsAllowed)
            {
                StopProcessing = true;

                ShowInformation(ResHelper.GetString("general.modifynotallowed"));
            }
        }


        /// <summary>
        /// Fired on the item validation.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="errorMessage">Returning the error message</param>
        protected void UIForm_OnItemValidation(object sender, ref string errorMessage)
        {
            // Validation of the item
            var ctrl = (FormEngineUserControl)sender;
            if (ctrl == null)
            {
                return;
            }

            if (!ctrl.CheckUnique && ((ctrl.FieldInfo == null) || !ctrl.FieldInfo.IsUnique))
            {
                return;
            }

            string field = ctrl.Field;

            // Save the field value
            SetDataValue(field, ctrl.Value);

            // Check if the field is unique
            if (field.Equals(EditedObject.Generalized.CodeNameColumn, StringComparison.InvariantCultureIgnoreCase))
            {
                string niceObjectType = TypeHelper.GetNiceObjectTypeName(EditedObject.TypeInfo.ObjectType);

                // Validates requirement of parent object
                errorMessage = ValidateParentObjectRequirement(niceObjectType);
                if (!String.IsNullOrEmpty(errorMessage))
                {
                    return;
                }

                if (!EditedObject.CheckUniqueCodeName())
                {
                    errorMessage = String.Format(CoreServices.Localization.GetAPIString("general.codenamenotunique", null, "The {0} with code name '{1}' already exists."), niceObjectType.ToLowerInvariant(), HTMLHelper.HTMLEncode(ValidationHelper.GetString(ctrl.Value, String.Empty)));
                }
            }
            else
            {
                // Skip check for invalid values
                bool skipCheck = !String.IsNullOrEmpty(errorMessage);

                // Skip uniqueness test when value is empty and field allows empty value
                if ((FormInformation != null) && DataHelper.IsEmpty(ctrl.Value))
                {
                    FormFieldInfo fieldInfo = FormInformation.GetFormField(field);
                    if ((fieldInfo != null) && (fieldInfo.AllowEmpty))
                    {
                        skipCheck = true;
                    }
                }

                if (!skipCheck && !EditedObject.CheckUniqueValues(field))
                {
                    // Get the name from the field label
                    string fieldName = field;
                    var fieldInfo = ctrl.FieldInfo;
                    if (fieldInfo != null)
                    {
                        // Trim additional characters (colon, required mark) from the end of the text
                        fieldName = fieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldCaption, ContextResolver, null, true).TrimEnd(ResHelper.Colon.ToCharArray()).TrimEnd(ResHelper.RequiredMark.ToCharArray());
                    }

                    errorMessage = String.Format(ResHelper.GetString("general.mustbeunique"), fieldName);
                }
            }
        }


        private string ValidateParentObjectRequirement(string niceObjectType)
        {
            // Check if parent object is required for code name uniqueness validation, or parent is already specified
            if ((EditedObject.TypeInfo.ParentIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN) || (ParentObject != null))
            {
                return null;
            }

            var editObj = EditedObject.Generalized;

            // Try to get parent value from form field
            var parentFieldControl = FieldControls[EditedObject.TypeInfo.ParentIDColumn];
            if (parentFieldControl != null)
            {
                var parentId = ValidationHelper.GetInteger(parentFieldControl.Value, 0);

                editObj.ObjectParentID = parentId;
            }

            // Check if edited object has really parent identifier set
            if (!DataHelper.IsValidID(editObj.ObjectParentID))
            {
                if (ParentObject == null)
                {
                    return String.Format(CoreServices.Localization.GetString("uiform.codenamevalidationparentmissing"), niceObjectType);
                }

                editObj.ObjectParentID = ParentObject.Generalized.ObjectID;
            }

            return null;
        }


        /// <summary>
        /// Internal method to save data to the database.
        /// </summary>
        protected override bool SaveDataInternal()
        {
            base.SaveDataInternal();

            string redirectUrlAfterCreate = null;

            try
            {
                var editedObj = EditedObject.Generalized;

                // Ensure the parent settings
                if (!DataHelper.IsValidID(editedObj.ObjectParentID) && HasParentObject(editedObj))
                {
                    editedObj.ObjectParentID = ParentObject.Generalized.ObjectID;
                }

                bool isInsert = !DataHelper.IsValidID(editedObj.ObjectID);

                DisplayNameChanged = editedObj.ItemChanged(editedObj.DisplayNameColumn) || ResHelper.ContainsLocalizationMacro(editedObj.ObjectDisplayName);

                // Check if order column exists and is set
                string orderColumn = EditedObject.TypeInfo.OrderColumn;
                bool orderIsSet = !DataHelper.IsEmpty(EditedObject.GetValue(orderColumn));

                // Set the correct order of the item
                bool setOrder = isInsert && !orderIsSet && (orderColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
                if (setOrder)
                {
                    // Set last position by default
                    EditedObject.SetValue(orderColumn, EditedObject.Generalized.GetLastObjectOrder());
                }

                // Save the object to the database
                EditedObject.Generalized.SetObject();

                // Other types of NewObjectPosition have to be processed after the object insertion
                if (setOrder && (NewObjectPosition != ObjectOrderEnum.Last))
                {
                    if (NewObjectPosition == ObjectOrderEnum.First)
                    {
                        EditedObject.Generalized.SetObjectOrder(1);
                    }
                    else if (NewObjectPosition == ObjectOrderEnum.Alphabetical)
                    {
                        EditedObject.Generalized.SetObjectAlphabeticalOrder();
                    }
                }

                // Update the edited object in context is saved successfully
                UIContext.EditedObject = base.EditedObject = EditedObject;

                if (isInsert && EditedObject.TypeInfo.SupportsLocking && CMSObjectManager.KeepNewObjectsCheckedOut)
                {
                    ObjectManager.CheckOutObject(EditedObject);
                }

                // Set flag to refresh tabs/breadcrumbs
                PerformTabsRefresh = DisplayNameChanged && !isInsert && RefreshHeader;

                // Handle the redirection
                // If RedirectUrlAfterCreate = String.Empty - > no redirection after create new object - special cases
                if (isInsert && (RedirectUrlAfterCreate != String.Empty))
                {
                    // Set the redirection after creation
                    redirectUrlAfterCreate = RedirectUrlAfterCreate ?? RedirectUrlAfterSave;
                    if (redirectUrlAfterCreate == null)
                    {
                        throw new Exception("The RedirectUrlAfterCreate property must be set for creating of the new objects.");
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowError(ResHelper.GetStringFormat("general.sourcecontrolerror", ex.Message));
                EventLogProvider.LogException(TypeHelper.GetNiceObjectTypeName(ObjectType), "SubmitForm", ex);

                return false;
            }
            catch (SecurityException ex)
            {
                ShowError(ResHelper.GetStringFormat("general.mediumtrusterror", ex.Message));
                mDisplayExternallyModified = false;

                return false;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ShowError(HTMLHelper.HTMLEncode(ex.Message));

                // Log the error
                if (!(ex is InfoObjectException))
                {
                    EventLogProvider.LogException(TypeHelper.GetNiceObjectTypeName(ObjectType), "SubmitForm", ex);
                }
            }

            // If redirect is required, do it (it has to outside of try catch block, otherwise "Thread was being aborted" exception is thrown).
            if (!String.IsNullOrEmpty(redirectUrlAfterCreate))
            {
                RedirectUrlAfterSave = redirectUrlAfterCreate;

                return true;
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
            // Reload the control values
            LoadControlValues();

            base.AfterSaveActionsInternal(redirectUrlAfterSave, showChangesSaved);
        }


        /// <summary>
        /// Returns true if edited object has relation to a parent and it exists; if it doesn't exist an exception is thrown.
        /// Returns false if edited object doesn't have relation to a parent.
        /// </summary>
        /// <param name="editedObject">Edited object</param>
        private bool HasParentObject(GeneralizedInfo editedObject)
        {
            if ((editedObject != null) && (editedObject.TypeInfo.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                if (ParentObject == null)
                {
                    throw new InvalidOperationException("Parent object is not defined.");
                }

                return true;
            }

            return false;
        }

        #endregion
    }
}
