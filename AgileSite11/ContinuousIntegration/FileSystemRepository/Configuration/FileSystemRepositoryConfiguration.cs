using System;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.IO;


namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Configuration of a file system repository.
    /// Use <see cref="FileSystemRepositoryConfigurationBuilder"/> to create a new configuration.
    /// </summary>
    /// <seealso cref="FileSystemRepositoryConfigurationBuilder"/>
    public class FileSystemRepositoryConfiguration
    {
        private readonly Func<string[]> mGetCustomTableObjectTypesMethod;


        /// <summary>
        /// Gets root path to file system repository (with trailing backslash).
        /// </summary>
        public string RepositoryRootPath
        {
            get;
            internal set;
        }


        /// <summary>
        /// Gets the set of object types to be stored in the repository.
        /// </summary>
        /// <remarks>
        /// The set includes root types and their child types.
        /// </remarks>
        public ISet<string> ObjectTypes
        {
            get;
            internal set;
        }


        /// <summary>
        /// Gets the set of main object types to be stored in the repository. Main object type is type without a parent type.
        /// </summary>
        public ISet<string> MainObjectTypes
        {
            get;
            internal set;
        }


        /// <summary>
        /// Gets the dictionary of object types filter conditions [ObjectType] -> [WhereCondition]. Only objects meeting these conditions will be stored.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Values in this dictionary have <see cref="CodenameFilter.WhereCondition"/> merged in for given object type (key). 
        /// </para>
        /// <para>
        /// For object filtering in memory during the restoration process, use <see cref="ObjectTypesCodenameFilter"/>.
        /// </para>
        /// </remarks>
        public IDictionary<string, IWhereCondition> ObjectTypesFilterConditions
        {
            get;
            internal set;
        }


        /// <summary>
        /// Gets the dictionary of object types codename filter [ObjectType] -> [CodenameFilter]. Only objects meeting these conditions will be restored.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This filter is used when object are filtered in memory by codename during the restore process.
        /// </para>
        /// <para>
        /// When filtering objects from database, use <see cref="ObjectTypesFilterConditions"/>, where <see cref="CodenameFilter.WhereCondition"/> is merged in.
        /// </para>
        /// </remarks>
        public IDictionary<string, CodenameFilter> ObjectTypesCodenameFilter
        {
            get;
            internal set;
        }


        /// <summary>
        /// Internal constructor. Use <see cref="FileSystemRepositoryConfigurationBuilder"/> to create a new configuration.
        /// </summary>
        /// <seealso cref="FileSystemRepositoryConfigurationBuilder"/>
        internal FileSystemRepositoryConfiguration()
        {
        }


        /// <summary>
        /// Internal constructor. Use <see cref="FileSystemRepositoryConfigurationBuilder"/> to create a new configuration.
        /// </summary>
        /// <seealso cref="FileSystemRepositoryConfigurationBuilder"/>
        internal FileSystemRepositoryConfiguration(IEnumerable<string> objectTypes, IEnumerable<string> mainObjectTypes, string repositoryRootPath, 
            IDictionary<string, IWhereCondition> objectTypesFilterConditions, IDictionary<string, CodenameFilter> objectTypesCodenameFilter, Func<string[]> getCustomTableObjectTypesMethod)
        {
            mGetCustomTableObjectTypesMethod = getCustomTableObjectTypesMethod;

            ObjectTypes = new HashSet<string>(objectTypes, StringComparer.InvariantCultureIgnoreCase);
            MainObjectTypes = new HashSet<string>(mainObjectTypes, StringComparer.InvariantCultureIgnoreCase);
            RepositoryRootPath = repositoryRootPath;
            ObjectTypesFilterConditions = new Dictionary<string, IWhereCondition>(objectTypesFilterConditions, StringComparer.InvariantCultureIgnoreCase);
            ObjectTypesCodenameFilter = objectTypesCodenameFilter == null ?
                new Dictionary<string, CodenameFilter>(StringComparer.InvariantCultureIgnoreCase) :
                new Dictionary<string, CodenameFilter>(objectTypesCodenameFilter, StringComparer.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Combines given <paramref name="relativePath"/> with <see cref="RepositoryRootPath"/> to produce an absolute path.
        /// </summary>
        /// <param name="relativePath">Relative path within the repository.</param>
        internal string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(RepositoryRootPath ?? "", relativePath);
        }


        /// <summary>
        /// Reloads custom table object types and adds them to current <see cref="ObjectTypes"/> of this configuration.
        /// </summary>
        internal void ReloadCustomTableObjectTypes()
        {
            if (mGetCustomTableObjectTypesMethod != null)
            {
                var customTableObjectTypes = mGetCustomTableObjectTypesMethod();
                ObjectTypes.AddRange(customTableObjectTypes);
            }
        }
    }
}
