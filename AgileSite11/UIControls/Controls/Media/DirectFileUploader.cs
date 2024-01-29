using System;
using System.Collections;
using System.Text;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for direct file uploader controls.
    /// </summary>
    public class DirectFileUploader : FileUpload
    {
        #region "Variables"

        private MultifileUploaderModeEnum mUploadMode = MultifileUploaderModeEnum.DirectSingle;
        private Unit mWidth = new Unit(0);
        private Unit mHeight = new Unit(0);
        private bool mEnabled = true;

        /// <summary>
        /// JS function for showing errors.
        /// </summary>
        protected const string ERROR_FUNCTION = "ShowError();";

        #endregion


        #region "Public properties"


        /// <summary>
        /// Provides access to properties of inner controls necessary for extensibility.
        /// </summary>
        public virtual IUploadHandler UploadHandler
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Gets or sets the style of the button.
        /// Button is not rendered at all when <see cref="ShowIconMode"/> is true. Therefore this property has no effect.
        /// </summary>
        public virtual ButtonStyle ButtonStyle
        {
            get
            {
                return ButtonStyle.None;
            }
            set
            {
            }
        }


        /// <summary>
        /// Container width.
        /// </summary>
        public virtual Unit Width
        {
            get
            {
                return mWidth;
            }
            set
            {
                mWidth = value;
            }
        }


        /// <summary>
        /// Container height.
        /// </summary>
        public virtual Unit Height
        {
            get
            {
                return mHeight;
            }
            set
            {
                mHeight = value;
            }
        }


        /// <summary>
        /// Upload mode for the uploader application.
        /// </summary>
        public virtual MultifileUploaderModeEnum UploadMode
        {
            get
            {
                return mUploadMode;
            }
            set
            {
                mUploadMode = value;
            }
        }


        /// <summary>
        /// Value indicating whether multiselect is enabled in the open file dialog window.
        /// </summary>
        public virtual bool Multiselect
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether to render the control in enabled or disabled state.
        /// </summary>
        public virtual bool Enabled
        {
            get
            {
                return mEnabled;
            }
            set
            {
                mEnabled = value;
            }
        }


        /// <summary>
        /// Max number of possible upload files.
        /// </summary>
        public virtual int MaxNumberToUpload
        {
            get;
            set;
        }


        /// <summary>
        /// Site ID form metafile upload.
        /// </summary>
        public virtual int SiteID
        {
            get;
            set;
        }


        /// <summary>
        /// Category/Group for uploaded metafile.
        /// </summary>
        public virtual string Category
        {
            get;
            set;
        }


        /// <summary>
        /// Metafile ID for reupload.
        /// </summary>
        public virtual int MetaFileID
        {
            get;
            set;
        }


        /// <summary>
        /// Object ID for metafile upload.
        /// </summary>
        public virtual int ObjectID
        {
            get;
            set;
        }


        /// <summary>
        /// Object type for metafile upload.
        /// </summary>
        public virtual string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if full refresh after upload is needed
        /// </summary>
        public virtual bool FullRefresh
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if progress should be displayed.
        /// Applied only for multi file uploader.
        /// </summary>
        public bool ShowProgress
        {
            get;
            set;
        }


        /// <summary>
        /// Control identifier.
        /// </summary>
        protected string Identifier
        {
            get
            {
                String identifier = ViewState["Identifier"] as String;
                if (identifier == null)
                {
                    ViewState["Identifier"] = identifier = Guid.NewGuid().ToString("N");
                }

                return identifier;
            }
        }


        /// <summary>
        /// Whether to display button with text or icon.
        /// </summary>
        public bool ShowIconMode
        {
            get;
            set;
        }

        #endregion

        
        #region "Protected methods"

        /// <summary>
        /// Handles client-side error displaying.
        /// </summary>
        protected void Page_Error(object sender, EventArgs e)
        {
            StringBuilder errorScript = new StringBuilder();
            errorScript.Append("function ShowError(){alert(", ScriptHelper.GetString(GetString("uploader.uploadfailed")), ");}", ERROR_FUNCTION);
            ScriptHelper.RegisterStartupScript(this, typeof(string), "errorMessage", ScriptHelper.GetScript(errorScript.ToString()));
        }


        /// <summary>
        /// Generates CSS styles for the control.
        /// </summary>
        /// <param name="containerDivId">Container div client ID</param>
        protected string CreateCss(string containerDivId)
        {
            string mainSelector = String.IsNullOrEmpty(ControlGroup) ? "#" + containerDivId : "." + ControlGroup;

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} {{ cursor: pointer !important;", mainSelector);
            if (DisplayInline)
            {
                sb.AppendFormat("float:left; }} .RTL {0} {{ float:right;", mainSelector);
            }
            sb.Append("}");
            // upload image definition
            sb.AppendFormat("{0} .UploaderImage {{ background-position: top left; float:left; ", mainSelector);
            sb.Append("}");
            sb.AppendFormat(".RTL {0} .UploaderImage {{ background-position: top right; float:right;}}", mainSelector);

            // Inner div css definition
            sb.AppendFormat("{0} .InnerDiv {{ white-space:nowrap; ", mainSelector);

            if (!String.IsNullOrEmpty(AdditionalStyle))
            {
                sb.Append(AdditionalStyle);
            }
            sb.Append("}");

            return sb.ToString();
        }


        /// <summary>
        /// Generates iframe URL.
        /// </summary>
        /// <param name="containerDivId">Container div client ID</param>
        /// <param name="uploaderFrameId">Uploader iframe client ID</param>
        /// <returns>URL for iframe</returns>
        protected string GetIframeUrl(string containerDivId, string uploaderFrameId)
        {
            WindowHelper.Remove(Identifier);

            Hashtable properties = new Hashtable();

            if (AttachmentGUID != Guid.Empty)
            {
                properties.Add("attachmentguid", AttachmentGUID);
            }
            if (AttachmentGroupGUID != Guid.Empty)
            {
                properties.Add("attachmentgroupguid", AttachmentGroupGUID);
            }
            if (!string.IsNullOrEmpty(AttachmentGUIDColumnName))
            {
                properties.Add("attachmentguidcolumnname", AttachmentGUIDColumnName);
            }
            if (ResizeToWidth != ImageHelper.AUTOSIZE)
            {
                properties.Add("autoresize_width", ResizeToWidth);
            }
            if (ResizeToHeight != ImageHelper.AUTOSIZE)
            {
                properties.Add("autoresize_height", ResizeToHeight);
            }
            if (ResizeToMaxSideSize != ImageHelper.AUTOSIZE)
            {
                properties.Add("autoresize_maxsidesize", ResizeToMaxSideSize);
            }
            if (FormGUID != Guid.Empty)
            {
                properties.Add("formguid", FormGUID);
            }
            if (DocumentID != 0)
            {
                properties.Add("documentid", DocumentID);
            }
            if (NodeParentNodeID != 0)
            {
                properties.Add("parentid", NodeParentNodeID);
            }
            if (NodeGroupID != 0)
            {
                properties.Add("nodegroupid", NodeGroupID);
            }
            if (!string.IsNullOrEmpty(NodeClassName))
            {
                properties.Add("classname", NodeClassName);
            }
            if (MetaFileID != 0)
            {
                properties.Add("metafileid", MetaFileID);
            }
            if (ObjectID != 0)
            {
                properties.Add("objectid", ObjectID);
            }
            if (!string.IsNullOrEmpty(ObjectType))
            {
                properties.Add("objecttype", ObjectType);
            }
            if (!string.IsNullOrEmpty(Category))
            {
                properties.Add("category", Category);
            }
            if (SiteID != 0)
            {
                properties.Add("siteid", SiteID);
            }
            if (InsertMode)
            {
                properties.Add("insertmode", true);
            }
            if (OnlyImages)
            {
                properties.Add("onlyimages", true);
            }
            if (RaiseOnClick)
            {
                properties.Add("click", true);
            }
            if (AllowedExtensions != null)
            {
                properties.Add("allowedextensions", AllowedExtensions);
            }
            if (!string.IsNullOrEmpty(ParentElemID))
            {
                properties.Add("parentelemid", ParentElemID);
            }
            properties.Add("islive", IsLiveSite);

            properties.Add("source", CMSDialogHelper.GetMediaSource(SourceType));

            // If library information specified - pass it to the inner control (for new media file creation)
            if ((LibraryID > 0) && (LibraryFolderPath != null))
            {
                properties.Add("libraryid", LibraryID);
                properties.Add("path", LibraryFolderPath);
                properties.Add("mediafileid", MediaFileID);
                properties.Add("ismediathumbnail", IsMediaThumbnail);

                if (!string.IsNullOrEmpty(MediaFileName))
                {
                    properties.Add("filename", MediaFileName);
                }
            }

            if (IncludeNewItemInfo)
            {
                properties.Add("includeinfo", IncludeNewItemInfo);
            }

            if (NodeSiteName != null)
            {
                properties.Add("sitename", NodeSiteName);
            }

            if (!string.IsNullOrEmpty(AfterSaveJavascript))
            {
                properties.Add("aftersave", AfterSaveJavascript);
            }

            if (!string.IsNullOrEmpty(TargetFolderPath))
            {
                properties.Add("targetfolder", TargetFolderPath);
            }

            if (!string.IsNullOrEmpty(TargetFileName))
            {
                properties.Add("targetfilename", TargetFileName);
            }

            // Store properties
            WindowHelper.Add(Identifier, properties);

            // Create URL
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("?identifier={0}", Identifier);
            sb.AppendFormat("&frameid={0}", uploaderFrameId);
            sb.AppendFormat("&containerid={0}", containerDivId);
            sb.AppendFormat("&parentelemid={0}", ParentElemID);

            string validationHash = QueryHelper.GetHash(sb.ToString());

            const string UPLOADER_FOLDER = "~/CMSModules/Content/Attachments/DirectFileUploader/";

            if (AuthenticationHelper.IsAuthenticated())
            {
                sb.Insert(0, UPLOADER_FOLDER + "FileUploader.aspx");
            }
            else
            {
                sb.Insert(0, UPLOADER_FOLDER + "PublicFileUploader.aspx");
            }

            sb.Append("&hash=", validationHash);

            return sb.ToString();
        }

        #endregion
    }
}
