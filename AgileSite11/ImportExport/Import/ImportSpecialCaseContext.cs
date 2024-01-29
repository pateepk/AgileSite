using System;

using CMS.Base;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Class representing context for import special cases.
    /// </summary>
    public class ImportSpecialCaseContext : DisposableObject
    {
        #region "Constructors"

        /// <summary>
        /// Constructor. Ensures context for special actions during import/export process. Use this context when you modify any instance of object to prevent from unwanted tasks logging.
        /// </summary>
        /// <param name="settings">Import settings</param>
        public ImportSpecialCaseContext(SiteImportSettings settings)
        {
            Using(new CMSActionContext()
                {
                    LogSynchronization = settings.LogSynchronization,
                    LogIntegration = settings.LogIntegration,
                    CreateVersion = settings.CreateVersion,
                    AllowAsyncActions = true,
                    TouchParent = true,
                });
        }

        #endregion
    }
}