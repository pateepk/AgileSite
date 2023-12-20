using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine.Generators
{
    /// <summary>
    /// A partial class which defines the parameters and constructor for the info provider T4 runtime template.
    /// </summary>
    public partial class BindingInfoProviderTemplate
    {
        #region "Properties"

        /// <summary>
        /// Gets the info class name.
        /// </summary>
        public string InfoClassName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the info class name in plural.
        /// </summary>
        public string InfoClassNamePluralized
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the info provider class name.
        /// </summary>
        public string InfoProviderClassName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the namespace.
        /// </summary>
        public string Namespace
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the binding left side variable name.
        /// </summary>
        public string LeftVariableName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the binding right side variable name.
        /// </summary>
        public string RightVariableName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the binding left side column name.
        /// </summary>
        public string LeftSideColumnName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the binding right side column name.
        /// </summary>
        public string RightSideColumnName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the binding left side variable name.
        /// </summary>
        public string LeftObjectName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the binding right side variable name.
        /// </summary>
        public string RightObjectName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the binding left side name without spaces.
        /// </summary>
        public string LeftObjectCodeName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the binding right side name without spaces.
        /// </summary>
        public string RightObjectCodeName
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates whether class should be generated as partial. 
        /// </summary>
        /// <remarks>
        /// Partial class is not generated for <see cref="SystemContext.DevelopmentMode"/> only.
        /// </remarks>
        internal bool GeneratePartialClass
        {
            get
            {
                return !SystemContext.DevelopmentMode;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates the info provider template and initializes it using the specified data class.
        /// </summary>
        /// <param name="dataClass">Data class</param>
        public BindingInfoProviderTemplate(DataClassInfo dataClass)
        {
            // Get class names from data class code name
            var niceClassName = TextHelper.FirstLetterToUpper(TypeHelper.GetNiceName(dataClass.ClassName));
            InfoClassName = niceClassName + "Info";
            InfoClassNamePluralized = TypeHelper.GetPlural(niceClassName);
            InfoProviderClassName = InfoClassName + "Provider";

            // Get code generations settings from data class
            var settings = dataClass.ClassCodeGenerationSettingsInfo;

            var form = new DataDefinition(dataClass.ClassFormDefinition);
            foreach (var p in form.GetFields<FieldInfo>())
            {
                if (!string.IsNullOrEmpty(p.ReferenceToObjectType) && (p.ReferenceType == ObjectDependencyEnum.Binding))
                {
                    string columnName = p.Name;
                    string variableName = TypeHelper.GetNiceName(p.ReferenceToObjectType);
                    string objectName = TypeHelper.GetNiceObjectTypeName(p.ReferenceToObjectType);

                    var referencedObject = ModuleManager.GetReadOnlyObject(p.ReferenceToObjectType);
                    string codeName = TextHelper.FirstLetterToUpper(TypeHelper.GetNiceName(referencedObject.TypeInfo.ObjectClassName));

                    if (string.IsNullOrEmpty(LeftSideColumnName))
                    {
                        LeftSideColumnName = columnName;
                        LeftVariableName = variableName;
                        LeftObjectName = objectName;
                        LeftObjectCodeName = codeName;
                    }
                    else
                    {
                        RightSideColumnName = columnName;
                        RightVariableName = variableName;
                        RightObjectName = objectName;
                        RightObjectCodeName = codeName;

                        // When we found the second reference, we can end the process - no more than two references are supported
                        break;
                    }
                }
            }

            // Get namespace from module code name
            Namespace = string.IsNullOrEmpty(settings.NameSpace) ? ProviderHelper.GetCodeName(PredefinedObjectType.RESOURCE, dataClass.ClassResourceID) : settings.NameSpace;

            if (string.IsNullOrEmpty(Namespace))
            {
                Namespace = "CMS";
            }
        }

        #endregion
    }
}