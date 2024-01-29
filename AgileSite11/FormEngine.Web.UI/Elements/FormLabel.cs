using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.MacroEngine;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Form label control for a specific form field.
    /// </summary>
    [ToolboxData("<{0}:FormLabel runat=server/>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class FormLabel : LocalizedLabel
    {
        #region "Variables"

        private bool mUseFFI = true;
        private bool mCssClassSet;
        private bool mRequiredMarkSet;
        private bool mDisplayColonSet;

        private FormFieldInfo mFormFieldInfo;

        #endregion


        #region "Properties"

        /// <summary>
        /// Field name for which the label applies. This property is only supported in templated form.
        /// </summary>
        public string Field
        {
            get;
            set;
        }


        /// <summary>
        /// Label's CSS class.
        /// </summary>
        public override string CssClass
        {
            get
            {
                return base.CssClass;
            }
            set
            {
                base.CssClass = value;

                mCssClassSet = true;
            }
        }


        /// <summary>
        /// Indicates if required mark (* by default) should be displayed at the end of the text. False by default.
        /// </summary>
        public override bool ShowRequiredMark
        {
            get
            {
                return base.ShowRequiredMark;
            }
            set
            {
                base.ShowRequiredMark = value;

                mRequiredMarkSet = true;
            }
        }


        /// <summary>
        /// Display colon at the end of the text.
        /// </summary>
        public override bool DisplayColon
        {
            get
            {
                return base.DisplayColon;
            }
            set
            {
                base.DisplayColon = value;
                mDisplayColonSet = true;
            }
        }


        /// <summary>
        /// Form where the control is used.
        /// </summary>
        internal BasicForm Form
        {
            get;
            set;
        }


        /// <summary>
        /// Field definition if the label is connected with a field.
        /// </summary>
        internal FormFieldInfo FormFieldInfo
        {
            get
            {
                if ((mFormFieldInfo == null) && !String.IsNullOrEmpty(Field) && (Form != null) && (Form.FormInformation != null))
                {
                    mFormFieldInfo = Form.FormInformation.GetFormField(Field);
                }

                return mFormFieldInfo;
            }
            set
            {
                mFormFieldInfo = value;
            }
        }


        /// <summary>
        /// Indicates if control should load properties from FormFieldInfo. True by default.
        /// </summary>
        protected bool UseFFI
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

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public FormLabel()
        {
            base.CssClass = "EditingFormLabel";
            EnableViewState = false;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Override for CreateChildControls method.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            SetupControl();
        }


        /// <summary>
        /// Configures label control with parent settings if allowed.
        /// </summary>
        private void SetupControl()
        {
            if (UseFFI && (Form != null))
            {
                // Use the form's settings if not set explicitly
                if (!mCssClassSet)
                {
                    CssClass = Form.FieldCaptionCssClass;
                }

                // If DisplayColon wasn't set explicitly use the form's setting
                if (!mDisplayColonSet)
                {
                    DisplayColon = Form.UseColonBehindLabel;
                }

                var fieldInfo = FormFieldInfo;
                if (fieldInfo != null)
                {
                    // Use field's settings if not set explicitly
                    if (!mRequiredMarkSet && Form.MarkRequiredFields)
                    {
                        ShowRequiredMark = !fieldInfo.AllowEmpty && !FormHelper.IsFieldOfType(fieldInfo, FormFieldControlTypeEnum.CheckBoxControl);
                    }
                    
                    if (String.IsNullOrEmpty(Text) && String.IsNullOrEmpty(ResourceString))
                    {
                        string caption = fieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldCaption, Form.ContextResolver);
                        if (MacroProcessor.IsLocalizationMacro(caption))
                        {
                            ResourceString = MacroProcessor.RemoveLocalizationMacroBrackets(caption);
                        }
                        else
                        {
                            Text = caption;
                        }
                    }

                    if (String.IsNullOrEmpty(ToolTip) && String.IsNullOrEmpty(ToolTipResourceString))
                    {
                        string description = fieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldDescription, Form.ContextResolver);
                        if (MacroProcessor.IsLocalizationMacro(description))
                        {
                            ToolTipResourceString = MacroProcessor.RemoveLocalizationMacroBrackets(description);
                        }
                        else
                        {
                            ToolTip = description;
                        }
                    }

                    // Add caption CSS style
                    string captionStyle = fieldInfo.GetPropertyValue(FormFieldPropertyEnum.CaptionStyle, Form.ContextResolver);
                    if (!String.IsNullOrEmpty(captionStyle))
                    {
                        this.Attributes.Add("style", captionStyle);
                    }
                }
            }
        }

        #endregion
    }
}