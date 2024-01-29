using System;
using System.Xml;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.FormEngine
{
    using TypedDataSet = InfoDataSet<FormUserControlInfo>;

    /// <summary>
    /// Class to provide FormUserControl management.
    /// </summary>
    public class FormUserControlInfoProvider : AbstractInfoProvider<FormUserControlInfo, FormUserControlInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public FormUserControlInfoProvider()
            : base(FormUserControlInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
                GUID = true
            })
        {
        }

        #endregion


        #region "Private fields"

        /// <summary>
        /// Form user controls directory.
        /// </summary>
        public const string FormUserControlsDirectory = "~/CMSFormControls";

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns dataset with all FormUserControlInfo objects.
        /// </summary>        
        public static ObjectQuery<FormUserControlInfo> GetFormUserControls()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns dataset with all FormUserControlInfo objects.
        /// </summary>        
        [Obsolete("Use method GetFormUserControls() instead")]
        public static TypedDataSet GetAllFormUserControls()
        {
            return GetFormUserControls().OrderBy("UserControlDisplayName").TypedResult;
        }


        /// <summary>
        /// Returns dataset with all FormUserControlInfo objects.
        /// </summary>
        /// <param name="orderBy">Order by statement to use</param>
        /// <param name="where">Where condition to filter data</param>
        [Obsolete("Use method GetFormUserControls() instead")]
        public static TypedDataSet GetFormUserControls(string where, string orderBy)
        {
            return ProviderObject.GetFormUserControlsInternal(where, orderBy);
        }


        /// <summary>
        /// Returns dataset with all FormUserControlInfo objects.
        /// </summary>
        /// <param name="columns">Selected columns</param>
        /// <param name="orderBy">Order by statement to use</param>
        /// <param name="where">Where condition to filter data</param>
        [Obsolete("Use method GetFormUserControls() instead")]
        public static TypedDataSet GetFormUserControls(string columns, string where, string orderBy)
        {
            return ProviderObject.GetFormUserControlsInternal(columns, where, orderBy);
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static FormUserControlInfo GetFormUserControlInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the FormUserControlInfo structure for the specified FormUserControl.
        /// </summary>
        /// <param name="userControlCodeName">Form user control code name</param>
        public static FormUserControlInfo GetFormUserControlInfo(string userControlCodeName)
        {
            return ProviderObject.GetInfoByCodeName(userControlCodeName);
        }


        /// <summary>
        /// Returns the FormUserControlInfo structure for the specified FormUserControl.
        /// </summary>
        /// <param name="userControlId">Form user control ID</param>
        public static FormUserControlInfo GetFormUserControlInfo(int userControlId)
        {
            return ProviderObject.GetInfoById(userControlId);
        }


        /// <summary>
        /// Sets the specified FormUserControl info data.
        /// </summary>
        /// <param name="infoObj">FormUserControlInfo object to set (save as new or update existing)</param>
        public static void SetFormUserControlInfo(FormUserControlInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified FormUserControl.
        /// </summary>
        /// <param name="infoObj">FormUserControl object to delete</param>
        public static void DeleteFormUserControlInfo(FormUserControlInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified FormUserControl.
        /// </summary>
        /// <param name="userControlID">FormUserControl ID</param>
        public static void DeleteFormUserControlInfo(int userControlID)
        {
            FormUserControlInfo infoObj = GetFormUserControlInfo(userControlID);
            DeleteFormUserControlInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified FormUserControl.
        /// </summary>
        /// <param name="userControlCodeName">FormUserControl code name</param>
        public static void DeleteFormUserControlInfo(string userControlCodeName)
        {
            FormUserControlInfo fu = GetFormUserControlInfo(userControlCodeName);
            DeleteFormUserControlInfo(fu);
        }


        /// <summary>
        /// Returns virtual path to the form user control file.
        /// </summary>
        /// <param name="controlInfo">Form user control info object</param>
        [Obsolete("Use FormUserControlLoader.LoadFormControl instead.")]
        public static string GetFormUserControlUrl(FormUserControlInfo controlInfo)
        {
            string url = controlInfo.UserControlFileName;
            if (url.StartsWithCSafe("~/"))
            {
                return SystemContext.ApplicationPath.TrimEnd('/') + url.TrimStart('~');
            }

            url = FormUserControlsDirectory + "/" + url;

            return url;
        }


        /// <summary>
        /// Returns string array with two values - default data type and default size. If control is not found then "text" and "500" are returned.
        /// </summary>
        /// <param name="controlName">Control code name</param>
        /// <returns>Returns string array with two values. First is default data type for control. Second is default size.</returns>
        public static string[] GetUserControlDefaultDataType(string controlName)
        {
            string[] defaultDataType = new string[2];

            FormUserControlInfo control = GetFormUserControlInfo(controlName);
            if (control != null)
            {
                defaultDataType[0] = control.UserControlDefaultDataType.ToLowerCSafe();
                defaultDataType[1] = ValidationHelper.GetString(control.UserControlDefaultDataTypeSize, "0");
            }
            else
            {
                defaultDataType[0] = "text";
                defaultDataType[1] = "500";
            }
            return defaultDataType;
        }


        /// <summary>
        /// Returns control type enum from integer representation.
        /// </summary>
        /// <param name="type">Type as integer</param>
        public static FormUserControlTypeEnum GetTypeEnum(int type)
        {
            switch (type)
            {
                case 0:
                    return FormUserControlTypeEnum.Input;

                case 1:
                    return FormUserControlTypeEnum.Multifield;

                case 2:
                    return FormUserControlTypeEnum.Selector;

                case 3:
                    return FormUserControlTypeEnum.Uploader;

                case 4:
                    return FormUserControlTypeEnum.Viewer;

                case 5:
                    return FormUserControlTypeEnum.Visibility;

                case 6:
                    return FormUserControlTypeEnum.Filter;

                case 7:
                    return FormUserControlTypeEnum.Captcha;
            }

            return FormUserControlTypeEnum.Unspecified;
        }


        /// <summary>
        /// Merges FormInfo of parent user control with default values of child user control.
        /// </summary>
        /// <param name="formInfo">Original form info</param>
        /// <param name="inheritedFormInfo">Inherited form info</param>
        /// <returns>Merged XML with parent control definition and child default values</returns>
        public static string MergeDefaultValues(string formInfo, string inheritedFormInfo)
        {
            if (inheritedFormInfo.StartsWithCSafe("<defaultvalues>", true))
            {
                // Parse XML for both form definitions
                XmlDocument xmlForm = new XmlDocument();
                xmlForm.LoadXml(formInfo);

                XmlDocument xmlDefaultValues = new XmlDocument();
                xmlDefaultValues.LoadXml(inheritedFormInfo);

                // Iterate through field nodes in alternative form definition
                var documentElement = xmlDefaultValues.DocumentElement;
                if (documentElement != null)
                {
                    foreach (XmlElement altField in documentElement.ChildNodes)
                    {
                        // Update default value attribute
                        XmlAttribute defValue = altField.Attributes["value"];
                        if (defValue != null)
                        {
                            altField.SetAttribute("defaultvalue", defValue.Value);
                            altField.Attributes.Remove(defValue);
                        }

                        // Process the field element
                        if (altField.LocalName.EqualsCSafe("field", true))
                        {
                            XmlAttribute nameAttr = altField.Attributes["name"];
                            if (nameAttr != null)
                            {
                                // Get field with the same column name from original definition
                                XmlNode orgField = TableManager.SelectFieldNode(xmlForm.DocumentElement, "column", nameAttr.Value);

                                if (orgField != null)
                                {
                                    // Merge element attributes
                                    XmlHelper.SetXmlNodeAttributes(orgField, XmlHelper.GetXmlAttributes(altField.Attributes));
                                }
                            }
                        }
                    }
                }

                return xmlForm.DocumentElement.OuterXml;
            }

            return FormHelper.MergeFormDefinitions(formInfo, inheritedFormInfo);
        }


        /// <summary>
        /// Deletes all child form controls of selected form control
        /// </summary>
        /// <param name="infoObj">Selected FormUserControl</param>
        private void DeleteChildUserControls(FormUserControlInfo infoObj)
        {
            if (infoObj != null)
            {
                var ids = GetFormUserControls().WhereEquals("UserControlParentID", infoObj.UserControlID).Column("UserControlID").GetListResult<int>();
                foreach (var id in ids)
                {
                    DeleteFormUserControlInfo(id);
                }
            }
        }


        /// <summary>
        /// Clears hash tables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        public static void Clear(bool logTasks)
        {
            ProviderObject.ClearHashtables(logTasks);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns dataset with all FormUserControlInfo objects.
        /// </summary>
        /// <param name="columns">Selected columns</param>
        /// <param name="orderBy">Order by statement to use</param>
        /// <param name="where">Where condition to filter data</param>
        [Obsolete("Use method GetFormUserControls() instead")]
        protected virtual TypedDataSet GetFormUserControlsInternal(string columns, string where, string orderBy)
        {
            return GetFormUserControls().Where(where).OrderBy(orderBy).Columns(columns).BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Returns dataset with all FormUserControlInfo objects.
        /// </summary>
        /// <param name="orderBy">Order by statement to use</param>
        /// <param name="where">Where condition to filter data</param>
        [Obsolete("Use method GetFormUserControls() instead")]
        protected virtual TypedDataSet GetFormUserControlsInternal(string where, string orderBy)
        {
            return GetFormUserControls().Where(where).OrderBy(orderBy).BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(FormUserControlInfo info)
        {
            base.SetInfo(info);

            // Clear cached form info to load new definition
            info.UserControlMergedParameters = null;

            // Clear cached form control definitions
            FormHelper.ClearFormControlParameters(true);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(FormUserControlInfo info)
        {
            DeleteChildUserControls(info);

            base.DeleteInfo(info);
        }

        #endregion
    }
}