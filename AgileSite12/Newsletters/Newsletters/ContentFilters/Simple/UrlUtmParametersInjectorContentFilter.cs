using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;
using CMS.Core;

namespace CMS.Newsletters.Filters
{
    /// <summary>
    /// Filter for injecting UTM parameters to URLs within email content.
    /// </summary>
    internal sealed class UrlUtmParametersInjectorContentFilter : IEmailContentFilter
    {
        private const string UTM_SOURCE = "utm_source";
        private const string UTM_CAMPAIGN = "utm_campaign";
        private const string UTM_MEDIUM = "utm_medium";

        private const string UTM_EMAIL_VALUE = "email";

        private static readonly Regex hyperlinkRegex = RegexHelper.GetRegex(@"(?<prefix><a[\s|\S]*?href(\s)*?=(\s)*?)(?<quote>[""'])(?<url>.*?)\k<quote>(?<suffix>[\s|\S]*?>)", RegexOptions.IgnoreCase);

        private readonly SiteInfo site;
        private readonly IssueInfo issue;
        private readonly string baseUrl;


        /// <summary>
        /// Creates instance of <see cref="UrlUtmParametersInjectorContentFilter"/> class.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <param name="site">Issue site.</param>
        /// <param name="baseUrl">Base URL to use for resolving.</param>
        public UrlUtmParametersInjectorContentFilter(IssueInfo issue, SiteInfo site, string baseUrl)
        {
            this.issue = issue;
            this.site = site;
            this.baseUrl = baseUrl;
        }


        /// <summary>
        /// Applies the filter to the given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Text to transform.</param>
        /// <returns>Text with applied filter transformation.</returns>
        public string Apply(string text)
        {
            var baseLink = new Link(baseUrl);
            var aliases = GetAliases();

            return hyperlinkRegex.Replace(text, (m) =>
            {
                var link = new Link(HTMLHelper.HTMLDecode(m.Groups["url"].Value));

                if (IsDomainLink(link, baseLink, site.DomainName, aliases) || UtmParametersForAllLinks())
                {
                    link = CreateUrlWithUtm(link, issue.IssueUTMSource, issue.IssueUTMCampaign);
                }
                return String.Concat(m.Groups["prefix"], m.Groups["quote"], link.ToString(), m.Groups["quote"], m.Groups["suffix"]);
            });
        }


        private bool UtmParametersForAllLinks()
        {
            var utmParametersForAllLinks = Service.Resolve<IAppSettingsService>()["CMSEmailUtmParametersForAllLinks"];
            return ValidationHelper.GetBoolean(utmParametersForAllLinks, false);
        }


        private IList<string> GetAliases()
        {
            return SiteDomainAliasInfoProvider.GetDomainAliases(site.SiteID)
                                              .Items
                                              .ToList()
                                              .Select(siteDomain => siteDomain.SiteDomainAliasName)
                                              .ToList();
        }


        /// <summary>
        /// Returns true if <paramref name="link" /> is domain link. Membership to the domain is determined by other parameters. 
        /// Relative link is considered as domain link.
        /// </summary>
        public bool IsDomainLink(Link link, Link baselink, string domainName, IList<string> domainAliases)
        {
            if (MacroProcessor.ContainsMacro(link.ToString()))
            {
                return false;
            }

            if (link.IsRelative())
            {
                return true;
            }

            var linkDomain = link.GetDomain();
            string baseUrlDomain = baselink.IsRelative() ? string.Empty : baselink.GetDomain();

            return URLHelper.DomainMatch(baseUrlDomain, linkDomain, true) || 
                   URLHelper.DomainMatch(baselink.ToString(), linkDomain, true) || 
                   URLHelper.DomainMatch(domainName, linkDomain, true) || 
                   domainAliases.Any(aliasDomain => URLHelper.DomainMatch(aliasDomain, linkDomain, true));
        }


        /// <summary>
        /// Creates link with utm_parameters in query string.
        /// </summary>
        public Link CreateUrlWithUtm(Link link, string utmSource, string utmCampaign)
        {
            var queryString = PrepareQueryString(
                IsUtmPresentInLink(link, UTM_SOURCE) ? string.Empty : utmSource,
                IsUtmPresentInLink(link, UTM_CAMPAIGN) ? string.Empty : utmCampaign,
                IsUtmPresentInLink(link, UTM_MEDIUM) ? string.Empty : UTM_EMAIL_VALUE);

            return link.AppendQueryString(queryString);
        }


        private bool IsUtmPresentInLink(Link link, string utmParamName)
        {
            return link.HasParameter(utmParamName);
        }


        private string PrepareQueryString(string utmSource, string utmCampaign, string utmMedium)
        {
            var queryCollection = HttpUtility.ParseQueryString(string.Empty);

            if (!string.IsNullOrEmpty(utmSource))
            {
                queryCollection.Add(UTM_SOURCE, utmSource);
            }

            if (!string.IsNullOrEmpty(utmCampaign))
            {
                queryCollection.Add(UTM_CAMPAIGN, utmCampaign);
            }

            if (!string.IsNullOrEmpty(utmMedium))
            {
                queryCollection.Add(UTM_MEDIUM, utmMedium);
            }

            return queryCollection.ToString();
        }
    }
}
