using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Forums;
using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

using Microsoft.Win32;

using SystemIO = System.IO;

[assembly: RegisterObjectType(typeof(ForumAttachmentInfo), ForumAttachmentInfo.OBJECT_TYPE)]

namespace CMS.Forums
{
    /// <summary>
    /// ForumAttachment data container class.
    /// </summary>
    public class ForumAttachmentInfo : AbstractInfo<ForumAttachmentInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "forums.forumattachment";


        /// <summary>
        /// Type information.
        /// </summary>        
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ForumAttachmentInfoProvider), OBJECT_TYPE, "Forums.ForumAttachment", "AttachmentID", "AttachmentLastModified", "AttachmentGUID", null, null, "AttachmentBinary", "AttachmentSiteID", "AttachmentPostID", ForumPostInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.FORUMS,
            MimeTypeColumn = "AttachmentMimeType",
            ExtensionColumn = "AttachmentFileExtension",
            SizeColumn = "AttachmentFileSize",
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                ObjectFileNameFields = { "AttachmentFileName" },
                SeparatedFields = new List<SeparatedField>
                {
                    new SeparatedField("AttachmentBinary")
                    {
                        IsBinaryField = true,
                        FileName = "file",
                        FileExtensionFieldName = "AttachmentFileExtension"
                    }
                }
           }
        };

        #endregion


        #region "Private variables"

        /// <summary>
        /// Input stream avatar data.
        /// </summary>
        protected byte[] mInputStreamData = null;

        /// <summary>
        /// Indicates whether the data from input stream were processed.
        /// </summary>
        protected bool mStreamProcessed = false;

        /// <summary>
        /// Current attachment forum id.
        /// </summary>
        private int? mAttachmentForumId = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Attachment site ID.
        /// </summary>
        public virtual int AttachmentSiteID
        {
            get
            {
                return GetIntegerValue("AttachmentSiteID", 0);
            }
            set
            {
                SetValue("AttachmentSiteID", value);
            }
        }


        /// <summary>
        /// File size.
        /// </summary>
        public virtual int AttachmentFileSize
        {
            get
            {
                return GetIntegerValue("AttachmentFileSize", 0);
            }
            set
            {
                SetValue("AttachmentFileSize", value);
            }
        }


        /// <summary>
        /// File name.
        /// </summary>
        public virtual string AttachmentFileName
        {
            get
            {
                return GetStringValue("AttachmentFileName", "");
            }
            set
            {
                SetValue("AttachmentFileName", value);
            }
        }


        /// <summary>
        /// Binary data.
        /// </summary>
        public virtual byte[] AttachmentBinary
        {
            get
            {
                return ValidationHelper.GetBinary(GetValue("AttachmentBinary"), null);
            }
            set
            {
                SetValue("AttachmentBinary", value);
            }
        }


        /// <summary>
        /// ID of post which is associated with attachment.
        /// </summary>
        public virtual int AttachmentPostID
        {
            get
            {
                return GetIntegerValue("AttachmentPostID", 0);
            }
            set
            {
                SetValue("AttachmentPostID", value);
            }
        }


        /// <summary>
        /// Attachment ID.
        /// </summary>
        public virtual int AttachmentID
        {
            get
            {
                return GetIntegerValue("AttachmentID", 0);
            }
            set
            {
                SetValue("AttachmentID", value);
            }
        }


        /// <summary>
        /// File extension.
        /// </summary>
        public virtual string AttachmentFileExtension
        {
            get
            {
                return GetStringValue("AttachmentFileExtension", "");
            }
            set
            {
                SetValue("AttachmentFileExtension", value);
            }
        }


        /// <summary>
        /// Time of last modification.
        /// </summary>
        public virtual DateTime AttachmentLastModified
        {
            get
            {
                return GetDateTimeValue("AttachmentLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("AttachmentLastModified", value);
            }
        }


        /// <summary>
        /// Mime type.
        /// </summary>
        public virtual string AttachmentMimeType
        {
            get
            {
                return GetStringValue("AttachmentMimeType", "");
            }
            set
            {
                SetValue("AttachmentMimeType", value);
            }
        }


        /// <summary>
        /// Attachment GUID.
        /// </summary>
        public virtual Guid AttachmentGUID
        {
            get
            {
                return GetGuidValue("AttachmentGUID", Guid.Empty);
            }
            set
            {
                SetValue("AttachmentGUID", value);
            }
        }


        /// <summary>
        /// Image height.
        /// </summary>
        public virtual int AttachmentImageHeight
        {
            get
            {
                return GetIntegerValue("AttachmentImageHeight", 0);
            }
            set
            {
                SetValue("AttachmentImageHeight", value, 0);
            }
        }


        /// <summary>
        /// Image width.
        /// </summary>
        public virtual int AttachmentImageWidth
        {
            get
            {
                return GetIntegerValue("AttachmentImageWidth", 0);
            }
            set
            {
                SetValue("AttachmentImageWidth", value, 0);
            }
        }


        /// <summary>
        /// Gets the attachment forum ID.
        /// </summary>
        public virtual int AttachmentForumID
        {
            get
            {
                if (mAttachmentForumId == null)
                {
                    int forumId = 0;
                    // Get forum post info to get forum id
                    ForumPostInfo fpi = ForumPostInfoProvider.GetForumPostInfo(AttachmentPostID);
                    if (fpi != null)
                    {
                        forumId = fpi.PostForumID;
                    }

                    mAttachmentForumId = forumId;
                }

                return mAttachmentForumId.Value;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ForumAttachmentInfoProvider.DeleteForumAttachmentInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ForumAttachmentInfoProvider.SetForumAttachmentInfo(this);
        }


        /// <summary>
        /// Gets collection of dependency keys to be touched when modifying the current object.
        /// </summary>
        protected override ICollection<string> GetCacheDependencies()
        {
            var dependencies = base.GetCacheDependencies();

            // Key by attachment guid and site id
            dependencies.Add("forumattachment|" + AttachmentGUID.ToString() + "|" + AttachmentSiteID);

            // Key by forum id
            dependencies.Add("forumattachment|" + AttachmentForumID);

            return dependencies;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ForumAttachment object.
        /// </summary>
        public ForumAttachmentInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ForumAttachment object from the given DataRow.
        /// </summary>
        public ForumAttachmentInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ForumAttachment object based on the file posted through the upload control.
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="maxWidth">Maximal image width</param>
        /// <param name="maxHeight">Maximal image height</param>
        /// <param name="maxSideSize">Maximal side size</param>
        public ForumAttachmentInfo(string filePath, int maxWidth, int maxHeight, int maxSideSize)
            : base(TYPEINFO)
        {
            FileInfo fi = FileInfo.New(filePath);

            // Set the metafile
            AttachmentFileName = URLHelper.GetSafeFileName(Path.GetFileName(filePath), null);
            AttachmentFileExtension = Path.GetExtension(filePath);
            AttachmentFileSize = Convert.ToInt32(fi.Length);

            // Get mime type 
            RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(AttachmentFileExtension.ToLowerCSafe());
            if ((registryKey != null) && (registryKey.GetValue("Content Type") != null))
            {
                AttachmentMimeType = registryKey.GetValue("Content Type").ToString();
            }

            // Read file content
            AttachmentBinary = new byte[AttachmentFileSize];
            AttachmentBinary = File.ReadAllBytes(filePath);

            // Set the image properties
            if (ImageHelper.IsMimeImage(AttachmentMimeType))
            {
                ImageHelper ih = new ImageHelper(AttachmentBinary);
                Image img = null;
                if ((maxHeight == 0) || (maxWidth == 0))
                {
                    AttachmentImageWidth = maxWidth = ih.ImageWidth;
                    AttachmentImageHeight = maxHeight = ih.ImageHeight;
                }

                // Resize image width and height
                else
                {
                    // Dont resize to bigger images
                    if (ih.ImageWidth < maxWidth)
                    {
                        maxWidth = ih.ImageWidth;
                    }

                    if (ih.ImageHeight < maxHeight)
                    {
                        maxHeight = ih.ImageHeight;
                    }

                    img = ih.GetResizedImage(maxWidth, maxHeight);
                }

                // Resize 
                if ((maxSideSize > 0) && ((maxSideSize <= maxWidth) || (maxSideSize <= maxHeight)))
                {
                    img = ih.GetResizedImage(maxSideSize);
                }

                if (img != null)
                {
                    var ms = new SystemIO.MemoryStream();
                    img.Save(ms, ImageFormat.Jpeg);
                    AttachmentBinary = ms.ToArray();

                    // Setup new properties
                    string filename = AttachmentFileName.Substring(0, AttachmentFileName.Length - AttachmentFileExtension.Length);
                    AttachmentFileName = filename + ".jpg";
                    AttachmentFileSize = (int)ms.Length;
                    AttachmentImageHeight = img.Height;
                    AttachmentImageWidth = img.Width;
                    AttachmentFileExtension = ".jpg";
                    AttachmentMimeType = "image/jpeg";
                }
            }
        }


        /// <summary>
        /// Constructor - Creates a new ForumAttachment object based on the file posted through the upload control.
        /// </summary>
        /// <param name="postedFile">Posted file</param>
        /// <param name="maxWidth">Maximal image width</param>
        /// <param name="maxHeight">Maximal image height</param>
        /// <param name="maxSideSize">Maximal side size</param>
        public ForumAttachmentInfo(HttpPostedFile postedFile, int maxWidth, int maxHeight, int maxSideSize)
            : base(TYPEINFO)
        {
            int fileNameStartIndex = postedFile.FileName.LastIndexOfCSafe("\\") + 1;

            // Set the metafile
            AttachmentFileName = URLHelper.GetSafeFileName(postedFile.FileName.Substring(fileNameStartIndex), null);
            AttachmentFileExtension = Path.GetExtension(postedFile.FileName);
            AttachmentFileSize = postedFile.ContentLength;
            AttachmentMimeType = MimeTypeHelper.GetSafeMimeType(postedFile.FileName, postedFile.ContentType);


            // Make copy of the posted attachment file data
            if ((postedFile.InputStream.CanRead) && (postedFile.InputStream.Position == 0))
            {
                AttachmentBinary = new byte[AttachmentFileSize];
                postedFile.InputStream.Read(AttachmentBinary, 0, AttachmentFileSize);
            }
            else
            {
                throw new Exception("[ForumAttachmentInfo.ForumAttachmentInfo]: Input stream is not at the beginning position.");
            }

            // Set the image properties
            if (ImageHelper.IsMimeImage(AttachmentMimeType))
            {
                ImageHelper ih = new ImageHelper(AttachmentBinary);
                Image img = null;
                if ((maxHeight == 0) || (maxWidth == 0))
                {
                    AttachmentImageWidth = maxWidth = ih.ImageWidth;
                    AttachmentImageHeight = maxHeight = ih.ImageHeight;
                }

                // Resize image width and height
                else
                {
                    // Dont resize to bigger images
                    if (ih.ImageWidth < maxWidth)
                    {
                        maxWidth = ih.ImageWidth;
                    }

                    if (ih.ImageHeight < maxHeight)
                    {
                        maxHeight = ih.ImageHeight;
                    }

                    img = ih.GetResizedImage(maxWidth, maxHeight);
                }

                // Resize 
                if ((maxSideSize > 0) && ((maxSideSize <= maxWidth) || (maxSideSize <= maxHeight)))
                {
                    img = ih.GetResizedImage(maxSideSize);
                }

                if (img != null)
                {
                    var ms = new SystemIO.MemoryStream();
                    img.Save(ms, ImageFormat.Jpeg);
                    AttachmentBinary = ms.ToArray();

                    // Setup new properties
                    string filename = AttachmentFileName.Substring(0, AttachmentFileName.Length - AttachmentFileExtension.Length);
                    AttachmentFileName = filename + ".jpg";
                    AttachmentFileSize = (int)ms.Length;
                    AttachmentImageHeight = img.Height;
                    AttachmentImageWidth = img.Width;
                    AttachmentFileExtension = ".jpg";
                    AttachmentMimeType = "image/jpeg";
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Removes object dependencies.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            SiteInfo si = SiteInfoProvider.GetSiteInfo(AttachmentSiteID);
            if (si != null)
            {
                // Delete all file occurrences in file system
                ForumAttachmentInfoProvider.DeleteAttachmentFile(si.SiteName, AttachmentGUID.ToString(), false, false);
            }

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }

        #endregion
    }
}