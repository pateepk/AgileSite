using System;
using System.Linq;
using System.Text;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// CMSAbstractWizardWebPart extends CMSAbstractWebPart with Wizard manager properties
    /// </summary>
    public abstract class CMSAbstractWizardWebPart: CMSAbstractWebPart
    {
        #region "Variables"

        private IDocumentWizardManager wizardManager = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Current wizard manager 
        /// </summary>
        public IDocumentWizardManager WizardManager
        {
            get
            {
                if (wizardManager == null)
                {
                    wizardManager = DocumentWizardManager;
                    if (wizardManager == null)
                    {
                        throw new Exception("The control with ID '" + this.ID + "' requires a IPageWizardManager on the page. The IPageWizardManager must appear before any controls that need it.");
                    }
                }
                return wizardManager;
            }
        }

        #endregion

    }
}
