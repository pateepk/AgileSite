using CMS.DataEngine;
using CMS.Synchronization;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Base class for log document change.
    /// </summary>
    public class BaseLogDocumentChangeSettings : AbstractSynchronizationSettings
    {
        #region "Variables"

        private TreeProvider mTree = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Tree provider.
        /// </summary>
        public TreeProvider Tree
        {
            get
            {
                if (mTree == null)
                {
                    mTree = new TreeProvider();
                    mTree.AllowAsyncActions = RunAsynchronously;
                }

                return mTree;
            }
            set
            {
                mTree = value;
            }
        }


        /// <summary>
        /// Additional task parameters.
        /// </summary>
        public TaskParameters TaskParameters
        {
            get;
            set;
        }


        /// <summary>
        /// If true, also the PublishDocument task is logged for Published documents under workflow along with an PublishDocument task.
        /// </summary>
        public bool EnsurePublishTask
        {
            get;
            set;
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        public BaseLogDocumentChangeSettings()
            :base()
        { }

        #endregion
    }
}