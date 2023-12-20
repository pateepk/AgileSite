using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.OnlineForms;
using CMS.Search;
using CMS.SiteProvider;

[assembly: RegisterObjectType(typeof(BizFormInfo), BizFormInfo.OBJECT_TYPE)]

namespace CMS.OnlineForms
{
    /// <summary>
    /// <see cref="BizFormInfo"/> stores information about General, Autoresponder, Email notification and similar tabs.
    /// <see cref="FormClassInfo"/> stores the structure of the form. That means for example form definition (Fields tab) and search fields settings.
    /// <see cref="BizFormItem"/> stores the data that visitors fill on the website.
    /// </summary>
    public class BizFormInfo : AbstractInfo<BizFormInfo>
    {
        #region "Constants"

        /// <summary>
        /// Object type
        /// </summary>
        public const string BIZFORM_PREFIX = ImportExportHelper.BIZFORM_PREFIX;

        #endregion


        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.form";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BizFormInfoProvider), OBJECT_TYPE, "CMS.Form", "FormID", "FormLastModified", "FormGUID", "FormName", "FormDisplayName", null, "FormSiteID", null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONTENTMANAGEMENT),
                },
                ExcludedStagingColumns = new List<string>
                {
                    "FormItems"
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,
            DependsOn = new List<ObjectDependency> { new ObjectDependency("FormClassID", FormClassInfo.OBJECT_TYPE_FORM, ObjectDependencyEnum.Required) },
            ChildDependencyColumns = "FormClassID",
            ModuleName = "cms.form",
            Feature = FeatureEnum.BizForms,
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONTENTMANAGEMENT),
                },
            },
            HasMetaFiles = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            DeleteObjectWithAPI = true
        };

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            BizFormInfoProvider.DeleteBizFormInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BizFormInfoProvider.SetBizFormInfo(this);
        }

        #endregion


        #region "Variables"

        private SafeDictionary<int, string> mAuthorizedRoles;

        #endregion


        #region "Properties"

        /// <summary>
        /// Hashtable of roles the form is allowed for.
        /// RoleId is key and RoleName is value.
        /// </summary>
        public SafeDictionary<int, string> AuthorizedRoles
        {
            get
            {
                LoadRoles();

                return mAuthorizedRoles;
            }
        }


        /// <summary>
        /// Gets or sets items count.
        /// </summary>
        public virtual int FormItems
        {
            get
            {
                return GetIntegerValue("FormItems", 0);
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                SetValue("FormItems", value);
            }
        }


        /// <summary>
        /// Form display name.
        /// </summary>
        public virtual string FormDisplayName
        {
            get
            {
                return GetStringValue("FormDisplayName", "");
            }
            set
            {
                SetValue("FormDisplayName", value);
            }
        }


        /// <summary>
        /// Report fields.
        /// </summary>
        public virtual string FormReportFields
        {
            get
            {
                return GetStringValue("FormReportFields", "");
            }
            set
            {
                SetValue("FormReportFields", value);
            }
        }


        /// <summary>
        /// Form code name.
        /// </summary>
        public virtual string FormName
        {
            get
            {
                return GetStringValue("FormName", "");
            }
            set
            {
                SetValue("FormName", value);
            }
        }


        /// <summary>
        /// Form id.
        /// </summary>
        public virtual int FormID
        {
            get
            {
                return GetIntegerValue("FormID", 0);
            }
            set
            {
                SetValue("FormID", value);
            }
        }


        /// <summary>
        /// Redirect to url.
        /// </summary>
        public virtual string FormRedirectToUrl
        {
            get
            {
                return GetStringValue("FormRedirectToUrl", null);
            }
            set
            {
                SetValue("FormRedirectToUrl", value);
            }
        }


        /// <summary>
        /// Clear form.
        /// </summary>
        public virtual bool FormClearAfterSave
        {
            get
            {
                return GetBooleanValue("FormClearAfterSave", false);
            }
            set
            {
                SetValue("FormClearAfterSave", value);
            }
        }


        /// <summary>
        /// Display text.
        /// </summary>
        public virtual string FormDisplayText
        {
            get
            {
                return GetStringValue("FormDisplayText", null);
            }
            set
            {
                SetValue("FormDisplayText", value);
            }
        }


        /// <summary>
        /// From E-mail.
        /// </summary>
        public virtual string FormSendFromEmail
        {
            get
            {
                return GetStringValue("FormSendFromEmail", null);
            }
            set
            {
                SetValue("FormSendFromEmail", value);
            }
        }


        /// <summary>
        /// To E-mail.
        /// </summary>
        public virtual string FormSendToEmail
        {
            get
            {
                return GetStringValue("FormSendToEmail", null);
            }
            set
            {
                SetValue("FormSendToEmail", value);
            }
        }


        /// <summary>
        /// E-mail subject.
        /// </summary>
        public virtual string FormEmailSubject
        {
            get
            {
                return GetStringValue("FormEmailSubject", null);
            }
            set
            {
                SetValue("FormEmailSubject", value);
            }
        }


        /// <summary>
        /// Form notification e-mail template text.
        /// </summary>
        public virtual string FormEmailTemplate
        {
            get
            {
                return GetStringValue("FormEmailTemplate", null);
            }
            set
            {
                SetValue("FormEmailTemplate", value);
            }
        }


        /// <summary>
        /// Attach uploaded documents to notification e-mail.
        /// </summary>
        public virtual bool FormEmailAttachUploadedDocs
        {
            get
            {
                return GetBooleanValue("FormEmailAttachUploadedDocs", true);
            }
            set
            {
                SetValue("FormEmailAttachUploadedDocs", value);
            }
        }


        /// <summary>
        /// Class id.
        /// </summary>
        public virtual int FormClassID
        {
            get
            {
                return GetIntegerValue("FormClassID", 0);
            }
            set
            {
                SetValue("FormClassID", value);
            }
        }


        /// <summary>
        /// Site id.
        /// </summary>
        public virtual int FormSiteID
        {
            get
            {
                return GetIntegerValue("FormSiteID", 0);
            }
            set
            {
                SetValue("FormSiteID", value);
            }
        }


        /// <summary>
        /// Submit button text.
        /// </summary>
        public virtual string FormSubmitButtonText
        {
            get
            {
                return GetStringValue("FormSubmitButtonText", "");
            }
            set
            {
                SetValue("FormSubmitButtonText", value);
            }
        }


        /// <summary>
        /// Submit image button.
        /// </summary>
        public virtual string FormSubmitButtonImage
        {
            get
            {
                return GetStringValue("FormSubmitButtonImage", "");
            }
            set
            {
                SetValue("FormSubmitButtonImage", value);
            }
        }


        /// <summary>
        /// Field name whose value is used as recipient email address the confirmation email is sent to.
        /// </summary>
        public virtual string FormConfirmationEmailField
        {
            get
            {
                return GetStringValue("FormConfirmationEmailField", "");
            }
            set
            {
                SetValue("FormConfirmationEmailField", value);
            }
        }


        /// <summary>
        /// Form confirmation template text.
        /// </summary>
        public virtual string FormConfirmationTemplate
        {
            get
            {
                return GetStringValue("FormConfirmationTemplate", null);
            }
            set
            {
                SetValue("FormConfirmationTemplate", value);
            }
        }


        /// <summary>
        /// Form confirmation send from email.
        /// </summary>
        public virtual string FormConfirmationSendFromEmail
        {
            get
            {
                return GetStringValue("FormConfirmationSendFromEmail", "");
            }
            set
            {
                SetValue("FormConfirmationSendFromEmail", value);
            }
        }


        /// <summary>
        /// Form confirmation email subject.
        /// </summary>
        public virtual string FormConfirmationEmailSubject
        {
            get
            {
                return GetStringValue("FormConfirmationEmailSubject", "");
            }
            set
            {
                SetValue("FormConfirmationEmailSubject", value);
            }
        }


        /// <summary>
        /// Form GUID.
        /// </summary>
        public virtual Guid FormGUID
        {
            get
            {
                return GetGuidValue("FormGUID", Guid.Empty);
            }
            set
            {
                SetValue("FormGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime FormLastModified
        {
            get
            {
                return GetDateTimeValue("FormLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("FormLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Form access.
        /// </summary>
        public virtual FormAccessEnum FormAccess
        {
            get
            {
                switch (ValidationHelper.GetInteger(GetValue("FormAccess"), 0))
                {
                    case 0:
                        mAuthorizedRoles = null;
                        return FormAccessEnum.AllBizFormUsers;

                    case 1:
                        return FormAccessEnum.OnlyAuthorizedRoles;

                    default:
                        mAuthorizedRoles = null;
                        return FormAccessEnum.AllBizFormUsers;
                }
            }
            set
            {
                switch (value)
                {
                    case FormAccessEnum.AllBizFormUsers:
                        SetValue("FormAccess", 0);
                        break;

                    case FormAccessEnum.OnlyAuthorizedRoles:
                        SetValue("FormAccess", 1);
                        break;
                }
            }
        }


        /// <summary>
        /// Form definition.
        /// </summary>
        public virtual FormInfo Form
        {
            get
            {
                var dataClassName = DataClassInfoProvider.GetDataClassInfo(FormClassID)?.ClassName ?? "";

                return FormHelper.GetFormInfo(dataClassName, true);
            }
        }


        /// <summary>
        /// Indicates if bizform activity is logged.
        /// </summary>
        public virtual bool FormLogActivity
        {
            get
            {
                return GetBooleanValue("FormLogActivity", false);
            }
            set
            {
                SetValue("FormLogActivity", value);
            }
        }


        /// <summary>
        /// Form development model (default value is <see cref="FormDevelopmentModelEnum.WebForms"/>).
        /// </summary>
        /// <seealso cref="FormDevelopmentModelEnum"/>
        public virtual int FormDevelopmentModel
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("FormDevelopmentModel"), 0);
            }
            set
            {
                SetValue("FormDevelopmentModel", value);
            }
        }


        /// <summary>
        /// MVC form builder layout serialized in JSON.
        /// </summary>
        public virtual string FormBuilderLayout
        {
            get
            {
                return GetStringValue("FormBuilderLayout", "");
            }
            set
            {
                SetValue("FormBuilderLayout", value);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BizFormInfo object.
        /// </summary>
        public BizFormInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BizFormInfo object from the given DataRow.
        /// </summary>
        public BizFormInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        private void LoadRoles()
        {
            if (mAuthorizedRoles == null)
            {
                var tempTable = new SafeDictionary<int, string>();

                var roles = BizFormInfoProvider.GetFormAuthorizedRoles(FormID);
                foreach (var role in roles)
                {
                    tempTable[role.RoleID] = role.RoleName;
                }

                mAuthorizedRoles = tempTable;
            }
        }


        /// <summary>
        /// Indicates whether form is allowed for specified role.
        /// </summary>
        /// <param name="roleId">Role id</param>
        public bool IsFormAllowedForRole(int roleId)
        {
            // Check authorized roles for this form
            return ((FormAccess == FormAccessEnum.AllBizFormUsers) || (AuthorizedRoles[roleId] != null));
        }


        /// <summary>
        /// Indicates whether specified user is authorized for specified form.
        /// </summary>
        /// <param name="user">User info</param>
        /// <param name="siteName">Site name</param>
        public bool IsFormAllowedForUser(UserInfo user, string siteName)
        {
            if ((user == null) || String.IsNullOrEmpty(siteName) || AuthorizedRoles == null)
            {
                return false;
            }

            // Check authorized roles for this form
            if (user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || (FormAccess == FormAccessEnum.AllBizFormUsers))
            {
                return true;
            }

            return AuthorizedRoles.Values
                .Cast<string>()
                .Any(roleName => user.IsInRole(roleName, siteName));
        }


        /// <summary>
        /// Indicates whether specified user is authorized for specified form.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="siteName">Site name</param>
        public bool IsFormAllowedForUser(string userName, string siteName)
        {
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(siteName))
            {
                UserInfo user = UserInfoProvider.GetUserInfo(userName);

                return IsFormAllowedForUser(user, siteName);
            }
            return false;
        }


        /// <summary>
        /// Sets Hashtable with authorized roles to NULL -> enforce hashtable reload next time the data are needed.
        /// </summary>
        public void ClearAuthorizedRoles()
        {
            mAuthorizedRoles = null;
        }


        /// <summary>
        /// Converts permissions enum to permission code name when CheckPermission() is called.
        /// </summary>
        /// <param name="permission">Permissions enum</param>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return "ReadForm";

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return "EditForm";

                default:
                    return base.GetPermissionName(permission);
            }
        }


        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            switch (permission)
            {
                case PermissionsEnum.Destroy:
                    return userInfo.IsAuthorizedPerResource("cms.globalpermissions", "DestroyObjects", siteName, false) ||
                           userInfo.IsAuthorizedPerResource("cms.form", "DestroyForm", siteName, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected internal override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            var originalClass = DataClassInfoProvider.GetDataClassInfo(FormClassID);
            if (originalClass == null)
            {
                return;
            }

            if (originalObject.GetIntegerValue("FormDevelopmentModel", 0) == (int)FormDevelopmentModelEnum.Mvc && !SiteInfoProvider.GetSiteInfo(settings.CloneToSiteID).SiteIsContentOnly)
            {
                throw new NotSupportedException(ResHelper.GetString("clone.bizform.incompatibletypes"));
            }

            // Resolve automatic code name
            EnsureCodeName();

            FormInfo formInfo = new FormInfo(originalClass.ClassFormDefinition);

            // Get primary key definition. Its name was cloned from original variant. New name is based on specified code name of new form.
            FormFieldInfo primaryKeyDefinition = formInfo.GetFields<FormFieldInfo>().First(x => x.PrimaryKey);

            string originalPrimaryKeyName = primaryKeyDefinition.Name;
            string newPrimaryKeyName = BizFormInfoProvider.GenerateFormPrimaryKeyName(FormName);

            // Name and caption of a new form is cloned from original form. Set it to newly generated so it corresponds with cloned form name.
            primaryKeyDefinition.Name = newPrimaryKeyName;
            primaryKeyDefinition.Caption = newPrimaryKeyName;

            // Backup original class form definition
            var originalClassFormDefinition = originalClass.ClassFormDefinition;

            // Generate form definition of cloned class and set it to old class. This is needed because new class is based on original one.
            originalClass.ClassFormDefinition = formInfo.GetXmlDefinition();

            var originalSearchSettings = originalClass.ClassSearchSettings;

            // Update primary key column name in search settings
            var searchSettings = originalClass.ClassSearchSettingsInfos;
            searchSettings.RenameColumn(originalPrimaryKeyName, primaryKeyDefinition.Name);
            originalClass.ClassSearchSettings = searchSettings.GetData();

            // Set unique class name based on given form name
            settings.CodeName = DataClassInfoProvider.GetUniqueClassName(BIZFORM_PREFIX + FormName);
            DataClassInfo clonedClass = (DataClassInfo)originalClass.Generalized.InsertAsClone(settings, result);

            // Set original class form definition back to original class
            originalClass.ClassFormDefinition = originalClassFormDefinition;
            originalClass.ClassSearchSettings = originalSearchSettings;

            // Form layout is cloned from original class. It can contain old name of primary key. This methods renames it.
            FormHelper.RenameFieldInFormLayout(clonedClass.ClassID, originalPrimaryKeyName, primaryKeyDefinition.Name);

            FormClassID = clonedClass.ClassID;

            Insert();
        }


        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("Items", m => m.GetFormItems());
        }


        /// <summary>
        /// Removes object dependencies. First tries to execute removedependencies query, if not found, automatic process is executed.
        /// </summary>
        /// <param name="deleteAll">If false, only required dependencies are deleted, dependencies with default value are replaced with default value and nullable values are replaced with null</param>
        /// <param name="clearHashtables">If true, hashtables of all objecttypes which were potentionally modified are cleared</param>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            // Remove search indexes for this biz form
            DeleteOnLineFormIndex(BizFormItemProvider.BIZFORM_ITEM_PREFIX + DataClassInfoProvider.GetClassName(FormClassID));

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }


        /// <summary>
        /// Removes given on-line form from all the On-Line form indexes.
        /// </summary>
        /// <param name="objectType">BizFrom object type</param>
        private static void DeleteOnLineFormIndex(string objectType)
        {
            List<int> indexIds = SearchIndexInfoProvider.GetIndexIDs(new List<string> { SearchHelper.ONLINEFORMINDEX });
            if (indexIds == null)
            {
                return;
            }

            // Loop through all on-line form indexes
            foreach (int indexId in indexIds)
            {
                SearchIndexInfo indexInfo = SearchIndexInfoProvider.GetSearchIndexInfo(indexId);
                if (indexInfo?.IndexSettings?.Items != null)
                {
                    // Create new settings
                    SearchIndexSettings newSettings = new SearchIndexSettings();
                    bool save = false;
                    foreach (var sisi in indexInfo.IndexSettings.Items)
                    {
                        // Copy only those bizforms to the index which does not contain given objecttype
                        if (sisi.Value.ClassNames.EqualsCSafe(objectType, true))
                        {
                            // Something was skipped, we need to save the index settings
                            save = true;
                        }
                        else
                        {
                            newSettings.Items.Add(sisi.Key, sisi.Value);
                        }
                    }
                    if (save)
                    {
                        indexInfo.IndexSettings = newSettings;
                        indexInfo.Update();

                        // Rebuild the index
                        SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Rebuild, null, null, indexInfo.IndexName, indexInfo.IndexID);
                    }
                }
            }
        }


        /// <summary>
        /// Checks the object license. Returns true if the licensing conditions for this object were matched
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, uses current domain</param>
        protected sealed override bool CheckLicense(ObjectActionEnum action, string domainName)
        {
            return BizFormInfoProvider.CheckLicense(action, domainName);
        }


        /// <summary>
        /// Gets the automatic code name for the object
        /// </summary>
        protected override string GetAutomaticCodeName()
        {
            // Dots are not allowed in form name
            return ValidationHelper.GetIdentifier(base.GetAutomaticCodeName());
        }


        /// <summary>
        /// Returns the form items. Result of this method is not cached.
        /// Use <see cref="BizFormItemProvider"/> if you need more advanced way for obtaining the form items.
        /// </summary>
        private IInfoObjectCollection GetFormItems()
        {
            var classInfo = DataClassInfoProvider.GetDataClassInfo(FormClassID);
            return classInfo != null ? new InfoObjectCollection(BizFormItemProvider.GetObjectType(classInfo.ClassName)) : null;
        }

        #endregion
    }
}