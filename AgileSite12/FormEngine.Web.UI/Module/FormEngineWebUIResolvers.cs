using System;
using System.Linq;
using System.Xml;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.EventLog;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class FormEngineWebUIResolvers : ResolverDefinition
    {
        #region "Private properties"

        private readonly static StringSafeDictionary<MacroResolver> resolvers = new StringSafeDictionary<MacroResolver>();

        #endregion


        /// <summary>
        /// Clears resolvers.
        /// </summary>
        /// <param name="logTask">If true, web farm tasks are logged</param>
        public static void ClearResolvers(bool logTask)
        {
            resolvers.Clear();

            if (logTask)
            {
                WebFarmHelper.CreateTask(new ClearFormResolversWebFarmTask());
            }
        }


        /// <summary>
        /// Returns a resolver based on given name.
        /// </summary>
        /// <param name="name">Name of the resolver</param>
        public static MacroResolver GetResolver(string name)
        {
            if (name == null)
            {
                return null;
            }

            MacroResolver resolver = null;
            string key = name.ToLowerCSafe();

            // Get cached resolver
            if (resolvers.ContainsKey(key))
            {
                resolver = resolvers[key];
            }
            else
            {
                // Get resolver
                if (key.StartsWithCSafe(FormHelper.FORM_PREFIX, true))
                {
                    resolver = GetFormResolver(key.Substring(5));
                }
                else if (key.StartsWithCSafe(FormHelper.FORMDEFINITION_PREFIX, true))
                {
                    resolver = GetFormDefinitionResolver(name.Substring(15));
                }

                // Store resolver
                resolvers[key] = resolver;
            }

            return resolver;
        }


        /// <summary>
        /// Returns new instance of resolver for given class (includes all fields, etc.).
        /// </summary>
        /// <param name="className">Name of the class</param>
        private static MacroResolver GetFormResolver(string className)
        {
            FormInfo fi = FormHelper.GetFormInfo(className, true);

            if (fi == null)
            {
                return null;
            }

            var resolver = GetFormResolver(fi);

            // Check if provided name is alternative form name
            if (className.IndexOfCSafe('.') != className.LastIndexOfCSafe('.'))
            {
                className = className.Substring(0, className.LastIndexOfCSafe('.'));
            }

            DataClassInfo dc = DataClassInfoProvider.GetDataClassInfo(className);
            SetEditedObjectDetails(resolver, dc);

            return resolver;
        }


        /// <summary>
        /// Returns new instance of resolver based on given form definition.
        /// </summary>
        /// <param name="formDefinition">Form definition</param>
        private static MacroResolver GetFormDefinitionResolver(string formDefinition)
        {
            FormInfo fi = null;
            try
            {
                fi = new FormInfo(formDefinition);
            }
            catch (XmlException)
            {
                //Exception is logged
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("FormControlResolver", "GetResolver", ex);
            }

            if (fi != null)
            {
                return GetFormResolver(fi);
            }

            return null;
        }


        /// <summary>
        /// Returns new instance of resolver for given class (includes all fields, etc.).
        /// </summary>
        /// <param name="formInfo">FormInfo</param>
        private static MacroResolver GetFormResolver(FormInfo formInfo)
        {
            var resolver = MacroResolver.GetInstance();

            // Register each field under its name
            foreach (var fieldInfo in formInfo.ItemsList.OfType<FormFieldInfo>())
            {
                resolver.SetNamedSourceData(fieldInfo.Name, new FormControlMacroContainer { Info = fieldInfo });
            }

            // Current field value
            resolver.SetNamedSourceData("Value", string.Empty);

            // Add form information
            resolver.SetNamedSourceData("Form", new FormMacroContainer<BasicForm>());

            // Add form mode information
            resolver.SetNamedSourceData("FormMode", string.Empty);

            // Fill resolver with visible form controls
            StringSafeDictionary<FormControlMacroContainer> fields = new StringSafeDictionary<FormControlMacroContainer>();
            foreach (var fieldInfo in formInfo.ItemsList.OfType<FormFieldInfo>())
            {
                fields[fieldInfo.Name] = new FormControlMacroContainer { Info = fieldInfo };
            }
            resolver.SetNamedSourceData("Fields", new SafeDictionaryContainer<FormControlMacroContainer>(fields));

            // Fill resolver with form categories
            StringSafeDictionary<FormCategoryInfo> categories = new StringSafeDictionary<FormCategoryInfo>();
            foreach (var categoryInfo in formInfo.ItemsList.OfType<FormCategoryInfo>())
            {
                categories[categoryInfo.CategoryName] = categoryInfo;
            }
            resolver.SetNamedSourceData("Categories", new SafeDictionaryContainer<FormCategoryInfo>(categories));

            RegisterFormResolverEnumData(resolver);

            return resolver;
        }


        /// <summary>
        /// Registers form's enumeration data into given macro resolver
        /// </summary>
        /// <param name="resolver">Macro resolver</param>
        internal static void RegisterFormResolverEnumData(MacroResolver resolver)
        {
            // Add available enumerations
            resolver.SetNamedSourceData("FormModeEnum", new EnumDataContainer(typeof (FormModeEnum)));
            resolver.SetNamedSourceData("FieldDataTypeEnum", new EnumerableDataContainer<string>(DataTypeManager.FieldTypes));
            resolver.SetNamedSourceData("FormFieldControlTypeEnum", new EnumDataContainer(typeof (FormFieldControlTypeEnum)));
        }


        private static void SetEditedObjectDetails(MacroResolver resolver, DataClassInfo classInfo)
        {
            if (classInfo.ClassIsDocumentType)
            {
                resolver.SetNamedSourceData("EditedObject", TreeNode.New(classInfo.ClassName));
            }
            else
            {
                var obj = ModuleManager.GetReadOnlyObjectByClassName(classInfo.ClassName);
                if (obj != null)
                {
                    resolver.SetNamedSourceData("EditedObject", obj);
                    if (!string.IsNullOrEmpty(obj.Generalized.ParentObjectType))
                    {
                        resolver.SetNamedSourceData("ParentObject", ModuleManager.GetReadOnlyObject(obj.Generalized.ParentObjectType));
                    }
                }
            }
        }
    }
}