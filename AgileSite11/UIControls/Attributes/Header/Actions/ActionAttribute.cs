using System;

using CMS.Base.Web.UI.ActionsConfig;

namespace CMS.UIControls
{
    /// <summary>
    /// Adds action to the page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ActionAttribute : AbstractElementAttribute, ICMSFunctionalAttribute
    {
        #region "Properties"

        /// <summary>
        /// Index of the action.
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


        /// <summary>
        /// Tooltip text, has higher priority than the tooltip resource string.
        /// </summary>
        public string TooltipText
        {
            get;
            set;
        }


        /// <summary>
        /// Tooltip resource string.
        /// </summary>
        public string TooltipResourceString
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">Index of the action</param>
        public ActionAttribute(int index)
        {
            Index = index;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">Index of the action</param>
        /// <param name="resourceString">Resource string for the item</param>
        /// <param name="targetUrl">Target URL</param>
        public ActionAttribute(int index, string resourceString, string targetUrl)
        {
            Index = index;
            ResourceString = resourceString;
            TargetUrl = targetUrl;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">Index of the action</param>
        /// <param name="resourceString">Resource string for the item</param>
        /// <param name="targetUrl">Target URL</param>
        /// <param name="javascript">Javascript to execute</param>
        /// <param name="tooltipResourceString">Tooltip of the action</param>
        public ActionAttribute(int index, string resourceString, string targetUrl, string javascript, string tooltipResourceString)
        {
            Index = index;
            ResourceString = resourceString;
            TargetUrl = targetUrl;
            Javascript = javascript;
            TooltipResourceString = tooltipResourceString;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Applies the attribute data to the page (object).
        /// </summary>
        /// <param name="sender">Sender object</param>
        public virtual void Apply(object sender)
        {
            if (sender is CMSPage)
            {
                // Let the page set the breadcrumbs
                CMSPage page = (CMSPage)sender;

                if (CheckVisibility())
                {
                    HeaderAction action = new HeaderAction
                                              {
                                                  Index = Index,
                                                  Text = GetText(ResourceString, Text),
                                                  Tooltip = GetText(TooltipResourceString, TooltipText),
                                                  RedirectUrl = GetUrl(page, TargetUrl),
                                                  OnClientClick = Resolve(Javascript)
                                              };

                    page.AddHeaderAction(action);
                }
            }
            else
            {
                throw new Exception("[ActionAttribute.Apply]: The attribute [Action] is only valid on CMSPage class.");
            }
        }

        #endregion
    }
}