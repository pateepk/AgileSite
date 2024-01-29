using System;
using System.Linq;
using System.Text;

using CMS.Modules;

namespace CMS.UIControls
{
    /// <summary>
    /// Tab created event arguments
    /// </summary>
    public class TabCreatedEventArgs
    {
        /// <summary>
        /// UI Element
        /// </summary>
        public UIElementInfo UIElement;

        /// <summary>
        /// Created tab
        /// </summary>
        public UITabItem Tab;

        /// <summary>
        /// Created tab index
        /// </summary>
        public int TabIndex;
    }
}
