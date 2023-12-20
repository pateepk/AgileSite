using System;
using System.Web;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for file upload controls.
    /// </summary>
    public class FileUpload : CMSAdminControl
    {
        #region "Variables"

        private string mNodeSiteName;
        private string mImageUrl = String.Empty;
        private string mImageUrlOver = String.Empty;
        private string mLoadingImageUrl = String.Empty;
        private string mDocumentCulture = "en-us";
        private Guid mAttachmentGroupGUID = Guid.Empty;
        private Guid mFormGUID = Guid.Empty;
        private string mText = String.Empty;
        private bool mInserMode;
        private string mInnerElementClass = "innerDiv";
        private string mInnerLoadingDivClass = "innerLoadingDiv";
        private int mResizeToWidth = ImageHelper.AUTOSIZE;
        private int mResizeToHeight = ImageHelper.AUTOSIZE;
        private int mResizeToMaxSideSize = ImageHelper.AUTOSIZE;
        private MediaSourceEnum mSourceType = MediaSourceEnum.Web;

        #endregion


        #region "Properties"

        /// <summary>
        /// Current site name.
        /// </summary>
        public virtual string NodeSiteName
        {
            get
            {
                return mNodeSiteName ?? (mNodeSiteName = SiteContext.CurrentSiteName);
            }
            set
            {
                mNodeSiteName = value;
            }
        }


        /// <summary>
        /// Gets or sets control group of the control.
        /// </summary>
        public virtual string ControlGroup
        {
            get;
            set;
        }


        /// <summary>
        /// If set, NodeGroupID of content file is set also.
        /// </summary>
        public virtual int NodeGroupID
        {
            get;
            set;
        }


        /// <summary>
        /// Uploaded file.
        /// </summary>
        public virtual HttpPostedFile PostedFile
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// ID of the media file.
        /// </summary>
        public virtual int MediaFileID
        {
            get;
            set;
        }


        /// <summary>
        /// Media file name.
        /// </summary>
        public virtual string MediaFileName
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether the uploader should upload media file thumbnail or basic media file.
        /// </summary>
        public virtual bool IsMediaThumbnail
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the permissions should be explicitly checked.
        /// </summary>
        public new virtual bool CheckPermissions
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if supported browser.
        /// </summary>
        public virtual bool RaiseOnClick
        {
            get;
            set;
        }


        /// <summary>
        /// File upload user control.
        /// </summary>
        public virtual CMSFileUpload FileUploadControl
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// URL of the IFrame uploader resides in.
        /// </summary>
        public virtual string IFrameUrl
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Indicates if the control should be displayed in inline mode.
        /// </summary>
        public virtual bool DisplayInline
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets which files with extensions are allowed to be uploaded.
        /// </summary>
        public virtual string AllowedExtensions
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if control is in insert mode (only new attachments are added, no update).
        /// </summary>
        public virtual bool InsertMode
        {
            get
            {
                return ValidationHelper.GetBoolean(mInserMode, false);
            }
            set
            {
                mInserMode = value;
            }
        }


        /// <summary>
        /// Width of upload image.
        /// </summary>
        public virtual int ImageWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Height of upload image.
        /// </summary>
        public virtual int ImageHeight
        {
            get;
            set;
        }


        /// <summary>
        /// URL of upload image.
        /// </summary>
        public virtual string ImageUrl
        {
            get
            {
                return mImageUrl;
            }
            set
            {
                mImageUrl = value;
            }
        }


        /// <summary>
        /// URL of upload image hover.
        /// </summary>
        public virtual string ImageUrlOver
        {
            get
            {
                return mImageUrlOver;
            }
            set
            {
                mImageUrlOver = value;
            }
        }


        /// <summary>
        /// Text displayed on uploader.
        /// </summary>
        public virtual string Text
        {
            get
            {
                return mText;
            }
            set
            {
                mText = value;
            }
        }


        /// <summary>
        /// CSS class of element containing upload button/icon.
        /// </summary>
        public virtual string InnerElementClass
        {
            get
            {
                return mInnerElementClass;
            }
            set
            {
                mInnerElementClass = value;
            }
        }


        /// <summary>
        /// CSS class of element containing upload progress.
        /// </summary>
        public virtual string InnerLoadingElementClass
        {
            get
            {
                return mInnerLoadingDivClass;
            }
            set
            {
                mInnerLoadingDivClass = value;
            }
        }


        /// <summary>
        /// Additional style appended to generated style.
        /// </summary>
        public string AdditionalStyle
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether control should set up in any case.
        /// </summary>
        public virtual bool ForceLoad
        {
            get;
            set;
        }


        /// <summary>
        /// URL of image shown when loading.
        /// </summary>
        public virtual string LoadingImageUrl
        {
            get
            {
                return mLoadingImageUrl;
            }
            set
            {
                mLoadingImageUrl = value;
            }
        }


        /// <summary>
        /// GUID of attachment.
        /// </summary>
        public virtual Guid AttachmentGUID
        {
            get
            {
                return ValidationHelper.GetGuid(ViewState["AttachmentGUID"], Guid.Empty);
            }
            set
            {
                ViewState["AttachmentGUID"] = value;
            }
        }


        /// <summary>
        /// GUID of attachment group.
        /// </summary>
        public virtual Guid AttachmentGroupGUID
        {
            get
            {
                return mAttachmentGroupGUID;
            }
            set
            {
                mAttachmentGroupGUID = value;
            }
        }


        /// <summary>
        /// Name of document attachment column.
        /// </summary>
        public virtual string AttachmentGUIDColumnName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if file extension sould be included in file name.
        /// </summary>
        public virtual bool IncludeExtension { get; set; }


        /// <summary>
        /// Width of attachment.
        /// </summary>
        public virtual int ResizeToWidth
        {
            get
            {
                return mResizeToWidth;
            }
            set
            {
                mResizeToWidth = value;
            }
        }


        /// <summary>
        /// Height of attachment.
        /// </summary>
        public virtual int ResizeToHeight
        {
            get
            {
                return mResizeToHeight;
            }
            set
            {
                mResizeToHeight = value;
            }
        }


        /// <summary>
        /// Maximum side size of attachment.
        /// </summary>
        public virtual int ResizeToMaxSideSize
        {
            get
            {
                return mResizeToMaxSideSize;
            }
            set
            {
                mResizeToMaxSideSize = value;
            }
        }


        /// <summary>
        /// GUID of form.
        /// </summary>
        public virtual Guid FormGUID
        {
            get
            {
                return mFormGUID;
            }
            set
            {
                mFormGUID = value;
            }
        }


        /// <summary>
        /// ID of document.
        /// </summary>
        public virtual int DocumentID
        {
            get;
            set;
        }


        /// <summary>
        /// Parent node ID.
        /// </summary>
        public virtual int NodeParentNodeID
        {
            get;
            set;
        }


        /// <summary>
        /// New document culture short name.
        /// </summary>
        public virtual string DocumentCulture
        {
            get
            {
                return mDocumentCulture;
            }
            set
            {
                mDocumentCulture = value;
            }
        }


        /// <summary>
        /// Document class name.
        /// </summary>
        public virtual string NodeClassName
        {
            get;
            set;
        }


        /// <summary>
        /// ID of parent element.
        /// </summary>
        public virtual string ParentElemID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets type of the content uploaded by the control.
        /// </summary>
        public virtual MediaSourceEnum SourceType
        {
            get
            {
                return mSourceType;
            }
            set
            {
                mSourceType = value;
            }
        }


        /// <summary>
        /// ID of the current library.
        /// </summary>
        public virtual int LibraryID
        {
            get;
            set;
        }


        /// <summary>
        /// Folder path of the current library.
        /// </summary>
        public virtual string LibraryFolderPath
        {
            get;
            set;
        }


        /// <summary>
        /// JavaScript function name called after save of new file.
        /// </summary>
        public virtual string AfterSaveJavascript
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the post-upload JavaScript function call should include created attachment GUID information.
        /// </summary>
        public virtual bool IncludeNewItemInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if only images is allowed for upload.
        /// </summary>
        public virtual bool OnlyImages
        {
            get;
            set;
        }


        /// <summary>
        /// Target folder path, to which physical files will be uploaded.
        /// </summary>
        public virtual string TargetFolderPath
        {
            get;
            set;
        }


        /// <summary>
        /// Target file name, to which will be used after files will be uploaded.
        /// </summary>
        public virtual string TargetFileName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the target file path
        /// </summary>
        public string TargetFilePath
        {
            get
            {
                return TargetFolderPath.TrimEnd('/', '\\') + "/" + TargetFileName;
            }
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Checks if file is allowed according current settings.
        /// </summary>
        protected void CheckAllowedExtensions()
        {
            if (FileUploadControl.PostedFile != null)
            {
                string fileName = FileUploadControl.PostedFile.FileName;

                string extensions = $";{AllowedExtensions?.ToLowerInvariant()};";
                string extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();

                if (OnlyImages && !ImageHelper.IsImage(extension))
                {
                    throw new InvalidOperationException("Only images can be uploaded. If you want to insert other media, use Insert image or media button.");
                }
                if (!String.IsNullOrEmpty(AllowedExtensions) && !extensions.Contains($";{extension};"))
                {
                    throw new InvalidOperationException($"You cannot upload files with the '{extension}' extension. Only following extensions are allowed: {AllowedExtensions.TrimEnd(';').Replace(";", ", ")}.");
                }
            }
        }

        #endregion
    }
}