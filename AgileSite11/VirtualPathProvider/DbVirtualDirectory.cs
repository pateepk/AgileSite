using System;
using System.Data;
using System.Collections;
using System.Web.Hosting;
using System.Collections.Generic;

using CMS.IO;
using CMS.Base;
using CMS.Helpers;

namespace CMS.VirtualPathProvider
{
    /// <summary>
    /// Virtual directory handler.
    /// </summary>
    public class DbVirtualDirectory : VirtualDirectory
    {
        #region "Variables"
        
        private DbPathProvider dbPathProvider = null;

        private static ArrayList children = new ArrayList();
        private static ArrayList directories = new ArrayList();
        private List<VirtualFile> files = null;

        private string mPhysicalDirectory = null;
        private bool? mProvideFolderContent = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Child nodes.
        /// </summary>
        public override IEnumerable Children
        {
            get
            { 
                return children;
            }
        }


        /// <summary>
        /// Child directories.
        /// </summary>
        public override IEnumerable Directories
        {
            get
            {
                return directories;
            }
        }


        /// <summary>
        /// Child files.
        /// </summary>
        public override IEnumerable Files
        {
            get
            {
                if (files == null)
                {
                    files = new List<VirtualFile>();

                    // Check if folder content can be provided (allows batch compilation)
                    if (ProvideFolderContent)
                    {
                        if (!String.IsNullOrEmpty(PhysicalDirectory))
                        {
                            // Add the files from physical directory
                            DirectoryInfo dir = DirectoryInfo.New(PhysicalDirectory);
                            if (dir.Exists)
                            {
                                FileInfo[] fileList = dir.GetFiles();
                                if (fileList != null)
                                {
                                    foreach (FileInfo fi in fileList)
                                    {
                                        // Add the virtual file
                                        string filePath = VirtualPath.TrimEnd('/') + "/" + fi.Name;
                                        VirtualFile vFile = dbPathProvider.GetFile(filePath);

                                        files.Add(vFile);
                                    }
                                }
                            }
                        }
                    }
                }

                return files;
            }
        }


        /// <summary>
        /// Physical directory.
        /// </summary>
        protected virtual string PhysicalDirectory
        {
            get
            {
                if ((mPhysicalDirectory == null) && (VirtualPath != null))
                {
                    mPhysicalDirectory = DbPathProvider.GetPhysicalFilePath(VirtualPath).ToLowerCSafe().TrimEnd('\\');
                }

                return mPhysicalDirectory;
            }
        }


        /// <summary>
        /// Indicates whether current directory provide folder content due to compile optimization
        /// </summary>
        protected virtual bool ProvideFolderContent 
        { 
            get
            {
                if (mProvideFolderContent == null)
                {
                    mProvideFolderContent = !VirtualPathHelper.IsSingleFileCompilationPath(VirtualPath);
                }

                return mProvideFolderContent.Value;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        public DbVirtualDirectory(string virtualDir, DbPathProvider provider)
            : base(virtualDir)
        {
            dbPathProvider = provider;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public DbVirtualDirectory(string virtualDir, DbPathProvider provider, bool provideFolderContent)
            : this(virtualDir, provider)
        {
            mProvideFolderContent = provideFolderContent;
        }

        #endregion
    }
}