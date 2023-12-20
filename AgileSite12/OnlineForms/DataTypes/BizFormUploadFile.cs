using System;
using System.Globalization;

using CMS.DataEngine;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Class represents a file uploaded via BizForm submission.
    /// </summary>
    public class BizFormUploadFile
    {
        /// <summary>
        /// Data type identifier. Used to identify related <see cref="DataType"/> in the <see cref="DataTypeManager"/>.
        /// </summary>
        public const string DATATYPE_FORMFILE = "bizformfile";


        /// <summary>
        /// Gets or sets the system file name of the uploaded file.
        /// </summary>
        public string SystemFileName { get; set; }


        /// <summary>
        /// Gets or sets the original file name of the uploaded file.
        /// </summary>
        public string OriginalFileName { get; set; }


        internal static BizFormUploadFile ConvertToBizFormUploadFile(object value, BizFormUploadFile defaultValue, CultureInfo culture)
        {
            if (String.IsNullOrEmpty(value as string))
            {
                return defaultValue;
            }

            var split = (value as string).Split(new char[] { '/' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2)
            {
                return defaultValue;
            }

            return new BizFormUploadFile()
            {
                SystemFileName = split[0],
                OriginalFileName = split[1]
            };
        }


        internal static object ConvertToDatabaseValue(BizFormUploadFile value, object defaultValue, CultureInfo culture)
        {
            if (value == null)
            {
                return defaultValue;
            }

            return $"{value.SystemFileName}/{value.OriginalFileName}";
        }
    }
}
