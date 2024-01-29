using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.IO;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Builder for file system repository configuration.
    /// </summary>
    public sealed class FileSystemRepositoryConfigurationBuilder : IFileSystemRepositoryConfigurationBuilder
    {
        // Base path for storing serialized objects.
        private const string DEFAULT_REPOSITORY_ROOT_PATH = @"App_Data\CIRepository";
        private const string REPOSITORY_CONFIG_FILENAME = "repository.config";
        private readonly ISet<string> mObjectTypes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly string mRepositoryRootPath;
        private readonly bool mEnsureConfigurationFile;
        private readonly IMainObjectTypeProvider mMainObjectTypeProvider;
        private readonly ICustomTableTypeProvider mCustomTableTypeProvider;
        private readonly IDictionary<string, IWhereCondition> mObjectTypesFilterConditions = new Dictionary<string, IWhereCondition>();
        private IRepositoryConfigurationLoader mRepositoryConfigurationBuilder;


        /// <summary>
        /// Creates initial configuration or loads the configuration from physical file.
        /// </summary>
        internal IRepositoryConfigurationLoader ConfigurationBuilder
        {
            get
            {
                return mRepositoryConfigurationBuilder ?? (mRepositoryConfigurationBuilder = new RepositoryConfigurationFileLoader());
            }
            set
            {
                mRepositoryConfigurationBuilder = value;
            }
        }


        /// <summary>
        /// Set of object types to be stored in the repository. When this set is empty, repository configuration file is loaded and used.
        /// </summary>
        public ISet<string> ObjectTypes
        {
            get
            {
                return mObjectTypes;
            }
        }


        /// <summary>
        /// Gets the file system repository root path (with trailing backslash).
        /// </summary>
        public string RepositoryRootPath
        {
            get
            {
                return mRepositoryRootPath;
            }
        }


        /// <summary>
        /// Gets the collection of filter conditions ([object type] -> [filter condition])
        /// </summary>
        internal IDictionary<string, IWhereCondition> ObjectTypesFilterConditions
        {
            get
            {
                return mObjectTypesFilterConditions;
            }
        }


        /// <summary>
        /// Creates builder with all configuration properties set to default values.
        /// </summary>
        /// <param name="repositoryRootPath">Relative path to repository root. Default repository path is used if not provided.</param>
        /// <param name="ensureConfigurationFile">Determines whether initial configuration file is created when the repository contains none.</param>
        /// <param name="customTableTypeProvider">Provider implementation for obtaining dynamic object types.</param>
        /// <param name="mainObjectTypeProvider">Provider implementation for obtaining mnain object types.</param>
        /// <remarks>
        /// The initial configuration file is ensured when it is accessed. This typically occurs upon the <see cref="Build"/> method call.
        /// </remarks>
        internal FileSystemRepositoryConfigurationBuilder(string repositoryRootPath = null, bool ensureConfigurationFile = false,
            ICustomTableTypeProvider customTableTypeProvider = null, IMainObjectTypeProvider mainObjectTypeProvider = null)
        {
            if (String.IsNullOrEmpty(repositoryRootPath))
            {
                repositoryRootPath = DEFAULT_REPOSITORY_ROOT_PATH;
            }

            mRepositoryRootPath = Path.EnsureEndBackslash(Path.Combine(SystemContext.WebApplicationPhysicalPath, repositoryRootPath));
            mEnsureConfigurationFile = ensureConfigurationFile;
            mCustomTableTypeProvider = customTableTypeProvider ?? new CustomTableTypeProvider();
            mMainObjectTypeProvider = mainObjectTypeProvider ?? new MainObjectTypeProvider(ObjectTypeManager.RegisteredTypes);
        }


        /// <summary>
        /// Builds file system repository configuration.
        /// </summary>
        /// <returns>File system repository configuration.</returns>
        /// <exception cref="RepositoryConfigurationException">Thrown when loading of repository configuration fails or <see cref="ObjectTypes"/> property is misconfigured.</exception>
        public FileSystemRepositoryConfiguration Build()
        {
            var configuration = ObjectTypes.Count > 0
                ? LoadConfigurationFromObjectTypes()
                : LoadConfigurationFromFileOrInfos();

            return configuration;
        }


        /// <summary>
        /// Loads configuration from <see cref="ObjectTypes"/> collection set to the instance.
        /// </summary>
        /// <exception cref="RepositoryConfigurationException">Thrown when loading of repository configuration fails.</exception>
        private FileSystemRepositoryConfiguration LoadConfigurationFromObjectTypes()
        {
            ObjectTypes.ValidateContinuousIntegrationSupport();

            var mainObjectTypes = ObjectTypes
                .SelectMainObjectTypes()
                .ToArray();

            var mergedConditions = MergeFilterConditions(ObjectTypesFilterConditions, LoadDefaultFilterConditions(ObjectTypes));

            return new FileSystemRepositoryConfiguration(ObjectTypes, mainObjectTypes, RepositoryRootPath, mergedConditions, null, null);
        }


        /// <summary>
        /// Loads configuration from file or uses default configuration if configuration file doesn't exist.
        /// </summary>
        /// <remarks>
        /// Creates an initial configuration file, if the repository root contains none
        /// and <see cref="mEnsureConfigurationFile"/> is set to true.
        /// </remarks>
        /// <exception cref="RepositoryConfigurationException">Thrown when loading of repository configuration fails.</exception>
        private FileSystemRepositoryConfiguration LoadConfigurationFromFileOrInfos()
        {
            var configFile = LoadConfigurationFile();
            var mainTypeInfos = GetMainTypeInfos(configFile);
            var objectTypes = GetObjectTypes(mainTypeInfos, configFile);
            var customTableObjectTypeInfos = GetCustomTableObjectTypes(configFile);

            var allObjectTypes = objectTypes.Union(customTableObjectTypeInfos);

            ExpandGlobalExcludedCodeNameFilter(configFile, allObjectTypes);

            var objectTypesCodenameFilter = LoadCodeNameFilterFromConfigFile(configFile);
            var mergedConditions = MergeFilterConditions(LoadDefaultFilterConditions(objectTypes), objectTypesCodenameFilter.ToDictionary(x => x.Key, x => x.Value.WhereCondition));

            return new FileSystemRepositoryConfiguration(allObjectTypes, mainTypeInfos.SelectObjectType(), RepositoryRootPath, mergedConditions, objectTypesCodenameFilter, () => GetCustomTableObjectTypes(configFile));
        }


        /// <summary>
        /// Returns main object types specified in loaded configuration with fall-back to configuration in individual object type infos
        /// </summary>
        private ObjectTypeInfo[] GetMainTypeInfos(RepositoryConfigurationFile configFile)
        {
            return (configFile.IncludedObjectTypes.Count > 0
                ? configFile.IncludedObjectTypes.Where(t => !IsCustomTableObjectType(t))
                : mMainObjectTypeProvider.GetObjectTypes())
                .Except(configFile.ExcludedObjectTypes, StringComparer.InvariantCultureIgnoreCase)
                .ToContinuousIntegrationSupportingTypeInfos()
                .Where(typeInfo => typeInfo.IsMainObject)
                .ToArray();
        }


        /// <summary>
        /// Returns object types derived from the main object types specified in the loaded configuration <seealso cref="GetMainTypeInfos"/>
        /// </summary>
        private static string[] GetObjectTypes(IEnumerable<ObjectTypeInfo> mainTypeInfos, RepositoryConfigurationFile configFile)
        {
            return mainTypeInfos
                .SelectManyObjectTypesAndSubtypes()
                .Except(configFile.ExcludedObjectTypes, StringComparer.InvariantCultureIgnoreCase)
                .ToArray();
        }


        /// <summary>
        /// Returns all custom table object types according to given configuration
        /// </summary>
        private string[] GetCustomTableObjectTypes(RepositoryConfigurationFile configFile)
        {
            IEnumerable<string> customTableItems;

            if (configFile.IncludedObjectTypes.Any())
            {
                customTableItems = configFile.IncludedObjectTypes.Where(IsCustomTableObjectType);
            }
            else
            {
                customTableItems = mCustomTableTypeProvider.GetTypes();
            }

            customTableItems = customTableItems.Except(configFile.ExcludedObjectTypes, StringComparer.InvariantCultureIgnoreCase);

            return customTableItems.ToArray();
        }


        /// <summary>
        /// Returns <c>true</c>, if given <paramref name="objectType"/> is a custom table object type.
        /// </summary>
        /// <param name="objectType">Object type name.</param>
        private static bool IsCustomTableObjectType(string objectType)
        {
            return objectType.StartsWith(PredefinedObjectType.CUSTOM_TABLE_ITEM_PREFIX, StringComparison.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Returns default filtering where conditions for object types specified by <paramref name="objectTypes"/>.
        /// The conditions are loaded from <see cref="ContinuousIntegrationSettings.FilterCondition"/>.
        /// </summary>
        /// <param name="objectTypes">Object types filter conditions should be loaded for</param>
        private IDictionary<string, IWhereCondition> LoadDefaultFilterConditions(IEnumerable<string> objectTypes)
        {
            var result = new Dictionary<string, IWhereCondition>();
            foreach (var objectType in objectTypes)
            {
                var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
                if (typeInfo?.ContinuousIntegrationSettings.FilterCondition != null)
                {
                    result[objectType] = typeInfo.ContinuousIntegrationSettings.FilterCondition;
                }
            }
            return result;
        }


        /// <summary>
        /// Merges where conditions from two collections.
        /// </summary>
        private IDictionary<string, IWhereCondition> MergeFilterConditions(IDictionary<string, IWhereCondition> firstCollection, IDictionary<string, IWhereCondition> secondCollection)
        {
            var result = new Dictionary<string, IWhereCondition>(firstCollection);
            foreach (var objectType in secondCollection.Keys)
            {
                IWhereCondition baseCondition;
                result[objectType] = result.TryGetValue(objectType, out baseCondition) ? new WhereCondition(baseCondition).Where(secondCollection[objectType]) : secondCollection[objectType];
            }

            return result;
        }


        /// <summary>
        /// Creates <see cref="CodenameFilter"/> for object filtering based on configuration file.
        /// </summary>
        /// <param name="configurationFile">Repository configuration file</param>
        private IDictionary<string, CodenameFilter> LoadCodeNameFilterFromConfigFile(RepositoryConfigurationFile configurationFile)
        {
            var result = new Dictionary<string, CodenameFilter>();
            foreach (var excludedCodeNames in configurationFile.ExcludedCodeNames)
            {
                var objectType = excludedCodeNames.ObjectType;
                var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
                if (typeInfo == null)
                {
                    continue;
                }

                var filterColumn = typeInfo.ContinuousIntegrationSettings.FilterColumn;
                if (filterColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    continue;
                }

                var codeNames = excludedCodeNames.CodeNames;
                if (codeNames.Count <= 0)
                {
                    continue;
                }

                var prefixes = codeNames.Where(x => x.EndsWith("%", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.TrimEnd('%')).ToList();
                var suffixes = codeNames.Where(x => x.StartsWith("%", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.TrimStart('%')).ToList();
                var codenames = codeNames.Where(x => !x.StartsWith("%", StringComparison.InvariantCultureIgnoreCase) && !x.EndsWith("%", StringComparison.InvariantCultureIgnoreCase)).ToList();

                result[objectType] = new CodenameFilter(filterColumn, prefixes, suffixes, codenames);
            }

            return result;
        }


        /// <summary>
        /// Expands global setting for excluded code names into specified object types. 
        /// Existing excluded code name configurations are not overwritten by global settings.
        /// </summary>
        /// <param name="configurationFile">Repository configuration file.</param>
        /// <param name="objectTypes">List of object types which supports continuous integration.</param>
        private static void ExpandGlobalExcludedCodeNameFilter(RepositoryConfigurationFile configurationFile, IEnumerable<string> objectTypes)
        {
            // Check if global settings are present
            var globalItem = configurationFile.ExcludedCodeNames.FirstOrDefault(t => String.IsNullOrEmpty(t.ObjectType));
            if (globalItem == null)
            {
                return;
            }

            configurationFile.ExcludedCodeNames.Remove(globalItem);

            var missingObjectTypes = objectTypes.Except(configurationFile.ExcludedCodeNames.Select(o => o.ObjectType));
            foreach (var objectType in missingObjectTypes)
            {
                // Create code name filter for all supported object types based on global settings
                configurationFile.ExcludedCodeNames.Add(new ExcludedObjectTypeCodeNames
                {
                    ObjectType = objectType,
                    CodeNames = globalItem.CodeNames
                });
            }
        }


        /// <summary>
        /// Loads configuration file.
        /// </summary>
        /// <remarks>
        /// Creates an initial configuration file, if the repository root contains none
        /// and <see cref="mEnsureConfigurationFile"/> is set to true.
        /// </remarks>
        /// <exception cref="RepositoryConfigurationException">Thrown when loading of repository configuration fails.</exception>
        /// <returns>Loaded configuration file or new instance of <see cref="RepositoryConfigurationFile"/> if configuration file doesn't exist in repository.</returns>
        internal RepositoryConfigurationFile LoadConfigurationFile()
        {
            var configFilePath = Path.Combine(RepositoryRootPath, REPOSITORY_CONFIG_FILENAME);
            try
            {
                if (mEnsureConfigurationFile && !File.Exists(configFilePath))
                {
                    DefaultRepositoryConfigurationFileCreator.StoreInitial(mMainObjectTypeProvider, configFilePath);
                }

                return ConfigurationBuilder.Load(configFilePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new RepositoryConfigurationException(String.Format("Unable to access repository configuration file '{0}'. This is typically a result of improperly configured permissions for the file.", configFilePath), ex);
            }
            catch (Exception ex)
            {
                string message = (mEnsureConfigurationFile) ?
                    String.Format("Loading of repository configuration file '{0}' failed. If the configuration file content is malformed, you can either fix it manually or delete the file, so it can be automatically recreated upon next loading.", configFilePath) :
                    String.Format("Loading of repository configuration file '{0}' failed. If the configuration file content is malformed, you have to edit the file manually to fix it.", configFilePath);
                throw new RepositoryConfigurationException(message, ex);
            }
        }
    }
}
