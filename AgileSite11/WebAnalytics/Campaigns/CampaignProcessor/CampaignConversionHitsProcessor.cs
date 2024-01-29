using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Activities;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.WebAnalytics.Internal;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Calculates campaign reports based on relevant <see cref="ActivityInfo"/> records.
    /// </summary>
    internal class CampaignConversionHitsProcessor
    {
        private readonly ICampaignConversionHitsService mHitsService;
        private readonly ICampaignConversionHitsAggregator mHitsAggregator;
        private readonly IActivityQueueProcessor mActivityQueue;
        private readonly IActivityUrlHashService mHashService;

        private DateTime mTo;
        private int mLastActivityID;


        /// <summary>
        /// Creates new instance of <see cref="CampaignConversionHitsProcessor"/>
        /// </summary>
        /// <remarks> 
        /// Report is calculated as an increment added to the already calculated data. 
        /// Calculation start is stored in <see cref="CampaignInfo.CampaignCalculatedTo"/> field.
        /// </remarks>
        public CampaignConversionHitsProcessor()
        {
            mHitsService = Service.Resolve<ICampaignConversionHitsService>();
            mHitsAggregator = Service.Resolve<ICampaignConversionHitsAggregator>();
            mActivityQueue = Service.Resolve<IActivityQueueProcessor>();
            mHashService = Service.Resolve<IActivityUrlHashService>();
        }


        /// <summary>
        /// Calculates campaign conversion report.
        /// </summary>
        /// <remarks>
        /// Calculation is executed only for campaign which has related not processed <see cref="ActivityInfo"/> records.
        /// </remarks>
        /// <param name="siteID">Represents site ID on which campaigns` reports are calculated. 
        /// If <c>0</c>, reports for all campaigns on all sites will be calculated.</param>
        public void CalculateReports(int siteID)
        {
            InitCalculation();

            var campaigns = GetCampaigns(siteID);
            foreach (var campaign in campaigns)
            {
                CalculateReportInternal(campaign);
            }
        }


        /// <summary>
        /// Calculates report for given campaign.
        /// </summary>
        /// <param name="campaign">Instance of <see cref="CampaignInfo"/> for which is report calculated.</param>
        public void CalculateReport(CampaignInfo campaign)
        {
            InitCalculation();

            CalculateReportInternal(campaign);
        }


        /// <summary>
        /// Calculates report for given campaign.
        /// </summary>
        /// <remarks>
        /// It is expected that <see cref="InitCalculation"/> method was called previously.
        /// </remarks>
        /// <param name="campaign">Instance of <see cref="CampaignInfo"/> for which is report calculated.</param>
        private void CalculateReportInternal(CampaignInfo campaign)
        {
            if (campaign == null)
            {
                return;
            }

            // Initialize data common for all conversions within the campaign
            var calculationData = new CampaignCalculationData(campaign);

            var hitsSourceTable = CreateCampaignConversionHitsDataTable();

            // Calculate hits for all campaign`s conversions
            foreach (var type in calculationData.DistinctConversionTypes)
            {
                var hitsTable = CalculateCampaignConversionsHits(calculationData, type);

                if (!DataHelper.DataSourceIsEmpty(hitsTable))
                {
                    hitsSourceTable.Merge(hitsTable);
                }
            }

            var groupedTable = ReplaceEmptyUtmSourceAndRegroupConversions(hitsSourceTable);

            using (var tr = new CMSTransactionScope())
            {
                mHitsService.UpsertCampaignConversionHits(campaign.CampaignID, groupedTable, mTo);

                mHitsAggregator.AggregateHits(calculationData.Conversions);

                tr.Commit();
            }
        }


        private void InitCalculation()
        {
            mTo = DateTime.Now;

            // Process queued activities (they should be included in the report, because they were created before mTo)
            mActivityQueue.InsertActivitiesFromQueueToDB();

            mLastActivityID = GetLastActivityID();
        }


        /// <summary>
        /// Pre-processes data for bulk SQL upsert.
        /// </summary>
        /// <remarks>
        /// Ensures ActivityUTMSource for organic hits, i.e. hits with empty UTM source.
        /// Groups rows with the same ActivityUTMSource.
        /// </remarks>
        /// <param name="sourceTable">Computed conversion hits.</param>
        private DataTable ReplaceEmptyUtmSourceAndRegroupConversions(DataTable sourceTable)
        {
            if (sourceTable == null)
            {
                return null;
            }

            ReplaceEmptyUtmSourceWithDefaultValue(sourceTable);
            
            return RegroupConversionsByUtmSourceAndContent(sourceTable);
        }


        private void ReplaceEmptyUtmSourceWithDefaultValue(DataTable sourceTable)
        {
            foreach (DataRow row in sourceTable.Rows.Cast<DataRow>().Where(row => row["ActivityUTMSource"] == DBNull.Value))
            {
                row["ActivityUTMSource"] = CampaignProcessorConstants.DEFAULT_UTM_SOURCE;
            }
        }


        private DataTable RegroupConversionsByUtmSourceAndContent(DataTable sourceTable)
        {
            // In case user defined UTM source name is same as the default one -> group data by ActivityUTMSource and CampaignConversionID
            // Data are bulk upserted to database using SQL merge which requires grouped data (it processes all rows at once)
            var groupedData = GroupConversionsByUtmSourceAndContent(sourceTable);
            return ConvertGroupedConversionsToDataTable(groupedData, sourceTable);
        }


        private DataTable ConvertGroupedConversionsToDataTable(IEnumerable<CampaignConversionHits> groupedData, DataTable sourceTable)
        {
            var groupedTable = sourceTable.Clone();

            foreach (var row in groupedData)
            {
                groupedTable.Rows.Add(row.Hits, row.CampaignConversionID, row.ActivityUtmSource, row.ActivityUtmContent);
            }

            return groupedTable;
        }


        private IEnumerable<CampaignConversionHits> GroupConversionsByUtmSourceAndContent(DataTable sourceTable)
        {
            return sourceTable
                .AsEnumerable()
                .GroupBy(row => new
                {
                    Source = row.Field<string>("ActivityUTMSource"),
                    Content = row.Field<string>("ActivityUTMContent"),
                    ConversionID = row.Field<int>("CampaignConversionID")
                })
                .Select(grp => new CampaignConversionHits
                {
                    Hits = grp.Select(r => r.Field<int>("Hits")).Sum(),
                    CampaignConversionID = grp.Key.ConversionID,
                    ActivityUtmSource = grp.Key.Source,
                    ActivityUtmContent = grp.Key.Content
                });
        }


        /// <summary>
        /// Creates dataset definition representing user defined type Type_Analytics_CampaignConversionHitsTable.
        /// </summary>
        /// <remarks>
        /// Table columns must be defined in the same order as are columns in user defined type Type_Analytics_CampaignConversionHitsTable.
        /// </remarks>
        private DataTable CreateCampaignConversionHitsDataTable()
        {
            var table = new DataTable();
            table.TableName = "HitsToUpsertTable";

            table.Columns.Add("Hits", typeof(int));
            table.Columns.Add("CampaignConversionID", typeof(int));
            table.Columns.Add("ActivityUTMSource", typeof(string));
            table.Columns.Add("ActivityUTMContent", typeof(string));

            return table;
        }


        /// <summary>
        /// Calculates report increment for <see cref="CampaignCalculationData.Campaign"/> and <see cref="ActivityInfo.ActivityType"/>. 
        /// </summary>
        /// <param name="calculationData">Information needed for report calculation.</param>
        /// <param name="conversionType">Campaign conversion type included in the report.</param>
        private DataTable CalculateCampaignConversionsHits(CampaignCalculationData calculationData, CampaignConversionType conversionType)
        {
            var isContentOnly = calculationData.SiteIsContentOnly;
            var type = conversionType.ActivityType;

            var activityParameterColumn = GetActivityParameterColumn(type, conversionType.IsForAll, isContentOnly);

            var conversions = calculationData.Conversions.Where(c => c.CampaignConversionActivityType == type).ToList();
            IDictionary<int, IEnumerable<long>> conversionIdentifiers = new Dictionary<int, IEnumerable<long>>();

            var query = GetActivityQuery(calculationData, type);

            // Do not group by -> just count activities
            if (activityParameterColumn == "ActivityType")
            {
                query
                    .Columns("ActivityUTMSource", "ActivityUTMContent")
                    .AddColumn(new CountColumn().As("Hits"))
                    .GroupBy("ActivityUTMSource", "ActivityUTMContent");
            }
            else
            {
                conversionIdentifiers = GetConversionsItemsIdentifiers(conversions, type, isContentOnly, calculationData.Campaign.CampaignSiteID);
                query
                    .Columns("ActivityUTMSource", "ActivityUTMContent", activityParameterColumn)
                    .AddColumn(new CountColumn().As("Hits"))
                    .GroupBy("ActivityUTMSource", "ActivityUTMContent", activityParameterColumn)
                    .WhereIn(activityParameterColumn, conversionIdentifiers.SelectMany(pair => pair.Value).Distinct().ToList());
            }

            var result = query.Result;
            var table = result.Tables[0];

            table = SetConversionIDs(table, conversions, activityParameterColumn, conversionIdentifiers);

            return table;
        }


        private ObjectQuery<ActivityInfo> GetActivityQuery(CampaignCalculationData calculationData, string type)
        {
            return new ObjectQuery<ActivityInfo>()
                .From(new QuerySource(new QuerySourceTable(new ObjectSource<ActivityInfo>(), hints: SqlHints.NOLOCK)))
                .WhereEquals("ActivityType", type)
                .WhereEquals("ActivityCampaign", calculationData.Campaign.CampaignUTMCode)
                .WhereGreaterOrEquals("ActivityID", calculationData.StartActivityID)
                .WhereLessOrEquals("ActivityID", mLastActivityID);
        }


        /// <summary>
        /// Returns conversion item identifiers for WHERE IN clause according their type. 
        /// </summary>
        /// <param name="conversions">Conversion collection</param>
        /// <param name="type">Activity type</param>
        /// <param name="isContentOnly">Indicates whether the campaign is on content only site</param>
        /// <param name="campaignSiteId">Campaign site id</param>
        /// <returns>Dictionary where as key is conversion ID and values are conversion item identifiers 
        /// (in case of page visit on A/B tested page we can have more of them)</returns>
        private IDictionary<int, IEnumerable<long>> GetConversionsItemsIdentifiers(IEnumerable<CampaignConversionInfo> conversions, string type, bool isContentOnly, int campaignSiteId)
        {
            IDictionary<int, IEnumerable<long>> conversionIdentifiers = new Dictionary<int, IEnumerable<long>>();

            foreach (var conversion in conversions)
            {
                var identifiers = GetConversionItemIdentifiers(conversion, type, isContentOnly, campaignSiteId);
                conversionIdentifiers.Add(conversion.CampaignConversionID, identifiers);
            }

            return conversionIdentifiers;
        }


        private IEnumerable<long> GetConversionItemIdentifiers(CampaignConversionInfo conversion, string type, bool isContentOnly, int campaignSiteId)
        {
            if (isContentOnly && (PredefinedActivityType.PAGE_VISIT == type))
            {
                return new[] { GetCampaignConversionHash(conversion)};
            }

            if (PredefinedActivityType.PAGE_VISIT == type)
            {
                return GetConversionItemIdentifiersForPageVisit(conversion, campaignSiteId);
            }

            return new long[] { conversion.CampaignConversionItemID };
        }


        /// <summary>
        /// In case there is running A/B test on selected page we have to include all variant node IDs into calculation.
        /// </summary>
        private IEnumerable<long> GetConversionItemIdentifiersForPageVisit(CampaignConversionInfo conversion, int campaignSiteId)
        {
            var selectedNodePath = GetNodePathByNodeId(conversion.CampaignConversionItemID, campaignSiteId);
            if (selectedNodePath != null)
            {
                var abTestId = GetAbTestId(selectedNodePath);
                if (abTestId != 0)
                {
                    var abTestOriginalPath = GetAbTestOriginalPath(abTestId);
                    var paths = GetAbTestVariantsPaths(abTestId);
                    paths.Add(abTestOriginalPath);
                    return paths.Select(path => (long)GetNodeIdByNodePath(path, campaignSiteId));
                }
            }

            return new long[]{conversion.CampaignConversionItemID};
        }


        private static string GetNodePathByNodeId(int nodeId, int campaignSiteId)
        {
            return DocumentHelper.GetDocuments()
                                 .OnSite(campaignSiteId)
                                 .WhereEquals("NodeId", nodeId)
                                 .Column("NodeAliasPath")
                                 .GetScalarResult<string>();
        }


        /// <summary>
        /// Tries to resolve A/B tests by provided path. Method looks into A/B test original document path and into A/B variant paths.
        /// </summary>
        private int GetAbTestId(string nodePath)
        {
            var nodeId = GetAbTestByOriginalPagePath(nodePath);
            if (nodeId == 0)
            {
                nodeId = GetAbTestByVariantPath(nodePath);
            }
            return nodeId;
        }


        private static int GetAbTestByOriginalPagePath(string abTestOriginalPath)
        {
            return new ObjectQuery(PredefinedObjectType.ABTEST).Column("ABTestId")
                                                               .WhereEquals("ABTestOriginalPage", abTestOriginalPath)
                                                               .GetScalarResult(0);
        }


        private static int GetAbTestByVariantPath(string variantPath)
        {
            return new ObjectQuery(PredefinedObjectType.ABVARIANT).Column("ABVariantTestId")
                                                                  .WhereEquals("ABVariantPath", variantPath)
                                                                  .GetScalarResult(0);
        }


        private static string GetAbTestOriginalPath(int abTestId)
        {
            return new ObjectQuery(PredefinedObjectType.ABTEST).Column("ABTestOriginalPage")
                                                               .WhereEquals("ABTestId", abTestId)
                                                               .GetScalarResult<string>();
        }


        private static IList<string> GetAbTestVariantsPaths(int abTestId)
        {
            return new ObjectQuery(PredefinedObjectType.ABVARIANT).Column("ABVariantPath")
                                                                  .WhereEquals("ABVariantTestId", abTestId)
                                                                  .GetListResult<string>();
        }


        private static int GetNodeIdByNodePath(string path, int campaignSiteId)
        {
            return DocumentHelper.GetDocuments()
                                 .OnSite(campaignSiteId)
                                 .Path(path, PathTypeEnum.Single)
                                 .Column("NodeID")
                                 .GetScalarResult(0);
        }


        /// <summary>
        /// Calculates hash from <see cref="CampaignConversionInfo.CampaignConversionURL"/>.
        /// </summary>
        /// <param name="conversion">Campaign conversion</param>
        private long GetCampaignConversionHash(CampaignConversionInfo conversion)
        {
            return mHashService.GetActivityUrlHash(conversion.CampaignConversionURL);
        }


        /// <summary>
        /// Assigns a relevant conversion identifier for each hit item. Assigns the identifier based on the type of conversion.
        /// When the same conversion is present in both conversions and campaign steps,
        /// the database row is duplicated with the corresponding CampaignConversionID value.
        /// </summary>
        /// <param name="hitsTable">Table containing hits for one activity type.</param>
        /// <param name="conversions">Campaign conversion list.</param>
        /// <param name="activityParameterColumn">Name of the column which contains the activity parameter.</param>
        /// <param name="conversionIdentifiers"></param>
        /// <returns>Table with the assigned conversions.</returns>
        private DataTable SetConversionIDs(DataTable hitsTable, List<CampaignConversionInfo> conversions, string activityParameterColumn, IDictionary<int, IEnumerable<long>> conversionIdentifiers)
        {
            hitsTable.Columns.Add("CampaignConversionID", typeof(int));

            var hitsWithIdentifiers = hitsTable.Clone();

            foreach (DataRow hit in hitsTable.Rows)
            {
                var mainConversion = conversions.FirstOrDefault(c => IsRelatedConversion(c, hit, activityParameterColumn, conversionIdentifiers) && !c.CampaignConversionIsFunnelStep);
                var stepConversion = conversions.FirstOrDefault(c => IsRelatedConversion(c, hit, activityParameterColumn, conversionIdentifiers) && c.CampaignConversionIsFunnelStep);

                if (mainConversion != null)
                {
                    hit["CampaignConversionID"] = mainConversion.CampaignConversionID;
                    hitsWithIdentifiers.Rows.Add(hit.ItemArray);
                }
                if (stepConversion != null)
                {
                    hit["CampaignConversionID"] = stepConversion.CampaignConversionID;
                    hitsWithIdentifiers.Rows.Add(hit.ItemArray);
                }
            }

            if (hitsWithIdentifiers.Columns.Contains(activityParameterColumn))
            {
                hitsWithIdentifiers.Columns.Remove(activityParameterColumn);
            }

            hitsWithIdentifiers.AcceptChanges();

            return hitsWithIdentifiers;
        }


        /// <summary>
        /// Checks if a conversion has the same item specific identifier as calculated hit row.
        /// </summary>
        /// <param name="conversion"></param>
        /// <param name="hit"></param>
        /// <param name="activityParameterColumn"></param>
        /// <param name="conversionIdentifiers"></param>
        private bool IsRelatedConversion(CampaignConversionInfo conversion, DataRow hit, string activityParameterColumn, IDictionary<int, IEnumerable<long>> conversionIdentifiers)
        {
            if (activityParameterColumn == "ActivityURLHash")
            {
                long hash = ValidationHelper.GetLong(DataHelper.GetDataRowValue(hit, activityParameterColumn), 0);
                return GetCampaignConversionHash(conversion) == hash;
            }

            var identifier = DataHelper.GetIntValue(hit, activityParameterColumn);

            if (conversionIdentifiers.ContainsKey(conversion.CampaignConversionID))
            {
                return conversionIdentifiers[conversion.CampaignConversionID].Contains(identifier);
            }


            return conversion.CampaignConversionItemID == identifier;
        }




        /// <summary>
        /// Finds ID of the last activity which will be included in the report.
        /// </summary>
        private int GetLastActivityID()
        {
            // Rely on ActivityID rising over time
            var lastActivity = ActivityInfoProvider.GetActivities()
                                                   .TopN(1)
                                                   .Columns("ActivityID")
                                                   .OrderBy(OrderDirection.Descending, "ActivityID")
                                                   .WhereLessOrEquals("ActivityCreated", mTo)
                                                   .FirstOrDefault();
            if (lastActivity != null)
            {
                return lastActivity.ActivityID;
            }

            return int.MaxValue;
        }


        private string GetActivityParameterColumn(string activityType, bool isForAll, bool campaignIsContentOnly = false)
        {
            if (isForAll)
            {
                return "ActivityType";
            }

            switch (activityType)
            {
                case PredefinedActivityType.PAGE_VISIT:
                    return campaignIsContentOnly ? "ActivityURLHash" : "ActivityNodeID";
                case PredefinedActivityType.EVENT_BOOKING:
                    return "ActivityItemDetailID";
                case PredefinedActivityType.EXTERNAL_SEARCH:
                case PredefinedActivityType.INTERNAL_SEARCH:
                case PredefinedActivityType.REGISTRATION:
                case PredefinedActivityType.PURCHASE:
                    return "ActivityType";

                default:
                    return "ActivityItemID";
            }
        }


        /// <summary>
        /// Returns query for campaigns for which report will be calculated.
        /// </summary>
        /// <param name="siteID">ID of the site to return campaign for. Use <c>0</c> for all sites.</param>
        private ObjectQuery<CampaignInfo> GetCampaigns(int siteID)
        {
            // Calculate statistics only for campaigns which are currently running 
            // or campaigns which have stopped recently but there may be still data to process.
            var campaigns = CampaignInfoProvider.GetCampaigns()
                                                .Where(new WhereCondition()
                                                    .WhereLessOrEquals("CampaignOpenFrom", mTo)
                                                    .And()
                                                    .Where(new WhereCondition()
                                                        .WhereGreaterOrEquals("CampaignOpenTo", mTo)
                                                        .Or()
                                                        .WhereNull("CampaignOpenTo")
                                                        .Or()
                                                        .WhereLessThan("CampaignCalculatedTo", "CampaignOpenTo".AsColumn())));

            if (siteID > 0)
            {
                campaigns = campaigns.OnSite(siteID);
            }

            return campaigns;
        }


        /// <summary>
        /// Represents object for SQL data table type Type_Analytics_CampaignConversionHitsTable.
        /// </summary>
        private struct CampaignConversionHits
        {
            public int Hits
            {
                get;
                set;
            }
            

            public int CampaignConversionID
            {
                get;
                set;
            }


            public string ActivityUtmSource
            {
                get;
                set;
            }


            public string ActivityUtmContent
            {
                get;
                set;
            }
        }
    }
}
