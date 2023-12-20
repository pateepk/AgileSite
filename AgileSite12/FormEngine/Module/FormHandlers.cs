using CMS.DataEngine;

namespace CMS.FormEngine
{
    /// <summary>
    /// Workflow handlers
    /// </summary>
    internal class FormHandlers
    {
        /// <summary>
        /// Initializes the membership handlers
        /// </summary>
        public static void Init()
        {
            DataClassInfo.TYPEINFO.Events.Insert.After += Insert_Update_After;
            DataClassInfo.TYPEINFO.Events.Update.After += Insert_Update_After;
        }


        /// <summary>
        /// Executes after insert or update of DataClassInfo
        /// </summary>
        private static void Insert_Update_After(object sender, ObjectEventArgs e)
        {
            ProviderHelper.ClearHashtables(AlternativeFormInfo.OBJECT_TYPE, true);
            FormHelper.ClearFormInfos(true);
        }
    }
}
