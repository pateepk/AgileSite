using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine.Generators
{
    /// <summary>
    /// A partial class which defines the parameters and constructor for the info provider T4 runtime template.
    /// </summary>
    public partial class InfoProviderTemplate
    {
        /// <summary>
        /// Indicates whether a codename column is defined.
        /// </summary>
        public bool HasCodeNameColumn
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates whether a GUID column is defined.
        /// </summary>
        public bool HasGUIDColumn
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
        /// Gets the pluralized info class name.
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
        /// Gets the value that indicates if the ID hashtable is to be used by the provider.
        /// </summary>
        public bool UseIdHashtable
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets a value that indicates if the name hashtable is to be used by the provider.
        /// </summary>
        public bool UseNameHashtable
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets a value that indicates if the GUID hashtable is to be used by the provider.
        /// </summary>
        public bool UseGuidHashtable
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets a value that indicates if any hastable is to be used by the provider.
        /// </summary>
        public bool UseHashtables
        {
            get
            {
                return UseIdHashtable || UseNameHashtable || UseGuidHashtable;
            }
        }


        /// <summary>
        /// Gets a value that indicates if the info can be a site object (i.e. can have site ID specified).
        /// </summary>
        public bool IsSiteObject
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets a value that indicates if the info provider has any advanced methods.
        /// </summary>
        public bool HasAdvancedMethods
        {
            get
            {
                return IsSiteObject;
            }
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


        /// <summary>
        /// Creates the info provider template and initializes it using the specified data class.
        /// </summary>
        /// <param name="dataClass">Data class</param>
        public InfoProviderTemplate(DataClassInfo dataClass)
        {
            // Get class names from data class code name
            var niceClassName = TextHelper.FirstLetterToUpper(TypeHelper.GetNiceName(dataClass.ClassName));
            InfoClassName = niceClassName + "Info";
            InfoClassNamePluralized = TypeHelper.GetPlural(niceClassName);
            InfoProviderClassName = InfoClassName + "Provider";

            // Get code generations settings from data class
            var settings = dataClass.ClassCodeGenerationSettingsInfo;

            HasCodeNameColumn = !string.IsNullOrEmpty(settings.CodeNameColumn) && (settings.CodeNameColumn != "null");
            HasGUIDColumn = !string.IsNullOrEmpty(settings.GuidColumn) && (settings.GuidColumn != "null");

            UseIdHashtable = settings.UseIdHashtable;
            UseNameHashtable = settings.UseNameHashtable && HasCodeNameColumn;
            UseGuidHashtable = settings.UseGuidHashtable && HasGUIDColumn;
            IsSiteObject = !string.IsNullOrEmpty(settings.SiteIdColumn);

            // Get namespace from module code name
            if (string.IsNullOrEmpty(settings.NameSpace))
            {
                Namespace = ProviderHelper.GetCodeName(PredefinedObjectType.RESOURCE, dataClass.ClassResourceID);
            }
            else
            {
                Namespace = settings.NameSpace;
            }

            if (string.IsNullOrEmpty(Namespace))
            {
                Namespace = "CMS";
            }
        }
    }
}