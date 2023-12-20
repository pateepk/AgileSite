using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Globalization;
using CMS.Globalization.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.SiteProvider;

using TimeZoneInfo = CMS.Globalization.TimeZoneInfo;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Basic form engine independent on CMS. It displays the specified form.
    /// </summary>
    [ToolboxData("<{0}:BasicForm runat=server></{0}:BasicForm>")]
    [ParseChildren(true)]
    public class BasicForm : AbstractBasicForm, IFormStyleConfiguration
    {
        #region "Events"

        /// <summary>
        /// Before data load event.
        /// </summary>
        public event EventHandler OnBeforeDataLoad;


        /// <summary>
        /// After data load event.
        /// </summary>
        public event EventHandler OnAfterDataLoad;


        /// <summary>
        /// Before validation event.
        /// </summary>
        public event EventHandler OnBeforeValidate;


        /// <summary>
        /// After validation event.
        /// </summary>
        public event EventHandler OnAfterValidate;


        /// <summary>
        /// Before form data retrieval event. It is called during saving process after successful validation and before data are retrieved from form controls.
        /// </summary>
        public event EventHandler OnBeforeDataRetrieval;


        /// <summary>
        /// Before redirect event (after successful save).
        /// </summary>
        public event EventHandler OnBeforeRedirect;


        /// <summary>
        /// Before form data save event. It is called during saving process after data are retrieved from form controls and can be processed further (e.g. saved to DB).
        /// </summary>
        public event EventHandler OnBeforeSave;


        /// <summary>
        /// After form data save event. It is called during saving process when the form content is successfully saved.
        /// </summary>
        public event EventHandler OnAfterSave;

        /// <summary>
        /// Occurs when saving data to the database fails.
        /// </summary>
        public event EventHandler OnSaveFailed;

        /// <summary>
        /// Occurs when validation of the form fails.
        /// </summary>
        public event EventHandler OnValidationFailed;


        /// <summary>
        /// Occurs when permission can be checked. It is called at the beginning of the saving process.
        /// </summary>
        public event EventHandler OnCheckPermissions;


        /// <summary>
        /// Occurs when a field is validated.
        /// </summary>
        public event OnItemValidationEventHandler OnItemValidation;


        /// <summary>
        /// Occurs when the file is uploaded.
        /// </summary>
        public event EventHandler OnUploadFile;


        /// <summary>
        /// Occurs when the file is deleted.
        /// </summary>
        public event EventHandler OnDeleteFile;


        /// <summary>
        /// Fired on the change of a form control which has some dependencies.
        /// </summary>
        internal event EventHandler ControlWithDependenciesChanged;


        /// <summary>
        /// Fired after the form is loaded but before the <see cref="OnAfterDataLoad"/> event.
        /// </summary>
        internal event EventHandler DummyFieldDataLoad;

        #endregion


        #region "Constants"

        /// <summary>
        /// Prefix for the default validation error message.
        /// Concatenated with ".errorvalidationerror" resource string.
        /// If not found, general prefix is used.
        /// </summary>
        private const string mErrorMessagePrefix = "basicform";

        #endregion


        #region "Variables"

        /// <summary>
        /// Validation error message
        /// </summary>
        private string mValidationErrorMessage;


        /// <summary>
        /// Form submit button.
        /// </summary>
        protected FormSubmitButton mSubmitButton;


        /// <summary>
        /// Form submit image button.
        /// </summary>
        protected ImageButton mSubmitImageButton;


        /// <summary>
        /// Placeholder for category list.
        /// </summary>
        protected PlaceHolder categoryListPlaceholder;


        /// <summary>
        /// Category list panel, may be displayed above formPanel.
        /// </summary>
        protected Panel categoryListPanel;


        /// <summary>
        /// If true, colon(:) is placed behind field label.
        /// </summary>
        private bool mUseColonBehindLabel = true;

        private string mHtmlAreaToolbar = String.Empty;
        private string mHtmlAreaToolbarLocation = String.Empty;
        private FormTypeEnum mFormType = FormTypeEnum.BasicForm;
        private bool? mShowImageButton;
        private string mExpandCategoryImageUrl;
        private string mCollapseCategoryImageUrl;


        /// <summary>
        /// Nonpublic fields are shown if true.
        /// </summary>
        protected bool mShowPrivateFields = true;


        /// <summary>
        /// Form visibility info.
        /// </summary>
        private FormVisibilityInfo visibilityInfo;


        /// <summary>
        /// Indicates if external editing should be allowed for the controls.
        /// </summary>
        private bool mAllowExternalEditing = true;


        /// <summary>
        /// Indicates if field emptiness should be checked on validation.
        /// </summary>
        private bool mCheckFieldEmptiness = true;


        /// <summary>
        /// Web part context resolver.
        /// </summary>
        private MacroResolver mContextResolver;


        /// <summary>
        /// Indicates if entered form data are valid. If it's null the validation needs to be done.
        /// </summary>
        private bool? mIsValid;


        private bool? mIsFirstLoad;


        /// <summary>
        /// CSS class that is added to editing form control after its validation failed. Default value is "Error".
        /// </summary>
        private string mFieldErrorCssClass;


        private IDictionary<string, object> mAdditionalData;

        private List<string> mFieldsToHide;

        private StringSafeDictionary<bool> mFieldsVisibility;

        private LayoutTypeEnum mFormLayoutType = LayoutTypeEnum.Html;

        private HtmlTextWriterTag? mTagKey;

        private Panel mFormPanel;

        // IFormStyleConfiguration properties' inner fields
        private string mFormCssClass;
        private string mFieldCssClass;
        private string mGroupCssClass;
        private string mFieldGroupCssClass;
        private string mFieldGroupCaptionCssClass;
        private string mFieldValueCellCssClass;
        private string mFieldCaptionCellCssClass;
        private string mFieldCaptionCssClass;
        private string mFieldErrorCellCssClass;
        private string mFieldErrorLabelCssClass;
        private string mFieldVisibilityCellCssClass;
        private string mFieldVisibilityCssClass;
        private string mExplanationTextCssClass;
        private string mFormButtonCssClass;
        private string mFormButtonPanelCssClass;

        private IFormStyleConfiguration mLayoutStyleConfiguration;
        private IFormStyleConfiguration mFormDefaultStyle;

        #endregion


        #region "Inner controls properties"

        /// <summary>
        /// Layout template, if set, the given layout is used.
        /// </summary>
        [TemplateInstance(TemplateInstance.Single)]
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue(null), Browsable(false)]
        public ITemplate LayoutTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Form submit button.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FormSubmitButton SubmitButton
        {
            get
            {
                return mSubmitButton ?? (mSubmitButton = new FormSubmitButton
                {
                    ID = "btnOK",
                    ResourceString = "general.submit",
                    CssClass = FormButtonCssClass,
                    CausesValidation = false,
                    EnableViewState = false,
                    Form = this
                });
            }
            set
            {
                mSubmitButton = value;
            }
        }


        /// <summary>
        /// Form submit image button.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ImageButton SubmitImageButton
        {
            get
            {
                return mSubmitImageButton ?? (mSubmitImageButton = new ImageButton
                {
                    ID = "btnOK",
                    CausesValidation = false,
                    CssClass = FormButtonCssClass,
                    AlternateText = ResHelper.GetString("general.ok"),
                    EnableViewState = false
                });
            }
            set
            {
                mSubmitImageButton = value;
            }
        }


        /// <summary>
        /// Information label.
        /// </summary>
        public virtual Label InfoLabel
        {
            get;
            set;
        }


        /// <summary>
        /// Label for the errors.
        /// </summary>
        public virtual Label ErrorLabel
        {
            get;
            set;
        }


        /// <summary>
        /// Messages placeholder
        /// </summary>
        public virtual MessagesPlaceHolder MessagesPlaceHolder
        {
            get;
            set;
        }


        /// <summary>
        /// Overrides the generation of the SPAN tag with custom tag.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (mTagKey == null)
                {
                    mTagKey = SettingsKeyInfoProvider.GetValue("CMSControlElement", SiteName).Trim().Equals("div", StringComparison.OrdinalIgnoreCase) ?
                        HtmlTextWriterTag.Div : HtmlTextWriterTag.Span;
                }
                return (HtmlTextWriterTag)mTagKey;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Control's UI Context
        /// </summary>
        public UIContext UIContext
        {
            get
            {
                return UIContextHelper.GetUIContext(this);
            }
        }


        /// <summary>
        /// Returns form outer panel.
        /// </summary>
        public Panel FormPanel
        {
            get
            {
                return mFormPanel ?? (mFormPanel = new Panel
                {
                    ID = "pnlForm"
                });
            }
            private set
            {
                mFormPanel = value;
            }
        }


        /// <summary>
        /// Returns panel surrounding submit button if generated by automatic layouts, otherwise returns null.
        /// </summary>
        public Panel FormButtonPanel
        {
            get;
            internal set;
        }


        /// <summary>
        /// Set width of all labels according to the widest label.
        /// This property is applied only if DefaultFormLayout is set to <see cref="FormLayoutEnum.Divs"/>.
        /// </summary>
        public bool AutomaticLabelWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if error message should be displayed after validation failed.
        /// </summary>
        public bool ShowValidationErrorMessage
        {
            get;
            set;
        }


        /// <summary>
        /// Default validation error message.
        /// Shown when <see cref="ValidationErrorMessage"/> is not set manually.
        /// </summary>
        protected virtual string DefaultValidationErrorMessage
        {
            get
            {
                return ResHelper.GetString(mErrorMessagePrefix + ".errorvalidationerror|general.errorvalidationerror");
            }
        }


        /// <summary>
        /// Optional error message displayed after validation failed.
        /// </summary>
        public string ValidationErrorMessage
        {
            get
            {
                if (string.IsNullOrEmpty(mValidationErrorMessage))
                {
                    // Check if context is already initialized
                    if (CMSHttpContext.Current == null)
                    {
                        return null;
                    }
                    // Return default error message
                    return DefaultValidationErrorMessage;
                }

                return mValidationErrorMessage;
            }
            set
            {
                mValidationErrorMessage = value;
            }
        }


        /// <summary>
        /// Form context resolver.
        /// </summary>
        public virtual MacroResolver ContextResolver
        {
            get
            {
                if (mContextResolver == null)
                {
                    mContextResolver = MacroContext.CurrentResolver.CreateChild();
                    // Set current culture for resolving
                    mContextResolver.Culture = Thread.CurrentThread.CurrentUICulture.ToString();
                }
                return mContextResolver;
            }
            set
            {
                mContextResolver = value;
            }
        }


        /// <summary>
        /// If true, labels of the controls are processed to have the first letter upper case.
        /// </summary>
        public bool EnsureFirstLetterUpperCase
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the current display context which is used if control contains DisplayIn value.
        /// </summary>
        public string DisplayContext
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether to display the "Edit value" button in EditingFormControls.
        /// </summary>
        public bool AllowMacroEditing
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates in what control is this basic form used.
        /// </summary>
        public FormTypeEnum FormType
        {
            get
            {
                return mFormType;
            }
            set
            {
                mFormType = value;
            }
        }


        /// <summary>
        /// Indicates if the form control is enabled.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;

                // Set the inner controls enabled status
                if (FieldEditingControls != null)
                {
                    foreach (DictionaryEntry entry in FieldEditingControls)
                    {
                        if (entry.Value is EditingFormControl control)
                        {
                            control.Enabled = base.Enabled;

                            var field = FormInformation.GetFormField(entry.Key.ToString());
                            if (field != null)
                            {
                                control.Enabled &= field.Enabled;
                            }
                        }
                    }
                }

                if (FieldControls != null)
                {
                    foreach (DictionaryEntry entry in FieldControls)
                    {
                        if (entry.Value is FormEngineUserControl control)
                        {
                            control.Enabled = base.Enabled;

                            var field = FormInformation.GetFormField(entry.Key.ToString());
                            if (field != null)
                            {
                                control.Enabled &= field.Enabled;
                            }
                        }
                    }
                }
                if (SubmitButton != null)
                {
                    SubmitButton.Enabled = base.Enabled;
                }
                if (SubmitImageButton != null)
                {
                    SubmitImageButton.Enabled = base.Enabled;
                }
            }
        }


        /// <summary>
        /// Indicates whether to show private fields or not.
        /// </summary>
        public virtual bool ShowPrivateFields
        {
            get
            {
                return mShowPrivateFields;
            }
            set
            {
                mShowPrivateFields = value;
            }
        }


        /// <summary>
        /// Definition of the form layout.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FormLayout
        {
            get;
            set;
        }


        /// <summary>
        /// Form layout type.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LayoutTypeEnum FormLayoutType
        {
            get
            {
                return mFormLayoutType;
            }
            set
            {
                mFormLayoutType = value;
            }
        }


        /// <summary>
        /// Gets a dictionary that provides additional context data for form controls.
        /// </summary>
        public IDictionary<string, object> AdditionalData
        {
            get
            {
                return mAdditionalData ?? (mAdditionalData = new Dictionary<string, object>());
            }
        }


        /// <summary>
        /// Hashtable with IsMacro flags.
        /// </summary>
        public Hashtable MacroTable
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets macro resolver name.
        /// </summary>
        public string ResolverName
        {
            get
            {
                return ContextResolver.ResolverName;
            }
            set
            {
                ContextResolver.ResolverName = value;
            }
        }


        /// <summary>
        /// Controls associated with FormItems.
        /// </summary>
        public Dictionary<IDataDefinitionItem, List<Control>> AssociatedControls
        {
            get;
            set;
        }


        /// <summary>
        /// List of fields to hide (empty by default).
        /// </summary>
        public List<string> FieldsToHide
        {
            get
            {
                return mFieldsToHide ?? (mFieldsToHide = new List<string>());
            }
        }


        /// <summary>
        /// HTML editor toolbar set name.
        /// </summary>
        public string HtmlAreaToolbar
        {
            get
            {
                return mHtmlAreaToolbar;
            }
            set
            {
                mHtmlAreaToolbar = value ?? String.Empty;
            }
        }


        /// <summary>
        /// HTML editor toolbar location.
        /// </summary>
        public string HtmlAreaToolbarLocation
        {
            get
            {
                return mHtmlAreaToolbarLocation;
            }
            set
            {
                mHtmlAreaToolbarLocation = value ?? String.Empty;
            }
        }


        /// <summary>
        /// Show image button flag.
        /// </summary>
        public bool ShowImageButton
        {
            get
            {
                if (mShowImageButton == null)
                {
                    mShowImageButton = SubmitImageButton.ImageUrl.Trim() != String.Empty;
                }

                return mShowImageButton.Value;
            }
            set
            {
                mShowImageButton = value;
            }
        }


        /// <summary>
        /// Current SiteName.
        /// </summary>
        public string SiteName
        {
            get
            {
                if (ViewState["SiteName"] != null)
                {
                    string siteName = Convert.ToString(ViewState["SiteName"]);
                    if (!String.IsNullOrEmpty(siteName.Trim()))
                    {
                        return siteName;
                    }
                }

                if ((Context != null) && (SiteContext.CurrentSite != null))
                {
                    return SiteContext.CurrentSiteName;
                }
                return String.Empty;
            }
            set
            {
                ViewState["SiteName"] = value;
            }
        }


        /// <summary>
        /// If true, colon(:) is placed behind field label.
        /// </summary>
        public bool UseColonBehindLabel
        {
            get
            {
                return mUseColonBehindLabel;
            }
            set
            {
                mUseColonBehindLabel = value;
            }
        }


        /// <summary>
        /// If true, required field labels are marked with resource string.
        /// </summary>
        public bool MarkRequiredFields
        {
            get;
            set;
        }


        /// <summary>
        /// If true, default values will be set to the disabled fields. Default value is true in insert mode and false in edit mode.
        /// </summary>
        public bool SetDefaultValuesToDisabledFields
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["SetDefaultValuesToDisabledFields"], IsInsertMode);
            }
            set
            {
                ViewState["SetDefaultValuesToDisabledFields"] = value;
            }
        }


        /// <summary>
        /// Identifies the instance of form.
        /// </summary>
        public Guid FormGUID
        {
            get
            {
                Guid guid = ValidationHelper.GetGuid(ViewState["FormGUID"], Guid.Empty);
                if (guid == Guid.Empty)
                {
                    guid = Guid.NewGuid();
                    ViewState["FormGUID"] = guid;
                }
                return guid;
            }
            set
            {
                ViewState["FormGUID"] = value;
            }
        }


        /// <summary>
        /// Form layout.
        /// </summary>
        [Category("Behavior"), Description("Default form layout")]
        public FormLayoutEnum DefaultFormLayout
        {
            get
            {
                if (ViewState["DefaultFormLayout"] == null)
                {
                    return FormLayoutEnum.Divs;
                }
                return ((FormLayoutEnum)(ViewState["DefaultFormLayout"]));
            }
            set
            {
                ViewState["DefaultFormLayout"] = value;

                // Clear layout style configuration when layout changed
                mLayoutStyleConfiguration = null;
            }
        }


        /// <summary>
        /// Default field layout.
        /// </summary>
        [Category("Behavior"), Description("Default field layout of individual fields")]
        public FieldLayoutEnum DefaultFieldLayout
        {
            get;
            set;
        }


        /// <summary>
        /// Default category name.
        /// </summary>
        [Category("Behavior"), Description("Default category name.")]
        public string DefaultCategoryName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["DefaultCategoryName"], String.Empty);
            }
            set
            {
                ViewState["DefaultCategoryName"] = value;
            }
        }


        /// <summary>
        /// If true, system fields are hidden.
        /// </summary>
        public bool HideSystemFields
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["HideSystemFields"], false);
            }
            set
            {
                ViewState["HideSystemFields"] = value;
            }
        }


        /// <summary>
        /// Additional parameters for the dialog windows.
        /// </summary>
        public string DialogParameters
        {
            get;
            set;
        }


        /// <summary>
        /// Edited object. TreeNode in CMSForm, IDataClass in BizForm and CustomTableForm,
        /// AbstractInfo or DataClassInfo in DataForm.
        /// </summary>
        public object EditedObject
        {
            get;
            set;
        }


        /// <summary>
        /// Parent object. Setup in CMSFrom in insert mode, contains TreeNode.
        /// </summary>
        public object ParentObject
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if field visibility should be applied.
        /// </summary>
        public bool ApplyVisibility
        {
            get;
            set;
        }


        /// <summary>
        /// This name is used if ApplyVisibility is 'true' to get visibility definition of current user.
        /// </summary>
        public string VisibilityFormName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if field visibility could be edited.
        /// </summary>
        public bool AllowEditVisibility
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if external editing should be allowed for the controls.
        /// </summary>
        public bool AllowExternalEditing
        {
            get
            {
                return mAllowExternalEditing;
            }
            set
            {
                mAllowExternalEditing = value;
            }
        }


        /// <summary>
        /// Indicates that field should be checked on emptiness in validation step by BasicForm. Default TRUE.
        /// It is preferable to set to FALSE for controls with complex input such as filter form.
        /// </summary>
        public bool CheckFieldEmptiness
        {
            get
            {
                return mCheckFieldEmptiness;
            }
            set
            {
                mCheckFieldEmptiness = value;
            }
        }


        /// <summary>
        /// URL to which the form redirects after successful save.
        /// </summary>
        public string RedirectUrlAfterSave
        {
            get;
            set;
        }


        /// <summary>
        /// Heading level for the field group caption.
        /// </summary>
        public int FieldGroupHeadingLevel
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value indicating whether heading for the field group caption is anchor.
        /// </summary>
        public bool FieldGroupHeadingIsAnchor
        {
            get;
            set;
        }


        /// <summary>
        /// CSS class which will be used to wrap form control.
        /// </summary>
        public string FormCssClass
        {
            get
            {
                string cssClass = mFormCssClass ?? LayoutStyleConfiguration.FormCssClass;
                if (IsDesignMode)
                {
                    cssClass += " editing-form-design-mode";
                }

                return cssClass;
            }
            set
            {
                mFormCssClass = value;
            }
        }


        /// <summary>
        /// CSS class for the whole field (usually row).
        /// </summary>
        public string FieldCssClass
        {
            get
            {
                return mFieldCssClass ?? LayoutStyleConfiguration.FieldCssClass;
            }
            set
            {
                mFieldCssClass = value;
            }
        }


        /// <summary>
        /// CSS class for the whole field group with heading.
        /// </summary>
        public string GroupCssClass
        {
            get
            {
                return mGroupCssClass ?? LayoutStyleConfiguration.GroupCssClass;
            }
            set
            {
                mGroupCssClass = value;
            }
        }


        /// <summary>
        /// CSS class for the whole field group.
        /// </summary>
        public string FieldGroupCssClass
        {
            get
            {
                return mFieldGroupCssClass ?? LayoutStyleConfiguration.FieldGroupCssClass;
            }
            set
            {
                mFieldGroupCssClass = value;
            }
        }


        /// <summary>
        /// CSS class for the field group caption.
        /// </summary>
        public string FieldGroupCaptionCssClass
        {
            get
            {
                return mFieldGroupCaptionCssClass ?? LayoutStyleConfiguration.FieldGroupCaptionCssClass;
            }
            set
            {
                mFieldGroupCaptionCssClass = value;
            }
        }


        /// <summary>
        /// CSS class for the individual field control cell.
        /// </summary>
        public string FieldValueCellCssClass
        {
            get
            {
                return mFieldValueCellCssClass ?? LayoutStyleConfiguration.FieldValueCellCssClass;
            }
            set
            {
                mFieldValueCellCssClass = value;
            }
        }


        /// <summary>
        /// CSS class for the individual field label cell.
        /// </summary>
        public string FieldCaptionCellCssClass
        {
            get
            {
                return mFieldCaptionCellCssClass ?? LayoutStyleConfiguration.FieldCaptionCellCssClass;
            }
            set
            {
                mFieldCaptionCellCssClass = value;
            }
        }


        /// <summary>
        /// CSS class for the individual field label.
        /// </summary>
        public string FieldCaptionCssClass
        {
            get
            {
                return mFieldCaptionCssClass ?? LayoutStyleConfiguration.FieldCaptionCssClass;
            }
            set
            {
                mFieldCaptionCssClass = value;
            }
        }


        /// <summary>
        /// CSS class for the individual field error label cell.
        /// </summary>
        public string FieldErrorCellCssClass
        {
            get
            {
                return mFieldErrorCellCssClass ?? LayoutStyleConfiguration.FieldErrorCellCssClass;
            }
            set
            {
                mFieldErrorCellCssClass = value;
            }
        }


        /// <summary>
        /// CSS class for the individual field error label.
        /// </summary>
        public string FieldErrorLabelCssClass
        {
            get
            {
                return mFieldErrorLabelCssClass ?? LayoutStyleConfiguration.FieldErrorLabelCssClass;
            }
            set
            {
                mFieldErrorLabelCssClass = value;
            }
        }


        /// <summary>
        /// CSS class that is added to editing form control after its validation failed. Default value is "Error".
        /// </summary>
        public string FieldErrorCssClass
        {
            get
            {
                string cssClass = mFieldErrorCssClass ?? LayoutStyleConfiguration.FieldErrorCssClass;
                if (String.IsNullOrEmpty(cssClass))
                {
                    cssClass = "Error";
                }
                return cssClass;
            }
            set
            {
                mFieldErrorCssClass = value;
            }
        }


        /// <summary>
        /// CSS class for the individual field visibility cell.
        /// </summary>
        public string FieldVisibilityCellCssClass
        {
            get
            {
                return mFieldVisibilityCellCssClass ?? LayoutStyleConfiguration.FieldVisibilityCellCssClass;
            }
            set
            {
                mFieldVisibilityCellCssClass = value;
            }
        }


        /// <summary>
        /// CSS class for the individual field visibility control.
        /// </summary>
        public string FieldVisibilityCssClass
        {
            get
            {
                return mFieldVisibilityCssClass ?? LayoutStyleConfiguration.FieldVisibilityCssClass;
            }
            set
            {
                mFieldVisibilityCssClass = value;
            }
        }


        /// <summary>
        /// CSS class that is added to container that is wrapping content after text (only if content before text is empty). Default value is "ExplanationText".
        /// </summary>
        public string ExplanationTextCssClass
        {
            get
            {
                return mExplanationTextCssClass ?? LayoutStyleConfiguration.ExplanationTextCssClass;
            }
            set
            {
                mExplanationTextCssClass = value;
            }
        }


        /// <summary>
        /// CSS class for the form button.
        /// </summary>
        public string FormButtonCssClass
        {
            get
            {
                return mFormButtonCssClass ?? LayoutStyleConfiguration.FormButtonCssClass;
            }
            set
            {
                mFormButtonCssClass = value;
            }
        }


        /// <summary>
        /// CSS class for the form button panel.
        /// </summary>
        public string FormButtonPanelCssClass
        {
            get
            {
                return mFormButtonPanelCssClass ?? LayoutStyleConfiguration.FormButtonPanelCssClass;
            }
            set
            {
                mFormButtonPanelCssClass = value;
            }
        }


        /// <summary>
        /// Forces categories to get to default collapsed state.
        /// </summary>
        public bool ForceReloadCategories
        {
            get;
            set;
        }


        /// <summary>
        /// Image URL for expanding a category.
        /// </summary>
        public string ExpandCategoryImageUrl
        {
            get
            {
                return mExpandCategoryImageUrl ?? (mExpandCategoryImageUrl = UIHelper.GetImageUrl(null, "CMSModules/CMS_PortalEngine/WebpartProperties/plus.png"));
            }
            set
            {
                mExpandCategoryImageUrl = value;
            }
        }


        /// <summary>
        /// Image URL for collapsing a category.
        /// </summary>
        public string CollapseCategoryImageUrl
        {
            get
            {
                return mCollapseCategoryImageUrl ?? (mCollapseCategoryImageUrl = UIHelper.GetImageUrl(null, "CMSModules/CMS_PortalEngine/WebpartProperties/minus.png"));
            }
            set
            {
                mCollapseCategoryImageUrl = value;
            }
        }


        /// <summary>
        /// Determines whether to allow mode switching (simple &lt;-&gt; advanced).
        /// </summary>
        public bool AllowModeSwitch
        {
            get;
            set;
        }


        /// <summary>
        /// Returns if the form is in simple or advanced mode. Depends on <see cref="AllowModeSwitch"/> property.
        /// </summary>
        public bool IsSimpleMode
        {
            get;
            set;
        }


        /// <summary>
        /// Layout of the form
        /// </summary>
        public AbstractLayout Layout
        {
            get;
            protected set;
        }


        /// <summary>
        /// First field element.
        /// </summary>
        public FormField FirstField
        {
            get;
            protected set;
        }


        /// <summary>
        /// Last field element.
        /// </summary>
        public FormField LastField
        {
            get;
            protected set;
        }


        /// <summary>
        /// Determines whether the form is in design mode.
        /// </summary>
        public bool IsDesignMode
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if inner controls are loaded for the first time (the form may be initialized on a postback).
        /// </summary>
        public bool IsFirstLoad
        {
            get
            {
                if (mIsFirstLoad == null)
                {
                    mIsFirstLoad = (ViewState["IsFirstLoad"] == null);
                }

                return mIsFirstLoad.Value;
            }
            private set
            {
                if (ViewState["IsFirstLoad"] == null)
                {
                    ViewState["IsFirstLoad"] = mIsFirstLoad = value;
                }
            }
        }

        #endregion


        #region "Other properties"

        /// <summary>
        /// Collections of fields' visibility (key = field name, value = field's evaluated visibility)
        /// Visibility of the field which uses value from other field in its visibility macro expression might be different from its final visibility.
        /// </summary>
        private StringSafeDictionary<bool> FieldsVisibility
        {
            get
            {
                return mFieldsVisibility ?? (mFieldsVisibility = GetFieldsVisibility());
            }
        }


        /// <summary>
        /// DataClassInfo of edited object.
        /// </summary>
        protected DataClassInfo ClassInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Form information.
        /// </summary>
        public override FormInfo FormInformation
        {
            get
            {
                return base.FormInformation;
            }
            set
            {
                base.FormInformation = value;

                if (value != null)
                {
                    // Set current context resolver
                    base.FormInformation.ContextResolver = ContextResolver;
                }
            }
        }


        /// <summary>
        /// Alternative form information.
        /// </summary>
        public AlternativeFormInfo AltFormInformation
        {
            get;
            set;
        }


        /// <summary>
        /// Style-set used by form. Styles are based on <see cref="DefaultFormLayout"/> and <see cref="FormDefaultStyle"/>.
        /// </summary>
        private IFormStyleConfiguration LayoutStyleConfiguration
        {
            get
            {
                return mLayoutStyleConfiguration ?? (mLayoutStyleConfiguration = GetStyleConfiguration());
            }
        }


        /// <summary>
        /// Default form-specific style configuration.
        /// </summary>
        protected virtual IFormStyleConfiguration FormDefaultStyle
        {
            get
            {
                return mFormDefaultStyle ?? (mFormDefaultStyle = new BasicFormDefaultStyle());
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public BasicForm()
        {
            DefaultFieldLayout = FieldLayoutEnum.TwoColumns;

            ShowValidationErrorMessage = true;

            FieldGroupHeadingIsAnchor = true;
            FieldGroupHeadingLevel = 4;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">Source data</param>
        public BasicForm(IDataContainer data)
            : this()
        {
            Data = data;
        }

        #endregion


        #region "Control events"

        /// <summary>
        /// Pre-render event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Set form CSS class after layout has been decided
            FormPanel.CssClass = FormCssClass;

            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                FormPanel.Visible = false;
                return;
            }

            if (ShowImageButton)
            {
                SubmitImageButton.Enabled = Enabled;
            }
            else
            {
                SubmitButton.Enabled = Enabled;
            }

            if (FormButtonPanel != null)
            {
                // Hide button panel if exists and buttons are hidden
                FormButtonPanel.Visible = (ShowImageButton || SubmitButton.Visible);
            }

            // Display / hide the info and error labels
            if (InfoLabel != null)
            {
                InfoLabel.Visible = !String.IsNullOrEmpty(InfoLabel.Text);
            }
            if (ErrorLabel != null)
            {
                ErrorLabel.Visible = !String.IsNullOrEmpty(ErrorLabel.Text);
            }

            // Process macros
            ProcessMacros();

            // Connect associated controls
            ConnectAssociatedControls();
        }


        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                output.Write("[BasicForm: {0}]", ID);
            }
            else
            {
                base.Render(output);
            }
        }


        /// <summary>
        /// Saves form data into Data container after the submit button is clicked.
        /// </summary>
        protected void SaveDataClick(object sender, EventArgs e)
        {
            SaveData(RedirectUrlAfterSave);
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Initializes the form.
        /// </summary>
        protected override void InitializeControl()
        {
            if (InitCompleted || StopProcessing)
            {
                return;
            }

            // Init macro resolver
            InitResolver();

            // Connect main form labels with messages place holder
            EnsureLabels();

            // Initialize the form in inherited form controls
            InitFormInternal();

            if (StopProcessing)
            {
                return;
            }

            // Init main form panels
            InitializeMainPanel();

            InitCompleted = true;

            // Load form
            LoadData(Data);

            // Here some additional components can be inserted
            AddControlsAfterInternal();

            // Init submit button event
            if (ShowImageButton)
            {
                SubmitImageButton.Click += SaveDataClick;
            }
            else
            {
                SubmitButton.Click += SaveDataClick;
            }
        }


        /// <summary>
        /// Internal method to initialize the form.
        /// </summary>
        protected virtual void InitFormInternal()
        {
            // No action in base class
        }


        /// <summary>
        /// Internal method to add components before the form from inherited forms.
        /// </summary>
        protected virtual void AddControlsBeforeInternal()
        {
            // No action in base class
        }


        /// <summary>
        /// Internal method to add components after the form from inherited forms.
        /// </summary>
        protected virtual void AddControlsAfterInternal()
        {
            // No action in base class
        }


        /// <summary>
        /// Initializes main form container.
        /// </summary>
        /// <param name="resetControls">Indicates if the controls collection should be cleared</param>
        protected void InitializeMainPanel(bool resetControls = false)
        {
            if (resetControls)
            {
                Controls.Clear();
                FormPanel = null;
                FirstField = null;
                LastField = null;
            }

            // Here an additional control can be inserted before the form
            AddControlsBeforeInternal();

            // Initialize main panel
            Controls.Add(FormPanel);
        }


        /// <summary>
        /// Changes fields' visible attribute according to user visibility settings or changes form definition if visibility can be edited.
        /// </summary>
        protected void ProcessUserVisibility()
        {
            // Apply field visibility if enabled and user info is edited
            if (ApplyVisibility && Data.ContainsColumn("UserVisibility"))
            {
                // Get visibility info for specific form name from user's visibility definition
                string userVisibility = ValidationHelper.GetString(Data.GetValue("UserVisibility"), String.Empty);
                visibilityInfo = new FormVisibilityInfo(userVisibility, VisibilityFormName);

                // Merge user visibility with form definition
                FormInformation = FormHelper.MergeVisibility(FormInformation, visibilityInfo.GetFormInfo(), false);

                // Changes fields' visible attribute according to visibility
                ProcessVisibility();
            }

            // Prepare fields' visibility attributes if visibility editing enabled
            if (AllowEditVisibility && Data.ContainsColumn("UserVisibility"))
            {
                // Initialize hashtable
                FieldVisibilityControls = new Hashtable();

                // Get visibility info for specific form name from user's visibility definition
                string userVisibility = ValidationHelper.GetString(Data.GetValue("UserVisibility"), String.Empty);
                visibilityInfo = new FormVisibilityInfo(userVisibility, VisibilityFormName);

                // Merge user visibility with form definition
                FormInformation = FormHelper.MergeVisibility(FormInformation, visibilityInfo.GetFormInfo(), false);
            }
        }


        /// <summary>
        /// Changes fields' visible attribute according to visibility.
        /// </summary>
        protected void ProcessVisibility()
        {
            if (FormInformation == null)
            {
                return;
            }
            foreach (FormFieldInfo ffi in FormInformation.GetFields(true, true))
            {
                // Control is by default invisible
                ffi.Visible = false;
                switch (ffi.Visibility)
                {
                    // Set control to be invisible always
                    case FormFieldVisibilityTypeEnum.None:
                        ffi.Visible = false;
                        break;

                    // Set control to be visible always
                    case FormFieldVisibilityTypeEnum.All:
                        ffi.Visible = true;
                        break;

                    // Set control to be visible only if current user is authenticated
                    case FormFieldVisibilityTypeEnum.Authenticated:
                        if (AuthenticationHelper.IsAuthenticated())
                        {
                            ffi.Visible = true;
                        }
                        break;
                }
            }
        }


        /// <summary>
        /// Changes fields' visible attribute according to display context.
        /// </summary>
        protected void ProcessContextVisibility()
        {
            if (FormInformation == null)
            {
                return;
            }

            foreach (FormFieldInfo ffi in FormInformation.GetFields(true, true))
            {
                // Check display context
                if (!String.IsNullOrEmpty(ffi.DisplayIn) && (CMSString.Compare(DisplayContext, ffi.DisplayIn) != 0))
                {
                    ffi.Visible = false;
                }
            }
        }


        /// <summary>
        /// Initializes macro resolver data sources.
        /// </summary>
        protected virtual void InitResolver()
        {
            var resolver = ContextResolver;

            // Add form to resolver
            resolver.SetNamedSourceDataCallback("Form", r => new FormMacroContainer<BasicForm>(this));

            // Add data to resolver
            resolver.SetNamedSourceDataCallback("Data", r => Data);

            // Add form mode information
            resolver.SetNamedSourceDataCallback("FormMode", r => Mode);

            // Add edited and parent objects
            resolver.SetNamedSourceDataCallback("EditedObject", r => EditedObject);
            resolver.SetNamedSourceDataCallback("ParentObject", r => ParentObject);

            // Register UIContext
            resolver.SetNamedSourceDataCallback("UIContext", r => UIContext);

            FormEngineWebUIResolvers.RegisterFormResolverEnumData(resolver);

            // Fill resolver with visible form controls
            resolver.SetNamedSourceDataCallback("Fields", r =>
            {
                var fields = new StringSafeDictionary<FormControlMacroContainer>();
                if (FieldControls != null)
                {
                    foreach (string name in FieldControls.TypedKeys)
                    {
                        fields[name] = new FormControlMacroContainer(FieldControls[name]);
                        string guid = FormInformation.GetFormField(name).Guid.ToString();
                        fields[guid] = fields[name];
                    }
                }

                return new SafeDictionaryContainer<FormControlMacroContainer>(fields);
            });

            // Fill resolver with form categories
            resolver.SetNamedSourceDataCallback("Categories", r =>
            {
                StringSafeDictionary<FormCategoryInfo> categories = new StringSafeDictionary<FormCategoryInfo>();

                var formCategories = FormInformation.ItemsList.OfType<FormCategoryInfo>().Where(c => !String.IsNullOrEmpty(c.CategoryName));

                foreach (var categoryInfo in formCategories)
                {
                    categories[categoryInfo.CategoryName] = categoryInfo;
                }
                return new SafeDictionaryContainer<FormCategoryInfo>(categories);
            });

            resolver.OnGetValue += ContextResolver_OnGetValue;
        }


        /// <summary>
        /// Provides the connection to particular fields by their name
        /// </summary>
        /// <param name="name">Value name</param>
        private object ContextResolver_OnGetValue(string name)
        {
            // Get the field control and wrap it into macro container
            var ctrl = FieldControls?[name];
            if (ctrl != null)
            {
                return new FormControlMacroContainer(ctrl);
            }

            return null;
        }


        /// <summary>
        /// Connects associated controls (labels and inputs) together, for accessibility and other reasons.
        /// </summary>
        protected void ConnectAssociatedControls()
        {
            if (FieldControls != null)
            {
                // Add associated ID to field labels
                foreach (DictionaryEntry field in FieldControls)
                {
                    var ctrl = field.Value as Control;
                    var name = (string)field.Key;

                    // Try to get form field info
                    FormFieldInfo ffi = null;
                    if (FormInformation != null)
                    {
                        ffi = FormInformation.GetFormField(name);
                    }

                    // Get label from hashtable
                    LocalizedLabel ctrlLabel = FieldLabels[name];
                    if (ctrlLabel != null)
                    {
                        // Initialize proper associated ID
                        if ((ctrl is WebControl) && ((ctrl is TextBox) || (ctrl is RadioButton) || (ctrl is CheckBox) || (ctrl is ListBox) || (ctrl is DropDownList)))
                        {
                            // Standard web controls
                            ctrlLabel.AssociatedControlID = ((WebControl)ctrl).ID;
                        }
                        else if (ctrl is FormEngineUserControl)
                        {
                            // Form engine user control
                            FormEngineUserControl fuc = (FormEngineUserControl)ctrl;
                            fuc.EnsureControls();

                            if (fuc.InputClientID != null)
                            {
                                ctrlLabel.AssociatedControlClientID = fuc.InputClientID;
                            }
                            else if ((ffi != null) && (ffi.FieldType != FormFieldControlTypeEnum.CustomUserControl))
                            {
                                ctrlLabel.AssociatedControlID = fuc.ID;
                            }
                        }
                        else if ((ctrl is WebControl) && !(ctrl is CheckBoxList))
                        {
                            // Web control
                            ctrlLabel.AssociatedControlClientID = EditingFormControl.GetInputClientID(ctrl.Controls);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Initializes values for invisible fields.
        /// </summary>
        /// <param name="useDefaultValue">Determines whether fields should be loaded with their default values or with the actual ones</param>
        protected virtual void HandleFieldsValues(bool useDefaultValue)
        {
            if (FormInformation != null)
            {
                // Process invisible fields
                foreach (FormFieldInfo ffi in FormInformation.GetFields<FormFieldInfo>().Where(x => !x.Visible || (!ShowPrivateFields && !x.PublicField) || (HideSystemFields && x.System)))
                {
                    try
                    {
                        // Get value
                        object value = DataHelper.GetNull(Data.GetValue(ffi.Name));
                        object defaultValue;

                        // Load default value for invisible fields when in insert mode
                        if (useDefaultValue)
                        {
                            // Get field default value and try to resolve macros
                            string defValue = GetDefaultValue(ffi);

                            if ((ffi.DataType == FieldDataType.DateTime) && DateTimeHelper.IsNowOrToday(defValue))
                            {
                                // Resolve ##NOW## or ##TODAY## macros
                                defaultValue = TimeZoneMethods.DateTimeConvert(DateTime.Now, TimeZoneType, CustomTimeZone);
                            }
                            else
                            {
                                // Use given default value
                                defaultValue = DataTypeManager.ConvertToSystemType(TypeEnum.Field, ffi.DataType, defValue);
                            }

                            // Always preset default value for a required field to ensure database consistency, if the value is not provided and macro editing is disabled
                            if (!AllowMacroEditing && (value == null) && (!String.IsNullOrEmpty(defValue) || !ffi.AllowEmpty))
                            {
                                // "defaultvalue" is not-null value based on the field's data type
                                Data.SetValue(ffi.Name, defaultValue);
                            }
                        }
                        else
                        {
                            // In editing mode, use current value as default value
                            defaultValue = value;
                        }

                        // Add value to macro resolver
                        var macro = new FormControlMacroContainer
                        {
                            Info = ffi,
                            Value = defaultValue
                        };

                        ContextResolver.SetNamedSourceData(ffi.Name, macro);
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        EventLogProvider.LogException("BasicForm", "HandleFieldsValues", ex);
                    }
                }
            }
        }


        /// <summary>
        /// Internal method to save data to the database.
        /// </summary>
        protected virtual bool SaveDataInternal()
        {
            // No action in base class
            return true;
        }


        /// <summary>
        /// Performs additional actions after save (redirecting or displaying info message).
        /// </summary>
        /// <param name="redirectUrlAfterSave">If specified, user is redirected to this URL after data is successfully saved</param>
        /// <param name="showChangesSaved">If true and redirect URL is not set, Info message "Changes were saved" is displayed</param>
        protected virtual void AfterSaveActionsInternal(string redirectUrlAfterSave, bool showChangesSaved)
        {
            if (String.IsNullOrEmpty(redirectUrlAfterSave) && !String.IsNullOrEmpty(RedirectUrlAfterSave))
            {
                redirectUrlAfterSave = RedirectUrlAfterSave;
            }

            // If redirection URL set, redirect
            if (!String.IsNullOrEmpty(redirectUrlAfterSave))
            {
                // Raise the OnBeforeRedirect event
                RaiseOnBeforeRedirect(this, EventArgs.Empty);

                // Resolve macros in the redirect URL
                redirectUrlAfterSave = ResolveMacros(redirectUrlAfterSave);

                URLHelper.Redirect(UrlResolver.ResolveUrl(redirectUrlAfterSave));
            }
            else if (showChangesSaved && (InfoLabel != null))
            {
                ShowChangesSaved();
            }
        }


        /// <summary>
        /// Invokes OnCheckPermissions event.
        /// </summary>
        protected void RaiseOnCheckPermissions()
        {
            OnCheckPermissions?.Invoke(this, null);
        }


        /// <summary>
        /// Returns CSS class belonging to specified form field element.
        /// </summary>
        /// <param name="formField">Form field configuration</param>
        /// <param name="cssProperty">Property containing CSS class.</param>
        internal string GetFieldCssClass(FormFieldInfo formField, FormFieldPropertyEnum cssProperty)
        {
            string cssClass = formField?.GetPropertyValue(cssProperty, ContextResolver);
            if (String.IsNullOrEmpty(cssClass))
            {
                switch (cssProperty)
                {
                    case FormFieldPropertyEnum.CaptionCssClass:
                        cssClass = FieldCaptionCssClass;
                        break;

                    case FormFieldPropertyEnum.CaptionCellCssClass:
                        cssClass = FieldCaptionCellCssClass;
                        break;

                    case FormFieldPropertyEnum.ControlCellCssClass:
                        cssClass = FieldValueCellCssClass;
                        break;

                    case FormFieldPropertyEnum.FieldCssClass:
                        cssClass = FieldCssClass;
                        break;
                }
            }

            return cssClass;
        }

        #endregion


        #region "Layout methods"

        /// <summary>
        /// Loads the form layout.
        /// </summary>
        protected void LoadLayout()
        {
            IsFirstLoad = true;

            if (String.IsNullOrEmpty(FormLayout) && (ClassInfo != null))
            {
                // Use base class form layout
                FormLayout = ClassInfo.ClassFormLayout;
                FormLayoutType = ClassInfo.ClassFormLayoutType;
            }

            if (!String.IsNullOrEmpty(AltFormInformation?.FormLayout))
            {
                // Use the alt.form layout
                FormLayout = AltFormInformation.FormLayout;
                FormLayoutType = AltFormInformation.FormLayoutType;
            }

            // Load HTML layout
            if (!String.IsNullOrEmpty(FormLayout) && (FormLayoutType == LayoutTypeEnum.Html))
            {
                LoadFormLayout();
            }
            // Load the layout template (ASCX layout)
            else if ((LayoutTemplate != null) || (!String.IsNullOrEmpty(FormLayout) && (FormLayoutType == LayoutTypeEnum.Ascx)))
            {
                LoadTemplateLayout();
            }
            // Load default layout
            else
            {
                LoadDefaultLayout();
            }
        }


        /// <summary>
        /// Loads the layout template.
        /// </summary>
        protected virtual void LoadTemplateLayout()
        {
            InitializeMainPanel(true);

            // Load template from ASCX custom layout
            if (!String.IsNullOrEmpty(FormLayout) && (FormLayoutType == LayoutTypeEnum.Ascx))
            {
                string url = null;

                if (!String.IsNullOrEmpty(AltFormInformation?.FormLayout) && (AltFormInformation.FormLayoutType == LayoutTypeEnum.Ascx))
                {
                    url = AltFormInformation.Generalized.GetVirtualFileRelativePath(AlternativeFormInfo.EXTERNAL_COLUMN_CODE, AltFormInformation.FormVersionGUID);
                }
                else if (ClassInfo != null)
                {
                    url = ClassInfo.Generalized.GetVirtualFileRelativePath(DataClassInfo.EXTERNAL_COLUMN_CODE, ClassInfo.ClassVersionGUID);
                }

                try
                {
                    LayoutTemplate = Page.LoadTemplate(url);
                }
                catch (Exception ex)
                {
                    ShowError(ResHelper.GetString("cmsform.notinitializederror"), HTMLHelper.HTMLEncode(ex.Message));
                    StopProcessing = true;

                    // Log the error
                    EventLogProvider.LogException("BasicForm", "Layout initialization", ex, (Context != null) ? SiteContext.CurrentSiteID : 0);
                }
            }

            AbstractLayout layout = new LayoutTemplate(this);

            layout.OnAfterRegisterFormControl += layout_OnAfterRegisterFormControl;
            layout.LoadLayout();

            Layout = layout;
        }


        /// <summary>
        /// Event after control has been registered in template layout.
        /// </summary>
        protected virtual void layout_OnAfterRegisterFormControl(object sender, EventArgs e)
        {
            // Setup the field layout
            if (DefaultFieldLayout != FieldLayoutEnum.Default)
            {
                var args = e as ControlRegistrationEventArgs;
                if (args?.ChildControl is FormField formField)
                {
                    // Register first and last fields
                    if (FirstField == null)
                    {
                        FirstField = formField;
                    }

                    LastField = formField;
                }
            }
        }


        /// <summary>
        /// Loads form layout.
        /// </summary>
        protected virtual void LoadFormLayout()
        {
            AbstractLayout layout = new LayoutForm(this);

            layout.LoadLayout();

            Layout = layout;
        }


        /// <summary>
        /// Loads default layout.
        /// </summary>
        protected virtual void LoadDefaultLayout()
        {
            AbstractLayout layout;

            // Load layout
            switch (DefaultFormLayout)
            {
                case FormLayoutEnum.TablePerCategory:
                    layout = new TablePerCategoryDefaultLayout(this, categoryListPanel);
                    break;

                case FormLayoutEnum.FieldSets:
                    layout = new FieldSetDefaultLayout(this, categoryListPanel);
                    break;

                case FormLayoutEnum.Divs:
                    layout = new DivDefaultLayout(this, categoryListPanel);
                    break;

                // Standard and default
                default:
                    layout = new SingleTableDefaultLayout(this, categoryListPanel);
                    break;
            }

            layout.LoadLayout();

            Layout = layout;
        }


        /// <summary>
        /// Ensures the general labels on the form.
        /// </summary>
        protected virtual void EnsureLabels()
        {
            if (MessagesPlaceHolder != null)
            {
                if (InfoLabel == null)
                {
                    InfoLabel = MessagesPlaceHolder.InfoLabel;
                }
                if (ErrorLabel == null)
                {
                    ErrorLabel = MessagesPlaceHolder.ErrorLabel;
                }
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Reloads the form data.
        /// </summary>
        public virtual void ReloadData()
        {
            StopProcessing = false;

            if (!InitCompleted)
            {
                InitializeControl();
            }
            else
            {
                LoadData(Data);

                if ((LayoutTemplate != null) && (FormLayoutType == LayoutTypeEnum.Ascx))
                {
                    // Allow to add additional components in inherited forms
                    AddControlsAfterInternal();
                }
            }
        }


        /// <summary>
        /// Renders form and sets field values.
        /// </summary>
        /// <param name="dr">DataRow containing form data</param>
        public virtual void LoadData(DataRow dr)
        {
            IDataContainer data = new DataRowContainer(dr);
            LoadData(data);
        }


        /// <summary>
        /// Renders form and sets field values.
        /// </summary>
        /// <param name="data">DataContainer containing form data</param>
        public virtual void LoadData(IDataContainer data)
        {
            if ((data == null) || StopProcessing || !InitCompleted)
            {
                return;
            }

            Data = data;

            // Raise the BeforeDataLoad event
            OnBeforeDataLoad?.Invoke(this, null);

            if (StopProcessing)
            {
                return;
            }

            // Set correct time zone for child controls
            if (TimeZoneHelper.TimeZonesEnabled)
            {
                // As timezonemanager is needed before child control is added to basic form's control collection, add timezonemanager to request. See TimeZoneHelper.GetTimeZoneManager(Control) for more info.
                RequestStockHelper.Add("RequestTimeZoneManager", this);

                TimeZoneInfo tzi;
                if (!IsLiveSite)
                {
                    tzi = TimeZoneHelper.GetTimeZoneInfo(MembershipContext.AuthenticatedUser, SiteContext.CurrentSite);
                }
                else
                {
                    TimeZoneUIMethods.GetDateTimeForControl(this, DateTime.Now, out tzi);
                }

                if (tzi != null)
                {
                    TimeZoneType = TimeZoneTypeEnum.Custom;
                    CustomTimeZone = tzi;
                }
            }

            // Fields initialization
            FieldLabels = new StringSafeDictionary<LocalizedLabel>();
            FieldControls = new StringSafeDictionary<FormEngineUserControl>();
            FieldEditingControls = new StringSafeDictionary<EditingFormControl>();
            FieldErrorLabels = new StringSafeDictionary<LocalizedLabel>();
            AssociatedControls = new Dictionary<IDataDefinitionItem, List<Control>>();
            FieldActionsControls = new StringSafeDictionary<FieldActions>();
            FieldUpdatePanels = new StringSafeDictionary<UpdatePanel>();
            Fields.Clear();

            // Reset form panel
            FormPanel.Controls.Clear();

            // Process user visibility
            ProcessUserVisibility();

            // Process context visibility
            ProcessContextVisibility();

            // Handle non-visible fields
            HandleFieldsValues(IsInsertMode);

            SpellCheckFields.Clear();

            // Initialize form layout
            LoadLayout();

            // Ensure basic styles
            if (MessagesPlaceHolder != null)
            {
                MessagesPlaceHolder.BasicStyles = IsLiveSite;
                if (!IsLiveSite)
                {
                    MessagesPlaceHolder.WrapperControlID = FormPanel.ID;
                }
            }

            // Raise the internal DummyFieldDataLoad event
            DummyFieldDataLoad?.Invoke(this, null);

            // Raise the AfterDataLoad event
            OnAfterDataLoad?.Invoke(this, null);
        }


        /// <summary>
        /// Validates the data, returns true if succeeded.
        /// </summary>
        public virtual bool ValidateData()
        {
            if (mIsValid != null)
            {
                return (bool)mIsValid;
            }

            // Set fields' visibility and availability based on macros
            ProcessMacros();

            // Raise the BeforeValidate event
            OnBeforeValidate?.Invoke(this, null);

            if (StopProcessing)
            {
                mIsValid = false;
                return false;
            }

            // Validate the fields
            bool isValid = true;
            if (FieldControls != null)
            {
                // Process all field controls
                foreach (string name in Fields.Where(x => !FieldsToHide.Contains(x)))
                {
                    string validationResult = String.Empty;
                    FormFieldInfo ffi = null;

                    FormEngineUserControl ctrl = FieldControls[name];
                    if (ctrl != null)
                    {
                        // Try to get form field info
                        if (FormInformation != null)
                        {
                            ffi = FormInformation.GetFormField(name);
                            if (ffi == null)
                            {
                                throw new Exception("[BasicForm.ValidateData]: Information for form field '" + name + "' not found.");
                            }
                        }

                        if (ctrl.Enabled || ProcessDisabledFields)
                        {
                            // Validate control's value
                            FormControlValidation controlValidation = new FormControlValidation(FieldEditingControls[name], ctrl, ffi, CheckFieldEmptiness, ContextResolver);
                            ctrl.CheckRegularExpression = ctrl.CheckMinMaxLength = ctrl.Visible;
                            validationResult = controlValidation.Validate();

                            // Custom validation through OnItemValidation event if set
                            if (OnItemValidation != null)
                            {
                                // Pass error to item validation to know whether item is valid or not
                                string valError = validationResult;
                                OnItemValidation(ctrl, ref valError);
                                if (!String.IsNullOrEmpty(valError) && !valError.Equals(validationResult, StringComparison.InvariantCulture))
                                {
                                    // Custom validation error
                                    if (!String.IsNullOrEmpty(validationResult))
                                    {
                                        // Add a space between two error messages
                                        validationResult += " ";
                                    }
                                    validationResult += valError;
                                }
                            }
                        }
                    }
                    // Specified control doesn't exist
                    else
                    {
                        validationResult = ResHelper.GetString("basicform.nocontrol");
                    }

                    // Display error label if validation failed
                    if (DisplayErrorLabel(name, validationResult))
                    {
                        isValid = false;
                    }
                }
            }

            // Raise AfterValidate event
            OnAfterValidate?.Invoke(this, null);

            if (StopProcessing)
            {
                isValid = false;
            }

            // If not valid raise ValidationFailed event
            if (!isValid)
            {
                OnValidationFailed?.Invoke(this, null);

                if (ShowValidationErrorMessage)
                {
                    // Set general error label if available
                    // Messages placeholder info label is used
                    if (ErrorLabel != null)
                    {
                        PrependError(ValidationErrorMessage, "<br /><br />");
                    }
                }
            }

            mIsValid = isValid;

            return (bool)mIsValid;
        }


        /// <summary>
        /// Saves data and optionally redirects user to specified URL.
        /// </summary>
        /// <param name="redirectUrlAfterSave">If specified, user is redirected to this URL after data is successfully saved</param>
        /// <param name="showChangesSaved">If true and redirect URL is not set, Info message "Changes were saved" is displayed</param>
        public bool SaveData(string redirectUrlAfterSave, bool showChangesSaved = true)
        {
            // Validate
            if (!ValidateData())
            {
                return false;
            }

            // Raise CheckPermissions event
            RaiseOnCheckPermissions();

            if (StopProcessing)
            {
                return false;
            }

            // Raise BeforeDataRetrieval event
            OnBeforeDataRetrieval?.Invoke(this, null);

            if (StopProcessing)
            {
                return false;
            }

            // Skip if there are no visible fields
            if (FieldEditingControls != null)
            {
                // Load the fields values
                foreach (string columnName in Data.ColumnNames)
                {
                    FormEngineUserControl ctrl = FieldControls[columnName];
                    EditingFormControl efc = FieldEditingControls[columnName];

                    // Try to get macro value from editing form control
                    if ((efc != null) && (MacroTable != null))
                    {
                        MacroTable[columnName.ToLowerInvariant()] = (efc.IsMacro ? efc.Value : null);
                    }

                    if ((ctrl != null) && (ctrl.HasValue || IsInsertMode) && (ctrl.Enabled || ProcessDisabledFields))
                    {
                        // Save the value (if not macro) into Data container
                        if (MacroTable?[columnName.ToLowerInvariant()] == null)
                        {
                            SaveControlValue(columnName, ctrl);
                        }

                        // Save other values into Data container
                        object[,] returnedValues = ctrl.GetOtherValues();
                        if (returnedValues != null)
                        {
                            for (int valueIndex = 0; valueIndex <= returnedValues.GetUpperBound(0); valueIndex++)
                            {
                                SetDataValue(Convert.ToString(returnedValues[valueIndex, 0]), returnedValues[valueIndex, 1]);
                            }
                        }
                    }

                    // Update user's visibility settings for the form
                    if (AllowEditVisibility && FieldVisibilityControls.Contains(columnName))
                    {
                        // Get visibility control for current field
                        if (FieldVisibilityControls[columnName] is FormEngineUserControl ctrlVisibility)
                        {
                            // Update form visibility info
                            visibilityInfo.SetVisibilityField(columnName, ValidationHelper.GetString(ctrlVisibility.Value, null), VisibilityFormName);
                        }
                    }
                }

                // Update user's visibility definition
                if (AllowEditVisibility && Data.ContainsColumn("UserVisibility") && (visibilityInfo != null))
                {
                    string userVisibility = ValidationHelper.GetString(Data.GetValue("UserVisibility"), String.Empty);
                    userVisibility = FormVisibilityInfo.MergeVisibility(userVisibility, visibilityInfo.GetFormInfo(), VisibilityFormName);
                    Data.SetValue("UserVisibility", userVisibility);
                }
            }

            // Raise BeforeSave event
            OnBeforeSave?.Invoke(this, null);

            if (StopProcessing)
            {
                return false;
            }

            // Save the data internally
            if (SaveDataInternal())
            {
                // Raise the OnAfterSave event
                RaiseOnAfterSave(this, EventArgs.Empty);
                if (StopProcessing)
                {
                    return false;
                }

                // Perform additional actions
                AfterSaveActionsInternal(redirectUrlAfterSave, showChangesSaved);

                return true;
            }
            else
            {
                RaiseOnSaveFailed(this, EventArgs.Empty);
            }

            return false;
        }


        /// <summary>
        /// Gets the value of a specified field.
        /// </summary>
        /// <param name="fieldName">Field name</param>
        public object GetFieldValue(string fieldName)
        {
            // Get the value
            var ctrl = FieldEditingControls[fieldName] ?? (Control)FieldControls[fieldName];

            object value = ctrl != null ? GetControlValue(ctrl) : GetDataValue(fieldName);

            if (value == DBNull.Value)
            {
                value = null;
            }

            return value;
        }


        /// <summary>
        /// Gets the multi-field values of a specified field.
        /// </summary>
        /// <param name="fieldName">Field name</param>
        public object[,] GetMultifieldValues(string fieldName)
        {
            var ctrl = FieldControls?[fieldName];

            return ctrl?.GetOtherValues();
        }


        /// <summary>
        /// Returns true if the given category is collapsed
        /// </summary>
        /// <param name="categoryName">Category name</param>
        public bool IsCategoryCollapsed(string categoryName)
        {
            return Layout.IsCategoryCollapsed(categoryName);
        }


        /// <summary>
        /// Checks if control is included in editing controls hashtable.
        /// </summary>
        /// <param name="fieldName">Name of field with control</param>
        /// <returns>Returns TRUE if field is found. Otherwise returns FALSE.</returns>
        public bool IsFieldAvailable(string fieldName)
        {
            return (FieldEditingControls?[fieldName] != null);
        }


        /// <summary>
        /// Checks if the given field is evaluated as visible.
        /// </summary>
        /// <param name="fieldName">Form field name</param>
        public bool IsFieldVisible(string fieldName)
        {
            return (FieldsVisibility.ContainsKey(fieldName) && FieldsVisibility[fieldName]);
        }


        /// <summary>
        /// Returns TRUE if basic form displays any control.
        /// </summary>
        public bool IsAnyFieldVisible()
        {
            if (FormInformation != null)
            {
                return FormInformation.GetFields(true, false).Any(ffi => ffi.Visible);
            }
            return false;
        }


        /// <summary>
        /// Gets where condition from all form engine user controls.
        /// </summary>
        public string GetWhereCondition()
        {
            if (FieldControls == null || FieldControls?.Values.Count <= 0)
            {
                return null;
            }

            string whereCondition = null;
            // Combine where conditions from all form engine user controls
            foreach (FormEngineUserControl ctrl in FieldControls.Values)
            {
                var condition = ctrl?.GetWhereCondition();
                if (!string.IsNullOrEmpty(condition))
                {
                    whereCondition = SqlHelper.AddWhereCondition(whereCondition, condition);
                }
            }
            return whereCondition;
        }


        /// <summary>
        /// Raises the OnUploadFile event.
        /// </summary>
        public void RaiseOnUploadFile(object sender, EventArgs e)
        {
            OnUploadFile?.Invoke(sender, e);
        }


        /// <summary>
        /// Raises the OnDeleteFile event.
        /// </summary>
        public void RaiseOnDeleteFile(object sender, EventArgs e)
        {
            OnDeleteFile?.Invoke(sender, e);
        }


        /// <summary>
        /// Raises the OnAfterSave event.
        /// </summary>
        public void RaiseOnAfterSave(object sender, EventArgs e)
        {
            OnAfterSave?.Invoke(sender, e);
        }


        /// <summary>
        /// Raises the OnSaveFailed event.
        /// </summary>
        public void RaiseOnSaveFailed(object sender, EventArgs e)
        {
            OnSaveFailed?.Invoke(sender, e);
        }


        /// <summary>
        /// Raises the OnBeforeRedirect event.
        /// </summary>
        public void RaiseOnBeforeRedirect(object sender, EventArgs e)
        {
            OnBeforeRedirect?.Invoke(sender, e);
        }


        /// <summary>
        /// Ensures correct set the messages placeholder.
        /// </summary>
        /// <param name="plcMess">Page messages placeholder</param>
        public void EnsureMessagesPlaceholder(MessagesPlaceHolder plcMess)
        {
            MessagesPlaceHolder = plcMess;
            ErrorLabel = plcMess.ErrorLabel;
            InfoLabel = plcMess.InfoLabel;
        }


        /// <summary>
        /// Resolves the macros within current WebPart context.
        /// </summary>
        /// <param name="inputText">Input text to resolve</param>
        public string ResolveMacros(string inputText)
        {
            return ContextResolver.ResolveMacros(inputText);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Processes macros of form controls and sets their properties.
        /// </summary>
        private void ProcessMacros()
        {
            // Don't process visible and enabled macros in design mode
            if (IsDesignMode)
            {
                return;
            }

            // Initialize dictionary with form controls
            StringSafeDictionary<IFormControl> editingControls = null;
            if (FieldEditingControls != null && FieldEditingControls.Count > 0)
            {
                editingControls = new StringSafeDictionary<IFormControl>(FieldEditingControls);
            }
            else if (FieldControls != null && FieldControls.Count > 0)
            {
                editingControls = new StringSafeDictionary<IFormControl>(FieldControls);
            }
            if (editingControls == null || editingControls.Count == 0 || FormInformation == null)
            {
                return;
            }

            bool firstCategory = true;

            // Change visibility and enabled/disabled setting for each control
            // depending on macro settings; exclude private and system fields if set so
            foreach (KeyValuePair<FormCategoryInfo, List<FormFieldInfo>> categoryKeyValue in FormInformation.GetHierarchicalFormElements(x => (x.PublicField || ShowPrivateFields) && !(HideSystemFields && x.System)))
            {
                FormCategoryInfo category = categoryKeyValue.Key;

                // Get visibility of category
                bool categoryVisible = ValidationHelper.GetBoolean(category.GetPropertyValue(FormCategoryPropertyEnum.Visible, ContextResolver), false);

                bool anyVisibleField = false;

                // Resolve macros for each control in category
                foreach (FormFieldInfo ffi in categoryKeyValue.Value)
                {
                    if (!editingControls.ContainsKey(ffi.Name))
                    {
                        continue;
                    }

                    IFormControl formControl = editingControls[ffi.Name];

                    bool visibleMacroResult = !FieldsToHide.Contains(ffi.Name);
                    string visibleMacro = ffi.GetPropertyValue(FormFieldPropertyEnum.VisibleMacro);

                    if (!String.IsNullOrEmpty(visibleMacro))
                    {
                        visibleMacroResult &= ValidationHelper.GetBoolean(ResolveMacros(visibleMacro), visibleMacroResult);
                    }

                    // Merge field visibility with category visibility
                    visibleMacroResult &= categoryVisible;

                    // Set field visibility with all its associated controls
                    if (AssociatedControls.ContainsKey(ffi))
                    {
                        AssociatedControls[ffi].ForEach(x => x.Visible = visibleMacroResult);
                    }
                    // Set visibility of field's label
                    if (FieldLabels[ffi.Name] != null)
                    {
                        FieldLabels[ffi.Name].Visible = visibleMacroResult;
                    }

                    formControl.Visible = visibleMacroResult;

                    if (!visibleMacroResult && !FieldsToHide.Contains(ffi.Name))
                    {
                        FieldsToHide.Add(ffi.Name);
                    }

                    anyVisibleField |= visibleMacroResult;

                    // Reset values for invisible fields
                    if (!visibleMacroResult && IsInsertMode)
                    {
                        formControl.Value = GetDefaultValue(ffi);
                    }

                    // Enable/disable fields
                    string enabledMacro = ffi.GetPropertyValue(FormFieldPropertyEnum.EnabledMacro);
                    if (!String.IsNullOrEmpty(enabledMacro))
                    {
                        bool enabledMacroResult = ValidationHelper.GetBoolean(ResolveMacros(enabledMacro), ffi.Enabled);

                        formControl.Enabled = ffi.Enabled = enabledMacroResult;
                        if (!enabledMacroResult && IsInsertMode && SetDefaultValuesToDisabledFields)
                        {
                            formControl.Value = GetDefaultValue(ffi);
                        }
                    }
                }

                // Set visibility of category and category associated controls
                if (AssociatedControls.Count > 0)
                {
                    if (AssociatedControls.ContainsKey(category))
                    {
                        AssociatedControls[category].ForEach(x => x.Visible = categoryVisible && anyVisibleField);
                    }
                    else if (firstCategory && (category.CategoryName == null) && (AssociatedControls.First().Key is FormCategoryInfo))
                    {
                        // Process first 'virtual' category
                        AssociatedControls.First().Value.ForEach(x => x.Visible = categoryVisible && anyVisibleField);
                    }
                }

                firstCategory = false;
            }
        }


        /// <summary>
        /// Gets collections of fields' visibility (key = field name, value = field's evaluated visibility)
        /// Visibility of the field which uses value from other field in its visibility macro expression might be different from its final visibility.
        /// </summary>
        private StringSafeDictionary<bool> GetFieldsVisibility()
        {
            var result = new StringSafeDictionary<bool>();

            if (FormInformation != null)
            {
                // Get results of macros to result dictionary
                foreach (KeyValuePair<FormCategoryInfo, List<FormFieldInfo>> categoryKeyValue in FormInformation.GetHierarchicalFormElements(x => (x.PublicField || ShowPrivateFields) && !(HideSystemFields && x.System)))
                {
                    FormCategoryInfo category = categoryKeyValue.Key;

                    // Set visibility of category and category associated controls
                    bool categoryVisible = ValidationHelper.GetBoolean(category.GetPropertyValue(FormCategoryPropertyEnum.Visible, ContextResolver), false);

                    // Resolve macros for each control in category
                    foreach (FormFieldInfo ffi in categoryKeyValue.Value)
                    {
                        bool visibleMacroResult = ffi.Visible && !FieldsToHide.Contains(ffi.Name);
                        string visibleMacro = ffi.GetPropertyValue(FormFieldPropertyEnum.VisibleMacro);
                        if (!String.IsNullOrEmpty(visibleMacro))
                        {
                            visibleMacroResult &= ValidationHelper.GetBoolean(ResolveMacros(visibleMacro), visibleMacroResult);
                        }
                        // Merge field visibility with category visibility
                        visibleMacroResult &= categoryVisible;
                        result.Add(ffi.Name, visibleMacroResult);
                    }
                }
            }

            return result;
        }


        private IFormStyleConfiguration GetStyleConfiguration()
        {
            switch (DefaultFormLayout)
            {
                case FormLayoutEnum.Divs:
                    return new DivLayoutStyle(FormDefaultStyle);

                case FormLayoutEnum.FieldSets:
                    return new FieldSetLayoutStyle(FormDefaultStyle);

                case FormLayoutEnum.SingleTable:
                    return new SingleTableLayoutStyle(FormDefaultStyle);

                case FormLayoutEnum.TablePerCategory:
                    return new TablePerCategoryLayoutCssClass(FormDefaultStyle);

                default:
                    throw new InvalidOperationException(DefaultFormLayout + " is unsupported form layout type.");
            }
        }


        /// <summary>
        /// Returns field's default value with resolved macros if macro editing is not allowed or the field requires macro resolving.
        /// </summary>
        /// <param name="field">Form field info object</param>
        private string GetDefaultValue(FormFieldInfo field)
        {
            string result = null;
            if (field != null)
            {
                bool resolveMacros = field.GetResolveDefaultValue(!AllowMacroEditing);

                result = field.GetPropertyValue(FormFieldPropertyEnum.DefaultValue, resolveMacros ? ContextResolver : null);
            }

            return result;
        }


        /// <summary>
        /// Registers a form control which has some dependencies so they can be informed on the main control change.
        /// </summary>
        /// <param name="control">Control with dependencies</param>
        internal void RegisterControlWithDependencies(FormEngineUserControl control)
        {
            if (control != null)
            {
                control.Changed += ControlWithDependencies_Changed;
            }
        }


        private void ControlWithDependencies_Changed(object sender, EventArgs e)
        {
            ControlWithDependenciesChanged?.Invoke(sender, e);
        }

        #endregion


        #region "FormTypeEnum methods"

        /// <summary>
        /// Returns the enumeration representation of the form type.
        /// </summary>
        /// <param name="str">Form type codename</param>
        public static FormTypeEnum StringToFormTypeEnum(string str)
        {
            switch (str.ToLowerInvariant())
            {
                case "basicform":
                    return FormTypeEnum.BasicForm;

                case "bizform":
                    return FormTypeEnum.BizForm;

                case "cmsform":
                    return FormTypeEnum.CMSForm;

                case "customtableform":
                    return FormTypeEnum.CustomTableForm;

                case "dataform":
                    return FormTypeEnum.DataForm;

                case "uiform":
                    return FormTypeEnum.UIForm;

                default:
                    throw new Exception("[FormTypeEnumFunctions.StringToFormTypeEnum]:  This string value is not supported");
            }
        }


        /// <summary>
        /// Returns the string representation of the form type.
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static string FormTypeEnumToString(FormTypeEnum value)
        {
            switch (value)
            {
                case FormTypeEnum.BasicForm:
                    return "BASICFORM";

                case FormTypeEnum.BizForm:
                    return "BIZFORM";

                case FormTypeEnum.CMSForm:
                    return "CMSFORM";

                case FormTypeEnum.CustomTableForm:
                    return "CUSTOMTABLEFORM";

                case FormTypeEnum.UIForm:
                    return "UIFORM";

                case FormTypeEnum.DataForm:
                    return "DATAFORM";

                default:
                    throw new Exception("[FormTypeEnumFunctions.FormTypeEnumToString]: This enumeration value is not supported");
            }
        }

        #endregion


        #region "Info Messages methods"

        /// <summary>
        /// Creates local messages placeholder.
        /// </summary>
        protected virtual MessagesPlaceHolder CreateMessagesPlaceHolder()
        {
            return new MessagesPlaceHolder
            {
                ID = "pM",
                IsLiveSite = IsLiveSite
            };
        }


        /// <summary>
        /// Shows the general changes saved message.
        /// </summary>
        public virtual void ShowChangesSaved()
        {
            ShowConfirmation(null);
        }


        #region "Confirmation"

        /// <summary>
        /// Shows the general changes saved message.
        /// </summary>
        /// <param name="text">Custom text</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public virtual void ShowConfirmation(string text, bool persistent = false)
        {
            if (InfoLabel != null)
            {
                // Messages placeholder info label is used
                if ((MessagesPlaceHolder != null) && (MessagesPlaceHolder.InfoLabel == InfoLabel))
                {
                    MessagesPlaceHolder.ShowConfirmation(text, persistent);
                }
                else
                {
                    if (String.IsNullOrEmpty(text))
                    {
                        text = ResHelper.GetString("general.changessaved");
                    }

                    InfoLabel.Text = text;
                    InfoLabel.Visible = true;
                }
            }
            else
            {
                throw new Exception("[BasicForm.ShowConfirmation]: Missing messages placeholder.");
            }
        }


        /// <summary>
        /// Adds confirmation text to existing message on the page, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Information message</param>
        /// <param name="separator">Separator</param>
        public virtual void AddConfirmation(string text, string separator = null)
        {
            if (InfoLabel != null)
            {
                // Messages placeholder info label is used
                if ((MessagesPlaceHolder != null) && (MessagesPlaceHolder.InfoLabel == InfoLabel))
                {
                    MessagesPlaceHolder.AddConfirmation(text, separator);
                }
                else
                {
                    InfoLabel.Text = String.Join(separator, InfoLabel.Text, text);
                }
            }
            else
            {
                throw new Exception("[BasicForm.AddConfirmation]: Missing messages placeholder.");
            }
        }

        #endregion


        #region "Information"

        /// <summary>
        /// Shows the given information on the page, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Information message</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public virtual void ShowInformation(string text, string description = null, string tooltipText = null, bool persistent = true)
        {
            if (InfoLabel != null)
            {
                // Messages placeholder info label is used
                if ((MessagesPlaceHolder != null) && (MessagesPlaceHolder.InfoLabel == InfoLabel))
                {
                    MessagesPlaceHolder.ShowInformation(text, description, tooltipText, persistent);
                }
                else
                {
                    InfoLabel.Text = text;
                    InfoLabel.ToolTip = tooltipText;
                    InfoLabel.Visible = true;
                }
            }
            else
            {
                throw new Exception("[BasicForm.ShowInformation]: Missing messages placeholder.");
            }
        }


        /// <summary>
        /// Adds information text to existing message on the page.
        /// </summary>
        /// <param name="text">Information message</param>
        /// <param name="separator">Separator</param>
        public virtual void AddInformation(string text, string separator = null)
        {
            if (InfoLabel != null)
            {
                // Messages placeholder info label is used
                if ((MessagesPlaceHolder != null) && (MessagesPlaceHolder.InfoLabel == InfoLabel))
                {
                    MessagesPlaceHolder.AddInformation(text, separator);
                }
                else
                {
                    InfoLabel.Text = String.Join(separator, InfoLabel.Text, text);
                }
            }
            else
            {
                throw new Exception("[BasicForm.AddInformation]: Missing messages placeholder.");
            }
        }

        #endregion


        #region "Error"

        /// <summary>
        /// Displays error label.
        /// </summary>
        /// <param name="name">Name of the field</param>
        /// <param name="validationResult">Error text</param>
        /// <returns>TRUE if the error label should be displayed</returns>
        public bool DisplayErrorLabel(string name, string validationResult)
        {
            var errorLabel = FieldErrorLabels[name];
            var editingControl = FieldEditingControls[name];
            var formControl = FieldControls[name];

            // Editing form controls are used only in automatic and custom HTML layouts
            var inputContainer = (editingControl?.NestedControlPanel ?? formControl?.Parent) as WebControl;

            if (!String.IsNullOrEmpty(validationResult))
            {
                if (!IsLiveSite && (MessagesPlaceHolder != null))
                {
                    // Try to get form field info
                    var ffi = FormInformation?.GetFormField(name);

                    string anchorScript = null;
                    var infoLabel = FieldLabels[name];
                    if (infoLabel != null)
                    {
                        anchorScript = "onclick=\"" + MessagesPlaceHolder.GetAnchorScript(infoLabel.ClientID, name) + ";\"";
                    }

                    // Get caption without special characters (colon, required mark etc.)
                    string caption = ResHelper.LocalizeString(ffi?.GetPropertyValue(FormFieldPropertyEnum.FieldCaption, ContextResolver))
                            ?.TrimEnd(ResHelper.Colon.ToCharArray())
                            ?.TrimEnd(ResHelper.RequiredMark.ToCharArray());

                    if (String.IsNullOrEmpty(caption))
                    {
                        caption = name;
                    }

                    // Ensure colon
                    if (!caption.EndsWith(ResHelper.Colon, StringComparison.CurrentCulture))
                    {
                        caption += ResHelper.Colon;
                    }

                    string message = !String.IsNullOrEmpty(caption) ? String.Join(" ", $"<span class=\"Anchor\" {anchorScript} >{caption}</span>", validationResult) : validationResult;
                    MessagesPlaceHolder.AddError(message);
                }
                else
                {
                    // Display the validation message
                    if (errorLabel != null)
                    {
                        errorLabel.Text = validationResult;
                        errorLabel.Visible = true;
                    }
                }

                // Add CSS class that indicates validation failure
                inputContainer?.AddCssClass(FieldErrorCssClass);

                // If some validation error found, the result is not valid
                return true;
            }

            // Validation succeeded
            if (errorLabel != null)
            {
                errorLabel.Visible = false;
            }

            // Remove CSS class that indicates validation failure
            inputContainer?.RemoveCssClass(FieldErrorCssClass);

            return false;
        }


        /// <summary>
        /// Shows the specified error message, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Error message text</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        public virtual void ShowError(string text, string description = null, string tooltipText = null)
        {
            if (ErrorLabel != null)
            {
                // Messages placeholder info label is used
                if ((MessagesPlaceHolder != null) && (MessagesPlaceHolder.ErrorLabel == ErrorLabel))
                {
                    MessagesPlaceHolder.ShowError(text, description, tooltipText);
                }
                else
                {
                    ErrorLabel.Text = text;
                    ErrorLabel.ToolTip = tooltipText;
                    ErrorLabel.Visible = true;
                }
            }
            else
            {
                throw new Exception("[BasicForm.ShowError]: Missing messages placeholder.");
            }
        }


        /// <summary>
        /// Prepends error text to existing message on the page.
        /// </summary>
        /// <param name="text">Error message text</param>
        /// <param name="separator">Separator</param>
        public virtual void PrependError(string text, string separator = null)
        {
            if (ErrorLabel != null)
            {
                if ((MessagesPlaceHolder != null) && (MessagesPlaceHolder.ErrorLabel == ErrorLabel))
                {
                    MessagesPlaceHolder.PrependError(text, separator);
                }
                else
                {
                    ErrorLabel.Text = text;
                }
            }
            else
            {
                throw new Exception("[BasicForm.PrependError]: Missing messages placeholder.");
            }
        }


        /// <summary>
        /// Adds error text to existing message on the page.
        /// </summary>
        /// <param name="text">Information message</param>
        /// <param name="separator">Separator</param>
        public virtual void AddError(string text, string separator = null)
        {
            if (ErrorLabel != null)
            {
                // Messages placeholder info label is used
                if ((MessagesPlaceHolder != null) && (MessagesPlaceHolder.ErrorLabel == ErrorLabel))
                {
                    MessagesPlaceHolder.AddError(text, separator);
                }
                else
                {
                    ErrorLabel.Text = String.Join(separator, ErrorLabel.Text, text);
                }
            }
            else
            {
                throw new Exception("[BasicForm.AddError]: Missing messages placeholder.");
            }
        }

        #endregion


        #region "Warning"

        /// <summary>
        /// Shows the specified warning message, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Warning message text</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        public virtual void ShowWarning(string text, string description = null, string tooltipText = null)
        {
            if (InfoLabel != null)
            {
                // Messages placeholder info label is used
                if ((MessagesPlaceHolder != null) && (MessagesPlaceHolder.InfoLabel == InfoLabel))
                {
                    MessagesPlaceHolder.ShowWarning(text, description, tooltipText);
                }
                else
                {
                    InfoLabel.Text = text;
                    InfoLabel.ToolTip = tooltipText;
                    InfoLabel.Visible = true;
                }
            }
            else
            {
                throw new Exception("[BasicForm.ShowWarning]: Missing messages placeholder.");
            }
        }


        /// <summary>
        /// Adds warning text to existing message on the page.
        /// </summary>
        /// <param name="text">Information message</param>
        /// <param name="separator">Separator</param>
        public virtual void AddWarning(string text, string separator = null)
        {
            if (InfoLabel != null)
            {
                // Messages placeholder info label is used
                if ((MessagesPlaceHolder != null) && (MessagesPlaceHolder.InfoLabel == InfoLabel))
                {
                    MessagesPlaceHolder.AddWarning(text, separator);
                }
                else
                {
                    InfoLabel.Text = String.Join(separator, InfoLabel.Text, text);
                }
            }
            else
            {
                throw new Exception("[BasicForm.AddWarning]: Missing messages placeholder.");
            }
        }

        #endregion

        #endregion
    }
}