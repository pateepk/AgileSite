using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Hosting;

using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.VirtualPathProvider
{
    /// <summary>
    /// Virtual file.
    /// </summary>
    public class DbVirtualFile : VirtualFile
    {
        #region "Variables"

        private bool? mIsPhysicalFile = null;
        private string mPhysicalFile = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Representation of virtual file object (IVirtualFileObject)
        /// </summary>
        public IVirtualFileObject VirtualFileObject
        {
            get;
            set;
        }


        /// <summary>
        /// Ensures virtual path dependencies for virtual file pointing to the physical file
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        /// <param name="virtualPathDependencies">Current virtual path dependencies</param>
        internal IEnumerable EnsureVirtualDependencies(string virtualPath, IEnumerable virtualPathDependencies)
        {
            // Change dependencies if current virtual path is using physical file path (e.g. virtual web parts: Virtual path is pointing to the physical web part location (change web part code))
            if ((VirtualFileObject != null) && VirtualFileObject.IsStoredExternally && (!String.IsNullOrEmpty(VirtualFileObject.PhysicalFilePath)))
            {
                List<string> newItems = new List<string>();
                foreach (string path in virtualPathDependencies)
                {
                    if (virtualPath.EqualsCSafe(path, true))
                    {
                        // Get virtual path from absolute path
                        string newPath = URLHelper.ResolveUrl("~" + Path.EnsureSlashes(VirtualFileObject.PhysicalFilePath.Substring(SystemContext.WebApplicationPhysicalPath.Length)));
                        newItems.Add(newPath);
                    }
                    else
                    {
                        newItems.Add(path);
                    }
                }

                // Return new dependencies collection
                return newItems;
            }

            // By default return original dependencies
            return virtualPathDependencies;
        }


        /// <summary>
        /// Physical file path
        /// </summary>
        internal virtual string PhysicalFilePath
        {
            get
            {
                if (mPhysicalFile == null)
                {
                    if (IsPhysicalFile)
                    {
                        if ((VirtualFileObject != null) && (!String.IsNullOrEmpty(VirtualFileObject.PhysicalFilePath)))
                        {
                            mPhysicalFile = VirtualFileObject.PhysicalFilePath;
                        }
                        else if (VirtualPath != null)
                        {
                            mPhysicalFile = DbPathProvider.GetPhysicalFilePath(VirtualPath).ToLowerCSafe();
                        }
                    }
                    else
                    {
                        mPhysicalFile = String.Empty;
                    }
                }

                return mPhysicalFile;
            }
        }


        /// <summary>
        /// Indicates whether current virtual file is representation of physical file
        /// </summary>
        internal bool IsPhysicalFile
        {
            get
            {
                if (mIsPhysicalFile == null)
                {
                    if (VirtualFileObject != null)
                    {
                        mIsPhysicalFile = VirtualFileObject.IsStoredExternally;
                    }
                    else
                    {
                        mIsPhysicalFile = false;
                    }
                }

                return mIsPhysicalFile.Value;
            }
        }


        /// <summary>
        /// Object hash used for compiling
        /// </summary>
        internal virtual string ObjectHash
        {
            get
            {
                if (VirtualFileObject != null)
                {
                    return VirtualFileObject.ObjectHash;
                }
                return String.Empty;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// DbVirtualFile constructor.
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        /// <param name="fileObject">IVirtualFileObject instance</param>
        public DbVirtualFile(string virtualPath, IVirtualFileObject fileObject)
            : this(virtualPath)
        {
            this.VirtualFileObject = fileObject;
        }


        /// <summary>
        /// DbVirtualFile constructor.
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        public DbVirtualFile(string virtualPath)
            : base(virtualPath)
        {
            if (virtualPath.StartsWithCSafe("//", StringComparison.Ordinal))
            {
                string message = "[DbVirtualFile]: Virtual file '" + virtualPath + "' could not be compiled, please check the code.\n\n";

                Exception last = CMSHttpContext.Current.Server.GetLastError();
                if (last is HttpCompileException)
                {
                    message = "[DbVirtualFile]: Virtual file could not be compiled, please check the code.\n\n" + last.Message.Replace("http://server//", "/") + "\n\n";
                }

                message += "See EventLog for details.";

                throw new HttpCompileException(message);
            }
        }


        /// <summary>
        /// Returns true if the file exist
        /// </summary>
        public virtual bool Exists()
        {
            if (VirtualFileObject != null)
            {
                return true;
            }

            // Determine whether the file exists on the virtual file system.
            return IsPhysicalFile;
        }


        /// <summary>
        /// Overridden 'Open()' method for virtual file.
        /// </summary>
        /// <returns>Stream.</returns>
        public override System.IO.Stream Open()
        {
            System.IO.Stream stream = null;

            // Log the virtual file request
            VirtualPathLog.Log(VirtualPath, IsPhysicalFile, false);

            if (VirtualFileObject != null)
            {
                stream = new System.IO.MemoryStream();

                string fileContent = VirtualFileObject.Content;
                if (!String.IsNullOrEmpty(fileContent))
                {
                    // Put the file content on the stream.
                    var writer = new System.IO.StreamWriter(stream, UnicodeEncoding.Unicode);

                    writer.Write(fileContent);
                    writer.Flush();
#pragma warning disable BH1014 // Do not use System.IO
                    stream.Seek(0, System.IO.SeekOrigin.Begin);
#pragma warning restore BH1014 // Do not use System.IO
                }
            }
            else if (IsPhysicalFile)
            {
                // Direct stream from physical file
                stream = FileStream.New(PhysicalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            return stream;
        }

        #endregion
    }
}