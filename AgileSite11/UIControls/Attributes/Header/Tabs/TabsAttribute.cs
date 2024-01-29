using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Tabs definition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TabsAttribute : AbstractAttribute, ICMSFunctionalAttribute
    {
        #region "Properties"

        /// <summary>
        /// Target URL for the breadcrumb item.
        /// </summary>
        public string TargetFrame
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the module from which the UI elements should be loaded as tabs.
        /// </summary>
        public string ModuleName
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the UI element whose child UI elements should be loaded as tabs. If not specified, module root UI element is used.
        /// </summary>
        public string ElementName
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="targetFrame">Name of the target frame</param>
        public TabsAttribute(string targetFrame)
        {
            TargetFrame = targetFrame;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="moduleName">Name of the module from which the UI elements should be loaded as tabs</param>
        /// <param name="elementName">Name of the UI element whose child UI elements should be loaded as tabs. If not specified, module root UI element is used</param>
        /// <param name="targetFrame">Name of the target frame</param>
        public TabsAttribute(string moduleName, string elementName, string targetFrame)
        {
            ModuleName = moduleName;
            ElementName = elementName;
            TargetFrame = targetFrame;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Applies the attribute data to the page (object).
        /// </summary>
        /// <param name="sender">Sender object</param>
        public void Apply(object sender)
        {
            if (sender is CMSPage)
            {
                // Let the page set the breadcrumbs
                CMSPage page = (CMSPage)sender;

                if (!string.IsNullOrEmpty(ModuleName))
                {
                    // Load tabs from UI elements
                    page.InitTabs(ModuleName, ElementName, TargetFrame);
                }
                else
                {
                    // Init tabs manually
                    page.InitTabs(TargetFrame);
                }
            }
            else
            {
                throw new Exception("[TabAttribute.Apply]: The attribute [Tabs] is only valid on CMSPage class.");
            }
        }

        #endregion
    }
}