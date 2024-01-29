using System.Linq;

using CMS;
using CMS.Base;
using CMS.Community.Web.UI;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.UIControls;

[assembly: RegisterCustomClass("GroupEditTabsExtender", typeof(GroupEditTabsExtender))]

namespace CMS.Community.Web.UI
{
    /// <summary>
    /// Extender for edit group horizontal tabs
    /// </summary>
    public class GroupEditTabsExtender : UITabsExtender
    {
        /// <summary>
        /// Initialization of tabs.
        /// </summary>
        public override void OnInitTabs()
        {
            Control.OnTabCreated += OnTabCreated;
        }


        /// <summary>
        /// Event handling creation of tabs.
        /// </summary>
        private void OnTabCreated(object sender, TabCreatedEventArgs e)
        {
            if (e.Tab == null)
            {
                return;
            }

            var tab = e.Tab;
            if (!tab.TabName.ToLowerCSafe().EqualsCSafe("editgroupcustomfields"))
            {
                return;
            }

            // Check custom fields of group
            FormInfo formInfo = FormHelper.GetFormInfo(PredefinedObjectType.GROUP, false);
            if (!formInfo.GetFormElements(true, false, true).Any())
            {
                e.Tab = null;
            }
        }
    }
}