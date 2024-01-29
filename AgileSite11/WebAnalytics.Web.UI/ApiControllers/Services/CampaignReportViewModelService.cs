using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Activities;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.PortalEngine.Internal;
using CMS.WebAnalytics.Internal;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Provides view model for campaign report.
    /// </summary>
    internal class CampaignReportViewModelService : ICampaignReportViewModelService
    {
        private readonly ICampaignStatisticsService statisticsService;

        public CampaignReportViewModelService(ICampaignStatisticsService statisticsService)
        {
            this.statisticsService = statisticsService;
        }


        /// <summary>
        /// Returns view model for specified campaign.
        /// </summary>
        /// <param name="campaign">Campaign to create view model for.</param>
        public CampaignReportViewModel GetViewModel(CampaignInfo campaign)
        {
            if (campaign == null)
            {
                throw new ArgumentNullException("campaign");
            }

            var conversionReports = GetConversionReports(campaign);

            var viewModel = new CampaignReportViewModel()
            {
                CampaignName = campaign.CampaignDisplayName,
                CampaignDescription = campaign.CampaignDescription,
                CampaignStatus = campaign.GetCampaignStatus(DateTime.Now).ToString(),
                CampaignLaunchDate = GetDate(campaign.CampaignOpenFrom),
                CampaignFinishDate = GetDate(campaign.CampaignOpenTo),
                CampaignReportUpdated = GetDate(campaign.CampaignCalculatedTo),

                CampaignConversions = conversionReports,
                CampaignSourceDetails = GetSourceDetails(campaign, conversionReports),
                CampaignObjective = GetCampaignObjective(campaign)
            };

            return viewModel;
        }


        /// <summary>
        /// Creates collection with distinct conversion sources with links to their details.
        /// </summary>
        /// <param name="campaign">Campaign to get sources for.</param>
        /// <param name="conversionReports">List of individual conversion reports.</param>
        private IEnumerable<CampaignReportSourceDetailsViewModel> GetSourceDetails(CampaignInfo campaign, IEnumerable<CampaignReportConversionViewModel> conversionReports)
        {
            // Collect distinct source names from conversion reports.
            var sources = new HashSet<UtmDisplayParameters>();
            foreach (var report in conversionReports)
            {
                foreach (var source in report.ConversionSources)
                {
                    sources.Add(new UtmDisplayParameters
                    {
                        UtmSource = source.SourceName,
                        UtmContent = source.ContentName
                    });
                }
            }

            // Find details links for source names
            return GetSourceDetailLinks(campaign, sources);
        }


        /// <summary>
        /// Returns campaign objective information (its name, target value and actual value) for given campaign.  
        /// </summary>
        /// <param name="campaign">Campaign to get objective for.</param>
        private object GetCampaignObjective(CampaignInfo campaign)
        {
            var objective = statisticsService.GetObjectiveStatistics(campaign.CampaignID);

            if (objective == null)
            {
                return null;
            }

            return new
            {
                objective.Target,
                objective.Actual,
                objective.Name
            };
        }


        /// <summary>
        /// Creates report view models for individual conversions of given campaign.
        /// </summary>
        /// <param name="campaign">Campaign to get conversion reports for.</param>
        private List<CampaignReportConversionViewModel> GetConversionReports(CampaignInfo campaign)
        {
            var campaignID = campaign.CampaignID;

            // Get all conversion from given campaign
            var conversions = CampaignConversionInfoProvider.GetCampaignConversions()
                .WhereEquals("CampaignConversionCampaignID", campaign.CampaignID)
                .OrderBy("CampaignConversionOrder")
                .ToList();

            // Get all hits for all campaign conversions and group them by conversion
            var campaignHitService = Service.Resolve<ICampaignConversionHitsService>();
            var hitsByConversionId = campaignHitService.GetCampaignHits(campaignID);

            var conversionReports = new List<CampaignReportConversionViewModel>();
            foreach (var conversion in conversions)
            {
                List<CampaignConversionHitsInfo> conversionHits;
                hitsByConversionId.TryGetValue(conversion.CampaignConversionID, out conversionHits);

                // Create report for the conversion
                var conversionReport = GetConversionReport(conversion, conversionHits ?? Enumerable.Empty<CampaignConversionHitsInfo>());
                conversionReports.Add(conversionReport);
            }

            return conversionReports;
        }


        /// <summary>
        /// Creates view model for given conversion using supplied hits.
        /// </summary>
        /// <param name="conversion">Conversion to create report view model for.</param>
        /// <param name="hits">Hits corresponding to the conversion</param>
        private CampaignReportConversionViewModel GetConversionReport(CampaignConversionInfo conversion, IEnumerable<CampaignConversionHitsInfo> hits)
        {
            var sources = GetSourceReports(hits);

            var conversionReport = new CampaignReportConversionViewModel
            {
                ConversionTypeName = GetConversionTypeName(conversion),
                ConversionName = conversion.CampaignConversionDisplayName,
                ConversionSources = sources,
                ConversionHitsCount = sources.Sum(s => s.SourceHitsCount),
                IsFunnelStep = conversion.CampaignConversionIsFunnelStep,
                CampaignConversionID = conversion.CampaignConversionID
            };

            return conversionReport;
        }


        /// <summary>
        /// Creates source report view models for given hits.
        /// </summary>
        /// <param name="hits">Conversion hits to create view model for.</param>
        private List<CampaignReportSourceViewModel> GetSourceReports(IEnumerable<CampaignConversionHitsInfo> hits)
        {
            return hits.Select(h => new CampaignReportSourceViewModel()
            {
                SourceName = h.CampaignConversionHitsSourceName,
                ContentName = h.CampaignConversionHitsContentName,
                SourceHitsCount = h.CampaignConversionHitsCount,
            }).ToList();
        }


        /// <summary>
        /// Creates collection with distinct conversion sources with links to newsletter issue details.
        /// </summary>
        /// <param name="campaign">Campaign to get sources for.</param>
        /// <param name="sourceNames">Source names to get links for.</param>
        private IEnumerable<CampaignReportSourceDetailsViewModel> GetSourceDetailLinks(CampaignInfo campaign, ICollection<UtmDisplayParameters> sourceNames)
        {
            if (ModuleEntryManager.IsModuleLoaded(ModuleName.NEWSLETTER))
            {
                // Create dictionary mapping source names to links leading to email issue details
                // The key is taken from email issue and its casing does not necessarily match with casing of campaign source
                var linksBySource = GetLinksForSources(campaign, sourceNames);

                // Return all sourceNames, add detail links if available
                return sourceNames.Select(name => new CampaignReportSourceDetailsViewModel
                {
                    SourceName = name.UtmSource,
                    ContentName = name.UtmContent,
                    EmailLinkDetail = linksBySource.ContainsKey(name.UtmSource) ? linksBySource[name.UtmSource] : null
                });
            }

            return new List<CampaignReportSourceDetailsViewModel>();
        }

        private IDictionary<string, EmailLinkDetailViewModel> GetLinksForSources(CampaignInfo campaign, IEnumerable<UtmDisplayParameters> sourceNames)
        {
            return new ObjectQuery(PredefinedObjectType.NEWSLETTERISSUE)
                    .Columns("IssueID", "IssueSubject", "IssueUTMSource", "IssueNewsletterID")
                    .WhereEquals("IssueUTMCampaign", campaign.CampaignUTMCode)
                    .WhereIn("IssueUTMSource", sourceNames.Select(x => x.UtmSource).ToList())
                    .ToList()
                    .GroupBy(issue => issue.GetStringValue("IssueUTMSource", String.Empty), issue => issue, (source, issues) => new
                    {
                        Name = source,
                        Link = GetEmailReportLink(issues)
                    }, StringComparer.CurrentCultureIgnoreCase)
                    .ToDictionary(detail => detail.Name, detail => detail.Link, StringComparer.CurrentCultureIgnoreCase);
        }


        /// <summary>
        /// Gets email report link.
        /// </summary>
        /// <param name="issues">Newsletter issues</param>
        private EmailLinkDetailViewModel GetEmailReportLink(IEnumerable<BaseInfo> issues)
        {
            var issue = issues.FirstOrDefault();
            if (issue == null)
            {
                return null;
            }

            var uiLinkProvider = Service.Resolve<IUILinkProvider>();
            var url = URLHelper.GetAbsoluteUrl(uiLinkProvider.GetSingleObjectLink("CMS.Newsletter", "EditIssueProperties", new ObjectDetailLinkParameters
            {
                ObjectIdentifier = issue.Generalized.ObjectID,
                AllowNavigationToListing = true,
                TabName = "Newsletter.Issue.Reports.Overview",
                ParentObjectIdentifier = issue.Generalized.ObjectParentID
            }));

            return new EmailLinkDetailViewModel
            {
                Text = issue.GetValue("IssueSubject").ToString(),
                Url = url
            };
        }


        /// <summary>
        /// Gets name of the conversion type (activity type on which the conversion is based).
        /// </summary>
        /// <param name="conversion">Conversion to get type for.</param>
        private string GetConversionTypeName(CampaignConversionInfo conversion)
        {
            var activityType = ActivityTypeInfoProvider.GetActivityTypeInfo(conversion.CampaignConversionActivityType);
            if (activityType != null)
            {
                return activityType.ActivityTypeDisplayName;
            }

            return string.Empty;
        }


        private DateTime? GetDate(DateTime dateTime)
        {
            if (dateTime != DateTimeHelper.ZERO_TIME)
            {
                return dateTime;
            }

            return null;
        }


        private struct UtmDisplayParameters
        {
            public string UtmSource
            {
                get;
                set;
            }

            public string UtmContent
            {
                get;
                set;
            }
        }
    }
}
