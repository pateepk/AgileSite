using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Globalization;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Control provides editing of field validation rules.
    /// </summary>
    public class FieldMacroRuleDesigner : CompositeControl
    {
        #region "Variables and constants"

        private FormEngineUserControl mRuleEditor;
        private Panel mEditPanel;

        private readonly Panel mPnlRules = new Panel()
        {
            CssClass = "rule-designer-rules"
        };


        /// <summary>
        /// Indicates that there is no rule edited.
        /// </summary>
        private const int EDITED_INDEX_NONE = -1;


        /// <summary>
        /// Indicates that new rule is edited.
        /// </summary>
        private const int EDITED_INDEX_ADD = -2;

        #endregion


        #region "Delegates & events"

        /// <summary>
        /// Fired if new rule is added.
        /// </summary>
        public event EventHandler RuleAdded;

        #endregion


        #region "Private properties"

        private List<FieldMacroRule> mValue
        {
            get
            {
                return (List<FieldMacroRule>)ViewState["Value"];
            }
            set
            {
                ViewState["Value"] = value;
            }
        }


        private int mEditedIndex
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
        /// Gets or sets list with field macro rules.
        /// </summary>
        public List<FieldMacroRule> Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;

                // Deselect rule if Value is set.
                mEditedIndex = EDITED_INDEX_NONE;
            }
        }


        /// <summary>
        /// Default error message which will be used as watermark.
        /// </summary>
        public string DefaultErrorMessage
        {
            get
            {
                return ValidationHelper.GetString(ViewState["DefaultErrorMessage"], string.Empty);
            }
            set
            {
                ViewState["DefaultErrorMessage"] = value;
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

            CssClass = "field-rule-designer";
            Controls.Add(mPnlRules);

            mRuleEditor = (FormEngineUserControl)Page.LoadUserControl("~/CMSFormControls/System/FieldMacroRuleEditor.ascx");
            mRuleEditor.ID = "ruleEditor";

            CreateEditPanel();

            var btnAdd = new CMSButton()
            {
                ButtonStyle = ButtonStyle.Default,
                Text = ResHelper.GetString("FieldRuleDesigner.AddRule"),
                EnableViewState = false
            };

            btnAdd.Click += btnAdd_Click;

            Controls.Add(btnAdd);
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
                if (!string.IsNullOrEmpty(eventArgument))
                {
                    string[] data = eventArgument.Split(':');

                    switch (data[0])
                    {
                        // Remove selected rule
                        case "ruleDesignerRemoveRule":
                            RemoveRule(Int32.Parse(data[1]));
                            break;

                        // Edit selected rule
                        case "ruleDesignerEditRule":
                            mEditedIndex = Int32.Parse(data[1]);
                            mRuleEditor.Value = mValue[mEditedIndex];
                            break;

                        // Close rule editor
                        case "ruleDesignerHideEditor":
                            mEditedIndex = EDITED_INDEX_NONE;
                            mRuleEditor.Value = null;
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Pre-render event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            GenerateRules();

            // Hide rules panel if empty
            mPnlRules.Visible = (mPnlRules.Controls.Count > 0);
        }


        /// <summary>
        /// Customized LoadViewState.
        /// </summary>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(((Pair)savedState).First);

            // Set watermark text to Error message field in Rule editor
            mRuleEditor.SetValue("DefaultErrorMessage", DefaultErrorMessage);

            GenerateRules();
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
            mEditedIndex = EDITED_INDEX_ADD;
            mRuleEditor.Value = null;
        }


        private void btnSubmit_Click(object sender, EventArgs e)
        {
            FieldMacroRule rule = (FieldMacroRule)mRuleEditor.Value;
            if (rule != null)
            {
                if (mEditedIndex == EDITED_INDEX_ADD)
                {
                    Value.Add(rule);
                }
                else
                {
                    Value[mEditedIndex] = rule;
                }

                mEditedIndex = EDITED_INDEX_NONE;
                mRuleEditor.Value = null;
                RaiseOnRuleChanged();
            }
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            mEditedIndex = EDITED_INDEX_NONE;
            mRuleEditor.Value = null;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Composes panel for rule editing.
        /// </summary>
        private void CreateEditPanel()
        {
            mEditPanel = new Panel()
            {
                CssClass = "rule-row edit-rule-row",
                Visible = true,
                ID = "editPanel"
            };

            var btnSubmit = new CMSButton()
            {
                ButtonStyle = ButtonStyle.Primary,
                Text = ResHelper.GetString("general.apply"),
                ID = "buttonSubmit",
                EnableViewState = false,
                OnClientClick = @"if (typeof FormBuilder != 'undefined') {
    FormBuilder.showSavingInfo();
}"
            };

            var btnCancel = new CMSButton()
            {
                ButtonStyle = ButtonStyle.Default,
                Text = ResHelper.GetString("general.cancel"),
                ID = "buttonCancel",
                EnableViewState = false
            };

            Panel pnlFooter = new Panel()
            {
                ID = "pnlFooter"
            };

            pnlFooter.Controls.Add(btnSubmit);
            pnlFooter.Controls.Add(btnCancel);

            mEditPanel.Controls.Add(mRuleEditor);
            mEditPanel.Controls.Add(pnlFooter);

            btnSubmit.Click += btnSubmit_Click;
            btnCancel.Click += btnCancel_Click;
        }


        /// <summary>
        /// Removes rule with specified index.
        /// </summary>
        private void RemoveRule(int index)
        {
            if (Value == null)
            {
                return;
            }

            mValue.RemoveAt(index);

            RaiseOnRuleChanged();
        }


        /// <summary>
        /// Generates rule controls by the rule list.
        /// </summary>
        private void GenerateRules()
        {
            if (Value == null)
            {
                return;
            }

            mPnlRules.Controls.Clear();

            for (int i = 0; i < mValue.Count; i++)
            {
                if (mEditedIndex == i)
                {
                    AddEditPanel();
                }
                else
                {
                    CreateRule(mValue[i], i == mEditedIndex, i);
                }
            }

            if (mEditedIndex == EDITED_INDEX_ADD)
            {
                AddEditPanel();
            }
        }


        /// <summary>
        /// Adds dialog for rule editing.
        /// </summary>
        private void AddEditPanel()
        {
            mPnlRules.Controls.Add(mEditPanel);
        }


        /// <summary>
        /// Creates default view for given rule and adds it to the main panel.
        /// </summary>
        private void CreateRule(FieldMacroRule rule, bool isHighlighted, int index)
        {
            string clientClick;
            if (isHighlighted)
            {
                clientClick = String.Format(@"__doPostBack('{0}', 'ruleDesignerHideEditor');return false;", UniqueID);
            }
            else
            {
                clientClick = String.Format(@"__doPostBack('{0}', 'ruleDesignerEditRule:{1}');return false;", UniqueID, index);
            }

            mPnlRules.Controls.Add(new LiteralControl(String.Format("<div class=\"rule-row{0}\">", isHighlighted ? " selected-row" : String.Empty)));

            Label label = new Label
                {
                    EnableViewState = false
                };

            bool showWarning = false;

            try
            {
                label.Text = MacroResolver.Resolve(MacroRuleTree.GetRuleText(rule.MacroRule, false, true, TimeZoneTransformation));
                label.Attributes.Add("onclick", clientClick);

                if (label.Text.Trim().StartsWithCSafe("rule(", true))
                {
                    label.Text = MacroRuleTree.GetRuleText(label.Text, false, false, TimeZoneTransformation);
                }
            }
            catch (Exception)
            {
                label.Text = MacroRuleTree.GetRuleText(rule.MacroRule, false, false, TimeZoneTransformation);
                label.Attributes.Add("class", "rule-disabled");
                showWarning = true;
            }

            MacroIdentityOption identityOption;
            label.Text = MacroSecurityProcessor.RemoveMacroSecurityParams(label.Text, out identityOption);

            mPnlRules.Controls.Add(label);

            mPnlRules.Controls.Add(new LiteralControl("<div class=\"rule-actions\">"));

            if (showWarning)
            {
                CMSIcon imgWarning = new CMSIcon()
                {
                    CssClass = "rule-warning icon-exclamation-triangle",
                    ToolTip = String.Format(ResHelper.GetString("FieldRuleDesigner.RuleWarning"), GetMacroRuleName(rule.MacroRule))
                };
                mPnlRules.Controls.Add(imgWarning);
            }

            var removeButton = new CMSAccessibleButton()
            {
                CssClass = "remove-rule",
                IconCssClass = "icon-bin",
                IconOnly = true,
                ToolTip = ResHelper.GetString("FieldRuleDesigner.RemoveRule"),
                ID = "btnRemove" + index,
                EnableViewState = false,
                OnClientClick = String.Format(@"
if(confirm('{0}')){{
    __doPostBack('{1}', 'ruleDesignerRemoveRule:{2}');
    if (typeof FormBuilder != 'undefined') {{
        FormBuilder.showSavingInfo();
    }}
}}
return false;", ResHelper.GetString("FieldRuleDesigner.RemoveRuleConfirmation"), UniqueID, index)
            };

            mPnlRules.Controls.Add(removeButton);
            mPnlRules.Controls.Add(new LiteralControl("</div></div>"));
        }


        /// <summary>
        /// Displays correct timezone for the rule parameter values.
        /// </summary>
        /// <param name="o">Parameter values</param>
        private object TimeZoneTransformation(object o)
        {
            if (o is DateTime)
            {
                DateTime dt = (DateTime)o;
                Globalization.TimeZoneInfo usedTimeZone = null;
                return TimeZoneHelper.GetCurrentTimeZoneDateTimeString(dt, MembershipContext.AuthenticatedUser, SiteContext.CurrentSite, out usedTimeZone);
            }

            return o;
        }


        /// <summary>
        /// Raise rule added event handler.
        /// </summary>
        protected void RaiseOnRuleChanged()
        {
            if (RuleAdded != null)
            {
                RuleAdded(this, null);
            }
        }


        /// <summary>
        /// Returns code name of field validation rule used in specified rule.
        /// </summary>
        /// <param name="ruleText">Rule text</param>
        private string GetMacroRuleName(string ruleText)
        {
            if (!String.IsNullOrEmpty(ruleText))
            {
                try
                {
                    MacroExpression xml = MacroExpression.ExtractParameter(ruleText, "rule", 1);

                    if ((xml != null) && (xml.Type == ExpressionType.Value))
                    {

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(xml.Value.ToString());
                        return doc.SelectSingleNode("//r/@n").Value;
                    }
                }
                catch
                {
                    // XML cannot be loaded
                }
            }

            return String.Empty;
        }

        #endregion
    }
}
