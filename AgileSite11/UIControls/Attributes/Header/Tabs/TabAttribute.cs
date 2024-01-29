using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Adds tab to the page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class TabAttribute : AbstractElementAttribute, ICMSFunctionalAttribute
    {
        #region "Properties"

        /// <summary>
        /// Index of the tab.
        /// </summary>
        public int Index
        {
            get;
            set;
        }


        /// <summary>
        /// Text of the breadcrumbs, has higher priority than the resource string.
        /// </summary>
        public string Text
        {
            get;
            set;
        }


        /// <summary>
        /// Resource string for the text.
        /// </summary>
        public string ResourceString
        {
            get;
            set;
        }


        /// <summary>
        /// Target URL for the breadcrumb item.
        /// </summary>
        public string TargetUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Javascript to execute on click.
        /// </summary>
        public string Javascript
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">Index of the tab</param>
        public TabAttribute(int index)
        {
            Index = index;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">Index of the tab</param>
        /// <param name="resourceString">Resource string for the item</param>
        /// <param name="targetUrl">Target URL</param>
        public TabAttribute(int index, string resourceString, string targetUrl)
            : this(index)
        {
            ResourceString = resourceString;
            TargetUrl = targetUrl;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">Index of the tab</param>
        /// <param name="resourceString">Resource string for the item</param>
        /// <param name="targetUrl">Target URL</param>
        /// <param name="javascript">Javascript</param>
        public TabAttribute(int index, string resourceString, string targetUrl, string javascript)
            : this(index, resourceString, targetUrl)
        {
            Javascript = javascript;
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

                if (CheckVisibility())
                {
                    page.SetTab(Index, GetText(ResourceString, Text), GetUrl(page, TargetUrl), Resolve(Javascript));
                }
            }
            else
            {
                throw new Exception("[TabAttribute.Apply]: The attribute [Tab] is only valid on CMSPage class.");
            }
        }

        #endregion
    }
}