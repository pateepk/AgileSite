using System.Web.Mvc;

using CMS.Core;
using CMS.EventLog;
using CMS.SiteProvider;

using Kentico.Forms.Web.Mvc.Widgets;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>    
    /// Attribute handling errors in <see cref="KenticoFormWidgetController.Index"/>.
    /// </summary>
    internal sealed class FormWidgetExceptionFilterAttribute : FilterAttribute, IExceptionFilter
    {
        /// <summary>
        /// Logs exception to the event log. Responds with 200 status code and no markup.
        /// </summary>
        /// <param name="filterContext">Provides the exception context.</param>
        public void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                filterContext.ExceptionHandled = true;
                filterContext.Result = new EmptyResult();
                filterContext.HttpContext.Response.StatusCode = 200;

                var form = filterContext.Controller.ViewData.Model as FormWidgetViewModel;

                EventLogProvider.LogException("FormWidget", "EXCEPTION", filterContext.Exception, SiteContext.CurrentSiteID,
                additionalMessage: $"Selected form '{form.FormName}' is not in valid state. This event is logged only once.",
                loggingPolicy: LoggingPolicy.ONLY_ONCE);

                var formController = filterContext.Controller as KenticoFormWidgetController;
                if (formController != null)
                {
                    filterContext.Result = formController.GetErrorMessageIfAny(form?.FormName);
                }
            }
        }
    }
}
