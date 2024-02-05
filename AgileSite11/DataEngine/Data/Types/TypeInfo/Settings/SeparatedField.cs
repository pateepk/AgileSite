using System;
using System.Linq;
using System.Text;


namespace CMS.DataEngine
{
    /// <summary>
    /// Definition of separated field configuration. Content of separated field is stored in extra file when using continuous integration.
    /// </summary>
    public class SeparatedField
    {
        private const string DEFAULT_EXTENSION = ".dat";


        /// <summary>
        /// Info field name.
        /// </summary>
        public string FieldName
        {
            get;
            private set;
        }


        /// <summary>
        /// Name of the separated file.
        /// </summary>
        public string FileName
        {
            get;
            set;
        }


        /// <summary>
        /// Extension of the separated file. This extension has lower priority than <see cref="FileExtensionFieldName"/>.
        /// </summary>
        public string FileExtension
        {
            get;
            set;
        }


        /// <summary>
        /// Name of info field containing separated file extension. This extension has higher priority than <see cref="FileExtension"/>.
        /// </summary>
        public string FileExtensionFieldName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the separated field contains binary data or not.
        /// </summary>
        public bool IsBinaryField
        {
            get;
            set;
        }


        /// <summary>
        /// Creates a new instance of SeparatedField for given field name.
        /// </summary>
        /// <param name="name">Info field name.</param>
        public SeparatedField(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Field name cannot be empty", "name");
            }

            FieldName = name;
            FileName = name;
            FileExtension = DEFAULT_EXTENSION;
        }
    }
}
