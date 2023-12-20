using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Contains methods for generating sample data for the Web analytics module.
    /// </summary>
    internal class WebAnalyticsDataGenerator
    {
        private readonly SiteInfo mSite;

        private const int TOTAL_MONTHS_VISITS = 20000;
        private const int VISITS_VARIANCE = 10;
        private const int VISITS_GRADIENT = 6;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="site">Site the analytics will be generated for</param>
        public WebAnalyticsDataGenerator(SiteInfo site)
        {
            mSite = site;
        }


        /// <summary>
        /// Performs web analytics sample data generation.
        /// </summary>
        public void Generate()
        {
            var where = new WhereCondition().WhereEquals("StatisticsSiteID", mSite.SiteID).WhereIn("StatisticsCode", StatisticCodeNames);
            StatisticsInfoProvider.RemoveAnalyticsData(DateTimeHelper.ZERO_TIME, DateTimeHelper.ZERO_TIME, mSite.SiteID, where.ToString(true));

            var generators = RegisterGenerators();

            DateTime to = DateTime.Now.AddDays(1);
            DateTime from = to.AddDays(-35);

            var random = new Random();

            RegisteredUsers(random);

            int totalDays = (to - from).Days;

            for (DateTime startDate = from; startDate < to; startDate = startDate.AddDays(1))
            {
                int daysFromStart = (startDate - from).Days;
                int visitsPerDay =
                    (int)
                        ((double)TOTAL_MONTHS_VISITS / totalDays + Logit(random.NextDouble()) * VISITS_VARIANCE +
                         (daysFromStart - totalDays / 2) * VISITS_GRADIENT);

                // We can assume only one fifth of the visitor come with Spanish culture
                // 0.2-0.3 of visitors are randomly chosen to be Spanish.
                int spanishVisitors = (int)((0.2 + random.NextDouble() * 0.1) * visitsPerDay);
                int englishVisitors = visitsPerDay - spanishVisitors;

                var visitors = new Dictionary<string, int> { { "en-US", englishVisitors }, { "es-ES", spanishVisitors } };
                generators(startDate, visitors, random);

                WebAnalyticsEvents.GenerateStatistics.StartEvent(startDate, visitors);
            }
        }


        /// <summary>
        /// Registers all methods generating the sample data to the output delegate.
        /// </summary>
        /// <returns>Delegates list containing all generators</returns>
        private Action<DateTime, Dictionary<string, int>, Random> RegisterGenerators()
        {
            Action<DateTime, Dictionary<string, int>, Random> generators = null;

            generators += PageViewsAndAverageTimeOnPage;
            generators += Countries;
            generators += VisitorsAndTopLandingAndExitPages;
            generators += OnSiteAndExternalKeywords;
            generators += FileDownloads;
            generators += AggregatedViews;
            generators += InvalidPages;
            generators += SearchEngines;
            generators += UrlReferrals;
            generators += MobileDevices;
            generators += BrowserTypes;
            generators += FlashJavaAndSilverligthSupport;
            generators += OperatingSystems;
            generators += ScreenColors;
            generators += ScreenResolution;
            generators += Conversions;
            return generators;
        }


        /// <summary>
        /// Statistics code names being generated. 
        /// </summary>
        public static string[] StatisticCodeNames =
        { 
            "aggviews", "avgtimeonpage", "browsertype", "countries", "crawler", "exitpage", "filedownloads", "flash", 
            "java", "landingpage", "mobiledevice", "onsitesearchkeyword", "operatingsystem", 
            "pagenotfound", "pageviews", "referringsite_direct", "referringsite_local", 
            "referringsite_referring", "registereduser", "referringsite_search", "screencolor", 
            "screenresolution", "searchkeyword", "silverlight", "urlreferrals", "visitfirst", 
            "visitreturn"
        };


        #region "Data sources"

        private readonly Dictionary<string, int> mFileDownloadsDataSource = new Dictionary<string, int>
        {
            {"/Files/logo",10}, 
            {"/Files/Pertnership-program.pdf",6}, 
            {"/Files/Price-list.xlsx",7}, 
            {"/Ebooks/Everything-about-coffee.epub",10}, 
            {"/Ebooks/Everything-about-coffee.pdf",2},
            {"/Ebooks/How-to-be-better-barista.epub",9}, 
            {"/Ebooks/How-to-be-better-barista.pdf",3}, 
            {"/Manuals/Mazzer-Super-Jolly-DR.pdf",7},  
            {"/Manuals/Macap-MD4.pdf",6}
        };


        private readonly Dictionary<string, int> mSearchKeywordDataSource = new Dictionary<string, int>
        {
            {"coffee", 10},
            {"arabica", 9},
            {"colombia", 9},
            {"techniques", 3},
            {"partnership", 4},
            {"wholesale", 3},
            {"nicaragua dipilto", 2},
            {"panama honey", 2},
            {"grinders", 8},
            {"macap m2d", 1},
            {"porlex", 2},
            {"espresso", 4},
            {"hario", 5},
            {"plunger", 6},
            {"aeropress", 5},
            {"filter", 7},
            {"accessories", 7},
        };


        private readonly Dictionary<string, int> mSearchEnginesDataSource = new Dictionary<string, int>
        {
            {"Google", 10},
            {"Bing", 7},
            {"Yahoo", 4},
        };


        private readonly Dictionary<string, int> mReferralsDataSource = new Dictionary<string, int>
        {
            {"http://www.facebook.com/", 10},
            {"http://coffeegeeks.com/articles/where-to-get-best-coffee.html", 5},
            {"http://www.hario.com/resellers/", 3},
            {"http://coffeedictionary.org/ethiopia/yirgacheffe/stores", 2},
            {"http://coffeedictionary.org/colombia/carlos-imbachi/stores", 1},
        };


        private readonly Dictionary<string, int> mMobileDevicesDataSource = new Dictionary<string, int>
        {
            {"Alcatel One Touch", 4},
            {"Amazon Kindle Fire", 6},
            {"Apple iPad 4", 8},
            {"Apple iPhone 4", 8},
            {"Apple iPhone 5S", 10},
            {"Apple iPhone 6", 9},
            {"Apple iPhone 6 Plus", 7},
            {"BlackBerry 9900", 3},
            {"BlackBerry Z30", 5},
            {"Google Nexus 10", 8},
            {"Google Nexus 9", 7},
            {"HTC One S", 5},
            {"HTC Sensation", 4},
            {"HTC Thunderbolt", 4},
            {"Huawei Ascend Y300", 3},
            {"LG G3", 6},
            {"LG Optimus L5", 5},
            {"Motorola Moto E", 4},
            {"Motorola Moto G", 4},
            {"Nokia Lumia 920", 4},
            {"Nokia Lumia 928", 5},
            {"Samsung Galaxy Nexus", 7},
            {"Samsung Galaxy Note", 6},
            {"Samsung Galaxy S6", 9},
            {"Sony Xperia S", 6},
            {"Sony Xperia Z", 7},
        };


        private readonly Dictionary<string, int> mCountriesDataSource = new Dictionary<string, int>
        {
            {"USA", 50},
            {"Mexico", 3},
            {"Spain", 3},
            {"Canada", 12},
            {"Germany", 3},
            {"UK", 5},
            {"Japan", 2},
            {"Czech Republic", 1},
        };


        private readonly Dictionary<string, int> mBrowsersDataSource = new Dictionary<string, int>
        {
            {"IE10.0", 7},
            {"Internet Explorer 11", 35},
            {"Firefox36.0", 7},
            {"Unknown", 10},
            {"Firefox37.0", 30},
            {"Chrome", 50},
            {"Opera9.8", 1},
            {"Android Browser", 8},
            {"Safari8.0", 20},
        };


        private readonly Dictionary<string, int> mScreenColorDataSource = new Dictionary<string, int>
        {
            {"24-bit", 50},
            {"16-bit", 2},
            {"32-bit", 1},
        };


        private readonly string[] mNameDataSource =
        {
            "Jannine Stevens",
            "Bart Christians",
            "Kacey Chambers",
            "Mervin Victor",
            "Deb Albertson",
            "Barclay Cory",
            "Leona Graves",
            "Jordana Attaway",
            "Eveleen Foss",
            "Tessa Myers",
            "Fannie	Alvarez",
            "Debra Klein",
            "Lula Clarke",
            "Ed Hanson",
            "Lorraine Roberson",
            "Phillip Watson",
            "Patricia Webster",
            "Jackie Schwartz",
            "Jennie Gregory",
            "Darlene Henry",
            "Santos Olson",
            "Rhonda Diaz",
            "Cheryl Garrett",
            "Wanda Ballard",
            "Robert Silva",
            "Hannah Watkins",
            "Katrina Larson",
            "Kerry Horton",
            "Jonathon Schmidt",
            "Vickie Garza",
            "Ora Barton",
            "Suzanne Patton",
            "Jared Walsh",
            "June Ray",
            "Henrietta Rodgers",
        };


        private readonly Dictionary<string, int> mOperatingSystemDataSource = new Dictionary<string, int>
        {
            {"Windows", 60},
            {"Linux", 3},
            {"OS X", 6},
            {"UNIX", 5},
            {"Unknown OS", 12},
        };


        private readonly Dictionary<string, int> mScreenResolutionDataSource = new Dictionary<string, int>
        {
            {"1366×768", 18},
            {"1920×1080", 7},
            {"360×640", 6},
            {"1024×768", 5},
            {"1280×800", 5},
            {"1440×900", 4},
            {"1280×1024", 4},
            {"1600×900", 4},
            {"320×568", 3},
            {"1680×1050", 2},
        };

        #endregion


        #region "Visitors hits"

        /// <summary>
        /// Logs countries obtained from <see cref="mCountriesDataSource"/>.
        /// </summary>
        private void Countries(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            LogHit(HitLogProvider.COUNTRIES, visitors.Sum(visitor => visitor.Value), 0, date, GetRandomDataSourceValue(mCountriesDataSource, random), null, 0);
        }


        /// <summary>
        /// Iterates names in <see cref="mNameDataSource" /> and log the name as registered user for random day in last month.
        /// </summary>
        private void RegisteredUsers(Random random)
        {
            for (int index = 0; index < mNameDataSource.Length; index++)
            {
                var name = mNameDataSource[index];
                var randomDay = random.Next(-30, 0);
                LogHit(HitLogProvider.REGISTEREDUSER, 1, 0, DateTime.Now.AddDays(randomDay), name, null, index + 1);
            }
        }


        /// <summary>
        /// Logs mobile devices obtained from <see cref="mMobileDevicesDataSource"/>.
        /// </summary>
        private void MobileDevices(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            var visitorsPart = GetRelativeAmountOfVisitors(0.6, 0.2, random, visitors);
            var mobileDevicesRatio = GetDataSourceFrequency(mMobileDevicesDataSource);
            int totalVisitors = visitorsPart.Sum(visitor => visitor.Value);

            foreach (var mobileDevice in mMobileDevicesDataSource)
            {
                LogHit(HitLogProvider.MOBILEDEVICE, (int)(mobileDevicesRatio[mobileDevice.Key] * totalVisitors), 0, date, mobileDevice.Key, null, 0);
            }
        }

        #endregion


        #region "Traffic sources hits"

        /// <summary>
        /// Logs URL Referrals obtained from <see cref="mReferralsDataSource"/>. Logs referring site as well (in this case the hits are aggregated by referral domain).
        /// </summary>
        public void UrlReferrals(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            var visitorsPart = GetRelativeAmountOfVisitors(0.15, 0.1, random, visitors);
            var urlReferralsRatio = GetDataSourceFrequency(mReferralsDataSource);
            int totalVisitors = visitorsPart.Sum(visitor => visitor.Value);

            foreach (var urlReferral in mReferralsDataSource)
            {
                int visitorCount = (int)(urlReferralsRatio[urlReferral.Key] * totalVisitors);
                LogHit(HitLogProvider.URL_REFERRALS, visitorCount, 0, date, urlReferral.Key, null, 0);
                LogHit(HitLogProvider.REFERRINGSITE + "_referring", visitorCount, 0, date, URLHelper.GetDomain(urlReferral.Key), null, 0);
            }
        }


        /// <summary>
        /// Logs search engines obtained from <see cref="mSearchEnginesDataSource"/>. Logs search referring site as well.
        /// </summary>
        public void SearchEngines(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            var visitorsPart = GetRelativeAmountOfVisitors(0.3, 0.1, random, visitors);
            var searchEnginesRatios = GetDataSourceFrequency(mSearchEnginesDataSource);
            int totalVisitors = visitorsPart.Sum(visitor => visitor.Value);

            foreach (var searchEngine in mSearchEnginesDataSource)
            {
                int visitorCount = (int)(searchEnginesRatios[searchEngine.Key] * totalVisitors);
                LogHit(HitLogProvider.CRAWLER, visitorCount, 0, date, searchEngine.Key, null, 0);
                LogHit(HitLogProvider.REFERRINGSITE + "_search", visitorCount, 0, date, searchEngine.Key, null, 0);
            }
        }

        #endregion


        #region "Content hits"

        /// <summary>
        /// Generates Aggregated views representing the views through RSS or atom feeds, therefore documents are filtered to match only Article type.
        /// </summary>
        private void AggregatedViews(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            var visitorsPart = GetRelativeAmountOfVisitors(0.05, 0.001, random, visitors);

            foreach (var visitor in visitorsPart)
            {
                // Storing in another variable to avoid non intentional closure effects
                var pair = visitor;
                foreach (var document in GetRandomDocuments(5, 3, "DancingGoat.Article").Where(document => document.DocumentCulture == pair.Key))
                {
                    LogHit(HitLogProvider.AGGREGATED_VIEWS, visitor.Value, 0, date, document.NodeAliasPath, document.DocumentCulture, document.NodeID);
                }
            }
        }


        /// <summary>
        /// Logs file downloads obtained from the <see cref="mFileDownloadsDataSource"/>
        /// </summary>
        public void FileDownloads(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            var visitorsPart = GetRelativeAmountOfVisitors(0.05, 0.001, random, visitors);

            foreach (var visitor in visitorsPart)
            {
                LogHit(HitLogProvider.FILE_DOWNLOADS, visitor.Value, 0, date,
                    GetRandomDataSourceValue(mFileDownloadsDataSource, random), visitor.Key, 0);
            }
        }


        /// <summary>
        /// Splits visitors between returning and the new ones. For the new visitors, logs first visit hit and both top and exit landing pages.
        /// </summary>
        private void VisitorsAndTopLandingAndExitPages(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            Dictionary<string, int> returningVisitors;
            Dictionary<string, int> firstVisitors;

            SplitVisitors(0.7, 0.1, random, visitors, out firstVisitors, out returningVisitors);

            LogHit(HitLogProvider.VISITORS_FIRST, firstVisitors.Sum(visitor => visitor.Value), 0, date, mSite.SiteName, null, 0);
            LogHit(HitLogProvider.VISITORS_RETURNING, returningVisitors.Sum(visitor => visitor.Value), 0, date, mSite.SiteName, null, 0);

            var randomDocuments = GetRandomDocuments(5, 3).ToList();

            foreach (var visitor in firstVisitors)
            {
                var filteredDocuments =
                    randomDocuments.Where(document => document.DocumentCulture == visitor.Key).ToList();

                foreach (var document in filteredDocuments)
                {
                    LogHit(HitLogProvider.EXITPAGE, visitor.Value / filteredDocuments.Count(), 0, date,
                        document.NodeAliasPath,
                        document.DocumentCulture, document.NodeID);
                    LogHit(HitLogProvider.LANDINGPAGE, visitor.Value / filteredDocuments.Count(), 0, date,
                        document.NodeAliasPath,
                        document.DocumentCulture, document.NodeID);

                    LogHit(HitLogProvider.REFERRINGSITE + "_direct", visitor.Value / filteredDocuments.Count(), 0, date,
                        document.NodeAliasPath,
                        document.DocumentCulture, document.NodeID);
                }
            }
        }


        /// <summary>
        /// Logs both on-site and external search keywords obtained from the <see cref="mSearchKeywordDataSource"/>.
        /// </summary>
        private void OnSiteAndExternalKeywords(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            var visitorsPart = GetRelativeAmountOfVisitors(0.1, 0.02, random, visitors);

            var externalKeywordDataSource = new Dictionary<string, int>(mSearchKeywordDataSource);
            externalKeywordDataSource.Add("(not provided)", 40);

            LogHit(HitLogProvider.ONSITESEARCHKEYWORD, visitorsPart.Sum(visitor => visitor.Value), 0, date, GetRandomDataSourceValue(mSearchKeywordDataSource, random), null, 0);
            LogHit(HitLogProvider.SEARCHKEYWORD, visitorsPart.Sum(visitor => visitor.Value), 0, date, GetRandomDataSourceValue(externalKeywordDataSource, random), null, 0);
        }


        /// <summary>
        /// Picks random documents and alter their alias paths by adding space or removing some of the letters from the end of the path.
        /// These paths are then logged as invalid ones.
        /// </summary>
        private void InvalidPages(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            var visitorsPart = GetRelativeAmountOfVisitors(0.05, 0.001, random, visitors);
            var documents = GetRandomDocuments(3, 1).ToList();

            for (int i = 0; i < documents.Count; i++)
            {
                var document = documents[i];
                string newAliasPath = document.NodeAliasPath;

                // This path is likely to be correct after the altering, therefore should be omitted
                if (newAliasPath.Length <= 2)
                {
                    continue;
                }

                // For first two iterated documents insert space somewhere in the node alias path
                if (i < 2)
                {
                    newAliasPath = newAliasPath.Insert(random.Next(0, newAliasPath.Length - 1), " ");
                }
                // For the rest two documents remove some letters from the end of the node alias path
                else
                {
                    int length = document.NodeAliasPath.Length;
                    newAliasPath = document.NodeAliasPath.Substring(0, random.Next(2, length));
                }

                LogHit(HitLogProvider.PAGE_NOT_FOUND, visitorsPart.Sum(visitor => visitor.Value), 0, date, newAliasPath, document.DocumentCulture, 0);
            }
        }


        /// <summary>
        /// Picks random documents for both cultures, distributes number of visitors across the pages while penalizing long document alias paths and logs page view. Logs average time on page for
        /// every processed document as well as direct referring site.
        /// </summary>
        private void PageViewsAndAverageTimeOnPage(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            var randomDocuments = GetRandomDocuments(5, 3).ToList();

            Func<string, double> getVisitorsRatio = culture => visitors[culture] / randomDocuments.Where(document => document.DocumentCulture == culture).Sum(document => 1.0 / document.NodeAliasPath.Length);

            var visitorsRatio = visitors.ToDictionary(visitor => visitor.Key, visitor => getVisitorsRatio(visitor.Key));

            foreach (var document in randomDocuments)
            {
                int visitorsPerDocument = (int)(1.0 / document.NodeAliasPath.Length * visitorsRatio[document.DocumentCulture]);

                LogHit(HitLogProvider.PAGE_VIEWS, visitorsPerDocument, 0, date, document.NodeAliasPath, document.DocumentCulture, document.NodeID);
                LogHit(HitLogProvider.AVGTIMEONPAGE, 1, GetRandomValue(random, 10, 40), date, document.NodeAliasPath, document.DocumentCulture, document.NodeID);
            }
        }

        #endregion


        #region "Browser capabilities hits"

        /// <summary>
        /// Logs browser types obtained from <see cref="mBrowsersDataSource"/>.
        /// </summary>
        private void BrowserTypes(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            LogHit(HitLogProvider.BROWSER_TYPE, visitors.Sum(visitor => visitor.Value), 0, date, GetRandomDataSourceValue(mBrowsersDataSource, random), null, 0);
        }


        /// <summary>
        /// Logs support for Flash, Java and Silverlight.
        /// </summary>
        private void FlashJavaAndSilverligthSupport(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            ThirdPartySupport(date, visitors, random, 0.8, HitLogProvider.FLASH, "hf", "nf");
            ThirdPartySupport(date, visitors, random, 0.85, HitLogProvider.SILVERLIGHT, "hs", "ns");
            ThirdPartySupport(date, visitors, random, 0.95, HitLogProvider.JAVA, "hj", "nj");
        }


        /// <summary>
        /// Logs third party application support (usable for logging Flash, Java and Silverlight support).
        /// </summary>
        /// <param name="date">Date the hit will be assigned to</param>
        /// <param name="visitors">Contains number of visitor per every culture</param>
        /// <param name="random">Random number generator</param>
        /// <param name="threshold">Relative threshold determining how many visitors do not support third party applications</param>
        /// <param name="hitType">Codename of the statistics</param>
        /// <param name="supportedValue">Value will be logged for the supporting visitors</param>
        /// <param name="notsupportedValue">Value will be logged fo the not supporting visitors</param>
        private void ThirdPartySupport(DateTime date, Dictionary<string, int> visitors, Random random, double threshold,
            string hitType, string supportedValue, string notsupportedValue)
        {
            Dictionary<string, int> notSupportingVisitors;
            Dictionary<string, int> supportingVisitors;

            SplitVisitors(threshold, 0.2, random, visitors, out notSupportingVisitors, out supportingVisitors);

            LogHit(hitType, notSupportingVisitors.Sum(visitor => visitor.Value), 0, date, notsupportedValue, null, 0);
            LogHit(hitType, supportingVisitors.Sum(visitor => visitor.Value), 0, date, supportedValue, null, 0);
        }


        /// <summary>
        /// Logs browser types obtained from <see cref="mOperatingSystemDataSource"/>.
        /// </summary>
        private void OperatingSystems(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            LogHit(HitLogProvider.OPERATINGSYSTEM, visitors.Sum(visitor => visitor.Value), 0, date, GetRandomDataSourceValue(mOperatingSystemDataSource, random), null, 0);
        }


        /// <summary>
        /// Logs browser types obtained from <see cref="mScreenColorDataSource"/>.
        /// </summary>
        private void ScreenColors(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            LogHit(HitLogProvider.SCREENCOLOR, visitors.Sum(visitor => visitor.Value), 0, date, GetRandomDataSourceValue(mScreenColorDataSource, random), null, 0);
        }


        /// <summary>
        /// Logs browser types obtained from <see cref="mScreenResolutionDataSource"/>.
        /// </summary>
        private void ScreenResolution(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            LogHit(HitLogProvider.SCREENRESOLUTION, visitors.Sum(visitor => visitor.Value), 0, date, GetRandomDataSourceValue(mScreenResolutionDataSource, random), null, 0);
        }

        #endregion


        #region "Campaign & conversion hits"

        /// <summary>
        /// Logs hits for top 6 conversions found within the site.
        /// </summary>
        private void Conversions(DateTime date, Dictionary<string, int> visitors, Random random)
        {
            var conversions = ConversionInfoProvider.GetConversions().OnSite(mSite.SiteID).ToList();
            int visitorsPerConversion = visitors.Sum(visitor => visitor.Value) / conversions.Count;

            foreach (var conversion in conversions)
            {
                LogHit(HitLogProvider.CONVERSIONS, (int)Math.Round(0.75 * (random.NextDouble() / 2) * visitorsPerConversion), 0, date, conversion.ConversionName, null, 0);
            }
        }

        #endregion


        /// <summary>
        /// Gets relative frequency of every item in <paramref name="input"/> on interval [0,1].
        /// </summary>
        /// <param name="input">Dictionary containing all data source items with their absolute frequency</param>
        /// <returns>Computed relative frequency for every item in <paramref name="input"/></returns>
        private Dictionary<string, double> GetDataSourceFrequency(Dictionary<string, int> input)
        {
            int whole = input.Sum(pair => pair.Value);
            return input.ToDictionary(pair => pair.Key, pair => 1.0 / whole * pair.Value);
        }


        /// <summary>
        /// Gets random value from the <paramref name="input"/> while weight of every value is defined by its value in the input dictionary.
        /// </summary>
        /// <param name="input">Contains all the values the result will be picked from, where key is the output value and key is weight of the item</param>
        /// <param name="random">Random number generator</param>
        /// <returns>Single value randomly picked from <paramref name="input"/> with respect to the item weight.</returns>
        private string GetRandomDataSourceValue(Dictionary<string, int> input, Random random)
        {
            int whole = input.Sum(pair => pair.Value);
            var ordered = input.OrderByDescending(pair => pair.Value).ToList();

            int randomFeed = random.Next(1, whole + 1);
            int counter = 0;

            foreach (var pair in ordered)
            {
                counter += pair.Value;
                if (counter >= randomFeed)
                {
                    return pair.Key;
                }
            }

            return null;
        }


        /// <summary>
        /// Splits visitors given in <paramref name="visitors"/> according to <paramref name="threshold"/> with respect to <paramref name="variance"/>. 
        /// </summary>
        /// <param name="threshold">Relative amount of visitors, where those for interval [0-threshold] are stored in <paramref name="belowThreshold"/>, while the rest in <paramref name="overThreshold"/></param>
        /// <param name="variance">Specifies weight of the random variance added to the threshold</param>
        /// <param name="random">Random number generator</param>
        /// <param name="visitors">Input dictionary of visitors which will be splitted. Contains key value pair for every visitor culture</param>
        /// <param name="belowThreshold">Out parameter where visitors below the threshold will be stored</param>
        /// <param name="overThreshold">OUt parameter where visitors over the threshold will be stored</param>
        private void SplitVisitors(double threshold, double variance, Random random, Dictionary<string, int> visitors,
            out Dictionary<string, int> belowThreshold, out Dictionary<string, int> overThreshold)
        {
            belowThreshold = new Dictionary<string, int>();
            overThreshold = new Dictionary<string, int>();

            foreach (var visitor in visitors)
            {
                double ratio = threshold + ((1 - threshold) * variance) * random.NextDouble();
                int visitorsUnderThresold = (int)(visitor.Value * ratio);

                belowThreshold.Add(visitor.Key, visitorsUnderThresold);
                overThreshold.Add(visitor.Key, visitor.Value - visitorsUnderThresold);
            }
        }


        /// <summary>
        /// Gets relative amount of visitors from <paramref name="visitors"/> for each input culture.
        /// </summary>
        /// <param name="relativeAmount">Relative amount of visitors which will be returned. Has to be in interval [0,1]</param>
        /// <param name="variance">Specifies weight of the random variance added to the threshold</param>
        /// <param name="random">Random number generator</param>
        /// <param name="visitors">Input dictionary of visitors from which the output will be obtained. Contains key value pair for every visitor culture</param>
        /// <returns>Dictionary containing only relative amount of visitors from the <paramref name="visitors"/></returns>
        private Dictionary<string, int> GetRelativeAmountOfVisitors(double relativeAmount, double variance, Random random,
            Dictionary<string, int> visitors)
        {
            double ratio = relativeAmount + ((1 - relativeAmount) * variance) * random.NextDouble();
            return visitors.ToDictionary(visitor => visitor.Key, visitor => (int)(visitor.Value * ratio));
        }


        /// <summary>
        /// Gets pseudo-random number from the  <paramref name="baseValue"/> vicinity. The least value returned is always 1.
        /// </summary>
        /// <param name="random">Random number generator</param>
        /// <param name="baseValue">Specifies the base value of the result</param>
        /// <param name="variance">Specifies how much does the result differ from <paramref name="baseValue"/></param>
        /// <returns>Pseudo-random number in <paramref name="baseValue"/> vicinity</returns>
        private int GetRandomValue(Random random, int baseValue = 5, int variance = 10)
        {
            return Math.Max(1, baseValue + (int)Logit(random.NextDouble()) * variance);
        }


        /// <summary>
        /// Gets <paramref name="numberOfEnglishDocuments"/> documents in English culture and <paramref name="numberOfSpanishDocuments"/> in Spanish culture from the current site. 
        /// </summary>
        /// <param name="numberOfEnglishDocuments">Number of documents in English culture to be obtained</param>
        /// <param name="numberOfSpanishDocuments">Number of documents in Spanish culture to be obtained</param>
        /// <param name="documentType">Specifies filter of document types. Leave null for all document types</param>
        /// <returns>Collection of randomly obtained documents from the current site</returns>
        public IEnumerable<TreeNode> GetRandomDocuments(int numberOfEnglishDocuments, int numberOfSpanishDocuments, string documentType = null)
        {
            Func<string, int, IEnumerable<TreeNode>> getDocuments = (culture, numberOfDocuments) =>
            {
                var result = DocumentHelper.GetDocuments().Culture(culture);
                if (documentType != null)
                {
                    result = result.Type(documentType);
                }

                return result.ToList().OrderBy(n => Guid.NewGuid()).Take(numberOfDocuments);
            };

            return getDocuments("en-US", numberOfEnglishDocuments).Union(getDocuments("es-ES", numberOfSpanishDocuments));
        }


        /// <summary>
        /// Function, which maps interval [0,1] to whole real domain.
        /// Input value is reduced to interval [1e-12, 1 - 1e-12] to avoid infinity or extreme large results.
        /// </summary>
        /// <param name="x">Input value.</param>
        /// <returns>Product of Logit function on <paramref name="x"/></returns>
        public double Logit(double x)
        {
            x = Math.Max(0.000000000001, x);
            x = Math.Min(0.999999999999, x);

            return Math.Log(x / (1 - x));
        }


        /// <summary>
        /// Performs logging of the hit.
        /// </summary>
        private void LogHit(string codeName, int visits, int value, DateTime logTime, string objectName, string culture,
            int objectId)
        {
            HitLogProcessor.SaveLogToDatabase(new LogRecord
            {
                CodeName = codeName,
                Hits = visits,
                Value = value,
                LogTime = logTime,
                ObjectName = objectName,
                ObjectId = objectId,
                SiteName = mSite.SiteName,
                Culture = culture
            });
        }
    }
}
