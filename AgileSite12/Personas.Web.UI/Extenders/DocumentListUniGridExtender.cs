using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Modules;
using CMS.Personas.Web.UI;
using CMS.SiteProvider;
using CMS.UIControls;
using CMS.UIControls.UniGridConfig;

[assembly: RegisterExtender(typeof (DocumentListUniGridExtender))]

namespace CMS.Personas.Web.UI
{
    /// <summary>
    /// Extender of the UniGrid on the document listing page. This extender adds another column 
    /// to the UniGrid which displays pictures of the personas assigned to the document.
    /// </summary>
    internal sealed class DocumentListUniGridExtender : Extender<IExtensibleUniGrid>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public DocumentListUniGridExtender()
            : base("DocumentListUniGrid")
        {
        }


        /// <summary>
        /// Allows the extender to attach a behavior to the specified instance.
        /// </summary>
        /// <param name="instance">The instance to attach a behavior to.</param>
        protected override void Initialize(IExtensibleUniGrid instance)
        {
            if (!LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.Personas) ||
                !ResourceSiteInfoProvider.IsResourceOnSite(ModuleName.PERSONAS, SiteContext.CurrentSiteName))
            {
                return;
            }

            // This dictionary maps NodeIDs to List of assigned personas to this Node. It is filled in
            // UniGrid's OnAfterRetrieveData event and used in additional column's external data source event. 
            // OnAfterRetrieveData always fires earlier than external data bound, so it will work.
            // This approach is used to perform only one SQL query for all rows.
            Dictionary<int, List<int>> nodePersonas = null;

            instance.OnAfterRetrieveData += dataSet =>
            {
                var nodeIDs = dataSet.Tables[0].Rows.Cast<DataRow>().Select(row => DataHelper.GetIntValue(row, "NodeID"));

                var groups = PersonaNodeInfoProvider.GetPersonaNodes()
                                                    .WhereIn("NodeID", nodeIDs.ToList())
                                                    .AsEnumerable() // GroupBy is not supported by DataQUery, 
                                                                    // so grouping has to be done in memory
                                                    .GroupBy(personaNode => personaNode.NodeID);

                nodePersonas = groups.ToDictionary(g => g.Key, g => g.Select(personaNode => personaNode.PersonaID).ToList());

                return dataSet;
            };

            var column = new Column
            {
                Caption = "$personas.personas$",
                Source = "NodeID",
                ExternalSourceName = "PersonaPictures",
                AllowSorting = false,
            };

            instance.AddAdditionalColumn(column, (sender, sourceName, nodeID) => RenderPersonaPictures(
                ValidationHelper.GetInteger(nodeID, 0), nodePersonas));
        }


        /// <summary>
        /// Creates control which displays images of personas assigned to the document specified by <paramref name="nodeID"/>.
        /// </summary>
        /// <param name="nodeID">NodeID of document whose personas should be displayed</param>
        /// <param name="nodePersonas">Mapping of NodeIDs to the list of persona IDs which are assigned to the Node</param>
        /// <returns>Control which displays persona images or empty string if document does not have any persona</returns>
        private object RenderPersonaPictures(int nodeID, Dictionary<int, List<int>> nodePersonas)
        {
            if (!nodePersonas.ContainsKey(nodeID))
            {
                // If document is not assigned to any persona, return empty string, so cell will remain empty
                return "";
            }

            var personas = from personaID in nodePersonas[nodeID]
                           orderby personaID
                           // PersonaInfoProvider caches personas by ID (hashtables), so this call is not so expensive
                           let persona = PersonaInfoProvider.GetPersonaInfoById(personaID) 
                           where persona != null
                           select persona;

            return new PersonaPicturesInGrid(personas.ToList());
        }
    }
}