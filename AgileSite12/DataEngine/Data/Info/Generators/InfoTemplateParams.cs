using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;

namespace CMS.DataEngine.Generators
{
    /// <summary>
    /// A partial class which defines the parameters and constructor for the info T4 runtime template.
    /// </summary>
    public partial class InfoTemplate
    {
        #region "Properties"

        /// <summary>
        /// Module name
        /// </summary>
        public string ModuleName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the info class name.
        /// </summary>
        public string InfoClassName
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
        /// Gets the object type.
        /// </summary>
        public string ObjectType
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the object class name.
        /// </summary>
        public string ObjectClassName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the info properties description.
        /// </summary>
        public IEnumerable<InfoTemplateProperty> Properties
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the name of the ID column.
        /// </summary>
        public string IdColumn
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the name of the display name column.
        /// </summary>
        public string DisplayNameColumn
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the name of the code name column.
        /// </summary>
        public string CodeNameColumn
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the name of the GUID column.
        /// </summary>
        public string GuidColumn
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the name of the "last modified" column.
        /// </summary>
        public string LastModifiedColumn
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the name of the binary column.
        /// </summary>
        public string BinaryColumn
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the name of the site ID column.
        /// </summary>
        public string SiteIdColumn
        {
            get;
            private set;
        }


        /// <summary>
        /// Determines whether there is at least one field which has a reference set to some object type.
        /// </summary>
        public bool HasReferences
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
        /// Creates the info template and initializes it using the specified data class.
        /// </summary>
        /// <param name="dataClass">Data class</param>
        public InfoTemplate(DataClassInfo dataClass)
        {
            ObjectType = AddQuotes(dataClass.ClassName.ToLowerInvariant());

            // Get class names from data class code name
            var niceClassName = TextHelper.FirstLetterToUpper(TypeHelper.GetNiceName(dataClass.ClassName));
            InfoClassName = niceClassName + "Info";
            InfoProviderClassName = InfoClassName + "Provider";

            // Get properties from data class
            var form = new DataDefinition(dataClass.ClassFormDefinition);
            Properties = form.GetFields<FieldInfo>().Select(f => new InfoTemplateProperty(f));
            HasReferences = form.GetFields<FieldInfo>().Any(f => !string.IsNullOrEmpty(f.ReferenceToObjectType));

            var idField = form.GetFields<FieldInfo>().FirstOrDefault(f => f.PrimaryKey);
            IdColumn = idField != null ? AddQuotes(idField.Name) : "null";

            // Get code generation settings from data class
            var settings = dataClass.ClassCodeGenerationSettingsInfo;
            if (!string.IsNullOrEmpty(settings.ObjectType))
            {
                ObjectType = AddQuotes(settings.ObjectType.ToLowerInvariant());
            }
            DisplayNameColumn = GetTemplateString(settings.DisplayNameColumn);
            CodeNameColumn = GetTemplateString(settings.CodeNameColumn);
            GuidColumn = GetTemplateString(settings.GuidColumn);
            LastModifiedColumn = GetTemplateString(settings.LastModifiedColumn);
            BinaryColumn = GetTemplateString(settings.BinaryColumn);
            SiteIdColumn = GetTemplateString(settings.SiteIdColumn);

            ObjectClassName = AddQuotes(dataClass.ClassName);

            var resourceName = ProviderHelper.GetCodeName(PredefinedObjectType.RESOURCE, dataClass.ClassResourceID);

            ModuleName = resourceName;

            // Get namespace from module code name
            Namespace = string.IsNullOrEmpty(settings.NameSpace) ? resourceName : settings.NameSpace;

            if (string.IsNullOrEmpty(Namespace))
            {
                Namespace = "CMS";
            }
        }


        private string AddQuotes(string text)
        {
            return string.Format("\"{0}\"", text);
        }


        private string GetTemplateString(string value)
        {
            return string.IsNullOrEmpty(value) ? "null" : AddQuotes(value);
        }

        #endregion


        /// <summary>
        /// A property description for the info T4 runtime template.
        /// </summary>
        public class InfoTemplateProperty
        {
            #region "Properties"

