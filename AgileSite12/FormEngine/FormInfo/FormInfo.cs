using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.FormEngine
{
    /// <summary>
    /// Ensures management of XML file that represents the form definition.
    /// </summary>
    [Serializable]
    public class FormInfo : DataDefinition
    {
        #region "Variables"

        /// <summary>
        /// Indicates if any item uses HtmlArea.
        /// </summary>
        private bool? mUsesHtmlArea;

        private MacroResolver mContextResolver;

        #endregion


        #region "Constant"

        /// <summary>
        /// Dashboard display context.
        /// </summary>
        public const string DISPLAY_CONTEXT_DASHBOARD = "dashboard";


        /// <summary>
        /// Name of attribute version used in XML documents.
        /// </summary>
        public const string ATTRIBUTE_VERSION = "version";

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets FormInfo name.
        /// </summary>
        public string FormName
        {
            get;
            set;
        }


        /// <summary>
        /// Context macro resolver.
        /// </summary>
        public MacroResolver ContextResolver
        {
            get
            {
                return mContextResolver ?? (mContextResolver = MacroResolverStorage.GetRegisteredResolver(FormHelper.FORMDEFINITION_PREFIX + GetXmlDefinition()));
            }
            set
            {
                mContextResolver = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Empty constructor
        /// </summary>
        public FormInfo()
            : base(null)
        {
        }


        /// <summary>
        /// Constructor, creates the form info structure and loads specified form definition.
        /// </summary>
        /// <param name="formDefinition">XML definition of the form</param>
        public FormInfo(string formDefinition)
            : base(formDefinition)
        {
        }

        #endregion


        #region "Form field methods"

        /// <summary>
        /// Gets a unique field name based on the existing field names in the form
        /// </summary>
        /// <param name="name">Base field name</param>
        public string GetUniqueFieldName(string name)
        {
            // Do not change the name if there is no name collision
            if (GetFormField(name) == null)
            {
                return name;
            }

            // Add index to get unique name
            int index = 1;
            string newName = name + index;

            while (GetFormField(newName) != null)
            {
                index++;
                newName = name + index;
            }

            return newName;
        }


        /// <summary>
        /// Returns field object with data from specified field.
        /// </summary>
        /// <param name="fieldName">Field to get data from</param>
        /// <returns>Field or null if field does not exist</returns>
        public FormFieldInfo GetFormField(string fieldName)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException("fieldName");
            }

            return GetFields<FormFieldInfo>()
                .FirstOrDefault(x => x.Name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase));
        }


        /// <summary>
        /// Returns field object with data from specified field.
        /// </summary>
        /// <param name="guid">Form field guid</param>
        /// <returns>Field or null if field does not exist</returns>
        public FormFieldInfo GetFormField(Guid guid)
        {
            if (guid == Guid.Empty)
            {
                return null;
            }

            return GetFields<FormFieldInfo>()
                .FirstOrDefault(x => x.Guid == guid);
        }


        /// <summary>
        /// Returns true if the given field exists in the form definition
        /// </summary>
        /// <param name="fieldName">Field name to check</param>
        public bool FieldExists(string fieldName)
        {
            return GetFormField(fieldName) != null;
        }


        /// <summary>
        /// Update specified field node.
        /// </summary>
        /// <param name="fieldName">Name of a field to update</param>
        /// <param name="field">Data for update</param>
        /// <exception cref="ArgumentOutOfRangeException">Field fieldName does not exist</exception>
        public void UpdateFormField(string fieldName, FormFieldInfo field)
        {
            ItemsList[ItemsList.IndexOf(GetFormField(fieldName))] = field;
        }


        /// <summary>
        /// Adds form item at the specified index or at the end if the index is not specified.
        /// </summary>
        /// <param name="formItem">Inserted form item</param>
        /// <param name="index">Index of newly inserted item</param>
        public void AddFormItem(IDataDefinitionItem formItem, int index = -1)
        {
            if (formItem == null)
            {
                return;
            }

            if (index < 0)
            {
                index = ItemsList.Count;
            }

            ItemsList.Insert(index, formItem);
        }


        /// <summary>
        /// Removes specified form field.
        /// </summary>
        /// <param name="fieldName">Name of a field to remove</param>
        /// <returns>True if successfully removed, false otherwise</returns>
        public bool RemoveFormField(string fieldName)
        {
            return ItemsList.Remove(GetFormField(fieldName));
        }


        /// <summary>
        /// Removes fields matching the given condition
        /// </summary>
        /// <param name="condition">Field condition</param>
        /// <returns>Returns true if at least one item was removed</returns>
        public bool RemoveFields(Func<FormFieldInfo, bool> condition)
        {
            var items = ItemsList;
            var toDelete = new List<FormFieldInfo>();

            // Process all fields
            foreach (var field in items)
            {
                var ffi = field as FormFieldInfo;
                if ((ffi != null) && condition(ffi))
                {
                    toDelete.Add(ffi);
                }
            }

            bool update = false;

            // Remove the fields
            foreach (var field in toDelete)
            {
                if (items.Remove(field))
                {
                    // Mark that at least one item was removed
                    update = true;
                }
            }

            return update;
        }


        /// <summary>
        /// Moves specified form field down.
        /// </summary>
        /// <param name="fieldName">Name of a field to move</param>
        public void MoveFormFieldDown(string fieldName)
        {
            // Get index of searched item
            int index = ItemsList.IndexOf(GetFormField(fieldName));

            // Move down only if item found and not at last position
            if ((index > -1) && (index < ItemsList.Count - 1))
            {
                IDataDefinitionItem item = ItemsList[index];
                ItemsList.RemoveAt(index);
                ItemsList.Insert(index + 1, item);
            }
        }


        /// <summary>
        /// Moves specified form field up.
        /// </summary>
        /// <param name="fieldName">Name of a field to move</param>
        public void MoveFormFieldUp(string fieldName)
        {
            // Get index of searched item
            int index = ItemsList.IndexOf(GetFormField(fieldName));

            // Move down only if item found and not at last position
            if (index > 0)
            {
                IDataDefinitionItem item = ItemsList[index];
                ItemsList.RemoveAt(index);
                ItemsList.Insert(index - 1, item);
            }
        }


        /// <summary>
        /// Moves specified form field to specified position in category. If category isn't specified, than field is moved to default category.
        /// </summary>
        /// <param name="fieldName">Name of a field to move</param>
        /// <param name="categoryName">Name of category to which the field is moved</param>
        /// <param name="newPosition">Field's new position in category among visible fields; starts from 0</param>
        public void MoveFormFieldToPositionInCategory(string fieldName, string categoryName, int newPosition)
        {
            IDataDefinitionItem item = GetFormField(fieldName);

            int fieldIndex = ItemsList.IndexOf(item);

            if (fieldIndex >= 0 && ItemsList.Count > 1)
            {
                ItemsList.RemoveAt(fieldIndex);

                int categoryIndex = ItemsList.IndexOf(GetFormCategory(categoryName));

                int newIndex = categoryIndex + 1;
                int visibleFieldCount = 0;

                while (newIndex < ItemsList.Count && (ItemsList[newIndex].GetType() != typeof(FormCategoryInfo)) && newPosition != visibleFieldCount)
                {
                    if (((FormFieldInfo)ItemsList[newIndex]).Visible)
                    {
                        visibleFieldCount++;
                    }

                    newIndex++;
                }

                if (newIndex > ItemsList.Count)
                {
                    newIndex = ItemsList.Count;
                }

                ItemsList.Insert(newIndex, item);
            }
        }


        /// <summary>
        /// Returns an List of field names.
        /// </summary>
        /// <param name="includeDummyFields">Indicates if dummy fields should be included.</param>
        /// <param name="order">Allows to order fields before names are returned.</param>
        public List<string> GetColumnNames(bool includeDummyFields = true, Func<FormFieldInfo, bool> order = null)
        {
            var fields = GetFields<FormFieldInfo>();

            if (!includeDummyFields)
            {
                fields = fields.Where(x => !x.IsDummyField);
            }

            if (order != null)
            {
                fields = fields.OrderBy(order);
            }

            return fields.Select(x => x.Name).ToList();
        }


        /// <summary>
        /// Returns empty data row with column names and types.
        /// </summary>
        /// <param name="convertMacroColumn">If true, columns with macro as default parameter are changed to data type string.</param>
        public DataRow GetDataRow(bool convertMacroColumn = true)
        {
            DataSet ds = new DataSet();
            DataTable table = new DataTable();

            // Create new table column for each columnName
            var columnNames = GetColumnNames();
            if (columnNames != null)
            {
                foreach (string columnName in columnNames)
                {
                    if ((columnName != null) && !table.Columns.Contains(columnName))
                    {
                        FormFieldInfo ffi = GetFormField(columnName);

                        bool isMacro;
                        string defValue = ffi.GetPropertyValue(FormFieldPropertyEnum.DefaultValue, out isMacro);

                        // Macro is stored in text column
                        if ((isMacro || MacroProcessor.ContainsMacro(defValue)) && convertMacroColumn)
                        {
                            table.Columns.Add(columnName, typeof(string));
                        }
                        else
                        {
                            table.Columns.Add(columnName, DataTypeManager.GetSystemType(TypeEnum.Field, ffi.DataType));
                        }
                    }
                }
            }

            ds.Tables.Add(table);
            DataRow row = table.NewRow();
            table.Rows.Add(row);

            // Return new row with specified schema
            return row;
        }


        /// <summary>
        /// Returns an List of FormFieldInfo objects.
        /// </summary>
        /// <param name="visible">Indicates whether object with visible set to 'True' should be returned</param>
        /// <param name="invisible">Indicates whether object with visible set to 'False' should be returned</param>
        /// <param name="includeSystem">Indicates whether also system fields are included</param>
        /// <param name="onlyPrimaryKeys">Indicates whether only primary keys should be returned</param>
        /// <param name="includeDummyFields">Indicates whether dummy fields are included</param>
        public List<FormFieldInfo> GetFields(bool visible, bool invisible, bool includeSystem = true, bool onlyPrimaryKeys = false, bool includeDummyFields = true)
        {
            var query = GetFields<FormFieldInfo>();

            if (onlyPrimaryKeys)
            {
                query = query.Where(x => x.PrimaryKey);
            }

            if (!visible)
            {
                query = query.Where(x => !x.Visible);
            }

            if (!invisible)
            {
                query = query.Where(x => x.Visible);
            }

            if (!includeSystem)
            {
                query = query.Where(x => !x.System);
            }

            if (!includeDummyFields)
            {
                query = query.Where(x => !x.IsDummyField);
            }

            return query.ToList();
        }


        /// <summary>
        /// Returns the List of the FormFieldInfo objects.
        /// </summary>
        /// <param name="fieldType">Form field control type</param>
        public List<FormFieldInfo> GetFields(FormFieldControlTypeEnum fieldType)
        {
            return GetFields<FormFieldInfo>()
                .Where(x => FormHelper.IsFieldOfType(x, fieldType))
                .ToList();
        }


        /// <summary>
        /// Returns the List of the FormFieldInfo objects.
        /// </summary>
        /// <param name="fieldDataType">Form field data type</param>
        public List<FormFieldInfo> GetFields(string fieldDataType)
        {
            return GetFields<FormFieldInfo>()
                .Where(x => x.DataType.Equals(fieldDataType, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }


        /// <summary>
        /// Updates the existing fields from the FormInfo definition.
        /// </summary>
        /// <param name="existing">FormInfo with existing fields</param>
        public void UpdateExistingFields(FormInfo existing)
        {
            // No changes if form not present
            if (existing != null)
            {
                var currentColumns = GetColumnNames();
                if (currentColumns != null)
                {
                    foreach (string column in currentColumns)
                    {
                        // Get new field definition
                        FormFieldInfo field = existing.GetFormField(column);
                        if (field != null)
                        {
                            UpdateFormField(column, field);
                        }
                    }
                }
            }
        }

        #endregion;


        #region "Form category methods"

        /// <summary>
        /// Returns CategoryInfo object specified by category name.
        /// </summary>
        /// <param name="name">Category name</param>
        public FormCategoryInfo GetFormCategory(string name)
        {
            return GetFields<FormCategoryInfo>()
                .FirstOrDefault(x => x.CategoryName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }


        /// <summary>
        /// Update specified category node.
        /// </summary>
        /// <param name="categoryName">Name of category to update</param>
        /// <param name="fci">FormCategoryInfo object with data for update</param>
        public void UpdateFormCategory(string categoryName, FormCategoryInfo fci)
        {
            ItemsList[ItemsList.IndexOf(GetFormCategory(categoryName))] = fci;
        }


        /// <summary>
        /// Remove specified form category.
        /// </summary>
        /// <param name="categoryName">Name of a category to remove</param>
        public void RemoveFormCategory(string categoryName)
        {
            ItemsList.Remove(GetFormCategory(categoryName));
        }


        /// <summary>
        /// Removes all categories from the form that do not contain any fields
        /// </summary>
        public void RemoveEmptyCategories()
        {
            var items = ItemsList;
            var fieldsInCategory = 0;
            IDataDefinitionItem lastCategory = null;
            var deleteFields = new List<IDataDefinitionItem>();

            foreach (var field in items)
            {
                if (field is FormCategoryInfo)
                {
                    // Mark category to delete
                    if ((lastCategory != null) && (fieldsInCategory == 0))
                    {
                        deleteFields.Add(lastCategory);
                    }

                    lastCategory = field;
                    fieldsInCategory = 0;
                }
                else
                {
                    fieldsInCategory++;
                }
            }

            // Mark category to delete
            if ((lastCategory != null) && (fieldsInCategory == 0))
            {
                deleteFields.Add(lastCategory);
            }

            // Remove the categories
            foreach (var field in deleteFields)
            {
                items.Remove(field);
            }
        }


        /// <summary>
        /// Add a form category at the specified index or at the end if the index is not specified.
        /// </summary>
        /// <param name="categObj">FormCategoryInfo object with data to add</param>
        /// <param name="index">Index to which to add the category</param>
        public void AddFormCategory(FormCategoryInfo categObj, int index = -1)
        {
            if (index < 0)
            {
                index = ItemsList.Count;
            }
            ItemsList.Insert(index, categObj);
        }


        /// <summary>
        /// Moves specified form category down.
        /// </summary>
        /// <param name="categoryName">Name of a category to move</param>
        public void MoveFormCategoryDown(string categoryName)
        {
            // Get index of searched item
            int index = ItemsList.IndexOf(GetFormCategory(categoryName));

            // Move down only if item found and not at last position
            if ((index > -1) && (index < ItemsList.Count - 1))
            {
                IDataDefinitionItem item = ItemsList[index];
                ItemsList.RemoveAt(index);
                ItemsList.Insert(index + 1, item);
            }
        }


        /// <summary>
        /// Moves specified form category up.
        /// </summary>
        /// <param name="categoryName">Name of a category to move</param>
        public void MoveFormCategoryUp(string categoryName)
        {
            // Get index of searched item
            int index = ItemsList.IndexOf(GetFormCategory(categoryName));

            // Move down only if item found and not at last position
            if (index > 0)
            {
                IDataDefinitionItem item = ItemsList[index];
                ItemsList.RemoveAt(index);
                ItemsList.Insert(index - 1, item);
            }
        }


        /// <summary>
        /// Returns an List of category names.
        /// </summary>
        public List<string> GetCategoryNames()
        {
            return GetFields<FormCategoryInfo>()
                .Select(x => x.CategoryName)
                .ToList();
        }


        /// <summary>
        /// Returns closest category the field is placed under or null if there is no category above the field.
        /// </summary>
        /// <param name="field">Field info object</param>
        public FormCategoryInfo GetClosestCategory(FormFieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            FormCategoryInfo category = null;

            int index = ItemsList.IndexOf(field);
            while ((index > 0) && (category == null))
            {
                category = ItemsList[--index] as FormCategoryInfo;
            }

            return category;
        }

        #endregion;


        #region "Getting XML"

        /// <summary>
        /// Gets empty root FormInfo XML node.
        /// </summary>
        public static XmlDocument GetEmptyFormDocument()
        {
            var doc = new XmlDocument();

            doc.AppendChild(CreateRootElement(doc));

            return doc;
        }


        private static XmlElement CreateRootElement(XmlDocument doc)
        {
            var elem = doc.CreateElement("form");

            var attributes = new Dictionary<string, string>
            {
                { ATTRIBUTE_VERSION, FormInfoVersionCode.LATEST_VERSION },
            };

            elem.AddAttributes(attributes);

            return elem;
        }


        /// <summary>
        /// Gets the XML element for the data
        /// </summary>
        /// <param name="doc">Parent XML document</param>
        public override XmlElement GetXmlElement(XmlDocument doc)
        {
            var elem = CreateRootElement(doc);

            // Create XML representation of form items
            foreach (IDataDefinitionItem item in ItemsList)
            {
                var itemNode = item.GetXmlNode(doc);
                elem.AppendChild(itemNode);
            }

            return elem;
        }


        /// <summary>
        /// Returns FormInfo definition in XML.
        /// </summary>
        public string GetXmlDefinition()
        {
            var xml = new XmlDocument();
            xml.AppendChild(GetXmlElement(xml));

            return xml.InnerXml;
        }


        /// <summary>
        /// Returns FromInfo definition in XML used only for FormVisibilityInfo with limited FormFieldInfo attributes.
        /// </summary>
        public XmlElement GetVisibilityElement()
        {
            XmlDocument xml = GetEmptyFormDocument();

            var formElement = xml.DocumentElement;
            if (formElement != null)
            {
                // Set form name (used only in FormVisibilityInfo)
                if (!String.IsNullOrEmpty(FormName))
                {
                    formElement.SetAttribute("name", FormName);
                }

                // Create XML representation of form items
                foreach (FormFieldInfo item in GetFields<FormFieldInfo>())
                {
                    formElement.AppendChild(item.GetVisibilityXml(xml));
                }
            }

            return formElement;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true, if the form uses one or more visible HTML area fields.
        /// </summary>
        public bool UsesHtmlArea()
        {
            return mUsesHtmlArea ?? GetFields(FormFieldControlTypeEnum.HtmlAreaControl).Any();
        }


        /// <summary>
        /// Returns hierarchical dictionary of items. Each category contains list of its fields.
        /// First fields without category are added to empty category.
        /// </summary>
        /// <param name="f">Selective function for fields(not categories)</param>
        public Dictionary<FormCategoryInfo, List<FormFieldInfo>> GetHierarchicalFormElements(Func<FormFieldInfo, bool> f)
        {
            var dictionary = new Dictionary<FormCategoryInfo, List<FormFieldInfo>>();

            // first category is empty
            var category = new FormCategoryInfo();
            dictionary.Add(category, new List<FormFieldInfo>());

            foreach (var item in ItemsList)
            {
                if (item is FormCategoryInfo)
                {
                    category = item as FormCategoryInfo;
                    dictionary.Add(category, new List<FormFieldInfo>());
                }
                else
                {
                    var field = item as FormFieldInfo;
                    if ((f != null) && f(field))
                    {
                        dictionary[category].Add(field);
                    }
                }
            }
            return dictionary;
        }


        /// <summary>
        /// Returns the List of the form elements.
        /// </summary>
        /// <param name="visible">Return visible fields</param>
        /// <param name="invisible">Return invisible fields</param>
        /// <param name="hideSystemFields">Return non-system fields only</param>
        public List<IDataDefinitionItem> GetFormElements(bool visible, bool invisible, bool hideSystemFields = false)
        {
            IEnumerable<IDataDefinitionItem> items = ItemsList;

            if (!visible)
            {
                items = items.Where(x => !((x is FormFieldInfo) && ((FormFieldInfo)x).Visible));
            }

            if (!invisible)
            {
                items = items.Where(x => !((x is FormFieldInfo) && !((FormFieldInfo)x).Visible));
            }

            if (hideSystemFields)
            {
                items = items.Where(x => !((x is FormFieldInfo) && ((FormFieldInfo)x).System));
            }

            return items.ToList();
        }


        /// <summary>
        /// Combines the form with another form info.
        /// </summary>
        /// <param name="newForm">Form to include</param>
        /// <param name="overwriteExisting">If true, existing fields from source form are overwritten by new form fields if their names match</param>
        /// <param name="excludeColumns">Columns to exclude from form combining, separated by semicolon (;)</param>
        /// <param name="includeCategories">If true, categories from the new form will be added</param>
        /// <param name="preserveCategory">If false than even if overwriteExisting is false, field is moved to the current category (but the properties of the field are not overwritten)</param>
        public void CombineWithForm(FormInfo newForm, bool overwriteExisting, string excludeColumns = null, bool includeCategories = false, bool preserveCategory = true)
        {
            CombineWithForm(newForm, new CombineWithFormSettings
            {
                OverwriteExisting = overwriteExisting,
                ExcludeColumns = excludeColumns,
                IncludeCategories = includeCategories,
                PreserveCategory = preserveCategory
            });
        }


        /// <summary>
        /// Combines the form with another form info.
        /// </summary>
        /// <param name="newForm">Form to include</param>
        /// <param name="settings">Combine settings</param>
        public void CombineWithForm(FormInfo newForm, CombineWithFormSettings settings)
        {
            var excludeColumns = settings.ExcludeColumns ?? string.Empty;
            excludeColumns = ";" + excludeColumns.ToLowerCSafe() + ";";

            // Get wanted elements
            var query = newForm.GetFormElements(true, true);

            if (settings.RemoveEmptyCategories)
            {
                RemoveEmptyCategories();
            }

            var categoryNames = GetCategoryNames();
            // Check if original form has any categories
            var ensureCategory = GetFields<FormCategoryInfo>().Any();

            // Process all elements (fields and categories)
            foreach (var elem in query)
            {
                // Add field
                var ffi = elem as FormFieldInfo;
                if (ffi != null)
                {
                    if (!excludeColumns.Contains(";" + ffi.Name.ToLowerCSafe() + ";"))
                    {
                        // Get existing field
                        var existing = GetFormField(ffi.Guid) ?? GetFormField(ffi.Name);
                        if (existing == null)
                        {
                            // Add new field if not yet exists (do not add primary keys)
                            if (!ffi.PrimaryKey)
                            {
                                if (ensureCategory && (!ffi.IsDummyField || ffi.IsDummyFieldFromMainForm))
                                {
                                    // Create default category
                                    var fci = new FormCategoryInfo();
                                    fci.CategoryName = "unsorted";
                                    fci.SetPropertyValue(FormCategoryPropertyEnum.Caption, "{$general.unsorted$}");
                                    fci.SetPropertyValue(FormCategoryPropertyEnum.Visible, "true");

                                    AddFormCategory(fci);
                                    ensureCategory = false;
                                }

                                AddFormItem(ffi);
                            }
                        }
                        else
                        {
                            // Else update with new info if set to update
                            if (settings.OverwriteExisting || (settings.OverwriteHidden && !existing.Visible))
                            {
                                UpdateFormField(existing.Name, ffi);
                            }
                            else if (!settings.PreserveCategory)
                            {
                                // Do not overwrite, but move to the current category
                                RemoveFormField(existing.Name);
                                AddFormItem(existing);
                            }
                        }
                    }
                }
                else
                {
                    // Add category, ignore the ones that already exists
                    var fci = elem as FormCategoryInfo;
                    if ((fci != null) && settings.IncludeCategories && !categoryNames.Contains(fci.CategoryName, StringComparer.InvariantCultureIgnoreCase))
                    {
                        AddFormCategory(fci);
                        ensureCategory = false;
                    }
                }
            }

            if (settings.RemoveEmptyCategories)
            {
                RemoveEmptyCategories();
            }
        }


        /// <summary>
        /// Loads the default values into the DataRow.
        /// </summary>
        /// <param name="dr">Datarow to load</param>
        /// <param name="xmlDefintion">XML definition</param>
        /// <param name="overrideDefaultValue">If true, default values are overridden</param>
        public void LoadDefaultValues(DataRow dr, string xmlDefintion, bool overrideDefaultValue = false)
        {
            if ((dr != null) && (!String.IsNullOrEmpty(xmlDefintion)))
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(xmlDefintion);

                if (xml.DocumentElement == null)
                {
                    return;
                }

                // Get the fields
                var fields = GetFields(true, true);

                // Process all fields
                foreach (FormFieldInfo ffi in fields)
                {
                    // Get the field
                    XmlNode fieldNode = TableManager.SelectFieldNode(xml.DocumentElement, "name", ffi.Name);
                    if (fieldNode != null)
                    {
                        string inheritedDefaultValue = fieldNode.Attributes["value"].Value;
                        if (MacroProcessor.ContainsMacro(inheritedDefaultValue))
                        {
                            ffi.SetPropertyValue(FormFieldPropertyEnum.DefaultValue, inheritedDefaultValue, true);
                            inheritedDefaultValue = MacroResolver.Resolve(inheritedDefaultValue);
                        }

                        // This prevent empty overridden values to be replaced by default values
                        if (overrideDefaultValue)
                        {
                            ffi.SetPropertyValue(FormFieldPropertyEnum.DefaultValue, inheritedDefaultValue);
                        }
                        DataHelper.SetDataRowValue(dr, ffi.Name, inheritedDefaultValue);
                    }
                }
            }
        }


        /// <summary>
        /// Loads the default values into the DataRow.
        /// </summary>
        /// <param name="dr">Datarow to load</param>
        /// <param name="enableMacros">If true, macro expression as default values are allowed</param>
        public void LoadDefaultValues(DataRow dr, bool enableMacros = false)
        {
            // Set resolve type enum
            FormResolveTypeEnum resolveType = enableMacros ? FormResolveTypeEnum.AllFields : FormResolveTypeEnum.None;
            LoadDefaultValues(dr, resolveType);
        }


        /// <summary>
        /// Loads the default values into the DataRow.
        /// </summary>
        /// <param name="dr">Datarow to load</param>
        /// <param name="resolveType">Type of macro resolving</param>
        /// <param name="onlyVisible">If set True data are loaded for visible fields only; default value is False</param>
        public void LoadDefaultValues(DataRow dr, FormResolveTypeEnum resolveType, bool onlyVisible = false)
        {
            LoadDefaultValuesInternal(dr, resolveType, onlyVisible);
        }


        /// <summary>
        /// Loads the default values into the data container.
        /// </summary>
        /// <param name="container">Container to load</param>
        /// <param name="resolveType">Type of macro resolving</param>
        /// <param name="onlyVisible">If set True data are loaded for visible fields only; default value is False</param>
        public void LoadDefaultValues(IDataContainer container, FormResolveTypeEnum resolveType = FormResolveTypeEnum.None, bool onlyVisible = false)
        {
            LoadDefaultValuesInternal(container, resolveType, onlyVisible);
        }


        private void LoadDefaultValuesInternal(object container, FormResolveTypeEnum resolveType, bool onlyVisible)
        {
            if (container == null)
            {
                return;
            }

            // Get fields with some default value
            var fields = GetFields<FormFieldInfo>()
                    .Where(x => !String.IsNullOrEmpty(x.GetPropertyValue(FormFieldPropertyEnum.DefaultValue)));

            if (onlyVisible)
            {
                // Get only visible fields
                fields = fields.Where(x => x.Visible);
            }

            foreach (FormFieldInfo ffi in fields)
            {
                // Get default value, optionally resolved and converted to field's data type
                var value = ffi.GetTypedDefaultValue(resolveType, ContextResolver);

                // Set default value into given data container (IDataContainer or DataRow)
                SetDefaultValueInternal(container, ffi.Name, value);
            }
        }


        private void SetDefaultValueInternal(object container, string fieldName, object value)
        {
            if (value != null)
            {
                var dataCont = container as IDataContainer;
                if (dataCont != null)
                {
                    dataCont.SetValue(fieldName, value);
                }
                else
                {
                    var rowCont = container as DataRow;
                    if (rowCont != null)
                    {
                        DataHelper.SetDataRowValue(rowCont, fieldName, value);
                    }
                }
            }
        }


        /// <summary>
        /// Ensures the default values into the data container.
        /// </summary>
        /// <param name="container">Container to load</param>
        /// <param name="resolveType">Type of macro resolving</param>
        public bool EnsureDefaultValues(IDataContainer container, FormResolveTypeEnum resolveType = FormResolveTypeEnum.None)
        {
            bool valueEnsured = false;

            if (container != null)
            {
                // Get the fields with default value which doesn't allow empty value
                var fields = GetFields<FormFieldInfo>()
                    .Where(x => !string.IsNullOrEmpty(x.GetPropertyValue(FormFieldPropertyEnum.DefaultValue)) && !x.AllowEmpty);

                // Process all fields
                foreach (FormFieldInfo ffi in fields)
                {
                    // Get current value
                    object currentValue = container.GetValue(ffi.Name);

                    // If value is empty
                    if (DataHelper.IsEmpty(currentValue))
                    {
                        var value = ffi.GetTypedDefaultValue(resolveType, ContextResolver);

                        container.SetValue(ffi.Name, value);
                        valueEnsured = true;
                    }
                }
            }

            return valueEnsured;
        }


        /// <summary>
        /// Clones the object.
        /// </summary>
        public FormInfo Clone()
        {
            FormInfo fi = new FormInfo();
            fi.FormName = FormName;

            foreach (IDataDefinitionItem item in ItemsList)
            {
                fi.ItemsList.Add(item.Clone());
            }
            return fi;
        }

        #endregion


        #region "Loading from XML"

        /// <summary>
        /// Loads the definition from the XML node
        /// </summary>
        /// <param name="element">XML node</param>
        public override void LoadFromXmlElement(XmlElement element)
        {
            // Get form name
            if (element != null)
            {
                FormName = element.GetAttribute("name");
            }

            base.LoadFromXmlElement(element);
        }


        /// <summary>
        /// Creates a new field definition object
        /// </summary>
        protected override IDataDefinitionItem NewFieldDefinition()
        {
            return new FormFieldInfo();
        }


        /// <summary>
        /// Loads the item from the XML node
        /// </summary>
        /// <param name="node">XML node</param>
        protected override IDataDefinitionItem LoadItem(XmlNode node)
        {
            // Form Category Info
            if (node.Name.Equals("category", StringComparison.OrdinalIgnoreCase))
            {
                return LoadCategory(node);
            }

            return base.LoadItem(node);
        }


        /// <summary>
        /// Loads the category from the XML node
        /// </summary>
        /// <param name="node">XML node</param>
        private IDataDefinitionItem LoadCategory(XmlNode node)
        {
            //Initialize FormCategoryInfo from XML
            var fci = NewCategoryDefinition();
            if (fci != null)
            {
                fci.LoadFromXmlNode(node);

                ItemsList.Add(fci);
            }

            return fci;
        }


        /// <summary>
        /// Creates a new category definition object
        /// </summary>
        protected virtual IDataDefinitionItem NewCategoryDefinition()
        {
            return new FormCategoryInfo();
        }


        /// <summary>
        /// Loads the field from the XML node
        /// </summary>
        /// <param name="node">XML node</param>
        protected override IDataDefinitionItem LoadField(XmlNode node)
        {
            var ffi = (FormFieldInfo)base.LoadField(node);
            if (ffi != null)
            {
                // Check if FFI uses HtmlArea
                if (FormHelper.IsFieldOfType(ffi, FormFieldControlTypeEnum.HtmlAreaControl))
                {
                    mUsesHtmlArea = true;
                }
            }

            return ffi;
        }


        /// <summary>
        /// Creates data container based on this form definition
        /// </summary>
        /// <param name="loadDefaultValues">If true, default values are loaded to the data container</param>
        public IDataContainer CreateDataContainer(bool loadDefaultValues = true)
        {
            var dc = new DataContainer();

            foreach (var columnName in GetColumnNames())
            {
                dc.SetValue(columnName, null);
            }

            if (loadDefaultValues)
            {
                LoadDefaultValues(dc);
            }

            return dc;
        }

        #endregion
    }
}