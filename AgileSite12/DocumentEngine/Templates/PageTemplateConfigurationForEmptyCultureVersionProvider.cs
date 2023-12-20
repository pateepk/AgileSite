using System;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Provides the best template configuration for a new page culture version.
    /// </summary>
    public class PageTemplateConfigurationForEmptyCultureVersionProvider
    {
        /// <summary>
        /// Provides the best template configuration for a new page culture version.
        /// </summary>
        /// <param name="nodeId">Node ID.</param>
        /// <param name="defaultCulture">Culture code of the default culture.</param>
        public PageTemplateConfiguration Get(int nodeId, string defaultCulture)
        {
            if (nodeId <= 0)
            {
                throw new ArgumentException(nameof(nodeId));
            }

            if (string.IsNullOrEmpty(defaultCulture))
            {
                throw new ArgumentNullException(nameof(defaultCulture));
            }

            var sourceTemplateConfiguration = GetSourceConfiguration(nodeId, defaultCulture);
            if (string.IsNullOrEmpty(sourceTemplateConfiguration))
            {
                return null;
            }

            return new PageTemplateConfigurationSerializer().Deserialize(sourceTemplateConfiguration);
        }


        internal virtual string GetSourceConfiguration(int nodeId, string defaultCulture)
        {
            return DocumentHelper.GetDocuments()
                .Columns("DocumentPageTemplateConfiguration")
                .WhereEquals("NodeID", nodeId)
                .Culture(defaultCulture)
                .CombineWithAnyCulture()
                .GetScalarResult<string>();
        }
    }
}
