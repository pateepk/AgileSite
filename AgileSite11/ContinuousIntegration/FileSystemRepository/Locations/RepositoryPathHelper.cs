using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.IO;

using SystemIO = System.IO;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Provides methods for working with file paths in file system repository.
    /// </summary>
    public class RepositoryPathHelper
    {
        #region "Constants"

        /// <summary>
        /// Maximum length of regular file name (without extension).
        /// </summary>
        /// <seealso cref="FILE_NAME_HASH_LENGTH"/>
        public const int MAX_FILE_NAME_LENGTH = 50;


        /// <summary>
        /// Maximum length of group folder suffix.
        /// </summary>
        /// <seealso cref="GROUP_FOLDER_SUFFIX_HASH_LENGTH"/>
        public const int MAX_GROUP_FOLDER_SUFFIX_LENGTH = 10;


        /// <summary>
        /// Maximum length of group file name (without extension).
        /// </summary>
        /// <seealso cref="GROUP_FILE_NAME_HASH_LENGTH"/>
        public const int MAX_GROUP_FILE_NAME_LENGTH = 10;


        /// <summary>
        /// Length of hash used when appending regular file name with a hash. The file name's length is truncated to (<see cref="MAX_FILE_NAME_LENGTH"/> - <see cref="FILE_NAME_HASH_LENGTH"/> - 1).
        /// One character is needed for delimiter. It is assumed that portion of hash is unique enough to avoid collisions.
        /// </summary>
        public const int FILE_NAME_HASH_LENGTH = 10;


        /// <summary>
        /// Length of hash used when appending group folder suffix with a hash. The suffix's length is truncated to (<see cref="MAX_GROUP_FOLDER_SUFFIX_LENGTH"/> - <see cref="GROUP_FOLDER_SUFFIX_HASH_LENGTH"/> - 1).
        /// One character is needed for delimiter. It is assumed that portion of hash is unique enough to avoid collisions.
        /// </summary>
        public const int GROUP_FOLDER_SUFFIX_HASH_LENGTH = 4;


        /// <summary>
        /// Length of hash used when appending group file name with a hash. The file name's length is truncated to (<see cref="MAX_GROUP_FILE_NAME_LENGTH"/> - <see cref="GROUP_FILE_NAME_HASH_LENGTH"/> - 1).
        /// One character is needed for delimiter. It is assumed that portion of hash is unique enough to avoid collisions.
        /// </summary>
        public const int GROUP_FILE_NAME_HASH_LENGTH = 4;


        /// <summary>
        /// Delimiter between hashed name and group suffix (used when creating name of folder for grouped files).
        /// </summary>
        protected internal const char GROUP_SUFFIX_DELIMITER = '#';


        /// <summary>
        /// Name of the folder for global objects.
        /// </summary>
        internal const string GLOBAL_OBJECTS_FOLDER = "@global";


        /// <summary>
        /// File extension used for serialized objects.
        /// </summary>
        private const string FILE_EXTENSION = ".xml";

        #endregion


        #region "Variables"

        /// <summary>
        /// Translation helper object. Used in serialization to optimize database calls.
        /// </summary>
        protected readonly ContinuousIntegrationTranslationHelper mTranslationHelper;


        /// <summary>
        /// Configuration of a file system repository.
        /// </summary>
        protected readonly FileSystemRepositoryConfiguration mRepositoryConfiguration;

        #endregion


        #region "Internal classes"

        /// <summary>
        /// Holds a pair of default identifier for file system name and custom one.
        /// </summary>
        private class IdentifierPair
        {
            public string Default
            {
                get;
                private set;
            }


            public string Custom
            {
                get;
                private set;
            }


            public IdentifierPair(string defaultIdentifier, string customIdentifier)
            {
                Default = defaultIdentifier;
                Custom = customIdentifier;
            }
        }

        #endregion


        #region "Public methods"


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repositoryConfiguration">Configuration of a file system repository.</param>
        /// <param name="translationHelper">Translation helper object. Used to optimize database calls when asked for the same information multiple times.</param>
        public RepositoryPathHelper(FileSystemRepositoryConfiguration repositoryConfiguration, ContinuousIntegrationTranslationHelper translationHelper = null)
        {
            mTranslationHelper = translationHelper ?? new ContinuousIntegrationTranslationHelper();
            mRepositoryConfiguration = repositoryConfiguration;
        }


        /// <summary>
        /// Gets relative path to file where serialized <paramref name="baseInfo"/> is to be stored (within the file system repository).
        /// </summary>
        /// <param name="baseInfo">BaseInfo of the object to be stored.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseInfo"/> is null.</exception>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="baseInfo"/> is a binding object.</exception>
        public virtual string GetFilePath(BaseInfo baseInfo)
        {
            string folderPath = GetRelativeFolderPath(baseInfo);
            string fileName = GetFileName(baseInfo, true);

            return Path.Combine(folderPath, fileName);
        }


        /// <summary>
        /// Gets relative path to group file where serialized data are to be stored (within the file system repository). A group of files
        /// is used when serializing one base info to multiple files.
        /// </summary>
        /// <param name="baseInfo">BaseInfo of the object to be stored.</param>
        /// <param name="groupName">Name of the group. The name is converted to file system safe characters.</param>
        /// <param name="desiredGroupFileName">Name of file within the group. The name is converted to file system safe characters.</param>
        /// <param name="withExtension">Indicates whether file name should have the extension appended.</param>
        public virtual string GetFilePath(BaseInfo baseInfo, string groupName, string desiredGroupFileName, bool withExtension = true)
        {
            string folderPath = GetRelativeFolderPath(baseInfo);

            string groupFolderName = GetFileName(baseInfo, false);
            string groupFolderNameSuffix = GetGroupFolderSuffix(groupName);
            string resultingGroupFolderName = groupFolderName + GROUP_SUFFIX_DELIMITER + groupFolderNameSuffix;

            string groupFileName = GetGroupFileName(desiredGroupFileName, withExtension);

            return Path.Combine(folderPath, resultingGroupFolderName, groupFileName);
        }


        /// <summary>
        /// Gets relative path of separate field's file.
        /// </summary>
        /// <param name="baseInfo">Base info to be stored.</param>
        /// <param name="infoRelativePath">Relative path to file in which the given base info is to be stored.</param>
        /// <param name="separatedField">Separated field whose path is returned.</param>
        /// <returns>Relative path to given separated field's path.</returns>
        public virtual string GetSeparatedFieldPath(BaseInfo baseInfo, string infoRelativePath, SeparatedField separatedField)
        {
            string fileExtension = null;
            if (!String.IsNullOrEmpty(separatedField.FileExtensionFieldName) && (separatedField.FileExtensionFieldName != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                fileExtension = baseInfo.GetValue(separatedField.FileExtensionFieldName) as string;
            }
            if (String.IsNullOrEmpty(fileExtension))
            {
                fileExtension = separatedField.FileExtension;
            }

            return GetSeparatedFieldPath(infoRelativePath, separatedField.FileName) + "." + fileExtension.TrimStart('.');
        }


        /// <summary>
        /// Gets relative path without extension to separated field file where field's data are to be stored (within the file system repository).
        /// </summary>
        /// <param name="baseInfoPath">Relative path to file in which the given base info is to be stored.</param>
        /// <param name="fieldFileName">Name of separated field's file without extension.</param>
        public virtual string GetSeparatedFieldPath(string baseInfoPath, string fieldFileName)
        {
            baseInfoPath = GetPathWithoutExtension(baseInfoPath);
            string fieldSuffix = GetGroupFolderSuffix(fieldFileName);

            return baseInfoPath + GROUP_SUFFIX_DELIMITER + fieldSuffix;
        }


        /// <summary>
        /// Gets path without the file extension.
        /// </summary>
        /// <param name="path">Path to be processed.</param>
        /// <returns>Path without file extension.</returns>
        public virtual string GetPathWithoutExtension(string path)
        {
            if (path.EndsWith("\\", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Given path is directory path not file path.");
            }

            if (path.Substring(path.LastIndexOf('\\')).Contains('.'))
            {
                path = path.Substring(0, path.LastIndexOf('.'));
            }
            return path;
        }


        /// <summary>
        /// Gets name of the file where serialized <paramref name="baseInfo"/> will be stored.
        /// </summary>
        /// <param name="baseInfo">BaseInfo of the object to be stored.</param>
        /// <param name="withExtension">Indicates whether file name should have the extension appended.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseInfo"/> is null.</exception>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="baseInfo"/> is a binding object.</exception>
        public virtual string GetFileName(BaseInfo baseInfo, bool withExtension)
        {
            if (baseInfo == null)
            {
                throw new ArgumentNullException("baseInfo");
            }

            var fileName = GetObjectName(baseInfo);
            if (fileName == null)
            {
                throw new NotSupportedException("Object type " + baseInfo.TypeInfo.ObjectType + " is not supported because it has no codename or GUID column defined.");
            }

            var resultingName = FileSystemRepositoryHelper.GetFileSystemName(fileName.Custom, fileName.Default, MAX_FILE_NAME_LENGTH, FILE_NAME_HASH_LENGTH);

            return withExtension ? resultingName + FILE_EXTENSION : resultingName;
        }


        /// <summary>
        /// Gets name of the group file from <paramref name="desiredGroupFileName"/>. Group file name is transformed
        /// in order to contain file system safe characters. The group file name is at most <see cref="MAX_GROUP_FILE_NAME_LENGTH"/> characters in length.
        /// </summary>
        /// <param name="desiredGroupFileName">Group file name used when creating the resulting file name.</param>
        /// <param name="withExtension">Indicates whether file name should have the extension appended.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="desiredGroupFileName"/> is null or empty.</exception>
        public virtual string GetGroupFileName(string desiredGroupFileName, bool withExtension)
        {
            if (String.IsNullOrEmpty(desiredGroupFileName))
            {
                throw new ArgumentException("Group name must not be null or empty string.", "desiredGroupFileName");
            }

            var resultingName = FileSystemRepositoryHelper.GetFileSystemName(desiredGroupFileName, MAX_GROUP_FILE_NAME_LENGTH, GROUP_FILE_NAME_HASH_LENGTH);

            return withExtension ? resultingName + FILE_EXTENSION : resultingName;
        }


        /// <summary>
        /// Gets relative paths to the serialization files of given <paramref name="objectType"/> stored in repository.
        /// </summary>
        /// <param name="objectType">Object type</param>
        internal virtual IEnumerable<RepositoryLocationsCollection> GetExistingSerializationFiles(string objectType)
        {
            if (!Directory.Exists(mRepositoryConfiguration.RepositoryRootPath))
            {
                // No directory, no files
                return Enumerable.Empty<RepositoryLocationsCollection>();
            }

            var paths = Directory.GetDirectories(mRepositoryConfiguration.RepositoryRootPath)
                                 .SelectMany(x => GetExistingSerializationFiles(objectType, x));

            return GetRelativeRepositoryPaths(paths);
        }


        /// <summary>
        /// Gets relative paths to the serialization files of given <paramref name="baseInfo"/> stored in repository.
        /// </summary>
        /// <param name="baseInfo">Base info for which to list the serialization files.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseInfo"/> is null.</exception>
        public virtual IEnumerable<RepositoryLocationsCollection> GetExistingSerializationFiles(BaseInfo baseInfo)
        {
            if (baseInfo == null)
            {
                throw new ArgumentNullException("baseInfo");
            }

            var result = new List<RepositoryLocationsCollection>();

            // Get serialization file name without extension - the file name is a common prefix of all group files' directories related to this base info
            string rootedFolderPath = mRepositoryConfiguration.GetAbsolutePath(GetRelativeFolderPath(baseInfo));
            string fileName = GetFileName(baseInfo, false);

            string mainSerializationFile = Path.Combine(rootedFolderPath, fileName + FILE_EXTENSION);
            RepositoryLocationsCollection mainFiles = null;
            if (File.Exists(mainSerializationFile))
            {
                mainFiles = new RepositoryLocationsCollection(mainSerializationFile);

                // Add separated fields' files
                Directory.GetFiles(rootedFolderPath, fileName + "#*").ToList().ForEach(separatedFieldFile => mainFiles.Add(mainSerializationFile, separatedFieldFile));
            }

            // Iterate non-empty group directories and add the main serialization files to each group
            var groupDirectories = GetDirectoriesIfExists(rootedFolderPath, fileName + "#*");
            var groupRepositoryCollections = groupDirectories
                .Select(groupDirectory => GetGroupFilesCollection(groupDirectory, mainFiles))
                .Where(groupFiles => groupFiles != null)
                .Select(groupFiles => new RepositoryLocationsCollection(groupFiles));
            result.AddRange(groupRepositoryCollections);

            // The main serialization files have to be included at least once in the result
            if ((result.Count == 0) && (mainFiles != null))
            {
                result.Add(mainFiles);
            }

            return GetRelativeRepositoryPaths(result);
        }
        

        /// <summary>
        /// Gets file system safe representation of group suffix (without the <see cref="GROUP_SUFFIX_DELIMITER"/>).
        /// The group folder suffix is at most <see cref="MAX_GROUP_FOLDER_SUFFIX_LENGTH"/> long.
        /// </summary>
        /// <param name="groupName">Group name to be transformed to file system safe representation.</param>
        /// <returns>Group name to be used as suffix in file system name representation.</returns>
        /// <remarks>
        /// Files which logically belong together can be grouped on the file system level. The group is distinguished by adding
        /// a suffix to the original file name.
        /// </remarks>
        public virtual string GetGroupFolderSuffix(string groupName)
        {
            if (String.IsNullOrEmpty(groupName))
            {
                throw new ArgumentException("Group name must not be null or empty string.", "groupName");
            }

            return FileSystemRepositoryHelper.GetFileSystemName(groupName, MAX_GROUP_FOLDER_SUFFIX_LENGTH, GROUP_FOLDER_SUFFIX_HASH_LENGTH);
        }


        /// <summary>
        /// Gets relative path to folder where serialized <paramref name="baseInfo"/> is to be stored.
        /// </summary>
        /// <param name="baseInfo">BaseInfo of the object to be stored.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseInfo"/> is null.</exception>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="baseInfo"/> is a binding object.</exception>
        public virtual string GetRelativeFolderPath(BaseInfo baseInfo)
        {
            if (baseInfo == null)
            {
                throw new ArgumentNullException("baseInfo");
            }

            return GetFolder(baseInfo);
        }


        /// <summary>
        /// Gets file extension (including the '.' character) of a file in repository identified by its relative path without extension.
        /// </summary>
        /// <param name="extensionlessRelativePath">Relative path within the repository identifying the file.</param>
        /// <returns>Extension of the file, or null if no file matching <paramref name="extensionlessRelativePath"/> is found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="extensionlessRelativePath"/> is null.</exception>
        public virtual string GetSerializationFileExtension(string extensionlessRelativePath)
        {
            if (extensionlessRelativePath == null)
            {
                throw new ArgumentNullException("extensionlessRelativePath");
            }

            var absolutePath = Path.Combine(mRepositoryConfiguration.RepositoryRootPath, extensionlessRelativePath);
            var searchDirectory = Path.GetDirectoryName(absolutePath);
            var fileSearchPattern = Path.GetFileNameWithoutExtension(absolutePath) + ".*";

            if (!Directory.Exists(searchDirectory))
            {
                // Directory does not exists, no files exist
                return null;
            }

            var matchingFiles = Directory.GetFiles(searchDirectory, fileSearchPattern);

            return matchingFiles.Length != 0 
                ? Path.GetExtension(matchingFiles[0])
                : null;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets absolute paths to the serialization files of objects of given object type. The <paramref name="path"/> specified
        /// either a global or site folder in which to search for the object type's files.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="path">The path to the directory to search (global or site specific folder).</param>
        private IEnumerable<RepositoryLocationsCollection> GetExistingSerializationFiles(string objectType, string path)
        {
            var objectTypePath = Path.Combine(path, FileSystemRepositoryHelper.GetFileSystemName(objectType, MAX_FILE_NAME_LENGTH, FILE_NAME_HASH_LENGTH));

            if (Directory.Exists(objectTypePath))
            {
                var directories = Directory.GetDirectories(objectTypePath);

                // Filter out directories which are used for file grouping
                var parentCodeNameDirectories = directories.Where(dir => !Path.GetFileName(dir).Contains(GROUP_SUFFIX_DELIMITER));
                var groupDirectories = directories.Where(dir => Path.GetFileName(dir).Contains(GROUP_SUFFIX_DELIMITER));

                return parentCodeNameDirectories.SelectMany(dir => GetGroupedSerializationFiles(dir))
                                                               .Union(GetGroupedSerializationFiles(objectTypePath, groupDirectories));
            }

            return Enumerable.Empty<RepositoryLocationsCollection>();
        }


        /// <summary>
        /// Gets serialization files contained in <paramref name="directoryPath"/> together with their grouped files.
        /// </summary>
        /// <param name="directoryPath">Absolute path to directory where the serialization files reside.</param>
        /// <param name="groupDirectories">Optional enumeration of group directories contained within <paramref name="directoryPath"/> (absolute paths) - if available from previous directory enumeration.</param>
        /// <returns>Absolute paths of grouped serialization files.</returns>
        private IEnumerable<RepositoryLocationsCollection> GetGroupedSerializationFiles(string directoryPath, IEnumerable<string> groupDirectories = null)
        {
            if (groupDirectories == null)
            {
                groupDirectories = Directory.GetDirectories(directoryPath).Where(dir => Path.GetFileName(dir).Contains(GROUP_SUFFIX_DELIMITER));
            }

            var result = new List<RepositoryLocationsCollection>();

            var serializationFilesByPrefix = GetFilesFromDirectory(directoryPath);

            ISet<string> processedSerializationFiles = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            // Match group directories to their corresponding main serialization file
            foreach (var groupDirectory in groupDirectories)
            {
                var groupDirectoryPrefix = groupDirectory.Substring(0, groupDirectory.LastIndexOf(GROUP_SUFFIX_DELIMITER));
                if (serializationFilesByPrefix.ContainsKey(groupDirectoryPrefix))
                {
                    // Add main file and group files - the main file can belong to multiple groups
                    var groupFiles = GetGroupFilesCollection(groupDirectory, serializationFilesByPrefix[groupDirectoryPrefix]);
                    if (groupFiles != null)
                    {
                        result.Add(groupFiles);
                        processedSerializationFiles.Add(groupDirectoryPrefix);
                    }
                }
                else
                {
                    // Add group files only
                    var groupCollection = GetGroupFilesCollection(groupDirectory);
                    if (groupCollection != null)
                    {
                        result.Add(groupCollection);
                    }
                }
            }

            // Add remaining main serialization files (which have no matching group directory)
            foreach (var serializationFile in serializationFilesByPrefix.Where(it => !processedSerializationFiles.Contains(it.Key)))
            {
                result.Add(serializationFile.Value);
            }

            return result;
        }


        /// <summary>
        /// Gets collection of files in given directory. Each item contains file paths of one object. Items are indexed by main file name without extension.
        /// </summary>
        /// <param name="directoryPath">Directory to be processed.</param>
        /// <returns>Collection of files in given directory.</returns>
        private IDictionary<string, RepositoryLocationsCollection> GetFilesFromDirectory(string directoryPath)
        {
            var result = new Dictionary<string, RepositoryLocationsCollection>(StringComparer.InvariantCultureIgnoreCase);

            var serializationFiles = Directory.GetFiles(directoryPath);
            var mainFiles = serializationFiles.Where(it => !it.Contains(GROUP_SUFFIX_DELIMITER) && it.EndsWith(FILE_EXTENSION, StringComparison.InvariantCultureIgnoreCase));
            var additionalFiles = serializationFiles.Where(it => it.Contains(GROUP_SUFFIX_DELIMITER)).ToList();

            foreach (var mainFile in mainFiles)
            {
                var fileName = mainFile.Substring(0, mainFile.Length - FILE_EXTENSION.Length);

                var collection = new RepositoryLocationsCollection(mainFile);
                foreach (var additionalFile in additionalFiles.Where(it => it.StartsWith(fileName + GROUP_SUFFIX_DELIMITER, StringComparison.InvariantCultureIgnoreCase)))
                {
                    collection.Add(mainFile, additionalFile);
                }

                result.Add(fileName, collection);
            }

            return result;
        }


        /// <summary>
        /// Gets collection of files in given group directory. Group's files can be combined with object's main files if <paramref name="mainFileCollection"/> is given.
        /// </summary>
        /// <param name="groupDirectoryPath">Group directory path.</param>
        /// <param name="mainFileCollection">Collection of object's main files.</param>
        /// <returns>Collection of files in given group directory (optionally combined with <paramref name="mainFileCollection"/>). Returns null if group directory is empty.</returns>
        private RepositoryLocationsCollection GetGroupFilesCollection(string groupDirectoryPath, RepositoryLocationsCollection mainFileCollection = null)
        {
            var result = mainFileCollection != null ? (RepositoryLocationsCollection)mainFileCollection.Clone() : new RepositoryLocationsCollection();

            var serializationFiles = Directory.GetFiles(groupDirectoryPath);
            var mainFiles = serializationFiles.Where(it => !Path.GetFileName(it).Contains(GROUP_SUFFIX_DELIMITER)).ToList();
            var additionalFiles = serializationFiles.Except(mainFiles).ToList();

            if (!mainFiles.Any())
            {
                return null;
            }

            foreach (var mainFile in mainFiles)
            {
                result.Add(mainFile);

                var fileName = mainFile.Substring(0, mainFile.Length - FILE_EXTENSION.Length);
                foreach (var additionalFile in additionalFiles.Where(it => it.StartsWith(fileName + GROUP_SUFFIX_DELIMITER, StringComparison.InvariantCultureIgnoreCase)))
                {
                    result.Add(mainFile, additionalFile);
                }
            }

            return result;
        }


        /// <summary>
        /// Trims absolute repository paths to paths relative to repository root.
        /// </summary>
        /// <param name="repositoryPaths">Enumeration of <see cref="RepositoryLocationsCollection"/> to be processed</param>
        private IEnumerable<RepositoryLocationsCollection> GetRelativeRepositoryPaths(IEnumerable<RepositoryLocationsCollection> repositoryPaths)
        {
            if (repositoryPaths == null)
            {
                throw new ArgumentNullException("repositoryPaths");
            }

            var repositoryRootPath = Path.EnsureEndBackslash(mRepositoryConfiguration.RepositoryRootPath);
            int repositoryPathLength = repositoryRootPath.Length;

            return repositoryPaths.Select(locations => (RepositoryLocationsCollection)locations.Clone(x => x.Substring(repositoryPathLength)));
        }


        /// <summary>
        /// Gets object's name. Name is retrieved from object's fields specified in <see cref="ContinuousIntegrationSettings.ObjectFileNameFields"/> or default object name is used.
        /// Object's GUID is used if object has no code name column defined, otherwise object's code name is returned for default name.
        /// </summary>
        /// <param name="baseInfo">Object for which to get the name.</param>
        /// <returns>Given object's name.</returns>
        private IdentifierPair GetObjectName(BaseInfo baseInfo)
        {
            var generalized = baseInfo.Generalized;
            
            if (generalized.TypeInfo.IsBinding)
            {
                // Get parent identifier
                return GetParentIdentifier(baseInfo);
            }

            var name = GetObjectDefaultName(baseInfo);
            if (name != null)
            {
                var customName = GetCustomObjectIdentifier(mTranslationHelper.TranslationReferenceLoader.LoadExtendedFromInfoObject(baseInfo), name);

                return new IdentifierPair(name, customName);
            }
            return null;
        }


        /// <summary>
        /// Gets default object's name. Object's GUID is used if object has no code name column defined, otherwise object's code name is returned.
        /// </summary>
        /// <param name="baseInfo">Object for which to get the name.</param>
        /// <returns>Given object's name.</returns>
        private string GetObjectDefaultName(BaseInfo baseInfo)
        {
            string name = null;
            var generalized = baseInfo.Generalized;

            var typeInfo = baseInfo.TypeInfo;
            var identificationField = typeInfo.ContinuousIntegrationSettings.IdentificationField;
            if (!String.IsNullOrEmpty(identificationField) && (identificationField != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                name = generalized.GetValue(identificationField).ToString();
            }
            else if (!String.IsNullOrEmpty(generalized.CodeNameColumn) && (generalized.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                // Get code name
                name = generalized.ObjectCodeName;
            }
            else if (!String.IsNullOrEmpty(generalized.TypeInfo.GUIDColumn) && (generalized.TypeInfo.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                // Get object GUID
                name = generalized.ObjectGUID.ToString();
            }

            return name;
        }


        /// <summary>
        /// Gets relative path to location where serialized <paramref name="baseInfo"/> is to be stored.
        /// </summary>
        /// <param name="baseInfo">BaseInfo of the object to be stored.</param>
        private string GetFolder(BaseInfo baseInfo)
        {
            string result;

            // Bindings with Site ID column are bindings of global objects with site. 
            if (baseInfo.TypeInfo.IsSiteBinding || baseInfo.IsGlobal)
            {
                result = GLOBAL_OBJECTS_FOLDER;
            }
            else
            {
                // Site folder uses object identifier, not the parent one
                var defaultIdentifier = baseInfo.Site.Generalized.ObjectCodeName;
                var customIdentifier = GetCustomObjectIdentifier(mTranslationHelper.TranslationReferenceLoader.LoadExtendedFromInfoObject(baseInfo.Site), defaultIdentifier);
                result = FileSystemRepositoryHelper.GetFileSystemName(customIdentifier, baseInfo.Site.Generalized.ObjectCodeName, MAX_FILE_NAME_LENGTH, FILE_NAME_HASH_LENGTH);
            }

            var objectTypeFolder = baseInfo.TypeInfo.ContinuousIntegrationSettings.ObjectTypeFolderName;
            if (String.IsNullOrEmpty(objectTypeFolder))
            {
                // Use object type if folder is not specified
                objectTypeFolder = baseInfo.TypeInfo.ObjectType;
            }

            result = Path.Combine(result, FileSystemRepositoryHelper.GetFileSystemName(objectTypeFolder, MAX_FILE_NAME_LENGTH, FILE_NAME_HASH_LENGTH));

            // Parent
            if (baseInfo.TypeInfo.IsBinding)
            {
                // Bindings are stored in by-parent files rather than sub-folders
                return result;
            }

            var parentIdentifier = GetParentIdentifier(baseInfo);
            if (parentIdentifier != null)
            {
                result = Path.Combine(result, FileSystemRepositoryHelper.GetFileSystemName(parentIdentifier.Custom, parentIdentifier.Default, MAX_FILE_NAME_LENGTH, FILE_NAME_HASH_LENGTH));
            }

            return result;
        }
        

        private IdentifierPair GetParentIdentifier(BaseInfo baseInfo)
        {
            if (String.IsNullOrEmpty(baseInfo.Generalized.ParentObjectType))
            {
                // Do empty check, default value is empty string.
                return null;
            }

            var parentTranslation = mTranslationHelper.TranslationReferenceLoader.LoadExtendedFromDatabase(baseInfo.Generalized.ParentObjectType, baseInfo.Generalized.ObjectParentID);
            if (parentTranslation != null)
            {
                var defaultParentIdentifier = parentTranslation.ToString();
                var customParentIdentifier = GetCustomParentIdentifier(baseInfo.TypeInfo.ObjectType, parentTranslation, defaultParentIdentifier);

                return new IdentifierPair(defaultParentIdentifier, customParentIdentifier);
            }
            return null;
        }


        private string GetCustomObjectIdentifier(ExtendedTranslationReference translationReference, string defaultIdentifier)
        {
            var customObjectIdentifier = FileSystemRepositoryNamingProvider.GetObjectIdentifier(translationReference, defaultIdentifier);

            return String.IsNullOrEmpty(customObjectIdentifier) ? defaultIdentifier : customObjectIdentifier;
        }


        private string GetCustomParentIdentifier(string childObjectType, ExtendedTranslationReference translationReference, string defaultIdentifier)
        {
            var customParentIdentifier = FileSystemRepositoryNamingProvider.GetParentIdentifier(childObjectType, translationReference, defaultIdentifier);

            return String.IsNullOrEmpty(customParentIdentifier) ? defaultIdentifier : customParentIdentifier;
        }


        /// <summary>
        /// Gets directories in <paramref name="path"/>, if it exists. Otherwise returns empty enumeration.
        /// </summary>
        /// <param name="path">Path to directory.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>Directories in <paramref name="path"/>, or empty enumeration.</returns>
        /// <remarks>
        /// The implementation is optimistic (assumes the directory exists).
        /// </remarks>
        private static IEnumerable<string> GetDirectoriesIfExists(string path, string searchPattern)
        {
            try
            {
                return Directory.GetDirectories(path, searchPattern);
            }
            catch (SystemIO.DirectoryNotFoundException)
            {
                return Enumerable.Empty<string>();
            }
        }

        #endregion
    }
}
