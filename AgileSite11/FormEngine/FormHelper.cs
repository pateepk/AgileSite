using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.FormEngine
{
    /// <summary>
    /// Class providing form helper methods.
    /// </summary>
    public static class FormHelper
    {
        #region "Constants"

        /// <summary>
        /// Constant used to determine form field info which doesn't contain before/after definition of system field.
        /// </summary>
        public const string CORE = "_core";


        /// <summary>
        /// Resolver name prefix for forms/classes - 'form.'
        /// </summary>
        public const string FORM_PREFIX = "form.";


        /// <summary>
        /// Resolver name prefix for other objects with form definition - 'formdefinition.'
        /// </summary>
        public const string FORMDEFINITION_PREFIX = "formdefinition.";


        // Name of the element where field settings are stored.
        internal const string FIELD_SETTIGNS_NODE = "settings";

        // Name of the element where field properties are stored.
        internal const string FIELD_PROPERTIES_NODE = "properties";

        // Name of the element where field validation rules are stored.
        internal const string FIELD_RULES_NODE = "rules";


        // Text representation of validation macro rules (used to ensure backward compatibility)
        internal const string MACRO_RULE_MIN_STRING_LENGTH = "{{%Rule(\"Value.Length >= {0}\", \"<rules><r pos=\\\"0\\\" par=\\\"\\\" op=\\\"and\\\" n=\\\"MinLength\\\" ><p n=\\\"minlength\\\"><t>{1}</t><v>{1}</v><r>false</r><d></d><vt>integer</vt></p></r></rules>\")%}}";
        internal const string MACRO_RULE_REGULAR_EXPRESSION = "{{%Rule(\"Value.Matches(\\\"{0}\\\")\", \"<rules><r pos=\\\"0\\\" par=\\\"\\\" op=\\\"and\\\" n=\\\"RegExp\\\" ><p n=\\\"regexp\\\"><t>{1}</t><v>{1}</v><r>false</r><d></d><vt>text</vt></p></r></rules>\")%}}";
        internal const string MACRO_RULE_MAX_STRING_LENGTH = "{{%Rule(\"Value.Length <= {0}\", \"<rules><r pos=\\\"0\\\" par=\\\"\\\" op=\\\"and\\\" n=\\\"MaxLength\\\" ><p n=\\\"maxlength\\\"><t>{1}</t><v>{1}</v><r>false</r><d></d><vt>integer</vt></p></r></rules>\")%}}";
        internal const string MACRO_RULE_MAX_VALUE = "{{%Rule(\"Value <= {0}\", \"<rules><r pos=\\\"0\\\" par=\\\"\\\" op=\\\"and\\\" n=\\\"MaxValue\\\" ><p n=\\\"maxvalue\\\"><t>{1}</t><v>{1}</v><r>false</r><d></d><vt>double</vt></p></r></rules>\")%}}";
        internal const string MACRO_RULE_MIN_VALUE = "{{%Rule(\"Value >= {0}\", \"<rules><r pos=\\\"0\\\" par=\\\"\\\" op=\\\"and\\\" n=\\\"MinValue\\\" ><p n=\\\"minvalue\\\"><t>{1}</t><v>{1}</v><r>false</r><d></d><vt>double</vt></p></r></rules>\")%}}";
        internal const string MACRO_RULE_DATE_FROM = "{{%Rule(\"ToDateTime(Value) >= ToDateTime(\\\"{0}\\\")\", \"<rules><r pos=\\\"0\\\" par=\\\"\\\" op=\\\"and\\\" n=\\\"DateFrom\\\" ><p n=\\\"date\\\"><t>{1}</t><v>{1}</v><r>false</r><d></d><vt>datetime</vt></p></r></rules>\")%}}";
        internal const string MACRO_RULE_DATE_TO = "{{%Rule(\"ToDateTime(Value) <= ToDateTime(\\\"{0}\\\")\", \"<rules><r pos=\\\"0\\\" par=\\\"\\\" op=\\\"and\\\" n=\\\"DateTo\\\" ><p n=\\\"date\\\"><t>{1}</t><v>{1}</v><r>false</r><d></d><vt>datetime</vt></p></r></rules>\")%}}";

        // XPath expression for case insensitive selection of child nodes by names
        private const string XPATH_SELECT_CHILD_CASEINSENSITIVE = "descendant::node()[translate(name(),'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') ='{0}']{1}";


        // Form control setting name
        internal const string CONTROL_NAME_SETTING_KEY = "controlname";

        // Name of the attribute with field GUID
        private const string FIELD_GUID_ATTRIBUTE = "guid";

        // Name of the attribute with field name
        private const string FIELD_NAME_ATTRIBUTE = "column";

        #endregion


        #region "Variables"

        // Cached FormInfo objects.
        private static readonly CMSStatic<ProviderDictionary<string, FormInfo>> mFormInfos = new CMSStatic<ProviderDictionary<string, FormInfo>>(() => new ProviderDictionary<string, FormInfo>("FormInfo", null, StringComparer.InvariantCultureIgnoreCase, true));

        // Caching for form control parameters.
        private static readonly CMSStatic<ProviderDictionary<string, FormInfo>> mFormControlParameterInfos = new CMSStatic<ProviderDictionary<string, FormInfo>>(() => new ProviderDictionary<string, FormInfo>("FormControlParamFormInfo", null, StringComparer.InvariantCultureIgnoreCase, true));

        // Regular expression to match if form definition is in the latest version.
        private static readonly CMSRegex mLatestVersionRegExp = new CMSRegex(String.Format(@"^<form\s*{0}=[""']{1}[""']\s*>", FormInfo.ATTRIBUTE_VERSION, FormInfoVersionCode.LATEST_VERSION), true);

        private static Regex mRuleNameRegExp;

        // Collection of data types which have defined default control. 
        private static readonly HashSet<string> mSupportedDataTypes = new HashSet<string>(){ FieldDataType.Text, FieldDataType.LongText, FieldDataType.Decimal, FieldDataType.Integer, FieldDataType.DateTime, FieldDataType.Boolean, FieldDataType.File, FieldDataType.Guid, FieldDataType.DocAttachments, FieldDataType.DocRelationships }; 

        #endregion


        #region "Properties"

        /// <summary>
        /// Cached web part FormInfo objects.
        /// </summary>
        private static ProviderDictionary<string, FormInfo> FormInfos
        {
            get
            {
                return mFormInfos;
            }
        }


        /// <summary>
        /// Caching for form control parameters.
        /// </summary>
        private static ProviderDictionary<string, FormInfo> FormControlParameterInfos
        {
            get
            {
                return mFormControlParameterInfos;
            }
        }


        /// <summary>
        /// Regular expression to match the name in form validation rule.
        /// </summary>
        private static Regex RuleNameRegExp
        {
            get
            {
                return mRuleNameRegExp ?? (mRuleNameRegExp = RegexHelper.GetRegex(@"Rule.*<r\s[^>]*n=\\""([a-zA-Z0-9]*)\\""\s*>", true));
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Converts string to FormFieldControlType.
        /// </summary>
        /// <param name="controlTypeString">String to convert</param>
        public static FormFieldControlTypeEnum GetFormFieldControlType(string controlTypeString)
        {
            FormFieldControlTypeEnum controlType;

            switch (controlTypeString.ToLowerInvariant())
            {
                case FormFieldControlTypeCode.CALENDAR:
                    controlType = FormFieldControlTypeEnum.CalendarControl;
                    break;

                case FormFieldControlTypeCode.CHECKBOX:
                    controlType = FormFieldControlTypeEnum.CheckBoxControl;
                    break;

                case FormFieldControlTypeCode.USERCONTROL:
                case FormFieldControlTypeCode.CUSTOMUSERCONTROL:
                    controlType = FormFieldControlTypeEnum.CustomUserControl;
                    break;

                case FormFieldControlTypeCode.DROPDOWNLIST:
                    controlType = FormFieldControlTypeEnum.DropDownListControl;
                    break;

                case FormFieldControlTypeCode.FILEPATH:
                    controlType = FormFieldControlTypeEnum.FileSelectionControl;

                    break;

                case FormFieldControlTypeCode.HTMLAREA:
                    controlType = FormFieldControlTypeEnum.HtmlAreaControl;
                    break;

                case FormFieldControlTypeCode.IMAGEPATH:
                    controlType = FormFieldControlTypeEnum.ImageSelectionControl;
                    break;

                case FormFieldControlTypeCode.MEDIAPATH:
                    controlType = FormFieldControlTypeEnum.MediaSelectionControl;
                    break;

                case FormFieldControlTypeCode.LABEL:
                    controlType = FormFieldControlTypeEnum.LabelControl;
                    break;

                case FormFieldControlTypeCode.MULTIPLECHOICE:
                    controlType = FormFieldControlTypeEnum.MultipleChoiceControl;
                    break;

                case FormFieldControlTypeCode.LISTBOX:
                    controlType = FormFieldControlTypeEnum.ListBoxControl;
                    break;

                case FormFieldControlTypeCode.RADIOBUTTONS:
                    controlType = FormFieldControlTypeEnum.RadioButtonsControl;
                    break;

                case FormFieldControlTypeCode.TEXTAREA:
                    controlType = FormFieldControlTypeEnum.TextAreaControl;
                    break;

                case FormFieldControlTypeCode.BBEDITOR:
                    controlType = FormFieldControlTypeEnum.BBEditorControl;
                    break;

                case FormFieldControlTypeCode.TEXTBOX:
                case FormFieldControlTypeCode.INTEGER_NUMBER_TEXTBOX:
                case FormFieldControlTypeCode.DECIMAL_NUMBER_TEXTBOX:
                case FormFieldControlTypeCode.LONG_NUMBER_TEXTBOX:
                    controlType = FormFieldControlTypeEnum.TextBoxControl;
                    break;

                case FormFieldControlTypeCode.UPLOAD:
                    controlType = FormFieldControlTypeEnum.UploadControl;
                    break;

                case FormFieldControlTypeCode.DIRECTUPLOAD:
                    controlType = FormFieldControlTypeEnum.DirectUploadControl;
                    break;

                case FormFieldControlTypeCode.DOCUMENT_ATTACHMENTS:
                    controlType = FormFieldControlTypeEnum.DocumentAttachmentsControl;
                    break;

                default:
                    controlType = FormFieldControlTypeEnum.Unknown;
                    break;
            }

            return controlType;
        }


        /// <summary>
        /// Converts FormFieldControlType to string.
        /// </summary>
        /// <param name="controlType">FormFieldControlType to convert</param>
        public static string GetFormFieldControlTypeString(FormFieldControlTypeEnum controlType)
        {
            return Enum.GetName(typeof(FormFieldControlTypeEnum), controlType);
        }


        /// <summary>
        /// Returns true if FormFieldInfo is of specified type.
        /// </summary>
        public static bool IsFieldOfType(FormFieldInfo ffi, FormFieldControlTypeEnum type)
        {
            if (ffi != null)
            {
                string controlName = ValidationHelper.GetString(ffi.Settings[CONTROL_NAME_SETTING_KEY], String.Empty).ToLowerInvariant();

                // Return true if field type is of given type or if field is custom control with settings of given type
                if ((ffi.FieldType == type) || ((ffi.FieldType == FormFieldControlTypeEnum.CustomUserControl) && (controlName == GetFormFieldControlTypeString(type).ToLowerInvariant())))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Returns true if field uses list control (DropDownList, RadioButtonsList, ...)
        /// </summary>
        public static bool HasListControl(FormFieldInfo fieldInfo)
        {
            return IsFieldOfType(fieldInfo, FormFieldControlTypeEnum.DropDownListControl)
                   || IsFieldOfType(fieldInfo, FormFieldControlTypeEnum.RadioButtonsControl)
                   || IsFieldOfType(fieldInfo, FormFieldControlTypeEnum.ListBoxControl)
                   || IsFieldOfType(fieldInfo, FormFieldControlTypeEnum.MultipleChoiceControl);
        }


        /// <summary>
        /// Returns if field's control is of file type.
        /// </summary>
        /// <param name="fieldInfo">Form field info with control type</param>
        public static bool HasFileUploadControl(FormFieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                return false;
            }

            // Compare the control against default upload control
            if (IsFieldOfType(fieldInfo, FormFieldControlTypeEnum.UploadControl))
            {
                return true;
            }

            // Get form user control
            string controlName = ValidationHelper.GetString(fieldInfo.Settings[CONTROL_NAME_SETTING_KEY], String.Empty);
            if (!string.IsNullOrEmpty(controlName))
            {
                FormUserControlInfo controlInfo = FormUserControlInfoProvider.GetFormUserControlInfo(controlName);
                if (controlInfo != null)
                {
                    var hasFileUpload = controlInfo.UserControlForFile;

                    // when default data type differs from file, file cannot be used
                    if (controlInfo.UserControlShowInBizForms && !String.Equals(controlInfo.UserControlDefaultDataType, FieldDataType.File, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return false;
                    }

                    return hasFileUpload;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns DataSet with FormUserControlInfo objects that supports specified data type.
        /// </summary>
        /// <param name="dataType">Supported data type</param>
        /// <param name="emptyRow">Indicates whether empty data row is added to the result</param>
        /// <param name="fieldEditorControls">Indicates which controls should be displayed when selecting custom user control from the list</param>
        /// <param name="simplifiedMode">Indicates whether it is a simplified mode</param>
        /// <param name="controlType">Form user control type</param>
        public static DataSet GetFormUserControlsForDataType(string dataType, bool emptyRow, FieldEditorControlsEnum fieldEditorControls, bool simplifiedMode, FormUserControlTypeEnum controlType = FormUserControlTypeEnum.Unspecified)
        {
            DataSet ds = new DataSet();

            if (fieldEditorControls != FieldEditorControlsEnum.None)
            {
                string where = GetWhereConditionForDataType(dataType, fieldEditorControls, simplifiedMode, controlType);

                ds = FormUserControlInfoProvider.GetFormUserControls().Columns("UserControlCodeName, UserControlDisplayName").Where(where).OrderBy("UserControlDisplayName");

                // Convert values to lower case strings
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    dr[0] = dr[0].ToString().ToLowerInvariant();
                }
            }

            // Add empty row if required
            if (emptyRow)
            {
                DataRow dr = ds.Tables[0].NewRow();
                // User control display name
                dr["UserControlDisplayName"] = ResHelper.GetString("General.selectnone");
                // User control file name
                dr["UserControlCodeName"] = string.Empty;
                // Insert empty row
                ds.Tables[0].Rows.InsertAt(dr, 0);
            }

            return ds;
        }


        /// <summary>
        /// Gets WHERE condition to select controls for given data type and control type (on-line forms, page types, etc).
        /// </summary>
        public static string GetWhereConditionForDataType(string dataType, FieldEditorControlsEnum fieldEditorControls, bool simplifiedMode, FormUserControlTypeEnum controlType)
        {
            if (fieldEditorControls == FieldEditorControlsEnum.None)
            {
                return SqlHelper.NO_DATA_WHERE;
            }

            string where = null;

            // Prepare WHERE condition for data type
            if (!simplifiedMode)
            {
                where = GetDataTypeWhereCondition(dataType);
            }

            // Prepare WHERE condition for control placement
            if (fieldEditorControls != FieldEditorControlsEnum.All)
            {
                where = SqlHelper.AddWhereCondition(where, GetControlTypeColumn(fieldEditorControls) + " = 1");
            }

            // Prepare WHERE condition for control type
            if (controlType != FormUserControlTypeEnum.Unspecified)
            {
                where = SqlHelper.AddWhereCondition(where, "UserControlType = " + (int)controlType);
            }

            return where;
        }


        /// <summary>
        /// Returns column name of FormUserControlInfo class for given data type.
        /// </summary>
        /// <param name="fieldType">Data type</param>
        public static string GetDataTypeColumn(string fieldType)
        {
            if (fieldType == FieldDataType.ALL)
            {
                return null;
            }

            var group = GetDataTypeGroup(fieldType);

            return GetDataTypeColumnForGroup(group);
        }


        /// <summary>
        /// Returns column name of FormUserControlInfo class for given data type group.
        /// </summary>
        /// <param name="group">Data type group</param>
        public static string GetDataTypeColumnForGroup(string group)
        {
            return "UserControlFor" + group;
        }


        /// <summary>
        /// Gets the group for the given field type
        /// </summary>
        /// <param name="fieldType">Field type</param>
        private static string GetDataTypeGroup(string fieldType)
        {
            // Get group from the data type 
            string group = DataTypeManager.GetDataType(TypeEnum.Field, fieldType)?.TypeGroup;

            // Fallback to text if group not found
            if (String.IsNullOrEmpty(group))
            {
                group = "Text";
            }

            return group;
        }


        /// <summary>
        /// Determines whether given control is valid for given controls enum.
        /// </summary>
        /// <param name="controlName">Control name</param>
        /// <param name="controlType">Control type</param>
        public static bool IsValidControl(String controlName, FieldEditorControlsEnum controlType)
        {
            var column = GetControlTypeColumn(controlType);
            if (String.IsNullOrEmpty(column))
            {
                // Control is valid
                return true;
            }

            var info = FormUserControlInfoProvider.GetFormUserControlInfo(controlName);

            return info?.GetBooleanValue(column, false) ?? false;
        }


        /// <summary>
        /// Returns name of  FormUserControlInfo column for specified control type location.
        /// </summary>
        /// <param name="controlType">FieldEditorControlsEnum specifies control location</param>
        private static string GetControlTypeColumn(FieldEditorControlsEnum controlType)
        {
            switch (controlType)
            {
                case FieldEditorControlsEnum.Bizforms:
                    return "UserControlShowInBizForms";

                case FieldEditorControlsEnum.DocumentTypes:
                    return "UserControlShowInDocumentTypes";

                case FieldEditorControlsEnum.SystemTables:
                    return "UserControlShowInSystemTables";

                case FieldEditorControlsEnum.Controls:
                    return "UserControlShowInWebParts";

                case FieldEditorControlsEnum.Reports:
                    return "UserControlShowInReports";

                case FieldEditorControlsEnum.CustomTables:
                    return "UserControlShowInCustomTables";

                case FieldEditorControlsEnum.Visibility:
                    return "UserControlForVisibility";
            }
            return null;
        }


        /// <summary>
        /// Returns dataset with control types and user controls.
        /// </summary>
        /// <param name="fieldType">Field type, can be empty for simplified mode</param>
        /// <param name="fieldEditorControls">Indicates which controls should be displayed when selecting custom user control from the list</param>      
        /// <param name="simplifiedMode">Indicates whether get controls for simplified mode(advanced mode is set by default)</param>        
        /// <param name="isPrimary">Indicates whether it is a 'primary key' field</param>
        /// <param name="type">FormUserControlTypeEnum type of control</param>
        public static DataSet GetFieldControlTypesWithUserControls(string fieldType, FieldEditorControlsEnum fieldEditorControls, bool simplifiedMode, bool isPrimary, FormUserControlTypeEnum type = FormUserControlTypeEnum.Unspecified)
        {
            DataSet dsResult;

            if (!isPrimary)
            {
                dsResult = GetFormUserControlsForDataType(fieldType, false, fieldEditorControls, simplifiedMode, type);
            }
            else
            {
                dsResult = FormUserControlInfoProvider.GetFormUserControls().Columns("UserControlCodeName, UserControlDisplayName").Where("UserControlCodeName LIKE 'LabelControl'").OrderBy("UserControlDisplayName");
                dsResult.Tables[0].Rows[0][0] = dsResult.Tables[0].Rows[0][0].ToString().ToLowerInvariant();
            }

            return dsResult;
        }


        /// <summary>
        /// Returns DataSet with available form control types for specified column data type and field editor controls.
        /// </summary>
        /// <param name="dataType">Supported data type</param>
        /// <param name="fieldEditorControls">Indicates which controls should be displayed when selecting custom user control from the list</param>
        /// <param name="isPrimary">Indicates if field is primary field</param>
        public static DataSet GetAvailableControlTypes(string dataType, FieldEditorControlsEnum fieldEditorControls, bool isPrimary)
        {
            // Get available form control types
            DataSet dsResult;
            if (!isPrimary)
            {
                dsResult = GetAvailableControlTypesValue(dataType, fieldEditorControls);
            }
            else
            {
                dsResult = FormUserControlInfoProvider.GetFormUserControls().Column("DISTINCT UserControlType AS 'value'").Where("UserControlCodeName LIKE 'LabelControl'").OrderBy("UserControlType");
                dsResult.Tables[0].Rows[0][0] = dsResult.Tables[0].Rows[0][0].ToString().ToLowerInvariant();
            }

            // Create new column for form control type names
            DataColumn typeName = new DataColumn("text");
            typeName.DataType = Type.GetType("System.String");
            dsResult.Tables[0].Columns.Add(typeName);

            // Fill form control type names
            foreach (DataRow dr in dsResult.Tables[0].Rows)
            {
                int type = ValidationHelper.GetInteger(dr["value"], -1);
                if (type == -1)
                {
                    dr.Delete();
                }
                else
                {
                    FormUserControlTypeEnum enumType = (FormUserControlTypeEnum)type;
                    dr["text"] = ResHelper.GetString("formusercontroltypeenum." + enumType);
                }
            }

            return dsResult;
        }


        /// <summary>
        /// Returns DataSet with available control types for specified conditions.
        /// </summary>
        /// <param name="dataType">Supported data type</param>
        /// <param name="fieldEditorControls">Indicates which controls should be displayed when selecting custom user control from the list</param>
        private static DataSet GetAvailableControlTypesValue(string dataType, FieldEditorControlsEnum fieldEditorControls)
        {
            DataSet ds = new DataSet();

            if (fieldEditorControls != FieldEditorControlsEnum.None)
            {
                string where = GetDataTypeWhereCondition(dataType);

                // Prepare WHERE condition for control placement
                if (fieldEditorControls != FieldEditorControlsEnum.All)
                {
                    where = SqlHelper.AddWhereCondition(where, GetControlTypeColumn(fieldEditorControls) + " = 1");
                }

                ds = FormUserControlInfoProvider.GetFormUserControls().Column("DISTINCT UserControlType AS 'value'").Where(where).OrderBy("UserControlType");

                // Get integers indicating what type is allowed for given control
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    dr[0] = ValidationHelper.GetInteger(dr[0], -1);
                }
            }

            return ds;
        }


        /// <summary>
        /// Gets the where condition for Form controls for the given data type
        /// </summary>
        /// <param name="dataType">Data type</param>
        public static string GetDataTypeWhereCondition(string dataType)
        {
            string where = null;

            // Get the column
            var dataTypeColumn = GetDataTypeColumn(dataType);
            if (!String.IsNullOrEmpty(dataTypeColumn))
            {
                where = dataTypeColumn + " = 1";
            }

            return where;
        }


        /// <summary>
        /// Returns basic form definition of the table with primary key only.
        /// </summary>
        /// <param name="primaryKey">Primary key name</param>         
        public static string GetBasicFormDefinition(string primaryKey)
        {
            // Create empty form definition 
            FormInfo fi = new FormInfo();
            FormFieldInfo ffiPrimaryKey = new FormFieldInfo();

            // Fill FormInfo object
            ffiPrimaryKey.Name = primaryKey;
            ffiPrimaryKey.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, primaryKey);
            ffiPrimaryKey.DataType = FieldDataType.Integer;
            ffiPrimaryKey.FieldType = FormFieldControlTypeEnum.CustomUserControl;
            ffiPrimaryKey.Settings[CONTROL_NAME_SETTING_KEY] = GetFormFieldControlTypeString(FormFieldControlTypeEnum.LabelControl).ToLowerInvariant();
            ffiPrimaryKey.PrimaryKey = true;
            ffiPrimaryKey.System = false;
            ffiPrimaryKey.Visible = false;
            ffiPrimaryKey.Size = 0;
            ffiPrimaryKey.AllowEmpty = false;

            // Add field to form definition
            fi.AddFormItem(ffiPrimaryKey);

            return fi.GetXmlDefinition();
        }


        /// <summary>
        /// Loads the default class values to the Data container.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="container">Data container</param>
        /// <param name="resolveType">Type of macro resolving</param>
        public static void LoadDefaultValues(string className, IDataContainer container, FormResolveTypeEnum resolveType = FormResolveTypeEnum.None)
        {
            // Get the form info object
            FormInfo fi = GetFormInfo(className, true);
            fi?.LoadDefaultValues(container, resolveType);
        }


        /// <summary>
        /// Ensures the default class values to the Data container for required fields (not allowing empty value) without a value.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="container">Data container</param>
        /// <param name="resolveType">Type of macro resolving</param>
        /// <returns>Returns TRUE if at least one default value was ensured.</returns>
        public static bool EnsureDefaultValues(string className, IDataContainer container, FormResolveTypeEnum resolveType = FormResolveTypeEnum.None)
        {
            return GetFormInfo(className, true)?
                .EnsureDefaultValues(container, resolveType)
                ?? false;
        }


        /// <summary>
        /// Returns form field data type from input table column data.
        /// </summary>
        /// <param name="type">Column data type</param>
        /// <param name="maxLength">Maximum length of the column value</param>
        public static string GetFormFieldDataType(Type type, int maxLength)
        {
            // Special case for string
            if (type == typeof(string))
            {
                return maxLength < 1073741823 ? FieldDataType.Text : FieldDataType.LongText;
            }

            // Get type based on system type
            var dataType = DataTypeManager.GetDataType(type);
            if (dataType != null)
            {
                return dataType.FieldType;
            }

            // Default to text
            return FieldDataType.Text;
        }


        /// <summary>
        /// Returns default form field control type according to the form field data type.
        /// </summary>
        /// <param name="fieldType">Form field data type</param>
        /// <param name="isPrimary">Indicates whether it is a primary field</param>
        public static FormFieldControlTypeEnum GetFormFieldDefaultControlType(string fieldType, bool isPrimary)
        {
            if (isPrimary)
            {
                return FormFieldControlTypeEnum.LabelControl;
            }

            switch (fieldType)
            {
                case FieldDataType.DateTime:
                    return FormFieldControlTypeEnum.CalendarControl;

                case FieldDataType.Boolean:
                    return FormFieldControlTypeEnum.CheckBoxControl;

                case FieldDataType.LongText:
                    return FormFieldControlTypeEnum.TextAreaControl;

                case FieldDataType.File:
                    return FormFieldControlTypeEnum.UploadControl;

                case FieldDataType.Guid:
                    return FormFieldControlTypeEnum.LabelControl;

                case FieldDataType.DocAttachments:
                    return FormFieldControlTypeEnum.DocumentAttachmentsControl;

                default:
                    return FormFieldControlTypeEnum.TextBoxControl;
            }
        }


        /// <summary>
        /// Returns default form field control type according to the form field data type.
        /// If the control type of the configured control is not allowed, returns null.
        /// </summary>
        /// <param name="fieldType">Form field data type</param>
        /// <param name="controlType">Control type</param>
        public static string GetFormFieldDefaultControlType(string fieldType, FieldEditorControlsEnum controlType = FieldEditorControlsEnum.All)
        {
            string dataTypeGroup = GetDataTypeGroup(fieldType);

            // Check if fieldType has defined default control
            if (mSupportedDataTypes.Contains(dataTypeGroup, StringComparer.OrdinalIgnoreCase))
            {
                var controlName = SettingsKeyInfoProvider.GetValue($"CMSDefaultControlFor{dataTypeGroup}");

                // Check if the control is allowed
                if (IsValidControl(controlName, controlType))
                {
                    return controlName;
                }
            }
            return null;
        }


        /// <summary>
        /// Merges original FormInfo and alternative FormInfos and returns the resulting FormInfo.
        /// </summary>
        /// <param name="original">Default/original FormInfo</param>
        /// <param name="visibility">Visibility FormInfo</param>
        /// <param name="includeAllAltFields">If true, result will contain event those fields which are not in the original FormInfo</param>
        public static FormInfo MergeVisibility(FormInfo original, FormInfo visibility, bool includeAllAltFields)
        {
            if (visibility == null)
            {
                // Return original form definition
                return original;
            }

            // Iterate through field nodes in alternative form definition
            foreach (IDataDefinitionItem item in visibility.GetFormElements(true, true))
            {
                // Process the category element
                var altFci = item as FormCategoryInfo;
                if (altFci != null)
                {
                    var origFci = original.GetFormCategory(altFci.CategoryName);

                    // Merge categories
                    if (origFci != null)
                    {
                        original.UpdateFormCategory(origFci.CategoryName, altFci);
                    }
                    // Append alternative category
                    else if (includeAllAltFields)
                    {
                        original.AddFormCategory(altFci);
                    }
                }
                // Process the field element
                else
                {
                    var altFfi = (FormFieldInfo)item;
                    var origFfi = original.GetFormField(altFfi.Name);

                    // Merges fields
                    if (origFfi != null)
                    {
                        origFfi.Name = altFfi.Name;
                        origFfi.Visibility = altFfi.Visibility;

                        // Merge settings values
                        if ((altFfi.Settings != null) && (altFfi.Settings.Count > 0))
                        {
                            origFfi.Settings = GetSettingsDifferences(origFfi.Settings, altFfi.Settings, false);
                        }
                    }
                    // Appends alternative field
                    else if (includeAllAltFields)
                    {
                        original.AddFormItem(altFfi);
                    }
                }
            }
            return original;
        }


        /// <summary>
        /// Merges original and alternative form definitions and returns the result.
        /// </summary>
        /// <param name="original">Default/original form definition</param>
        /// <param name="alternative">Alternative form definition (just differences from original)</param>
        /// <param name="includeAllAltFields">If true, result will contain even those fields which are not in the original definition (optional, default value is true)</param>
        public static string MergeFormDefinitions(string original, string alternative, bool includeAllAltFields = true)
        {
            if (string.IsNullOrEmpty(alternative))
            {
                // Return original form definition
                return original;
            }

            // Parse XML for both form definitions
            XmlDocument xmlOrigDoc = new XmlDocument();
            xmlOrigDoc.LoadXml(EnsureFormDefinitionFormat(original));
            XmlDocument xmlAltDoc = new XmlDocument();
            xmlAltDoc.LoadXml(EnsureAlternativeFormDefinitionFormat(alternative));

            var origDocElement = xmlOrigDoc.DocumentElement;

            // Iterate through field nodes in alternative form definition
            if (xmlAltDoc.DocumentElement != null)
            {
                int lastKnownIndex = -1;

                foreach (XmlNode altField in xmlAltDoc.DocumentElement.ChildNodes)
                {
                    var isField = true;
                    XmlNode orgField = null;

                    // Process the field element
                    if (altField.LocalName.Equals("field", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (altField.Attributes == null)
                        {
                            continue;
                        }

                        // Get field with the same GUID from original definition
                        var guidAttr = altField.Attributes[FIELD_GUID_ATTRIBUTE];
                        if (guidAttr != null)
                        {
                            orgField = TableManager.SelectFieldNode(origDocElement, FIELD_GUID_ATTRIBUTE, guidAttr.Value);
                        }

                        // If the field has no GUID, or GUID values does not match try to find a field with the same name
                        if (orgField == null)
                        {
                            var columnAttr = altField.Attributes[FIELD_NAME_ATTRIBUTE];
                            if (columnAttr != null)
                            {
                                orgField = TableManager.SelectFieldNode(origDocElement, FIELD_NAME_ATTRIBUTE, columnAttr.Value);
                            }
                        }
                    }
                    // Process the category element
                    else if (altField.LocalName.Equals("category", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isField = false;

                        if (altField.Attributes == null)
                        {
                            continue;
                        }

                        var nameAttr = altField.Attributes["name"];
                        if (nameAttr != null)
                        {
                            // Get category with the same name from original definition
                            orgField = origDocElement.SelectSingleNode("category[@name='" + nameAttr.Value + "']");
                        }
                    }

                    XmlNode orderElem;
                    int index;
                    if (orgField != null)
                    {
                        string orgFieldName = String.Empty;
                        string orgGuidValue = string.Empty;
                        if (isField)
                        {
                            // Preserve the original field name (alternative form definition must not change it)
                            orgFieldName = orgField.Attributes[FIELD_NAME_ATTRIBUTE].Value;
                            orgGuidValue = orgField.Attributes[FIELD_GUID_ATTRIBUTE].Value;
                        }

                        // Merge element attributes
                        XmlHelper.SetXmlNodeAttributes(orgField, XmlHelper.GetXmlAttributes(altField.Attributes));

                        if (isField)
                        {
                            // Restore the original field name
                            orgField.Attributes[FIELD_NAME_ATTRIBUTE].Value = orgFieldName;
                            orgField.Attributes[FIELD_GUID_ATTRIBUTE].Value = orgGuidValue;
                        }

                        // Check for properties node
                        XmlNode orgProperties = orgField.SelectSingleNode(FIELD_PROPERTIES_NODE);
                        XmlNode altProperties = altField.SelectSingleNode(FIELD_PROPERTIES_NODE);

                        string items;
                        if (altProperties != null)
                        {
                            // Get properties differences
                            items = GetNodesDifferences(orgProperties, altProperties, FIELD_PROPERTIES_NODE, true);
                            if (!string.IsNullOrEmpty(items))
                            {
                                if (orgProperties != null)
                                {
                                    orgProperties.InnerXml = items;
                                }
                                else
                                {
                                    // Create properties help node
                                    var properties = xmlOrigDoc.CreateElement(FIELD_PROPERTIES_NODE);
                                    properties.InnerXml = items;

                                    // Append whole properties
                                    orgField.AppendChild(properties);
                                }
                            }
                        }

                        XmlNode orgSettings = orgField.SelectSingleNode(FIELD_SETTIGNS_NODE);
                        XmlNode altSettings = altField.SelectSingleNode(FIELD_SETTIGNS_NODE);

                        // Check for settings node
                        if (altSettings != null)
                        {
                            // Get settings differences
                            items = GetNodesDifferences(orgSettings, altSettings, FIELD_SETTIGNS_NODE, true);
                            if (!string.IsNullOrEmpty(items))
                            {
                                if (orgSettings != null)
                                {
                                    orgSettings.InnerXml = items;
                                }
                                else
                                {
                                    // Create settings help node
                                    var settings = xmlOrigDoc.CreateElement(FIELD_SETTIGNS_NODE);
                                    settings.InnerXml = items;

                                    // Append whole settings
                                    orgField.AppendChild(settings);
                                }
                            }
                        }

                        XmlNode orgRules = orgField.SelectSingleNode(FIELD_RULES_NODE);
                        XmlNode altRules = altField.SelectSingleNode(FIELD_RULES_NODE);

                        if (altRules != null)
                        {
                            if (orgRules != null)
                            {
                                orgField.RemoveChild(orgRules);
                            }

                            orgField.AppendChild(xmlOrigDoc.ImportNode(altRules, true));
                        }

                        // Change element order if it was changed in alternative form definition
                        index = GetElementOrderIndex(altField, ref lastKnownIndex);
                        if (index >= 0)
                        {
                            // Get child node from position specified by the index
                            orderElem = origDocElement.ChildNodes[index];

                            // Swap child nodes
                            if ((orderElem != null) && (orgField != orderElem))
                            {
                                XmlNode afterOrigElem = orgField.NextSibling;
                                origDocElement.RemoveChild(orgField);
                                origDocElement.InsertBefore(orgField, orderElem);
                                origDocElement.RemoveChild(orderElem);
                                if ((afterOrigElem != null) && (afterOrigElem != orderElem))
                                {
                                    origDocElement.InsertBefore(orderElem, afterOrigElem);
                                }
                                else
                                {
                                    origDocElement.AppendChild(orderElem);
                                }
                            }
                        }
                    }
                    else if (includeAllAltFields)
                    {
                        // Add entire alternative element which is not present in the original definition
                        XmlNode newNode = xmlOrigDoc.ImportNode(altField, true);
                        if (origDocElement != null)
                        {
                            // Get element order if it was set
                            index = GetElementOrderIndex(altField, ref lastKnownIndex);
                            if ((index < 0) || (index >= origDocElement.ChildNodes.Count))
                            {
                                // Add element to the end
                                origDocElement.AppendChild(newNode);
                            }
                            else
                            {
                                // Get child node from position specified by the index
                                orderElem = origDocElement.ChildNodes[index];
                                if (orderElem != null)
                                {
                                    origDocElement.InsertBefore(newNode, orderElem);
                                }
                            }
                        }
                    }
                }
            }

            return origDocElement != null ? origDocElement.OuterXml : null;
        }


        /// <summary>
        /// Returns value of element's "order" attribute if specified, -1 if not specified and previous siblings don't specify the "order" attribute
        /// or index calculated based on a previous sibling specified index.
        /// </summary>
        /// <param name="element">Xml element</param>
        /// <param name="lastKnownIndex">Holds value of the last element index</param>
        private static int GetElementOrderIndex(XmlNode element, ref int lastKnownIndex)
        {
            if (element == null)
            {
                return -1;
            }

            int index = ValidationHelper.GetInteger(XmlHelper.GetAttributeValue(element, "order"), -1);
            if (index >= 0)
            {
                lastKnownIndex = index;
            }

            if (lastKnownIndex >= 0 && index < 0)
            {
                // Previous siblings have "order" attribute set - try to get next index
                index = ++lastKnownIndex;
            }

            return index;
        }


        /// <summary>
        /// Combines two form definitions together. Fields from second definition are added at the end of first definition.
        /// </summary>
        /// <param name="def1">First form definition</param>
        /// <param name="def2">Second form definition</param>
        public static string CombineFormDefinitions(string def1, string def2)
        {
            if (string.IsNullOrEmpty(def2))
            {
                // Return original form definition
                return def1;
            }

            // Ensure valid XML - create empty XML if necessary
            if (string.IsNullOrEmpty(def1))
            {
                def1 = "<form></form>";
            }

            // Parse XML for both form definitions
            XmlDocument xmlOrigDoc = new XmlDocument();
            xmlOrigDoc.LoadXml(def1);
            XmlDocument xmlAltDoc = new XmlDocument();
            xmlAltDoc.LoadXml(def2);

            // Iterate through field nodes in alternative form definition
            if ((xmlOrigDoc.DocumentElement != null) && (xmlAltDoc.DocumentElement != null))
            {
                foreach (XmlNode altField in xmlAltDoc.DocumentElement.ChildNodes)
                {
                    xmlOrigDoc.DocumentElement.AppendChild(xmlOrigDoc.ImportNode(altField, true));
                }
            }

            return xmlOrigDoc.DocumentElement?.OuterXml;
        }


        /// <summary>
        /// Returns "alternative form fields definition" containing only specified attribute and value for all fields
        /// E.g. Set visible false for all fields in alt. form.
        /// </summary>
        /// <param name="sourceXml">Default/original form definition</param>
        /// <param name="attributeName">Attribute which should be set to default</param>
        /// <param name="value">Default value</param>        
        public static string GetFormFieldsWithDefaultValue(string sourceXml, string attributeName, string value)
        {
            // Parse XML for both form definitions
            XmlDocument xmlOrigDoc = new XmlDocument();
            xmlOrigDoc.LoadXml(sourceXml);

            // Create output (difference) form definition
            XmlDocument xmlOutDoc = new XmlDocument();
            var outputNode = xmlOutDoc.CreateElement("form");
            xmlOutDoc.AppendChild(outputNode);

            foreach (XmlNode field in xmlOrigDoc.DocumentElement.ChildNodes)
            {
                if ((field.Attributes[FIELD_NAME_ATTRIBUTE] != null) && (field.Attributes[attributeName] != null))
                {
                    string fieldName = field.Attributes[FIELD_NAME_ATTRIBUTE].Value;

                    // Create field node
                    var newField = xmlOutDoc.CreateElement("field");
                    outputNode.AppendChild(newField);

                    // Append 'column' attribute to 'field' node
                    newField.SetAttribute(FIELD_NAME_ATTRIBUTE, fieldName);

                    // Append attribute with default value
                    XmlHelper.SetXmlNodeAttributes(newField, new[,] { { attributeName, value } });
                }
            }

            // Return non-empty 'form' element or null
            if ((xmlOutDoc.DocumentElement != null) && (xmlOutDoc.DocumentElement.InnerXml != string.Empty))
            {
                return xmlOutDoc.InnerXml;
            }

            return null;
        }


        /// <summary>
        /// Returns XML differences between original and alternative form definitions
        /// (i.e. returns only fields that differs from each other)        
        /// </summary>
        /// <param name="original">Default/original form definition</param>
        /// <param name="alternative">Alternative form definition</param>
        /// <param name="includeAllAltFields">If true, result will contain event those fields which are not in the original definition</param>
        public static string GetFormDefinitionDifference(string original, string alternative, bool includeAllAltFields = false)
        {
            // Parse XML for both form definitions
            XmlDocument xmlOrigDoc = new XmlDocument();
            xmlOrigDoc.LoadXml(EnsureFormDefinitionFormat(original));
            XmlDocument xmlAltDoc = new XmlDocument();
            xmlAltDoc.LoadXml(EnsureFormDefinitionFormat(alternative));

            // Create output (difference) form definition
            XmlDocument xmlOutDoc = new XmlDocument();
            var outputNode = xmlOutDoc.CreateElement("form");
            AddLatestVersionAttribute(outputNode);
            xmlOutDoc.AppendChild(outputNode);

            string elemName = null;
            int index = 0;

            // Iterate through field nodes in alternative form definition
            if (xmlAltDoc.DocumentElement != null)
            {
                foreach (XmlNode altField in xmlAltDoc.DocumentElement.ChildNodes)
                {
                    XmlNode orgField = null;
                    XmlNode orderElem = null;
                    XmlNode newField = null;
                    bool orderChanged = false;

                    // Get element with the same index as the element from alt.form to check order change
                    if (xmlOrigDoc.DocumentElement.ChildNodes.Count > index)
                    {
                        orderElem = xmlOrigDoc.DocumentElement.ChildNodes[index];
                    }

                    string items;
                    string[,] diffAttributes;
                    // Process the field element
                    if (altField.LocalName.Equals("field", StringComparison.InvariantCultureIgnoreCase) && (altField.Attributes[FIELD_NAME_ATTRIBUTE] != null))
                    {
                        elemName = altField.Attributes[FIELD_NAME_ATTRIBUTE].Value;

                        // Compare fields with the same column names
                        orgField = TableManager.SelectFieldNode(xmlOrigDoc.DocumentElement, FIELD_NAME_ATTRIBUTE, elemName);
                        if (orgField != null)
                        {
                            // Check if field order has been changed
                            orderChanged = (orderElem == null) || (orderElem.Attributes[FIELD_NAME_ATTRIBUTE] == null) || (!elemName.Equals(orderElem.Attributes[FIELD_NAME_ATTRIBUTE].Value, StringComparison.InvariantCultureIgnoreCase));

                            // Get all attributes that should be included in form definitions difference
                            diffAttributes = GetDifferentAttributes(orgField.Attributes, altField.Attributes);
                            if (diffAttributes != null)
                            {
                                // Create new 'field' node and append it to base node ('form')
                                newField = EnsureNewField(xmlOutDoc, outputNode, elemName);

                                // Append other attributes that differ from original ones
                                XmlHelper.SetXmlNodeAttributes(newField, diffAttributes);
                            }

                            XmlNode orgSettings = orgField.SelectSingleNode(FIELD_SETTIGNS_NODE);
                            XmlNode altSettings = altField.SelectSingleNode(FIELD_SETTIGNS_NODE);

                            // Check for settings node
                            if ((orgSettings != null) || (altSettings != null))
                            {
                                // Get settings differences
                                items = GetNodesDifferences(orgSettings, altSettings, FIELD_SETTIGNS_NODE, false);
                                if (items != null)
                                {
                                    if (newField == null)
                                    {
                                        // Create new 'field' node and append it to base node ('form')
                                        newField = EnsureNewField(xmlOutDoc, outputNode, elemName);
                                    }

                                    var settings = xmlOutDoc.CreateElement(FIELD_SETTIGNS_NODE);
                                    settings.InnerXml = items;

                                    // Append settings
                                    newField.AppendChild(settings);
                                }
                            }

                            if (orderChanged && (newField == null))
                            {
                                // Ensure moved field
                                newField = EnsureNewField(xmlOutDoc, outputNode, elemName);
                            }
                        }
                        else if (includeAllAltFields)
                        {
                            // Copy entire alt.form field to differences if allowed
                            newField = xmlOutDoc.ImportNode(altField, true);
                            outputNode.AppendChild(newField);

                            orderChanged = true;
                        }
                    }
                    // Process the category element
                    else if (altField.LocalName.Equals("category", StringComparison.InvariantCultureIgnoreCase) && (altField.Attributes["name"] != null))
                    {
                        elemName = altField.Attributes["name"].Value;

                        // Compare categories with the same column names
                        orgField = xmlOrigDoc.DocumentElement.SelectSingleNode("category[@name='" + elemName + "']");
                        if (orgField != null)
                        {
                            // Check if field order has been changed
                            orderChanged = (orderElem == null) || (orderElem.Attributes["name"] == null) || (!elemName.Equals(orderElem.Attributes["name"].Value, StringComparison.InvariantCultureIgnoreCase));

                            // Get all attributes that should be included in form definitions difference
                            diffAttributes = GetDifferentAttributes(orgField.Attributes, altField.Attributes);
                            if (diffAttributes != null)
                            {
                                // Create new 'category' node and append it to base node ('form')
                                newField = EnsureNewCategory(xmlOutDoc, outputNode, elemName);

                                // Append other attributes that differ from original ones
                                XmlHelper.SetXmlNodeAttributes(newField, diffAttributes);
                            }

                            if (orderChanged && (newField == null))
                            {
                                // Ensure moved category
                                newField = EnsureNewCategory(xmlOutDoc, outputNode, elemName);
                            }
                        }
                        else if (includeAllAltFields)
                        {
                            // Copy entire alt.form field to differences if allowed
                            newField = xmlOutDoc.ImportNode(altField, true);
                            outputNode.AppendChild(newField);

                            orderChanged = true;
                        }
                    }

                    if (orgField != null)
                    {
                        XmlNode orgProperties = orgField.SelectSingleNode(FIELD_PROPERTIES_NODE);
                        XmlNode altProperties = altField.SelectSingleNode(FIELD_PROPERTIES_NODE);

                        // Check for properties node
                        if ((orgProperties != null) || (altProperties != null))
                        {
                            // Get properties differences
                            items = GetNodesDifferences(orgProperties, altProperties, FIELD_PROPERTIES_NODE, false);
                            if (items != null)
                            {
                                if (newField == null)
                                {
                                    // Create new node and append it to base node ('form')
                                    if (altField.LocalName.Equals("field", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        newField = EnsureNewField(xmlOutDoc, outputNode, elemName);
                                    }
                                    else
                                    {
                                        newField = EnsureNewCategory(xmlOutDoc, outputNode, elemName);
                                    }
                                }

                                var properties = xmlOutDoc.CreateElement(FIELD_PROPERTIES_NODE);
                                properties.InnerXml = items;

                                // Append properties
                                newField.AppendChild(properties);
                            }
                        }
                    }

                    if (altField.LocalName.Equals("field", StringComparison.InvariantCultureIgnoreCase) && (orgField != null))
                    {
                        XmlNode orgRules = orgField.SelectSingleNode(FIELD_RULES_NODE);
                        XmlNode altRules = altField.SelectSingleNode(FIELD_RULES_NODE);

                        // Get the rules difference
                        if ((altRules != null) && ((orgRules == null) || (altRules.OuterXml != orgRules.OuterXml)))
                        {
                            if (newField == null)
                            {
                                newField = EnsureNewField(xmlOutDoc, outputNode, elemName);
                            }

                            newField.AppendChild(xmlOutDoc.ImportNode(altRules, true));
                        }
                        else if (orgRules != null && altRules == null)
                        {
                            if (newField == null)
                            {
                                newField = EnsureNewField(xmlOutDoc, outputNode, elemName);
                            }

                            newField.AppendChild(newField.OwnerDocument.CreateElement(FIELD_RULES_NODE));
                        }
                    }

                    if (orderChanged && (newField != null))
                    {
                        // Append an attribute with new field position
                        XmlHelper.SetXmlNodeAttributes(newField, new[,] { { "order", index.ToString() } });
                    }

                    index++;
                }
            }

            // Return non-empty 'form' element or null
            if (xmlOutDoc.DocumentElement != null)
            {
                return xmlOutDoc.DocumentElement.InnerXml != string.Empty ? xmlOutDoc.InnerXml : null;
            }

            return null;
        }


        /// <summary>
        /// Prepares new XML node for field specified by caption and appends it to the parent XML node.
        /// </summary>
        /// <param name="doc">XML document</param>
        /// <param name="parent">Parent node</param>
        /// <param name="caption">Caption for new field</param>
        private static XmlNode EnsureNewField(XmlDocument doc, XmlNode parent, string caption)
        {
            return EnsureNewElement(doc, parent, "field", FIELD_NAME_ATTRIBUTE, caption);
        }


        /// <summary>
        /// Prepares new XML node for category specified by name and appends it to the parent XML node.
        /// </summary>
        /// <param name="doc">XML document</param>
        /// <param name="parent">Parent node</param>
        /// <param name="name">Name for new category</param>
        private static XmlNode EnsureNewCategory(XmlDocument doc, XmlNode parent, string name)
        {
            return EnsureNewElement(doc, parent, "category", "name", name);
        }


        private static XmlNode EnsureNewElement(XmlDocument doc, XmlNode parent, string elementName, string attributeName, string attributeValue)
        {
            // Create new XML element and append it to the parent node
            var element = doc.CreateElement(elementName);
            parent.AppendChild(element);

            // Append attribute to the new element
            element.SetAttribute(attributeName, attributeValue);

            return element;
        }


        /// <summary>
        /// Returns true if the given form definition uses latest version format.
        /// </summary>
        /// <param name="definition"></param>
        internal static bool IsLatestVersion(string definition)
        {
            return mLatestVersionRegExp.IsMatch(definition);
        }


        /// <summary>
        /// Returns minimal form definition (only root element) if the given form definition is empty.
        /// </summary>
        private static string EnsureFormDefinition(string definition)
        {
            if (String.IsNullOrEmpty(definition))
            {
                return FormInfo.GetEmptyFormDocument().InnerXml;
            }

            return definition;
        }


        /// <summary>
        /// Ensures form definition format for backward compatibility.
        /// </summary>
        /// <param name="definition">Form definition</param>
        internal static string EnsureFormDefinitionFormat(string definition)
        {
            definition = EnsureFormDefinition(definition);

            // No conversion needed if the definition is in latest format
            if (!IsLatestVersion(definition))
            {
                var formInfo = new FormInfo(definition);
                return formInfo.GetXmlDefinition();
            }

            return definition;
        }


        /// <summary>
        /// Ensures alternative form definition format for backward compatibility.
        /// </summary>
        /// <param name="definition">Form definition</param>
        internal static string EnsureAlternativeFormDefinitionFormat(string definition)
        {
            definition = EnsureFormDefinition(definition);

            // No conversion needed if the definition is in latest format
            if (IsLatestVersion(definition))
            {
                return definition;
            }

            // Parse XML of form definition
            XmlDocument xmlDoc = new XmlDocument();
            if (!String.IsNullOrEmpty(definition))
            {
                xmlDoc.LoadXml(definition);
            }

            // Create output form definition
            XmlDocument xmlOutDoc = new XmlDocument();
            var outputNode = xmlOutDoc.CreateElement("form");
            AddLatestVersionAttribute(outputNode);
            xmlOutDoc.AppendChild(outputNode);

            if (xmlDoc.DocumentElement != null)
            {
                foreach (XmlNode field in xmlDoc.DocumentElement.ChildNodes)
                {
                    string elemName;
                    XmlAttribute attribute;
                    XmlNode fieldProperty;
                    XmlNode newFieldProperties;
                    XmlNode fieldProperties;
                    XmlNode newElement;

                    // Process fields
                    if (field.LocalName.Equals("field", StringComparison.InvariantCultureIgnoreCase) && (field.Attributes[FIELD_NAME_ATTRIBUTE] != null))
                    {
                        elemName = field.Attributes[FIELD_NAME_ATTRIBUTE].Value;
                        newElement = EnsureNewField(xmlOutDoc, outputNode, elemName);

                        // Append properties from original field
                        fieldProperties = field.SelectSingleNode(FIELD_PROPERTIES_NODE);
                        if (fieldProperties != null)
                        {
                            newFieldProperties = xmlOutDoc.ImportNode(fieldProperties, true);
                            newElement.AppendChild(newFieldProperties);
                        }
                        else
                        {
                            newFieldProperties = null;
                        }

                        foreach (XmlAttribute origAttribute in field.Attributes)
                        {
                            // Attributes from FormFieldPropertyEnum are converted to properties
                            if (Enum.GetNames(typeof(FormFieldPropertyEnum)).Any(a => a.Equals(origAttribute.Name, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                // Create properties element if not exists
                                if (newFieldProperties == null)
                                {
                                    newFieldProperties = xmlOutDoc.CreateElement(FIELD_PROPERTIES_NODE);
                                    newElement.AppendChild(newFieldProperties);
                                }

                                // Add property
                                fieldProperty = xmlOutDoc.CreateElement(origAttribute.Name);
                                fieldProperty.InnerText = origAttribute.Value;
                                newFieldProperties.AppendChild(fieldProperty);
                            }
                            else
                            {
                                // Other attributes add as attributes
                                attribute = xmlOutDoc.CreateAttribute(origAttribute.Name);
                                attribute.Value = origAttribute.Value;
                                newElement.Attributes.Append(attribute);
                            }
                        }

                        // Append rules from original field
                        XmlNode fieldRules = field.SelectSingleNode(FIELD_RULES_NODE);
                        if (fieldRules != null)
                        {
                            XmlNode newFieldRules = xmlOutDoc.ImportNode(fieldRules, true);
                            newElement.AppendChild(newFieldRules);
                        }

                        ConvertValidationFromAttributesToRules(newElement);

                        // Append settings
                        XmlNode settings = field.SelectSingleNode(FIELD_SETTIGNS_NODE);
                        if (settings != null)
                        {
                            newElement.AppendChild(xmlOutDoc.ImportNode(settings, true));
                        }
                    }
                    // Process categories
                    else if (field.LocalName.Equals("category", StringComparison.InvariantCultureIgnoreCase) && (field.Attributes["name"] != null))
                    {
                        elemName = field.Attributes["name"].Value;
                        newElement = EnsureNewCategory(xmlOutDoc, outputNode, elemName);

                        // Append properties from original field
                        fieldProperties = field.SelectSingleNode(FIELD_PROPERTIES_NODE);
                        if (fieldProperties != null)
                        {
                            newFieldProperties = xmlOutDoc.ImportNode(fieldProperties, true);
                            newElement.AppendChild(newFieldProperties);
                        }
                        else
                        {
                            newFieldProperties = null;
                        }

                        foreach (XmlAttribute origAttribute in field.Attributes)
                        {
                            // Attributes from FormCategoryPropertyEnum are converted to properties
                            if (Enum.GetNames(typeof(FormCategoryPropertyEnum)).Any(a => a.Equals(origAttribute.Name, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                // Create properties element if not exists
                                if (newFieldProperties == null)
                                {
                                    newFieldProperties = xmlOutDoc.CreateElement(FIELD_PROPERTIES_NODE);
                                    newElement.AppendChild(newFieldProperties);
                                }

                                // Add property
                                fieldProperty = xmlOutDoc.CreateElement(origAttribute.Name);
                                fieldProperty.InnerText = origAttribute.Value;
                                newFieldProperties.AppendChild(fieldProperty);
                            }
                            else
                            {
                                // Other attributes add as attributes
                                attribute = xmlOutDoc.CreateAttribute(origAttribute.Name);
                                attribute.Value = origAttribute.Value;
                                newElement.Attributes.Append(attribute);
                            }
                        }
                    }
                }
            }
            return xmlOutDoc.InnerXml;
        }


        /// <summary>
        /// Adds version attribute with latest version as value to xmlNode.
        /// </summary>
        /// <param name="outputNode">Form XmlNode</param>
        public static void AddLatestVersionAttribute(XmlNode outputNode)
        {
            XmlAttribute newAttribute = outputNode.OwnerDocument.CreateAttribute(FormInfo.ATTRIBUTE_VERSION);
            newAttribute.Value = FormInfoVersionCode.LATEST_VERSION;
            outputNode.Attributes.Append(newAttribute);
        }


        /// <summary>
        /// Returns array with different attributes which are in both collections.
        /// Attribute with name <see cref="FIELD_GUID_ATTRIBUTE"/> is returned even if unchanged.
        /// </summary>
        /// <param name="origAttributes">Collection of original attributes</param>
        /// <param name="newAttributes">Collection of new attributes</param>
        /// <returns>2-dimensional array: 0-attribute name, 1-attribute value</returns>
        private static string[,] GetDifferentAttributes(XmlAttributeCollection origAttributes, XmlAttributeCollection newAttributes)
        {
            string[,] result = null;

            ArrayList attList = new ArrayList();
            if ((newAttributes != null) && (newAttributes.Count > 0))
            {
                // Compare new non-empty attributes with their originals
                foreach (XmlAttribute newAtt in newAttributes)
                {
                    XmlAttribute att = origAttributes[newAtt.LocalName];
                    // Store attributes which are only in new attributes
                    // or that ones which have different value or attribute name is FIELD_GUID_ATTRIBUTE
                    if ((att == null) || (att.Value != newAtt.Value) || att.LocalName.Equals(FIELD_GUID_ATTRIBUTE, StringComparison.Ordinal))
                    {
                        attList.Add(new[] { newAtt.LocalName, newAtt.Value });
                    }
                }
            }
            if ((origAttributes != null) && (origAttributes.Count > 0))
            {
                // Also include empty attributes which are non-empty in original
                // (FieldEditor doesn't return empty attributes)
                foreach (XmlAttribute origAtt in origAttributes)
                {
                    if (newAttributes != null)
                    {
                        if (newAttributes[origAtt.LocalName] == null)
                        {
                            attList.Add(new[] { origAtt.LocalName, string.Empty });
                        }
                    }
                }
            }

            if (attList.Count > 0)
            {
                // Transform ArrayList to 2-dimensional string array
                object[] array = attList.ToArray();
                int items = attList.Count;
                result = new string[items, 2];
                for (int i = 0; i < items; i++)
                {
                    result[i, 0] = ((string[])array[i])[0];
                    result[i, 1] = ((string[])array[i])[1];
                }
            }

            return result;
        }


        /// <summary>
        /// Returns original settings in hash table modified by new settings. If new settings doesn't differ then method returns original settings.
        /// </summary>
        /// <param name="origSettings">Original 'settings' node</param>
        /// <param name="newSettings">New 'settings' node</param>
        /// <param name="includeNonExisting">If true empty settings which are non-empty in original are included too</param>
        private static Hashtable GetSettingsDifferences(Hashtable origSettings, Hashtable newSettings, bool includeNonExisting)
        {
            var result = origSettings ?? new Hashtable(StringComparer.InvariantCultureIgnoreCase);

            // Loop through new settings 
            foreach (DictionaryEntry item in newSettings)
            {
                // Add setting which is not in original settings
                if (result[item.Key] == null)
                {
                    result.Add(item.Key, item.Value);
                }
                // Change setting which differs
                else if (origSettings[item.Key] != item.Value)
                {
                    result[item.Key] = item.Value;
                }
            }

            // Modify empty settings which are non-empty in original
            // (FieldEditor doesn't return empty settings)
            if (includeNonExisting)
            {
                foreach (DictionaryEntry item in result)
                {
                    if (!newSettings.ContainsKey(item.Key))
                    {
                        result[item.Key] = null;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Returns differences in XML of new properties or settings from original.
        /// </summary>
        /// <param name="origNode">Original node</param>
        /// <param name="newNode">New node</param>
        /// <param name="nodeName">Name of the node</param>
        /// <param name="includeOriginal">If true empty properties which are non-empty in original are included too</param>
        private static string GetNodesDifferences(XmlNode origNode, XmlNode newNode, string nodeName, bool includeOriginal)
        {
            string result = null;

            // Check if original node is desired node
            if ((origNode != null) && origNode.LocalName.Equals(nodeName, StringComparison.InvariantCultureIgnoreCase))
            {
                XmlDocument xml = new XmlDocument();

                // Create temporary node
                var temp = xml.CreateElement(nodeName);
                XmlNode newItem;
                if ((newNode != null) && (newNode.ChildNodes.Count > 0))
                {
                    // Compare new non-empty items with their originals
                    foreach (XmlNode item in newNode.ChildNodes)
                    {
                        // XPath for selection 
                        var xPath = String.Format(XPATH_SELECT_CHILD_CASEINSENSITIVE, item.LocalName.ToLowerInvariant(), String.Empty);

                        // Skip duplicates except the first occurrence (PLATFORM-5399)
                        var duplicates = newNode.SelectNodes(xPath);
                        if ((duplicates != null) && (duplicates.Count > 1) && (item != duplicates[0]))
                        {
                            continue;
                        }

                        // Try to get specified item from original
                        var origItem = origNode.SelectSingleNode(xPath + "[1]");

                        string[,] diffAttributes = null;
                        if (origItem != null)
                        {
                            diffAttributes = GetDifferentAttributes(origItem.Attributes, item.Attributes);
                        }

                        if ((origItem == null) || (origItem.InnerXml != item.InnerXml) || (diffAttributes != null) || includeOriginal)
                        {
                            // Add the item
                            newItem = xml.ImportNode(item, true);
                            temp.AppendChild(newItem);
                        }
                    }
                }

                // Also include items which are non-empty in original
                // (FieldEditor doesn't return empty properties)
                foreach (XmlNode item in origNode.ChildNodes)
                {
                    if (newNode?.SelectSingleNode(String.Format(XPATH_SELECT_CHILD_CASEINSENSITIVE, item.LocalName.ToLowerInvariant(), "[1]")) == null)
                    {
                        if (includeOriginal)
                        {
                            // Append item from original
                            newItem = xml.ImportNode(item, true);
                        }
                        else
                        {
                            // Create new properties item with empty value (it means the item was removed)
                            newItem = xml.CreateElement(item.LocalName);
                            newItem.InnerXml = string.Empty;
                        }
                        temp.AppendChild(newItem);
                    }
                }

                result = !string.IsNullOrEmpty(temp.InnerXml) ? temp.InnerXml : null;
            }
            else
            {
                if ((newNode != null) && newNode.LocalName.Equals(nodeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Return new node if original ones don't exist
                    result = newNode.InnerXml;
                }
            }

            return result;
        }


        /// <summary>
        /// Returns initialized FormFieldInfo structure according to the specified table column schema.
        /// </summary>
        /// <param name="dci">Data class</param>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Database table column name</param>
        public static FormFieldInfo GetFormFieldInfo(DataClassInfo dci, string tableName, string columnName)
        {
            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(columnName))
            {
                return null;
            }

            var connString = dci?.ClassConnectionString;
            var manager = new TableManager(connString);

            // Get table column information from database structure information
            var ds = manager.GetColumnInformation(tableName, columnName);
            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return null;
            }

            var list = manager.GetPrimaryKeyColumns(tableName);
            var isPrimary = list.Contains(columnName, StringComparer.InvariantCultureIgnoreCase);

            var field = new FormFieldInfo();
            field.LoadFromTableData(ds.Tables[0].Rows[0], isPrimary, true);

            // Set additional flags
            field.External = true;

            return field;
        }


        /// <summary>
        /// Returns FormInfo object for specified class name or alternative name. 
        /// Method loops through supplied class list and returns first object existing.
        /// If alternative name is specified then it is merged with default class and then returned
        /// If coupled class exists for given alternative form then it is merged as well.
        /// If alternative form info not found then its default class is returned.
        /// You can also merge two forms in the result by using the + operator between names, such as "customtable.sampletable.filter+customtable.sampletable.extra"
        /// </summary>
        /// <param name="classes">Class names or alternative names separated by semicolon</param>
        /// <param name="clone">When returned FormInfo is used only for reading then cloning can be set to FALSE</param>
        /// <param name="fallbackToDefault">If true and alternative form is not found, the process attempts to fall back to the class default form</param>
        /// <param name="onlyVisible">If true, only visible fields are provided in the result. This also removes any empty categories in the form (considering them invisible).</param>
        /// <returns>Null if no FormInfo found for any of the specified classes. Otherwise returns first FormInfo which was found.</returns>
        public static FormInfo GetFormInfo(string classes, bool clone, bool fallbackToDefault = true, bool onlyVisible = false)
        {
            if (!String.IsNullOrEmpty(classes))
            {
                string[] list = classes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                bool alternativeName = false;

                // Loop through all class names or alternative names
                foreach (string classname in list)
                {
                    // Combine several form definitions if required
                    var key = new StringList(classname.ToLowerInvariant(), onlyVisible);

                    if (classname.Contains("+"))
                    {
                        // If it is alternative form name then try to find it in cache
                        FormInfo result = FormInfos[key];
                        if (result != null)
                        {
                            return GetResult(result, clone);
                        }

                        result = new FormInfo();
                        bool found = false;

                        // Combine particular form names
                        string[] forms = classname.Split('+');
                        foreach (string form in forms)
                        {
                            var fi = GetFormInfo(form, false, false, onlyVisible);
                            if (fi != null)
                            {
                                found = true;
                                result.CombineWithForm(fi, new CombineWithFormSettings
                                {
                                    IncludeCategories = true,
                                    OverwriteHidden = true,
                                    PreserveCategory = true,
                                    RemoveEmptyCategories = onlyVisible
                                });
                            }
                        }

                        if (found)
                        {
                            FormInfos[key] = result;

                            return GetResult(result, clone);
                        }
                    }

                    // Check if provided name is alternative form name 
                    if (classname.IndexOf('.') != classname.LastIndexOf('.'))
                    {
                        alternativeName = true;

                        // If it is alternative form name then try to find it in cache
                        var afi = FormInfos[key];
                        if (afi != null)
                        {
                            return GetResult(afi, clone);
                        }

                        // If alternative form name not found in cache then try to find it in DB
                        var altInfo = AlternativeFormInfoProvider.GetAlternativeFormInfo(classname);
                        if (altInfo != null)
                        {
                            // Alternative form found, get its class
                            var ci = DataClassInfoProvider.GetDataClassInfo(altInfo.FormClassID);
                            if (ci != null)
                            {
                                string formDefinition = ci.ClassFormDefinition;

                                // Check if CoupledClass exists and merge it with class
                                if (altInfo.FormCoupledClassID > 0)
                                {
                                    // If coupled class is defined combine form definitions
                                    var coupledClass = DataClassInfoProvider.GetDataClassInfo(altInfo.FormCoupledClassID);
                                    if (coupledClass != null)
                                    {
                                        formDefinition = MergeFormDefinitions(formDefinition, coupledClass.ClassFormDefinition);
                                    }
                                }

                                // Get merged FormInfo definition and store it in cache
                                formDefinition = MergeFormDefinitions(formDefinition, altInfo.FormDefinition);
                                afi = new FormInfo(formDefinition);

                                if (onlyVisible)
                                {
                                    afi.RemoveFields(ffi => !ffi.Visible);
                                    afi.RemoveEmptyCategories();
                                }

                                FormInfos[key] = afi;

                                return GetResult(afi, clone);
                            }
                        }

                        // Do not attempt to get default form if fall-back is not allowed
                        if (!fallbackToDefault)
                        {
                            continue;
                        }
                    }

                    // If alternative FormInfo not found then get class name
                    string name = alternativeName ? classname.Substring(0, classname.LastIndexOf('.')) : classname;

                    // Try to find it in cache
                    var nameKey = new StringList(name.ToLowerInvariant(), onlyVisible);

                    var cfi = FormInfos[nameKey];
                    if (cfi != null)
                    {
                        return GetResult(cfi, clone);
                    }

                    // If not found in cache then try to find it in DB
                    var classInfo = DataClassInfoProvider.GetDataClassInfo(name);
                    if (classInfo != null)
                    {
                        // Class found, store it in cache
                        try
                        {
                            cfi = new FormInfo(classInfo.ClassFormDefinition);
                        }
                        catch
                        {
                            // No need to log event - it has been already logged
                            return null;
                        }

                        if (onlyVisible)
                        {
                            cfi.RemoveFields(ffi => !ffi.Visible);
                            cfi.RemoveEmptyCategories();
                        }

                        FormInfos[nameKey] = cfi;

                        return GetResult(cfi, clone);
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the resulting form info
        /// </summary>
        /// <param name="fi">Form info to return</param>
        /// <param name="clone">If true, the result is cloned</param>
        private static FormInfo GetResult(FormInfo fi, bool clone)
        {
            return clone ? fi.Clone() : fi;
        }


        /// <summary>
        /// Returns cached form control parameters.
        /// </summary>
        /// <param name="formControlName">Name of a form control</param>
        /// <param name="formControlParameters">XML with FormInfo definition for form control parameters. This parameter is optional and will be used only if cached FormInfo won't be found</param>
        /// <param name="clone">Indicates if cloned object should be returned or original one instead. Use cloning when FormInfo will be modified. Don't use cloning when FormInfo will be used only for reading data</param>
        public static FormInfo GetFormControlParameters(string formControlName, string formControlParameters, bool clone)
        {
            if (String.IsNullOrEmpty(formControlName))
            {
                return null;
            }

            // Check if cached FormInfo with control parameters exists
            FormInfo fcp = FormControlParameterInfos[formControlName.ToLowerInvariant()];

            if (fcp != null)
            {
                return GetResult(fcp, clone);
            }

            // FormInfo is not in cache = create new one
            fcp = new FormInfo(formControlParameters);

            // Store it in cache
            FormControlParameterInfos[formControlName] = fcp;

            return GetResult(fcp, clone);
        }


        /// <summary>
        /// Clears cached FormInfo objects.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        public static void ClearFormInfos(bool logTasks)
        {
            FormInfos?.Clear(logTasks);
        }


        /// <summary>
        /// Clears cached form control parameters.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        public static void ClearFormControlParameters(bool logTasks)
        {
            FormControlParameterInfos?.Clear(logTasks);
        }


        /// <summary>
        /// Clears all cached objects.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        internal static void Clear(bool logTasks)
        {
            ClearFormInfos(logTasks);
            ClearFormControlParameters(logTasks);
        }


        /// <summary>
        /// Changes field name in form layout definition.
        /// </summary>
        /// <param name="classId">Class ID (data class ID)</param>
        /// <param name="oldFieldName">Old field name</param>
        /// <param name="newFieldName">New field name</param>
        public static void RenameFieldInFormLayout(int classId, string oldFieldName, string newFieldName)
        {
            if (classId > 0)
            {
                // Rename fields in "default" layout for data class
                DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(classId);
                if (dci != null)
                {
                    string layout = dci.ClassFormLayout;
                    // Update matching layout only
                    if (RenameFieldInFormLayout(oldFieldName, newFieldName, ref layout, dci.ClassFormLayoutType))
                    {
                        dci.ClassFormLayout = layout;
                        DataClassInfoProvider.SetDataClassInfo(dci);
                    }
                }

                // Rename field in alternative forms (if any)
                DataSet objIds = AlternativeFormInfoProvider.GetAlternativeForms()
                                                .WhereEquals("FormClassID", classId)
                                                .WhereNotNull("FormLayout")
                                                .Column("FormID");

                if (!DataHelper.DataSourceIsEmpty(objIds))
                {
                    foreach (DataRow dr in objIds.Tables[0].Rows)
                    {
                        int altFormId = ValidationHelper.GetInteger(dr["FormID"], 0);
                        RenameFieldInAlternativeFormLayout(altFormId, oldFieldName, newFieldName);
                    }
                }
            }
        }


        /// <summary>
        /// Changes field name in alternative form layout definition.
        /// </summary>
        /// <param name="altFormId">Class ID (data class ID)</param>
        /// <param name="oldFieldName">Old field name</param>
        /// <param name="newFieldName">New field name</param>
        public static void RenameFieldInAlternativeFormLayout(int altFormId, string oldFieldName, string newFieldName)
        {
            if (altFormId > 0)
            {
                AlternativeFormInfo afi = AlternativeFormInfoProvider.GetAlternativeFormInfo(altFormId);
                if (afi != null)
                {
                    string layout = afi.FormLayout;
                    if (RenameFieldInFormLayout(oldFieldName, newFieldName, ref layout, afi.FormLayoutType))
                    {
                        afi.FormLayout = layout;
                        AlternativeFormInfoProvider.SetAlternativeFormInfo(afi);
                    }
                }
            }
        }


        /// <summary>
        /// Returns TRUE if successfully changes field name in form layout definition otherwise FALSE.
        /// </summary>
        /// <param name="oldFieldName">Old field name</param>
        /// <param name="newFieldName">New field name</param>
        /// <param name="layout">Form layout definition</param>
        /// <param name="layoutType">Form layout type</param>
        private static bool RenameFieldInFormLayout(string oldFieldName, string newFieldName, ref string layout, LayoutTypeEnum layoutType)
        {
            if (!String.IsNullOrEmpty(layout))
            {
                Regex re = GetLayoutFieldNameRegExp(oldFieldName, layoutType);

                if ((re != null) && re.IsMatch(layout))
                {
                    string replaceString;

                    switch (layoutType)
                    {
                        case LayoutTypeEnum.Ascx:
                            replaceString = string.Format(@"$1{0}""", newFieldName);
                            break;

                        default:
                            replaceString = string.Format(@"$1:{0}$$$$", newFieldName);
                            break;
                    }

                    // Replace field names
                    layout = re.Replace(layout, replaceString);

                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Returns regular expression based on layout type which selects specified field name in form layout.
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="layoutType">Form layout type</param>
        private static Regex GetLayoutFieldNameRegExp(string fieldName, LayoutTypeEnum layoutType)
        {
            switch (layoutType)
            {
                case LayoutTypeEnum.Ascx:
                    return RegexHelper.GetRegex(@"(Field="")" + fieldName + @"""", true);
                case LayoutTypeEnum.Html:
                    return RegexHelper.GetRegex(@"(\$\$[a-zA-Z]+?):" + fieldName + @"\$\$", true);
                default:
                    return null;
            }
        }


        /// <summary>
        /// Checks if column name is unique in system tables which are joined with given table into view.
        /// </summary>
        /// <param name="className">Name of the class, where column with columnName is created</param>
        /// <param name="columnName">Name of the column to be checked</param>
        public static bool ColumnExistsInView(string className, string columnName)
        {
            string[] views = null;

            switch (className.ToLowerInvariant())
            {
                case PredefinedObjectType.USER:
                case PredefinedObjectType.USERSETTINGS:
                    views = new [] { "View_Community_Member" }; // with cms.usersettings
                    break;
            }

            return (views != null) && ViewsHaveColumn(views, columnName);
        }


        /// <summary>
        /// Checks if column specified by name is present in views specified by view name.
        /// </summary>
        /// <param name="views">Array of classNames, in which table to search</param>
        /// <param name="columnName">Name of column, which is searched for</param>
        private static bool ViewsHaveColumn(IEnumerable<string> views, string columnName)
        {
            TableManager tm = new TableManager(null);

            foreach (string view in views)
            {
                if (tm.ColumnExistsInView(view, columnName))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Updates the classes inherited from the given class.
        /// </summary>
        /// <param name="parentClass">Parent class</param>
        /// <param name="originalParentClassId">Class ID of the original parent to avoid loops; if not set ClassID of parentClass is used</param>
        public static void UpdateInheritedClasses(DataClassInfo parentClass, int originalParentClassId = 0)
        {
            if (parentClass == null)
            {
                return;
            }

            if (originalParentClassId == 0)
            {
                originalParentClassId = parentClass.ClassID;
            }

            // Get the inherited classes
            var classes = DataClassInfoProvider.GetClasses().Where("ClassInheritsFromClassID", QueryOperator.Equals, parentClass.ClassID);

            if (classes.HasResults())
            {
                // This is potentially a long-running operation, do not allow it to execute in transaction
                if (CMSTransactionScope.IsInTransaction)
                {
                    throw new NotSupportedException("This action is not allowed to be called in an open transaction. It is a long-running operation, and could cause heavy database locks, or connection timeouts.");
                }

                // Update the inherited classes
                foreach (var inheritedClass in classes)
                {
                    // Update the class
                    UpdateInheritedClass(parentClass, inheritedClass, originalParentClassId);
                }
            }
        }


        /// <summary>
        /// Updates the class inherited from the given class.
        /// </summary>
        /// <param name="parentClass">Parent class info</param>
        /// <param name="inheritedClass">Inherited class info</param>
        /// <param name="originalParentClassId">Class ID of the original parent to avoid loops; if not set ClassID of parentClass is used</param>
        public static void UpdateInheritedClass(DataClassInfo parentClass, DataClassInfo inheritedClass, int originalParentClassId = 0)
        {
            if (parentClass == null || inheritedClass == null)
            {
                return;
            }

            if (originalParentClassId == 0)
            {
                originalParentClassId = parentClass.ClassID;
            }

            if (inheritedClass.ClassID == originalParentClassId)
            {
                throw new Exception("[FormHelper.UpdateInheritedClasses]: Cannot inherit '" + inheritedClass.ClassDisplayName + "', there is a loop in the inheritance chain.");
            }

            // Update within transaction to maintain consistency
            using (var tr = new CMSTransactionScope())
            {
                // Prepare Form infos
                var parentFi = new FormInfo(parentClass.ClassFormDefinition);
                var inheritedFi = new FormInfo(inheritedClass.ClassFormDefinition);

                var elems = parentFi.GetFormElements(true, true);

                // Get the current inherited fields and categories
                var inheritedFields = GetInheritedFields(inheritedFi);
                var inheritedCategories = GetInheritedCategories(inheritedFi);

                // Process all elements (fields and categories)
                int index = 0;
                foreach (IDataDefinitionItem elem in elems)
                {
                    object existing;

                    // Get form field info
                    var ffi = elem as FormFieldInfo;
                    if (ffi != null)
                    {
                        if (!ffi.PrimaryKey)
                        {
                            // Prepare the info
                            ffi = (FormFieldInfo)ffi.Clone();
                            ffi.IsInherited = true;
                            ffi.System = false;

                            // Add only non-primary keys
                            existing = inheritedFi.GetFormField(ffi.Name);

                            // Always replace with inherited field settings
                            if (existing != null)
                            {
                                inheritedFi.UpdateFormField(ffi.Name, ffi);
                            }
                            else
                            {
                                inheritedFi.AddFormItem(ffi, index);

                                // Hide new field in alternative forms if necessary
                                HideFieldInAlternativeForms(ffi, inheritedClass);
                            }

                            inheritedFields[ffi.Name.ToLowerInvariant()] = null;
                        }
                    }
                    else
                    {
                        // Get form category info
                        var fci = elem as FormCategoryInfo;
                        if (fci != null)
                        {
                            fci.IsInherited = true;

                            existing = inheritedFi.GetFormCategory(fci.CategoryName);

                            // Always replace with inherited category settings
                            if (existing != null)
                            {
                                inheritedFi.UpdateFormCategory(fci.CategoryName, fci);
                            }
                            else
                            {
                                inheritedFi.AddFormCategory(fci, index);
                            }

                            inheritedCategories[fci.CategoryName.ToLowerInvariant()] = null;
                        }
                    }

                    index++;
                }

                // Remove remaining inherited fields (were deleted)
                foreach (var ffi in inheritedFields.TypedValues.OfType<FormFieldInfo>())
                {
                    int order = inheritedFi.ItemsList.IndexOf(ffi);

                    // Delete field from class and its alternative forms
                    inheritedFi.RemoveFormField(ffi.Name);
                    RemoveFieldFromAlternativeForms(inheritedClass, ffi.Name, order);
                }

                // Remove remaining inherited categories (were deleted)
                foreach (var fci in inheritedCategories.TypedValues.OfType<FormCategoryInfo>())
                {
                    inheritedFi.RemoveFormCategory(fci.CategoryName);
                }

                // Only change it if the form definition is different (to prevent loops)
                string newFormDefinition = inheritedFi.GetXmlDefinition();

                bool updateDb = newFormDefinition != inheritedClass.ClassFormDefinition;
                if (updateDb)
                {
                    // Update definition
                    inheritedClass.ClassFormDefinition = newFormDefinition;
                }

                // Save the class
                DataClassInfoProvider.SetDataClassInfo(inheritedClass);

                if (updateDb)
                {
                    // Clear default queries
                    QueryInfoProvider.ClearDefaultQueries(inheritedClass, true, true);
                }

                // Commit the changes
                tr.Commit();
            }

            // Recursively update inherited classes
            UpdateInheritedClasses(inheritedClass, originalParentClassId);
        }


        /// <summary>
        /// Gets the table of the fields that are inherited
        /// </summary>
        /// <param name="fi">Form info to examine</param>
        public static SafeDictionary<string, IDataDefinitionItem> GetInheritedFields(FormInfo fi)
        {
            var fields = new SafeDictionary<string, IDataDefinitionItem>();

            // Remove all currently inherited elements
            foreach (var ffi in fi.GetFormElements(true, true).OfType<FormFieldInfo>())
            {
                if (ffi.IsInherited)
                {
                    fields.Add(ffi.Name.ToLowerInvariant(), ffi);
                }
            }

            return fields;
        }


        /// <summary>
        /// Gets the table of the categories that are inherited.
        /// </summary>
        /// <param name="fi">Form info to examine</param>
        public static SafeDictionary<string, IDataDefinitionItem> GetInheritedCategories(FormInfo fi)
        {
            var categories = new SafeDictionary<string, IDataDefinitionItem>();

            // Remove all currently inherited elements
            foreach (var fci in fi.GetFormElements(true, true).OfType<FormCategoryInfo>())
            {
                if (fci.IsInherited)
                {
                    categories.Add(fci.CategoryName.ToLowerInvariant(), fci);
                }
            }

            return categories;
        }


        /// <summary>
        /// Removes the inherited fields from the given form info.
        /// </summary>
        /// <param name="fi">Form info</param>
        /// <param name="deleteFields">If false, the inherited fields are only unmarked as inherited</param>
        public static void RemoveInheritedFields(FormInfo fi, bool deleteFields)
        {
            // Remove all currently inherited elements
            var elems = fi.GetFormElements(true, true);

            foreach (IDataDefinitionItem elem in elems)
            {
                var ffi = elem as FormFieldInfo;
                if (ffi != null)
                {
                    if (ffi.IsInherited)
                    {
                        // Remove the field if inherited
                        if (deleteFields)
                        {
                            fi.RemoveFormField(ffi.Name);
                        }
                        else
                        {
                            ffi.IsInherited = false;
                        }
                    }
                }
                else
                {
                    var fci = elem as FormCategoryInfo;
                    if (fci != null)
                    {
                        if (fci.IsInherited)
                        {
                            // Remove the category if inherited
                            if (deleteFields)
                            {
                                fi.RemoveFormCategory(fci.CategoryName);
                            }
                            else
                            {
                                fci.IsInherited = false;
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Removes the inheritance from the given class.
        /// </summary>
        /// <param name="inheritedClass">Inherited class</param>
        /// <param name="deleteFields">If true, the fields are supposed to be deleted</param>
        public static void RemoveInheritance(DataClassInfo inheritedClass, bool deleteFields)
        {
            if (!inheritedClass.ClassIsCoupledClass)
            {
                return;
            }

            // Update within transaction to maintain consistency
            using (var tr = new CMSTransactionScope())
            {
                // Prepare Form info
                FormInfo fi = new FormInfo(inheritedClass.ClassFormDefinition);

                // Remove the fields (or unmark them as inherited)
                RemoveInheritedFields(fi, deleteFields);

                // Only change it if the form definition is different (to prevent loops)
                string newFormDefinition = fi.GetXmlDefinition();

                bool updateDb = (newFormDefinition != inheritedClass.ClassFormDefinition);
                if (updateDb)
                {
                    // Update definition
                    inheritedClass.ClassFormDefinition = newFormDefinition;
                }

                // Save the class
                DataClassInfoProvider.SetDataClassInfo(inheritedClass);

                if (updateDb)
                {
                    // Clear default queries
                    QueryInfoProvider.ClearDefaultQueries(inheritedClass, true, true);
                }

                // Commit the changes
                tr.Commit();
            }

            // Recursively update inherited classes
            UpdateInheritedClasses(inheritedClass);
        }


        /// <summary>
        /// Gets FormFieldInfo with initialized settings (Settings and SettingMacroTable properties) from specified XML representation.
        /// </summary>
        /// <param name="settingsXml">Form control settings XML</param>
        public static FormFieldInfo GetFormControlSettingsFromXML(string settingsXml)
        {
            // Get form field info
            var ffi = new FormFieldInfo();
            var document = new XmlDocument();
            document.LoadXml("<form><field>" + settingsXml + "</field></form>");
            if (document.DocumentElement != null)
            {
                ffi.LoadFromXmlNode(document.DocumentElement.ChildNodes[0]);
            }

            return ffi;
        }


        /// <summary>
        /// Gets settings XML representation from specified FormFieldInfo.
        /// </summary>
        /// <param name="ffi">Form field info</param>
        public static string GetFormControlSettingsXml(FormFieldInfo ffi)
        {
            // Get XML
            var document = new XmlDocument();
            document.LoadXml("<form></form>");
            XmlNode node = ffi.GetXmlNode(document).SelectSingleNode(FIELD_SETTIGNS_NODE);
            if (node != null)
            {
                return node.OuterXml;
            }
            return null;
        }


        /// <summary>
        /// Ensures category at the beginning of the form definition.
        /// </summary>
        /// <param name="formDefinition">Form definition</param>
        /// <param name="categoryName">Default category name</param>
        public static string EnsureDefaultCategory(string formDefinition, string categoryName)
        {
            if (String.IsNullOrEmpty(formDefinition))
            {
                return formDefinition;
            }

            var xmlDocument = new XmlDocument();

            try
            {
                xmlDocument.LoadXml(formDefinition);
            }
            catch (XmlException)
            {
                return formDefinition;
            }

            var rootNode = xmlDocument.DocumentElement;

            if ((rootNode == null) || !rootNode.Name.Equals("form", StringComparison.InvariantCultureIgnoreCase))
            {
                return formDefinition;
            }

            if (rootNode.HasChildNodes && rootNode.FirstChild.Name.Equals("category", StringComparison.InvariantCultureIgnoreCase))
            {
                return formDefinition;
            }

            var categoryNode = xmlDocument.CreateElement("category");
            categoryNode.SetAttribute("name", categoryName);
            rootNode.PrependChild(categoryNode);
            formDefinition = xmlDocument.OuterXml;

            return formDefinition;
        }


        /// <summary>
        /// Removes field from alternative definition and keeps order of other items.
        /// </summary>
        /// <param name="formDefinition">Form definition from which the item will be removed</param>
        /// <param name="fieldName">Name of field which will be removed</param>
        /// <param name="order">Item order in original form</param>
        public static string RemoveFieldFromAlternativeDefinition(string formDefinition, string fieldName, int order)
        {
            return RemoveFormItemAlternativeDefinition(formDefinition, fieldName, order, "field");
        }


        /// <summary>
        /// Removes category from alternative definition and keeps order of other items.
        /// </summary>
        /// <param name="formDefinition">Form definition from which the item will be removed</param>
        /// <param name="categoryName">Name of category which will be removed</param>
        /// <param name="order">Item order in original form</param>
        public static string RemoveCategoryFromAlternativeDefinition(string formDefinition, string categoryName, int order)
        {
            return RemoveFormItemAlternativeDefinition(formDefinition, categoryName, order, "category");
        }


        /// <summary>
        /// Removes form item (field or category) from alternative definition and keeps order of other items.
        /// </summary>
        /// <param name="itemName">Item to remove</param>
        /// <param name="order">Item order in original form</param>
        /// <param name="type">Type of form item - field or category</param>
        /// <param name="formDefinition">Form definition from which the item will be removed</param>
        private static string RemoveFormItemAlternativeDefinition(string formDefinition, string itemName, int order, string type)
        {
            if (string.IsNullOrEmpty(formDefinition))
            {
                return string.Empty;
            }

            // Load form definition
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.LoadXml(formDefinition);
            }
            catch (XmlException)
            {
                // either no definition or malformed one - do not clear anything
                return formDefinition;
            }

            // Get XML attribute name to check
            string attribute = type.Equals("field", StringComparison.InvariantCultureIgnoreCase) ? FIELD_NAME_ATTRIBUTE : "name";

            XmlNode item = xml.DocumentElement.SelectSingleNode(type + "[@" + attribute + "='" + itemName + "']");

            if (item != null)
            {
                order = ValidationHelper.GetInteger(XmlHelper.GetAttributeValue(item, "order"), -1);

                // If item exists in alternative definition but has not set order attribute, get its position in XML
                if (order == -1)
                {
                    int orderInAltForm = 0;
                    foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                    {
                        if (node.Equals(item))
                        {
                            break;
                        }
                        orderInAltForm++;
                    }
                    order = orderInAltForm;
                }

                // Remove item
                item.ParentNode.RemoveChild(item);
            }

            // Update order of items after deleted item
            foreach (XmlNode node in xml.DocumentElement.ChildNodes)
            {
                XmlAttribute orderAttribute = node.Attributes["order"];
                if (orderAttribute != null)
                {
                    int itemOrder = ValidationHelper.GetInteger(orderAttribute.Value, -1);
                    if (itemOrder > order)
                    {
                        orderAttribute.Value = (itemOrder - 1).ToString();
                    }
                }
            }
            return xml.OuterXml;
        }


        /// <summary>
        /// Hides given field in alternative forms belonging to given class if hiding new fields is enabled.
        /// </summary>
        /// <param name="fieldInfo">Form field info to be hidden</param>
        /// <param name="dci">Class with alternative forms</param>
        public static void HideFieldInAlternativeForms(FormFieldInfo fieldInfo, DataClassInfo dci)
        {
            // Hide field for alternative forms that require it
            if (dci == null)
            {
                return;
            }

            var altforms = AlternativeFormInfoProvider.GetAlternativeForms()
                                                      .Where(GetAlternativeFormsWhere(dci))
                                                      .WhereTrue("FormHideNewParentFields");

            foreach (AlternativeFormInfo afi in altforms)
            {
                afi.HideField(fieldInfo);
                AlternativeFormInfoProvider.SetAlternativeFormInfo(afi);
            }
        }


        /// <summary>
        /// Returns where condition allowing to select alternative forms belonging to given class.
        /// </summary>
        /// <param name="classInfo">Class with alternative forms</param>
        internal static WhereCondition GetAlternativeFormsWhere(DataClassInfo classInfo)
        {
            var whereCondition = new WhereCondition().WhereEquals("FormClassID", classInfo.ClassID);

            // If edited class is user settings, update user alt.forms which are combined with user settings too
            if (classInfo.ClassName.Equals(PredefinedObjectType.USERSETTINGS, StringComparison.InvariantCultureIgnoreCase))
            {
                DataClassInfo userClass = DataClassInfoProvider.GetDataClassInfo(PredefinedObjectType.USER);
                if (userClass != null)
                {
                    whereCondition.Or().Where(new WhereCondition()
                                                    .WhereEquals("FormClassID", userClass.ClassID)
                                                    .WhereEquals("FormCoupledClassID", classInfo.ClassID)
                                                    );
                }
            }

            return whereCondition;
        }


        /// <summary>
        /// Removes field from alternative forms belonging to given class.
        /// </summary>
        /// <param name="formClassInfo">Class with alternative forms</param>
        /// <param name="fieldName">Name of the deleted field</param>
        /// <param name="order">Order of deleted field in class</param>
        public static void RemoveFieldFromAlternativeForms(DataClassInfo formClassInfo, string fieldName, int order)
        {
            // Update alternative forms
            var where = GetAlternativeFormsWhere(formClassInfo);
            var altforms = AlternativeFormInfoProvider.GetAlternativeForms().Where(where);
            foreach (AlternativeFormInfo afi in altforms)
            {
                afi.FormDefinition = RemoveFieldFromAlternativeDefinition(afi.FormDefinition, fieldName, order);
                AlternativeFormInfoProvider.SetAlternativeFormInfo(afi);
            }
        }


        /// <summary>
        /// Returns a transformation action. If given macro table contains key passed to the action (second action parameter), it replaces current value (third action parameter) in the element passed to the action (first action parameter) and adds "ismacro" attribute to the element.
        /// </summary>
        /// <param name="macroTable">Macro table for the properties.</param>
        public static Action<XmlElement, string, object> GetPropertyMacroTransformation(IDictionary macroTable)
        {
            Action<XmlElement, string, object> transformation = null;

            if (macroTable != null)
            {
                transformation = (element, key, value) =>
                {
                    var macroValue = macroTable[key];
                    if ((macroValue != null) && (element != null))
                    {
                        element.SetAttribute("ismacro", "true");
                        element.InnerText = Convert.ToString(macroValue, CultureHelper.EnglishCulture);
                    }
                };
            }

            return transformation;
        }

        #endregion


        #region "Culture dependent methods"

        /// <summary>
        /// Gets double value in system (en) culture format.
        /// </summary>
        /// <param name="value">Value to be converted</param>
        public static string GetDoubleValueInSystemCulture(string value)
        {
            // Get double value in current culture
            double decValue = ValidationHelper.GetDouble(value, Double.NaN);

            // Try to get double in database culture (for fields in field editor which are not displayed)
            // These are not converted to current culture and stay in en-us culture
            if (Double.IsNaN(decValue))
            {
                decValue = ValidationHelper.GetDoubleSystem(value, Double.NaN);
            }

            // Value is a double number
            if (!Double.IsNaN(decValue))
            {
                // Convert to 'en-us' culture
                return Convert.ToString(decValue, CultureHelper.EnglishCulture);
            }

            return null;
        }


        /// <summary>
        /// Gets decimal value in system (en) culture format.
        /// </summary>
        /// <param name="value">Value to be converted</param>
        public static string GetDecimalValueInSystemCulture(string value)
        {
            // Try get decimal value in current culture or in 'en-us' culture (for fields in field editor which are not displayed and remain in 'en-us' culture format)
            decimal result;
            if (Decimal.TryParse(value, out result) || Decimal.TryParse(value, NumberStyles.Number, CultureHelper.EnglishCulture, out result))
            {
                // Convert to 'en-us' culture
                return result.ToString(CultureHelper.EnglishCulture);
            }

            return null;
        }


        /// <summary>
        /// Gets date-time value in system (en) culture format.
        /// </summary>
        /// <param name="value">Value to be converted</param>
        public static string GetDateTimeValueInSystemCulture(string value)
        {
            // Get date value in current culture
            DateTime dateValue = ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME);

            // Try to get date in database culture (for fields in field editor which are not displayed)
            // These are not converted to current culture and stay in en-us culture
            if (dateValue == DateTimeHelper.ZERO_TIME)
            {
                dateValue = ValidationHelper.GetDateTimeSystem(value, DateTimeHelper.ZERO_TIME);
            }

            // Value is a date-time
            if (dateValue != DateTimeHelper.ZERO_TIME)
            {
                // Convert to 'en-us' culture
                return Convert.ToString(dateValue, CultureHelper.EnglishCulture);
            }

            return null;
        }


        /// <summary>
        /// Gets date value in system (en) culture format.
        /// </summary>
        /// <param name="value">Value to be converted</param>
        public static string GetDateValueInSystemCulture(string value)
        {
            // Get date value in current culture
            DateTime dateValue = ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME);

            // Try to get date in database culture (for fields in field editor which are not displayed)
            // These are not converted to current culture and stay in en-us culture
            if (dateValue == DateTimeHelper.ZERO_TIME)
            {
                dateValue = ValidationHelper.GetDateTimeSystem(value, DateTimeHelper.ZERO_TIME);
            }

            // Value is a date-time
            if (dateValue != DateTimeHelper.ZERO_TIME)
            {
                // Convert to 'en-us' culture
                return dateValue.ToString("d", CultureHelper.EnglishCulture);
            }

            return null;
        }

        #endregion


        #region "BizForm path methods"

        /// <summary>
        /// Returns full file name ([name.extension] if extension is specified) or ([name] only if extension is not specified).
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="fileExtension">File extension</param>
        public static string GetFullFileName(string fileName, string fileExtension)
        {
            string file = fileName;

            if (!(string.IsNullOrEmpty(file) || string.IsNullOrEmpty(fileExtension)))
            {
                file += "." + fileExtension.TrimStart(new char[] { '.' });
            }

            return file;
        }


        /// <summary>
        /// Returns file name in format [OriginalFileName].[extension].
        /// </summary>
        /// <param name="fileNamesString">File names from database in format "[guid].[extension]/[originalfilename].[extension]"</param>
        public static string GetOriginalFileName(string fileNamesString)
        {
            if (!string.IsNullOrEmpty(fileNamesString))
            {
                string[] temp = fileNamesString.Split('/');
                return temp[temp.Length - 1];
            }

            return String.Empty;
        }


        /// <summary>
        /// Returns file name in format [guid].[extension].
        /// </summary>
        /// <param name="fileNamesString">File names from database in format "[guid].[extension]/[originalfilename].[extension]"</param>
        public static string GetGuidFileName(string fileNamesString)
        {
            if (!string.IsNullOrEmpty(fileNamesString))
            {
                return fileNamesString.Split('/')[0];
            }

            return String.Empty;
        }


        /// <summary>
        /// Gets path to the file in file system.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="fileName">File name</param>
        /// <param name="webFullPath">Physical path to the root of the web project (e.g. c:\WebProject\), if it is not specified web application physical path is used</param>
        /// <returns>Physical path</returns>
        public static string GetFilePhysicalPath(string siteName, string fileName, string webFullPath = null)
        {
            return GetBizFormFilesFolderPath(siteName, webFullPath) + fileName;
        }


        /// <summary>
        /// Returns BizForm files folder physical path according to 'CMSBizFormFilesFolder' settings key.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="webFullPath">Physical path to the root of the web project (e.g. c:\WebProject\), if it is not specified web application physical path is used (optional)</param>
        public static string GetBizFormFilesFolderPath(string siteName, string webFullPath = null)
        {
            // Get bizform files folder
            string filesFolderPath = BizFormFilesFolder(siteName);

            if (String.IsNullOrEmpty(webFullPath))
            {
                webFullPath = SystemContext.WebApplicationPhysicalPath;
            }

            // If settings key is not specified -> get default files folder path
            if (string.IsNullOrEmpty(filesFolderPath))
            {
                filesFolderPath = DirectoryHelper.CombinePath(webFullPath, siteName, "BizFormFiles") + "\\";
            }
            else
            {
                // Path is relative, for example: '~/filefolder', '/filefolder', 'filefolder'
                if (!Path.IsPathRooted(filesFolderPath) || filesFolderPath.StartsWith("/", StringComparison.InvariantCultureIgnoreCase))
                {
                    filesFolderPath = filesFolderPath.StartsWith("~/", StringComparison.InvariantCultureIgnoreCase) ? filesFolderPath.Substring(2) : filesFolderPath;
                    filesFolderPath = filesFolderPath.TrimStart('/');
                    filesFolderPath = DirectoryHelper.CombinePath(SystemContext.WebApplicationPhysicalPath, Path.EnsureBackslashes(filesFolderPath)) + "\\";
                }
                // Path is absolute, for example: 'c:\filefolder', 'c:\filefolder\'
                else
                {
                    filesFolderPath = Path.EnsureEndBackslash(filesFolderPath);
                }

                // Check if site specific folder should be used
                if (UseSiteSpecificCustomFolder(siteName))
                {
                    filesFolderPath = DirectoryHelper.CombinePath(filesFolderPath, siteName) + "\\";
                }
            }

            return filesFolderPath;
        }


        /// <summary>
        /// Indicates if site specific custom folder should be used for storing on-line form files.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns>True if custom folder is used, otherwise false</returns>
        public static bool UseSiteSpecificCustomFolder(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSUseBizFormsSiteFolder");
        }


        /// <summary>
        /// Gets custom on-line form file folder path.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns>Value of custom on-line form file folder</returns>
        public static string BizFormFilesFolder(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSBizFormFilesFolder");
        }

        #endregion


        #region "Validation macro rules methods"

        /// <summary>
        /// Returns validation rule name.
        /// </summary>
        /// <param name="ruleText">String representation of validation rule</param>
        public static string GetValidationRuleName(string ruleText)
        {
            Match match = RuleNameRegExp.Match(ruleText);

            return match.Success ? match.Result("$1") : string.Empty;
        }


        /// <summary>
        /// Returns value of parameter with given name.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="ruleText">String representation of validation rule</param>
        /// <returns></returns>
        public static string GetValidationRuleParameter(string paramName, string ruleText)
        {
            Regex regexp = new Regex(@"<p\s[^>]*n=\\""" + paramName + @"\\""\s*>.*<v>(.*)</v>.*(?=</p>)");
            Match match = regexp.Match(ruleText);

            return match.Success ? match.Result("$1") : string.Empty;
        }


        /// <summary>
        /// Adds macro rules values to given XmlNode.
        /// </summary>
        /// <param name="node">Field node</param>
        /// <param name="macroRules">Field macro rules enumeration</param>
        public static void AddFieldMacroRules(XmlNode node, IEnumerable<FieldMacroRule> macroRules)
        {
            XmlNode rulesNode = node.SelectSingleNode(FIELD_RULES_NODE);
            if (rulesNode == null)
            {
                // Add rules node and its child nodes
                rulesNode = node.OwnerDocument.CreateElement(FIELD_RULES_NODE);
            }

            foreach (FieldMacroRule rule in macroRules)
            {
                if (rule != null)
                {
                    rulesNode.AppendChild(rule.GetXML(node.OwnerDocument));
                }
            }

            // Add rules node to the field node (if it is not empty)
            if (!string.IsNullOrEmpty(rulesNode.InnerXml))
            {
                node.AppendChild(rulesNode);
            }
        }

        #endregion


        #region "Methods for backward compatibility"

        /// <summary>
        /// Converts validation from field attributes to macro rules.
        /// </summary>
        /// <param name="fieldNode">Field node</param>
        public static void ConvertValidationFromAttributesToRules(XmlNode fieldNode)
        {
            List<FieldMacroRule> macroRules = new List<FieldMacroRule>();

            // Create validation rules from field attributes
            AddFieldMacroRuleFromAttribute(fieldNode.Attributes["minstringlength"], MACRO_RULE_MIN_STRING_LENGTH, macroRules);
            AddFieldMacroRuleFromAttribute(fieldNode.Attributes["maxstringlength"], MACRO_RULE_MAX_STRING_LENGTH, macroRules);
            AddFieldMacroRuleFromAttribute(fieldNode.Attributes["minnumericvalue"], MACRO_RULE_MIN_VALUE, macroRules);
            AddFieldMacroRuleFromAttribute(fieldNode.Attributes["maxnumericvalue"], MACRO_RULE_MAX_VALUE, macroRules);
            AddFieldMacroRuleFromAttribute(fieldNode.Attributes["mindatetimevalue"], MACRO_RULE_DATE_FROM, macroRules);
            AddFieldMacroRuleFromAttribute(fieldNode.Attributes["maxdatetimevalue"], MACRO_RULE_DATE_TO, macroRules);
            AddFieldMacroRuleFromAttribute(fieldNode.Attributes["regularexpression"], MACRO_RULE_REGULAR_EXPRESSION, macroRules);

            AddFieldMacroRules(fieldNode, macroRules);
        }


        /// <summary>
        /// Adds validation rule to FieldMacroRules collection if attribute has value.
        /// </summary>
        /// <param name="attribute">Xml attribute</param>
        /// <param name="ruleText">Text expression of macro rule</param>
        /// <param name="macroRules">Collection of macro rules</param>
        private static void AddFieldMacroRuleFromAttribute(XmlAttribute attribute, string ruleText, ICollection<FieldMacroRule> macroRules)
        {
            if (attribute != null)
            {
                string value = XmlHelper.GetXmlAttributeValue(attribute, string.Empty);

                if (!string.IsNullOrEmpty(value))
                {
                    // Ensure proper form of regular expression
                    if (attribute.Name.EqualsCSafe("regularexpression", true))
                    {
                        if (!value.StartsWithCSafe("^"))
                        {
                            value = "^" + value;
                        }
                        if (!value.EndsWithCSafe("$"))
                        {
                            value += "$";
                        }
                    }

                    // Escape and encode parameter to correct format
                    value = value.Replace("\"", "\\\"").Replace("\\", "\\\\");
                    string encodedValue = HTMLHelper.HTMLEncode(value);
                    value = value.Replace("\"", "\\\"");

                    FieldMacroRule fmr = new FieldMacroRule();
                    fmr.MacroRule = string.Format(ruleText, value, encodedValue);
                    macroRules.Add(fmr);
                }
                attribute.OwnerElement.Attributes.Remove(attribute);
            }
        }

        #endregion
    }
}