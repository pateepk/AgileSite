using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Web farm synchronization for Documents.
    /// </summary>
    internal class FormSynchronization
    {
        private static bool? mSynchronizeBizFormFiles;


        /// <summary>
        /// Gets or sets value that indicates whether file synchronization for BizForm files is enabled.
        /// </summary>
        public static bool SynchronizeBizFormFiles
        {
            get
            {
                if (mSynchronizeBizFormFiles == null)
                {
                    return (CoreServices.WebFarm.SynchronizeFiles && SettingsKeyInfoProvider.GetBoolValue("CMSWebFarmSynchronizeBizFormFiles", "CMSWebFarmSynchronizeBizFormFiles", true));
                }

                return mSynchronizeBizFormFiles.Value;
            }
            set
            {
                mSynchronizeBizFormFiles = value;
            }
        }


        /// <summary>
        /// Initializes the tasks for forms synchronization
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask<UpdateBizFormFileWebFarmTask>();
            WebFarmHelper.RegisterTask<PromoteBizFormTempFileWebFarmTask>();
            WebFarmHelper.RegisterTask<DeleteBizFormFileWebFarmTask>(true);
            WebFarmHelper.RegisterTask<InvalidateBizFormTypeInfoWebFarmTask>(true);
            WebFarmHelper.RegisterTask<ClearBizFormTypeInfosWebFarmTask>(true);
        }

    }
}
