using System;
using System.Data;
using System.Linq;
using System.Text;

using CMS.DocumentEngine;
using CMS.Base;
using CMS.Helpers;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Document wizard step
    /// </summary>
    public class DocumentWizardStep : AbstractDataContainer<DocumentWizardStep>
    {
        #region "Variables"

        private string mStepUrl = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Step index
        /// </summary>
        public int StepIndex
        {
            get;
            set;
        }


        /// <summary>
        /// Step data
        /// </summary>
        public DataRowView StepData
        {
            get;
            set;
        }


        /// <summary>
        /// Step Url
        /// </summary>
        public string StepUrl
        {
            get
            {
                if ((mStepUrl == null) && (StepData != null))
                {
                    var node = TreeNode.New(StepData.Row);
                    mStepUrl = URLHelper.ResolveUrl(DocumentURLProvider.GetUrl(node));
                }

                return mStepUrl;
            }
            set
            {
                mStepUrl = value;
            }
        }

        #endregion


        #region "AbstractDataContainer methods"

        /// <summary>
        /// Register columns
        /// </summary>
        protected override void RegisterColumns()
        {
            base.RegisterColumns();

            RegisterColumn<int>("StepIndex", m => m.StepIndex);
            RegisterColumn("StepData", m => m.StepData);
            RegisterColumn("StepUrl", m => m.StepUrl);
        }

        #endregion
    }
}
