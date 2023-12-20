using System;

using CMS.Core;
using CMS.Helpers;
using CMS.Newsletters.Issues.Widgets.Configuration;

namespace CMS.Newsletters
{
    /// <summary>
    /// Extension methods for <see cref="IssueInfo"/> class.
    /// </summary>
    public static class IssueExtensions
    {
        /// <summary>
        /// Returns the name of issue variant.
        /// </summary>
        /// <param name="issue">Issue variant.</param>
        public static string GetVariantName(this IssueInfo issue)
        {
            return issue.IsOriginalVariant() ? ResHelper.GetString("newsletter.abvariantoriginal") : issue.IssueVariantName;
        }


        /// <summary>
        /// Indicates whether the issue is a variant that represents original issue.
        /// </summary>
        public static bool IsOriginalVariant(this IssueInfo issue)
        {
            return issue.IssueIsVariant && String.IsNullOrEmpty(issue.IssueVariantName);
        }


        /// <summary>
        /// Indicates whether the issue has a widget with unfilled required property.
        /// </summary>
        public static bool HasWidgetWithUnfilledRequiredProperty(this IssueInfo issue)
        {
            var widgetService = Service.Resolve<IZonesConfigurationServiceFactory>().Create(issue);
            return widgetService.HasWidgetWithUnfilledRequiredProperty();
        }


        /// <summary>
        /// Indicates whether the issue has a widget with missing widget definition.
        /// </summary>
        public static bool HasWidgetWithMissingDefinition(this IssueInfo issue)
        {
            var widgetService = Service.Resolve<IZonesConfigurationServiceFactory>().Create(issue);
            return widgetService.HasWidgetWithMissingDefinition();
        }
    }
}
