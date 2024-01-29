using System;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using WTE.Configuration;

namespace WTE.Helpers
{
    /// <summary>
    /// File manager class
    /// </summary>
    [Serializable()]
    public class WTEStorageManager
    {
        #region constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public WTEStorageManager()
            : base()
        {
        }

        #endregion constructor

        #region Public Methods

        #region read

        /// <summary>
        /// Reads binary file (such as Images) from persistant storage.
        /// </summary>
        /// <param name="filePath">Specify the relative path of the file.</param>
        /// <returns>Returns the binary content of the file.</returns>
        public static byte[] ReadBinaryFile(string filePath)
        {
            try
            {
                string physicalPath = GetPhysicalPath(filePath);
                return File.ReadAllBytes(physicalPath);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Reads text file from persistant storage.
        /// </summary>
        /// <param name="filePath">Specify the relative file path.</param>
        /// <returns>returns the text content of the file.</returns>
        public static string ReadTextFile(string filePath)
        {
            try
            {
                string physicalPath = GetPhysicalPath(filePath);
                return File.ReadAllText(physicalPath);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Read a text file into a string builder
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static StringBuilder GetStringBuilder(string filePath)
        {
            StringBuilder builder = new StringBuilder();

            StreamReader reader = GetStreamReader(filePath);
            if (reader != null)
            {
                builder = new StringBuilder(reader.ReadToEnd());
            }

            return builder;
        }

        /// <summary>
        /// Get a stream reader with a file name
        /// </summary>
        /// <param name="filePath"></param>
        public static StreamReader GetStreamReader(string filePath)
        {
            if (Exists(filePath))
            {
                return new StreamReader(GetPhysicalPath(filePath));
            }

            return null;
        }

        /// <summary>
        /// Get a XML Document
        /// </summary>
        /// <param name="filePath">the file name</param>
        /// <returns></returns>
        public static XmlDocument GetXMLDocument(string filePath)
        {
            string xml = WTEStorageManager.ReadTextFile(filePath);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
        }

        #endregion read

        #region write

        /// <summary>
        /// Writes binary file (such as Images) to persistant storage.
        /// </summary>
        /// <param name="fileData">Specify the binrary file content.</param>
        /// <param name="filePath">Specify the relative file path.</param>
        public static void WriteBinaryFile(byte[] fileData, string filePath)
        {
            try
            {
                WriteBinaryFile(fileData, filePath, HttpContext.Current);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Writes binary file (such as Images) to persistant storage.
        /// </summary>
        /// <param name="fileData">Specify the binrary file content.</param>
        /// <param name="filePath">Specify the relative file path.</param>
        /// <param name="context">the context</param>
        public static void WriteBinaryFile(byte[] fileData, string filePath, HttpContext context)
        {
            try
            {
                string physicalPath = GetPhysicalPath(filePath, context);

                // Create directory if not exists.
                FileInfo fileInfo = new FileInfo(physicalPath);
                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }

                // Write the binary content.
                File.WriteAllBytes(physicalPath, fileData);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Write to a text file
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="filePath"></param>
        /// <param name="context">the context</param>
        public static void WriteTextFile(string fileData, string filePath)
        {
            WriteTextFile(fileData, filePath, HttpContext.Current);
        }

        /// <summary>
        /// Write to a text file
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="filePath"></param>
        /// <param name="context">the context</param>
        public static void WriteTextFile(string fileData, string filePath, HttpContext context)
        {
            WriteTextFile(fileData, filePath, WTEFileMode.Write, context);
        }

        /// <summary>
        /// Append text to a text file
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="filePath"></param>
        public static void AppendTextFile(string fileData, string filePath)
        {
            AppendTextFile(fileData, filePath, HttpContext.Current);
        }

        /// <summary>
        /// Append text to a text file
        /// </summary>
        /// <param name="p_fileData"></param>
        /// <param name="p_filePath"></param>
        /// <param name="p_context">the context</param>
        public static void AppendTextFile(string p_fileData, string p_filePath, HttpContext p_context)
        {
            WriteTextFile(p_fileData, p_filePath, WTEFileMode.Append, p_context);
        }

        /// <summary>
        /// Writes text file to persistant storage.
        /// </summary>
        /// <param name="p_fileData">Specify the string that has the file content.</param>
        /// <param name="p_filePath">Specify the relative file path.</param>
        /// <param name="p_fileMode">Specify the file write mode operatation. </param>
        public static void WriteTextFile(string p_fileData, string p_filePath, WTEFileMode p_fileMode, HttpContext p_context)
        {
            try
            {
                string physicalPath = GetPhysicalPath(p_filePath, p_context);

                // Create directory if not exists.
                FileInfo fileInfo = new FileInfo(physicalPath);
                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }

                // Check file write mode and write content.
                if (p_fileMode == WTEFileMode.Append)
                {
                    File.AppendAllText(physicalPath, p_fileData);
                }
                else
                {
                    File.WriteAllText(physicalPath, p_fileData);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion write

        #region delete

        /// <summary>
        /// Deletes the file from persistant storage.
        /// </summary>
        /// <param name="filePath">Specify the relative file path.</param>
        public static bool DeleteFile(string filePath)
        {
            try
            {
                if (Exists(filePath))
                {
                    File.Delete(GetPhysicalPath(filePath));
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion delete

        #region copy

        /// <summary>
        /// Copy file
        /// </summary>
        /// <param name="p_sourceDirectory"></param>
        /// <param name="p_sourceFile"></param>
        /// <param name="p_destinationDirectory"></param>
        /// <param name="p_destinationFile"></param>
        /// <returns></returns>
        public static bool CopyFile(string p_sourceDirectory, string p_sourceFile, string p_destinationDirectory, string p_destinationFile)
        {
            bool success = false;
            bool isCopyDirectory = String.IsNullOrWhiteSpace(p_sourceFile);

            string sourceDir = p_sourceDirectory;
            string destDir = p_destinationDirectory;
            string sourceFile = p_sourceFile;
            string destFile = p_destinationFile;

            if (String.IsNullOrWhiteSpace(destDir))
            {
                destDir = sourceDir;
            }

            // To copy a folder's contents to a new location:
            // Create a new target folder, if necessary.
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            if (isCopyDirectory)
            {
                #region copy directory

                try
                {
                    if (Directory.Exists(sourceDir))
                    {
                        string[] files = Directory.GetFiles(sourceDir);

                        string sourceFileName = String.Empty;
                        string destinationFileName = String.Empty;

                        // Copy the files and overwrite destination files if they already exist.
                        foreach (string s in files)
                        {
                            // Use static Path methods to extract only the file name from the path.
                            sourceFileName = System.IO.Path.GetFileName(s);
                            destinationFileName = System.IO.Path.Combine(destDir, sourceFileName);
                            File.Copy(s, destFile, true);
                        }
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    WTELogging.LogException(ex);
                    success = false;
                }

                #endregion copy directory
            }
            else
            {
                #region copy file

                try
                {
                    string sourceFileName = Path.Combine(sourceDir, sourceFile);
                    string destFileName = Path.Combine(destDir, destFile);

                    if (File.Exists(sourceFileName))
                    {
                        File.Copy(sourceFileName, destFileName, true);
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    WTELogging.LogException(ex);
                    success = false;
                }

                #endregion copy file
            }

            return success;
        }

        /// <summary>
        /// Move file
        /// </summary>
        /// <param name="p_sourceDirectory"></param>
        /// <param name="p_sourceFile"></param>
        /// <param name="p_destinationDirectory"></param>
        /// <param name="p_destinationFile"></param>
        /// <returns></returns>
        public static bool MoveFile(string p_sourceDirectory, string p_sourceFile, string p_destinationDirectory, string p_destinationFile)
        {
            bool success = false;

            bool isMoveDirectory = String.IsNullOrWhiteSpace(p_sourceFile);

            string sourceDir = p_sourceDirectory;
            string destDir = p_destinationDirectory;
            string sourceFile = p_sourceFile;
            string destFile = p_destinationFile;

            if (String.IsNullOrWhiteSpace(destDir))
            {
                destDir = sourceDir;
            }

            // To copy a folder's contents to a new location:
            // Create a new target folder, if necessary.
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            if (isMoveDirectory)
            {
                //System.IO.Directory.Move(@"C:\Users\Public\public\test\", @"C:\Users\Public\private");

                #region move directory

                try
                {
                    if (Directory.Exists(sourceDir))
                    {
                        string[] files = Directory.GetFiles(sourceDir);

                        string sourceFileName = String.Empty;
                        string destinationFileName = String.Empty;

                        // move the file to the new location
                        foreach (string s in files)
                        {
                            // Use static Path methods to extract only the file name from the path.
                            sourceFileName = System.IO.Path.GetFileName(s);
                            destinationFileName = System.IO.Path.Combine(destDir, sourceFileName);
                            File.Move(sourceFileName, destinationFileName);
                        }
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    WTELogging.LogException(ex);
                    success = false;
                }

                #endregion move directory
            }
            else
            {
                #region copy file

                try
                {
                    string sourceFileName = Path.Combine(sourceDir, sourceFile);
                    string destFileName = Path.Combine(destDir, destFile);

                    if (File.Exists(sourceFileName))
                    {
                        File.Move(sourceFileName, destFileName);
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    WTELogging.LogException(ex);
                    success = false;
                }

                #endregion copy file
            }

            return success;
        }

        #endregion copy

        #region existance, permission checking

        /// <summary>
        /// Check file exists in persistant storage.
        /// </summary>
        /// <param name="p_filePath">Specify the relative file path.</param>
        /// <returns>Returns boolean indicating the file exists (TRUE) or not (FALSE).</returns>
        public static bool Exists(string p_filePath)
        {
            try
            {
                string physicalPath = GetPhysicalPath(p_filePath);
                return File.Exists(GetPhysicalPath(p_filePath));
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Check to see if the file is read only
        /// </summary>
        /// <param name="p_filePath"></param>
        /// <returns></returns>
        public static bool IsReadOnly(string p_filePath)
        {
            FileInfo fileInfo = GetFileInfo(p_filePath);
            if (fileInfo != null)
            {
                return fileInfo.IsReadOnly;
            }

            // it doesn't exists
            return false;
        }

        #endregion existance, permission checking

        #region get path

        /// <summary>
        /// Get HttpPath of file from file storage.
        /// </summary>
        /// <param name="p_filePath">Specify the relative file path.</param>
        /// <returns>Returns HttpPath of file.</returns>
        public static string HttpPath(string p_filePath)
        {
            return p_filePath;
        }

        /// <summary>
        /// Get mapped server path
        /// </summary>
        /// <param name="p_filePath"></param>
        /// <returns></returns>
        public static string GetPhysicalPath(string p_filePath)
        {
            return GetPhysicalPath(p_filePath, HttpContext.Current);
        }

        /// <summary>
        /// Get physical path based on the context object
        /// </summary>
        /// <param name="p_filePath"></param>
        /// <param name="p_context"></param>
        /// <returns></returns>
        public static string GetPhysicalPath(string p_filePath, HttpContext p_context)
        {
            string path = p_filePath;
            bool IsConsoleApplication = WTEConfiguration.IsConsoleApplication;
            try
            {
                if (p_context != null && p_context.Server != null)
                {
                    // go ahead and let the server map the path.
                    path = p_context.Server.MapPath(p_filePath);
                }
                else
                {
                    // we are missing the http context
                    string rootFolder = String.Empty;
                    string filepath = p_filePath.Replace("~/", String.Empty).Replace('/', '\\');

                    if (IsConsoleApplication)
                    {
                        rootFolder = GetCurrentDirectoryPath();
                    }
                    else
                    {
                        rootFolder = HttpRuntime.AppDomainAppPath;
                    }

                    if (filepath.Contains(rootFolder))
                    {
                        // it is already mapped, clean it up and return...
                        path = filepath;
                    }
                    else
                    {
                        path = String.Format("{0}\\{1}", GetCurrentDirectoryPath(), filepath);
                    }                 
                }
            }
            catch (Exception)
            {
                // something went wrong
                path = p_filePath;
            }
            return path;
        }

        /// <summary>
        /// Ensure physical path
        /// </summary>
        /// <param name="p_filePath"></param>
        /// <returns></returns>
        public static bool EnsurePhysicalPath(string p_filePath)
        {
            bool exists = EnsurePhysicalPath(p_filePath, HttpContext.Current);
            return exists;
        }

        /// <summary>
        /// Make sure the physical path exists
        /// </summary>
        /// <param name="p_filePath"></param>
        /// <param name="p_context"></param>
        /// <returns></returns>
        public static bool EnsurePhysicalPath(string p_filePath, HttpContext p_context)
        {
            bool exists = false;
            try
            {
                string physicalPath = GetPhysicalPath(p_filePath, p_context);

                // Create directory if not exists.
                FileInfo fileInfo = new FileInfo(physicalPath);
                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }

                exists = true;
            }
            catch (Exception)
            {
                throw;
            }
            return exists;
        }



        #endregion get path

        #region get file/directory info

        /// <summary>
        /// Get file info
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static FileInfo GetFileInfo(string filePath)
        {
            return GetFileInfo(filePath, HttpContext.Current);
        }

        /// <summary>
        /// Get file info
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static FileInfo GetFileInfo(string filePath, HttpContext context)
        {
            return new FileInfo(GetPhysicalPath(filePath, context));
        }

        /// <summary>
        /// Directory info
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public static DirectoryInfo GetDirectoryInfo(string directoryPath)
        {
            return GetDirectoryInfo(directoryPath, HttpContext.Current);
        }

        /// <summary>
        /// Directory info
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DirectoryInfo GetDirectoryInfo(string directoryPath, HttpContext context)
        {
            return new DirectoryInfo(GetPhysicalPath(directoryPath, context));
        }

        /// <summary>
        /// Get the current directory path
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDirectoryPath()
        {
            string dir = String.Empty;

            if (String.IsNullOrWhiteSpace(dir))
            {
                dir = System.IO.Directory.GetCurrentDirectory();
            }

            return dir;
        }

        #endregion get file/directory info

        #region create file names

        /// <summary>
        /// Get filename
        /// </summary>
        /// <param name="p_type"></param>
        /// <param name="p_fileName"></param>
        /// <param name="p_noExtraInfo"></param>
        /// <returns></returns>
        public static string GetFileName(WTEFileType p_type, string p_fileName, bool p_noExtraInfo)
        {
            bool addSiteName = false;
            bool addMachineName = true;
            bool addDate = true;
            bool addTime = false;
            string fname = String.Empty;
            string oname = String.Empty;
            string dname = String.Empty;
            fname = GetFileName(p_type, p_fileName, addSiteName, addMachineName, addDate, addTime, true, out oname, out dname);
            if (p_noExtraInfo)
            {
                return oname;
            }
            else
            {
                return fname;
            }
        }


        /// <summary>
        /// Get file name
        /// </summary>
        /// <param name="p_type"></param>
        /// <param name="p_fileName"></param>
        /// <param name="p_addSiteName"></param>
        /// <param name="p_addMachineName"></param>
        /// <param name="p_addDate"></param>
        /// <param name="p_addTime"></param>
        /// <param name="p_getCleanFileName"></param>
        /// <param name="p_oldFileName"></param>
        /// <param name="p_directory"></param>
        /// <returns></returns>
        public static string GetFileName(WTEFileType p_type, string p_fileName, bool p_addSiteName, bool p_addMachineName, bool p_addDate, bool p_addTime, bool p_getCleanFileName, out string p_oldFileName, out string p_directory)
        {
            //DateTime logTime = DateTime.Now;
            ////string logTimeString = logTime.ToString("yyyy_MM_dd_HHmmss");
            //string logTimeString = logTime.ToString("yyyy_MM_dd");
            //string logfilename = String.Format("{0}\\{1}_{2}.log", ParserSettings.LogDirectory, ParserSettings.ParserName, logTimeString);

            string filename = p_fileName;
            p_oldFileName = String.Empty;
            p_directory = String.Empty;

            if (!String.IsNullOrWhiteSpace(filename))
            {
                FileInfo info = WTEStorageManager.GetFileInfo(p_fileName);
                string directory = info.DirectoryName;
                string fname = Path.GetFileNameWithoutExtension(info.Name);
                string extension = info.Extension;
                string machineName = String.Empty;
                string date = String.Empty;
                string time = String.Empty;
                string originalFileName = String.Empty;

                if (String.IsNullOrWhiteSpace(extension))
                {
                    switch (p_type)
                    {
                        case WTEFileType.Text:
                            extension = ".txt";
                            break;

                        case WTEFileType.XML:
                            extension = ".xml";
                            break;

                        case WTEFileType.PDF:
                            extension = ".pdf";
                            break;

                        case WTEFileType.HTML:
                            extension = ".html";
                            break;

                        case WTEFileType.LOG:
                            extension = ".log";
                            break;

                        case WTEFileType.None:
                        default:
                            extension = ".log";
                            break;
                    }
                }

                if (!String.IsNullOrWhiteSpace(extension))
                {
                    extension = extension.Replace(".", "");
                }

                if (p_addMachineName)
                {
                    machineName = String.Format("_{0}", WTEConfiguration.GetMachineName());
                }

                DateTime logTime = DateTime.Now;
                if (p_addDate)
                {
                    date = String.Format("_{0}", logTime.ToString("yyyy_MM_dd"));
                }

                if (p_addTime)
                {
                    // full up to seconds
                    //time = String.Format("_{0}", logTime.ToString("hh_mm_ss_tt"));
                    time = String.Format("_{0}", logTime.ToString("hh_mm_tt"));
                }

                filename = String.Format("{0}{1}{2}{3}.{4}", fname, machineName, date, time, extension);
                p_directory = directory;

                if (p_getCleanFileName)
                {
                    p_oldFileName = String.Format("{0}.{1}", fname, extension);
                }
            }

            return filename;
        }

        #endregion create file names

        #endregion Public Methods
    }
}