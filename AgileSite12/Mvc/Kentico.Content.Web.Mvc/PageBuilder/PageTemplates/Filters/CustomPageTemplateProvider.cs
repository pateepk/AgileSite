using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DocumentEngine;

using PageTemplateConfigurationSerializerInternal = CMS.DocumentEngine.Internal.PageTemplateConfigurationSerializer;


namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    internal class CustomPageTemplateProvider
    {
        private readonly ISiteService siteService;


        public CustomPageTemplateProvider(ISiteService siteService)
        {
            this.siteService = siteService;
        }


        /// <summary>
        /// Returns a collection of custom page templates which are derived from a given <paramref name="defaultTemplates"/>.
        /// </summary>
        /// <param name="defaultTemplates">A collection of default templates.</param>
        public virtual IEnumerable<PageTemplateConfigurationInfo> GetTemplates(IEnumerable<PageTemplateDefinition> defaultTemplates)
        {
            if ((defaultTemplates == null) || !defaultTemplates.Any())
            {
                return Enumerable.Empty<PageTemplateConfigurationInfo>();
            }

            var defaultTemplatesIdentifiers = new HashSet<string>(defaultTemplates.Select(t => t.Identifier));

            return PageTemplateConfigurationInfoProvider
                .GetPageTemplateConfigurations()
                .OnSite(siteService.CurrentSite.SiteName)
                .ToList()
                .Where(templateConfiguration => defaultTemplatesIdentifiers.Contains(GetTemplateIdentifier(templateConfiguration)));
        }


        private string GetTemplateIdentifier(PageTemplateConfigurationInfo template)
        {
            var templateConfiguration = template.PageTemplateConfigurationTemplate;
            var serializer = new PageTemplateConfigurationSerializerInternal();
            var templateConfigurationInstance = serializer.Deserialize(templateConfiguration);

            return templateConfigurationInstance.Identifier;
        }
    }
}
