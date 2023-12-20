using System;
using System.Web.Mvc;

using CMS.Core;
using CMS.EventLog;
using CMS.Helpers;

using Kentico.Forms.Web.Mvc.Internal;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Attribute handling exceptions thrown during execution of <see cref="KenticoFormComponentMarkupController.EditorRow(string, int)"/>.
    /// </summary>
    internal sealed class EditorRowExceptionFilterAttribute : FilterAttribute, IExceptionFilter
    {
        private readonly IFormComponentDefinitionProvider formComponentDefinitionProvider;
        private readonly IFormComponentActivator formComponentActivator;

        /// <summary>
        /// Initializes a new instance of <see cref="EditorRowExceptionFilterAttribute"/>.
        /// </summary>
        public EditorRowExceptionFilterAttribute()
            : this(Service.Resolve<IFormComponentDefinitionProvider>(), Service.Resolve<IFormComponentActivator>())
        {
        }


        private EditorRowExceptionFilterAttribute(IFormComponentDefinitionProvider formComponentDefinitionProvider, IFormComponentActivator formComponentActivator)
        {
            this.formComponentDefinitionProvider = formComponentDefinitionProvider;
            this.formComponentActivator = formComponentActivator;
        }


        /// <summary>
        /// Logs exception to the event log. Responds with 200 status code and with markup for
        /// <see cref="InvalidComponent"/>.
        /// </summary>
        /// <param name="filterContext">Provides the exception context.</param>
        public void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                var originalModel = filterContext.Controller.ViewData.Model as FormComponent;
                var invalidComponentDefinition = formComponentDefinitionProvider.Get(FormComponentIdentifiers.INVALID_COMPONENT_IDENTIFIER);
                var invalidComponentProperties = formComponentActivator.CreateDefaultProperties(invalidComponentDefinition) as InvalidComponentProperties;

                invalidComponentProperties.Label = ResHelper.LocalizeString(originalModel?.BaseProperties.Label);
                invalidComponentProperties.Guid = Guid.NewGuid();
                invalidComponentProperties.ErrorMessage = ResHelper.GetStringFormat("kentico.formbuilder.error.view", originalModel?.Definition.Identifier ?? "Unknown");

                var model = new InvalidComponent(invalidComponentProperties)
                {
                    Name = String.IsNullOrEmpty(originalModel?.Name) ? $"invalid-component-{Guid.NewGuid()}" : originalModel.Name,
                    Definition = invalidComponentDefinition
                };

                var result = new PartialViewResult
                {
                    ViewName = "~/Views/Shared/Kentico/FormBuilder/_FormField.cshtml",
                    ViewData = new ViewDataDictionary()
                    {
                        Model = model
                    }
                };
                result.ViewData.AddFormFieldRenderingConfiguration(SystemRenderingConfigurations.EditorField);

                filterContext.Result = result;
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.StatusCode = 200;

                EventLogProvider.LogException(FormBuilderConstants.EVENT_LOG_SOURCE, filterContext.RouteData.Values["action"]?.ToString(), filterContext.Exception);
            }
        }
    }
}
