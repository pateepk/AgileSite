using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for SettingsGroupViewer, so it is possible to connect to events on settings page.
    /// </summary>
    public abstract class SettingsGroupViewerControl : CMSAdminEditControl
    {
        #region "Private variables"

        private AdvancedHandler mSettingsSave;

        // Settings
        private int mCategoryId;
        private string mCategoryName;
        private SettingsCategoryInfo mSettingsCategoryInfo;

        // Site
        private int mSiteId;
        private SiteInfo mSiteInfo;

        // Search
        private const int mSearchLimit = 2;

        #endregion


        #region "Properties"

        /// <summary>
        /// Text to search for
        /// </summary>
        protected string SearchText
        {
            get;
            set;
        } = String.Empty;


        /// <summary>
        /// Indicates if the key description should be included in the search
        /// </summary>
        protected bool SearchDescription
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Event fires when settings keys are being saved.
        /// </summary>
        public AdvancedHandler SettingsSave => mSettingsSave ?? (mSettingsSave = new AdvancedHandler());


        /// <summary>
        /// Gets or sets the settings category ID.
        /// </summary>
        public int CategoryID
        {
            get
            {
                if ((mCategoryId == 0) && (SettingsCategoryInfo != null))
                {
                    mCategoryId = SettingsCategoryInfo.CategoryID;
                }
                return mCategoryId;
            }
            set
            {
                mCategoryId = value;
                mCategoryName = null;
                mSettingsCategoryInfo = null;
            }
        }


        /// <summary>
        /// Gets or sets the settings category name.
        /// </summary>
        public string CategoryName
        {
            get
            {
                if ((mCategoryName == null) && (SettingsCategoryInfo != null))
                {
                    mCategoryName = SettingsCategoryInfo.CategoryName;
                }
                return mCategoryName;
            }
            set
            {
                mCategoryName = value;
                mCategoryId = 0;
                mSettingsCategoryInfo = null;
            }
        }


        /// <summary>
        /// Gets the SettingsCategoryInfo object for the specified CategoryID or CategoryName respectively.
        /// </summary>
        public SettingsCategoryInfo SettingsCategoryInfo
        {
            get
            {
                if (mSettingsCategoryInfo == null)
                {
                    if (mCategoryId > 0)
                    {
                        mSettingsCategoryInfo = SettingsCategoryInfoProvider.GetSettingsCategoryInfo(mCategoryId);
                    }
                    else
                    {
                        if (mCategoryName != null)
                        {
                            mSettingsCategoryInfo = SettingsCategoryInfoProvider.GetSettingsCategoryInfoByName(mCategoryName);
                        }
                    }
                }
                return mSettingsCategoryInfo;
            }
        }


        /// <summary>
        /// Gets the settings keys list for the current category.
        /// </summary>
        public List<SettingsKeyItem> KeyItems
        {
            get;
        } = new List<SettingsKeyItem>();


        /// <summary>
        /// ID of the site.
        /// </summary>
        public int SiteID
        {
            get
            {
                if ((mSiteId == 0) && SiteInfo != null)
                {
                    mSiteId = SiteInfo.SiteID;
                }
                return mSiteId;
            }
            set
            {
                mSiteId = value;
                mSiteInfo = null;
            }
        }


        /// <summary>
        /// Gets the site info object for the configured site.
        /// </summary>
        public SiteInfo SiteInfo
        {
            get
            {
                if (mSiteInfo == null && mSiteId != 0)
                {
                     mSiteInfo = SiteInfoProvider.GetSiteInfo(mSiteId);
                }

                return mSiteInfo;
            }
        }


        /// <summary>
        /// Gets or sets the where condition used to filter settings groups.
        /// All groups will be selected if not set.
        /// </summary>
        public string Where
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates if "these settings are global ..." message can shown.
        /// Is true by default.
        /// </summary>
        public bool AllowGlobalInfoMessage
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Gets a value that indicates if a valid search text is specified.
        /// </summary>
        private bool IsSearchTextValid => !string.IsNullOrEmpty(SearchText) && (SearchText.Length >= mSearchLimit);

        #endregion


        #region "Save methods"

        /// <summary>
        /// Validates the settings values and returns true if all are valid.
        /// </summary>
        private bool IsValid()
        {
            // Loop through all settings items
            for (int i = 0; i < KeyItems.Count; i++)
            {
                SettingsKeyItem item = KeyItems[i];

                var keyChanged = false;

                if (item.ValueControl is CMSTextBox)
                {
                    var textBox = (CMSTextBox)item.ValueControl;
                    textBox.Text = textBox.Text.Trim();
                    keyChanged = (textBox.Text != item.KeyValue);
                    item.KeyValue = textBox.Text;
                }
                else if (item.ValueControl is CMSCheckBox)
                {
                    var checkBox = (CMSCheckBox)item.ValueControl;
                    keyChanged = (checkBox.Checked.ToString() != item.KeyValue);
                    item.KeyValue = checkBox.Checked.ToString();
                }
                else if (item.ValueControl is FormEngineUserControl)
                {
                    var control = (FormEngineUserControl)item.ValueControl;
                    if (control.IsValid())
                    {
                        keyChanged = Convert.ToString(control.Value) != item.KeyValue;
                        item.KeyValue = Convert.ToString(control.Value);
                    }
                    else
                    {
                        item.ErrorLabel.Text = String.IsNullOrEmpty(control.ErrorMessage) ? GetString("Settings.ValidationError") : control.ErrorMessage;
                        item.ErrorLabel.Visible = !String.IsNullOrEmpty(item.ErrorLabel.Text);
                        ShowError(GetString("general.saveerror"));
                        return false;
                    }
                }

                if (item.InheritCheckBox != null)
                {
                    var inheritanceChanged = item.InheritCheckBox.Checked != item.KeyIsInherited;
                    keyChanged = inheritanceChanged || !item.KeyIsInherited && keyChanged;
                    item.KeyIsInherited = item.InheritCheckBox.Checked;
                }

                item.KeyChanged = keyChanged;
                if (!keyChanged)
                {
                    continue;
                }

                // Validation result
                string result = string.Empty;

                // Validation using regular expression if there is any
                if (!string.IsNullOrEmpty(item.ValidationRegexPattern) && (item.ValidationRegexPattern.Trim() != string.Empty))
                {
                    result = new Validator().IsRegularExp(item.KeyValue, item.ValidationRegexPattern, GetString("Settings.ValidationRegExError")).Result;
                }

                // Validation according to the value type (validate only nonempty values)
                if (string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(item.KeyValue))
                {
                    switch (item.KeyType.ToLowerInvariant())
                    {
                        case "int":
                            result = new Validator().IsInteger(item.KeyValue, GetString("Settings.ValidationIntError")).Result;
                            break;

                        case "double":
                            result = new Validator().IsDouble(item.KeyValue, GetString("Settings.ValidationDoubleError")).Result;
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(result))
                {
                    item.ErrorLabel.Text = result;
                    item.ErrorLabel.Visible = !String.IsNullOrEmpty(result);
                    return false;
                }

                // Update changes
                KeyItems[i] = item;
            }

            return true;
        }


        /// <summary>
        /// Saves changes made to settings keys into the database.
        /// </summary>
        public void SaveChanges()
        {
            // Validate values
            var isValid = IsValid();
            if (!isValid)
            {
                ShowError(GetString("general.saveerror"));
                return;
            }

            using (var h = SettingsSave.StartEvent())
            {
                if (h.CanContinue())
                {
                    // Update changes in database and hashtables
                    foreach (SettingsKeyItem tmpItem in KeyItems)
                    {
                        // Save only changed settings
                        if (!tmpItem.KeyChanged)
                        {
                            continue;
                        }

                        string keyName = tmpItem.KeyName;

                        object keyValue = tmpItem.KeyValue;
                        if (tmpItem.KeyIsInherited)
                        {
                            keyValue = DBNull.Value;
                        }

                        SettingsKeyInfoProvider.SetValue(keyName, SiteID, keyValue);
                    }

                    // Show message
                    ShowChangesSaved();
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Resets all keys in the current category to the default value.
        /// </summary>
        public void ResetToDefault()
        {
            if (SettingsCategoryInfo == null)
            {
                return;
            }

            // Get keys
            IEnumerable<SettingsKeyInfo> keys = GetGroups(SettingsCategoryInfo.CategoryName).SelectMany(g => GetKeys(g.CategoryID));

            // Set default values
            foreach (var key in keys)
            {
                SettingsKeyInfoProvider.SetValue(key.KeyName, SiteID, key.KeyDefaultValue);
            }
        }

        #endregion


        #region "Controls methods"

        /// <summary>
        /// Gets FormEngineUserControl instance for the input SettingsKeyInfo object.
        /// </summary>
        /// <param name="key">SettingsKeyInfo</param>
        /// <param name="groupNo">Number representing index of the processing settings group</param>
        /// <param name="keyNo">Number representing index of the processing SettingsKeyInfo</param>
        protected FormEngineUserControl GetFormEngineUserControl(SettingsKeyInfo key, int groupNo, int keyNo)
        {
            string controlNameOrPath = key.KeyEditingControlPath;
            if (string.IsNullOrEmpty(controlNameOrPath))
            {
                return null;
            }

            // Try to get form control by its name
            FormEngineUserControl control = null;
            var formUserControl = FormUserControlInfoProvider.GetFormUserControlInfo(controlNameOrPath);
            if (formUserControl != null)
            {
                control = FormUserControlLoader.LoadFormControl(Page, controlNameOrPath, "", loadDefaultProperties: false);

                if (control != null)
                {
                    var fi = FormHelper.GetFormControlParameters(controlNameOrPath, formUserControl.UserControlMergedParameters, false);
                    control.LoadDefaultProperties(fi);

                    if (!string.IsNullOrEmpty(key.KeyFormControlSettings))
                    {
                        control.FieldInfo = FormHelper.GetFormControlSettingsFromXML(key.KeyFormControlSettings);
                        control.LoadControlFromFFI();
                    }
                }
            }
            else
            {
                try
                {
                    control = Page.LoadUserControl(controlNameOrPath) as FormEngineUserControl;
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("Settings", "LoadControl", ex);
                }
            }

            if (control == null)
            {
                return null;
            }

            control.ID = $"key{groupNo}_{keyNo}";
            control.IsLiveSite = false;

            return control;
        }


        /// <summary>
        /// Gets <c>CategoryPanel</c> instance for the input settings group.
        /// </summary>
        /// <param name="group"><c>SettingsCategoryInfo</c> instance representing settings group</param>
        /// <param name="groupNo">Number representing index of the processing settings group</param>
        protected CategoryPanel GetCategoryPanel(SettingsCategoryInfo group, int groupNo)
        {
            string title;
            if (IsSearchTextValid)
            {
                var categories = SettingsCategoryInfoProvider.GetCategoriesOnPath(group.CategoryIDPath);
                var categoryNames = categories.Select(c =>
                {
                    var displayName = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(c.CategoryDisplayName));
                    if (c.CategoryIsGroup)
                    {
                        return displayName;
                    }

                    var url = $"~/CMSModules/Settings/Pages/Categories.aspx?selectedCategoryId={c.CategoryID}&selectedSiteId={SiteID}";
                    url = ResolveUrl(url);

                    var name = $"<a href=\"\" onclick=\"selectCategory('{url}');\">{displayName}</a>";
                    return name;
                });
                title = categoryNames.Join(" > ");
            }
            else
            {
                title = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(group.CategoryDisplayName));
            }

            var panel = new CategoryPanel
            {
                ID = $"CategoryPanel{groupNo}",
                DisplayRightPanel = false,
                AllowCollapsing = false,
                Text = title,
                RenderAs = HtmlTextWriterTag.Div
            };

            return panel;
        }


        /// <summary>
        /// Gets <c>CheckBox</c> control used for key value editing.
        /// </summary>
        /// <param name="groupNo">Number representing index of the processing settings group</param>
        /// <param name="keyNo">Number representing index of the processing SettingsKeyInfo</param>
        /// <param name="checked">Checked</param>
        /// <param name="enabled">Enabled</param>
        protected CMSCheckBox GetValueCheckBox(int groupNo, int keyNo, bool @checked, bool enabled)
        {
            var chkValue = new CMSCheckBox
            {
                ID = $"chkKey{groupNo}_{keyNo}",
                EnableViewState = false,
                Checked = @checked,
                Enabled = enabled,
                CssClass = "checkbox-no-label"
            };

            return chkValue;
        }


        /// <summary>
        /// Gets <c>TextBox</c> control used for key value editing.
        /// </summary>
        /// <param name="groupNo">Number representing index of the processing settings group</param>
        /// <param name="keyNo">Number representing index of the processing SettingsKeyInfo</param>
        /// <param name="text">Text</param>
        /// <param name="enabled">Enabled</param>
        protected TextBox GetValueTextBox(int groupNo, int keyNo, string text, bool enabled)
        {
            var txtValue = new CMSTextBox
            {
                ID = $"txtKey{groupNo}_{keyNo}",
                EnableViewState = false,
                Text = text,
                Enabled = enabled,
            };

            return txtValue;
        }


        /// <summary>
        /// Gets the text area form engine user control used for key value editing.
        /// </summary>
        /// <param name="groupNo">Number representing index of the processing settings group</param>
        /// <param name="keyNo">Number representing index of the processing SettingsKeyInfo</param>
        /// <param name="text">Text</param>
        /// <param name="enabled">Enabled</param>
        protected FormEngineUserControl GetValueTextArea(int groupNo, int keyNo, string text, bool enabled)
        {
            try
            {
                var txtValue = (FormEngineUserControl)LoadControl("~/CMSFormControls/Inputs/LargeTextArea.ascx");
                txtValue.ID = $"txtKey{groupNo}_{keyNo}";
                txtValue.EnableViewState = false;
                txtValue.Enabled = enabled;
                txtValue.Value = text;
                return txtValue;
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("Settings", "LOADCONTROL", ex);
                return null;
            }
        }


        /// <summary>
        /// Gets inherit <c>CheckBox</c> instance for the input <c>SettingsKeyInfo</c> object.
        /// </summary>
        /// <param name="groupNo">Number representing index of the processing settings group</param>
        /// <param name="keyNo">Number representing index of the processing SettingsKeyInfo</param>
        protected CMSCheckBox GetInheritCheckBox(int groupNo, int keyNo)
        {
            var chkInherit = new CMSCheckBox
            {
                ID = $"chkInherit{groupNo}_{keyNo}",
                Text = GetString("settings.keys.checkboxinheritglobal"),
                EnableViewState = true,
                AutoPostBack = true,
                CssClass = "field-value-override-checkbox"
            };

            return chkInherit;
        }


        /// <summary>
        /// Gets <c>Label</c> instance for the input <c>SettingsKeyInfo</c> object.
        /// </summary>
        /// <param name="groupNo">Number representing index of the processing settings group</param>
        /// <param name="keyNo">Number representing index of the processing SettingsKeyInfo</param>
        protected Label GetLabelError(int groupNo, int keyNo)
        {
            var label = new Label
            {
                ID = $"lblError{groupNo}_{keyNo}",
                EnableViewState = false,
                CssClass = "form-control-error",
                Visible = false
            };

            return label;
        }


        /// <summary>
        /// Gets <c>Label</c> instance for the input <c>SettingsKeyInfo</c> object.
        /// </summary>
        /// <param name="settingsKey"><c>SettingsKeyInfo</c> instance</param>
        /// <param name="inputControl">Input control associated to the label</param>
        /// <param name="groupNo">Number representing index of the processing settings group</param>
        /// <param name="keyNo">Number representing index of the processing SettingsKeyInfo</param>
        protected Label GetLabel(SettingsKeyInfo settingsKey, Control inputControl, int groupNo, int keyNo)
        {
            LocalizedLabel label = new LocalizedLabel
            {
                EnableViewState = false,
                ID = $"lblDispName{groupNo}_{keyNo}",
                CssClass = "control-label editing-form-label",
                Text = HTMLHelper.HTMLEncode(settingsKey.KeyDisplayName),
                DisplayColon = true
            };
            if (inputControl != null)
            {
                label.AssociatedControlID = inputControl.ID;
            }

            ScriptHelper.AppendTooltip(label, ResHelper.LocalizeString(settingsKey.KeyDescription), null);

            return label;
        }
        
        #endregion


        #region "Settings methods"

        /// <summary>
        /// If search text is valid, returns all setting groups. If not, returns all child groups of the specified category. 
        /// </summary>
        /// <param name="categoryName">Name of the parent setting category</param>
        /// <returns></returns>
        protected IEnumerable<SettingsCategoryInfo> GetGroups(string categoryName)
        {
            if (IsSearchTextValid)
            {
                var groups = SettingsCategoryInfoProvider.GetSettingsCategories("CategoryIsGroup = 1", "CategoryName");
                return groups;
            }
            else
            {
                var groups = SettingsCategoryInfoProvider.GetChildSettingsCategories(categoryName, Where);
                return groups.Where(c => c.CategoryIsGroup);
            }
        }


        /// <summary>
        /// Returns settings keys for given category, optionally filtered by site or search text. 
        /// </summary>
        /// <param name="categoryID">Settings category ID</param>
        protected IEnumerable<SettingsKeyInfo> GetKeys(int categoryID)
        {
            var query = SettingsKeyInfoProvider.GetSettingsKeys(categoryID)
                                               .WhereEqualsOrNull("KeyIsHidden", false);

            if (!CMSActionContext.CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                query.WhereNotIn(nameof(SettingsKeyInfo.KeyName), ProtectedSettings.KeyNames);
            }

            if (SiteID > 0)
            {
                query.WhereEqualsOrNull("KeyIsGlobal", false);
            }

            IEnumerable<SettingsKeyInfo> keys = query.OrderBy("KeyOrder", "KeyDisplayName");

            if (IsSearchTextValid)
            {
                keys = keys.Where(k => SettingsKeyInfoProvider.SearchSettingsKey(k, SearchText, SearchDescription));
            }

            return keys;
        }

        #endregion


        #region "Types"

        /// <summary>
        /// Setting key item.
        /// </summary>
        public struct SettingsKeyItem
        {
            /// <summary>
            /// Key name.
            /// </summary>
            public string KeyName;


            /// <summary>
            /// Key type.
            /// </summary>
            public string KeyType;


            /// <summary>
            /// Key value.
            /// </summary>
            public string KeyValue;


            /// <summary>
            /// Indicates if key is inherited.
            /// </summary>
            public bool KeyIsInherited;


            /// <summary>
            /// Indicates if key was changed.
            /// </summary>
            public bool KeyChanged;


            /// <summary>
            /// Setting category name.
            /// </summary>
            public string CategoryName;


            /// <summary>
            /// Key explanation text.
            /// </summary>
            public string ExplanationText;


            /// <summary>
            /// Validation regex pattern.
            /// </summary>
            public string ValidationRegexPattern;


            /// <summary>
            /// Value control.
            /// </summary>
            public Control ValueControl;


            /// <summary>
            /// Inheritance checkbox control.
            /// </summary>
            public CMSCheckBox InheritCheckBox;

            /// <summary>
            /// Error label control.
            /// </summary>
            public Label ErrorLabel;


            /// <summary>
            /// Parent category panel control.
            /// </summary>
            public CategoryPanel ParentCategoryPanel;
        }

        #endregion
    }
}