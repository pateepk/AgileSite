using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;

using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Form field control for the forms.
    /// </summary>
    [ToolboxData("<{0}:FormField runat=server></{0}:FormField>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class FormField : PlaceHolder
    {
        #region "Variables"

        /// <summary>
        /// Field layout
        /// </summary>
        private FieldLayoutEnum mLayout = FieldLayoutEnum.Default;


        /// <summary>
        /// Form control name
        /// </summary>
        private string mFormControl;


        /// <summary>
        /// Resource string for the label
        /// </summary>
        private string mResourceString;


        /// <summary>
        /// Text for the label
        /// </summary>
        private string mText;


        /// <summary>
        /// Resource string for the label tooltip
        /// </summary>
        private string mToolTipResourceString;


        /// <summary>
        /// Indicates if label should be displayed in inline layout
        /// </summary>
        private bool mDisplayLabel = true;


        /// <summary>
        /// Indicates if control should load properties from FormFieldInfo
        /// </summary>
        private bool mUseFFI = true;


        /// <summary>
        /// Indicates if required mark (*) should be displayed behind the field label. False by default
        /// </summary>
        private bool? mShowRequiredMark;


        /// <summary>
        /// Indicates if colon (:) should be displayed behind the field label.
        /// </summary>
        private bool? mDisplayColon;

        #endregion


        #region "Properties"

        /// <summary>
        /// Field name for the inner controls. This property is only supported in templated form.
        /// </summary>
        public string Field
        {
            get;
            set;
        }


        /// <summary>
        /// Layout of the field.
        /// </summary>
        public FieldLayoutEnum Layout
        {
            get
            {
                if (mLayout == FieldLayoutEnum.Default)
                {
                    mLayout = Form.DefaultFieldLayout;
                }

                return mLayout;
            }
            set
            {
                mLayout = value;
            }
        }


        /// <summary>
        /// Layout of the form.
        /// </summary>
        private FormLayoutEnum FormLayout
        {
            get
            {
                return Form.DefaultFormLayout;
            }
        }


        /// <summary>
        /// Text for the label, has higher priority.
        /// </summary>
        public string Text
        {
            get
            {
                if (string.IsNullOrEmpty(mText) && (FormFieldInfo != null) && UseFFI)
                {
                    mText = FormFieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldCaption, Form.ContextResolver);
                }

                return mText;
            }
            set
            {
                mText = value;
                if (Label != null)
                {
                    Label.Text = value;
                }
            }
        }


        /// <summary>
        /// Resource string for the label.
        /// </summary>
        public string ResourceString
        {
            get
            {
                if (string.IsNullOrEmpty(mResourceString) && (FormFieldInfo != null) && UseFFI)
                {
                    string caption = FormFieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldCaption, Form.ContextResolver);
                    if (MacroProcessor.IsLocalizationMacro(caption))
                    {
                        mResourceString = MacroProcessor.RemoveLocalizationMacroBrackets(caption);
                    }
                }

                return mResourceString;
            }
            set
            {
                mResourceString = value;
                if (Label != null)
                {
                    Label.ResourceString = value;
                }
            }
        }


        /// <summary>
        /// Indicates if colon (:) should be displayed behind the field label.
        /// If Form is set then Form.UseColonBehindLabel is used as default value, otherwise false.
        /// </summary>
        public bool DisplayColon
        {
            get
            {
                if (!mDisplayColon.HasValue)
                {
                    mDisplayColon = (Form != null) && Form.UseColonBehindLabel;
                }

                return mDisplayColon.Value;
            }
            set
            {
                mDisplayColon = value;
                if (Label != null)
                {
                    Label.DisplayColon = value;
                }
            }
        }


        /// <summary>
        /// Indicates if required mark (*) should be displayed behind the field label. False by default.
        /// </summary>
        public bool ShowRequiredMark
        {
            get
            {
                if (!mShowRequiredMark.HasValue)
                {
                    if ((Form != null) && Form.MarkRequiredFields && UseFFI && (!string.IsNullOrEmpty(Field) || (FormFieldInfo != null)))
                    {
                        if ((FormFieldInfo == null) && (Form.FormInformation != null))
                        {
                            // Ensure form field info
                            FormFieldInfo = Form.FormInformation.GetFormField(Field);
                        }

                        mShowRequiredMark = (FormFieldInfo != null) && !FormFieldInfo.AllowEmpty && !FormHelper.IsFieldOfType(FormFieldInfo, FormFieldControlTypeEnum.CheckBoxControl);
                    }
                    else
                    {
                        mShowRequiredMark = false;
                    }
                }

                return mShowRequiredMark.Value;
            }
            set
            {
                mShowRequiredMark = value;
                if (Label != null)
                {
                    Label.ShowRequiredMark = value;
                }
            }
        }


        /// <summary>
        /// Form control.
        /// </summary>
        public string FormControl
        {
            get
            {
                if (string.IsNullOrEmpty(mFormControl) && (FormFieldInfo != null) && UseFFI)
                {
                    mFormControl = ValidationHelper.GetString(FormFieldInfo.Settings["controlname"], null);
                }

                return mFormControl;
            }
            set
            {
                mFormControl = value;
            }
        }


        /// <summary>
        /// Editing control.
        /// </summary>
        public FormEngineUserControl EditingControl
        {
            get;
            protected set;
        }


        /// <summary>
        /// Error label.
        /// </summary>
        public FormErrorLabel ErrorLabel
        {
            get;
            protected set;
        }


        /// <summary>
        /// Field label.
        /// </summary>
        public FormLabel Label
        {
            get;
            protected set;
        }


        /// <summary>
        /// Field's CSS Class.
        /// </summary>
        public string CssClass
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the field is checked for uniqueness. This property is only supported in UIForm.
        /// </summary>
        public bool CheckUnique
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the value is trimmed. This property is only supported if used inside the form control.
        /// </summary>
        public bool Trim
        {
            get;
            set;
        }


        /// <summary>
        /// Value used for initialization as default value.
        /// </summary>
        public string Value
        {
            get;
            set;
        }


        /// <summary>
        /// Name of a resource string used for tooltip.
        /// </summary>
        public string ToolTipResourceString
        {
            get
            {
                if (string.IsNullOrEmpty(mToolTipResourceString) && (FormFieldInfo != null) && UseFFI)
                {
                    string fieldDescription = FormFieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldDescription, Form.ContextResolver);
                    if (MacroProcessor.IsLocalizationMacro(fieldDescription))
                    {
                        mToolTipResourceString = MacroProcessor.RemoveLocalizationMacroBrackets(fieldDescription);
                    }
                }

                return mToolTipResourceString;
            }
            set
            {
                mToolTipResourceString = value;
                if (Label != null)
                {
                    Label.ToolTipResourceString = value;
                }
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
            get
            {
                return mUseFFI;
            }
            set
            {
                mUseFFI = value;
            }
        }


        /// <summary>
        /// Indicates if label should be displayed in inline layout. Default value is True.
        /// </summary>
        public bool DisplayLabel
        {
            get
            {
                return mDisplayLabel;
            }
            set
            {
                mDisplayLabel = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Ensures the child controls of the control.
        /// </summary>
        public void EnsureControls()
        {
            EnsureChildControls();
        }


        /// <summary>
        /// Creates the child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Ensure the inner controls
            EnsureInnerControls();
        }


        /// <summary>
        /// Ensures the additional inner controls.
        /// </summary>
        protected virtual void EnsureInnerControls()
        {
            // Add additional layout controls
            switch (Layout)
            {
                case FieldLayoutEnum.TwoColumns:
                case FieldLayoutEnum.ThreeColumns:
                    string openingSequence;
                    string middleSequence;
                    string closingSequence;

                    switch (FormLayout)
                    {
                        case FormLayoutEnum.Divs:
                            openingSequence = "<div{0}><div{1}>";
                            middleSequence = "</div><div{0}>";
                            closingSequence = "</div></div>";
                            break;

                        default:
                            openingSequence = "<tr{0}><td{1}>";
                            middleSequence = "</td><td{0}>";
                            closingSequence = "</td></tr>";
                            break;
                    }

                    openingSequence = String.Format(openingSequence, CssHelper.GetCssClassAttribute(GetCssClass(FormFieldPropertyEnum.FieldCssClass)), CssHelper.GetCssClassAttribute(GetCssClass(FormFieldPropertyEnum.CaptionCellCssClass)));
                    middleSequence = String.Format(middleSequence, CssHelper.GetCssClassAttribute(GetCssClass(FormFieldPropertyEnum.ControlCellCssClass)));

                    // First column with the label
                    PlaceHolder plcFirst = new PlaceHolder();
                    plcFirst.Controls.Add(new LiteralControl(openingSequence));

                    // Load the label
                    LoadLabel(plcFirst);

                    plcFirst.Controls.Add(new LiteralControl(middleSequence));
                    Controls.AddAt(0, plcFirst);

                    // Load the form control
                    LoadFormControl(this);

                    // Third column with the error
                    PlaceHolder plcThird = new PlaceHolder();
                    if (Layout == FieldLayoutEnum.ThreeColumns)
                    {
                        plcThird.Controls.Add(new LiteralControl(middleSequence));
                    }

                    // Load the error label
                    LoadErrorLabel(plcThird);

                    plcThird.Controls.Add(new LiteralControl(closingSequence));
                    Controls.Add(plcThird);
                    break;

                case FieldLayoutEnum.Inline:
                    // Load form control in case of no nested controls
                    if ((Controls.Count == 0) && (FormControl != null) && (Field != null))
                    {
                        // Load the label
                        LoadLabel(this);

                        // Load form control
                        LoadFormControl(this);

                        // Load error label
                        LoadErrorLabel(this);
                    }
                    break;
            }
        }


        /// <summary>
        /// Returns CSS class belonging to specified form field element.
        /// </summary>
        /// <param name="cssProperty">Property containing CSS class.</param>
        private string GetCssClass(FormFieldPropertyEnum cssProperty)
        {
            return Form.GetFieldCssClass(UseFFI ? FormFieldInfo : null, cssProperty);
        }


        /// <summary>
        /// Loads the field label.
        /// </summary>
        /// <param name="parent">Parent control for the label</param>
        protected virtual void LoadLabel(Control parent)
        {
            if (DisplayLabel && (Field != null) && (!string.IsNullOrEmpty(Text) || !string.IsNullOrEmpty(ResourceString)))
            {
                // Create the field label
                Label = new FormLabel
                {
                    ID = "l" + Field,
                    ToolTipResourceString = ToolTipResourceString,
                    ResourceString = ResourceString,
                    DisplayColon = DisplayColon,
                    ShowRequiredMark = ShowRequiredMark,
                    Text = Text,
                    Form = Form,
                    FormFieldInfo = FormFieldInfo
                };

                parent.Controls.Add(Label);
            }
        }


        /// <summary>
        /// Loads the form control.
        /// </summary>
        /// <param name="parent">Parent control</param>
        protected virtual void LoadFormControl(Control parent)
        {
            // Original content
            if ((FormControl != null) && (Field != null))
            {
                // Load the form control
                var ctrl = new FormControl
                {
                    Form = Form,
                    FormControlName = FormControl,
                    Field = Field,
                    UseFFI = UseFFI,
                    FormFieldInfo = FormFieldInfo,
                    CheckUnique = CheckUnique,
                    Trim = Trim
                };

                ctrl.CssClass = CssClass ?? ctrl.CssClass;
                if (!String.IsNullOrEmpty(Value))
                {
                    // Override default value with external value
                    ctrl.Value = Value;
                }

                parent.Controls.Add(ctrl);

                EditingControl = ctrl.EditingControl;
            }
        }


        /// <summary>
        /// Loads the field error label.
        /// </summary>
        /// <param name="parent">Parent control for the label</param>
        protected virtual void LoadErrorLabel(Control parent)
        {
            if (Field != null)
            {
                // Create the error label
                ErrorLabel = new FormErrorLabel{ ID = "e" + Field };

                if (Form.FieldErrorLabelCssClass != null)
                {
                    ErrorLabel.CssClass = Form.FieldErrorLabelCssClass;
                }

                parent.Controls.Add(ErrorLabel);
            }
        }

        #endregion
    }
}