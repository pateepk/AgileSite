using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Provides helper methods for biz forms.
    /// </summary>
    public class BizFormHelper : AbstractHelper<BizFormHelper>
    {
        /// <summary>
        /// <para>
        /// Creates a new biz form by creating the underlying database table and setting the corresponding <see cref="BizFormInfo"/> and <see cref="DataClassInfo"/>.
        /// The database table name is prefixed using <see cref="GetFormTablePrefix"/> and processed by <see cref="GetFullTableName"/>
        /// which truncates excessive table names.
        /// </para>
        /// <para>
        /// If <paramref name="formName"/> is not specified or its value equals <see cref="InfoHelper.CODENAME_AUTOMATIC"/>,
        /// a new code name is created based on display name.
        /// </para>
        /// <para>
        /// The form name is sanitized via <see cref="ValidationHelper.GetIdentifier(object)"/>. The form name is used as table name
        /// if <paramref name="tableName"/> is not specified or its value equals to <see cref="InfoHelper.CODENAME_AUTOMATIC"/>.
        /// </para>
        /// </summary>
        /// <param name="formDisplayName">Display name of the form.</param>
        /// <param name="formName">Name of the form, or null.</param>
        /// <param name="tableName">Table name to store form data, or null.</param>
        /// <param name="siteInfo">Site of the form.</param>
        /// <returns>Returns the new biz form.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formDisplayName"/> or <paramref name="siteInfo"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="formDisplayName"/> is an empty string.</exception>
        /// <exception cref="BizFormTableNameNotUniqueException">Thrown when biz form table of specified name already exists.</exception>
        /// <exception cref="BizFormException">Thrown when an exception during database table creation, <see cref="BizFormInfo"/> creation or <see cref="DataClassInfo"/> creation occurs. See the inner exception for details.</exception>
        /// <remarks>
        /// The resulting <see cref="BizFormInfo"/> has <see cref="BizFormInfo.FormEmailAttachUploadedDocs"/> and <see cref="BizFormInfo.FormLogActivity"/> enabled by default.
        /// </remarks>
        public static BizFormInfo Create(string formDisplayName, string formName, string tableName, SiteInfo siteInfo)
        {
            return HelperObject.CreateInternal(formDisplayName, formName, tableName, siteInfo);
        }


        /// <summary>
        /// Gets prefix for biz form table name. The prefix is a string in format <c>Form_[siteName]_</c>.
        /// The site name is sanitized via <see cref="ValidationHelper.GetIdentifier(object)"/>.
        /// </summary>
        /// <param name="siteName">Site name to derive the prefix from.</param>
        /// <returns>Returns prefix for biz form table name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="siteName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="siteName"/> is an empty string.</exception>
        public static string GetFormTablePrefix(string siteName)
        {
            return HelperObject.GetFormTablePrefixInternal(siteName);
        }


        /// <summary>
        /// <para>
        /// Gets table name including prefix for biz form table. Both <paramref name="tableNamePrefix"/> and <paramref name="tableName"/> must be
        /// sanitized strings suitable for a table name.
        /// </para>
        /// <para>
        /// If the resulting table name is longer than 60 characters, it is truncated and appended an ordinal number to ensure uniqueness.
        /// </para>
        /// </summary>
        /// <param name="tableManager">Table manager to be used for uniqueness testing when truncating table name.</param>
        /// <param name="tableNamePrefix">Table name prefix from which to derive full table name.</param>
        /// <param name="tableName">Table name from which to derive full table name.</param>
        /// <returns>Returns full table name derived from a prefix and table name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableManager"/>, <paramref name="tableNamePrefix"/> or <paramref name="tableName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tableNamePrefix"/> or <paramref name="tableName"/> is an empty string.</exception>
        public static string GetFullTableName(TableManager tableManager, string tableNamePrefix, string tableName)
        {
            return HelperObject.GetFullTableNameInternal(tableManager, tableNamePrefix, tableName);
        }


        /// <summary>
        /// <para>
        /// Creates a new biz form by creating the underlying database table and setting the corresponding <see cref="BizFormInfo"/> and <see cref="DataClassInfo"/>.
        /// The database table name is prefixed using <see cref="GetFormTablePrefix"/> and processed by <see cref="GetFullTableName"/>
        /// which truncates excessive table names.
        /// </para>
        /// <para>
        /// If <paramref name="formName"/> is not specified or its value equals <see cref="InfoHelper.CODENAME_AUTOMATIC"/>,
        /// a new code name is created based on display name.
        /// </para>
        /// <para>
        /// The form name is sanitized via <see cref="ValidationHelper.GetIdentifier(object)"/>. The form name is used as table name
        /// if <paramref name="tableName"/> is not specified or its value equals to <see cref="InfoHelper.CODENAME_AUTOMATIC"/>.
        /// </para>
        /// </summary>
        /// <param name="formDisplayName">Display name of the form.</param>
        /// <param name="formName">Name of the form, or null.</param>
        /// <param name="tableName">Table name to store form data, or null.</param>
        /// <param name="siteInfo">Site of the form.</param>
        /// <returns>Returns the new biz form.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formDisplayName"/> or <paramref name="siteInfo"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="formDisplayName"/> is an empty string.</exception>
        /// <exception cref="BizFormTableNameNotUniqueException">Thrown when biz form table of specified name already exists.</exception>
        /// <exception cref="BizFormException">Thrown when an exception during database table creation, <see cref="BizFormInfo"/> creation or <see cref="DataClassInfo"/> creation occurs. See the inner exception for details.</exception>
        /// <remarks>
        /// The resulting <see cref="BizFormInfo"/> has <see cref="BizFormInfo.FormEmailAttachUploadedDocs"/> and <see cref="BizFormInfo.FormLogActivity"/> enabled by default.
        /// </remarks>
        protected virtual BizFormInfo CreateInternal(string formDisplayName, string formName, string tableName, SiteInfo siteInfo)
        {
            if (String.IsNullOrEmpty(formDisplayName))
            {
                throw formDisplayName == null ? new ArgumentNullException(nameof(formDisplayName)) : new ArgumentException("Form display name must be specified.", nameof(formDisplayName));
            }
            if (siteInfo == null)
            {
                throw new ArgumentNullException(nameof(siteInfo));
            }

            var bizFormInfo = CreateBizFormInfo(formDisplayName, formName, siteInfo);

            // Generate the table name
            if (String.IsNullOrEmpty(tableName) || (tableName == InfoHelper.CODENAME_AUTOMATIC))
            {
                tableName = bizFormInfo.FormName;
            }

            TableManager tm = new TableManager(null);
            tableName = GetFullTableName(tm, GetFormTablePrefix(siteInfo.SiteName), tableName);

            // TableName might not be unique if truncation was not performed
            if (tm.TableExists(tableName))
            {
                throw new BizFormTableNameNotUniqueException($"Table named '{tableName}' already exists. A unique table name must be provided.", tableName);
            }

            // If first letter of FormName is digit, add "PK" to beginning
            string primaryKey = BizFormInfoProvider.GenerateFormPrimaryKeyName(bizFormInfo.FormName);
            try
            {
                // Create new table in DB
                tm.CreateTable(tableName, primaryKey);
            }
            catch (Exception ex)
            {
                throw new BizFormException($"Creating table '{tableName}' for biz form '{formDisplayName}' failed. See the inner exception for details.", ex);
            }

            string className = BizFormConstants.BIZ_FORM_CLASS_NAMESPACE + "." + bizFormInfo.FormName;

            // Create the BizForm class
            DataClassInfo dci = BizFormInfoProvider.CreateBizFormDataClass(className, formDisplayName, tableName, primaryKey);
            try
            {
                // Create new bizform dataclass
                using (CMSActionContext context = new CMSActionContext())
                {
                    // Disable logging of tasks
                    context.DisableLogging();

                    DataClassInfoProvider.SetDataClassInfo(dci);
                }
            }
            catch (Exception ex)
            {
                CleanUpOnError(tableName, tm, dci);

                throw new BizFormException($"Setting data class for biz form '{formDisplayName}' failed. See the inner exception for details.", ex);
            }

            // Create new bizform
            bizFormInfo.FormClassID = dci.ClassID;
            try
            {
                BizFormInfoProvider.SetBizFormInfo(bizFormInfo);
            }
            catch (Exception ex)
            {
                CleanUpOnError(tableName, tm, dci, bizFormInfo);

                throw new BizFormException($"Setting biz form '{formDisplayName}' failed. See the inner exception for details.", ex);
            }

            return bizFormInfo;
        }


        private void CleanUpOnError(string tableName, TableManager tm, DataClassInfo dci = null, BizFormInfo bizForm = null)
        {
            if (tm.TableExists(tableName))
            {
                tm.DropTable(tableName);
            }
            if ((bizForm != null) && (bizForm.FormID > 0))
            {
                BizFormInfoProvider.DeleteBizFormInfo(bizForm);
            }
            if ((dci != null) && (dci.ClassID > 0))
            {
                DataClassInfoProvider.DeleteDataClassInfo(dci);
            }
        }


        /// <summary>
        /// <para>
        /// Creates a new <see cref="BizFormInfo"/> and initializes it using given display name, name and site ID.
        /// Does not set the info to the database.
        /// The method is used by <see cref="CreateInternal"/> to supply the <see cref="BizFormInfo"/> instance.
        /// </para>
        /// <para>
        /// If <paramref name="formName"/> is not specified or its value equals <see cref="InfoHelper.CODENAME_AUTOMATIC"/>,
        /// a new code name is created based on display name.
        /// </para>
        /// <para>
        /// The form name is sanitized via <see cref="ValidationHelper.GetIdentifier(object)"/> and can be safely used as part of the resulting table name.
        /// </para>
        /// </summary>
        /// <param name="formDisplayName">Display name of the form.</param>
        /// <param name="formName">Name of the form, or null.</param>
        /// <param name="siteInfo">Site of the form.</param>
        /// <returns>Returns a new <see cref="BizFormInfo"/> instance initialized with given values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formDisplayName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="formDisplayName"/> is an empty string.</exception>
        /// <remarks>
        /// The resulting <see cref="BizFormInfo"/> has <see cref="BizFormInfo.FormEmailAttachUploadedDocs"/> and <see cref="BizFormInfo.FormLogActivity"/> enabled by default.
        /// </remarks>
        protected virtual BizFormInfo CreateBizFormInfo(string formDisplayName, string formName, SiteInfo siteInfo)
        {
            if (String.IsNullOrEmpty(formDisplayName))
            {
                throw formDisplayName == null ? new ArgumentNullException(nameof(formDisplayName)) : new ArgumentException("Form display name must be specified.", nameof(formDisplayName));
            }

            var bizFormInfo = new BizFormInfo
            {
                FormDisplayName = formDisplayName,
                FormName = formName,
                FormSiteID = siteInfo.SiteID,
                FormEmailAttachUploadedDocs = true,
                FormItems = 0,
                FormLogActivity = true,
                FormDevelopmentModel = siteInfo.SiteIsContentOnly ? (int)FormDevelopmentModelEnum.Mvc : (int)FormDevelopmentModelEnum.WebForms
            };

            bizFormInfo.FormClearAfterSave = bizFormInfo.FormDevelopmentModel == (int)FormDevelopmentModelEnum.Mvc;
            bizFormInfo.Generalized.EnsureCodeName();

            string safeFormName = ValidationHelper.GetIdentifier(bizFormInfo.FormName);
            bizFormInfo.FormName = safeFormName;

            return bizFormInfo;
        }


        /// <summary>
        /// Gets prefix for biz form table name. The prefix is a string in format <c>Form_[siteName]_</c>.
        /// The site name is sanitized via <see cref="ValidationHelper.GetIdentifier(object)"/>.
        /// </summary>
        /// <param name="siteName">Site name to derive the prefix from.</param>
        /// <returns>Returns prefix for biz form table name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="siteName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="siteName"/> is an empty string.</exception>
        protected virtual string GetFormTablePrefixInternal(string siteName)
        {
            if (String.IsNullOrEmpty(siteName))
            {
                throw siteName == null ? new ArgumentNullException(nameof(siteName)) : new ArgumentException("Site name must be specified.", nameof(siteName));
            }

            return String.Format("Form_{0}_", ValidationHelper.GetIdentifier(siteName));
        }


        /// <summary>
        /// <para>
        /// Gets table name including prefix for biz form table. Both <paramref name="tableNamePrefix"/> and <paramref name="tableName"/> must be
        /// sanitized strings suitable for a table name.
        /// </para>
        /// <para>
        /// If the resulting table name is longer than 60 characters, it is truncated and appended an ordinal number to ensure uniqueness.
        /// </para>
        /// </summary>
        /// <param name="tableManager">Table manager to be used for uniqueness testing when truncating table name.</param>
        /// <param name="tableNamePrefix">Table name prefix from which to derive full table name.</param>
        /// <param name="tableName">Table name from which to derive full table name.</param>
        /// <returns>Returns full table name derived from a prefix and table name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableManager"/>, <paramref name="tableNamePrefix"/> or <paramref name="tableName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tableNamePrefix"/> or <paramref name="tableName"/> is an empty string.</exception>
        protected virtual string GetFullTableNameInternal(TableManager tableManager, string tableNamePrefix, string tableName)
        {
            if (tableManager == null)
            {
                throw new ArgumentNullException(nameof(tableManager));
            }
            if (String.IsNullOrEmpty(tableNamePrefix))
            {
                throw tableNamePrefix == null ? new ArgumentNullException(nameof(tableNamePrefix)) : new ArgumentException("Table name prefix must be specified.", nameof(tableNamePrefix));
            }
            if (String.IsNullOrEmpty(tableName))
            {
                throw tableName == null ? new ArgumentNullException(nameof(tableName)) : new ArgumentException("Table name must be specified.", nameof(tableName));
            }

            var resultingFullTableName = tableNamePrefix + tableName;

            // Table names longer than 60 characters are truncated and appended an ordinal number to ensure uniqueness
            if (resultingFullTableName.Length > 60)
            {
                string tmpTableName = resultingFullTableName.Substring(0, 59);
                int x = 1;
                do
                {
                    resultingFullTableName = tmpTableName + x;
                    x++;
                } while (tableManager.TableExists(resultingFullTableName));
            }

            return resultingFullTableName;
        }
    }
}
