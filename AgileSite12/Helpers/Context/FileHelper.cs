using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Core;
using CMS.IO;
using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// File helper methods.
    /// </summary>
    public static class FileHelper
    {
        #region "Variables"

        private static readonly Lazy<ISet<string>> mTextFileExtensions = new Lazy<ISet<string>>(() => new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "css", "skin", "txt", "xml", "js", "htm", "html", "config", "bat"
        });

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the given file extension represents a text file.
        /// </summary>
        /// <param name="extension">Extension to check, can be in both format with and without dot (.jpg, jpg).</param>
        /// <remarks>
        /// This method contains only limited set of extensions used by system and should not be used in custom code.
        /// </remarks>
        public static bool IsTextFileExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }

            return mTextFileExtensions.Value.Contains(extension.TrimStart('.'));
        }


        /// <summary>
        /// Gets the live path for the given file
        /// </summary>
        /// <param name="filePath">File path</param>
        public static string GetLiveFilePath(string filePath)
        {
            // Use GetResource handler for external files or files within zipped files
            if (StorageHelper.IsExternalStorage(filePath) || StorageHelper.IsZippedFilePath(filePath))
            {
                const string format = "~/CMSPages/GetResource.ashx?{0}={1}";

                string param = "file";

                if (filePath.EndsWithCSafe(".js", true))
                {
                    param = "scriptfile";
                }
                else if (ImageHelper.IsImage(Path.GetExtension(filePath)))
                {
                    param = "image";
                }

                filePath = String.Format(format, param, filePath);
            }

            return filePath;
        }


        /// <summary>
        /// Checks if the extension is within the given list of allowed extensions.
        /// </summary>
        /// <param name="extension">Extension to check</param>
        /// <param name="allowedExtensions">List of allowed extensions separated by semicolon, e.g. "jpg;png"</param>
        public static bool CheckExtension(string extension, string allowedExtensions)
        {
            allowedExtensions = String.Format(";{0};", allowedExtensions.Replace('.', ';'));
            extension = String.Format(";{0};", extension.Trim('.'));

            return (allowedExtensions.IndexOfCSafe(extension, true) >= 0);
        }


        /// <summary>
        /// Performs the string replacement within the file.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="what">String to replace (old string)</param>
        /// <param name="replaceWith">Replacement (new string)</param>
        public static void ReplaceInFile(string filePath, string what, string replaceWith)
        {
            ReplaceInFile(filePath, what, replaceWith, null);
        }


        /// <summary>
        /// Performs the string replacement within the file.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="what">String to replace (old string)</param>
        /// <param name="replaceWith">Replacement (new string)</param>
        /// <param name="encoding">File encoding</param>
        public static void ReplaceInFile(string filePath, string what, string replaceWith, Encoding encoding)
        {
            // Read the file
            if (encoding == null)
            {
                encoding = Encoding.Default;
            }

            string txt = File.ReadAllText(filePath);

            // Perform the replacement
            txt = txt.Replace(what, replaceWith);

            File.WriteAllText(filePath, txt, encoding);
        }


        /// <summary>
        /// Returns full physical path of a file or folder. Does not change the ending slash
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="webFullPath">Full path to the root of the web project (e.g. c:\WebProject\)</param>
        public static string GetFullFilePhysicalPath(string path, string webFullPath = null)
        {
            return StorageHelper.GetFullFilePhysicalPath(path, webFullPath);
        }


        /// <summary>
        /// Returns full physical path for a folder. Ensures the trailing backslash
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="webFullPath">Full path to the root of the web project (e.g. c:\WebProject\)</param>
        public static string GetFullFolderPhysicalPath(string path, string webFullPath = null)
        {
            return StorageHelper.GetFullFolderPhysicalPath(GetFullFilePhysicalPath(path, webFullPath));
        }


        /// <summary>
        /// Returns full path of the given path. Does not change the trailing slash
        /// </summary>
        /// <param name="originalPath">Object part of the path (e.g. /Text/text.ascx)</param>
        /// <param name="startingPath">Starting path of the full path (e.g. ~/CMSWebParts)</param>
        /// <param name="webFullPath">Full path to the root of the web project (e.g. c:\WebProject\)</param>
        /// <returns>Full path (e.g. c:\WebProject\CMSWebParts\Text\text.ascx)</returns>
        public static string GetFullPhysicalPath(string originalPath, string startingPath, string webFullPath)
        {
            // Ensure webFullPath
            if (webFullPath == null)
            {
                webFullPath = SystemContext.WebApplicationPhysicalPath;
            }

            string path = Path.EnsureSlashes(originalPath);

            if (!path.StartsWith("~/", StringComparison.Ordinal))
            {
                if (startingPath != null)
                {
                    startingPath = Path.EnsureSlashes(startingPath, true) + "/";
                }

                path = startingPath + path.Trim('/');
            }

            return DirectoryHelper.CombinePath(webFullPath, Path.EnsureBackslashes(path.Substring(2)));
        }


        /// <summary>
        /// Indicates if file name matches given pattern.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="pattern">Pattern (for example '*.jpg')</param>
        public static bool IsMatch(string fileName, string pattern)
        {
            Regex regexMask = RegexHelper.GetRegex(@"[\.\$\^\{\[\(\}\]\)\|\+\?\\]");
            pattern = regexMask.Replace(pattern, "\\$0").Replace("*", ".+");

            Regex regexMatch = RegexHelper.GetRegex(pattern, true);
            return regexMatch.IsMatch(fileName);
        }


        /// <summary>
        /// Checks if a URL resolves to an existing file on the local file system.
        /// </summary>
        /// <param name="url">URL to check; can be absolute, relative or app-relative</param>
        /// <returns><c>true</c>, if the specified URL points to a locally stored file</returns>
        public static bool FileExists(string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                return false;
            }

            // Replaces forward slashes with backslashes (URL/path mismatch)
            url = Path.EnsureSlashes(url).Trim();

            // Remove querystring from URL otherwise mapping to path will fail
            url = URLHelper.RemoveQuery(url);

            string virtualPath = URLHelper.GetVirtualPath(url);

            try
            {
                string physicalPath = URLHelper.GetPhysicalPath(virtualPath);
                return File.Exists(physicalPath);
            }
            catch (Exception)
            {
                // Catch exception because of possible invalid characters in path may thrown ArgumentException in inner methods and we do not propagate it
                return false;
            }
        }


        /// <summary>
        /// Checks if a URL resolves to an existing directory on the local file system.
        /// </summary>
        /// <param name="url">URL to check; can be absolute, relative or app-relative</param>
        /// <returns><c>true</c>, if the specified URL points to a locally stored directory</returns>
        public static bool DirectoryExists(string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                return false;
            }

            // Replaces forward slashes with backslashes (URL/path mismatch)
            url = url.Replace('\\', '/').Trim();
            
            string virtualPath = URLHelper.GetVirtualPath(url);

            try
            {
                string physicalPath = URLHelper.GetPhysicalPath(virtualPath);
                return Directory.Exists(physicalPath);
            }
            catch (Exception)
            {
                // Catch exception because of possible invalid characters in path may thrown ArgumentException in inner methods and we do not propagate it
                return false;
            }
        }


        /// <summary>
        /// Gets file checksum
        /// </summary>
        /// <param name="path">File path</param>
        public static string GetFileChecksum(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                using (FileStream file = FileStream.New(path, FileMode.Open, FileAccess.Read))
                {
                    return new BinaryData(file).Checksum;
                }
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Checks if the content of two given files is same
        /// </summary>
        /// <param name="file1">First file path to compare</param>
        /// <param name="file2">Second file path to compare</param>
        public static bool FilesMatch(string file1, string file2)
        {
            return (GetFileChecksum(file1) == GetFileChecksum(file2));
        }


        /// <summary>
        /// Copies specified directory including its subdirectories and all underlying files.
        /// </summary>
        /// <param name="sourcePath">Path of the source directory (can be relative starting with ~/)</param>
        /// <param name="targetPath">Path of the new copy of the directory including its name (can be relative starting with ~/)</param>
        public static void CopyDirectory(string sourcePath, string targetPath)
        {
            if (!DirectoryExists(sourcePath))
            {
                return;
            }

            if (sourcePath.Trim().StartsWith("~", StringComparison.Ordinal))
            {
                sourcePath = URLHelper.GetPhysicalPath(sourcePath);
            }

            if (targetPath.Trim().StartsWith("~", StringComparison.Ordinal))
            {
                targetPath = URLHelper.GetPhysicalPath(targetPath);
            }

            DirectoryHelper.CopyDirectory(sourcePath, targetPath);
        }


        /// <summary>
        /// Clones source codes (.ascx, .ascx.cs(vb), .designer.cs(vb)) for given control.
        /// </summary>
        /// <param name="srcFile">Source file (full path to source .ascx file)</param>
        /// <param name="dstFile">Target files (full path to target .ascx file)</param>
        /// <param name="dstCodeFile">Target relative path</param>
        public static void CloneControlSource(string srcFile, string dstFile, string dstCodeFile)
        {
            // Ensure directory
            DirectoryHelper.EnsureDiskPath(Path.GetDirectoryName(Path.EnsureEndBackslash(dstFile)), SystemContext.WebApplicationPhysicalPath);

            // Get path to the partial class name replace
            string wpPath = dstFile;
            if (!wpPath.StartsWith("~/", StringComparison.Ordinal))
            {
                int len = SystemContext.WebApplicationPhysicalPath.Length;
                wpPath = "~\\" + wpPath.Substring(len, wpPath.Length - len).TrimStart('\\');
            }

            wpPath = Path.GetDirectoryName(wpPath);
            string inherits = (wpPath.Replace('\\', '_').Replace('/', '_') + "_" + Path.GetFileNameWithoutExtension(dstFile).Replace('.', '_')).Trim('~').Trim('_');

            // Read .aspx file, replace classname and save as new file
            string text = File.ReadAllText(srcFile);
            File.WriteAllText(dstFile, ReplaceASCX(text, dstCodeFile, inherits));

            CopyControlFile(inherits, srcFile, dstFile, ".cs", false);
            CopyControlFile(inherits, srcFile, dstFile, ".designer.cs", false);
            CopyControlFile(inherits, srcFile, dstFile, ".vb", true);
            CopyControlFile(inherits, srcFile, dstFile, ".designer.vb", true);
        }


        /// <summary>
        /// Copies control's code behind file.
        /// </summary>
        /// <param name="className">Controls class name</param>
        /// <param name="srcFile">Source file</param>
        /// <param name="dstFile">Destination file</param>
        /// <param name="ext">Extension</param>
        /// <param name="isVB">Indicates if it should be handled like VB file or C# file.</param>
        private static void CopyControlFile(string className, string srcFile, string dstFile, string ext, bool isVB)
        {
            if (File.Exists(srcFile + ext))
            {
                string text = File.ReadAllText(srcFile + ext);
                File.WriteAllText(dstFile + ext, (isVB ? ReplaceASCXVB(text, className) : ReplaceASCXCS(text, className)));
            }
        }


        /// <summary>
        /// Replaces 'CodeFile' and 'Inherits' parameters in .asxc file.
        /// </summary>
        /// <param name="text">Ascx file</param>
        /// <param name="fname">New code file</param>
        /// <param name="inher">New inherits class name</param>
        /// <returns>New ascx file</returns>
        private static string ReplaceASCX(string text, string fname, string inher)
        {
            if (fname != null)
            {
                fname = Path.EnsureSlashes(fname);
            }

            // CodeFile
            Regex regex = RegexHelper.GetRegex("(.*CodeFile\\s*=\\s*\")(.*?)(\".*)", RegexOptions.Multiline);
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, "$1" + fname + ".cs$3", 1);
            }

            // Codebehind
            regex = RegexHelper.GetRegex("(.*Codebehind\\s*=\\s*\")(.*?)(\".*)", RegexOptions.Multiline);
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, "$1" + fname + ".cs$3", 1);
            }

            // Inherits
            regex = RegexHelper.GetRegex("(.*Inherits\\s*=\\s*\")(.*?)(\".*)", RegexOptions.Multiline);
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, "$1" + inher + "$3", 1);
            }
            return text;
        }


        /// <summary>
        /// Replaces class name in .ascx.cs file.
        /// </summary>
        /// <param name="text">Ascx.cs file</param>
        /// <param name="classname">New class name</param>
        /// <returns>New ascx.cs file</returns>
        private static string ReplaceASCXCS(string text, string classname)
        {
            // Correct class name
            string re = "public(?<firstpart>.*)class (?<secondpart>[0-9a-zA-Z_]+)";
            Regex r = RegexHelper.GetRegex(re, RegexOptions.Compiled | RegexOptions.Multiline);
            Match m = r.Match(text);

            // Get old class name from class definition. It's used in constructor replacement.
            String oldClass = String.Empty;

            if (m.Success)
            {
                if (m.Groups.Count > 2)
                {
                    oldClass = ValidationHelper.GetString(m.Groups[2], String.Empty);
                }

                text = Regex.Replace(text, re, "public${firstpart}class " + classname);
            }

            // Correct constructor name
            re = "public\\s+\\S*" + oldClass + "\\s*\\S*\\(";
            r = RegexHelper.GetRegex(re, RegexOptions.Compiled | RegexOptions.Multiline);
            if (r.IsMatch(text))
            {
                text = Regex.Replace(text, re, "public " + classname + "(");
            }

            return text;
        }


        /// <summary>
        /// Replaces class name in .ascx.vb file.
        /// </summary>
        /// <param name="text">Ascx.vb file</param>
        /// <param name="classname">New class name</param>
        /// <returns>New ascx.vb file</returns>
        private static string ReplaceASCXVB(string text, string classname)
        {
            // Correct class name
            Regex regex = RegexHelper.GetRegex("(.*Class)(.*?)(\n)*(.*?Inherits.*)", RegexOptions.Multiline);
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, "$1 " + classname + " $3$4", 1);
            }

            return text;
        }


        /// <summary>
        /// Returns filePath based on the given filePath which is unique within the folder.
        /// If the path is relative, relative path is returned, if the input path was absolute, absolute path is returned.
        /// </summary>
        /// <param name="filePath">File path (relative with ~/ or absolute)</param>
        public static string GetUniqueFileName(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                return filePath;
            }

            bool relative = filePath.StartsWith("~", StringComparison.Ordinal);
            if (relative)
            {
                filePath = URLHelper.GetPhysicalPath(filePath);
            }

            FileInfo origFile = FileInfo.New(filePath);

            string ext = origFile.Extension;
            string dir = origFile.Directory.FullName.TrimEnd('\\');
            string origName = Path.GetFileNameWithoutExtension(filePath);
            string newName = origName;

            int i = 1;
            while (File.Exists(dir + "\\" + newName + ext))
            {
                newName = origName + "_" + i++;
            }

            string fullPath = dir + "\\" + newName + ext;
            if (relative)
            {
                fullPath = "~/" + Path.EnsureSlashes(fullPath.Substring(URLHelper.GetPhysicalPath("~/").Length));
            }

            return fullPath;
        }


        /// <summary>
        /// Returns dirPath based on the given direPath which is unique within the parent folder.
        /// If the path is relative, relative path is returned, if the input path was absolute, absolute path is returned.
        /// </summary>
        /// <param name="dirPath">Directory path (relative with ~/ or absolute)</param>
        public static string GetUniqueDirectoryName(string dirPath)
        {
            if (String.IsNullOrEmpty(dirPath))
            {
                return dirPath;
            }

            bool relative = dirPath.StartsWith("~", StringComparison.Ordinal);
            if (relative)
            {
                dirPath = URLHelper.GetPhysicalPath(dirPath);
            }

            DirectoryInfo origDir = DirectoryInfo.New(dirPath);

            string rootPath = origDir.Parent.FullName;
            string origName = origDir.Name;
            string newName = origName;

            int i = 1;

            // Find unique folder name
            while (Directory.Exists(Path.Combine(rootPath, newName)))
            {
                newName = origName + "_" + i++;
            }

            string fullPath = Path.Combine(rootPath, newName);
            if (relative)
            {
                fullPath = "~/" + Path.EnsureSlashes(fullPath.Substring(URLHelper.GetPhysicalPath("~/").Length));
            }

            return fullPath;
        }


        /// <summary>
        /// Indicates if site specific custom folder should be used for storing files.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns>True if custom folder is used, otherwise false</returns>
        public static bool UseSiteSpecificCustomFolder(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSUseFilesSiteFolder"].ToBoolean(false);
        }


        /// <summary>
        /// Gets custom file folder path.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns>Value of custom file folder</returns>
        public static string FilesFolder(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSFilesFolder"].ToString("");
        }


        /// <summary>
        /// Gets the files location type.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static FilesLocationTypeEnum FilesLocationType(string siteName = null)
        {
            return CoreServices.Settings[siteName + ".CMSFilesLocationType"].ToEnum<FilesLocationTypeEnum>();
        }

        #endregion
    }
}