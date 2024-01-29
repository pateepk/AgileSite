using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Control provides editing of field options.
    /// </summary>
    public class OptionsDesigner : CompositeControl
    {
        #region "Variables and constants"

        private readonly SpecialFieldsDefinition mOptionDefinition = new SpecialFieldsDefinition();
        private readonly List<FormEngineUserControl> mOptionsControls = new List<FormEngineUserControl>();
        private readonly List<CheckBox> mDefaultValues = new List<CheckBox>();
        private readonly List<CMSAccessibleButton> mRemoveActions = new List<CMSAccessibleButton>();
        private readonly Panel mPnlOptions = new Panel
        {
            CssClass = "options-designer-options"
        };
        private readonly LocalizedLabel mLblAdd = new LocalizedLabel
        {
            CssClass = "Info",
            EnableViewState = false,
            ResourceString = "OptionsDesigner.AddOptionInfo"
        };


        /// <summary>
        /// Indicates which row is selected (-1 indicates that no row is selected).
        /// </summary>
        private int mSelectedRow = -1;


        /// <summary>
        /// Constant used as a value for options with empty text
        /// </summary>
        private const string EMPTY_OPTION = "##EMPTY##";


        /// <summary>
        /// Default option name - used as a prefix with number
        /// </summary>
        public const string DEFAULT_OPTION = "Option ";

        #endregion


        #region "Delegates & events"

        /// <summary>
        /// Fired if new option is added. 
        /// </summary>
        public event EventHandler OptionAdded;

        #endregion


        #region "Private properties"

        private string mOptionsDefinitionString
        {
            get
            {
                return ValidationHelper.GetString(ViewState["OptionsDefinition"], string.Empty);
            }
            set
            {
                ViewState["OptionsDefinition"] = value;
            }
        }


        private string mValue
        {
            get
            {
                return ValidationHelper.GetString(ViewState["Value"], string.Empty);
            }
            set
            {
                ViewState["Value"] = value;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets options definition in format "value;name" separated by new line.
        /// </summary>
        public string OptionsDefinition
        {
            get
            {
                for (int i = 0; i < mOptionsControls.Count; i++)
                {
                    string newValue = ValidationHelper.GetString(mOptionsControls[i].Value, string.Empty);

                    // Wipe out scripts
                    newValue = HTMLHelper.RemoveScripts(newValue);

                    if (mOptionDefinition[i].Value.StartsWithCSafe(EMPTY_OPTION))
                    {
                        if (!string.IsNullOrEmpty(newValue))
                        {
                            mOptionDefinition[i].Value = newValue;
                        }
                    }
                    else if (mOptionDefinition[i].Text.EqualsCSafe(mOptionDefinition[i].Value))
                    {
                        mOptionDefinition[i].Value = string.IsNullOrEmpty(newValue) ? GetUniqueItem(mOptionDefinition.Values, EMPTY_OPTION) : newValue;
                    }

                    mOptionDefinition[i].Text = newValue;
                }

                return mOptionsDefinitionString = mOptionDefinition.ToString();
            }
            set
            {
                mOptionsDefinitionString = value;
            }
        }


        /// <summary>
        /// Gets or sets options selected by default in format "option1|option2".
        /// </summary>
        public string Value
        {
            get
            {
                string result = string.Empty;
                for (int i = 0; i < mDefaultValues.Count; i++)
                {
                    if (AllowMultipleChoice)
                    {
                        if (mDefaultValues[i].Checked)
                        {
                            result += mOptionDefinition[i].Value + "|";
                        }
                    }
                    else
                    {
                        if (mDefaultValues[i].Checked)
                        {
                            result = mOptionDefinition[i].Value;
                        }
                    }
                }
                return mValue = result.TrimEnd('|');
            }
            set
            {
                mValue = value;
            }
        }


        /// <summary>
        /// Indicates if more than one option can be selected.
        /// </summary>
        public bool AllowMultipleChoice
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["AllowMultipleChoice"], false);
            }
            set
            {
                ViewState["AllowMultipleChoice"] = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CssClass = "options-designer";
            Controls.Add(mPnlOptions);

            var btnAdd = new CMSButton
            {
                ButtonStyle = ButtonStyle.Default,
                Text = ResHelper.GetString("OptionsDesigner.AddOption"),
                EnableViewState = false,
                OnClientClick = @"if (typeof FormBuilder != 'undefined') {
    FormBuilder.showSavingInfo();
}"
            };

            btnAdd.Click += btnAdd_Click;

            Controls.Add(btnAdd);
            Controls.Add(mLblAdd);
        }


        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (RequestHelper.IsPostBack())
            {
                string eventArgument = Page.Request.Params.Get("__EVENTARGUMENT");
                if (!string.IsNullOrEmpty(eventArgument))
                {
                    string[] data = eventArgument.Split(':');

                    switch (data[0])
                    {
                        case "optionDesignerRemoveOption":
                            RemoveOption(Int32.Parse(data[1]));
                            break;

                        case "optionDesignerMoveOption":
                            int previousIndex = Int32.Parse(data[1]);
                            int newIndex = Int32.Parse(data[2]);

                            MoveOption(previousIndex, newIndex);
                            break;
                    }
                }
            }

            GenerateOptions(mOptionsDefinitionString);
        }


        /// <summary>
        /// Pre-render event hadler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            string script = String.Format(@"
