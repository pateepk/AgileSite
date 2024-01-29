using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Class providing BizFormInfo management.
    /// </summary>
    public class BizFormInfoProvider : AbstractInfoProvider<BizFormInfo, BizFormInfoProvider>
    {
        #region "Variables"

        // License bizform count
        private static Hashtable licBizForms = new Hashtable();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public BizFormInfoProvider()
            : base(null, new HashtableSettings
                {
                    ID = true,
                    Name = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns the BizFormInfo structure for the specified class.
        /// </summary>
        /// <param name="classId">Class ID</param>
        public static BizFormInfo GetBizFormInfoForClass(int classId)
        {
            return ProviderObject.GetObjectQuery().WhereEquals("FormClassID", classId).TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        /// <param name="siteId">Site ID</param>
        public static BizFormInfo GetBizFormInfoByGUID(Guid guid, int siteId)
        {
            return ProviderObject.GetBizFormInfoInternal(guid, siteId);
        }


        /// <summary>
        /// Returns the BizFormInfo structure for the specified bizForm.
        /// </summary>
        /// <param name="bizFormId">BizForm ID</param>
        public static BizFormInfo GetBizFormInfo(int bizFormId)
        {
            return ProviderObject.GetBizFormInfoInternal(bizFormId);
        }


        /// <summary>
        /// Returns the BizFormInfo structure for the specified form and site.
        /// </summary>
        /// <param name="formName">Form name</param>
        /// <param name="siteId">Site ID</param>
        public static BizFormInfo GetBizFormInfo(string formName, int siteId)
        {
            return ProviderObject.GetBizFormInfoInternal(formName, siteId);
        }


        /// <summary>
        /// Returns the BizFormInfo structure for the specified form and site.
        /// </summary>
        /// <param name="formName">Form name</param>
        /// <param name="siteName">Site name</param>
        public static BizFormInfo GetBizFormInfo(string formName, string siteName)
        {
            // Get the site
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si == null)
            {
                return null;
            }

            return GetBizFormInfo(formName, si.SiteID);
        }


        /// <summary>
        /// Sets (updates or inserts) specified bizForm.
        /// </summary>
        /// <param name="bizForm">BizForm to set</param>
        public static void SetBizFormInfo(BizFormInfo bizForm)
        {
            ProviderObject.SetBizFormInfoInternal(bizForm);
        }


        /// <summary>
        /// Clear hashtable.
        /// </summary>
        private static void ClearBizHash()
        {
            licBizForms.Clear();
        }


        /// <summary>
        /// Deletes specified bizForm.
        /// </summary>
        /// <param name="bizFormObj">BizForm object</param>
        public static void DeleteBizFormInfo(BizFormInfo bizFormObj)
        {
            ProviderObject.DeleteBizFormInfoInternal(bizFormObj);
        }


        /// <summary>
        /// Deletes specified bizForm.
        /// </summary>
        /// <param name="bizFormId">BizForm ID</param>
        public static void DeleteBizFormInfo(int bizFormId)
        {
            BizFormInfo bizFormObj = GetBizFormInfo(bizFormId);
            DeleteBizFormInfo(bizFormObj);
        }


        /// <summary>
        /// Returns a query for all the BizFormInfo objects.
        /// </summary>
        public static ObjectQuery<BizFormInfo> GetBizForms()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns a query for all the BizFormInfo objects specified by site ID.
        /// </summary>
        /// <param name="siteID">Site ID</param>
        public static ObjectQuery<BizFormInfo> GetBizForms(int siteID)
        {
            return ProviderObject.GetBizFormsInternal(siteID);
        }


        /// <summary>
        /// Creates bizform data class definition based on the given parameters.
        /// </summary>
        /// <param name="className">Form class name</param>
        /// <param name="displayName">Form display name</param>
        /// <param name="tableName">Form table name</param>
        /// <param name="primaryKeyName">Table primary key column name</param>
        /// <param name="fields">List of additional fields</param>
        public static DataClassInfo CreateBizFormDataClass(string className, string displayName, string tableName, string primaryKeyName, List<FormFieldInfo> fields = null)
        {
            // Create new class info
            var ci = new FormClassInfo();

            ci.ClassIsDocumentType = false;
            ci.ClassIsCoupledClass = true;
            ci.ClassShowAsSystemTable = false;
            ci.ClassIsForm = true;
            ci.ClassDisplayName = displayName;
            ci.ClassTableName = tableName;

            // Create unique code name
            ci.ClassName = className;
            ci.Generalized.EnsureUniqueCodeName();

            // Update definition
            ci.ClassFormDefinition = GetDefinitionXML(primaryKeyName, fields);

           return ci;
        }


        /// <summary>
        /// Gets form definition XML for given primary key and custom fields
        /// </summary>
        /// <param name="primaryKeyName">Primary key name</param>
        /// <param name="fields">Custom fields</param>
        private static string GetDefinitionXML(string primaryKeyName, IEnumerable<FormFieldInfo> fields)
        {
            string formDefinition = FormHelper.GetBasicFormDefinition(primaryKeyName);
            FormInfo fi = new FormInfo(formDefinition);

            // Inserted / updated fields
            FormFieldInfo ffi = GetTimeField("FormInserted", "Form inserted");
            fi.AddFormItem(ffi);

            ffi = GetTimeField("FormUpdated", "Form updated");
            fi.AddFormItem(ffi);

            // Add additional fields
            if (fields != null)
            {
                foreach (var f in fields)
                {
                    fi.AddFormItem(f);
                }
            }

            var definition = fi.GetXmlDefinition();
            return definition;
        }


        /// <summary>
        /// Returns new form field info of the datetime datatype indicating when form data were changed(inserted/updated).
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="fieldCaption">Field caption</param>
        private static FormFieldInfo GetTimeField(string fieldName, string fieldCaption)
        {
            FormFieldInfo ffi = null;

            if ((!string.IsNullOrEmpty(fieldName)) && (!string.IsNullOrEmpty(fieldCaption)))
            {
                // Create FormFieldInfo
                ffi = new FormFieldInfo();
                ffi.Name = fieldName;
                ffi.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, fieldCaption);
                ffi.DataType = FieldDataType.DateTime;
                ffi.SetPropertyValue(FormFieldPropertyEnum.DefaultValue, string.Empty);
                ffi.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, string.Empty);
                ffi.FieldType = FormFieldControlTypeEnum.CustomUserControl;
                ffi.Settings.Add("controlname", FormHelper.GetFormFieldControlTypeString(FormFieldControlTypeEnum.CalendarControl).ToLowerInvariant());
                ffi.PrimaryKey = false;
                ffi.System = true;
                ffi.Visible = false;
                ffi.Size = 0;
                ffi.Precision = DataTypeManager.GetDataType(TypeEnum.Field, FieldDataType.DateTime).DefaultPrecision;
                ffi.AllowEmpty = false;
            }

            return ffi;
        }


        /// <summary>
        /// Returns DataSet with roles that are allowed for specified form.
        /// </summary>        
        /// <param name="formId">Form ID</param>
        public static InfoDataSet<RoleInfo> GetFormAuthorizedRoles(int formId)
        {
            return ProviderObject.GetFormAuthorizedRolesInternal(formId);
        }


        /// <summary>
        /// License version check.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature</param>
        /// <param name="action">Action</param>
        public static bool LicenseVersionCheck(string domain, FeatureEnum feature, ObjectActionEnum action)
        {
            // Parse domain name to remove port etc
            if (domain != null)
            {
                domain = LicenseKeyInfoProvider.ParseDomainName(domain);
            }
            
            int versionLimitations = LicenseKeyInfoProvider.VersionLimitations(domain, feature, action != ObjectActionEnum.Insert);

            if (versionLimitations == 0)
            {
                return true;
            }

            if (feature != FeatureEnum.BizForms)
            {
                return true;
            }

            if (licBizForms[domain] == null)
            {
                licBizForms[domain] = GetBizForms().OnSite(LicenseHelper.GetSiteIDbyDomain(domain)).GetCount();
            }

            try
            {
                // Try add
                if (action == ObjectActionEnum.Insert)
                {
                    if (versionLimitations < ValidationHelper.GetInteger(licBizForms[domain], -1) + 1)
                    {
                        return false;
                    }
                }

                // Get status
                if (action == ObjectActionEnum.Edit)
                {
                    if (versionLimitations < ValidationHelper.GetInteger(licBizForms[domain], 0))
                    {
                        return false;
                    }
                }
            }
            catch
            {
                ClearBizHash();
                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks the license.
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, current domain name is used</param>
        public static bool CheckLicense(ObjectActionEnum action = ObjectActionEnum.Edit, string domainName = null)
        {
            domainName = domainName ?? RequestContext.CurrentDomain;

            if (!LicenseVersionCheck(domainName, FeatureEnum.BizForms, action))
            {
                LicenseHelper.GetAllAvailableKeys(FeatureEnum.BizForms);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks the license for insert for a new bizform or for edit in other cases.
        /// </summary>
        public static void CheckLicense(BizFormInfo bizForm)
        {
            var action = ObjectActionEnum.Edit;

            if ((bizForm != null) && (bizForm.FormID <= 0))
            {
                action = ObjectActionEnum.Insert;
            }

            CheckLicense(action);
        }


        /// <summary>
        /// Refresh bizform items count.
        /// </summary>
        /// <param name="formName">Form name</param>
        /// <param name="siteId">Site ID</param>
        public static void RefreshDataCount(string formName, int siteId)
        {
            BizFormInfo bzi = GetBizFormInfo(formName, siteId);

            ProviderObject.RefreshDataCountInternal(bzi);
        }


        /// <summary>
        /// Refresh bizform items count.
        /// </summary>
        /// <param name="bizForm">BizForm info object</param>
        public static void RefreshDataCount(BizFormInfo bizForm)
        {
            ProviderObject.RefreshDataCountInternal(bizForm);
        }


        /// <summary>
        /// Delete all bizform data.
        /// </summary>
        /// <param name="formName">Form name</param>
        /// <param name="siteId">Site ID</param>
        public static void DeleteData(string formName, int siteId)
        {
            ProviderObject.DeleteDataInternal(formName, siteId);
        }


        /// <summary>
        /// Takes code name of the form and generates a primary key name from it. Generated primary key is safe to use in the 
        /// database. It does not contain forbidden characters, does not start with a number, etc.
        /// </summary>
        /// <param name="formCodeName">Code name of the FormInfo</param>
        /// <returns>Primary key name of the form</returns>
        /// <exception cref="ArgumentException"><paramref name="formCodeName"/> is null or empty</exception>
        public static string GenerateFormPrimaryKeyName(string formCodeName)
        {
            return ProviderObject.GenerateFormPrimaryKeyNameInternal(formCodeName);
        }

        #endregion


        #region "BizForm file methods"

        /// <summary>
        /// Deletes all files of specified BizForm record.
        /// </summary>
        /// <param name="classFormDefinition">BizForm class form definition</param>
        /// <param name="item">BizForm item</param>
        /// <param name="siteName">SiteName</param>
        public static void DeleteBizFormRecordFiles(string classFormDefinition, BizFormItem item, string siteName)
        {
            if (string.IsNullOrEmpty(siteName))
            {
                return;
            }

            // Get field names with uploader control type
            string[] columns = GetBizFormFileColumns(classFormDefinition);
            if ((columns != null) && (columns.Length > 0))
            {
                // Path to BizForm files in file system
                string filesFolderPath = FormHelper.GetBizFormFilesFolderPath(siteName);
                foreach (string column in columns)
                {
                    // Get file name in format "[guid].[extension]/[originalfilename].[extension]"
                    string fileName = ValidationHelper.GetString(item.GetValue(column), "");
                    // Delete file from file system
                    DeleteBizFormFile(fileName, filesFolderPath, siteName);
                }
            }
        }


        /// <summary>
        /// Deletes all files of the specified BizForm.
        /// </summary>
        /// <param name="className">BizForm class name</param>
        /// <param name="column">Name of the column where file information is stored, optional</param>
        /// <param name="siteId">Site ID</param>
        public static void DeleteBizFormFiles(string className, string column, int siteId)
        {
            ProviderObject.DeleteBizFormFilesInternal(className, column, siteId);
        }


        /// <summary>
        /// Returns names of the columns where BizForm file information is stored.
        /// </summary>
        /// <param name="classFormDefinition">BizForm's class form definition</param>
        public static string[] GetBizFormFileColumns(string classFormDefinition)
        {
            string[] result = null;

            FormInfo fi = new FormInfo(classFormDefinition);
            // Get fields with uploader control type
            var fields = fi.GetFields(FormFieldControlTypeEnum.UploadControl);
            if ((fields != null) && (fields.Any()))
            {
                result = fields.Select(x => x.Name).ToArray();
            }

            return result;
        }


        /// <summary>
        /// Deletes uploaded file from file system.
        /// </summary>
        /// <param name="fileName">File name in format "[guid].[extension]/[originalfilename].[extension]"</param>
        /// <param name="directoryPath">Directory path</param>
        /// <param name="siteName">Name of the site to which is bizform assigned. Is used for web farms</param>
        private static void DeleteBizFormFile(string fileName, string directoryPath, string siteName)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                return;
            }
            // Get GUID file name - "[guid].[extension]"
            fileName = fileName.Split('/')[0];

            if (!String.IsNullOrEmpty(fileName))
            {
                if (Directory.Exists(directoryPath))
                {
                    DirectoryInfo di = DirectoryInfo.New(directoryPath);

                    // Select all files with the same name ('<fileName>')
                    FileInfo[] filesInfos = di.GetFiles(fileName);
                    if (filesInfos != null)
                    {
                        // Delete all selected files
                        foreach (FileInfo file in filesInfos)
                        {
                            try
                            {
                                File.Delete(file.FullName);
                            }
                            catch (Exception ex)
                            {
                                // Log the exception
                                EventLogProvider.LogException("CMS.FormEngine.BizFormInfoProvider", "DeleteBizFormFile", ex);
                            }
                        }
                    }
                }
            }

            WebFarmHelper.CreateTask(FormTaskType.DeleteBizFormFile, "deletebizformfile", siteName, fileName);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        /// <param name="siteId">Site ID</param>
        protected BizFormInfo GetBizFormInfoInternal(Guid guid, int siteId)
        {
            return GetInfoByGuid(guid, siteId);
        }


        /// <summary>
        /// Returns the BizFormInfo structure for the specified bizForm.
        /// </summary>
        /// <param name="bizFormId">BizForm ID</param>
        protected BizFormInfo GetBizFormInfoInternal(int bizFormId)
        {
            return GetInfoById(bizFormId, useHashtable: true);
        }


        /// <summary>
        /// Returns the BizFormInfo structure for the specified form and site.
        /// </summary>
        /// <param name="formName">Form name</param>
        /// <param name="siteId">Site ID</param>
        protected BizFormInfo GetBizFormInfoInternal(string formName, int siteId)
        {
            if (string.IsNullOrEmpty(formName) || (siteId <= 0))
            {
                return null;
            }

            return GetInfoByCodeName(formName, siteId, true);
        }


        /// <summary>
        /// Sets (updates or inserts) specified bizForm.
        /// </summary>
        /// <param name="bizForm">BizForm to set</param>
        protected void SetBizFormInfoInternal(BizFormInfo bizForm)
        {
            SetInfo(bizForm);

            // When bizform is restored from recycle bin, version is restored or etc., it's needed to recalculate number of records.
            RefreshDataCountInternal(bizForm);

            ClearBizHash();
        }


        /// <summary>
        /// Deletes specified bizForm.
        /// </summary>
        /// <param name="bizFormObj">BizForm object</param>
        protected void DeleteBizFormInfoInternal(BizFormInfo bizFormObj)
        {
            if (bizFormObj != null)
            {
                // Delete object
                ProviderObject.DeleteInfo(bizFormObj);

                // Delete form class
                DataClassInfo ci = DataClassInfoProvider.GetDataClassInfo(bizFormObj.FormClassID);
                if (ci != null)
                {
                    // Delete all bizform files if uploader is used in the form
                    var className = ci.ClassName;
                    DeleteBizFormFiles(className, null, bizFormObj.FormSiteID);

                    using (CMSActionContext context = new CMSActionContext())
                    {
                        // Disable logging of tasks
                        context.DisableLogging();

                        ci.Generalized.DeleteObject();
                    }
                }
            }

            ClearBizHash();
        }


        /// <summary>
        /// Returns a query for all the BizFormInfo objects specified by site ID.
        /// </summary>
        /// <param name="siteID">SiteId</param>
        protected virtual ObjectQuery<BizFormInfo> GetBizFormsInternal(int siteID)
        {
            return GetObjectQuery().WhereEquals("FormSiteID", siteID);
        }


        /// <summary>
        /// Returns DataSet with roles that are allowed for specified form.
        /// </summary>        
        /// <param name="formId">Form ID</param>
        protected InfoDataSet<RoleInfo> GetFormAuthorizedRolesInternal(int formId)
        {
            // Prepare the parameters
            var parameters = new QueryDataParameters();
            parameters.Add("@FormID", formId);
            parameters.EnsureDataSet<RoleInfo>();

            return ConnectionHelper.ExecuteQuery("cms.form.selectformroles", parameters).As<RoleInfo>();
        }


        /// <summary>
        /// Refresh bizform items count.
        /// </summary>
        /// <param name="bzi">BizFormInfo</param>
        protected void RefreshDataCountInternal(BizFormInfo bzi)
        {
            if (bzi == null)
            {
                return;
            }

            DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(bzi.FormClassID);
            if (dci == null)
            {
                return;
            }

            // Get the data
            string tableName = dci.ClassTableName;
            if (tableName != null)
            {
                tableName = SqlHelper.RemoveOwner(tableName);
                if (tableName != null)
                {
                    tableName = tableName.Replace("]", "]]");
                }
            }

            DataSet ds = ConnectionHelper.ExecuteQuery("UPDATE CMS_Form SET FormItems = (SELECT COUNT(*) FROM [" + tableName + "]) WHERE FormID =" + bzi.FormID + "; SELECT FormItems FROM CMS_Form WHERE FormID =" + bzi.FormID, null, QueryTypeEnum.SQLQuery);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Get data count
                int count = ValidationHelper.GetInteger(ds.Tables[0].Rows[0][0], 0);

                // Update count
                bzi.FormItems = count;
            }
        }


        /// <summary>
        /// Delete all bizform data.
        /// </summary>
        /// <param name="formName">Form name</param>
        /// <param name="siteId">Site ID</param>
        protected void DeleteDataInternal(string formName, int siteId)
        {
            // Get bizform
            BizFormInfo bzi = GetBizFormInfo(formName, siteId);

            if (bzi == null)
            {
                return;
            }

            DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(bzi.FormClassID);

            if (dci == null)
            {
                return;
            }

            // Delete all BizForm files if uploader is used in the form
            DeleteBizFormFiles(dci.ClassName, null, siteId);

            // Get the data
            string tableName = dci.ClassTableName;
            if (tableName != null)
            {
                tableName = SqlHelper.RemoveOwner(tableName);
                if (tableName != null)
                {
                    tableName = tableName.Replace("]", "]]");
                }
            }

            ConnectionHelper.ExecuteQuery("DELETE FROM [" + tableName + "]", null, QueryTypeEnum.SQLQuery);

            bzi.FormItems = 0;
            SetBizFormInfo(bzi);
        }


        /// <summary>
        /// Deletes all files of the specified BizForm.
        /// </summary>
        /// <param name="className">BizForm class name</param>
        /// <param name="column">Name of the column where file information is stored, optional</param>
        /// <param name="siteId">Site ID</param>
        protected void DeleteBizFormFilesInternal(string className, string column, int siteId)
        {
            // Get class object
            DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(className);
            if (dci == null)
            {
                return;
            }

            string[] columnArr;
            
            if (string.IsNullOrEmpty(column))
            {
                // Get field names with uploader control type
                columnArr = GetBizFormFileColumns(dci.ClassFormDefinition);
            }
            else
            {
                columnArr = new string[1];
                columnArr[0] = column;
            }

            if (columnArr == null)
            {
                return;
            }

            string columns = String.Empty;
            string where = String.Empty;

            // Prepare where condition
            foreach (string col in columnArr)
            {
                columns += "[" + col + "],";
                where += "(NOT [" + col + "] IS NULL) OR ";
            }
            // Remove last ',' and OR
            columns = columns.TrimEnd(',');
            where = where.Remove(where.Length - 4);

            DataSet ds = ConnectionHelper.ExecuteQuery(dci.ClassName + ".selectall", null, where, null, 0, columns);

            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return;
            }

            // Get bizform site object
            SiteInfo site = SiteInfoProvider.GetSiteInfo(siteId);
            if (site == null)
            {
                return;
            }

            // Path to BizForm files in file system
            string filesFolderPath = FormHelper.GetBizFormFilesFolderPath(site.SiteName);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                foreach (string col in columnArr)
                {
                    // Get file name in format "[guid].[extension]/[originalfilename].[extension]"
                    string fileName = DataHelper.GetStringValue(row, col);
                    // Delete file from file system
                    DeleteBizFormFile(fileName, filesFolderPath, site.SiteName);
                }
            }
        }


        /// <summary>
        /// Takes code name of the form and generates a primary key name from it. Generated primary key is safe to use in the 
        /// database. It does not contain forbidden characters, does not start with a number, etc.
        /// </summary>
        /// <param name="formCodeName">Code name of the FormInfo</param>
        /// <returns>Primary key name of the form</returns>
        /// <exception cref="ArgumentException"><paramref name="formCodeName"/> is null or empty</exception>
        protected string GenerateFormPrimaryKeyNameInternal(string formCodeName)
        {
            if (string.IsNullOrEmpty(formCodeName))
            {
                throw new ArgumentException("[BizFormInfoProvider.GenerateFormPrimaryKeyNameInternal]: formCodeName cannot be null or empty", "formCodeName");
            }

            string primaryKeyName = ValidationHelper.GetIdentifier(formCodeName);

            if (char.IsDigit(primaryKeyName[0]))
            {
                primaryKeyName = "PK" + primaryKeyName;
            }

            return primaryKeyName + "ID";
        }

        #endregion
    }
}