using System;
using System.Collections;
using System.Web.Caching;
using System.Web.Hosting;

using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.VirtualPathProvider
{
    using FileTable = SafeDictionary<string, VirtualFile>;
    using DirectoryTable = SafeDictionary<string, VirtualDirectory>;

    /// <summary>
    /// Virtual path provider dynamically loads content from virtual paths.
    /// </summary>
    public class DbPathProvider : System.Web.Hosting.VirtualPathProvider, IVirtualPathProvider
    {
        #region "Events"

        /// <summary>
        /// Handler to provide custom virtual files.
        /// </summary>
        public static event EventHandler<VirtualFileEventArgs> OnGetVirtualFile;

        /// <summary>
        /// Handler to provide custom virtual directories.
        /// </summary>
        public static event EventHandler<VirtualDirectoryEventArgs> OnGetVirtualDirectory;

        #endregion


        #region "Variables"

        // Virtual files [path] -> [VirtualFile]
        private static readonly CMSStatic<FileTable> mVirtualFiles = new CMSStatic<FileTable>(() => new FileTable());

        // Virtual directories. [path] -> [VirtualDirectory]
        private static readonly CMSStatic<DirectoryTable> mVirtualDirectories = new CMSStatic<DirectoryTable>(() => new DirectoryTable());

        #endregion


        #region "Properties"

        /// <summary>
        /// Virtual files [path] -> [VirtualFile]
        /// </summary>
        private static FileTable VirtualFiles
        {
            get
            {
                return mVirtualFiles;
            }
        }


        /// <summary>
        /// Virtual directories. [path] -> [VirtualDirectory]
        /// </summary>
        private static DirectoryTable VirtualDirectories
        {
            get
            {
                return mVirtualDirectories;
            }
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Finds whether the file, which is part of given virtual path, exists on the virtual file system or on the normal file system.
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        public override bool FileExists(string virtualPath)
        {
            VirtualPathHelper.UsingVirtualPathProvider = true;

            // Get the file
            var file = GetVirtualFile(virtualPath) as DbVirtualFile;
            if (file != null)
            {
                return file.Exists();
            }

            // If not, the file is sought on the normal file system
            return Previous.FileExists(virtualPath);
        }


        /// <summary>
        /// This method is used by the compilation system to obtain a VirtualFile instance to work with a given virtual file path.
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        public override VirtualFile GetFile(string virtualPath)
        {
            var file = GetVirtualFile(virtualPath);
            if (file != null)
            {
                return file;

            }

            return Previous.GetFile(virtualPath);
        }


        /// <summary>
        /// This method is used by the compilation system to obtain a VirtualDirectory instance to work with a given virtual directory
        /// </summary>
        /// <param name="virtualDir">Virtual path</param>
        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            var dir = GetVirtualDirectory(virtualDir);
            if (dir != null)
            {
                return dir;
            }

            return Previous.GetDirectory(virtualDir);
        }


        /// <summary>
        /// Overridden method 'GetCacheDependency()'.
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        /// <param name="virtualPathDependencies">Dependencies</param>
        /// <param name="utcStart">UTC start</param>
        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            // Use default cache dependency for physical files
            var file = GetVirtualFile(virtualPath) as DbVirtualFile;
            if (file != null)
            {
                /* Do not use cache dependencies for virtual objects */
                return null;
            }

            return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }


        /// <summary>
        /// Gets the file hash for the given virtual file.
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        /// <param name="virtualPathDependencies">Path dependencies</param>
        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        {
            // Get the virtual file
            var file = GetVirtualFile(virtualPath) as DbVirtualFile;
            if (file != null)
            {
                // Use object hash for simple virtual objects
                if (!file.IsPhysicalFile)
                {
                    string result = file.ObjectHash;
                    string dependencyHash = null;
                    if (!String.IsNullOrEmpty(result))
                    {
                        // Ensure additional physical file dependencies
                        foreach (string path in virtualPathDependencies)
                        {
                            // Skip current file
                            if (!path.Equals(virtualPath, StringComparison.InvariantCulture))
                            {
                                string fullPath = URLHelper.GetPhysicalPath(path);
                                if (File.Exists(fullPath))
                                {
                                    dependencyHash += File.GetLastWriteTime(fullPath).Ticks;
                                }
                            }
                        }

                        return result + dependencyHash;
                    }
                }
                // Combine file hash and object hash, try change virtual path dependencies
                else
                {
                    return file.ObjectHash + Previous.GetFileHash(virtualPath, file.EnsureVirtualDependencies(virtualPath, virtualPathDependencies));
                }
            }

            // Use default file hash
            return Previous.GetFileHash(virtualPath, virtualPathDependencies);
        }

        #endregion


        #region "IVirtualPathProvider methods"

        /// <summary>
        /// Register current virtual path provider
        /// </summary>
        public virtual void Register()
        {
            // Do not register virtual path provider in ClientBuildManager domains (e.g aspnet_compiler.exe or visual studio compiler)
            if (!HostingEnvironment.InClientBuildManager)
            {
                HostingEnvironment.RegisterVirtualPathProvider(this);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns virtual file for the given path.
        /// </summary>
        protected virtual VirtualFile GetVirtualFile(string virtualPath)
        {
            // Try to ualD cached record
            VirtualFile file = VirtualFiles[virtualPath];
            if (file == null)
            {
                // Get virtual file via handler
                if (OnGetVirtualFile != null)
                {
                    VirtualFileEventArgs eventArgs = new VirtualFileEventArgs { VirtualPath = virtualPath };

                    OnGetVirtualFile(this, eventArgs);

                    if (eventArgs.VirtualFile != null)
                    {
                        VirtualFiles[virtualPath] = eventArgs.VirtualFile;
                        return eventArgs.VirtualFile;
                    }
                }

                // Try registered paths
                IVirtualFileObject virtFile = VirtualPathHelper.GetVirtualFileObject(virtualPath);
                if (virtFile != null)
                {
                    file = new DbVirtualFile(virtualPath, virtFile);
                    VirtualFiles[virtualPath] = file;
                    return file;
                }
            }

            return file;
        }


        /// <summary>
        /// Returns virtual directory for the given path.
        /// </summary>
        protected virtual VirtualDirectory GetVirtualDirectory(string virtualPath)
        {
            // Try to get cached record
            VirtualDirectory dir = VirtualDirectories[virtualPath];
            if (dir == null)
            {
                // Get virtual Directory via handler
                if (OnGetVirtualDirectory != null)
                {
                    VirtualDirectoryEventArgs eventArgs = new VirtualDirectoryEventArgs { VirtualPath = virtualPath };

                    OnGetVirtualDirectory(this, eventArgs);

                    if (eventArgs.VirtualDirectory != null)
                    {
                        VirtualDirectories[virtualPath] = eventArgs.VirtualDirectory;
                        return eventArgs.VirtualDirectory;
                    }
                }

                // Get virtual Directory for single file compilation file
                if (VirtualPathHelper.IsSingleFileCompilationPath(virtualPath))
                {
                    dir = new DbVirtualDirectory(virtualPath, this, false);
                    VirtualDirectories[virtualPath] = dir;
                }
            }

            return dir;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns file path from the given virtual path.
        /// </summary>
        /// <param name="virtualPath">Virtual file path</param>
        internal static string GetPhysicalFilePath(string virtualPath)
        {
            string path = URLHelper.UnResolveUrl(virtualPath, SystemContext.ApplicationPath);
            return URLHelper.GetPhysicalPath(path);
        }

        #endregion
    }
}