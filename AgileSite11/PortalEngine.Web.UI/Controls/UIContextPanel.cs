using System.Web.UI.WebControls;

using CMS.FormEngine.Web.UI;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// UI Context wrapper panel
    /// </summary>
    public class UIContextPanel : PlaceHolder, IUIContextManager
    {
        #region "Variables"

        private UIContext mUIContext;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true UI context is loaded from parent context when initialized
        /// </summary>
        public bool InheritFromParent
        {
            get;
            set;
        }


        /// <summary>
        /// Controls context
        /// </summary>
        public UIContext UIContext
        {
            get
            {
                if (mUIContext == null)
                {
                    mUIContext = InheritFromParent ? UIContextHelper.GetUIContext(Parent).Clone() : mUIContext = new UIContext();
                    mUIContext.AssignedControl = this;
                }

                return mUIContext;
            }
            set
            {
                mUIContext = value;
            }
        }

        #endregion
    }
}
