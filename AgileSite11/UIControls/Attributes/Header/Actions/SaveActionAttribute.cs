using System;

using CMS.Base.Web.UI.ActionsConfig;

namespace CMS.UIControls
{
    /// <summary>
    /// Adds save action to the page.
    /// </summary>
    public class SaveActionAttribute : ActionAttribute
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">Index of the action</param>
        public SaveActionAttribute(int index)
            : base(index)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">Index of the action</param>
        /// <param name="resourceString">Resource string for the item</param>
        /// <param name="targetUrl">Target URL</param>
        public SaveActionAttribute(int index, string resourceString, string targetUrl)
            : base(index, resourceString, targetUrl)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">Index of the action</param>
        /// <param name="resourceString">Resource string for the item</param>
        /// <param name="targetUrl">Target URL</param>
        /// <param name="javascript">Java-script to execute</param>
        /// <param name="tooltipResourceString">Tooltip of the action</param>
        public SaveActionAttribute(int index, string resourceString, string targetUrl, string javascript, string tooltipResourceString)
            : base(index, resourceString, targetUrl, javascript, tooltipResourceString)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Applies the attribute data to the page (object).
        /// </summary>
        /// <param name="sender">Sender object</param>
        public override void Apply(object sender)
        {
            if (sender is CMSPage)
            {
                // Let the page set the breadcrumbs
                CMSPage page = (CMSPage)sender;

                if (CheckVisibility())
                {
                    SaveAction action = new SaveAction
                    {
                        Index = Index
                    };

                    if (!string.IsNullOrEmpty(ResourceString) || !string.IsNullOrEmpty(Text))
                    {
                        action.Text = GetText(ResourceString, Text);
                    }
                    if (!string.IsNullOrEmpty(TooltipResourceString) || !string.IsNullOrEmpty(TooltipText))
                    {
                        action.Tooltip = GetText(TooltipResourceString, TooltipText);
                    }
                    if (!string.IsNullOrEmpty(TargetUrl))
                    {
                        action.RedirectUrl = GetUrl(page, TargetUrl);
                    }
                    if (!string.IsNullOrEmpty(Javascript))
                    {
                        action.OnClientClick = Resolve(Javascript);
                    }

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