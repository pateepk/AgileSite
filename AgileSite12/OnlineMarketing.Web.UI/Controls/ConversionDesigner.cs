using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Control provides management of A/B Test conversion configurations.
    /// </summary>
    public class ConversionDesigner : FormEngineUserControl
    {
        #region "Variables and constants"

        private FormEngineUserControl mConversionEditor;
        private Panel mEditPanel;
        private CMSButton mBtnAdd;
        private AlertLabel mTestStartedNoConversionLabel;

        private readonly Panel mPnlConversions = new Panel()
        {
            CssClass = "designer-conversions"
        };


        /// <summary>
        /// Indicates that there is no conversion edited.
        /// </summary>
        private const int EDITED_INDEX_NONE = -1;

        /// <summary>
        /// Indicates that new conversion is edited.
        /// </summary>
        private const int EDITED_INDEX_ADD = -2;

        private const int CONVERSION_MAX_LENGTH = 40;

        private const string ACTION_EDIT = "conversionDesignerEdit";

        private const string ACTION_REMOVE = "conversionDesignerRemove";

        private const string DEFAULT_PLACEHOLDER_VALUE = "1";

        #endregion


        #region "Private properties"

        private ABTestConversionConfiguration Configuration
        {
            get
            {
                var viewStateConfiguration = (ABTestConversionConfiguration)ViewState["Configuration"];

                if (viewStateConfiguration != null)
                {
                    return viewStateConfiguration;
                }

                var emptyConfiguration = new ABTestConversionConfiguration(null, String.Empty);
                ViewState["Configuration"] = emptyConfiguration;

                return emptyConfiguration;
            }
            set
            {
                ViewState["Configuration"] = value;
            }
        }


        private int EditedIndex
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["EditedIndex"], EDITED_INDEX_NONE);
            }
            set
            {
                ViewState["EditedIndex"] = value;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets list with conversion settings.
        /// </summary>
        public override object Value
        {
            get
            {
                return Configuration?.Serialize();
            }
            set
            {
                Configuration = new ABTestConversionConfiguration(null, value as string);

                // Deselect conversion if Value is set.
                EditedIndex = EDITED_INDEX_NONE;
            }
        }

        #endregion


        #region "Control events"

        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CssClass = "conversion-designer";

            var updatePanel = new UpdatePanel();
            updatePanel.ContentTemplateContainer.Controls.Add(mPnlConversions);

            mConversionEditor = (FormEngineUserControl)Page.LoadUserControl("~/CMSModules/OnlineMarketing/Controls/UI/ABTest/ConversionEditor.ascx");
            mConversionEditor.ID = "conversionEditor";

            CreateEditPanel();

            AppendAddButton(updatePanel.ContentTemplateContainer.Controls);

            AppendNoConversionText(updatePanel.ContentTemplateContainer.Controls);

            Controls.Add(updatePanel);
        }


        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (RequestHelper.IsPostBack())
            {
                // Handle postback actions...
                string eventArgument = Page.Request.Params.Get("__EVENTARGUMENT");
                if (String.IsNullOrEmpty(eventArgument))
                {
                    return;
                }

                string[] data = eventArgument.Split(':');
                switch (data[0])
                {
                    // Remove selected conversion
                    case ACTION_REMOVE:
                        RemoveConversion(Int32.Parse(data[1]));
                        break;

                    // Edit selected rule
                    case ACTION_EDIT:
                        EditedIndex = Int32.Parse(data[1]);
                        mConversionEditor.Value = Configuration.GetConversionAt(EditedIndex);
                        break;
                }
            }
        }


        /// <summary>
        /// Pre-render event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            GenerateConversions();

            var hasConversions = (mPnlConversions.Controls.Count > 0);

            // Hide rules panel if empty
            mPnlConversions.Visible = hasConversions;

            // Hide "add conversion" button if disabled or the conversion editor is visible
            mBtnAdd.Visible = Enabled && (EditedIndex == EDITED_INDEX_NONE);

            // Hide "Test w/o any conversions"
            mTestStartedNoConversionLabel.Visible = !Enabled && !hasConversions;
        }


        /// <summary>
        /// Customized LoadViewState.
        /// </summary>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(((Pair)savedState).First);

            GenerateConversions();
        }


        /// <summary>
        /// Customized SaveViewState.
        /// </summary>
        protected override object SaveViewState()
        {
            return new Pair(base.SaveViewState(), null);
        }


        private void btnAdd_Click(object sender, EventArgs e)
        {
            EditedIndex = EDITED_INDEX_ADD;
            mConversionEditor.Value = null;
        }


        private void btnSubmit_Click(object sender, EventArgs e)
        {
            var conversion = mConversionEditor.Value as ABTestConversion;
            if (conversion != null)
            {
                if (EditedIndex == EDITED_INDEX_ADD)
                {
                    Configuration.AddConversion(conversion);
                }
                else
                {
                    Configuration.SetConversionAt(EditedIndex, conversion);
                }

                EditedIndex = EDITED_INDEX_NONE;
                mConversionEditor.Value = null;
            }
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            EditedIndex = EDITED_INDEX_NONE;
            mConversionEditor.Value = null;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Composes panel for conversion editing.
        /// </summary>
        private void CreateEditPanel()
        {
            mEditPanel = new Panel()
            {
                CssClass = "conversion-row edit-conversion-row",
                Visible = true,
                ID = "editPanel"
            };

            var btnSubmit = new CMSButton()
            {
                ButtonStyle = ButtonStyle.Primary,
                Text = GetString("general.apply"),
                ID = "buttonSubmit",
                EnableViewState = false
            };

            var btnCancel = new CMSButton()
            {
                ButtonStyle = ButtonStyle.Default,
                Text = GetString("general.cancel"),
                ID = "buttonCancel",
                EnableViewState = false
            };

            var pnlFooter = new Panel()
            {
                ID = "pnlFooter"
            };

            pnlFooter.Controls.Add(btnSubmit);
            pnlFooter.Controls.Add(btnCancel);

            mEditPanel.Controls.Add(mConversionEditor);
            mEditPanel.Controls.Add(pnlFooter);

            btnSubmit.Click += btnSubmit_Click;
            btnCancel.Click += btnCancel_Click;
        }


        /// <summary>
        /// Removes conversion at specified index.
        /// </summary>
        private void RemoveConversion(int index)
        {
            EditedIndex = EDITED_INDEX_NONE;

            if (Configuration == null || index >= Configuration.ConversionsCount)
            {
                return;
            }

            Configuration.RemoveConversionAt(index);
        }


        /// <summary>
        /// Generates conversion controls by the list.
        /// </summary>
        private void GenerateConversions()
        {
            mPnlConversions.Controls.Clear();

            for (int i = 0; i < Configuration.ConversionsCount; i++)
            {
                if (EditedIndex == i)
                {
                    AddEditPanel();
                }
                else
                {
                    CreateConversion(Configuration.GetConversionAt(i), i);
                }
            }

            if (EditedIndex == EDITED_INDEX_ADD)
            {
                AddEditPanel();
            }
        }


        /// <summary>
        /// Adds dialog for conversion editing.
        /// </summary>
        private void AddEditPanel()
        {
            mPnlConversions.Controls.Add(mEditPanel);
        }


        /// <summary>
        /// Creates default view for given conversion and adds it to the main panel.
        /// </summary>
        private void CreateConversion(ABTestConversion conversion, int index)
        {
            mPnlConversions.Controls.Add(new LiteralControl("<div class=\"conversion-row\">") { EnableViewState = false });

            var relatedItem = !String.IsNullOrEmpty(conversion.RelatedItemDisplayName) ? conversion.RelatedItemDisplayName : conversion.RelatedItemIdentifier;
            if (!String.IsNullOrEmpty(relatedItem))
            {
                relatedItem = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(relatedItem)) + "<br />";
            }
            relatedItem = TextHelper.LimitLength(relatedItem, CONVERSION_MAX_LENGTH, CutTextEnum.Start);

            var conversionDefinition = ABTestConversionDefinitionRegister.Instance.Get(conversion.ConversionOriginalName);
            var conversionDefaultValue = conversionDefinition.DefaultValuePlaceholderText ?? DEFAULT_PLACEHOLDER_VALUE;
            var conversionValue = conversion.Value == 0 ? ResHelper.LocalizeString(conversionDefaultValue) : conversion.Value.ToString();
            var conversionCaption = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(conversionDefinition.ConversionDisplayName));

            var caption = new LiteralControl
            {
                EnableViewState = false,
                Text = $"<span class=\"Title\">{conversionCaption}</span><br />{relatedItem}{GetString("general.value")}: {conversionValue}"
            };

            var lblText = new Label
            {
                ID = "lblText" + index,
                EnableViewState = false
            };
            lblText.Controls.Add(caption);

            mPnlConversions.Controls.Add(lblText);

            if (Enabled)
            {
                lblText.Attributes.Add("onclick", $"__doPostBack('{lblText.UniqueID}', '{ACTION_EDIT}:{index}'); return false;");
            }
            else
            {
                lblText.AddCssClass("disabled");
            }

            mPnlConversions.Controls.Add(new LiteralControl("<div class=\"conversion-actions\">") { EnableViewState = false });

            if (Enabled)
            {
                var removeButton = new CMSAccessibleButton
                {
                    CssClass = "remove-conversion",
                    IconCssClass = "icon-bin",
                    IconOnly = true,
                    ToolTip = GetString("general.remove"),
                    ID = "btnRemove" + index,
                    EnableViewState = false
                };

                mPnlConversions.Controls.Add(removeButton);

                removeButton.OnClientClick = $"if(confirm('{GetString("campaign.conversion.deleteconfirmation")}')) {{ __doPostBack('{removeButton.UniqueID}', '{ACTION_REMOVE}:{index}'); }} return false;";
            }

            mPnlConversions.Controls.Add(new LiteralControl("</div></div>") { EnableViewState = false });
        }


        private void AppendAddButton(ControlCollection controls)
        {
            mBtnAdd = new CMSButton()
            {
                ButtonStyle = ButtonStyle.Default,
                Text = GetString("campaign.conversions.add"),
                EnableViewState = false
            };

            mBtnAdd.Click += btnAdd_Click;

            controls.Add(mBtnAdd);
        }


        private void AppendNoConversionText(ControlCollection controls)
        {
            mTestStartedNoConversionLabel = new AlertLabel
            {
                ResourceString = "campaign.conversions.teststartednoconversions",
                AlertType = MessageTypeEnum.Warning,
                EnableViewState = false
            };

            controls.Add(mTestStartedNoConversionLabel);
        }

        #endregion
    }
}
