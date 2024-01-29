using CMS.CMSImportExport;
using CMS.Base;

namespace CMS.Newsletters
{
    /// <summary>
    /// Handles special actions during the Newsletter export process.
    /// </summary>
    internal static class NewsletterExport
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        internal static void Init()
        {
            ImportExportEvents.SingleExportSelection.Execute += SingleExportSelection_Execute;
        }


        private static void SingleExportSelection_Execute(object sender, SingleExportSelectionEventArgs e)
        {
            var infoObj = e.InfoObject;
            var settings = e.Settings;

            if (infoObj.TypeInfo.ObjectType.ToLowerCSafe() == IssueInfo.OBJECT_TYPE)
            {
                // Select also all issue variants
                settings.SetObjectsProcessType(ProcessObjectEnum.Selected, IssueInfo.OBJECT_TYPE_VARIANT, true);
            }
        }

        #endregion
    }
}