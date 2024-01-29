using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using CMS.Base;
using CMS.Helpers;
using CMS.IO;

using SystemIO = System.IO;


namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Contains methods necessary to define and validate the behavior of file system repository. 
    /// </summary>
    internal class FileSystemRepositoryHelper : AbstractHelper<FileSystemRepositoryHelper>
    {
        #region "Constants"

        /// <summary>
        /// Delimiter between name and hash.
        /// </summary>
        protected const char HASH_DELIMITER = '@';


        /// <summary>
        /// Delimiter between parts of shortened file name.
        /// </summary>
        protected const string SHORTENED_FILENAME_PART_DELIMITER = "..";


        private const char RESERVED_FILE_SYSTEM_NAME_PREFIX_DELIMITER_REPLACEMENT = '-';

        #endregion


        #region "Variables"

        /// <summary>
        /// Hash algorithm, does not need to be cryptographically safe, but collision resistance is desired.
        /// </summary>
        private static readonly HashAlgorithm HashAlgorithm = FileSystemCryptoHelper.GetHashAlgorithm();


        /// <summary>
        /// Lock object for synchronization of hash computation (non-static members of <see cref="HashAlgorithm"/> are not guaranteed to be thread safe).
        /// </summary>
        private static readonly object HashAlgorithmLock = new object();

        #endregion


        #region "Public static members"

        /// <summary>
        /// Gets file system safe representation of <paramref name="name"/>. The resulting name does not exceed <paramref name="maxNameLength"/> characters in length.
        /// <paramref name="maxNameLength"/> must accommodate <paramref name="hashLength"/> and delimiting character.
        /// </summary>
        /// <param name="name">Name to be transformed to file system safe representation.</param>
        /// <param name="maxNameLength">Max length of the resulting name.</param>
        /// <param name="hashLength">Number of hash characters to be included in the resulting name, if hashing is needed.</param>
        /// <returns>Name in file system safe representation.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="hashLength"/> is not a positive integer or <paramref name="maxNameLength"/>is not greater than (hash length + 1) - the extra character is for hash delimiter.</exception>
        /// <remarks>
        /// The <paramref name="name"/> processing is similar to code name processing. Diacritics in Latin characters is removed,
        /// illegal characters are replaced by '_' (underscore), multiple '.' (dot) are grouped to single '.',
        /// leading and trailing '.' and '_' are trimmed.
        /// </remarks>
        public static string GetFileSystemName(string name, int maxNameLength, int hashLength)
        {
            return GetFileSystemName(name, null, maxNameLength, hashLength);
        }


        /// <summary>
        /// Gets file system safe representation of <paramref name="name"/>. The resulting name does not exceed <paramref name="maxNameLength"/> characters in length.
        /// <paramref name="maxNameLength"/> must accommodate <paramref name="hashLength"/> and delimiting character.
        /// </summary>
        /// <param name="name">Name to be transformed to file system safe representation.</param>
        /// <param name="fullName">Full name for given <paramref name="name"/>. The full name is used for the purpose of hash computation. Null means the full name is the same as <paramref name="name"/>.</param>
        /// <param name="maxNameLength">Max length of the resulting name.</param>
        /// <param name="hashLength">Number of hash characters to be included in the resulting name, if hashing is needed.</param>
        /// <returns>Name in file system safe representation.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty or <paramref name="fullName"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="hashLength"/> is not a positive integer or <paramref name="maxNameLength"/>is not greater than (hash length + 1) - the extra character is for hash delimiter.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="name"/> processing is similar to code name processing. Diacritics in Latin characters is removed,
        /// illegal characters are replaced by '_' (underscore), multiple '.' (dot) are grouped to single '.',
        /// leading and trailing '.' and '_' are trimmed.
        /// </para>
        /// <para>
        /// Specify the <paramref name="fullName"/> parameter to avoid file system name collisions when <paramref name="name"/> is some shortcut or human-readable form of <paramref name="fullName"/>.
        /// </para>
        /// </remarks>
        /// <example>
        /// The following code creates 2 file system names, which differ in their hash only
        /// <code>
        /// string articleFileSystemName1 = FileSystemRepositoryHelper.GetFileSystemName("Article", "/Path/To/Some/Document/Named/Article", 50, 10);
        /// string articleFileSystemName2 = FileSystemRepositoryHelper.GetFileSystemName("Article", "/Path/To/Different/Document/Named/Article", 50, 10);
        /// 
        /// // Produces result similar to:
        /// //   article@1234567890
        /// //   article@0987654321
        /// </code>
        /// The following code shows how to create a human-readable file name from a name, which would otherwise result in hash only (due to special characters processing)
        /// <code>
        /// string defaultFileSystemName = FileSystemRepositoryHelper.GetFileSystemName("/", 50, 10);
        /// string customFileSystemName = FileSystemRepositoryHelper.GetFileSystemName("root", "/", 50, 10);
        /// 
        /// // Produces result similar to:
        /// //   @1234567890
        /// //   root@1234567890
        /// </code>
        /// The following code demonstrates the difference between providing a name and full name pair for obtaining a human-readable name and why the same can not be achieved with name only.
        /// Let's assume we have an object representing a root of a tree (named "/") and we want a human-readable file system name to store such object
        /// <code>
        /// string customFileSystemName = FileSystemRepositoryHelper.GetFileSystemName("root", "/", 50, 10);
        /// string wrongCustomFileSystemName = FileSystemRepositoryHelper.GetFileSystemName("root", 50, 10); // This is wrong. If another object named "root" would exist, its name would collide with the custom name for "/"
        /// 
        /// // Produces result similar to:
        /// //   root@1234567890           The hash value is computed from "/"
        /// //   root
        /// </code>
        /// </example>
        public static string GetFileSystemName(string name, string fullName, int maxNameLength, int hashLength)
        {
            return HelperObject.GetFileSystemNameInternal(name, fullName, maxNameLength, hashLength);
        }


        /// <summary>
        /// Deletes a directory specified by <paramref name="directoryPath"/>.
        /// </summary>
        /// <remarks>
        /// If the delete method throws an <see cref="SystemIO.IOException"/> with <see cref="Exception.HResult"/> 0x80070091 (ERROR_DIR_NOT_EMPTY), it is ignored.
        /// </remarks>
        /// <param name="directoryPath">Absolute path to the directory.</param>
        /// <param name="recursive">True to remove directories, subdirectories, and files in path, otherwise false.</param>
        /// <returns>False if the delete method throws an <see cref="SystemIO.IOException"/> with <see cref="Exception.HResult"/> 0x80070091 (ERROR_DIR_NOT_EMPTY), otherwise true.</returns>
        public static bool DeleteDirectory(string directoryPath, bool recursive = false)
        {
            return HelperObject.DeleteDirectoryInternal(directoryPath, recursive);
        }


        /// <summary>
        /// Deletes repository directory identified by <paramref name="relativeDirectoryPath"/>. Does nothing when directory is not empty
        /// or relative path is null or empty string (to prevent unintended deletion of repository root).
        /// </summary>
        /// <param name="relativeDirectoryPath">Path within repository identifying the directory to be deleted.</param>
        /// <param name="repositoryRootPath">Root path of the repository.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repositoryRootPath"/> is null.</exception>
        public static void DeleteRepositoryDirectoryIfEmpty(string relativeDirectoryPath, string repositoryRootPath)
        {
            HelperObject.DeleteRepositoryDirectoryIfEmptyInternal(relativeDirectoryPath, repositoryRootPath);
        }

        #endregion


        #region "Internal overridable members"

        /// <summary>
        /// Gets file system safe representation of <paramref name="name"/>. The resulting name does not exceed <paramref name="maxNameLength"/> characters in length.
        /// <paramref name="maxNameLength"/> must accommodate <paramref name="hashLength"/> and delimiting character.
        /// </summary>
        /// <param name="name">Name to be transformed to file system safe representation.</param>
        /// <param name="fullName">Full name for given <paramref name="name"/>. The full name is used for the purpose of hash computation. Null means the full name is the same as <paramref name="name"/>.</param>
        /// <param name="maxNameLength">Max length of the resulting name.</param>
        /// <param name="hashLength">Number of hash characters to be included in the resulting name, if hashing is needed.</param>
        /// <returns>Name in file system safe representation.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty or <paramref name="fullName"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="hashLength"/> is not a positive integer or <paramref name="maxNameLength"/>is not greater than (hash length + 1) - the extra character is for hash delimiter.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="name"/> processing is similar to code name processing. Diacritics in Latin characters is removed,
        /// illegal characters are replaced by '_' (underscore), multiple '.' (dot) are grouped to single '.',
        /// leading and trailing '.' and '_' are trimmed.
        /// </para>
        /// <para>
        /// Specify the <paramref name="fullName"/> parameter to avoid file system name collisions when <paramref name="name"/> is some shortcut or human-readable form of <paramref name="fullName"/>.
        /// </para>
        /// </remarks>
        /// <example>
        /// The following code creates 2 file system names, which differ in their hash only
        /// <code>
        /// string articleFileSystemName1 = FileSystemRepositoryHelper.GetFileSystemName("Article", "/Path/To/Some/Document/Named/Article", 50, 10);
        /// string articleFileSystemName2 = FileSystemRepositoryHelper.GetFileSystemName("Article", "/Path/To/Different/Document/Named/Article", 50, 10);
        /// 
        /// // Produces result similar to:
        /// //   article@1234567890
        /// //   article@0987654321
        /// </code>
        /// The following code shows how to create a human-readable file name from a name, which would otherwise result in hash only (due to special characters processing)
        /// <code>
        /// string defaultFileSystemName = FileSystemRepositoryHelper.GetFileSystemName("/", 50, 10);
        /// string customFileSystemName = FileSystemRepositoryHelper.GetFileSystemName("root", "/", 50, 10);
        /// 
        /// // Produces result similar to:
        /// //   @1234567890
        /// //   root@1234567890
        /// </code>
        /// The following code demonstrates the difference between providing a name and full name pair for obtaining a human-readable name and why the same can not be achieved with name only.
        /// Let's assume we have an object representing a root of a tree (named "/") and we want a human-readable file system name to store such object
        /// <code>
        /// string customFileSystemName = FileSystemRepositoryHelper.GetFileSystemName("root", "/", 50, 10);
        /// string wrongCustomFileSystemName = FileSystemRepositoryHelper.GetFileSystemName("root", 50, 10); // This is wrong. If another object named "root" would exist, its name would collide with the custom name for "/"
        /// 
        /// // Produces result similar to:
        /// //   root@1234567890           The hash value is computed from "/"
        /// //   root
        /// </code>
        /// </example>
        protected virtual string GetFileSystemNameInternal(string name, string fullName, int maxNameLength, int hashLength)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name must not be null or empty string.", "name");
            }
            if ((fullName != null) && (fullName.Length == 0))
            {
                throw new ArgumentException("Full name must not be empty string.", "fullName");
            }
            if (hashLength <= 0)
            {
                throw new ArgumentOutOfRangeException("hashLength", "Hash length must be a positive integer.");
            }
            if (hashLength + 1 >= maxNameLength)
            {
                throw new ArgumentOutOfRangeException("maxNameLength", "Maximum name length must be greater than (hash length + delimiter).");
            }

            return GetFileSystemNameUnchecked(name, fullName ?? name, maxNameLength, hashLength);
        }


        /// <summary>
        /// Deletes a directory specified by <paramref name="directoryPath"/>. If the delete method
        /// throws an <see cref="SystemIO.IOException"/> with <see cref="Exception.HResult"/> 0x80070091 (ERROR_DIR_NOT_EMPTY), it is ignored.
        /// </summary>
        /// <param name="directoryPath">Absolute path to the directory.</param>
        /// <param name="recursive">True to remove directories, subdirectories, and files in path, otherwise false.</param>
        /// <returns>False if the delete method throws an <see cref="SystemIO.IOException"/> with <see cref="Exception.HResult"/> 0x80070091 (ERROR_DIR_NOT_EMPTY), otherwise true.</returns>
        protected virtual bool DeleteDirectoryInternal(string directoryPath, bool recursive)
        {
            const int ERROR_DIR_NOT_EMPTY = -2147024751; // 0x80070091

            try
            {
                Directory.Delete(directoryPath, recursive);
            }
            catch (SystemIO.IOException ex)
            {
                if (ex.HResult != ERROR_DIR_NOT_EMPTY)
                {
                    // MSDN: In some cases, if you have the specified directory open in File Explorer, the Delete method may not be able to delete it.
                    throw;
                }

                return false;
            }

            return true;
        }


        /// <summary>
        /// Deletes repository directory identified by <paramref name="relativeDirectoryPath"/>. Does nothing when directory is not empty
        /// or relative path is null or empty string (to prevent unintended deletion of repository root).
        /// </summary>
        /// <param name="relativeDirectoryPath">Path within repository identifying the directory to be deleted.</param>
        /// <param name="repositoryRootPath">Root path of the repository.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repositoryRootPath"/> is null.</exception>
        protected virtual void DeleteRepositoryDirectoryIfEmptyInternal(string relativeDirectoryPath, string repositoryRootPath)
        {
            if (String.IsNullOrEmpty(relativeDirectoryPath))
            {
                return;
            }
            if (repositoryRootPath == null)
            {
                throw new ArgumentNullException("repositoryRootPath");
            }

            var absolutePath = Path.Combine(repositoryRootPath, relativeDirectoryPath);
            if (!Directory.GetFiles(absolutePath).Any() && !Directory.GetDirectories(absolutePath).Any())
            {
                DeleteDirectory(absolutePath);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets file system safe representation of <paramref name="name"/>. The resulting name does not exceed <paramref name="maxNameLength"/> characters in length.
        /// The method does not check the length parameters for validity. <paramref name="maxNameLength"/> must accommodate <paramref name="hashLength"/> and delimiting character.
        /// </summary>
        /// <param name="name">Name to be transformed to file system safe representation.</param>
        /// <param name="fullName">Full name for given <paramref name="name"/>. The full name is used for the purpose of hash computation.</param>
        /// <param name="maxNameLength">Max length of the resulting name.</param>
        /// <param name="hashLength">Number of hash characters to be included in the resulting name, if hashing is needed.</param>
        /// <returns>Name in file system safe representation.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="name"/> processing is similar to code name processing. Diacritics in Latin characters is removed,
        /// illegal characters are replaced by '_' (underscore), multiple '.' (dot) are grouped to single '.',
        /// leading and trailing '.' and '_' are trimmed.
        /// </para>
        /// <para>
        /// Specify the <paramref name="fullName"/> parameter to avoid file system name collisions when <paramref name="name"/> is some shortcut or human-readable form of <paramref name="fullName"/>.
        /// </para>
        /// </remarks>
        private string GetFileSystemNameUnchecked(string name, string fullName, int maxNameLength, int hashLength)
        {
            string lowerCaseFullName = fullName.ToLowerInvariant();
            string fileSystemCustomName = name.ToLowerInvariant();
            string fileSystemSafeName = ReplaceUnsafeFileSystemCharacters(fileSystemCustomName);
            fileSystemSafeName = ReplaceReservedPrefix(fileSystemSafeName);

            // If file system name is different, add hash computed from original lower-cased name. Do the same if name contains hash delimiting character (to avoid collisions). Also use hash when collision with reserved name occurs.
            bool hashNeeded = !lowerCaseFullName.Equals(fileSystemSafeName, StringComparison.InvariantCultureIgnoreCase) || fileSystemSafeName.Contains(HASH_DELIMITER) || ValidationHelper.RESERVED_FILE_SYSTEM_NAMES.Contains(fileSystemSafeName);
            bool shortened = false;

            var fileSystemSafeNameLength = fileSystemSafeName.Length;

            // Compute maximum length of file system safe name which needs to be appended with a hash (hash length + one delimiting character).
            int maxHashedNameLength = maxNameLength - (hashLength + 1);

            if ((fileSystemSafeNameLength > maxNameLength) || (hashNeeded && fileSystemSafeNameLength > maxHashedNameLength))
            {
                // Shorten to meet max length requirement
                fileSystemSafeName = TextHelper.LimitLength(fileSystemSafeName, maxHashedNameLength, SHORTENED_FILENAME_PART_DELIMITER, false, CutTextEnum.Middle);
                shortened = true;
            }

            // If file system name was shortened, or hash is explicitly needed, add hash
            if (shortened || hashNeeded)
            {
                fileSystemSafeName = String.Format("{0}{1}{2}", fileSystemSafeName, HASH_DELIMITER, ComputeFileNameHash(lowerCaseFullName, hashLength));
            }

            return fileSystemSafeName;
        }


        /// <summary>
        /// Searches for occurrence of one of <see cref="ValidationHelper.RESERVED_FILE_SYSTEM_NAMES"/> as a prefix in <paramref name="name"/>, delimited by a '.' (dot) character.
        /// If such prefix is found, the delimiter is replaced by '-' (dash) (e.g. 'com1.something' is replaced by 'com1-something).
        /// </summary>
        private string ReplaceReservedPrefix(string name)
        {
            var prefix = ValidationHelper.FindReservedFileSystemNamePrefix(name);
            if (prefix == null)
            {
                return name;
            }

            return ReplaceAt(name, prefix.Length, RESERVED_FILE_SYSTEM_NAME_PREFIX_DELIMITER_REPLACEMENT);
        }


        /// <summary>
        /// Replaces character at position specified by <paramref name="index"/> in a string. No boundary check is performed.
        /// </summary>
        private string ReplaceAt(string str, int index, char replacement)
        {
            var workingString = new StringBuilder(str);
            workingString[index] = replacement;

            return workingString.ToString();
        }


        /// <summary>
        /// Computes hash from <paramref name="value"/> and truncates it to meet <paramref name="hashLength"/>.
        /// No boundary check is performed.
        /// </summary>
        /// <param name="value">Value to be hashed.</param>
        /// <param name="hashLength">Length of resulting hash.</param>
        /// <returns>Hexadecimal representation of hash for given <paramref name="value"/>, truncated to required length.</returns>
        private string ComputeFileNameHash(string value, int hashLength)
        {
            byte[] data = ComputeHashSynchronized(Encoding.UTF8.GetBytes(value));

            return FileSystemCryptoHelper.BytesToHexa(data, hashLength);
        }


        /// <summary>
        /// Synchronization wrapper for <see cref="HashAlgorithm"/>.
        /// </summary>
        /// <param name="data">Byte array for which to compute the hash.</param>
        /// <returns>Hash bytes compute from given byte array.</returns>
        private byte[] ComputeHashSynchronized(byte[] data)
        {
            lock (HashAlgorithmLock)
            {
                return HashAlgorithm.ComputeHash(data);
            }
        }


        /// <summary>
        /// Transforms <paramref name="name"/> to a string which does not contain any illegal or possibly problematic file system characters while maintaining human readability.
        /// </summary>
        /// <param name="name">Name for which to create file system safe name.</param>
        /// <returns>File system safe name.</returns>
        private string ReplaceUnsafeFileSystemCharacters(string name)
        {
            return ValidationHelper.GetCodeName(name, useUnicode: false, removeDiacritics: true, useCamelCase: false);
        }

        #endregion
    }
}
