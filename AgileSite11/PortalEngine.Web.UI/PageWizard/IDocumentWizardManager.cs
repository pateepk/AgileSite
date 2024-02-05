using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base.Web.UI;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Page wizard manager interface
    /// </summary>
    public interface IDocumentWizardManager
    {
        /// <summary>
        /// Restrict step order.
        /// </summary>
        bool RestrictStepOrder
        {
            get;
            set;
        }

        /// <summary>
        /// Final step URL.
        /// </summary>
        string FinalStepNextUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the last visited step index
        /// </summary>
        int LastVisitedStepIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the last confirmed step index
        /// </summary>
        int LastConfirmedStepIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Step collection
        /// </summary>
        List<DocumentWizardStep> Steps
        {
            get;
        }


        /// <summary>
        /// First step
        /// </summary>
        DocumentWizardStep FirstStep
        {
            get;
        }


        /// <summary>
        /// Current step
        /// </summary>
        DocumentWizardStep CurrentStep
        {
            get;
        }


        /// <summary>
        /// Last step
        /// </summary>
        DocumentWizardStep LastStep
        {
            get;
        }

        /// <summary>
        /// StepEventArgs
        /// </summary>
        StepEventArgs StepEventArgs
        {
            get;
        }


        /// <summary>
        /// Resets the wizard step indexes(LastConfirmedStep, LastVisitedStep)
        /// </summary>
        void ResetWizard();
    }
}
