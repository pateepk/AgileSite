using CMS.Core;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm synchronization for objects
    /// </summary>
    internal class DataTasks
    {
        private static bool? mSynchronizeMetaFiles;


        /// <summary>
        /// Gets or sets value that indicates whether file synchronization is enabled.
        /// </summary>
        public static bool SynchronizeMetaFiles
        {
            get
            {
                if (mSynchronizeMetaFiles == null)
                {
                    return (CoreServices.WebFarm.SynchronizeFiles && SettingsKeyInfoProvider.GetBoolValue("CMSWebFarmSynchronizeMetaFiles", "CMSWebFarmSynchronizeMetaFiles", true));
                }

                return mSynchronizeMetaFiles.Value;
            }
            set
            {
                mSynchronizeMetaFiles = value;
            }
        }


        /// <summary>
        /// Initializes the tasks
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask<UpdateMetaFileWebFarmTask>();
            WebFarmHelper.RegisterTask<DeleteMetaFileWebFarmTask>();
            WebFarmHelper.RegisterTask<DictionaryCommandWebFarmTask>(true, WebFarmTaskOptimizeActionEnum.GroupData);
            WebFarmHelper.RegisterTask<InvalidateObjectWebFarmTask>(true, WebFarmTaskOptimizeActionEnum.GroupData);
            WebFarmHelper.RegisterTask<InvalidateChildrenWebFarmTask>(true);
            WebFarmHelper.RegisterTask<InvalidateAllWebFarmTask>(true);
            WebFarmHelper.RegisterTask<ProcessObjectWebFarmTask>(true);
            WebFarmHelper.RegisterTask<RemoveReadOnlyObjectWebFarmTask>(true);
            WebFarmHelper.RegisterTask<ClearReadOnlyObjectsWebFarmTask>(true);
            WebFarmHelper.RegisterTask<RemoveClassStructureInfoWebFarmTask>(true);
            WebFarmHelper.RegisterTask<ClearClassStructureInfosWebFarmTask>(true);
            WebFarmHelper.RegisterTask<ClearHashtablesWebFarmTask>(true);
        }
    }
}
