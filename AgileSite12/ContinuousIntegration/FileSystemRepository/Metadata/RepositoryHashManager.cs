using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;
using CMS.Helpers.Internal;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Collection of repository locations and hashes of the content of all parts of serialization of all objects the manager was provided to.
    /// If hash value is null, the file in the corresponding location has been removed (typically occurs when base info uses separated fields
    /// with dynamic extension and the extension has changed, resulting in new file being created).
    /// </summary>
    /// <remarks>
    /// Repository location is a relative path starting from the repository root.
    /// </remarks>
    internal class RepositoryHashManager
    {
        #region "Constants"

        // Constant marking a relative path that was removed physically, so the file's hash should be removed from DB (if exists).
        private static readonly ChangeablePathHash PATH_HASH_REMOVED = new ChangeablePathHash
        {
            Changed = false,
            FromDb = false
        };


        // Converts ChangablePathHash into FileMetadataInfo (without ID)
        private static readonly Func<ChangeablePathHash, FileMetadataInfo> SELECT_METADATA_FROM_CHANGABLE_HASH_DELEGATE = storedHash => new FileMetadataInfo()
        {
            FileHash = storedHash.Hash,
            FileLocation = storedHash.RelativePath
        };

        #endregion


        #region "ChangablePathHash container"

        /// <summary>
        /// Internal class that handles <see cref="RelativePath"/> and its <see cref="Hash"/>. Class is intended for local use only.
        /// </summary>
        private class ChangeablePathHash
        {
            /// <summary>
            /// Path of a file, relative against <see cref="FileSystemRepositoryConfiguration.RepositoryRootPath"/>.
            /// </summary>
            public string RelativePath
            {
                get;
                set;
            }

            /// <summary>
            /// Hash of the <see cref="RelativePath"/> file's content.
            /// </summary>
            public string Hash
            {
                get;
                set;
            }


            /// <summary>
            /// If true, the <see cref="Hash"/> and <see cref="RelativePath"/> are either changed or new (against the data stored in DB) and an action needs to be performed.
            /// <para>If false, the <see cref="Hash"/> and <see cref="RelativePath"/> are same as the one in the DB and no action needs to be performed.</para>
            /// </summary>
            public bool Changed
            {
                get;
                set;
            }


            /// <summary>
            /// If true, the <see cref="Hash"/> and <see cref="RelativePath"/> was loaded from or merged with DB version (identified by <see cref="RelativePath"/>).
            /// <para>If false, the <see cref="Hash"/> and <see cref="RelativePath"/> was added manually and/or no merge with DB was performed.</para>
            /// </summary>
            public bool FromDb
            {
                get;
                set;
            }
        }

        #endregion


        #region "Private fields and properties"
        
        // Stores collection of relative (file) paths, files' hashes and their state against DB, indexed by relative path.
        private readonly Dictionary<string, ChangeablePathHash> mRepositoryLocationsHashes = new Dictionary<string, ChangeablePathHash>();

        /// <summary>
        /// Stores collection of relative (file) paths, files' hashes and their state against DB, indexed by relative path.
        /// </summary>
        private Dictionary<string, ChangeablePathHash> RepositoryLocationsHashes
        {
            get
            {
                return mRepositoryLocationsHashes;
            }
        }
        
        #endregion


        #region "Public methods"

        /// <summary>
        /// Saves hash of the file on given location.
        /// </summary>
        /// <param name="hashAlgorithm">Hash algorithm.</param>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <exception cref="ArgumentException">Thrown when either <paramref name="relativePath"/> is null or hash retrieved from <paramref name="hashAlgorithm"/> was invalid.</exception>
        public void SaveHash(HashAlgorithm hashAlgorithm, string relativePath)
        {
            var hash = CryptoHelper.BytesToHexa(hashAlgorithm.Hash);
            SaveHash(hash, relativePath);
        }


        /// <summary>
        /// Saves hash of the file on given location.
        /// </summary>
        /// <param name="hash">Hash itself.</param>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <exception cref="ArgumentException">Thrown when either <paramref name="relativePath"/> or <paramref name="hash"/> is null or empty string.</exception>
        public void SaveHash(string hash, string relativePath)
        {
            CheckRelativePath(relativePath);
            if (String.IsNullOrWhiteSpace(hash))
            {
                throw new ArgumentException("Hash parameter cannot be null or empty. To remove hash, use RemoveHash method.", "hash");
            }

            if (IsNewRelativePath(relativePath) || RepositoryLocationsHashes[relativePath] == PATH_HASH_REMOVED)
            {
                // Relative path (and hash) was either not stored in the manager yet or the path was marked for removal (thus new ChangablePathHash must be created)
                RepositoryLocationsHashes[relativePath] =
                    new ChangeablePathHash
                    {
                        RelativePath = relativePath,
                        Hash = hash,
                        Changed = true
                    };
            }
            else
            {
                if (IsSameHashAsExisting(hash, relativePath))
                {
                    // Re-saving existing relative path and hash is not a change
                    return;
                }

                // Relative path already existed within the manager, but hash is different so the item record must be changed
                var record = RepositoryLocationsHashes[relativePath];
                record.Hash = hash;
                record.Changed = true;
            }
        }


        /// <summary>
        /// Removes hash of the file on given location.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty</exception>
        public void RemoveHash(string relativePath)
        {
            CheckRelativePath(relativePath);

            RepositoryLocationsHashes[relativePath] = PATH_HASH_REMOVED;
        }


        /// <summary>
        /// Returns true, if <paramref name="hash"/> differs from hash stored within the <see cref="RepositoryHashManager"/>
        /// under the <paramref name="relativePath"/> key or if the key is not present, false otherwise.
        /// </summary>
        /// <param name="hash">Hash to check.</param>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <exception cref="ArgumentException">Thrown when either <paramref name="relativePath"/> or <paramref name="hash"/> is null or empty string.</exception>
        public bool HasHashChanged(string hash, string relativePath)
        {
            CheckRelativePath(relativePath);
            if (String.IsNullOrWhiteSpace(hash))
            {
                throw new ArgumentException("Hash parameter cannot be null or empty.", "hash");
            }

            return IsNewRelativePath(relativePath) || !IsSameHashAsExisting(hash, relativePath);
        }


        /// <summary>
        /// Returns hash of the file on given location.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty</exception>
        public string GetHash(string relativePath)
        {
            CheckRelativePath(relativePath);

            ChangeablePathHash changablePathHash;
            return RepositoryLocationsHashes.TryGetValue(relativePath, out changablePathHash)
                ? changablePathHash.Hash
                : null;
        }


        /// <summary>
        /// Clears all stored hashes, effectively reseting the object.
        /// </summary>
        public void Clear()
        {
            RepositoryLocationsHashes.Clear();
        }


        /// <summary>
        /// Returns collection of relative location of files and their hashes.
        /// </summary>
        public IDictionary<string, string> GetStoredHashes()
        {
            return RepositoryLocationsHashes.ToDictionary(
                hash => hash.Key,
                hash => (hash.Value == PATH_HASH_REMOVED) ? null : hash.Value.Hash);
        }


        /// <summary>
        /// Loads <see cref="FileMetadataInfo"/>s from DB and stores retrieved hashes within <see cref="RepositoryHashManager"/>.
        /// <para>Hashes of file paths that are not stored in the DB are preserved.</para>
        /// <para>Hashes of file paths that are not stored in the <see cref="RepositoryHashManager"/> are appended.</para>
        /// <para>
        /// Hashes of file paths stored in the DB overwrite the ones that eventually exist within <see cref="RepositoryHashManager"/>
        /// provided <paramref name="overwriteExistingHashes"/> is set to true (otherwise original hash is preserved and record is marked for update).
        /// </para>
        /// </summary>
        /// <param name="overwriteExistingHashes">If true, existing hashes are overwritten by data loaded from DB, otherwise, the distinct hashes are preserved and marked for update.</param>
        /// <param name="loadOnlyStoredMetadata">If true, only hashes of file paths that are stored in the <see cref="RepositoryHashManager"/> are loaded from DB, otherwise all hashes are loaded.</param>
        public void LoadFilesMetadataFromDatabase(bool overwriteExistingHashes = true, bool loadOnlyStoredMetadata = false)
        {
            var query = FileMetadataInfoProvider.GetFileMetadataInfos();
            if (loadOnlyStoredMetadata)
            {
                query = query.WhereIn("FileLocation", RepositoryLocationsHashes.Keys);
            }

            var mergedHashes = query.ToDictionary(
                    metadataInfo => metadataInfo.FileLocation,
                    metadataInfo => SelectChangableHashMergedWithMetadata(metadataInfo, overwriteExistingHashes),
                    StringComparer.InvariantCultureIgnoreCase);

            // Merge DB hashes with existing hashes collection
            foreach (var mergedHash in mergedHashes)
            {
                RepositoryLocationsHashes[mergedHash.Key] = mergedHash.Value;
            }
        }


        /// <summary>
        /// Stores relative file locations and the files' hashes in the database using <see cref="FileMetadataInfoProvider"/>.
        /// <para>New hashes are added to the DB. Removed hashes are deleted from the DB. Amended hashes are updated in the DB.</para>
        /// </summary>
        /// <param name="updateOnlyStoredMetadata">If true, only metadata that are stored in the <see cref="RepositoryHashManager"/> are updated,
        ///  otherwise all hashes are processed. Use true for update after single object operation, false for mass operation (Store all, Restore all)</param>
        public void UpdateFilesMetadataInDatabase(bool updateOnlyStoredMetadata = false)
        {
            if (!RepositoryLocationsHashes.Any())
            {
                // No records, no operation required
                return;
            }

            LoadFilesMetadataFromDatabase(overwriteExistingHashes: false, loadOnlyStoredMetadata: updateOnlyStoredMetadata);

            RemoveMarkedHashes();

            UpdateExistingHashes();

            BulkInsertNewHashes();
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns true if <paramref name="relativePath"/> does not exist within <see cref="RepositoryLocationsHashes"/>.
        /// </summary>
        /// <param name="relativePath">Path of a file, relative against <see cref="FileSystemRepositoryConfiguration.RepositoryRootPath"/>.</param>
        private bool IsNewRelativePath(string relativePath)
        {
            return !RepositoryLocationsHashes.ContainsKey(relativePath);
        }


        /// <summary>
        /// Returns true if <paramref name="newHash"/> does not match the hash already stored under <paramref name="relativePath"/> within <see cref="RepositoryLocationsHashes"/>.
        /// </summary>
        /// <param name="newHash">Hash of the (amended) <paramref name="relativePath"/> file's content.</param>
        /// <param name="relativePath">Path of a file, relative against <see cref="FileSystemRepositoryConfiguration.RepositoryRootPath"/>.</param>
        /// <remarks>Method expects the <paramref name="relativePath"/> to be valid and present key within the <see cref="RepositoryLocationsHashes"/> (with not-null value assigned).</remarks>
        private bool IsSameHashAsExisting(string newHash, string relativePath)
        {
            if (RepositoryLocationsHashes[relativePath] == PATH_HASH_REMOVED)
            {
                // When hash is marked for removal, it is never same as any other hash (acts as a different state)
                return false;
            }

            // Otherwise, compare hashes
            var originalHash = RepositoryLocationsHashes[relativePath].Hash;
            return String.Equals(originalHash, newHash, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Throws <see cref="ArgumentException"/> if the provided <paramref name="relativePath"/> is null or empty (or white spaced).
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is null or empty</exception>
        private static void CheckRelativePath(string relativePath)
        {
            if (String.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("Relative path cannot be null or empty.", "relativePath");
            }
        }


        /// <summary>
        /// Returns a record that is result of merge between <paramref name="metadataInfo"/> loaded from DB
        /// and (potentially) existing <see cref="ChangeablePathHash"/> record already present within <see cref="RepositoryHashManager"/>.
        /// <para>The way the merge is performed depends on value of <paramref name="overwriteExistingHashes"/>.</para>
        /// </summary>
        /// <param name="metadataInfo">File meta-data info loaded from DB.</param>
        /// <param name="overwriteExistingHashes">If true, existing hash is overwritten by data loaded from DB, otherwise, the original hash is preserved and marked for update.</param>
        private ChangeablePathHash SelectChangableHashMergedWithMetadata(FileMetadataInfo metadataInfo, bool overwriteExistingHashes)
        {
            // Decide whether preserve existing hash (false also if the hash does not exist yet)
            bool preserveExistingHash = !overwriteExistingHashes
                && !IsNewRelativePath(metadataInfo.FileLocation)
                && !IsSameHashAsExisting(metadataInfo.FileHash, metadataInfo.FileLocation);

            ChangeablePathHash loadedHash;
            if (preserveExistingHash)
            {
                // Use record that already exists, only mark is different from DB (~ changed)
                loadedHash = RepositoryLocationsHashes[metadataInfo.FileLocation];

                if (loadedHash == PATH_HASH_REMOVED)
                {
                    // When path is marked for removal, it has to remained marked for removal
                    return loadedHash;
                }

                // Set changed property depending on similarity of current and metadataInfo hashes
                loadedHash.Changed = true;
            }
            else
            {
                // Create new record, whether it is new record or existing one that will be overwritten
                loadedHash = new ChangeablePathHash
                {
                    RelativePath = metadataInfo.FileLocation,
                    Hash = metadataInfo.FileHash,
                    Changed = false,
                };
            }

            // Mark record as loaded from DB and return it
            loadedHash.FromDb = true;
            return loadedHash;
        }


        /// <summary>
        /// Removes (marked) location hashes from DB.
        /// </summary>
        private void RemoveMarkedHashes()
        {
            var removedHashesLocations = RepositoryLocationsHashes
                .Where(hashPair => hashPair.Value == PATH_HASH_REMOVED)
                .Select(hashPair => hashPair.Key)
                .ToArray();

            if (removedHashesLocations.Any())
            {
                FileMetadataInfoProvider.ProviderObject.BulkDelete(new WhereCondition().WhereIn("FileLocation", removedHashesLocations));
            }
        }


        /// <summary>
        /// Updates amended hashes in DB.
        /// </summary>
        private void UpdateExistingHashes()
        {
            var metadataToUpdate = RepositoryLocationsHashes
                .Values
                .Where(hash => hash.Changed && hash.FromDb)
                .Select(SELECT_METADATA_FROM_CHANGABLE_HASH_DELEGATE)
                .ToArray();

            foreach (var fileMetadataInfo in metadataToUpdate)
            {
                FileMetadataInfoProvider.SetFileMetadataInfo(fileMetadataInfo);
            }
        }


        /// <summary>
        /// Inserts new hashes into DB - file meta-data will be inserted in single bulk operation.
        /// </summary>
        private void BulkInsertNewHashes()
        {
            var metadataToInsert = RepositoryLocationsHashes
                .Values
                .Where(hash => hash.Changed && !hash.FromDb)
                .Select(SELECT_METADATA_FROM_CHANGABLE_HASH_DELEGATE)
                .ToArray();

            if (metadataToInsert.Any())
            {
                FileMetadataInfoProvider.BulkInsertFileMetadataInfos(metadataToInsert);
            }
        }

        #endregion
    }
}
