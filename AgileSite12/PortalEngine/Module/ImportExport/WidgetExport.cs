using CMS.Base;
using CMS.CMSImportExport;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Handles special actions during the Widget export process.
    /// </summary>
    internal static class WidgetExport
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


            if (infoObj.TypeInfo.ObjectType.ToLowerCSafe() == WidgetInfo.OBJECT_TYPE)
            {
                var selectedWebParts = WebPartExport.GetDependentWebPart(infoObj);
                // Add web parts
                if (selectedWebParts.Count != 0)
                {
                    settings.SetObjectsProcessType(ProcessObjectEnum.Selected, WebPartInfo.OBJECT_TYPE, false);
                    settings.SetSelectedObjects(selectedWebParts, WebPartInfo.OBJECT_TYPE, false);
                }
            }
        }

        #endregion
    }
}