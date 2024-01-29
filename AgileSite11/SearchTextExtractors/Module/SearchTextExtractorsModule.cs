using System;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Search.TextExtractors;

[assembly: RegisterModule(typeof(SearchTextExtractorsModule))]

namespace CMS.Search.TextExtractors
{
    /// <summary>
    /// Represents the Search Text Extractors module.
    /// </summary>
    public class SearchTextExtractorsModule : Module
    {
        /// <summary>
        /// Module constructor
        /// </summary>
        public SearchTextExtractorsModule()
            : base(new SearchTextExtractorsModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            SearchTextExtractorManager.RegisterExtractor("txt", new TextSearchTextExtractor());
            SearchTextExtractorManager.RegisterExtractor("csv", new TextSearchTextExtractor());
            SearchTextExtractorManager.RegisterExtractor("html", new HtmlSearchTextExtractor());
            SearchTextExtractorManager.RegisterExtractor("htm", new HtmlSearchTextExtractor());
            SearchTextExtractorManager.RegisterExtractor("xml", new XmlSearchTextExtractor());
            SearchTextExtractorManager.RegisterExtractor("docx", new DocxSearchTextExtractor());
            SearchTextExtractorManager.RegisterExtractor("xlsx", new XlsxSearchTextExtractor());
            SearchTextExtractorManager.RegisterExtractor("pptx", new PptxSearchTextExtractor());
            SearchTextExtractorManager.RegisterExtractor("pdf", new PdfSearchTextExtractor());
        }
    }
}