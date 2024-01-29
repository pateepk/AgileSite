using System;
using System.Data;

using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Class representing media library in the current context.
    /// </summary>
    [RegisterAllProperties]
    public class MediaLibraryContext : AbstractContext<MediaLibraryContext>
    {
        #region "Variables"

        private MediaLibraryInfo mCurrentMediaLibrary = null;
        private MediaFileInfo mCurrentMediaFile = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Current media library info object matching libraryid, libraryguid or libraryname 
        /// specified in the URL parameter of the current request
        /// </summary>
        public static MediaLibraryInfo CurrentMediaLibrary
        {
            get
            {
                return GetCurrentMediaLibrary();
            }
            set
            {
                Current.mCurrentMediaLibrary = value;
            }
        }


        /// <summary>
        /// Current media file info object matching fileid or fileguid specified in the URL parameter of the current request.
        /// </summary>
        public static MediaFileInfo CurrentMediaFile
        {
            get
            {
                return GetCurrentMediaFile();
            }
            set
            {
                Current.mCurrentMediaFile = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns information on the current media library according the library ID/ library GUID/ library name specified as an URL parameter of the current request
        /// </summary>
        public static MediaLibraryInfo GetCurrentMediaLibrary()
        {
            // Try to get the library info from the request items collection
            MediaLibraryInfo mli = Current.mCurrentMediaLibrary;
            if (mli == null)
            {
                // Try to get library info by its ID first
                int libraryId = QueryHelper.GetInteger("libraryid", 0);
                if (libraryId > 0)
                {
                    mli = MediaLibraryInfoProvider.GetMediaLibraryInfo(libraryId);
                }

                // If no library ID specified, try to look for GUID
                if (mli == null)
                {
                    Guid libraryGuid = QueryHelper.GetGuid("libraryguid", Guid.Empty);
                    if (libraryGuid != Guid.Empty)
                    {
                        string where = String.Format("LibraryGUID='{0}'", libraryGuid);
                        DataSet ds = MediaLibraryInfoProvider.GetMediaLibraries(where, null);
                        if (!DataHelper.DataSourceIsEmpty(ds))
                        {
                            mli = new MediaLibraryInfo(ds.Tables[0].Rows[0]);
                        }
                    }
                }

                // If library wasn't specified even by the GUID look for library name
                if (mli == null)
                {
                    string libraryName = QueryHelper.GetString("libraryname", string.Empty);
                    if (libraryName != string.Empty)
                    {
                        mli = MediaLibraryInfoProvider.GetMediaLibraryInfo(libraryName, SiteContext.CurrentSiteName);
                    }
                }

                // Get Library from CurrentFile if none of the previous (id, guid, name) is specified
                if (mli == null)
                {
                    if (CurrentMediaFile != null)
                    {
                        mli = MediaLibraryInfoProvider.GetMediaLibraryInfo(CurrentMediaFile.FileLibraryID);
                    }
                }

                // Save the media library to the request items
                Current.mCurrentMediaLibrary = mli;
            }
            return mli;
        }


        /// <summary>
        /// Returns information on the current media file according the fileid or fileguid 
        /// specified as an URL parameter of the current request
        /// </summary>
        public static MediaFileInfo GetCurrentMediaFile()
        {
            // Try to get the file info from the request items collection
            MediaFileInfo mfi = Current.mCurrentMediaFile;
            if (mfi == null)
            {
                // Try to get file info by its ID first
                int fileId = QueryHelper.GetInteger("fileid", 0);
                if (fileId > 0)
                {
                    mfi = MediaFileInfoProvider.GetMediaFileInfo(fileId);
                }

                // If no file ID specified, try to look for GUID
                if (mfi == null)
                {
                    Guid fileGuid = QueryHelper.GetGuid("fileguid", Guid.Empty);
                    if (fileGuid != Guid.Empty)
                    {
                        string where = String.Format("FileGUID='{0}'", fileGuid);
                        DataSet ds = MediaFileInfoProvider.GetMediaFiles(where, null);
                        if (!DataHelper.DataSourceIsEmpty(ds))
                        {
                            mfi = new MediaFileInfo(ds.Tables[0].Rows[0]);
                        }
                    }
                }

                // Save the media library to the request items
                Current.mCurrentMediaFile = mfi;
            }
            return mfi;
        }
        
        #endregion
    }
}