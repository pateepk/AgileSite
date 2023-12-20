using System;

using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.Sections;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Registers and sets default section.
    /// </summary>
    internal static class KenticoDefaultSection
    {
        /// <summary>
        /// Default section identifier.
        /// </summary>
        public const string DEFAULT_SECTION_IDENTIFIER = "Kentico.DefaultSection";


        /// <summary>
        /// Registers default section when <see cref="PageBuilderOptions.RegisterDefaultSection"/> set to <c>true</c> and
        /// sets built-in section <see cref="DEFAULT_SECTION_IDENTIFIER"/> as a default section when <see cref="PageBuilderOptions.DefaultSectionIdentifier"/>
        /// is <c>null</c>.
        /// </summary>
        /// <param name="options">PageBuilder options.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
        public static void Register(PageBuilderOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (options.RegisterDefaultSection)
            {
                var componentDefinitionStore = ComponentDefinitionStore<SectionDefinition>.Instance;
                componentDefinitionStore.Add(new SectionDefinition(DEFAULT_SECTION_IDENTIFIER,
                    typeof(KenticoDefaultSectionController),
                    "{$kentico.pagebuilder.defaultsection.name$}",
                    "{$kentico.pagebuilder.defaultsection.description$}",
                    "icon-square"));

                // In case no default section is specified, use built-in default section
                if (string.IsNullOrEmpty(options.DefaultSectionIdentifier))
                {
                    options.DefaultSectionIdentifier = DEFAULT_SECTION_IDENTIFIER;
                }
            }
        }
    }
}
