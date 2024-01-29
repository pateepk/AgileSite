using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;

[assembly: RegisterModuleUsageDataSource(typeof(DocumentEngineUsageDataSource))]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Module usage data for document engine.
    /// </summary>
    internal class DocumentEngineUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Document engine usage data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.DocumentEngine";
            }
        }


        /// <summary>
        /// Get document engine usage data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var result = ObjectFactory<IModuleUsageDataCollection>.New();

            var contentOnlyNodes = GetContentOnlyNodes().Tables[0].Rows[0];
            var contentOnlyPagesOnInstanceCount = DataHelper.GetStringValue(contentOnlyNodes, "ContentOnlyPagesOnInstanceCount");
            var assignedContentOnlyPageTypesCount = DataHelper.GetStringValue(contentOnlyNodes, "AssignedContentOnlyPageTypesCount");

            result.Add("ContentOnlyPagesOnInstanceCount", contentOnlyPagesOnInstanceCount);
            result.Add("AssignedContentOnlyPageTypesCount", assignedContentOnlyPageTypesCount);
            result.Add("ContentOnlyPageTypesCount", GetContentOnlyPageTypesCount());
            result.Add("ContentOnlyPagesPerSite", String.Join(",", GetContentOnlyPagesPerSite().ToArray()));
            result.Add("RelatedPagesCount", GetRelatedPagesCount());
            result.Add("RelatedPagesPerFieldCount", String.Join(",", GetRelatedPagesPerFieldCount().ToArray()));

            return result;
        }


        /// <summary>
        /// Returns MultiDocumentQuery with AssignedContentOnlyPageTypesCount and ContentOnlyPagesOnInstanceCount aggregated columns.
        /// </summary>
        private MultiDocumentQuery GetContentOnlyNodes()
        {
            return new TreeProvider().SelectNodes()
               .WhereTrue("NodeIsContentOnly")
               .Columns(
                   new AggregatedColumn(AggregationType.Count, "DISTINCT NodeClassID").As("AssignedContentOnlyPageTypesCount"),
                   new AggregatedColumn(AggregationType.Count, "NodeIsContentOnly").As("ContentOnlyPagesOnInstanceCount")
               );
        }


        private int GetRelatedPagesCount()
        {
            return RelationshipInfoProvider.GetRelationships().WhereTrue("RelationshipIsAdHoc").Count;
        }


        private IEnumerable<int> GetContentOnlyPagesPerSite()
        {
            return new DataQuery(PredefinedObjectType.SITE, "selectsitelist")
                .WhereTrue("SiteIsContentOnly")
                .Column("Documents")
                .GetListResult<int>();
        }


        private IEnumerable<int> GetRelatedPagesPerFieldCount()
        {
            return RelationshipInfoProvider
                .GetRelationships()
                .Columns(new CountColumn("RelationshipNameID").As("RelatedPagesPerFieldCount"))
                .WhereTrue("RelationshipIsAdHoc")
                .GroupBy("RelationshipNameID")
                .GetListResult<int>();
        }


        private int GetContentOnlyPageTypesCount()
        {
            return DataClassInfoProvider.GetClasses().WhereTrue("ClassIsContentOnly").WhereTrue("ClassIsDocumentType").Count;
        }
    }
}
