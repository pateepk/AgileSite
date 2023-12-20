using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.EventLog;
using CMS.LicenseProvider;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Retrieves list of registered component definitions for Page builder.
    /// </summary>
    public sealed class ComponentDefinitionProvider<TDefinition> : IComponentDefinitionProvider<TDefinition> 
        where TDefinition : ComponentDefinitionBase
    {
        private readonly IFeatureAvailabilityCheckerFactory checkerFactory;


        /// <summary>
        /// Creates instance of <see cref="ComponentDefinitionProvider{TDefinition}"/>
        /// </summary>
        public ComponentDefinitionProvider()
        {
            checkerFactory = new FeatureAvailabilityCheckerFactory();
        }


        internal ComponentDefinitionProvider(IFeatureAvailabilityCheckerFactory checkerFactory)
        {
            this.checkerFactory = checkerFactory;
        }


        /// <summary>
        /// Gets list of all registered component definitions.
        /// </summary>
        public IEnumerable<TDefinition> GetAll()
        {            
            var components = ComponentDefinitionStore<TDefinition>.Instance.GetAll().ToList();
            if (!components.Any())
            {
                return Enumerable.Empty<TDefinition>();
            }

            var checker = checkerFactory.GetAvailabilityChecker<TDefinition>();
            if (checker != null && !checker.IsFeatureAvailable())
            {
                Service.Resolve<IEventLogService>().LogEvent(EventType.WARNING, "ComponentDefinitionProvider", LicenseHelper.LICENSE_LIMITATION_EVENTCODE, "Feature is not available in the current license.");
                return Enumerable.Empty<TDefinition>();
            }

            return components;
        }
    }
}
