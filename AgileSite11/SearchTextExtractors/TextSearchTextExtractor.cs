using System;
using System.Text;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

using Ude;

namespace CMS.Search.TextExtractors
{
    /// <summary>
    /// Provides content extraction from text (.txt) files.
    /// </summary>
    public class TextSearchTextExtractor : ISearchTextExtractor
    {
        #region "Variables"

        private static bool? mDetectEncoding = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the extractor first tries to detect the encoding of the file to correctly interpret the text.
        /// </summary>
        internal static bool DetectEncoding
        {
            get
            {
                if (mDetectEncoding == null)
                {
                    mDetectEncoding = ValidationHelper.GetBoolean(CoreServices.AppSettings["CMSSearchDetectTextEncoding"], false);
                }
                return mDetectEncoding.Value;
            }
            set
            {
                mDetectEncoding = value;
            }
        }

        #endregion

        /// <summary>
        /// Extracts content from given data.
        /// </summary>
        /// <param name="data">Data to extract text from</param>
        /// <param name="context">Extraction context (ISeachDocument, Culture, etc.)</param>
        public XmlData ExtractContent(BinaryData data, ExtractionContext context)
        {
            if (data.Stream != null)
            {
                Encoding encoding = null;

                if (DetectEncoding)
                {
                    // Detect the encoding using Mozilla encoding detector
                    CharsetDetector cdet = new CharsetDetector();
                    cdet.Feed(data.Stream);
                    cdet.DataEnd();

                    string charset = cdet.Charset;
                    if (!string.IsNullOrEmpty(charset))
                    {
                        try
                        {
                            encoding = Encoding.GetEncoding(charset);

                            // Stream was read by the character encoder, move it to the beginning and read the content using detected encoding
                            data.Stream.Position = 0;
                        }
                        catch
                        {
                        }
                    }
                }

                // Default reading using Encoding.Default (this can read UTF and Default Win encoding)
                // Gives better results than Ecoding.UTF8 which can read correctly only UTF
                var result = data.Stream.ReadToEnd(encoding ?? Encoding.Default);

                return CreateXmlDataResult(result);
            }
            return null;
        }


        private XmlData CreateXmlDataResult(string content)
        {
            var xmlData = new XmlData();
            xmlData.SetValue(SearchFieldsConstants.CONTENT, content);

            return xmlData;
        }
    }
}
