using System;
using System.Web;
using System.Web.Mvc;

using CMS.Helpers;

using Kentico.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains helper methods for rendering HTML for alert messages.
    /// </summary>
    public static class MessageExtensions
    {
        /// <summary>
        /// Returns HTML markup with Info message.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="message">HTML encoded message.</param>
        public static MvcHtmlString InfoMessage(this ExtensionPoint<HtmlHelper> htmlHelper, IHtmlString message)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            return GetMessage(message, MessageType.Info);
        }


        /// <summary>
        /// Returns HTML markup with Error message.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="message">HTML encoded message.</param>
        public static MvcHtmlString ErrorMessage(this ExtensionPoint<HtmlHelper> htmlHelper, IHtmlString message)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            return GetMessage(message, MessageType.Error);
        }


        internal static MvcHtmlString GetMessage(IHtmlString message, MessageType messageType)
        {
            string iconClass;
            string screenReaderText;
            string alertTypeClass;

            switch (messageType)
            {
                case MessageType.Info:
                    iconClass = "icon-i-circle";
                    screenReaderText = ResHelper.GetString("general.info");
                    alertTypeClass = "ktc-alert-info";
                    break;
                case MessageType.Error:
                    iconClass = "icon-times-circle";
                    screenReaderText = ResHelper.GetString("general.error");
                    alertTypeClass = "ktc-alert-error";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), $"The '{nameof(MessageType)}.{messageType}' is not supported.");
            }

            var divAlertWrapper = new TagBuilder("div");
            divAlertWrapper.AddCssClass($"ktc-alert {alertTypeClass}");

            var divAlertLabel = new TagBuilder("div");
            divAlertLabel.AddCssClass("ktc-alert-label");
            divAlertLabel.InnerHtml = message.ToHtmlString();

            var icon = $"<span class=\"ktc-alert-icon\"><i class=\"{iconClass}\"></i><span class=\"ktc-sr-only\">{screenReaderText}</span></span>";
            divAlertWrapper.InnerHtml = icon + divAlertLabel;

            return new MvcHtmlString(divAlertWrapper.ToString());
        }


        internal enum MessageType
        {
            Info,
            Error
        }
    }
}
