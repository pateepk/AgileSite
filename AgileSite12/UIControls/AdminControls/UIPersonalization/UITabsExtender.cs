using System;
using System.Linq;
using System.Text;

using CMS.Base.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// UITabs Control extender.
    /// </summary>
    public abstract class UITabsExtender : UITabsExtender<UITabs>
    {
    }


    /// <summary>
    /// UITabs Control extender.
    /// </summary>
    public abstract class UITabsExtender<TControl> : ControlExtender<TControl> where TControl : UITabs
    {

        /// <summary>
        /// OnInit event handler.
        /// </summary>
        public override void OnInit()
        {
            OnInitTabs();
        }


        /// <summary>
        /// Initialization of tabs.
        /// </summary>
        public abstract void OnInitTabs();
    }
}
