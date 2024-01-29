using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Represents a configuration file of the repository. The configuration specifies which main object types are to be included in the repository.
    /// </summary>
    /// <remarks>
    /// This class is public for the purpose of serialization only. It is not intended for public use.
    /// </remarks>
    [Serializable]
    [XmlRoot(REPOSITORY_CONFIGURATION_ELEMENT_NAME)]
    public sealed class RepositoryConfigurationFile
    {
        internal const string REPOSITORY_CONFIGURATION_ELEMENT_NAME = "RepositoryConfiguration";
        internal const string INCLUDED_OBJECT_TYPES_ELEMENT_NAME = "IncludedObjectTypes";
        internal const string EXCLUDED_OBJECT_TYPES_ELEMENT_NAME = "ExcludedObjectTypes";
        internal const string OBJECT_TYPE_ELEMENT_NAME = "ObjectType";
        internal const string OBJECT_FILTERS_ELEMENT_NAME = "ObjectFilters";
        internal const string OBJECT_EXCLUDED_CODE_NAMES_ELEMENT_NAME = "ExcludedCodeNames";


        /// <summary>
        /// Set of main object types to be included in the repository. Empty set means all main object types.
        /// </summary>
        [XmlArray(INCLUDED_OBJECT_TYPES_ELEMENT_NAME)]
        [XmlArrayItem(OBJECT_TYPE_ELEMENT_NAME)]
        public HashSet<string> IncludedObjectTypes
        {
            get;
            private set;
        }


        /// <summary>
        /// Set of main object types to be excluded from the repository. Empty set means no main object types.
        /// </summary>
        /// <remarks>
        /// When an object type belongs to both <see cref="IncludedObjectTypes"/> and <see cref="ExcludedObjectTypes"/>,
        /// it is excluded from the repository.
        /// </remarks>
        [XmlArray(EXCLUDED_OBJECT_TYPES_ELEMENT_NAME)]
        [XmlArrayItem(OBJECT_TYPE_ELEMENT_NAME)]
        public HashSet<string> ExcludedObjectTypes
        {
            get;
            private set;
        }

        
        /// <summary>
        /// Set of excluded code names for each object type.
        /// </summary>
        [XmlArray(OBJECT_FILTERS_ELEMENT_NAME)]
        [XmlArrayItem(OBJECT_EXCLUDED_CODE_NAMES_ELEMENT_NAME)]
        public HashSet<ExcludedObjectTypeCodeNames> ExcludedCodeNames
        {
            get;
            private set;
        }


        /// <summary>
        /// Initializes a new instance of repository configuration file.
        /// </summary>
        public RepositoryConfigurationFile()
        {
            IncludedObjectTypes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            ExcludedObjectTypes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            ExcludedCodeNames = new HashSet<ExcludedObjectTypeCodeNames>();
        }
    }
}
