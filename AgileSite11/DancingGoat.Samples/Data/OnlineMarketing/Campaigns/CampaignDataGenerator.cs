using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Activities;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Contains methods for generating sample data for the Campaign module.
    /// </summary>
    internal class CampaignDataGenerator
    {
        private readonly SiteInfo mSite;
        private readonly string mContactFirstNamePrefix;
        private readonly CampaignConversionHitsProcessor mHitsProcessor;
        private readonly CampaignVisitorsProcessor mVisitorsProcessor;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="site">Site the campaign data will be generated for</param>
        /// <param name="contactFirstNamePrefix">First name prefix of contacts generated for sample campaigns.</param>
        public CampaignDataGenerator(SiteInfo site, string contactFirstNamePrefix)
        {
            mSite = site;
            mContactFirstNamePrefix = contactFirstNamePrefix;
            mHitsProcessor = new CampaignConversionHitsProcessor();
            mVisitorsProcessor = new CampaignVisitorsProcessor();
        }               

        /// <summary>
        /// Code name of the draft 'Coffee club membership' campaign.
        /// </summary>
        private const string CAMPAIGN_COFFEE_CLUB_MEMBERSHIP_DRAFT = "CoffeeClubMembership";


        /// <summary>
        /// Code name of the scheduled 'Coffee club membership' campaign.
        /// </summary>
        private const string CAMPAIGN_COFFEE_CLUB_MEMBERSHIP_SCHEDULED = "CoffeeClubMembershipTest";


        /// <summary>
        /// Code name of the running 'Cafe sample promotion' campaign.
        /// </summary>
        private const string CAMPAIGN_CAFE_SAMPLE_PROMOTION_RUNNING = "CafeSamplePromotion";


        /// <summary>
        /// Code name of the finished 'Cafe sample promotion' campaign.
        /// </summary>
        private const string CAMPAIGN_CAFE_SAMPLE_PROMOTION_FINISHED = "CafeSamplePromotionTest";


        private const string CONVERSION_PAGEVISIT_COLOMBIA = "Colombia";
        private const string CONVERSION_PAGEVISIT_AMERICAS_COFFEE_POSTER = "America's coffee poster";
        private const string CONVERSION_FORMSUBMISSION_TRY_FREE_SAMPLE = "Try a free sample";
        private const string CONVERSION_USERREGISTRATION = "User registration";

        private const string PAGE_PATH_COLOMBIA = "/Campaign-assets/Cafe-promotion/Colombia";
        private const string PAGE_PATH_THANK_YOU = "/Campaign-assets/Cafe-promotion/Thank-you";
        private const string PAGE_PATH_AMERICAS_COFFEE_POSTER = "/Campaign-assets/Cafe-promotion/America-s-coffee-poster";
        private const string PAGE_PATH_COFFEE_CLUB_MEMBERSHIP = "/Store/Coffee-Club-Membership";


        private readonly Guid NEWSLETTER_COLOMBIA_COFFEE_SAMPLE_PROMOTION_ISSUE_GUID =
            Guid.Parse("C818B404-0488-4558-B438-08167DE75824");


        private readonly Guid NEWSLETTER_COFFEE_CLUB_MEMBERSHIP_ISSUE_GUID =
            Guid.Parse("5045B325-F360-4536-8692-9454FA91EBAA");

        private const int CAMPAIGN_CAFE_SAMPLE_PROMOTION_FINISHED_CONTACTS_COUNT = 100;
        /* When 0 is used in TopN method, all records are returned. */
        private const int CAMPAIGN_CAFE_SAMPLE_PROMOTION_RUNNING_CONTACTS_COUNT = 0;


        private readonly Dictionary<string, IEnumerable<ActivityDataParameters>> CAMPAIGN_CAFE_SAMPLE_PROMOTION_FINISHED_HITS = new Dictionary
            <string, IEnumerable<ActivityDataParameters>>
        {
            {
                CONVERSION_PAGEVISIT_COLOMBIA, new List<ActivityDataParameters>
                {
                    GetActivityDataParameters("colombia_coffee_sample_promotion", String.Empty, 7),
                    GetActivityDataParameters("colombia_coffee_sample_promotion", "colombia_mail_1", 13),
                    GetActivityDataParameters("colombia_coffee_sample_promotion", "colombia_mail_2", 16),
                    GetActivityDataParameters("facebook", "fb_colombia", 30),
                    GetActivityDataParameters("twitter", String.Empty, 4),
                    GetActivityDataParameters("twitter", "twitter_post_1", 42),
                    GetActivityDataParameters("twitter", "twitter_post_2", 21)
                }
            },
            {
                CONVERSION_FORMSUBMISSION_TRY_FREE_SAMPLE, new List<ActivityDataParameters>
                {
                    GetActivityDataParameters("colombia_coffee_sample_promotion", String.Empty, 5),
                    GetActivityDataParameters("colombia_coffee_sample_promotion", "colombia_mail_1", 11),
                    GetActivityDataParameters("colombia_coffee_sample_promotion", "colombia_mail_2", 15),
                    GetActivityDataParameters("facebook", String.Empty, 4),
                    GetActivityDataParameters("facebook", "fb_colombia", 21),
                    GetActivityDataParameters("twitter", "twitter_post_1", 9),
                    GetActivityDataParameters("twitter", "twitter_post_2", 7)
                }
            },
            {
                CONVERSION_PAGEVISIT_AMERICAS_COFFEE_POSTER, new List<ActivityDataParameters>
                {
                    GetActivityDataParameters("colombia_coffee_sample_promotion", "colombia_mail_1", 5),
                    GetActivityDataParameters("colombia_coffee_sample_promotion", "colombia_mail_2", 3),
                    GetActivityDataParameters("facebook", "fb_colombia", 11),
                    GetActivityDataParameters("twitter", "twitter_post_1", 5),
                    GetActivityDataParameters("twitter", "twitter_post_2", 8),
                    GetActivityDataParameters("linkedin", String.Empty, 6)
                }
            },
            {
                CONVERSION_USERREGISTRATION, new List<ActivityDataParameters>
                {
                    GetActivityDataParameters("colombia_coffee_sample_promotion", String.Empty, 4),
                    GetActivityDataParameters("colombia_coffee_sample_promotion", "colombia_mail_1", 29),
                    GetActivityDataParameters("colombia_coffee_sample_promotion", "colombia_mail_2", 17),
                }
            }
        };

        
        private readonly Dictionary<string, IEnumerable<ActivityDataParameters>> CAMPAIGN_CAFE_SAMPLE_PROMOTION_RUNNING_HITS = new Dictionary
            <string, IEnumerable<ActivityDataParameters>>
        {
            {
                CONVERSION_PAGEVISIT_COLOMBIA, new List<ActivityDataParameters>
                {
                    GetActivityDataParameters("linkedin", "linkedin_colombia", 1429),
                    GetActivityDataParameters("facebook", String.Empty, 66),
                    GetActivityDataParameters("facebook", "fb_colombia_1", 1246),
                    GetActivityDataParameters("facebook", "fb_colombia_2", 1152),
                    GetActivityDataParameters("twitter", "twitter_colombia", 310)
                }
            },
            {
                CONVERSION_FORMSUBMISSION_TRY_FREE_SAMPLE, new List<ActivityDataParameters>
                {
                    GetActivityDataParameters("linkedin", "linkedin_colombia", 175),
                    GetActivityDataParameters("facebook", String.Empty, 77),
                    GetActivityDataParameters("facebook", "fb_colombia_1", 248),
                    GetActivityDataParameters("facebook", "fb_colombia_2", 173),
                    GetActivityDataParameters("twitter", "twitter_colombia", 58)
                }
            },
            {
                CONVERSION_PAGEVISIT_AMERICAS_COFFEE_POSTER, new List<ActivityDataParameters>
                {
                    GetActivityDataParameters("linkedin", String.Empty, 42),
                    GetActivityDataParameters("linkedin", "linkedin_colombia", 45),
                    GetActivityDataParameters("facebook", "fb_colombia_1", 110),
                    GetActivityDataParameters("facebook", "fb_colombia_2", 96),
                    GetActivityDataParameters("twitter", "twitter_colombia", 10)
                }
            },
            {
                CONVERSION_USERREGISTRATION, new List<ActivityDataParameters>
                {
                    GetActivityDataParameters("linkedin", "linkedin_colombia", 50),
                    GetActivityDataParameters("facebook", "fb_colombia_1", 43),
                    GetActivityDataParameters("facebook", "fb_colombia_2", 42),
                    GetActivityDataParameters("twitter", String.Empty, 3),
                    GetActivityDataParameters("twitter", "twitter_colombia", 12)
                }
            }
        };


        private static ActivityDataParameters GetActivityDataParameters(string utmSource, string utmContent, int count)
        {
            return new ActivityDataParameters
            {
                UtmSource = utmSource,
                UtmContent = utmContent,
                Count = count
            };
        }


        /// <summary>
        /// Performs campaigns sample data generating.
        /// </summary>
        public void Generate()
        {
            CampaignInfoProvider.GetCampaigns().ToList().ForEach(CampaignInfoProvider.DeleteCampaignInfo);

            /* Generate campaigns. */
            GenerateCoffeeClubMembershipCampaign();
            GenerateCafePromotionSampleCampaign();

            /* Generate campaign objectives */
            GenerateCampaignObjective(CAMPAIGN_CAFE_SAMPLE_PROMOTION_RUNNING, CONVERSION_FORMSUBMISSION_TRY_FREE_SAMPLE, 600);
            GenerateCampaignObjective(CAMPAIGN_CAFE_SAMPLE_PROMOTION_FINISHED, CONVERSION_FORMSUBMISSION_TRY_FREE_SAMPLE, 50);

            /* Generate activities */
            GenerateActivities(CAMPAIGN_CAFE_SAMPLE_PROMOTION_RUNNING, CAMPAIGN_CAFE_SAMPLE_PROMOTION_RUNNING_HITS, CAMPAIGN_CAFE_SAMPLE_PROMOTION_RUNNING_CONTACTS_COUNT);
            GenerateActivities(CAMPAIGN_CAFE_SAMPLE_PROMOTION_FINISHED, CAMPAIGN_CAFE_SAMPLE_PROMOTION_FINISHED_HITS, CAMPAIGN_CAFE_SAMPLE_PROMOTION_FINISHED_CONTACTS_COUNT);
        }


        private void GenerateCampaignObjective(string campaignName, string conversionName, int objectiveValue)
        {
            var campaign = CampaignInfoProvider.GetCampaignInfo(campaignName, mSite.SiteName);
            if (campaign == null)
            {
                return;
            }

            var conversion = CampaignConversionInfoProvider.GetCampaignConversions()
                                                           .WhereEquals("CampaignConversionDisplayName", conversionName)
                                                           .WhereEquals("CampaignConversionCampaignID", campaign.CampaignID)
                                                           .FirstOrDefault();
            if (conversion == null)
            {
                return;
            }

            var objective = new CampaignObjectiveInfo
            {
                CampaignObjectiveCampaignID = campaign.CampaignID,
                CampaignObjectiveCampaignConversionID = conversion.CampaignConversionID,
                CampaignObjectiveValue = objectiveValue
            };

            CampaignObjectiveInfoProvider.SetCampaignObjectiveInfo(objective);
        }


        /// <summary>
        /// Generates campaign.
        /// </summary>
        /// <param name="campaignData">Campaign data for generating.</param>
        private void GenerateCampaign(CampaignData campaignData)
        {
            var siteName = mSite.SiteName;
            var campaign = CampaignInfoProvider.GetCampaignInfo(campaignData.CampaignName, siteName);
            if (campaign != null)
            {
                return;
            }

            campaign = new CampaignInfo
            {
                CampaignName = campaignData.CampaignName,
                CampaignDisplayName = campaignData.CampaignDisplayName,
                CampaignDescription = campaignData.CampaignDescription,
                CampaignOpenFrom = campaignData.CampaignOpenFrom,
                CampaignOpenTo = campaignData.CampaignOpenTo,
                CampaignSiteID = mSite.SiteID,
                CampaignUTMCode = campaignData.CampaignUTMCode
            };

            CampaignInfoProvider.SetCampaignInfo(campaign);

            /* Add email to campaign promotion section  */
            CampaignDataGeneratorHelpers.AddNewsletterAsset(campaign, campaignData.CampaignEmailPromotion);
            var issue = ProviderHelper.GetInfoByGuid(PredefinedObjectType.NEWSLETTERISSUE, campaignData.CampaignEmailPromotion, campaign.CampaignSiteID);

            if (issue != null)
            {
                issue.SetValue("IssueStatus", 5);
                issue.SetValue("IssueMailoutTime", campaignData.CampaignOpenFrom);

                issue.Update();
            }

            /* Add page assets to campaign content inventory section */
            foreach (var itemPath in campaignData.CampaignContentInventory)
            {
                CampaignDataGeneratorHelpers.AddPageAsset(campaign.CampaignID, itemPath);
            }

            /* Add conversions to campaign report setup section */
            foreach (var conversion in campaignData.CampaignReportSetup)
            {
                CampaignDataGeneratorHelpers.CreateConversion(campaign.CampaignID, conversion);
            }
        }


        private void GenerateCoffeeClubMembershipCampaign()
        {
            var coffeeClubMembershipCampaign = new CampaignData
            {
                CampaignName = CAMPAIGN_COFFEE_CLUB_MEMBERSHIP_DRAFT,
                CampaignDisplayName = "Coffee club membership",
                CampaignDescription = "The goal of this campaign is to promote the Coffee Club, a new service that the Dancing Goat company provides for it's coffee geek customers.",
                CampaignUTMCode = "coffee_club_membership_draft",
                CampaignOpenFrom = DateTime.MinValue,
                CampaignOpenTo = DateTime.MinValue,
                CampaignEmailPromotion = NEWSLETTER_COFFEE_CLUB_MEMBERSHIP_ISSUE_GUID,
                CampaignContentInventory = new List<string>
                {
                      PAGE_PATH_COFFEE_CLUB_MEMBERSHIP
                },
                CampaignReportSetup = PrepareCoffeeClubMembershipConversions()
            };

            /* Generate draft 'Cafe sample promotion' campaign. */
            GenerateCampaign(coffeeClubMembershipCampaign);

            /* Generate scheduled 'Cafe sample promotion' campaign. */
            coffeeClubMembershipCampaign.CampaignName = CAMPAIGN_COFFEE_CLUB_MEMBERSHIP_SCHEDULED;
            coffeeClubMembershipCampaign.CampaignDisplayName = "Coffee club membership test";
            coffeeClubMembershipCampaign.CampaignOpenFrom = DateTime.Now.AddDays(6);
            coffeeClubMembershipCampaign.CampaignUTMCode = "coffee_club_membership_scheduled";

            GenerateCampaign(coffeeClubMembershipCampaign);
        }


        private void GenerateCafePromotionSampleCampaign()
        {
            var campaignCafePromotion = new CampaignData
            {
                CampaignName = CAMPAIGN_CAFE_SAMPLE_PROMOTION_RUNNING,
                CampaignDisplayName = "Cafe sample promotion",
                CampaignDescription = "The goal of this campaign is to increase the number of visitors in our cafes. We want to achieve that by sending out free coffee sample coupons that customers can redeem at the cafes. At the end of the process a poster download is offered to see whether people would be interested in such freebies.",
                CampaignUTMCode = "cafe_sample_promotion_running",
                CampaignOpenFrom = DateTime.Now.AddDays(-14),
                CampaignOpenTo = DateTime.MinValue,
                CampaignEmailPromotion = NEWSLETTER_COLOMBIA_COFFEE_SAMPLE_PROMOTION_ISSUE_GUID,
                CampaignContentInventory = new List<string>
                {
                      PAGE_PATH_COLOMBIA, PAGE_PATH_THANK_YOU, PAGE_PATH_AMERICAS_COFFEE_POSTER
                },
                CampaignReportSetup = PrepareCafeSamplePromotionConversions()
            };

            /* Generate running 'Cafe sample promotion' campaign. */
            GenerateCampaign(campaignCafePromotion);

            /* Generate finished 'Cafe sample promotion' campaign. */
            campaignCafePromotion.CampaignName = CAMPAIGN_CAFE_SAMPLE_PROMOTION_FINISHED;
            campaignCafePromotion.CampaignDisplayName = "Cafe sample promotion test";
            campaignCafePromotion.CampaignOpenTo = campaignCafePromotion.CampaignOpenFrom.AddDays(6);
            campaignCafePromotion.CampaignUTMCode = "cafe_sample_promotion_finished";

            GenerateCampaign(campaignCafePromotion);
        }


        private void GenerateActivities(string campaignName, Dictionary<string, IEnumerable<ActivityDataParameters>> conversionHits, int contactsCount)
        {
            var siteName = mSite.SiteName;
            var campaignCafePromotion = CampaignInfoProvider.GetCampaignInfo(campaignName, siteName);

            var pageVisitPoster = CampaignDataGeneratorHelpers.GetDocument(PAGE_PATH_AMERICAS_COFFEE_POSTER);
            var pageVisitColombia = CampaignDataGeneratorHelpers.GetDocument(PAGE_PATH_COLOMBIA);
            var formFreeSample = ProviderHelper.GetInfoById(PredefinedObjectType.BIZFORM, mSite.SiteID);

            /* Generate activities for campaign */
            CampaignDataGeneratorHelpers.DeleteOldActivities(campaignCafePromotion.CampaignUTMCode);
            var campaignContactsIDs = new ContactsIDData(mContactFirstNamePrefix, contactsCount);
            
            CampaignDataGeneratorHelpers.GenerateActivities(conversionHits[CONVERSION_PAGEVISIT_COLOMBIA], campaignCafePromotion, PredefinedActivityType.PAGE_VISIT, campaignContactsIDs, pageVisitColombia.NodeID);
            CampaignDataGeneratorHelpers.GenerateActivities(conversionHits[CONVERSION_PAGEVISIT_AMERICAS_COFFEE_POSTER], campaignCafePromotion, PredefinedActivityType.PAGE_VISIT, campaignContactsIDs, pageVisitPoster.NodeID);
            CampaignDataGeneratorHelpers.GenerateActivities(conversionHits[CONVERSION_USERREGISTRATION], campaignCafePromotion, PredefinedActivityType.REGISTRATION, campaignContactsIDs);
            CampaignDataGeneratorHelpers.GenerateActivities(conversionHits[CONVERSION_FORMSUBMISSION_TRY_FREE_SAMPLE], campaignCafePromotion, PredefinedActivityType.BIZFORM_SUBMIT, campaignContactsIDs, formFreeSample.Generalized.ObjectID);

            mHitsProcessor.CalculateReport(campaignCafePromotion);
            mVisitorsProcessor.CalculateVisitors(campaignCafePromotion);
        }


        private IEnumerable<CampaignConversionData> PrepareCafeSamplePromotionConversions()
        {
            var pageVisitPoster = CampaignDataGeneratorHelpers.GetDocument(PAGE_PATH_AMERICAS_COFFEE_POSTER);
            var pageVisitColombia = CampaignDataGeneratorHelpers.GetDocument(PAGE_PATH_COLOMBIA);
            var formFreeSample = ProviderHelper.GetInfoById(PredefinedObjectType.BIZFORM, mSite.SiteID);

            return new List<CampaignConversionData>
            {
                /*  Campaign conversions. */
                new CampaignConversionData
                {
                    ConversionName = "try_free_sample",
                    ConversionDisplayName = CONVERSION_FORMSUBMISSION_TRY_FREE_SAMPLE,
                    ConversionActivityType = PredefinedActivityType.BIZFORM_SUBMIT,
                    ConversionItemID = formFreeSample.Generalized.ObjectID,
                    ConversionOrder = 1,
                    ConversionIsFunnelStep = false
                },
                new CampaignConversionData
                {
                    ConversionName = "america_coffee_poster",
                    ConversionDisplayName = pageVisitPoster.DocumentName,
                    ConversionActivityType = PredefinedActivityType.PAGE_VISIT,
                    ConversionItemID = pageVisitPoster.NodeID,
                    ConversionOrder = 2,
                    ConversionIsFunnelStep = false
                },
                new CampaignConversionData
                {
                    ConversionName = "userregistration",
                    ConversionDisplayName = "",
                    ConversionActivityType = PredefinedActivityType.REGISTRATION,
                    ConversionItemID = null,
                    ConversionOrder = 3,
                    ConversionIsFunnelStep = false
                },

                /* Campaign journey steps. */
                new CampaignConversionData
                {
                    ConversionName = "colombia",
                    ConversionDisplayName = pageVisitColombia.DocumentName,
                    ConversionActivityType = PredefinedActivityType.PAGE_VISIT,
                    ConversionItemID = pageVisitColombia.NodeID,
                    ConversionOrder = 1,
                    ConversionIsFunnelStep = true
                },
                new CampaignConversionData
                {
                    ConversionName = "try_free_sample_1",
                    ConversionDisplayName = CONVERSION_FORMSUBMISSION_TRY_FREE_SAMPLE,
                    ConversionActivityType = PredefinedActivityType.BIZFORM_SUBMIT,
                    ConversionItemID = formFreeSample.Generalized.ObjectID,
                    ConversionOrder = 2,
                    ConversionIsFunnelStep = true
                },
                new CampaignConversionData
                {
                    ConversionName = "america_coffee_poster_1",
                    ConversionDisplayName = pageVisitPoster.DocumentName,
                    ConversionActivityType = PredefinedActivityType.PAGE_VISIT,
                    ConversionItemID = pageVisitPoster.NodeID,
                    ConversionOrder = 3,
                    ConversionIsFunnelStep = true
                }
            };
        }


        private IEnumerable<CampaignConversionData> PrepareCoffeeClubMembershipConversions()
        {
            var pageCoffeeClubMembership = CampaignDataGeneratorHelpers.GetDocument(PAGE_PATH_COFFEE_CLUB_MEMBERSHIP);


            return new List<CampaignConversionData>
            {
                /*  Campaign conversions. */
                new CampaignConversionData
                {
                    ConversionName = "coffee_club_membership",
                    ConversionDisplayName = pageCoffeeClubMembership.DocumentName,
                    ConversionActivityType = PredefinedActivityType.PURCHASEDPRODUCT,
                    ConversionItemID = pageCoffeeClubMembership.NodeSKUID,
                    ConversionOrder = 1,
                    ConversionIsFunnelStep = false
                },

                /* Campaign journey steps. */
                new CampaignConversionData
                {
                    ConversionName = "coffee_club_membership_1",
                    ConversionDisplayName = pageCoffeeClubMembership.DocumentName,
                    ConversionActivityType = PredefinedActivityType.PAGE_VISIT,
                    ConversionItemID = pageCoffeeClubMembership.NodeID,
                    ConversionOrder = 1,
                    ConversionIsFunnelStep = true
                },
                new CampaignConversionData
                {
                    ConversionName = "coffee_club_membership_2",
                    ConversionDisplayName = pageCoffeeClubMembership.DocumentName,
                    ConversionActivityType = PredefinedActivityType.PRODUCT_ADDED_TO_SHOPPINGCART,
                    ConversionItemID = pageCoffeeClubMembership.NodeSKUID,
                    ConversionOrder = 2,
                    ConversionIsFunnelStep = true
                },
                new CampaignConversionData
                {
                    ConversionName = "coffee_club_membership_3",
                    ConversionDisplayName = pageCoffeeClubMembership.DocumentName,
                    ConversionActivityType = PredefinedActivityType.PURCHASEDPRODUCT,
                    ConversionItemID = pageCoffeeClubMembership.NodeSKUID,
                    ConversionOrder = 3,
                    ConversionIsFunnelStep = true
                }
            };
        }

    }
}