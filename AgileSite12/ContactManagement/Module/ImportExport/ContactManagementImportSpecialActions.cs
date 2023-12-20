using CMS.CMSImportExport;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Handles special actions during the import process.
    /// </summary>
    internal static class ContactManagementImportSpecialActions
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.ProcessMainObject.Before += ProcessMainObject_Before;
        }


        private static void ProcessMainObject_Before(object sender, ImportEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;
            var objectType = infoObj.TypeInfo.ObjectType;
            var parameters = e.Parameters;

            using (new ImportSpecialCaseContext(settings))
            {
                if (objectType == ScoreInfo.OBJECT_TYPE)
                {
                    if (!parameters.SkipObjectUpdate && (parameters.ObjectProcessType == ProcessObjectEnum.All))
                    {
                        infoObj.SetValue("ScoreStatus", 2);
                        infoObj.SetValue("ScoreScheduledTaskID", null);
                    }
                }
            }
        }

        #endregion
    }
}