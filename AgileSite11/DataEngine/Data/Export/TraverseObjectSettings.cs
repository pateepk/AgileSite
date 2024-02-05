using System;
using System.Collections.Generic;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class encapsulating parameters for GeneralizedInfo serialization (export).
    /// </summary>
    public class TraverseObjectSettings : SynchronizationObjectSettings
    {
        #region "Variables"

        private ExportFormatEnum mFormat = ExportFormatEnum.XML;
        private string mLocalizeToLanguage;
        private bool mIncludeMetadata;
        private List<string> mExcludedNames;

        private string mReguestStockKey;

        #endregion


        #region "RequestStock Properties"

        /// <summary>
        /// Returns key for request stock helper caching.
        /// </summary>
        public override string RequestStockKey
        {
            get
            {
                return mReguestStockKey ?? (mReguestStockKey = String.Join("|", new object[]
                {
                    base.RequestStockKey,
                    Convert.ToInt32(IncludeMetadata),
                    Format,
                    LocalizeToLanguage
                }));
            }
        }

        #endregion


        #region "General Properties"

        /// <summary>
        /// Format of the export data (xml/json/etc.).
        /// </summary>
        public ExportFormatEnum Format
        {
            get
            {
                return mFormat;
            }
            set
            {
                mFormat = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// Gets or sets the encoding used for REST responses.
        /// </summary>
        public Encoding Encoding
        {
            get;
            set;
        }


        /// <summary>
        /// Culture code of the language to which the resulting response will be localized (if there are any localization macros - for example as a display name of the object).
        /// Null or empty string mean no localization is done to the response.
        /// </summary>
        public string LocalizeToLanguage
        {
            get
            {
                return mLocalizeToLanguage;
            }
            set
            {
                mLocalizeToLanguage = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, metadata are included in the result as well.
        /// </summary>
        public bool IncludeMetadata
        {
            get
            {
                return mIncludeMetadata;
            }
            set
            {
                mIncludeMetadata = value;
                mReguestStockKey = null;
            }
        }
        

        /// <summary>
        /// If true, REST request was for translation.
        /// </summary>
        public bool Translate
        {
            get;
            set;
        }


        /// <summary>
        /// Objects with codename or display name starting with these names will be filtered out.
        /// </summary>
        public List<string> ExcludedNames
        {
            get
            {
                return mExcludedNames;
            }
            set
            {
                mExcludedNames = value;
                mReguestStockKey = null;
            }
        }

        #endregion
    }
}