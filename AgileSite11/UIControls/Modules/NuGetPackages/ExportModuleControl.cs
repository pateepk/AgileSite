using System.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Base export module control
    /// Provides access to properties necessary for extender
    /// </summary>
    public abstract class ExportModuleControl : CMSUserControl
    {
        /// <summary>
        /// ID of the resource for which the package will be created
        /// </summary>
        public virtual int ResourceID
        {
            get;
            set;
        }


        /// <summary>
        /// Control to be placed to the footer of the dialog
        /// </summary>
        public virtual Control FooterControl
        {
            get;
            private set;
        }
    }
}