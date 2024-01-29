using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

using AjaxControlToolkit;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Base class for the textbox form control
    /// </summary>
    public abstract class TextBoxControl : FormEngineUserControl
    {
        #region "Constants"

        private const int FILTER_NUMBERS = 0;
        private const int FILTER_LOWERCASE = 1;
        private const int FILTER_UPPERCASE = 2;
        private const int FILTER_CUSTOM = 3;

        #endregion


        #region "Variables"

        private FilterModes mFilterMode = FilterModes.ValidChars;

        #endregion


        #region "Properties"

        /// <summary>
        /// Textbox control
        /// </summary>
        protected abstract CMSTextBox TextBox
        {
            get;
        }


        /// <summary>
        /// Maximum text length
        /// </summary>
        public int MaxLength
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("size"), 0);
            }
            set
            {
                SetValue("size", value);
                TextBox.MaxLength = value;
            }
        }


        /// <summary>
        /// Gets or sets the enabled state of the control.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return TextBox.Enabled;
            }
            set
            {
                TextBox.Enabled = value;
            }
        }


        /// <summary>
        /// Gets or sets form control value.
        /// </summary>
        public override object Value
        {
            get
            {
                if (Trim)
                {
                    return TextBox.Text.Trim();
                }

                return TextBox.Text;
            }
            set
            {
                // Convert the value to a proper type
                value = ConvertInputValue(value);

                TextBox.Text = ValidationHelper.GetString(value, null);
            }
        }


        /// <summary>
        /// Gets (or sets) the value indicating if form control is displayed as TextArea control.
        /// If FALSE then form control is displayed as TextBox control. Setting this value
        /// is performed only if FieldInfo is null.
        /// </summary>
        public bool IsTextArea
        {
            get
            {
                if (FormHelper.IsFieldOfType(FieldInfo, FormFieldControlTypeEnum.TextAreaControl))
                {
                    return true;
                }

                return ValidationHelper.GetBoolean(GetValue("IsTextArea"), false);
            }
            set
            {
                SetValue("IsTextArea", value);
            }
        }


        /// <summary>
        /// Gets or sets the unique name of the client function to be rendered, which can be used to get the value of the text box.
        /// </summary>
        public string ValueAccessFunctionName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ValueAccessFunctionName"), null);
            }
            set
            {
                SetValue("ValueAccessFunctionName", value);
            }
        }

        #endregion


        #region "Watermark properties"

        /// <summary>
        /// The text to show when the control has no value.
        /// </summary>
        public string WatermarkText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("WatermarkText"), null);
            }
            set
            {
                SetValue("WatermarkText", value);
            }
        }


        /// <summary>
        /// The CSS class to apply to the TextBox when it has no value (e.g. the watermark text is shown).
        /// </summary>
        public string WatermarkCssClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("WatermarkCssClass"), "WatermarkText");
            }
            set
            {
                SetValue("WatermarkCssClass", value);
            }
        }

        #endregion


        #region "Filter properties"

        /// <summary>
        /// Indicates if filter is enabled.
        /// </summary>
        public bool FilterEnabled
        {
            get
            {
                // For custom filter type the valid characters or invalid characters have to set
                if ((FilterTypeValue == FILTER_CUSTOM.ToString()))
                {
                    if (FilterMode == FilterModes.ValidChars)
                    {
                        return !String.IsNullOrEmpty(ValidChars);
                    }

                    return !String.IsNullOrEmpty(InvalidChars);
                }

                // Get the filter type from field settings and from control settings
                return (!String.IsNullOrEmpty(FilterTypeValue) || (FilterType != 0));
            }
        }


        /// <summary>
        /// A string consisting of all characters considered valid for the text field, if "Custom" is specified as the filter type. Otherwise this parameter is ignored.
        /// </summary>
        public string ValidChars
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ValidChars"), null);
            }
            set
            {
                SetValue("ValidChars", value);
            }
        }


        /// <summary>
        /// A string consisting of all characters considered invalid for the text field, if "Custom" is specified as the filter type and "InvalidChars" as the filter mode. Otherwise this parameter is ignored.
        /// </summary>
        public string InvalidChars
        {
            get
            {
                return ValidationHelper.GetString(GetValue("InvalidChars"), null);
            }
            set
            {
                SetValue("InvalidChars", value);
            }
        }


        /// <summary>
        /// An integer containing the interval (in milliseconds) in which the field's contents are filtered, defaults to 250.
        /// </summary>
        public int FilterInterval
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("FilterInterval"), 250);
            }
            set
            {
                SetValue("FilterInterval", value);
            }
        }


        /// <summary>
        /// The type of filter to apply, as a comma-separated combination of Numbers, LowercaseLetters, UppercaseLetters, and Custom. If Custom is specified, the ValidChars field will be used in addition to other settings such as Numbers.
        /// </summary>
        public FilterTypes FilterType
        {
            get;
            set;
        }


        /// <summary>
        /// This property gets the form control settings of the FilterType.
        /// </summary>
        public string FilterTypeValue
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FilterType"), null);
            }
        }


        /// <summary>
        /// The filter mode to apply, either ValidChars (default) or InvalidChars. If set to InvalidChars, FilterType must be set to Custom; if set to ValidChars, FilterType must contain Custom.
        /// </summary>
        public FilterModes FilterMode
        {
            get
            {
                object filterModeObj = GetValue("FilterMode");
                if (filterModeObj == null)
                {
                    return mFilterMode;
                }

                return ValidationHelper.GetBoolean(filterModeObj, false) ? FilterModes.InvalidChars : FilterModes.ValidChars;
            }
            set
            {
                mFilterMode = value;
            }
        }

        #endregion


        #region "Auto-complete properties"

        /// <summary>
        /// The web service method to be called. The signature of this method must match the following: 
        /// [System.Web.Services.WebMethod]
        /// [System.Web.Script.Services.ScriptMethod]
        /// public string[] GetCompletionList(string prefixText, int count) { ... }
        /// Note that you can replace "GetCompletionList" with a name of your choice, but the return type and parameter name and type must exactly match, including case.
        /// </summary>
        public string AutoCompleteServiceMethod
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AutoCompleteServiceMethod"), null);
            }
            set
            {
                SetValue("AutoCompleteServiceMethod", value);
            }
        }


        /// <summary>
        /// The path to the web service that the extender will pull the word\sentence completions from. If this is not provided, the service method should be a page method.
        /// </summary>
        public string AutoCompleteServicePath
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AutoCompleteServicePath"), null);
            }
            set
            {
                SetValue("AutoCompleteServicePath", value);
            }
        }


        /// <summary>
        /// User/page specific context provided to an optional overload of the web method described by ServiceMethod/ServicePath. If the context key is used, it should have the same signature with an additional parameter named contextKey of type string: 
        /// [System.Web.Services.WebMethod]
        /// [System.Web.Script.Services.ScriptMethod]
        /// public string[] GetCompletionList(
        /// string prefixText, int count, string contextKey) { ... }
        /// Note that you can replace "GetCompletionList" with a name of your choice, but the return type and parameter name and type must exactly match, including case.
        /// </summary>
        public string AutoCompleteContextKey
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AutoCompleteContextKey"), null);
            }
            set
            {
                SetValue("AutoCompleteContextKey", value);
            }
        }


        /// <summary>
        /// Minimum number of characters that must be entered before getting suggestions from the web service.
        /// </summary>
        public int AutoCompleteMinimumPrefixLength
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("AutoCompleteMinimumPrefixLength"), 2);
            }
            set
            {
                SetValue("AutoCompleteMinimumPrefixLength", value);
            }
        }


        /// <summary>
        /// Time in milliseconds when the timer will kick in to get suggestions using the web service.
        /// </summary>
        public int AutoCompleteCompletionInterval
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("AutoCompleteCompletionInterval"), 2);
            }
            set
            {
                SetValue("AutoCompleteCompletionInterval", value);
            }
        }


        /// <summary>
        /// Whether client side caching is enabled.
        /// </summary>
        public bool AutoCompleteEnableCaching
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AutoCompleteEnableCaching"), false);
            }
            set
            {
                SetValue("AutoCompleteEnableCaching", value);
            }
        }


        /// <summary>
        /// Number of suggestions to be retrieved from the web service.
        /// </summary>
        public int AutoCompleteCompletionSetCount
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("AutoCompleteCompletionSetCount"), 2);
            }
            set
            {
                SetValue("AutoCompleteCompletionSetCount", value);
            }
        }


        /// <summary>
        /// CSS class that will be used to style the completion list flyout.
        /// </summary>
        public string AutoCompleteCompletionListCssClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AutoCompleteCompletionListCssClass"), null);
            }
            set
            {
                SetValue("AutoCompleteCompletionListCssClass", value);
            }
        }


        /// <summary>
        /// CSS class that will be used to style an item in the AutoComplete list flyout.
        /// </summary>
        public string AutoCompleteCompletionListItemCssClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AutoCompleteCompletionListItemCssClass"), null);
            }
            set
            {
                SetValue("AutoCompleteCompletionListItemCssClass", value);
            }
        }


        /// <summary>
        /// CSS class that will be used to style a highlighted item in the AutoComplete list flyout.
        /// </summary>
        public string AutoCompleteCompletionListHighlightedItemCssClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AutoCompleteCompletionListHighlightedItemCssClass"), null);
            }
            set
            {
                SetValue("AutoCompleteCompletionListHighlightedItemCssClass", value);
            }
        }


        /// <summary>
        /// Specifies one or more character(s) used to separate words. The text in the AutoComplete textbox is tokenized using these characters and the webservice completes the last token.
        /// </summary>
        public string AutoCompleteDelimiterCharacters
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AutoCompleteDelimiterCharacters"), null);
            }
            set
            {
                SetValue("AutoCompleteDelimiterCharacters", value);
            }
        }


        /// <summary>
        /// Determines if the first option in the AutoComplete list will be selected by default.
        /// </summary>
        public bool AutoCompleteFirstRowSelected
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AutoCompleteFirstRowSelected"), false);
            }
            set
            {
                SetValue("AutoCompleteFirstRowSelected", value);
            }
        }


        /// <summary>
        ///  If true and DelimiterCharacters are specified, then the AutoComplete list items display suggestions for the current word to be completed and do not display the rest of the tokens.
        /// </summary>
        public bool AutoCompleteShowOnlyCurrentWordInCompletionListItem
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AutoCompleteShowOnlyCurrentWordInCompletionListItem"), false);
            }
            set
            {
                SetValue("AutoCompleteShowOnlyCurrentWordInCompletionListItem", value);
            }
        }

        #endregion


        #region "Control events"

        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Initialize properties
            if (!string.IsNullOrEmpty(WatermarkText) || FilterEnabled || (!string.IsNullOrEmpty(AutoCompleteServiceMethod) && !string.IsNullOrEmpty(AutoCompleteServicePath)))
            {
                ControlsHelper.EnsureScriptManager(Page);
            }
        }


        /// <summary>
        /// Load event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SetClientSideMaxLength();

            // Setup TextArea control
            if (IsTextArea)
            {
                // Set cols
                int cols = ValidationHelper.GetInteger(GetValue("cols"), 0);
                if (cols > 0)
                {
                    TextBox.Columns = cols;
                }

                // Set rows
                int rows = ValidationHelper.GetInteger(GetValue("rows"), 0);
                if (rows > 0)
                {
                    TextBox.Rows = rows;
                }

                // Set size
                TextBox.MaxLength = MaxLength;

                TextBox.TextMode = TextBoxMode.MultiLine;
                TextBox.Wrap = ValidationHelper.GetBoolean(GetValue("wrap"), true);
            }
            else
            {
                CheckMinMaxLength = true;
            }

            // Apply CSS styles
            if (!String.IsNullOrEmpty(CssClass))
            {
                TextBox.AddCssClass(CssClass);
                CssClass = null;
            }

            if (!String.IsNullOrEmpty(ControlStyle))
            {
                TextBox.Attributes.Add("style", ControlStyle);
                ControlStyle = null;
            }

            CheckRegularExpression = true;
            CheckFieldEmptiness = true;

            if (!string.IsNullOrEmpty(ValueAccessFunctionName))
            {
                // Render value access function
                var valueAccessFunction = string.Format("function {0}() {{ return document.getElementById('{1}').value; }}", ValueAccessFunctionName, TextBox.ClientID);
                ScriptHelper.RegisterClientScriptBlock(this, typeof(string), ValueAccessFunctionName, valueAccessFunction, true);
            }
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ApplyWatermark();
            ApplyFilter();
            ApplyAutocomplete();
        }


        private void ApplyAutocomplete()
        {
            // Autocomplete extender
            if (!string.IsNullOrEmpty(AutoCompleteServiceMethod) && !string.IsNullOrEmpty(AutoCompleteServicePath))
            {
                // Create extender
                AutoCompleteExtender exAuto = new AutoCompleteExtender();
                exAuto.ID = "exAuto";
                exAuto.TargetControlID = TextBox.ID;
                exAuto.EnableViewState = false;
                Controls.Add(exAuto);

                exAuto.ServiceMethod = AutoCompleteServiceMethod;
                exAuto.ServicePath = UrlResolver.ResolveUrl(AutoCompleteServicePath);
                exAuto.MinimumPrefixLength = AutoCompleteMinimumPrefixLength;
                exAuto.ContextKey = ContextResolver.ResolveMacros(AutoCompleteContextKey);
                exAuto.CompletionInterval = AutoCompleteCompletionInterval;
                exAuto.EnableCaching = AutoCompleteEnableCaching;
                exAuto.CompletionSetCount = AutoCompleteCompletionSetCount;
                exAuto.CompletionListCssClass = AutoCompleteCompletionListCssClass;
                exAuto.CompletionListItemCssClass = AutoCompleteCompletionListItemCssClass;
                exAuto.CompletionListHighlightedItemCssClass = AutoCompleteCompletionListHighlightedItemCssClass;
                exAuto.DelimiterCharacters = AutoCompleteDelimiterCharacters;
                exAuto.FirstRowSelected = AutoCompleteFirstRowSelected;
                exAuto.ShowOnlyCurrentWordInCompletionListItem = AutoCompleteShowOnlyCurrentWordInCompletionListItem;
            }
        }


        private void ApplyWatermark()
        {
            // Watermark extender
            // Disable watermark extender for nonempty fields (issue with value which is same as the watermark text)
            string resolvedWatermarkText = ContextResolver.ResolveMacros(WatermarkText);
            if (!String.IsNullOrEmpty(WatermarkText) && !String.IsNullOrEmpty(resolvedWatermarkText) && !CMSString.Equals(TextBox.Text, WatermarkText))
            {
                // Create extender
                TextBoxWatermarkExtender exWatermark = new TextBoxWatermarkExtender();
                exWatermark.ID = "exWatermark";
                exWatermark.TargetControlID = TextBox.ID;
                exWatermark.EnableViewState = false;
                Controls.Add(exWatermark);

                // Initialize extender
                exWatermark.WatermarkText = resolvedWatermarkText;
                exWatermark.WatermarkCssClass = TextBox.CssClass + " " + ValidationHelper.GetString(GetValue("WatermarkCssClass"), WatermarkCssClass);
            }
        }


        private void ApplyFilter()
        {
            if (FilterEnabled)
            {
                // Create extender
                FilteredTextBoxExtender exFilter = new FilteredTextBoxExtender();
                exFilter.ID = "exFilter";
                exFilter.TargetControlID = TextBox.ID;
                exFilter.EnableViewState = false;
                Controls.Add(exFilter);

                // Filter extender
                exFilter.FilterInterval = FilterInterval;

                // Set the filter type
                if (FilterTypeValue == null)
                {
                    exFilter.FilterType = FilterType;
                }
                else
                {
                    if (!string.IsNullOrEmpty(FilterTypeValue))
                    {
                        string[] types = FilterTypeValue.Split(new[]
                        {
                            ';',
                            '|'
                        }, StringSplitOptions.RemoveEmptyEntries);

                        if (types.Length > 0)
                        {
                            var filterType = GetFilterType(types);

                            exFilter.FilterType = filterType;
                        }
                    }
                }

                FilterModes filterMode = FilterMode;

                // Set valid and invalid characters
                if (exFilter.FilterType == FilterTypes.Custom)
                {
                    // When filter type is Custom only, filter mode can be anything
                    exFilter.FilterMode = filterMode;

                    if (filterMode == FilterModes.InvalidChars)
                    {
                        exFilter.InvalidChars = InvalidChars;
                    }
                    else
                    {
                        exFilter.ValidChars = ValidChars;
                    }
                }
                else
                {
                    // Otherwise filter type must be valid chars
                    exFilter.FilterMode = FilterModes.ValidChars;

                    // Set valid chars only if original filter mode was valid chars and filter type contains Custom
                    if ((filterMode == FilterModes.ValidChars) && ((exFilter.FilterType & FilterTypes.Custom) != 0))
                    {
                        exFilter.ValidChars = ValidChars;
                    }
                }
            }
        }


        private static FilterTypes GetFilterType(IEnumerable<string> types)
        {
            FilterTypes filterType = 0;

            foreach (string typeStr in types)
            {
                int type = ValidationHelper.GetInteger(typeStr, 0);
                switch (type)
                {
                    case FILTER_NUMBERS:
                        filterType |= FilterTypes.Numbers;
                        break;

                    case FILTER_LOWERCASE:
                        filterType |= FilterTypes.LowercaseLetters;
                        break;

                    case FILTER_UPPERCASE:
                        filterType |= FilterTypes.UppercaseLetters;
                        break;

                    case FILTER_CUSTOM:
                        filterType |= FilterTypes.Custom;
                        break;
                }
            }
            return filterType;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns the list of the field IDs (Client IDs of the inner controls) that should be spell checked.
        /// </summary>
        public override List<string> GetSpellCheckFields()
        {
            List<string> result = new List<string>();
            result.Add(TextBox.ClientID);
            return result;
        }


        /// <summary>
        /// Returns true if user control is valid.
        /// </summary>
        public override bool IsValid()
        {
            if (IsTextArea)
            {
                int maxControlSize = MaxLength;
                string error = null;

                // Get text length
                int textLength = TextBox.Text.Length;
                if (Trim)
                {
                    // Get trimmed text length
                    textLength = TextBox.Text.Trim().Length;
                }

                // Check max text length
                bool valid = CheckLength(0, maxControlSize, textLength, ref error, ErrorMessage);
                ValidationError = error;

                return valid;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// Sets client-side max length of the textbox control.
        /// </summary>
        private void SetClientSideMaxLength()
        {
            if (FieldInfo != null)
            {
                var maxLength = FieldInfo.GetMaxInputLength();
                if (maxLength > 0)
                {
                    TextBox.MaxLength = maxLength;
                }
            }
        }

        #endregion
    }
}