using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CMS.Helpers;
using CMS.IO;
using CMS.DocumentEngine;
using CMS.DataEngine;
using CMS.Membership;
using CMS.SiteProvider;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using SystemIO = System.IO;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Helper class for the file uploader.
    /// </summary>
    public class UploaderHelper
    {
        #region "Constants"

        /// <summary>
        /// Character used as separator for the name value pairs.
        /// </summary>
        private const char ARG_SEPARATOR = '|';


        /// <summary>
        /// Character used for separation of resize arguments.
        /// </summary>
        private const char RESIZE_ARG_SEPARATOR = ';';


        /// <summary>
        /// Default size of the chunk in Bytes for uploader.
        /// </summary>
        public const long UPLOAD_CHUNK_SIZE = 8 * 1024 * 1024;

        /// <summary>
        /// Content type for request that uploads a whole file in one chunk.
        /// </summary>
        private const string REQUEST_CONTENT_TYPE = "application/octet-stream";

        private const string ALLOWED_EXTENSIONS_HASHING_PURPOSE = "MultiFileUploader - AllowedExtensions";
        private const string ADDIOTIONAL_PARAMETERS_HASHING_PURPOSE = "MultiFileUploader - AddiotionalParameter";
        private const string MEDIA_LIBRARY_ARGS_HASHING_PURPOSE = "MultiFileUploader - MediaLibraryArgs";
        private const string FILE_ARGS_HASHING_PURPOSE = "MultiFileUploader - FileArgs";
        private const string META_FILE_ARGS_HASHING_PURPOSE = "MultiFileUploader - MetaFileArgs";
        private const string FORUM_ARGS_HASHING_PURPOSE = "MultiFileUploader - ForumArgs";
        private const string ATTACHEMENT_ARGS_HASHING_PURPOSE = "MultiFileUploader - AttachementArgs";

        #endregion


        #region "Variables"

        private readonly HttpContext mCtx;
        private readonly string mFileName;
        private readonly Guid mInstanceGuid;
        private readonly bool mComplete;
        private readonly bool mCanceled;
        private string mAllowedExtensions;
        private readonly bool mGetBytes;
        private readonly long mStartByte;
        private readonly int mCurrentFileIndex;
        private readonly int mFilesCount;
        private readonly long mCurrentFileSize;

        private int mResizeToWidth;
        private int mResizeToHeight;
        private int mResizeToMaxSide;

        private readonly NameValueCollection mAdditionalArgsCollection;
        private readonly bool mIsInsertMode;
        private readonly string mParentElementID;
        private readonly string mAfterSaveJavascript;
        private readonly string mTargetFolderPath;
        private readonly string mTargetFileName;
        private static string mTempPath;
        private readonly bool mIncludeNewItemInfo;
        private readonly bool mOnlyImages;
        private readonly bool mRaiseOnClick;
        private readonly string mEventTarget;

        private readonly NameValueCollection mAttachmentArgsCollection;
        private readonly NameValueCollection mMetaFileArgsCollection;
        private readonly NameValueCollection mFileArgsCollection;
        private readonly NameValueCollection mForumArgsCollection;
        private readonly NameValueCollection mMediaLibraryArgsCollection;

        private readonly AttachmentArgsStruct mAttachmentArgs;
        private readonly MetaFileArgsStruct mMetaFileArgs;
        private readonly FileArgsStruct mFileArgs;
        private readonly ForumArgsStruct mForumArgs;
        private readonly MediaLibraryArgsStruct mMediaLibraryArgs;

        /// <summary>
        /// Encapsulates important data for multipart upload.
        /// </summary>
        private class MultiPartUploadData
        {
            /// <summary>
            /// Unique identifier for one multipart upload.
            /// </summary>
            public string UploadSessionID { get; set; }


            /// <summary>
            /// Number of the part that is currently uploaded to external storage.
            /// </summary>
            public int PartNumber { get; set; }


            /// <summary>
            /// ETag of every part already uploaded to external storage.
            /// </summary>
            public List<string> PartIdentifiers { get; set; }


            /// <summary>
            /// Serializes <see cref="MultiPartUploadData"/> to Json.
            /// </summary>
            /// <returns>Serialized <see cref="MultiPartUploadData"/> in Json format.</returns>
            public string SerializeToJson()
            {
                return JsonConvert.SerializeObject(this, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// After script which will be evaluated after the upload.
        /// </summary>
        public string AfterScript
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets type of the content uploaded by the control.
        /// </summary>
        public MediaSourceEnum SourceType
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies the start byte.
        /// </summary>
        public long StartByte
        {
            get
            {
                return mStartByte;
            }
        }


        /// <summary>
        /// Indicates whether bytes should be got.
        /// </summary>
        public bool GetBytes
        {
            get
            {
                return mGetBytes;
            }
        }


        /// <summary>
        /// Indicates whether it is physical file upload.
        /// </summary>
        public bool IsPhysicalFileUpload
        {
            get
            {
                return SourceType == MediaSourceEnum.PhysicalFile;
            }
        }


        /// <summary>
        /// Indicates whether it is attachment upload.
        /// </summary>
        public bool IsAttachmentUpload
        {
            get
            {
                return (AttachmentArgsCollection.Count > 0) && ((AttachmentArgs.DocumentID > 0) || (AttachmentArgs.FormGuid != Guid.Empty));
            }
        }


        /// <summary>
        /// Indicates whether it is meta file upload.
        /// </summary>
        public bool IsMetaFileUpload
        {
            get
            {
                return (MetaFileArgsCollection.Count > 0) && (MetaFileArgs.ObjectID > 0);
            }
        }


        /// <summary>
        /// Indicates whether it is file upload.
        /// </summary>
        public bool IsFileUpload
        {
            get
            {
                return (FileArgsCollection.Count > 0) && (FileArgs.NodeID > 0);
            }
        }


        /// <summary>
        /// Indicates whether it is forum attachment upload.
        /// </summary>
        public bool IsForumAttachmentUpload
        {
            get
            {
                return (ForumArgsCollection.Count > 0) && (ForumArgs.PostForumID > 0);
            }
        }


        /// <summary>
        /// Indicates whether it is media library upload.
        /// </summary>
        public bool IsMediaLibraryUpload
        {
            get
            {
                return (MediaLibraryArgsCollection.Count > 0) && (MediaLibraryArgs.LibraryID > 0);
            }
        }


        /// <summary>
        /// Arguments for the media library upload.
        /// </summary>
        public MediaLibraryArgsStruct MediaLibraryArgs
        {
            get
            {
                return mMediaLibraryArgs;
            }
        }


        /// <summary>
        /// Arguments for the forum attachment upload.
        /// </summary>
        public ForumArgsStruct ForumArgs
        {
            get
            {
                return mForumArgs;
            }
        }


        /// <summary>
        /// Arguments for the file upload.
        /// </summary>
        public FileArgsStruct FileArgs
        {
            get
            {
                return mFileArgs;
            }
        }


        /// <summary>
        /// Arguments for the meta file upload.
        /// </summary>
        public MetaFileArgsStruct MetaFileArgs
        {
            get
            {
                return mMetaFileArgs;
            }
        }


        /// <summary>
        /// Arguments for the attachment upload.
        /// </summary>
        public AttachmentArgsStruct AttachmentArgs
        {
            get
            {
                return mAttachmentArgs;
            }
        }


        /// <summary>
        /// NameValueCollection of media library arguments.
        /// </summary>
        public NameValueCollection MediaLibraryArgsCollection
        {
            get
            {
                return mMediaLibraryArgsCollection;
            }
        }


        /// <summary>
        /// NameValueCollection of forum arguments.
        /// </summary>
        public NameValueCollection ForumArgsCollection
        {
            get
            {
                return mForumArgsCollection;
            }
        }


        /// <summary>
        /// NameValueCollection of file arguments.
        /// </summary>
        public NameValueCollection FileArgsCollection
        {
            get
            {
                return mFileArgsCollection;
            }
        }


        /// <summary>
        /// NameValueCollection of meta file arguments.
        /// </summary>
        public NameValueCollection MetaFileArgsCollection
        {
            get
            {
                return mMetaFileArgsCollection;
            }
        }


        /// <summary>
        /// NameValueCollection of attachment arguments.
        /// </summary>
        public NameValueCollection AttachmentArgsCollection
        {
            get
            {
                return mAttachmentArgsCollection;
            }
        }


        /// <summary>
        /// ClientID of the element which should be used for postback method as the target.
        /// </summary>
        public string EventTarget
        {
            get
            {
                return mEventTarget;
            }
        }


        /// <summary>
        /// Indicates whether some script script should be raised on click event.
        /// </summary>
        public bool RaiseOnClick
        {
            get
            {
                return mRaiseOnClick;
            }
        }


        /// <summary>
        /// Indicates whether only image files can be uploaded or not.
        /// </summary>
        public bool OnlyImages
        {
            get
            {
                return mOnlyImages;
            }
        }


        /// <summary>
        /// Indicates whether the info of the newly created item should be included in the after script.
        /// </summary>
        public bool IncludeNewItemInfo
        {
            get
            {
                return mIncludeNewItemInfo;
            }
        }


        /// <summary>
        /// Path to the target folder.
        /// </summary>
        public string TargetFolderPath
        {
            get
            {
                return mTargetFolderPath;
            }
        }


        /// <summary>
        /// Path to the temporary directory used for all uploaded files.
        /// </summary>
        public static string TempPath
        {
            get
            {
                return mTempPath ?? (mTempPath = DirectoryHelper.CombinePath(TempFileInfoProvider.GetTempFilesRootFolderPath(), "MultiFileUploader"));
            }
        }


        /// <summary>
        /// Name of target file.
        /// </summary>
        public string TargetFileName
        {
            get
            {
                return mTargetFileName;
            }
        }


        /// <summary>
        /// Name of the javascript function which should be fired on the client after the upload.
        /// </summary>
        public string AfterSaveJavascript
        {
            get
            {
                return mAfterSaveJavascript;
            }
        }


        /// <summary>
        /// ID of the parent element in which the upload control is placed.
        /// </summary>
        public string ParentElementID
        {
            get
            {
                return mParentElementID;
            }
        }


        /// <summary>
        /// Indicates whether the upload is running in the insert mode or not.
        /// </summary>
        public bool IsInsertMode
        {
            get
            {
                return mIsInsertMode;
            }
        }


        /// <summary>
        /// NameValueCollection of the additional arguments.
        /// </summary>
        public NameValueCollection AdditionalArgsCollection
        {
            get
            {
                return mAdditionalArgsCollection;
            }
        }


        /// <summary>
        /// Indicates whether the full refresh should be fired after the upload.
        /// </summary>
        public bool FullRefresh
        {
            get;
            set;
        }


        /// <summary>
        /// Error message displayed after upload.
        /// </summary>
        public string Message
        {
            get;
            set;
        }


        /// <summary>
        /// Max side size to which image should be resized to.
        /// </summary>
        public int ResizeToMaxSide
        {
            get
            {
                return mResizeToMaxSide;
            }
        }


        /// <summary>
        /// Height to which should be image resized to.
        /// </summary>
        public int ResizeToHeight
        {
            get
            {
                return mResizeToHeight;
            }
        }


        /// <summary>
        /// Width to which should be image resized to.
        /// </summary>
        public int ResizeToWidth
        {
            get
            {
                return mResizeToWidth;
            }
        }


        /// <summary>
        /// Extension which are allowed for uploaded files.
        /// </summary>
        public string AllowedExtensions
        {
            get
            {
                return mAllowedExtensions;
            }
        }


        /// <summary>
        /// Indicates whether the upload is finished or not.
        /// </summary>
        public bool Complete
        {
            get
            {
                return mComplete;
            }
        }


        /// <summary>
        /// Indicates whether the upload is canceled or not.
        /// </summary>
        public bool Canceled
        {
            get
            {
                return mCanceled;
            }
        }


        /// <summary>
        /// Path to the uploaded file on the server in the temporary directory.
        /// </summary>
        public string FilePath
        {
            get
            {
                string instance = InstanceGuid.ToString();
                return DirectoryHelper.CombinePath(TempPath, instance.Substring(0, 2), instance, FileName);
            }
        }


        /// <summary>
        /// Extension of the uploaded file.
        /// </summary>
        public string Extension
        {
            get
            {
                return Path.GetExtension(FileName);
            }
        }


        /// <summary>
        /// Name of the uploaded file without the file extension.
        /// </summary>
        public string Name
        {
            get
            {
                return Path.GetFileNameWithoutExtension(FileName);
            }
        }


        /// <summary>
        /// Name of the uploaded file.
        /// </summary>
        public string FileName
        {
            get
            {
                return ValidationHelper.GetSafeFileName(mFileName);
            }
        }


        /// <summary>
        /// Uploader instance guid.
        /// </summary>
        public Guid InstanceGuid
        {
            get
            {
                return mInstanceGuid;
            }
        }


        /// <summary>
        /// Current file index started from 1.
        /// </summary>
        public int CurrentFileIndex
        {
            get
            {
                return mCurrentFileIndex;
            }
        }


        /// <summary>
        /// Count of currently uploading files.
        /// </summary>
        public int FilesCount
        {
            get
            {
                return mFilesCount;
            }
        }


        /// <summary>
        /// Indicates if current upload is last.
        /// </summary>
        public bool IsLastUpload
        {
            get
            {
                return (FilesCount == CurrentFileIndex);
            }
        }


        /// <summary>
        /// Indicates if current upload is the first one.
        /// </summary>
        public bool IsFirstUpload
        {
            get
            {
                return (CurrentFileIndex == 1);
            }
        }


        /// <summary>
        /// Gets postback reference for the event target.
        /// </summary>
        public string EventTargetPostbackReference
        {
            get
            {
                return string.Format(@"__doPostBack('{0}','');", EventTarget);
            }
        }


        /// <summary>
        /// Returns the whole size of currently uploaded file.
        /// </summary>
        public long CurrentFileSize
        {
            get
            {
                return mCurrentFileSize;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Current HttpContext instance</param>
        public UploaderHelper(HttpContext context)
        {
            Message = string.Empty;
            AfterScript = string.Empty;

            mCtx = context;

            // Get basic parameters
            mFileName = ValidationHelper.GetString(mCtx.Request.QueryString["Filename"], "");
            mInstanceGuid = ValidationHelper.GetGuid(mCtx.Request.QueryString["InstanceGuid"], Guid.Empty);
            mComplete = ValidationHelper.GetBoolean(mCtx.Request.QueryString["Complete"], false);
            mCanceled = ValidationHelper.GetBoolean(mCtx.Request.QueryString["Canceled"], false);
            mGetBytes = ValidationHelper.GetBoolean(mCtx.Request.QueryString["GetBytes"], false);
            mStartByte = ValidationHelper.GetLong(mCtx.Request.QueryString["StartByte"], 0);
            mCurrentFileSize = ValidationHelper.GetLong(mCtx.Request.QueryString["FileSize"], 0);

            mCurrentFileIndex = ValidationHelper.GetInteger(mCtx.Request.QueryString["CurrentFileIndex"], 0);
            mFilesCount = ValidationHelper.GetInteger(mCtx.Request.QueryString["FilesCount"], 0);
            GetResizeArguments(ValidationHelper.GetString(mCtx.Request.QueryString["ResizeArgs"], ""));
            mAllowedExtensions = ValidationHelper.GetString(mCtx.Request.QueryString["AllowedExtensions"], "");

            // Validate allowed extensions
            if (!ValidArguments(mAllowedExtensions, ALLOWED_EXTENSIONS_HASHING_PURPOSE, ARG_SEPARATOR))
            {
                throw new Exception(ResHelper.GetString("general.badhashtext"));
            }
            else
            {
                string hashSeparator = String.Format("{0}Hash{0}", ARG_SEPARATOR);
                if (mAllowedExtensions.Contains(hashSeparator))
                {
                    mAllowedExtensions = mAllowedExtensions.Substring(0, mAllowedExtensions.IndexOf(hashSeparator, StringComparison.Ordinal));
                }
            }

            // Get additional parameters
            mAdditionalArgsCollection = ParseArguments(mCtx.Request.QueryString["AdditionalParameters"], ADDIOTIONAL_PARAMETERS_HASHING_PURPOSE, ARG_SEPARATOR);
            mIsInsertMode = ValidationHelper.GetBoolean(AdditionalArgsCollection["IsInsertMode"], false);
            mParentElementID = ValidationHelper.GetString(AdditionalArgsCollection["ParentElementID"], "");
            mAfterSaveJavascript = ValidationHelper.GetString(AdditionalArgsCollection["AfterSaveJavascript"], "");
            mTargetFolderPath = ValidationHelper.GetString(AdditionalArgsCollection["TargetFolderPath"], "");
            mTargetFileName = ValidationHelper.GetString(AdditionalArgsCollection["TargetFileName"], "");
            mIncludeNewItemInfo = ValidationHelper.GetBoolean(AdditionalArgsCollection["IncludeNewItemInfo"], false);
            mOnlyImages = ValidationHelper.GetBoolean(AdditionalArgsCollection["OnlyImages"], false);
            mRaiseOnClick = ValidationHelper.GetBoolean(AdditionalArgsCollection["RaiseOnClick"], false);
            mEventTarget = ValidationHelper.GetString(AdditionalArgsCollection["EventTarget"], "");

            SourceType = (MediaSourceEnum)Enum.Parse(typeof(MediaSourceEnum), ValidationHelper.GetString(AdditionalArgsCollection["SourceType"], "Attachment"), true);

            // Get attachment arguments
            mAttachmentArgsCollection = ParseArguments(mCtx.Request.QueryString["AttachmentArgs"], ATTACHEMENT_ARGS_HASHING_PURPOSE, ARG_SEPARATOR);
            mAttachmentArgs.DocumentID = ValidationHelper.GetInteger(AttachmentArgsCollection["DocumentID"], 0);
            mAttachmentArgs.ParentNodeID = ValidationHelper.GetInteger(AttachmentArgsCollection["DocumentParentNodeID"], 0);
            mAttachmentArgs.AttachmentGroupGuid = ValidationHelper.GetGuid(AttachmentArgsCollection["AttachmentGroupGUID"], Guid.Empty);
            mAttachmentArgs.FormGuid = ValidationHelper.GetGuid(AttachmentArgsCollection["FormGUID"], Guid.Empty);
            mAttachmentArgs.FieldAttachment = ValidationHelper.GetBoolean(AttachmentArgsCollection["IsFieldAttachment"], false);
            mAttachmentArgs.AttachmentGuidColumnName = ValidationHelper.GetString(AttachmentArgsCollection["AttachmentGUIDColumnName"], string.Empty);
            mAttachmentArgs.NodeClassName = ValidationHelper.GetString(AttachmentArgsCollection["NodeClassName"], string.Empty);
            mAttachmentArgs.AttachmentGUID = ValidationHelper.GetGuid(AttachmentArgsCollection["AttachmentGUID"], Guid.Empty);
            
            FullRefresh = ValidationHelper.GetBoolean(AttachmentArgsCollection["FullRefresh"], false);

            // Get metafile arguments
            mMetaFileArgsCollection = ParseArguments(mCtx.Request.QueryString["MetaFileArgs"], META_FILE_ARGS_HASHING_PURPOSE, ARG_SEPARATOR);
            mMetaFileArgs.MetaFileID = ValidationHelper.GetInteger(MetaFileArgsCollection["MetaFileID"], 0);
            mMetaFileArgs.ObjectID = ValidationHelper.GetInteger(MetaFileArgsCollection["ObjectID"], 0);
            mMetaFileArgs.ObjectType = ValidationHelper.GetString(MetaFileArgsCollection["ObjectType"], string.Empty);
            mMetaFileArgs.Category = ValidationHelper.GetString(MetaFileArgsCollection["Category"], string.Empty);
            mMetaFileArgs.SiteID = ValidationHelper.GetInteger(MetaFileArgsCollection["SiteID"], 0);

            // Get file arguments
            mFileArgsCollection = ParseArguments(mCtx.Request.QueryString["FileArgs"], FILE_ARGS_HASHING_PURPOSE, ARG_SEPARATOR);
            mFileArgs.NodeID = ValidationHelper.GetInteger(FileArgsCollection["NodeID"], 0);
            mFileArgs.NodeGroupID = ValidationHelper.GetInteger(FileArgsCollection["NodeGroupID"], 0);
            mFileArgs.Culture = ValidationHelper.GetString(FileArgsCollection["DocumentCulture"], "en-us");
            mFileArgs.IncludeExtension = ValidationHelper.GetBoolean(FileArgsCollection["IncludeExtension"], true);

            // Get forum arguments
            mForumArgsCollection = ParseArguments(mCtx.Request.QueryString["ForumArgs"], FORUM_ARGS_HASHING_PURPOSE, ARG_SEPARATOR);
            mForumArgs.PostForumID = ValidationHelper.GetInteger(ForumArgsCollection["PostForumID"], 0);
            mForumArgs.PostID = ValidationHelper.GetInteger(ForumArgsCollection["PostID"], 0);

            // Get media library arguments
            mMediaLibraryArgsCollection = ParseArguments(mCtx.Request.QueryString["MediaLibraryArgs"], MEDIA_LIBRARY_ARGS_HASHING_PURPOSE, ARG_SEPARATOR);
            mMediaLibraryArgs.LibraryID = ValidationHelper.GetInteger(MediaLibraryArgsCollection["MediaLibraryID"], 0);
            mMediaLibraryArgs.FolderPath = ValidationHelper.GetString(MediaLibraryArgsCollection["MediaFolderPath"], string.Empty);
            mMediaLibraryArgs.MediaFileID = ValidationHelper.GetInteger(MediaLibraryArgsCollection["MediaFileID"], 0);
            mMediaLibraryArgs.IsMediaThumbnail = ValidationHelper.GetBoolean(MediaLibraryArgsCollection["IsMediaThumbnail"], false);
            mMediaLibraryArgs.MediaFileName = ValidationHelper.GetString(MediaLibraryArgsCollection["MediaFileName"], string.Empty);
        }

        #endregion


        #region "Public Methods"

        /// <summary>
        /// Adds postback reference to the after script if event target has been specified. This will ca.
        /// </summary>
        public void AddEventTargetPostbackReference()
        {
            // Call __doPostBack if there is some event target specified
            if (!string.IsNullOrEmpty(EventTarget))
            {
                // For metafiles there is UniqueID of the control which should caused postback
                AfterScript += Environment.NewLine + EventTargetPostbackReference;
            }
        }


        /// <summary>
        /// Checks if file is allowed according current settings. If it is not then error message is thrown.
        /// </summary>
        public void IsExtensionAllowed()
        {
            // Load allowed extensions from settings
            if (String.IsNullOrEmpty(AllowedExtensions))
            {
                mAllowedExtensions = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + (IsMediaLibraryUpload ? ".CMSMediaFileAllowedExtensions" : ".CMSUploadExtensions"));
            }
            string extensions = String.Format(";{0};", AllowedExtensions.ToLowerCSafe());
            string extension = Extension.TrimStart('.').ToLowerCSafe();

            if (OnlyImages && !ImageHelper.IsImage(extension))
            {
                throw new Exception(ResHelper.GetString("attach.quickinsert.onlyimages"));
            }
            if (!string.IsNullOrEmpty(AllowedExtensions) && (!extensions.Contains(String.Format(";{0};", extension))))
            {
                throw new Exception(string.Format(ResHelper.GetString("attach.notallowedextension"), extension, AllowedExtensions.TrimEnd(';').Replace(";", ", ")));
            }
        }


        /// <summary>
        /// Check permissions.
        /// </summary>
        /// <param name="node">Tree node</param>
        public void CheckNodePermissions(TreeNode node)
        {
            // For new document
            if (AttachmentArgs.FormGuid != Guid.Empty)
            {
                if (AttachmentArgs.ParentNodeID == 0)
                {
                    throw new Exception(ResHelper.GetString("attach.document.parentmissing"));
                }

                if (!MembershipContext.AuthenticatedUser.IsAuthorizedToCreateNewDocument(AttachmentArgs.ParentNodeID, AttachmentArgs.NodeClassName))
                {
                    throw new Exception(ResHelper.GetString("attach.actiondenied"));
                }
            }
            // For existing document
            else if (AttachmentArgs.DocumentID > 0)
            {
                if (MembershipContext.AuthenticatedUser.IsAuthorizedPerDocument(node, NodePermissionsEnum.Modify) == AuthorizationResultEnum.Denied)
                {
                    throw new Exception(ResHelper.GetString("attach.actiondenied"));
                }
            }
        }


        /// <summary>
        /// Processes the given data according to the uploader mode.
        /// </summary>
        /// <returns>Returns true if the uploaded data were successfully processed.</returns>
        public bool ProcessFile()
        {
            try
            {
                IsExtensionAllowed();

                if (GetBytes)
                {
                    // Existence checking mode
                    FileInfo fi = FileInfo.New(FilePath);
                    mCtx.Response.Write(fi.Exists ? fi.Length.ToString() : "0");
                    mCtx.Response.Flush();
                    return false;
                }
                else
                {
                    // Uploading mode
                    using (FileStream fs = StartByte > 0 ? File.Open(FilePath, FileMode.Append, FileAccess.Write) : File.Create(FilePath))
                    {
                        CopyDataFromRequestToFileStream(mCtx.Request, fs);

                        // If the file needs to be uploaded in chunks to external storage, use multipart upload
                        if (CurrentFileSize > UPLOAD_CHUNK_SIZE)
                        {
                            var multipartUploadData = JsonConvert.DeserializeObject<MultiPartUploadData>(mCtx.Request.Form.Get(0));
                            var multiPartStream = fs as IMultiPartUploadStream;
                            if (multiPartStream != null)
                            {
                                UseMultiPartUpload(multiPartStream, multipartUploadData);
                                SendMultipartDataToClient(multipartUploadData);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Send error message when exception occurred
                mCtx.Response.Write("0|" + (ex.InnerException != null ? ex.InnerException.Message.Trim() : ex.Message.Trim()));
                mCtx.Response.Flush();
                return false;
            }

            return true;
        }


        /// <summary>
        /// Copies file sent from client to <paramref name="fileStream"/>.
        /// It can handle if the file is sent in multipart/form-data request or
        /// in body of application/octet-stream request.
        /// </summary>
        /// <param name="fileStream">FileStream that saves data sent from client to associated storage.</param>
        /// <param name="request">Http request to read data from.</param>
        private void CopyDataFromRequestToFileStream(HttpRequest request, FileStream fileStream)
        {
            SystemIO.Stream inputStream = null;
            if (request.Files.Count == 0 && request.ContentType.Equals(REQUEST_CONTENT_TYPE, StringComparison.OrdinalIgnoreCase))
            {
                inputStream = request.InputStream;
            }
            else if (request.Files.Count == 1)
            {
                inputStream = request.Files.Get(0).InputStream;
            }
            else
            {
                throw new InvalidOperationException("Request is in incorrect format for uploading a file.");
            }

            SaveFile(inputStream, fileStream);
        }


        /// <summary>
        /// Uploads data in smaller parts that are served by media library uploader to external storage. 
        /// </summary>
        /// <param name="stream">Stream that support multipart upload.</param>
        /// <param name="multipartUploadData">Data related to currently multipart uploaded file.</param>
        private void UseMultiPartUpload(IMultiPartUploadStream stream, MultiPartUploadData multipartUploadData)
        {
            // Start multipart upload process 
            if (multipartUploadData.PartNumber == 0)
            {
                multipartUploadData.UploadSessionID = stream.InitMultiPartUpload();
                multipartUploadData.PartNumber++;
                multipartUploadData.PartIdentifiers = multipartUploadData.PartIdentifiers ?? new List<string>();
            }

            // Upload part of the file
            var identifiers = stream.UploadStreamContentAsMultiPart(multipartUploadData.UploadSessionID, multipartUploadData.PartNumber);
            multipartUploadData.PartIdentifiers.AddRange(identifiers);

            if (!Complete)
            {
                multipartUploadData.PartNumber += identifiers.Count();
            }
            else
            {
                stream.CompleteMultiPartUploadProcess(multipartUploadData.UploadSessionID, multipartUploadData.PartIdentifiers);
            }
        }


        /// <summary>
        /// Sends informations about currently uploaded part of a file to client.
        /// </summary>
        /// <param name="multipartUploadData">Informations about currently uploaded part of a file </param>
        private void SendMultipartDataToClient(MultiPartUploadData multipartUploadData)
        {
            if (!Complete)
            {
                mCtx.Response.ContentType = "application/json";
                mCtx.Response.Write(multipartUploadData.SerializeToJson());
                mCtx.Response.Flush();
            }
        }


        /// <summary>
        /// Removes the temporary file.
        /// </summary>
        public void CleanTempFile()
        {
            // Remove temporary data
            if (File.Exists(FilePath))
            {
                AbortCurrentMultiPartUpload();
                File.Delete(FilePath);
            }
        }


        /// <summary>
        /// Cleans resources on external storage that has already been uploaded.
        /// </summary>
        private void AbortCurrentMultiPartUpload()
        {
            if (CurrentFileSize > UPLOAD_CHUNK_SIZE)
            {
                var provider = StorageHelper.GetStorageProvider(FilePath);

                using (var stream = provider.GetFileStream(FilePath, FileMode.Truncate))
                {
                    var multiPartStream = stream as IMultiPartUploadStream;
                    if (multiPartStream != null)
                    {
                        var multipartUploadData = JsonConvert.DeserializeObject<MultiPartUploadData>(mCtx.Request.Form.Get(0));
                        multiPartStream.AbortMultiPartUpload(multipartUploadData.UploadSessionID);
                    }
                }
            }
        }


        /// <summary>
        /// Copies file from the source path to the destination path.
        /// </summary>
        /// <param name="destinationPath">Path to the destination file</param>
        public void CopyFile(string destinationPath)
        {
            if (File.Exists(FilePath))
            {
                bool processed = false;

                DirectoryHelper.EnsureDiskPath(destinationPath, SystemContext.WebApplicationPhysicalPath);

                // Resize if the file is image
                if (ImageHelper.IsImage(Extension))
                {
                    if ((ResizeToWidth > 0) || (ResizeToHeight > 0) || (ResizeToMaxSide > 0))
                    {
                        byte[] imageData = File.ReadAllBytes(FilePath);

                        ImageHelper helper = new ImageHelper(imageData);
                        if (helper.CanResizeImage(ResizeToWidth, ResizeToHeight, ResizeToMaxSide))
                        {
                            int[] newDims = helper.EnsureImageDimensions(ResizeToWidth, ResizeToHeight, ResizeToMaxSide);

                            // If new dimensions are different from the original ones, resize the file
                            if (((newDims[0] != helper.ImageWidth) || (newDims[1] != helper.ImageHeight)) && ((newDims[0] > 0) && (newDims[1] > 0)))
                            {
                                imageData = helper.GetResizedImageData(newDims[0], newDims[1], ImageHelper.DefaultQuality);
                            }
                        }

                        // Overwrite the file
                        File.WriteAllBytes(destinationPath, imageData);

                        processed = true;
                    }
                }

                if (!processed)
                {
                    // Copy original
                    File.Copy(FilePath, destinationPath, true);
                }
            }
        }

        #endregion


        #region "Private Methods"

        /// <summary>
        /// Sets resize arguments according to the input string representation of them.
        /// </summary>
        /// <param name="resizeArguments">String representation of the resize arguments</param>
        private void GetResizeArguments(string resizeArguments)
        {
            // Split string by the separator
            string[] size = resizeArguments.Split(RESIZE_ARG_SEPARATOR);
            if (size.Length == 3)
            {
                mResizeToWidth = ValidationHelper.GetInteger(size[0], 0);
                mResizeToHeight = ValidationHelper.GetInteger(size[1], 0);
                mResizeToMaxSide = ValidationHelper.GetInteger(size[2], 0);
            }
        }


        /// <summary>
        /// Splits input arguments string by the specified separator and returns it as NameValueCollection.
        /// </summary>
        /// <param name="args">String containing name value pairs concatenated by the separator</param>
        /// <param name="purpose">A unique string identifying the purpose of the hash string.</param>
        /// <param name="separator">Separator for the name value pairs</param>
        /// <returns>New NameValueCollection.</returns>
        private static NameValueCollection ParseArguments(string args, string purpose, char separator)
        {
            if (!ValidArguments(args, purpose, separator))
            {
                throw new Exception(ResHelper.GetString("general.badhashtext"));
            }

            // Return collection
            NameValueCollection result = new NameValueCollection();

            if (string.IsNullOrEmpty(args))
            {
                return result;
            }

            // Split input string by the input separator
            string[] array = args.Split(separator);

            // Add name value pairs to the result
            for (int i = 0; i < array.Length; i += 2)
            {
                result.Add(array[i], array[i + 1]);
            }

            return result;
        }


        /// <summary>
        /// Validates arguments hash.
        /// </summary>
        /// <param name="argument">Arguments string</param>
        /// <param name="purpose">A unique string identifying the purpose of the hash string.</param>
        /// <param name="separator">Arguments Separator</param>
        private static bool ValidArguments(string argument, string purpose, char separator)
        {
            // Empty argument is valid
            if (String.IsNullOrEmpty(argument))
            {
                return true;
            }

            // Find hash string
            string[] splitSeparator = { String.Format("{0}Hash{0}", separator) };
            string[] split = argument.Split(splitSeparator, StringSplitOptions.RemoveEmptyEntries);

            // Hash string not found
            if (split.Length != 2)
            {
                return false;
            }

            // Validate hash
            if (ValidationHelper.ValidateHash(split[0], split[1], new HashSettings() { HashSalt = purpose }))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Writes the input stream to the output stream to file.
        /// </summary>
        /// <param name="input">Input stream with the data</param>
        /// <param name="output">Output FileStream where the data should be written</param>
        private static void SaveFile(SystemIO.Stream input, FileStream output)
        {
            input.CopyTo(output);
        }

        #endregion


        #region "Structures"

        /// <summary>
        /// Structure for forum arguments.
        /// </summary>
        public struct ForumArgsStruct
        {
            /// <summary>
            /// ID of the forum.
            /// </summary>
            public int PostForumID;

            /// <summary>
            /// ID of the post.
            /// </summary>
            public int PostID;
        }


        /// <summary>
        /// Structure for file arguments.
        /// </summary>
        public struct FileArgsStruct
        {
            /// <summary>
            /// ID of the node.
            /// </summary>
            public int NodeID;

            /// <summary>
            /// ID of the node group.
            /// </summary>
            public int NodeGroupID;

            /// <summary>
            /// Culture code.
            /// </summary>
            public string Culture;

            /// <summary>
            /// Indicates if file extension should be included in file name.
            /// </summary>
            public bool IncludeExtension;
        }


        /// <summary>
        /// Structure for meta file arguments.
        /// </summary>
        public struct MetaFileArgsStruct
        {
            /// <summary>
            /// ID of the metafile.
            /// </summary>
            public int MetaFileID;

            /// <summary>
            /// ID of the target object.
            /// </summary>
            public int ObjectID;

            /// <summary>
            /// Type of the object.
            /// </summary>
            public string ObjectType;

            /// <summary>
            /// Category.
            /// </summary>
            public string Category;

            /// <summary>
            /// ID of the current site.
            /// </summary>
            public int SiteID;
        }


        /// <summary>
        /// Structure for attachment arguments.
        /// </summary>
        public struct AttachmentArgsStruct
        {
            /// <summary>
            /// ID of the document.
            /// </summary>
            public int DocumentID;

            /// <summary>
            /// ID of the parent node.
            /// </summary>
            public int ParentNodeID;

            /// <summary>
            /// Guid of the attachment group.
            /// </summary>
            public Guid AttachmentGroupGuid;

            /// <summary>
            /// Guid of the form.
            /// </summary>
            public Guid FormGuid;

            /// <summary>
            /// Indicates whether it is field attachment.
            /// </summary>
            public bool FieldAttachment;

            /// <summary>
            /// Name of the column with guid.
            /// </summary>
            public string AttachmentGuidColumnName;

            /// <summary>
            /// Name of the node class.
            /// </summary>
            public string NodeClassName;

            /// <summary>
            /// Guid of the attachment.
            /// </summary>
            public Guid AttachmentGUID;
        }


        /// <summary>
        /// Structure for media library arguments.
        /// </summary>
        public struct MediaLibraryArgsStruct
        {
            /// <summary>
            /// ID of the media library.
            /// </summary>
            public int LibraryID;

            /// <summary>
            /// Path of the target folder.
            /// </summary>
            public string FolderPath;

            /// <summary>
            /// ID of the media file.
            /// </summary>
            public int MediaFileID;

            /// <summary>
            /// Indicates whether a thumbnail is uploaded.
            /// </summary>
            public bool IsMediaThumbnail;

            /// <summary>
            /// File name of the media file.
            /// </summary>
            public string MediaFileName;
        }

        #endregion
    }
}