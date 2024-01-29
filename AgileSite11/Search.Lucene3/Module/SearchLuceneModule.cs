using System;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Search.Lucene3;

[assembly: RegisterModule(typeof(SearchLuceneModule))]

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Represents the Search Lucene module.
    /// </summary>
    public class SearchLuceneModule : Module
    {
        /// <summary>
        /// Module constructor
        /// </summary>
        public SearchLuceneModule()
            : base(new SearchLuceneModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            LuceneSearchAnalyzer.Init();
        }


        /// <summary>
        /// Clears the module hash tables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            LuceneSearchAnalyzer.ClearAnalyzersTable();
        }
    }
}