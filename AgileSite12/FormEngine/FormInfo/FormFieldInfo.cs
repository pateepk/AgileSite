using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Xml;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.FormEngine
{
    /// <summary>
    /// Represents form field definition.
    /// </summary>
    public class FormFieldInfo : FieldBase<FormFieldInfo>
    {
        #region "Variables"

        private bool? mResolveDefaultValue;
        private FormFieldControlTypeEnum mFieldType = FormFieldControlTypeEnum.Unknown;

        private List<FieldMacroRule> mFieldMacroRules;
        private string mFileExtensions;
        private int mMinStringLength = -1;
        private int mMaxStringLength = -1;
        private string mMinValue = string.Empty;
        private string mMaxValue = string.Empty;
        private DateTime mMinDateTimeValue = DateTimeHelper.ZERO_TIME;
        private DateTime mMaxDateTimeValue = DateTimeHelper.ZERO_TIME;
        private string mRegularExpression = string.Empty;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates whether field is visible.
        /// </summary>
        public bool Visible
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Indicates whether field is Enabled.
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Type of the field.
        /// </summary>
        public FormFieldControlTypeEnum FieldType
        {
            get
            {
                return mFieldType;
            }
            set
            {
                mFieldType = value;

                // Add field type as new setting
                if (mFieldType != FormFieldControlTypeEnum.CustomUserControl)
                {
                    Settings[FormHelper.CONTROL_NAME_SETTING_KEY] = Enum.GetName(typeof(FormFieldControlTypeEnum), mFieldType)?.ToLowerInvariant();
                    mFieldType = FormFieldControlTypeEnum.CustomUserControl;
                }
            }
        }


        /// <summary>
        /// Macro table for the settings of the field form control
        /// </summary>
        public Hashtable SettingsMacroTable
        {
            get;
            set;
        } = new Hashtable(StringComparer.InvariantCultureIgnoreCase);


        /// <summary>
        /// Setting of the field form control [name] -> [value]
        /// </summary>
        public Hashtable Settings
        {
            get;
            set;
        } = new Hashtable(StringComparer.InvariantCultureIgnoreCase);


        /// <summary>
        /// List of field validation macro rules.
        /// </summary>
        public List<FieldMacroRule> FieldMacroRules
        {
            get
            {
                return mFieldMacroRules ?? (mFieldMacroRules = new List<FieldMacroRule>());
            }
            set
            {
                mFieldMacroRules = value;
            }
        }


        /// <summary>
        /// Indicates whether field is public.
        /// </summary>
        public bool PublicField
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether field should be spell-checked.
        /// </summary>
        public bool SpellCheck
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Indicates whether user can change field visibility.
        /// </summary>
        public bool AllowUserToChangeVisibility
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates who the field should be visible to.
        /// </summary>
        public FormFieldVisibilityTypeEnum Visibility
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates type of visibility form control.
        /// </summary>
        public string VisibilityControl
        {
            get;
            set;
        }


        /// <summary>
        /// Adjust the context in which the attribute can be displayed.
        /// </summary>
        public string DisplayIn
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value indicating if other fields are depending on this field.
        /// </summary>
        public bool HasDependingFields
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value indicating if field is depending on some other field.
        /// </summary>
        public bool DependsOnAnotherField
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates field to which this one is mapped.
        /// </summary>
        public string MappedToField
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether field is inheritable (value is set as default value of parent FormFieldInfo, field is disabled if possible) or not.
        /// </summary>
        public bool Inheritable
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the field will be included into translation service document while exporting to XLIFF.
        /// </summary>
        public bool TranslateField
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether to display field in simple mode of basicform or not.
        /// </summary>
        public bool DisplayInSimpleMode
        {
            get;
            set;
        }


        /// <summary>
        /// Minimal value.
        /// </summary>
        public string MinValue
        {
            get
            {
                return mMinValue;
            }
        }


        /// <summary>
        /// Maximal value.
        /// </summary>
        public string MaxValue
        {
            get
            {
                return mMaxValue;
            }
        }


        /// <summary>
        /// Minimal datetime value.
        /// </summary>
        public DateTime MinDateTimeValue
        {
            get
            {
                return mMinDateTimeValue;
            }
        }


        /// <summary>
        /// Maximal datetime value.
        /// </summary>
        public DateTime MaxDateTimeValue
        {
            get
            {
                return mMaxDateTimeValue;
            }
        }

        /// <summary>
        /// Minimal string length.
        /// </summary>
        public int MinStringLength
        {
            get
            {
                return mMinStringLength;
            }
        }


        /// <summary>
        /// Maximal string length.
        /// </summary>
        public int MaxStringLength
        {
            get
            {
                return mMaxStringLength;
            }
        }


        /// <summary>
        /// Regular expression.
        /// </summary>
        public string RegularExpression
        {
            get
            {
                return mRegularExpression;
            }
        }


        /// <summary>
        /// Gets or sets the XML data describing validation rules.
        /// </summary>
        public string ValidationRuleConfigurationsXmlData
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the XML data describing visibility condition.
        /// </summary>
        public string VisibilityConditionConfigurationXmlData
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        private void SetValidationProperties()
        {
            foreach (FieldMacroRule rule in FieldMacroRules)
            {
                switch (FormHelper.GetValidationRuleName(rule.MacroRule).ToLowerInvariant())
                {
                    case "minvalue":
                        mMinValue = FormHelper.GetValidationRuleParameter("minvalue", rule.MacroRule);
                        break;
                    case "maxvalue":
                        mMaxValue = FormHelper.GetValidationRuleParameter("maxvalue", rule.MacroRule);
                        break;
                    case "minlength":
                        mMinStringLength = ValidationHelper.GetInteger(FormHelper.GetValidationRuleParameter("minlength", rule.MacroRule), -1);
                        break;
                    case "maxlength":
                        mMaxStringLength = ValidationHelper.GetInteger(FormHelper.GetValidationRuleParameter("maxlength", rule.MacroRule), -1);
                        break;
                    case "datefrom":
                        mMinDateTimeValue = ValidationHelper.GetDateTime(FormHelper.GetValidationRuleParameter("date", rule.MacroRule), DateTimeHelper.ZERO_TIME);
                        break;
                    case "dateto":
                        mMaxDateTimeValue = ValidationHelper.GetDateTime(FormHelper.GetValidationRuleParameter("date", rule.MacroRule), DateTimeHelper.ZERO_TIME);
                        break;
                    case "regexp":
                        mRegularExpression = FormHelper.GetValidationRuleParameter("regexp", rule.MacroRule);
                        break;
                }
            }
        }


        /// <summary>
        /// Loads the field info from XML node
        /// </summary>
        /// <param name="fieldNode">Field node</param>
        public override void LoadFromXmlNode(XmlNode fieldNode)
        {
            // Backward compatibility
            Properties["fielddescription"] = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["fielddescription"], null);
            Properties["validationerrormessage"] = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["validationerrormessage"], null);
            Properties["captionstyle"] = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["captionstyle"], null);
            Properties["captioncssclass"] = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["captioncssclass"], null);
            Properties["inputcontrolstyle"] = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["inputcontrolstyle"], null);
            Properties["controlcssclass"] = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["controlcssclass"], null);
            Properties["fieldcssclass"] = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["fieldcssclass"], null);

            Properties["visiblemacro"] = null;
            Properties["enabledmacro"] = null;

            PropertiesMacroTable["visiblemacro"] = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["visiblemacro"], null);
            PropertiesMacroTable["enabledmacro"] = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["enabledmacro"], null);

            // Load attributes
            Visible = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["visible"], "false"));
            Enabled = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["enabled"], "true"));
            AllowUserToChangeVisibility = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["allowusertochangevisibility"], "false"));
            Visibility = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["visibility"], null).ToEnum<FormFieldVisibilityTypeEnum>();
            VisibilityControl = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["visibilitycontrol"], "DropDownVisibilityControl");
            DisplayIn = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["displayin"], null);
            Inheritable = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["inheritable"], "false"));
            mFileExtensions = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["fileextensions"], null);
            PublicField = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["publicfield"], "true"));
            SpellCheck = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["spellcheck"], "true"));
            HasDependingFields = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["hasdependingfields"], "false"));
            DependsOnAnotherField = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["dependsonanotherfield"], "false"));
            MappedToField = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["mappedtofield"], null);
            TranslateField = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["translatefield"], "false"));
            DisplayInSimpleMode = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["displayinsimplemode"], "false"));
            if (!XmlHelper.XmlAttributeIsEmpty(fieldNode.Attributes["resolvedefaultvalue"]))
            {
                mResolveDefaultValue = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["resolvedefaultvalue"], "true"));
            }

            base.LoadFromXmlNode(fieldNode);

            if (Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["ismacro"], "false")) && Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["resolvedefaultvalue"], "true")))
            {
                PropertiesMacroTable["defaultvalue"] = ValidationHelper.GetString(Properties["defaultvalue"], null);
            }

            // Process settings elements
            var settingsRoot = fieldNode.SelectSingleNode("settings");
            if (settingsRoot != null)
            {
                XmlNodeList settingsNodesList = settingsRoot.ChildNodes;

                // Fill hash table with settings pairs (settings name, settings value)
                foreach (XmlNode settingsNode in settingsNodesList)
                {
                    bool isMacro = ((settingsNode.Attributes["ismacro"] != null) && ValidationHelper.GetBoolean(settingsNode.Attributes["ismacro"].Value, false));
                    if (isMacro)
                    {
                        SettingsMacroTable[settingsNode.Name] = HttpUtility.HtmlDecode(settingsNode.InnerXml);
                        Settings[settingsNode.Name] = null;
                    }
                    else
                    {
                        SettingsMacroTable[settingsNode.Name] = null;
                        Settings[settingsNode.Name] = HttpUtility.HtmlDecode(settingsNode.InnerXml);
                    }
                }
            }
            // Add field type as new setting
            string fieldType = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["fieldtype"], "CustomUserControl");
            if (!String.IsNullOrEmpty(fieldType))
            {
                FieldType = FormHelper.GetFormFieldControlType(fieldType);
            }

            // Convert column 'fileextensions' to settings element 'allowed_extension'
            if (!String.IsNullOrEmpty(mFileExtensions))
            {
                Settings["allowed_extensions"] = mFileExtensions;
            }

            FormHelper.ConvertValidationFromAttributesToRules(fieldNode);
            LoadFieldMacroRules(fieldNode);
            LoadValidationRulesData(fieldNode);
            LoadVisibilityConditionData(fieldNode);

            SetValidationProperties();
        }


        /// <summary>
        /// Loads the field info from plain database structure data.
        /// </summary>
        /// <param name="row">Data row with structure information</param>
        /// <param name="isPrimary">Indicates if field represents primary key</param>
        /// <param name="isSystem">Indicates if field is system field</param>
        /// <remarks>Database structure data can be obtained via <see cref="CMS.DataEngine.TableManager.GetColumnInformation(string, string)"/>.</remarks>
        public override void LoadFromTableData(DataRow row, bool isPrimary, bool isSystem)
        {
            base.LoadFromTableData(row, isPrimary, isSystem);

            FieldType = FormHelper.GetFormFieldDefaultControlType(DataType, PrimaryKey);
            SetPropertyValue(FormFieldPropertyEnum.FieldCaption, Name);
            SetPropertyValue(FormFieldPropertyEnum.DefaultValue, DefaultValue);
        }


        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>Returns clone of FormFiedlInfo</returns>
        public override IDataDefinitionItem Clone()
        {
            var newObject = (FormFieldInfo)base.Clone();

            newObject.Settings = (Hashtable)Settings.Clone();
            newObject.SettingsMacroTable = (Hashtable)SettingsMacroTable.Clone();

            return newObject;
        }


        /// <summary>
        /// Gets XPath expression to select this node in generated XML.
        /// </summary>
        public string GetXPathExpression()
        {
            return "//field[@column='" + Name + "']";
        }


        /// <summary>
        /// Returns the XML node with visibility attribute set to false.
        /// </summary>
        /// <param name="doc">Root document</param>
        public XmlNode GetHiddenNode(XmlDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            // Create new field node
            var newFieldNode = doc.CreateElement("field");

            var attributes = new Dictionary<string, string>
            {
                { "column", Name },
                { "guid", Guid.ToString() },
                { "visible", "false" }
            };

            newFieldNode.AddAttributes(attributes);

            return newFieldNode;
        }


        /// <summary>
        /// Returns the XML node representing the FormFieldInfo object.
        /// </summary>
        /// <param name="doc">XML document</param>
        public override XmlNode GetXmlNode(XmlDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var attributes = GetAttributes();

            // Create new field node
            var newFieldNode = doc.CreateElement("field");

            // Add attributes
            newFieldNode.AddAttributes(attributes);

            // Add properties
            var propertiesNode = doc.CreateElement("properties");
            propertiesNode.AddChildElements(Properties, transform: FormHelper.GetPropertyMacroTransformation(PropertiesMacroTable));
            if (propertiesNode.HasChildNodes)
            {
                newFieldNode.AppendChild(propertiesNode);
            }

            // Add settings
            var settingsNode = doc.CreateElement("settings");
            settingsNode.AddChildElements(Settings, transform: FormHelper.GetPropertyMacroTransformation(SettingsMacroTable));
            if (settingsNode.HasChildNodes)
            {
                newFieldNode.AppendChild(settingsNode);
            }

            // Add macro rules
            FormHelper.AddFieldMacroRules(newFieldNode, FieldMacroRules);

            AddValidationRulesData(newFieldNode);
            AddVisibilityConditionData(newFieldNode);

            return newFieldNode;
        }


        /// <summary>
        /// Adds XML data describing validation rules to <paramref name="fieldNode"/>.
        /// </summary>
        private void AddValidationRulesData(XmlNode fieldNode)
        {
            AddOptionalFieldData(fieldNode, "validationrulesdata", ValidationRuleConfigurationsXmlData);
        }


        /// <summary>
        /// Adds XML data describing visibility condition to <paramref name="fieldNode"/>.
        /// </summary>
        private void AddVisibilityConditionData(XmlNode fieldNode)
        {
            AddOptionalFieldData(fieldNode, "visibilityconditiondata", VisibilityConditionConfigurationXmlData);
        }


        private void AddOptionalFieldData(XmlNode fieldNode, string elementName, string data)
        {
            if (!String.IsNullOrEmpty(data))
            {
                var dataNode = fieldNode.OwnerDocument.CreateElement(elementName);
                dataNode.InnerXml = data;
                fieldNode.AppendChild(dataNode);
            }
        }


        private Dictionary<string, string> GetAttributes()
        {
            var attributes = new Dictionary<string, string>();

            attributes["column"] = Name;
            attributes["visible"] = Visible ? "true" : null;
            attributes["columntype"] = DataType;
            attributes["fieldtype"] = (FieldType != FormFieldControlTypeEnum.CustomUserControl) ? FormHelper.GetFormFieldControlTypeString(FieldType) : null;
            attributes["allowempty"] = AllowEmpty ? "true" : null;
            attributes["isPK"] = PrimaryKey ? "true" : null;
            attributes["system"] = System ? "true" : null;
            attributes["columnsize"] = (Size <= 0) ? null : Convert.ToString(Size);
            attributes["columnprecision"] = (Precision < 0) ? null : Convert.ToString(Precision);
            attributes["publicfield"] = !PublicField ? "false" : null;
            attributes["spellcheck"] = !SpellCheck ? "false" : null;
            attributes["guid"] = Convert.ToString(Guid);
            attributes["external"] = External ? "true" : null;
            attributes["allowusertochangevisibility"] = AllowUserToChangeVisibility ? "true" : null;
            attributes["visibility"] = (Visibility != FormFieldVisibilityTypeEnum.All) ? Visibility.ToStringRepresentation() : null;
            attributes["visibilitycontrol"] = (!string.IsNullOrEmpty(VisibilityControl) && !VisibilityControl.Equals("dropdownvisibilitycontrol", StringComparison.InvariantCultureIgnoreCase)) ? VisibilityControl : null;
            attributes["displayin"] = DisplayIn;
            attributes["hasdependingfields"] = HasDependingFields ? "true" : null;
            attributes["dependsonanotherfield"] = DependsOnAnotherField ? "true" : null;
            attributes["isinherited"] = (IsInherited ? "true" : null);
            attributes["mappedtofield"] = MappedToField?.ToLowerInvariant();
            attributes["inheritable"] = Inheritable ? "true" : null;
            attributes["translatefield"] = TranslateField ? "true" : null;
            attributes["displayinsimplemode"] = DisplayInSimpleMode ? "true" : null;
            attributes["dummy"] = IsDummyField ? (IsDummyFieldFromMainForm ? "mainform" : "altform") : null;
            attributes["extra"] = IsExtraField ? "true" : null;
            attributes["isunique"] = IsUnique ? "true" : null;
            attributes["refobjtype"] = !string.IsNullOrEmpty(ReferenceToObjectType) ? ReferenceToObjectType : null;
            attributes["reftype"] = !string.IsNullOrEmpty(ReferenceToObjectType) ? ReferenceType.ToStringRepresentation() : null;
            attributes["resolvedefaultvalue"] = mResolveDefaultValue?.ToString();

            return attributes;
        }


        /// <summary>
        /// Loads field validation macro rules from given XmlNode.
        /// </summary>
        private void LoadFieldMacroRules(XmlNode fieldNode)
        {
            // Process rules elements
            if (fieldNode.SelectSingleNode("rules") != null)
            {
                XmlNodeList rulesNodesList = fieldNode.SelectSingleNode("rules").ChildNodes;

                foreach (XmlNode rulesNode in rulesNodesList)
                {
                    try
                    {
                        FieldMacroRule rule = new FieldMacroRule();
                        rule.LoadFromXml(rulesNode);
                        FieldMacroRules.Add(rule);
                    }
                    catch
                    {
                        // Macro rule could not be initialized
                    }
                }
            }
        }


        /// <summary>
        /// Loads XML data describing validation rules from <paramref name="fieldNode"/>.
        /// </summary>
        private void LoadValidationRulesData(XmlNode fieldNode)
        {
            LoadOptionalFieldData(fieldNode, "validationrulesdata", data => ValidationRuleConfigurationsXmlData = data);
        }


        /// <summary>
        /// Loads XML data describing visibility condition from <paramref name="fieldNode"/>.
        /// </summary>
        private void LoadVisibilityConditionData(XmlNode fieldNode)
        {
            LoadOptionalFieldData(fieldNode, "visibilityconditiondata", data => VisibilityConditionConfigurationXmlData = data);
        }


        private void LoadOptionalFieldData(XmlNode fieldNode, string elementName, Action<string> loadedDataAction)
        {
            XmlNode data;
            if ((data = fieldNode.SelectSingleNode(elementName)) != null)
            {
                loadedDataAction(data.InnerXml);
            }
        }


        /// <summary>
        /// Returns true if property is macro.
        /// </summary>
        /// <param name="property">Property</param>
        public bool IsMacro(FormFieldPropertyEnum property)
        {
            return PropertiesMacroTable?[property.ToStringRepresentation()] != null;
        }


        /// <summary>
        /// Returns true if setting is macro.
        /// </summary>
        /// <param name="settingName">Setting name</param>
        public bool SettingIsMacro(string settingName)
        {
            return SettingsMacroTable?[settingName] != null;
        }


        /// <summary>
        /// Returns typed resolved default value based on resolver type and field's setting (<see cref="GetResolveDefaultValue(bool)"/>).
        /// </summary>
        /// <param name="resolveType">Resolver type</param>
        /// <param name="macroResolver">Macro resolver</param>
        /// <param name="nullIfDefault">If true, and the resulting value is the default value, returns null</param>
        public object GetTypedDefaultValue(FormResolveTypeEnum resolveType, IMacroResolver macroResolver, bool nullIfDefault = false)
        {
            bool resolveMacro;
            string value = GetStringDefaultValue(resolveType, macroResolver, out resolveMacro);

            // For empty default value and field which allows null, return null
            if (String.IsNullOrEmpty(value) && AllowEmpty)
            {
                return null;
            }

            // Return macro value if resolving is disabled and default value is macro
            if (!resolveMacro && IsMacro(FormFieldPropertyEnum.DefaultValue))
            {
                return value;
            }

            // Return resolved (if enabled) default value converted according to field's data type
            return DataTypeManager.ConvertToSystemType(TypeEnum.Field, DataType, value, CultureHelper.EnglishCulture, nullIfDefault);
        }


        /// <summary>
        /// Returns resolved default value based on resolver type and field's setting (<see cref="GetResolveDefaultValue(bool)"/>).
        /// </summary>
        /// <param name="resolveType">Resolver type</param>
        /// <param name="macroResolver">Macro resolver</param>
        public string GetDefaultValue(FormResolveTypeEnum resolveType, IMacroResolver macroResolver)
        {
            bool resolveMacro;
            return GetStringDefaultValue(resolveType, macroResolver, out resolveMacro);
        }


        /// <summary>
        /// Returns resolved default value based on resolver type. Depends on field's setting (<see cref="GetResolveDefaultValue(bool)"/>).
        /// </summary>
        /// <param name="resolveType">Resolver type</param>
        /// <param name="macroResolver">Macro resolver</param>
        /// <param name="resolveMacro">Returns if macros could be resolved</param>
        private string GetStringDefaultValue(FormResolveTypeEnum resolveType, IMacroResolver macroResolver, out bool resolveMacro)
        {
            resolveMacro = (macroResolver != null) && ((resolveType == FormResolveTypeEnum.AllFields) ||
                    ((resolveType == FormResolveTypeEnum.Hidden) && !Visible) ||
                    (((resolveType == FormResolveTypeEnum.Visible) || (resolveType == FormResolveTypeEnum.WidgetVisible)) && Visible));

            if (resolveMacro && (resolveType != FormResolveTypeEnum.WidgetVisible) && (mResolveDefaultValue != null))
            {
                // Apply field's setting
                resolveMacro &= mResolveDefaultValue.Value;
            }

            return GetPropertyValue(FormFieldPropertyEnum.DefaultValue, resolveMacro ? macroResolver : null, null, resolveType == FormResolveTypeEnum.WidgetVisible);
        }


        /// <summary>
        /// Returns string that represents human readable form of field name.
        /// Tries to resolve field's caption.
        /// Field name is returned in case of invalid macro or empty caption.
        /// </summary>
        /// <param name="macroResolver">Macro resolver used to resolve caption, can be null</param>
        public string GetDisplayName(IMacroResolver macroResolver)
        {
            // Get field caption
            var fieldCaption = GetPropertyValue(FormFieldPropertyEnum.FieldCaption, macroResolver);

            // Field caption may contain invalid macro or doesn't have to be filled at all
            // So if empty, use field name
            if (String.IsNullOrEmpty(fieldCaption))
            {
                fieldCaption = Name;
            }

            return fieldCaption;
        }


        /// <summary>
        /// Gets unresolved property value.
        /// </summary>
        /// <param name="property">Property</param>
        public string GetPropertyValue(FormFieldPropertyEnum property)
        {
            bool isMacro;
            return GetPropertyValue(property, out isMacro);
        }


        /// <summary>
        /// Gets unresolved property value.
        /// </summary>
        /// <param name="property">Property</param>
        /// <param name="isMacro">Returns true if property contains macro</param>
        public string GetPropertyValue(FormFieldPropertyEnum property, out bool isMacro)
        {
            isMacro = IsMacro(property);
            if (isMacro)
            {
                return ValidationHelper.GetString(PropertiesMacroTable[property.ToStringRepresentation()], null);
            }

            return ValidationHelper.GetString(Properties[property.ToStringRepresentation()], null);
        }


        /// <summary>
        /// Gets resolved property value.
        /// </summary>
        /// <param name="property">Property</param>
        /// <param name="resolver">Macro resolver</param>
        /// <param name="macroSettings">Macro context</param>
        /// <param name="resolveAllMacros">Indicates if text values which contain macro expressions should be resolved too, default value is false - only macro values are resolved</param>
        public string GetPropertyValue(FormFieldPropertyEnum property, IMacroResolver resolver, MacroSettings macroSettings = null, bool resolveAllMacros = false)
        {
            bool isMacro;
            string value = GetPropertyValue(property, out isMacro);

            if ((resolver != null) && (isMacro || (resolveAllMacros && MacroProcessor.ContainsMacro(value))))
            {
                value = resolver.ResolveMacros(value, macroSettings);
            }
            return value;
        }


        /// <summary>
        /// Sets property value.
        /// </summary>
        /// <param name="property">Property which value is set</param>
        /// <param name="value">Property value</param>
        /// <param name="isMacro">Indicates if value is macro. Default value is false.</param>
        public void SetPropertyValue(FormFieldPropertyEnum property, string value, bool isMacro = false)
        {
            switch (property)
            {
                case FormFieldPropertyEnum.DefaultValue:
                    {
                        if (!MacroProcessor.ContainsMacro(value))
                        {
                            // Convert culture sensitive values to a proper format
                            switch (DataType.ToLowerInvariant())
                            {
                                case FieldDataType.Double:
                                    value = FormHelper.GetDoubleValueInSystemCulture(value);
                                    break;

                                case FieldDataType.Decimal:
                                    value = FormHelper.GetDecimalValueInSystemCulture(value);
                                    break;

                                case FieldDataType.Date:
                                    // Don't convert special values
                                    if (!DateTimeHelper.IsNowOrToday(value))
                                    {
                                        value = FormHelper.GetDateValueInSystemCulture(value);
                                    }
                                    break;

                                case FieldDataType.DateTime:
                                    // Don't convert special values
                                    if (!DateTimeHelper.IsNowOrToday(value))
                                    {
                                        value = FormHelper.GetDateTimeValueInSystemCulture(value);
                                    }
                                    break;
                            }
                        }
                    }
                    break;
            }

            string key = property.ToStringRepresentation();

            Properties[key] = value;
            PropertiesMacroTable[key] = isMacro ? value : null;
        }


        /// <summary>
        /// Gets the maximum input length for a given field
        /// </summary>
        public int GetMaxInputLength()
        {
            var maxLength = 0;
            var fieldType = DataType;

            var dataType = DataTypeManager.GetDataType(TypeEnum.Field, fieldType);
            if (dataType != null)
            {
                if (DataTypeManager.IsInteger(TypeEnum.Field, fieldType))
                {
                    //Set the maximum length of a number
                    if (string.IsNullOrEmpty(MinValue) || string.IsNullOrEmpty(MaxValue))
                    {
                        if (dataType.MaxValueLength > 0)
                        {
                            maxLength = dataType.MaxValueLength;
                        }
                    }
                    else
                    {
                        // Set max length to the bigger one
                        maxLength = Math.Max(MaxValue.Length, MinValue.Length);
                    }
                }
                else if (dataType.VariableSize && DataTypeManager.IsString(TypeEnum.Field, fieldType))
                {
                    maxLength = Size;
                }
            }

            return maxLength;
        }


        /// <summary>
        /// Returns the XML node representing the FormFieldInfo object only with limited properties.
        /// XML node contains only: field name, field visibility
        /// </summary>
        public XmlNode GetVisibilityXml(XmlDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            // field node to add
            var newFieldNode = doc.CreateElement("field");

            var attributes = new Dictionary<string, string> { { "column", Name } };

            if (Visibility != FormFieldVisibilityTypeEnum.All)
            {
                attributes.Add("visibility", Visibility.ToStringRepresentation());
            }

            newFieldNode.AddAttributes(attributes);

            return newFieldNode;
        }


        /// <summary>
        /// Returns if resolving of macros in the field's default value is enabled as far as that setting is specified, otherwise given default value is returned.
        /// </summary>
        /// <param name="defaultValue">This value is returned if 'resolve default value' setting is not set</param>
        public bool GetResolveDefaultValue(bool defaultValue)
        {
            return mResolveDefaultValue ?? defaultValue;
        }


        /// <summary>
        /// Sets the ability to resolve macros in field's default value.
        /// </summary>
        /// <param name="value">Value to be set</param>
        public void SetResolveDefaultValue(bool value)
        {
            mResolveDefaultValue = value;
        }


        /// <summary>
        /// Registers the Columns of this object for resolving data macros.
        /// </summary>
        protected override void RegisterColumns()
        {
            base.RegisterColumns();

            RegisterColumn("Visible", x => x.Visible);
            RegisterColumn("Enabled", x => x.Enabled);
            RegisterColumn("HasDependingFields", x => x.HasDependingFields);
            RegisterColumn("DependsOnAnotherField", x => x.DependsOnAnotherField);
            RegisterColumn("PublicField", x => x.PublicField);
            RegisterColumn("DisplayInSimpleMode", x => x.DisplayInSimpleMode);
            RegisterColumn("FieldType", x => x.FieldType);

            RegisterColumn("TranslateField", x => x.TranslateField);
            RegisterColumn("SpellCheck", x => x.SpellCheck);
            RegisterColumn("AllowUserToChangeVisibility", x => x.AllowUserToChangeVisibility);
            RegisterColumn("Visibility", x => x.Visibility);
            RegisterColumn("VisibilityControl", x => x.VisibilityControl);
            RegisterColumn("DisplayIn", x => x.DisplayIn);
            RegisterColumn("MappedToField", x => x.MappedToField);
            RegisterColumn("Inheritable", x => x.Inheritable);
            RegisterColumn("MinValue", x => x.MinValue);
            RegisterColumn("MaxValue", x => x.MaxValue);
            RegisterColumn("MinDateTimeValue", x => x.MinDateTimeValue);
            RegisterColumn("MaxDateTimeValue", x => x.MaxDateTimeValue);
            RegisterColumn("MinStringLength", x => x.MinStringLength);
            RegisterColumn("MaxStringLength", x => x.MaxStringLength);
            RegisterColumn("RegularExpression", x => x.RegularExpression);
        }

        #endregion
    }
}