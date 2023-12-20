using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Import object result
    /// </summary>
    public class ImportObjectResult
    {
        /// <summary>
        /// True if some data was imported
        /// </summary>
        public bool SomeDataImported
        {
            get;
            set;
        }


        /// <summary>
        /// Import result
        /// </summary>
        public UpdateResultEnum Result
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="result">Import result</param>
        /// <param name="someDataImported">True if some data was imported</param>
        public ImportObjectResult(UpdateResultEnum result, bool someDataImported)
        {
            Result = result;
            SomeDataImported = someDataImported;
        }
    }
}