using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.EventLog;

namespace Kentico.PageBuilder.Web.Mvc.Personalization
{
    /// <summary>
    /// Provides method to evaluate widget variant based on personalization settings.
    /// </summary>
    internal sealed class PersonalizationVariantEvaluator : IWidgetVariantEvaluator
    {
        private readonly IEnumerable<ConditionTypeDefinition> conditionTypes;
        private readonly IFeatureAvailabilityChecker checker;


        /// <summary>
        /// Creates an instance of <see cref="PersonalizationVariantEvaluator"/> class.
        /// </summary>
        /// <param name="provider">Component definition provider to retrieve list of registered personalization condition types.</param>
        /// <param name="checker">Personalization feature availability checker.</param>
        /// <exception cref="ArgumentNullException"><paramref name="provider"/> or <paramref name="checker"/> is <c>null</c>.</exception>
        public PersonalizationVariantEvaluator(IComponentDefinitionProvider<ConditionTypeDefinition> provider, IFeatureAvailabilityChecker checker)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            conditionTypes = provider.GetAll();

            this.checker = checker ?? throw new ArgumentNullException(nameof(checker));
        }


        /// <summary>
        /// Evaluates widget variants to select the one used for widget content rendering.
        /// </summary>
        /// <param name="widget">Widget configuration.</param>
        /// <returns>Widget variant to be used for widget content rendering.</returns>
        public WidgetVariantConfiguration Evaluate(WidgetConfiguration widget)
        {
            var conditionTypeIdentifier = widget.PersonalizationConditionTypeIdentifier;
            if (string.IsNullOrEmpty(conditionTypeIdentifier) || !IsPersonalizationAvailable())
            {
                return widget.Variants[0];
            }

            var conditionType = conditionTypes.FirstOrDefault(c => c.Identifier.Equals(conditionTypeIdentifier, StringComparison.InvariantCultureIgnoreCase));
            if (conditionType == null)
            {
                Service.Resolve<IEventLogService>().LogEvent(EventType.ERROR, "PersonalizationVariantEvaluator", "Evaluate", $"The '{conditionTypeIdentifier}' personalization condition type is not registered.");

                return widget.Variants[0];
            }

            foreach (var variant in widget.Variants.Skip(1).Reverse())
            {
                if (variant.PersonalizationConditionType.Evaluate())
                {
                    return variant;
                }
            }

            return widget.Variants[0];
        }


        private bool IsPersonalizationAvailable()
        {
            return checker.IsFeatureAvailable() && checker.IsFeatureEnabled();
        }
    }
}
