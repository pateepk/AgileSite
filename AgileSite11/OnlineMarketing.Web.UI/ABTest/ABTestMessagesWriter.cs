using System;
using System.Text;

using CMS.DocumentEngine;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Class that writes info messages into the page using a delegate <see cref="ShowMessageHandler"/>.
    /// </summary>
    public sealed class ABTestMessagesWriter
    {
        #region "Variables"

        private readonly ShowMessageHandler mShowMessage;

        #endregion


        #region "Delegates"

        /// <summary>
        /// Shows the specified message, optionally with a tooltip text and description.
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="text">Message text</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public delegate void ShowMessageHandler(MessageTypeEnum type, string text, string description = null, string tooltipText = null, bool persistent = true);

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets ShowMessage method handler.
        /// </summary>
        public ShowMessageHandler ShowMessage
        {
            get
            {
                return mShowMessage;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="showMessage">ShowMessage method from CMSPage</param>
        /// <exception cref="ArgumentNullException"><paramref name="showMessage"/> is null</exception>
        public ABTestMessagesWriter(ShowMessageHandler showMessage)
        {
            if (showMessage == null)
            {
                throw new ArgumentNullException("showMessage");
            }

            mShowMessage = showMessage;
        }


        /// <summary>
        /// Shows warning if any of AB variants do not have translations into all cultures.
        /// Applies only if test is multicultural and current site has more than one culture.
        /// </summary>
        /// <param name="abTest">AB test</param>
        /// <exception cref="ArgumentNullException"><paramref name="abTest"/> is null</exception>
        public void ShowMissingVariantsTranslationsWarning(ABTestInfo abTest)
        {
            if (abTest == null)
            {
                throw new ArgumentNullException("abTest");
            }

            var variants = ABVariantInfoProvider.GetVariantsWithMissingTranslations(abTest);
            if (variants.Count == 0)
            {
                return;
            }

            var builder = new StringBuilder();
            builder.Append("<ul>");
            foreach (var variant in variants)
            {
                builder.Append("<li>");

                var nodeId = TreePathUtils.GetNodeIdByAliasPath(SiteContext.CurrentSiteName, variant.ABVariantPath);
                if (nodeId > 0)
                {
                    builder.AppendFormat(
                        "<a href='{0}?action=edit&amp;nodeid={1}{2}' target='_blank'>{3}</a>",
                        URLHelper.ResolveUrl("~/Admin/cmsadministration.aspx"),
                        nodeId,
                        ApplicationUrlHelper.GetApplicationHash("cms.content", "content"),
                        HTMLHelper.HTMLEncode(ResHelper.LocalizeString(variant.ABVariantDisplayName))
                    );
                }
                else
                {
                    builder.Append(HTMLHelper.HTMLEncode(ResHelper.LocalizeString(variant.ABVariantDisplayName)));
                }

                builder.Append("</li>");
            }
            builder.Append("</ul>");

            ShowMessage(MessageTypeEnum.Warning, ResHelper.GetString("abtesting.missingvarianttranslations") + builder);
        }


        /// <summary>
        /// Shows status information about the test if it is scheduled to be started of finished. 
        /// </summary>
        /// <param name="abTest">ABTest</param>
        /// <param name="testStatus">AB test status</param>
        /// <exception cref="ArgumentNullException"><paramref name="abTest"/> is null</exception>
        public void ShowABTestScheduleInformation(ABTestInfo abTest, ABTestStatusEnum testStatus)
        {
            if (abTest == null)
            {
                throw new ArgumentNullException("abTest");
            }

            switch (testStatus)
            {
                case ABTestStatusEnum.Scheduled:
                    if ((abTest.ABTestOpenFrom != DateTimeHelper.ZERO_TIME) && (abTest.ABTestOpenTo != DateTimeHelper.ZERO_TIME))
                    {
                        ShowMessage(MessageTypeEnum.Information, String.Format(ResHelper.GetString("abtesting.scheduled.startfinish"), abTest.ABTestOpenFrom, abTest.ABTestOpenTo));
                    }
                    else if (abTest.ABTestOpenFrom != DateTimeHelper.ZERO_TIME)
                    {
                        ShowMessage(MessageTypeEnum.Information, String.Format(ResHelper.GetString("abtesting.scheduled.start"), abTest.ABTestOpenFrom));
                    }
                    break;

                case ABTestStatusEnum.Running:
                    if (abTest.ABTestOpenTo != DateTimeHelper.ZERO_TIME)
                    {
                        ShowMessage(MessageTypeEnum.Information, String.Format(ResHelper.GetString("abtesting.scheduled.finish"), abTest.ABTestOpenTo));
                    }
                    break;

                case ABTestStatusEnum.NotStarted:
                    ShowMessage(MessageTypeEnum.Information, ResHelper.GetString("abtesting.notstarted"));
                    break;
            }
        }


        /// <summary>
        /// Displays status information (running/disabled/none) about given AB test.
        /// </summary>
        /// <param name="abTest">AB test</param>
        /// <exception cref="ArgumentNullException"><paramref name="abTest"/> is null</exception>
        public void ShowStatusInfo(ABTestInfo abTest)
        {
            if (abTest == null)
            {
                throw new ArgumentNullException("abTest");
            }

            if (abTest.ABTestID <= 0)
            {
                return;
            }

            string statusPrefix = ResHelper.GetString("general.status") + ": ";
            ABTestStatusEnum status = ABTestStatusEvaluator.GetStatus(abTest);

            ShowMessage(MessageTypeEnum.Information, statusPrefix + ABTestStatusEvaluator.GetFormattedStatus(status));
        }

        #endregion
    }
}