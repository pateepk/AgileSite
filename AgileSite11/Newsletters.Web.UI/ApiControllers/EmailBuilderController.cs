using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CMS.Core;
using CMS.Newsletters.Filters;
using CMS.Newsletters.Issues.Widgets.Content;
using CMS.Newsletters.Issues.Widgets.Configuration;
using CMS.WebApi;
using CMS.Newsletters.Web.UI.Internal;

[assembly: RegisterCMSApiController(typeof(EmailBuilderController))]

namespace CMS.Newsletters.Web.UI.Internal
{
    /// <summary>
    /// Handles various operations with email widgets in email builder.
    /// </summary>
    /// <exclude />
    [AllowOnlyEditor]
    [HandleExceptions]
    [IsAuthorizedPerResource(ModuleName.NEWSLETTER, "Read")]
    [IsAuthorizedPerResource(ModuleName.NEWSLETTER, "AuthorIssues")]
    public sealed class EmailBuilderController : CMSApiController
    {
        private readonly IWidgetContentFilter customFilter;
        private readonly IZonesConfigurationServiceFactory factory;


        /// <summary>
        /// Creates an instance of the <see cref="EmailBuilderController"/> class.
        /// </summary>
        public EmailBuilderController()
            : this(Service.Resolve<IZonesConfigurationServiceFactory>(), null)
        {
        }


        /// <summary>
        /// Creates an instance of the <see cref="EmailBuilderController"/> class.
        /// </summary>
        /// <param name="factory">Factory to get the configuration service.</param>
        /// <param name="customFilter">Custom filter applied to the widget content. If not provided, default <see cref="EmailBuilderContentFilter"/> is used.</param>
        internal EmailBuilderController(IZonesConfigurationServiceFactory factory, IWidgetContentFilter customFilter)
        {
            this.factory = factory;
            this.customFilter = customFilter;
        }


        /// <summary>
        /// Inserts a new widget to the issue.
        /// </summary>
        /// <param name="parameters">Insert widget parameters.</param>
        /// <returns><see cref="WidgetContent"/> of inserted widget.</returns>
        [HttpPost]
        public WidgetContent InsertWidget(InsertWidgetParameters parameters)
        {
            try
            {
                var service = GetInitializedService(parameters.IssueIdentifier);
                var widget = service.InsertWidget(parameters.WidgetTypeIdentifier, parameters.ZoneIdentifier, parameters.Index);
                var filter = GetFilter(parameters.IssueIdentifier);

                return service.GetWidgetContent(widget.Identifier, filter);
            }
            catch (InvalidOperationException ex)
            {
                throw new HttpResponseException(CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }


        /// <summary>
        /// Moves the existing widget within the issue.
        /// </summary>
        /// <param name="parameters">Move widget parameters.</param>
        [HttpPost]
        public IHttpActionResult MoveWidget(MoveWidgetParameters parameters)
        {
            try
            {
                var service = GetInitializedService(parameters.IssueIdentifier);
                service.MoveWidget(parameters.WidgetIdentifier, parameters.ZoneIdentifier, parameters.Index);

                // The message is included because Firefox logs errors to console when receives empty response
                return Ok("OK");
            }
            catch (InvalidOperationException ex)
            {
                throw new HttpResponseException(CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }


        /// <summary>
        /// Removes widget from the issue.
        /// </summary>
        /// <param name="parameters">Remove widget parameters.</param>
        [HttpPost]
        public IHttpActionResult RemoveWidget(RemoveWidgetParameters parameters)
        {
            try
            {
                var service = GetInitializedService(parameters.IssueIdentifier);
                service.RemoveWidget(parameters.WidgetIdentifier);

                // The message is included because Firefox logs errors to console when receives empty response
                return Ok("OK");
            }
            catch (InvalidOperationException ex)
            {
                throw new HttpResponseException(CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }


        /// <summary>
        /// Gets content of the widget.
        /// </summary>
        /// <param name="issueIdentifier">Identifier of the issue.</param>
        /// <param name="widgetIdentifier">Identifier of the widget to retrieve.</param>
        /// <returns><see cref="WidgetContent"/> of requested widget.</returns>
        public WidgetContent GetWidgetContent(int issueIdentifier, Guid widgetIdentifier)
        {
            var service = GetInitializedService(issueIdentifier);
            var filter = GetFilter(issueIdentifier);

            return service.GetWidgetContent(widgetIdentifier, filter);
        }


        private IZonesConfigurationService GetInitializedService(int issueIdentifier)
        {
            return factory.Create(issueIdentifier);
        }


        private IWidgetContentFilter GetFilter(int issueIdentifier)
        {
            return customFilter ?? new EmailBuilderContentFilter(issueIdentifier);
        }


        private HttpResponseMessage CreateErrorResponse(HttpStatusCode statusCode, string message)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(message)
            };
        }
    }
}
