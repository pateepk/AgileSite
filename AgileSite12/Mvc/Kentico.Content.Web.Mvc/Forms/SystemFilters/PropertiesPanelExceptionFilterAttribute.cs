using System.Web.Mvc;

using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;

using Kentico.Forms.Web.Mvc.Internal;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Attribute handling errors thrown during execution of <see cref="KenticoFormBuilderPropertiesPanelController"/>'s actions.
    /// </summary>
    internal sealed class PropertiesPanelExceptionFilterAttribute : FilterAttribute, IExceptionFilter
    {
        /// <summary>
        /// Logs exception to the event log. Responds with 200 status code and with markup 
        /// containing error message.
        /// </summary>
        public void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                var action =  filterContext.RouteData.Values["action"] as string;
                var result = new ContentResult()
                {
                    Content = BuildResult(action)
                };

                filterContext.Result = result;
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.StatusCode = 200;

                EventLogProvider.LogException(FormBuilderConstants.EVENT_LOG_SOURCE, action, filterContext.Exception, SiteContext.CurrentSiteID);
            }
        }


        private string BuildResult(string action)
        {
            var builder = new TagBuilder("div");
            if (action == nameof(KenticoFormBuilderPropertiesPanelController.GetPropertiesMarkup))
            {
                builder.AddCssClass("ktc-form-builder-tab-content-inner");
            }

            builder.InnerHtml = MessageExtensions.GetMessage(new MvcHtmlString(ResHelper.GetString("kentico.formbuilder.error.propertiespanel")), MessageExtensions.MessageType.Error).ToString();

            return builder.ToString();
        }
    }
}
