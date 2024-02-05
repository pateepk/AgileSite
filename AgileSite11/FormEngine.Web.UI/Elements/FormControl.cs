using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Form control for the forms.
    /// </summary>
    [ToolboxData("<{0}:FormControl runat=server></{0}:FormControl>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    [ParseChildren(true)]
    public class FormControl : CMSDynamicWebControl, INamingContainer
    {
        #region "Variables"

        private FormEngineUserControl mEditingControl;
        private string mFormControlName;
        private MacroResolver mContextResolver;
        private string mMarkupCssClass;

        #endregion


        #region "Properties"

        /// <summary>
        /// Properties definition.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [MergableProperty(false)]
        public Collection<Property> Properties
        {
            get;
            set;
        }


        /// <summary>
        /// Editing control.
        /// </summary>
        public FormEngineUserControl EditingControl
        {
            get
            {
                EnsureInitialization();

                return mEditingControl;
            }
            protected set
            {
                mEditingControl = value;
            }
        }


        /// <summary>
        /// Field name for the inner controls. This property is only supported in templated form.
        /// </summary>
        public string Field
        {
            // Need to be separate because this property is used for control load
            get;
            set;
        }


        /// <summary>
        /// Form control name.
        /// </summary>
        public string FormControlName
        {
            get
            {
                if (string.IsNullOrEmpty(mFormControlName) && (FormFieldInfo != null) && UseFFI)
                {
                    mFormControlName = ValidationHelper.GetString(FormFieldInfo.Settings["controlname"], null);
                }

                return mFormControlName;
            }
            set
            {
                mFormControlName = value;
            }
        }


        /// <summary>
        /// Indicates if control is used on live site.
        /// </summary>
        public new bool IsLiveSite
        {
            get
            {
                return EditingControl.IsLiveSite;
            }
            set
            {
                EditingControl.IsLiveSite = value;
            }
        }


        /// <summary>
        /// Short ID of the control.
        /// </summary>
        public new string ShortID
        {
            get
            {
                return EditingControl.ShortID;
            }
            set
            {
                EditingControl.ShortID = value;
            }
        }


        /// <summary>
        /// Control CSS class.
        /// </summary>
        public new virtual string CssClass
        {
            get
            {
                return EditingControl.CssClass;
            }
            set
            {
                EditingControl.CssClass = mMarkupCssClass = value;
            }
        }


        /// <summary>
        /// If true, the field is checked for uniqueness. This property is only supported in UIForm.
        /// </summary>
        public bool CheckUnique
        {
            get
            {
                return EditingControl.CheckUnique;
            }
            set
            {
                EditingControl.CheckUnique = value;
            }
        }


        /// <summary>
        /// If true, the value is trimmed. This property is only supported if used inside the form control.
        /// </summary>
        public bool Trim
        {
            get
            {
                return EditingControl.Trim;
            }
            set
            {
                EditingControl.Trim = value;
            }
        }


        /// <summary>
        /// Value used for initialization as default value.
        /// </summary>
        public object Value
        {
            get
            {
                return EditingControl.Value;
            }
            set
            {
                EditingControl.Value = value;
            }
        }


        /// <summary>
        /// FormFieldInfo for current control.
        /// </summary>
        public FormFieldInfo FormFieldInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Form where the control is used.
        /// </summary>
        public BasicForm Form
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if control should load properties from FormFieldInfo. Default TRUE.
        /// </summary>
        public bool UseFFI
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Gets or sets whether a postback automatically occurs.
        /// </summary>
        public bool AutoPostBack
        {
            get
            {
                return EditingControl.HasDependingFields;
            }
            set
            {
                EditingControl.HasDependingFields = value;
            }
        }


        /// <summary>
        /// Gets or sets state enable.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return EditingControl.Enabled;
            }
            set
            {
                EditingControl.Enabled = value;
            }
        }


        /// <summary>
        /// Gets a value that indicates whether the control should set the disabled attribute of the rendered HTML element to "disabled" when the control's <see cref="P:System.Web.UI.WebControls.WebControl.IsEnabled" /> property is false.
        /// </summary>
        public override bool SupportsDisabledAttribute
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Gets or sets value converted to string
        /// </summary>
        public string Text
        {
            get
            {
                return EditingControl.Text;
            }
            set
            {
                EditingControl.Text = value;
            }
        }


		/// <summary>
		/// TagKey used to render (Div).
		/// </summary>
		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				return HtmlTextWriterTag.Div;
			}
		}

        #endregion


        #region "Private properties"

        private LiteralControl ContentBeforeLiteral
        {
            get;
            set;
        }


        private LiteralControl ContentAfterLiteral
        {
            get;
            set;
        }


        private LiteralControl ExplanationTextLiteral
        {
            get;
            set;
        }


        /// <summary>
        /// Macro resolver.
        /// </summary>
        private MacroResolver ContextResolver
        {
            get
            {
                if (mContextResolver == null)
                {
                    if (Form != null)
                    {
                        // Use resolver from form engine
                        mContextResolver = Form.ContextResolver;
                    }
                    else if (EditingControl != null)
                    {
                        // Use resolver from editing control because of using control outside form engine
                        mContextResolver = EditingControl.ContextResolver;
                    }
                    else
                    {
                        // Create child instance of current resolver
                        mContextResolver = MacroContext.CurrentResolver.CreateChild();
                    }

                }
                return mContextResolver;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Reloads inner controls. Can be used after additional initialization.
        /// </summary>
        public void Reload()
        {
            InitializeControl();
        }


        /// <summary>
        /// Returns the value of the given web part property property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public virtual object GetValue(string propertyName)
        {
            return EditingControl.GetValue(propertyName);
        }


        /// <summary>
        /// Returns the value of the given web part property property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="defaultValue">Default value</param>
        public virtual ReturnType GetValue<ReturnType>(string propertyName, ReturnType defaultValue)
        {
            return EditingControl.GetValue(propertyName, defaultValue);
        }


        /// <summary>
        /// Sets the property value of the control, setting the value affects only local property value.
        /// </summary>
        /// <param name="propertyName">Property name to set</param>
        /// <param name="value">New property value</param>
        public virtual void SetValue(string propertyName, object value)
        {
            EditingControl.SetValue(propertyName, value);
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// OnPreRender override.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (UseFFI && (FormFieldInfo != null) && (ContentBeforeLiteral != null) && (ContentAfterLiteral != null))
            {
                // Set content before and after
                ContentBeforeLiteral.Text = ContextResolver.ResolveMacros(FormFieldInfo.GetPropertyValue(FormFieldPropertyEnum.ContentBefore));
                ContentAfterLiteral.Text = ContextResolver.ResolveMacros(FormFieldInfo.GetPropertyValue(FormFieldPropertyEnum.ContentAfter));

                // Handle explanation text
                string explanationText = ContextResolver.ResolveMacros(FormFieldInfo.GetPropertyValue(FormFieldPropertyEnum.ExplanationText));
                if (!String.IsNullOrEmpty(explanationText))
                {
                    ExplanationTextLiteral.Text = String.Format("<div class=\"{1}\">{0}</div>", explanationText, (Form != null) ? Form.ExplanationTextCssClass : "ExplanationText");
                }
            }
        }


        /// <summary>
        /// Loads the form control
        /// </summary>
        protected override void InitializeControl()
        {
            Controls.Clear();

            // Original content
            if (FormControlName != null)
            {
                // Load the form control
                var ctrl = FormUserControlLoader.LoadFormControl(Page, FormControlName, Field, Form, false);
                if (ctrl != null)
                {
                    InitCompleted = true;

                    EditingControl = ctrl;

                    // Apply properties if available
                    if (Properties != null)
                    {
                        foreach (var property in Properties)
                        {
                            ctrl.SetValue(property.Name, property.Value);
                        }
                    }

                    // Load default properties
                    ctrl.LoadDefaultProperties(null, false);

                    // Ensure correct ID
                    if (string.IsNullOrEmpty(Field))
                    {
                        ctrl.ID = "f" + ID;
                    }
                    ctrl.Field = Field;
                    ctrl.Form = Form;
                    ctrl.IsLiveSite = (Form != null) ? Form.IsLiveSite : IsLiveSite;
                    ctrl.ContextResolver = ContextResolver;

                    // Init the form control values
                    if (UseFFI)
                    {
                        ctrl.FieldInfo = FormFieldInfo;
                        ctrl.LoadControlFromFFI();

                        // Add content before
                        ContentBeforeLiteral = new LiteralControl();
                        Controls.Add(ContentBeforeLiteral);

                        if (FormFieldInfo != null)
                        {
                            if (Form != null)
                            {
                                // Set control's default value
                                object value = Form.GetDataValue(Field);
                                string defaultValue = null;
                                if ((value == null) && Form.IsInsertMode)
                                {
                                    // Get field default value and try to resolve macros using control resolver
                                    defaultValue = FormFieldInfo.GetPropertyValue(FormFieldPropertyEnum.DefaultValue, ContextResolver);
                                }

                                if ((defaultValue != null) && ((defaultValue != string.Empty) || !FormFieldInfo.AllowEmpty))
                                {
                                    // Allow empty default value only if the field does not allow nulls
                                    ctrl.Value = defaultValue;
                                }
                                else
                                {
                                    ctrl.Value = value;
                                }
                            }
                        }
                    }

                    // Set CSS class which was specified in ASCX template to the control
                    ApplyMarkupCssClass();

                    // Add control
                    Controls.Add(ctrl);

                    if (UseFFI)
                    {
                        // Add explanation text
                        ExplanationTextLiteral = new LiteralControl();
                        Controls.Add(ExplanationTextLiteral);

                        // Add content after
                        ContentAfterLiteral = new LiteralControl();
                        Controls.Add(ContentAfterLiteral);
                    }
                }
            }
        }


        private void ApplyMarkupCssClass()
        {
            if (!string.IsNullOrEmpty(mMarkupCssClass))
            {
                EditingControl.CssClass = mMarkupCssClass;
            }
        }


        /// <summary>
        /// Ensures the first found child control of the given type in the given variable
        /// </summary>
        /// <param name="variable">Control variable</param>
        protected virtual ControlType EnsureChildControl<ControlType>(ref ControlType variable)
            where ControlType : Control
        {
            if (variable == null)
            {
                variable = ControlsHelper.GetChildControl<ControlType>(EditingControl);
            }

            return variable;
        }

        #endregion
    }
}