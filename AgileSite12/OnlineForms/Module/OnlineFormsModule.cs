using System.Collections.Generic;

using CMS;
using CMS.Activities;
using CMS.Helpers;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.OnlineForms;

[assembly: RegisterModule(typeof(OnlineFormsModule))]

namespace CMS.OnlineForms
{
    /// <summary>
    /// Represents the Form module.
    /// </summary>
    public class OnlineFormsModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public OnlineFormsModule()
            : base(new OnlineFormsModuleMetadata())
        {
        }


        #region "Methods"

        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            InfoProviderLoader.LoadProvider += AbstractProvider_LoadProvider;

            // Init form class info
            DataClassInfo.ReplaceWith<FormClassInfo>().WhenColumnValue(DataClassInfo.TYPEINFO.ObjectClassName, "ClassIsForm", v => ValidationHelper.GetBoolean(v, false));
        }


        /// <summary>
        /// Init module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
            FormSynchronization.Init();
            BizFormExport.Init();
            BizFormImport.Init();
            ImportSpecialActions.Init();

            // Init event handlers
            OnlineFormsHandlers.Init();

            RegisterMacroRule();
            RegisterDataTypes();
        }


        /// <summary>
        /// Gets the object created from the given DataRow.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public override BaseInfo GetObject(string objectType)
        {
            if (objectType != null && BizFormItemProvider.IsBizFormItemObjectType(objectType))
            {
                var dataClass = DataClassInfoProvider.GetDataClassInfo(BizFormItemProvider.GetClassName(objectType));

                // Check if form exists
                if (dataClass != null)
                {
                    return BizFormItem.New(dataClass.ClassName);
                }
            }

            return null;
        }


        /// <summary>
        /// Ensures provider for the BizForm items
        /// </summary>
        private static void AbstractProvider_LoadProvider(object sender, LoadProviderEventArgs e)
        {
            var objectType = e.ObjectType;
            if (BizFormItemProvider.IsBizFormItemObjectType(objectType))
            {
                // Check if form exists
                DataClassInfo dataClass = DataClassInfoProvider.GetDataClassInfo(BizFormItemProvider.GetClassName(objectType));
                e.Provider = (dataClass != null) ? BizFormItemProvider.LoadProviderInternal(objectType) : null;
                e.ProviderLoaded = true;
            }
        }


        /// <summary>
        /// Clears the module hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            BizFormItemProvider.Clear(logTasks);
        }


        /// <summary>
        /// Register metadata containing macro->dataquery translator to speed up contact group recalculation.
        /// </summary>
        private void RegisterMacroRule()
        {
            MacroRuleMetadataContainer.RegisterMetadata(new MacroRuleMetadata("ContactHasEnteredASpecificTextInASpecificFormSFieldInTheLastXDays", new CMSContactFilledFormFieldWithValueInstanceTranslator(),
                affectingActivities: new List<string>(1)
                {
                    PredefinedActivityType.BIZFORM_SUBMIT
                },
                affectingAttributes: null)
            );
        }


        private void RegisterDataTypes()
        {
            DataTypeManager.RegisterDataTypes(
                new DataType<BizFormUploadFile>("nvarchar(500)", BizFormUploadFile.DATATYPE_FORMFILE, "xs:string", BizFormUploadFile.ConvertToBizFormUploadFile,
                    BizFormUploadFile.ConvertToDatabaseValue, new BizFormUploadFileSerializer())
                {
                    TypeGroup = "File",
                    DefaultValueCode = "String.Empty",
                    SupportsTranslation = true,
                    AllowAsAliasSource = false,
                    Hidden = true,
                    UsableBySchemaType = false,
                    UsableBySqlType = false
                });
        }

        #endregion
    }
}