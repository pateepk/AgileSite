using System;
using System.Text;
using System.Xml;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Search.TextExtractors
{
    /// <summary>
    /// Provides content extraction from XML files.
    /// </summary>
    public class XmlSearchTextExtractor : ISearchTextExtractor
    {
        #region "Variables"

        private static bool? mIncludeComments = null;
        private static bool? mIncludeAttributes = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, comments are indexed.
        /// </summary>
        internal static bool IncludeComments
        {
            get
            {
                if (mIncludeComments == null)
                {
                    mIncludeComments = ValidationHelper.GetBoolean(CoreServices.AppSettings["CMSSearchIndexXmlComments"], false);
                }
                return mIncludeComments.Value;
            }
            set
            {
                mIncludeComments = value;
            }
        }


        /// <summary>
        /// If true, values of the node attributes are indexed.
        /// </summary>
        internal static bool IncludeAttributes
        {
            get
            {
                if (mIncludeAttributes == null)
                {
                    mIncludeAttributes = ValidationHelper.GetBoolean(CoreServices.AppSettings["CMSSearchIndexXmlAttributes"], false);
                }
                return mIncludeAttributes.Value;
            }
            set
            {
                mIncludeAttributes = value;
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
            if (data.Stream == null)
            {
                return null;
            }

            var result = new StringBuilder();

            using (XmlReader reader = new XmlTextReader(data.Stream))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                            result.Append(reader.Value, " ");
                            break;

                        case XmlNodeType.Element:
                            if (IncludeAttributes && reader.HasAttributes)
                            {
                                for (int i = 0; i < reader.AttributeCount; i++)
                                {
                                    result.Append(reader.GetAttribute(i), " ");
                                }
                            }
                            break;

                        case XmlNodeType.Comment:
                            if (IncludeComments)
                            {
                                result.Append(reader.Value, " ");
                            }
                            break;

                        case XmlNodeType.Attribute:
                            if (IncludeAttributes)
                            {
                                result.Append(reader.Value, " ");
                            }
                            break;
                    }
                }
            }

            // Set the content field
            var content = new XmlData();
            content.SetValue(SearchFieldsConstants.CONTENT, result.ToString());

            return content;
        }
    }
}
