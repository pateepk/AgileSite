using System.Web;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Wrapper class to provide source attachment data
    /// </summary>
    public class AttachmentSource
    {
        #region "Properties"

        /// <summary>
        /// Existing attachment
        /// </summary>
        internal DocumentAttachment Attachment
        {
            get;
            private set;
        }


        /// <summary>
        /// File path
        /// </summary>
        internal string FilePath
        {
            get;
            private set;
        }


        /// <summary>
        /// POsted file
        /// </summary>
        internal HttpPostedFile PostedFile
        {
            get;
            private set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor. Private so that the source can be passed only through implicit conversion
        /// </summary>
        private AttachmentSource()
        {

        }


        /// <summary>
        /// Implicit operator to convert DocumentAttachment to Attachment source
        /// </summary>
        /// <param name="attachment">Attachment</param>
        public static implicit operator AttachmentSource(DocumentAttachment attachment)
        {
            if (attachment == null)
            {
                return null;
            }

            return new AttachmentSource { Attachment = attachment };
        }


        /// <summary>
        /// Implicit operator to convert file path to Attachment source
        /// </summary>
        /// <param name="filePath">File path</param>
        public static implicit operator AttachmentSource(string filePath)
        {
            if (filePath == null)
            {
                return null;
            }

            return new AttachmentSource { FilePath = filePath };
        }


        /// <summary>
        /// Implicit operator to convert posted file to Attachment source
        /// </summary>
        /// <param name="postedFile">POsted file</param>
        public static implicit operator AttachmentSource(HttpPostedFile postedFile)
        {
            if (postedFile == null)
            {
                return null;
            }

            return new AttachmentSource { PostedFile = postedFile };
        }

        #endregion
    }
}