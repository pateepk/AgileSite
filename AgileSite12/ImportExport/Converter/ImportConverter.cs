using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using CMS.Helpers;
using CMS.IO;
using CMS.Base;

using Directory = CMS.IO.Directory;
using DirectoryInfo = CMS.IO.DirectoryInfo;
using File = CMS.IO.File;
using FileInfo = CMS.IO.FileInfo;
using IOExceptions = System.IO;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Import package conversion.
    /// </summary>
    internal static class ImportConverter
    {
        private static List<string> mVersions;
        private static Dictionary<string, KeyValuePair<string, string>> mNamespaces;


        /// <summary>
        /// Array of incompatible versions. (Target version of conversion.)
        /// </summary>
        private static IEnumerable<string> Versions
        {
            get
            {
                return mVersions ?? (mVersions = new List<string> { "11.0" });
            }
        }


        /// <summary>
        /// Set of namespaces conversions. [version] -> [[old_namespace] -> [new_namespace]]
        /// </summary>
        private static Dictionary<string, KeyValuePair<string, string>> Namespaces
        {
            get
            {
                if (mNamespaces == null)
                {
                    mNamespaces = new Dictionary<string, KeyValuePair<string, string>>();
                    // Example:
                    // mNamespaces["7.0"] = new KeyValuePair<string, string>("CMS.TreeEngine", "CMS.DocumentEngine");
                }

                return mNamespaces;
            }
        }


        #region "Public methods"

        /// <summary>
        /// Ensures conversion of namespaces between versions.
        /// </summary>
        /// <param name="settings">Site import settings</param>
        /// <param name="filePath">File path</param>
        public static void EnsureNamespaceConversion(SiteImportSettings settings, string filePath)
        {
            if (ImportExportHelper.IsCodeFile(filePath))
            {
                foreach (string version in Namespaces.Keys)
                {
                    // Conversion should be processed
                    if (settings.IsLowerVersion(version))
                    {
                        // Get conversion
                        KeyValuePair<string, string> conv = Namespaces[version];
                        // Process conversion
                        string pattern = "{0}";
                        if (filePath.EndsWithCSafe(".cs", true))
                        {
                            pattern = "using {0};";
                        }
                        else if (filePath.EndsWithCSafe(".vb", true))
                        {
                            pattern = "Imports {0}";
                        }

                        FileHelper.ReplaceInFile(filePath, string.Format(pattern, conv.Key), string.Format(pattern, conv.Value));
                    }
                }
            }
        }


        /// <summary>
        /// Ensures all actions for conversion.
        /// </summary>
        /// <param name="settings">Site import settings</param>
        public static string EnsureConversion(SiteImportSettings settings)
        {
            // List of necessary conversions
            var conversions = new List<string>();

            // Get conversions to process
            foreach (string conversionVersion in Versions)
            {
                // Conversion should be processed
                if (ImportExportHelper.IsLowerVersion(settings.Version, null, conversionVersion, null))
                {
                    conversions.Add(conversionVersion);
                }
            }

            // Convert package
            return ConvertPackage(settings, conversions);
        }


        /// <summary>
        /// Process all necessary conversions.
        /// </summary>
        /// <param name="originalSettings">Original site import settings</param>
        /// <param name="conversions">Conversions to be processed</param>
        private static string ConvertPackage(SiteImportSettings originalSettings, List<string> conversions)
        {
            // No conversion needed, return source file path
            if (conversions.Count == 0)
            {
                return originalSettings.SourceFilePath;
            }

            // Set target path 
            string sourcePath = originalSettings.SourceFilePath + ImportExportHelper.DATA_FOLDER;
            string targetPath = originalSettings.TemporaryFilesPath;
            string websitePath = originalSettings.WebsitePath;

            // Keep old cultures
            var oldCulture = Thread.CurrentThread.CurrentCulture;
            var oldUICulture = Thread.CurrentThread.CurrentUICulture;

            // Set current cultures to English
            Thread.CurrentThread.CurrentCulture = CultureHelper.EnglishCulture;
            Thread.CurrentThread.CurrentUICulture = CultureHelper.EnglishCulture;

            // Initialize settings and set version
            var exSettings = new SiteExportSettings(originalSettings.CurrentUser);
            var imSettings = new SiteImportSettings(originalSettings.CurrentUser);
            imSettings.Version = originalSettings.Version;
            imSettings.HotfixVersion = originalSettings.HotfixVersion;

            // Set web site path
            if (websitePath != null)
            {
                exSettings.WebsitePath = websitePath;
                imSettings.WebsitePath = websitePath;
            }

            using (CMSActionContext context = new CMSActionContext(originalSettings.CurrentUser))
            {
                context.LogEvents = false;

                try
                {
                    // Process all necessary conversions
                    foreach (string conversion in conversions)
                    {
                        switch (conversion)
                        {
                            case "11.0":
                                EnsureSeparateFolder(exSettings, imSettings, targetPath, conversion);
                                CopyDirectory(sourcePath, exSettings.TemporaryFilesPath + ImportExportHelper.DATA_FOLDER);
                                // Place for future conversions to version 11.0
                                break;

                            default:
                                throw new Exception("[ImportConverter.ConvertPackage]: Conversion for version " + conversion + " not defined!");
                        }
                    }
                }
                finally
                {
                    // Set back current cultures
                    Thread.CurrentThread.CurrentCulture = oldCulture;
                    Thread.CurrentThread.CurrentUICulture = oldUICulture;
                }
            }

            // Return new temporary files path
            return exSettings.TemporaryFilesPath;
        }


        /// <summary>
        /// Ensures separate folder for version conversion to keep track of package structure
        /// </summary>
        /// <param name="exSettings">Export settings</param>
        /// <param name="imSettings">Import settings</param>
        /// <param name="targetPath">Target files path</param>
        /// <param name="conversion">Conversion number</param>
        private static void EnsureSeparateFolder(SiteExportSettings exSettings, SiteImportSettings imSettings, string targetPath, string conversion)
        {
            // Set temporary files paths
            string conversionPath = targetPath + "##" + conversion.Replace(".", "_") + "##";
            exSettings.TemporaryFilesPath = conversionPath;
            imSettings.TemporaryFilesPath = conversionPath;
        }
                
        #endregion
        

        #region "Helper methods"

        /// <summary>
        /// Copy file for the import and unset the read-only attributes.
        /// </summary>
        /// <param name="sourcePath">Source path</param>
        /// <param name="destPath">Destination path</param>
        private static void CopyFile(string sourcePath, string destPath)
        {
            try
            {
                FileInfo sourceInfo = FileInfo.New(sourcePath);
                FileInfo targetInfo = FileInfo.New(destPath);

                if (sourceInfo.Exists)
                {
                    // Check if the copy file is necessary
                    bool copyFile = false;
                    if (!targetInfo.Exists)
                    {
                        copyFile = true;
                    }
                    else
                    {
                        if (sourceInfo.Length != targetInfo.Length)
                        {
                            // Different length, different file
                            copyFile = true;
                        }
                        else
                        {
                            try
                            {
                                // Compare the content
                                byte[] oldContent = File.ReadAllBytes(sourcePath);
                                byte[] newContent = File.ReadAllBytes(destPath);

                                // If content is different, copy the file
                                if (DataHelper.CompareByteArrays(oldContent, newContent))
                                {
                                    copyFile = true;
                                }
                            }
                            catch
                            {
                                // On error copy the file
                                copyFile = true;
                            }
                        }
                    }

                    // If target file doesn't exist or has different size copy file
                    if (copyFile)
                    {
                        // Copy file
                        DirectoryHelper.CopyFile(sourcePath, destPath);
                    }
                }
            }
            catch (IOExceptions.FileNotFoundException)
            {
            }
            catch (IOExceptions.DirectoryNotFoundException)
            {
            }
        }


        /// <summary>
        /// Copy directory including subdirectories and files.
        /// </summary>
        /// <param name="sourcePath">Source path</param>
        /// <param name="targetPath">Target path</param>
        private static void CopyDirectory(string sourcePath, string targetPath)
        {
            if (!Directory.Exists(sourcePath))
            {
                return;
            }

            DirectoryInfo sourceFolder = DirectoryInfo.New(sourcePath);

            // Create the directory if not exists
            if (!Directory.Exists(targetPath))
            {
                DirectoryHelper.CreateDirectory(targetPath);
            }

            // Copy subfolders
            foreach (DirectoryInfo subFolder in sourceFolder.GetDirectories())
            {
                if (!subFolder.Name.EndsWithCSafe("##NEW##"))
                {
                    CopyDirectory(DirectoryHelper.CombinePath(sourcePath, subFolder.Name), DirectoryHelper.CombinePath(targetPath, subFolder.Name));
                }
            }

            // Copy files
            foreach (FileInfo sourceFile in sourceFolder.GetFiles())
            {
                CopyFile(sourceFile.FullName, DirectoryHelper.CombinePath(targetPath, sourceFile.Name));
            }
        }

        #endregion
    }
}