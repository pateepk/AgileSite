using CMS.Core;
using CMS.Search.Lucene3;

namespace CMS.Search
{
    /// <summary>
    /// Web farm task used to clear Lucene search analyzers tables.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class ClearSearchAnalyzersWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="LuceneSearchAnalyzer.ClearAnalyzersTable"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            LuceneSearchAnalyzer.ClearAnalyzersTable();
        }
    }
}
