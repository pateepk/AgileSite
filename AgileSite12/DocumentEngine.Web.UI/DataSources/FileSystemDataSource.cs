using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Data;
using System.Web.UI.Design;

using CMS.Helpers;
using CMS.IO;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// File system data source server control.
    /// </summary>
    [ToolboxData("<{0}:FileSystemDataSource runat=server />"), Serializable]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class FileSystemDataSource : CMSBaseDataSource
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets the custom table name.
        /// </summary>
        public string Path
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets if files from sub directories should be included in data source.
        /// </summary>
        public bool IncludeSubDirs
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets filter for files.
        /// </summary>
        public string FilesFilter
        {
            get;
            set;
        }

        #endregion


        #region "Methods, events, handlers"

        /// <summary>
        /// Gets datasource from DB.
        /// </summary>
        /// <returns>Dataset as object</returns>
        protected override object GetDataSourceFromDB()
        {
            if (CMSHttpContext.Current == null)
            {
                return null;
            }

            // Initialize properties with dependence on filter settings
            if (SourceFilterControl != null)
            {
                SourceFilterControl.InitDataProperties(this);
            }

            DataSet ds = null;

            // Get directory if exists
            if (!String.IsNullOrEmpty(Path))
            {
                string path = Path;
                if (!IO.Path.IsPathRooted(Path))
                {
                    path = CMSHttpContext.Current.Server.MapPath(Path);
                }

                DirectoryInfo di = DirectoryInfo.New(path);

                if (di.Exists)
                {
                    FileInfo[] files = null;

                    // Apply filter
                    if (String.IsNullOrEmpty(FilesFilter))
                    {
                        // Get all files
                        FilesFilter = "*.*";

                        // Take into account Include subdirectories property
                        files = di.GetFiles(FilesFilter, (IncludeSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                    }
                    else
                    {
                        // Create array of all filters to be applied
                        string[] filters = FilesFilter.Split(';');

                        // Get files according to filter
                        foreach (string filter in filters)
                        {
                            // Fill in temporary array of files
                            FileInfo[] tempFiles = di.GetFiles(filter, (IncludeSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));

                            // Add files from this filter to complete result
                            if (files != null)
                            {
                                var allResults = new FileInfo[files.Length + tempFiles.Length];
                                files.CopyTo(allResults, 0);
                                tempFiles.CopyTo(allResults, files.Length);
                                files = new FileInfo[allResults.Length];
                                allResults.CopyTo(files, 0);
                            }
                            else
                            {
                                files = new FileInfo[tempFiles.Length];
                                tempFiles.CopyTo(files, 0);
                            }
                        }
                    }

                    if (files != null)
                    {
                        // Create new table
                        DataTable dt = new DataTable("files");

                        dt.Columns.Add(new DataColumn("FileName", typeof(string)));
                        dt.Columns.Add(new DataColumn("Extension", typeof(string)));
                        dt.Columns.Add(new DataColumn("FilePath", typeof(string)));
                        dt.Columns.Add(new DataColumn("FileURL", typeof(string)));
                        dt.Columns.Add(new DataColumn("Directory", typeof(string)));
                        dt.Columns.Add(new DataColumn("Created", typeof(DateTime)));
                        dt.Columns.Add(new DataColumn("Size", typeof(long)));
                        dt.Columns.Add(new DataColumn("Modified", typeof(DateTime)));

                        string rootPath = CMSHttpContext.Current.Request.MapPath("~/");

                        // Load data to new row
                        // Ensure that contained files exists -> required for external storages
                        foreach (FileInfo fi in files.Where(f => f.Exists))
                        {
                            try
                            {
                                // Create DataRow for file
                                DataRow dr = dt.NewRow();

                                dr["FileName"] = fi.Name;
                                dr["Extension"] = fi.Extension;
                                dr["FilePath"] = fi.FullName;
                                dr["FileURL"] = (fi.FullName.Length > rootPath.Length ? URLHelper.ResolveUrl("~/" + IO.Path.EnsureSlashes(fi.FullName.Remove(0, rootPath.Length))) : "");
                                dr["Directory"] = fi.Directory.Name;
                                dr["Created"] = fi.CreationTime;
                                dr["Modified"] = fi.LastWriteTime;
                                dr["Size"] = fi.Length;

                                dt.Rows.Add(dr);
                            }
                            catch (Exception)
                            {
                            }
                        }

                        // Sort results by directory name and file name
                        DataHelper.SortDataTable(dt, "FilePath ASC");

                        // Apply Where Condition on dataview
                        DataView dv = dt.DefaultView;

                        if (!String.IsNullOrEmpty(WhereCondition))
                        {
                            dv.RowFilter = WhereCondition;
                        }

                        // Apply OrderBy on datatable
                        if (!String.IsNullOrEmpty(OrderBy))
                        {
                            DataHelper.SortDataTable(dv.Table, OrderBy);
                        }

                        // Load table from dataview
                        dt = dv.ToTable();

                        // Create final dataset
                        ds = new DataSet();
                        ds.Tables.Add(dt);

                        // Apply TopN on dataset
                        DataHelper.RestrictRows(ds, TopN);
                    }
                }
            }

            return ds;
        }


        /// <summary>
        /// Gets cache key.
        /// </summary>
        protected override object[] GetDefaultCacheKey()
        {
            return new object[] { "filesystemdatasource", CacheHelper.BaseCacheKey, ClientID, WhereCondition, OrderBy, Path, IncludeSubDirs, FilesFilter };
        }

        #endregion
    }
}