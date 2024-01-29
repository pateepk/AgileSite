using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Adds breadcrumb to the page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class BreadcrumbAttribute : AbstractElementAttribute, ICMSFunctionalAttribute
    {
        #region "Properties"

        /// <summary>
        /// Index of the breadcrumb.
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
        /// Target frame for the breadcrumb item.
        /// </summary>
        public string TargetFrame
        {
            get;
            set;
        }


        /// <summary>
        /// JavaScript to execute on click.
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
        public BreadcrumbAttribute(int index)
        {
            Index = index;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">Index of the breadcrumb</param>
        /// <param name="resourceString">Resource string for the item</param>
        public BreadcrumbAttribute(int index, string resourceString)
        {
            Index = index;
            ResourceString = resourceString;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">Index of the breadcrumb</param>
        /// <param name="resourceString">Resource string for the item</param>
        /// <param name="targetUrl">Target URL</param>
        /// <param name="targetFrame">Target frame</param>
        public BreadcrumbAttribute(int index, string resourceString, string targetUrl, string targetFrame)
        {
            Index = index;
            ResourceString = resourceString;
            TargetUrl = targetUrl;
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
            // Do not apply the attribute
            if (!CheckEditedObject())
            {
                return;
            }

            if (sender is CMSPage)
            {
                // Let the page set the breadcrumbs
                CMSPage page = (CMSPage)sender;

                if (CheckVisibility())
                {
                    page.SetBreadcrumb(Index, GetText(ResourceString, Text), GetUrl(page, TargetUrl), TargetFrame, Resolve(Javascript));
                }
            }
            else
            {
                throw new Exception("[TabAttribute.Apply]: The attribute [Breadcrumb] is only valid on CMSPage class.");
            }
        }

        #endregion
    }
}