$cmsj('#{0} .options-designer-options .option-row').on('click', function(event) {{
    selectOptionRow(this);
    // Stop propagation to prevent remove class by body click event
    event.stopPropagation();
}});

$cmsj('html').on('click', function(event) {{
	$cmsj('#{0} .options-designer-options .selected-row').each(function(index, element){{
		$cmsj(element).removeClass('selected-row');
	}});
}});

function selectOptionRow(optionElement){{
    $cmsj('#{0} .options-designer-options .selected-row').each(function(index, element){{
	    $cmsj(element).removeClass('selected-row');
    }});
    $cmsj(optionElement).addClass('selected-row');
}}

$cmsj('#{0} .options-designer-options').sortable({{
    axis: 'y',
    containment: '#{0} .options-designer-options',
    handle: '.drag-option',
    start: function(event, ui){{
        selectOptionRow(ui.item);
        $cmsj(this).attr('data-previndex', ui.item.index());
    }},
    update: function(event, ui) {{
        var previousIndex = $cmsj(this).attr('data-previndex');
        var newIndex = ui.item.index();
        __doPostBack('{1}', 'optionDesignerMoveOption:' + previousIndex + ':' + newIndex);
        if (typeof FormBuilder != 'undefined') {{
            FormBuilder.showSavingInfo();
        }}
    }}
}});
", ClientID, UniqueID);

            ScriptHelper.RegisterJQueryUI(Page);
            ScriptHelper.RegisterStartupScript(this, GetType(), ClientID + "OptionsDesigner", script, true);

            // Show info message if there is no option available
            mLblAdd.Visible = (mOptionDefinition.Count == 0);
            if (mRemoveActions.Count == 1)
            {
                // Disable remove action if there is only one option available
                mRemoveActions[0].Enabled = false;
            }

            // Hide options panel if empty
            mPnlOptions.Visible = (mPnlOptions.Controls.Count > 0);
        }


        private void btnAdd_Click(object sender, EventArgs e)
        {
            string option = GetUniqueItem(mOptionDefinition.Texts);

            SpecialField specialField = new SpecialField
            {
                Value = option,
                Text = option
            };

            mOptionDefinition.Add(specialField);

            CreateOption(specialField);

            RaiseOnOptionChanged();
        }


        private void RemoveOption(int index)
        {
            mOptionDefinition.RemoveAt(index);

            mOptionsDefinitionString = mOptionDefinition.ToString();
            RaiseOnOptionChanged();
        }


        private void MoveOption(int previousIndex, int newIndex)
        {
            SpecialField specialField = mOptionDefinition[previousIndex];
            mOptionDefinition.Remove(specialField);
            mOptionDefinition.Insert(newIndex, specialField);

            mSelectedRow = newIndex;

            mOptionsDefinitionString = mOptionDefinition.ToString();
            RaiseOnOptionChanged();
        }


        /// <summary>
        /// Raise option added event handler.
        /// </summary>
        protected void RaiseOnOptionChanged()
        {
            if (OptionAdded != null)
            {
                OptionAdded(this, null);
            }
        }


        /// <summary>
        /// Customized LoadViewState.
        /// </summary>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            GenerateOptions(mOptionsDefinitionString);
        }


        /// <summary>
        /// Customized SaveViewState.
        /// </summary>
        protected override object SaveViewState()
        {
            mOptionsDefinitionString = mOptionDefinition.ToString();

            return base.SaveViewState();
        }


        /// <summary>
        /// Generates options controls by the options definition.
        /// </summary>
        /// <param name="options">Options definition</param>
        public void GenerateOptions(string options)
        {
            mOptionsControls.Clear();
            mDefaultValues.Clear();
            mOptionDefinition.Clear();
            mRemoveActions.Clear();
            mOptionDefinition.LoadFromText(options);
            mPnlOptions.Controls.Clear();

            for (int i = 0; i < mOptionDefinition.Count; i++)
            {
                CreateOption(mOptionDefinition[i], i == mSelectedRow);
            }

            SelectDefaultValues();
        }


        private void SelectDefaultValues()
        {
            // Prepare default selected options
            List<string> defaultValues = new List<string>();
            if (!string.IsNullOrEmpty(mValue))
            {
                defaultValues.AddRange(mValue.Split('|'));
            }

            // Select all items that contains default value
            for (int i = 0; i < mOptionsControls.Count; i++)
            {
                if (defaultValues.Contains(mOptionDefinition[i].Value))
                {
                    mDefaultValues[i].Checked = true;
                    // Select only first item
                    if (!AllowMultipleChoice)
                    {
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Returns unique value in format "prefix{number}" where number will be higher than 
        /// highest number from source options. (Apply only to strings in format "prefix{number}").
        /// </summary>
        /// <param name="items">Source collection of items</param>
        /// <param name="prefix">Prefix used in value; default value is 'Option '</param>
        public static string GetUniqueItem(IEnumerable<string> items, string prefix = DEFAULT_OPTION)
        {
            string defaultValue = prefix + "1";
            int previousItemNumber = 0;
            foreach (string item in items)
            {
                if (item.StartsWithCSafe(prefix, true))
                {
                    int itemNumber = ValidationHelper.GetInteger(item.Substring(prefix.Length), 0);
                    if (itemNumber > previousItemNumber)
                    {
                        previousItemNumber = itemNumber;
                        defaultValue = prefix + (previousItemNumber + 1);
                    }
                }
            }

            return defaultValue;
        }


        private void CreateOption(SpecialField option, bool isHighlighted = false)
        {
            mPnlOptions.Controls.Add(new LiteralControl(String.Format("<div class=\"option-row{0}\">", isHighlighted ? " selected-row" : String.Empty)));

            // Add drag icon before checkbox
            CMSIcon imgDragIcon = new CMSIcon
            {
                ID = "imgDragIcon" + mOptionsControls.Count,
                CssClass = "drag-option icon-dots-vertical",
                ToolTip = ResHelper.GetString("OptionsDesigner.MoveOption"),
                EnableViewState = false
            };

            mPnlOptions.Controls.Add(imgDragIcon);

            if (AllowMultipleChoice)
            {
                // Add checkbox before editing control
                var chkIsDefaultValue = new CMSCheckBox();
                chkIsDefaultValue.ID = "chkIsDefault" + mOptionsControls.Count;
                chkIsDefaultValue.ToolTip = ResHelper.GetString("OptionsDesigner.OptionIsCheckedByDefault");
                mPnlOptions.Controls.Add(chkIsDefaultValue);
                mDefaultValues.Add(chkIsDefaultValue);
            }
            else
            {
                // Add radio button before editing control
                var rbIsDefaultValue = new CMSRadioButton();
                rbIsDefaultValue.ID = "chkIsDefault" + mOptionsControls.Count;
                rbIsDefaultValue.GroupName = "optionsGroup";
                rbIsDefaultValue.ToolTip = ResHelper.GetString("OptionsDesigner.OptionIsCheckedByDefault");
                mPnlOptions.Controls.Add(rbIsDefaultValue);
                mDefaultValues.Add(rbIsDefaultValue);
            }

            // Add editing control 
            var textbox = Page.LoadUserControl("~/CMSFormControls/System/LocalizableTextBox.ascx") as FormEngineUserControl;
            if (textbox != null)
            {
                textbox.Value = option.Text;
                textbox.ID = "txtOption" + mOptionsControls.Count;
                mOptionsControls.Add(textbox);
                mPnlOptions.Controls.Add(textbox);
            }

            // Add Remove button
            var removeButton = new CMSAccessibleButton
            {
                IconOnly = true,
                IconCssClass = "icon-bin",
                CssClass = "remove-option",
                ToolTip = ResHelper.GetString("OptionsDesigner.RemoveOption"),
                ID = "btnRemove" + mOptionsControls.Count,
                EnableViewState = false,
                OnClientClick = String.Format(@"
if(confirm('{0}')){{
    __doPostBack('{1}', 'optionDesignerRemoveOption:{2}');
    if (typeof FormBuilder != 'undefined') {{
        FormBuilder.showSavingInfo();
    }}
}}
return false;", ResHelper.GetString("OptionsDesigner.RemoveOptionConfirmation"), UniqueID, mOptionDefinition.IndexOf(option))
            };

            // Store for later
            mRemoveActions.Add(removeButton);

            mPnlOptions.Controls.Add(removeButton);
            mPnlOptions.Controls.Add(new LiteralControl("</div>"));
        }

        #endregion
    }
}