            /// <summary>
            /// Gets the property name.
            /// </summary>
            public string Name
            {
                get;
                private set;
            }


            /// <summary>
            /// Gets the property comment.
            /// </summary>
            public string Comment
            {
                get;
                private set;
            }


            /// <summary>
            /// Gets the property type.
            /// </summary>
            public string Type
            {
                get;
                private set;
            }


            /// <summary>
            /// Gets a value that indicates if the property is not required to have a value.
            /// </summary>
            public bool AllowEmpty
            {
                get;
                private set;
            }


            /// <summary>
            /// Gets a default value of the property.
            /// </summary>
            public string DefaultValue
            {
                get;
                private set;
            }


            /// <summary>
            /// Gets a default value of the type.
            /// </summary>
            public string TypeDefaultValue
            {
                get;
                private set;
            }


            /// <summary>
            /// Gets the name of the method to be used to obtain the property value in the property get method.
            /// </summary>
            public string GetMethod
            {
                get;
                private set;
            }


            /// <summary>
            /// Gets a value that indicates if the GetValue method is to be used in the property get method.
            /// </summary>
            public bool UseGetValue
            {
                get;
                private set;
            }


            /// <summary>
            /// ObjectType to which the given field refers (for example as a foreign key).
            /// </summary>
            public string ReferenceToObjectType
            {
                get;
                set;
            }


            /// <summary>
            /// Type of the reference (used only when ReferenceToObjectType is set).
            /// </summary>
            public string ReferenceType
            {
                get;
                set;
            }


            /// <summary>
            /// Indicates if property has database representation.
            /// </summary>
            public bool HasDatabaseRepresentation
            {
                get;
                set;
            }

            #endregion


            #region "Methods"

            /// <summary>
            /// Creates a new instance initialized using the specified field info.
            /// </summary>
            /// <param name="field">Field info</param>
            public InfoTemplateProperty(FieldInfo field)
            {
                Name = field.Name;
                AllowEmpty = field.AllowEmpty;

                ReferenceToObjectType = field.ReferenceToObjectType;
                ReferenceType = field.ReferenceType.ToStringRepresentation();

                HasDatabaseRepresentation = !field.IsDummyField;

                // Build the comment
                var comment = new StringBuilder();
                var fieldDescription = ValidationHelper.GetString(field.Properties["fielddescription"], null);
                if (string.IsNullOrEmpty(fieldDescription))
                {
                    var words = TextHelper.SplitCamelCase(field.Name).ToArray();
                    
                    comment.Append(words.FirstOrDefault());
                    
                    for (int i = 1; i < words.Count(); i++)
                    {
                        var word = words[i];
                        if (word.Any(char.IsLower))
                        {
                            comment.Append(" ");
                            comment.Append(word.ToLowerInvariant());
                        }
                        else
                        {
                            comment.Append(" ");
                            comment.Append(word);
                        }
                    }
                }
                else
                {
                    comment.Append(CoreServices.Localization.LocalizeString(fieldDescription));
                }

                Comment = comment.ToString();

                // Get type information from data type
                DataType type = DataTypeManager.GetDataType(TypeEnum.Field, field.DataType);
                if (type != null)
                {
                    AllowEmpty &= type.AllowEmpty;
                    UseGetValue = true;
                    Type = type.TypeAlias ?? type.Type.Name;
                    GetMethod = type.ConversionMethod ?? "";

                    // Don't use default value if it is macro - default value of data type will be used
                    var explicitDefaultValue = string.IsNullOrEmpty(ValidationHelper.GetString(field.PropertiesMacroTable["defaultvalue"], string.Empty)) ? field.DefaultValue : string.Empty;

                    if (type.Type == typeof(bool))
                    {
                        explicitDefaultValue = explicitDefaultValue.ToLowerCSafe();
                    }

                    DefaultValue = type.GetDefaultValueCode(explicitDefaultValue, true) ?? "";
                    TypeDefaultValue = type.GetDefaultValueCode() ?? "";
                }
            }

            #endregion
        }
    }
}