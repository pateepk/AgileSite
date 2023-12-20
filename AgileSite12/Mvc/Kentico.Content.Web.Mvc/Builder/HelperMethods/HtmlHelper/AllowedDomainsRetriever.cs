using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.SiteProvider;

namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Retrieves allowed domains from/to which post messages can be received/sent on a client.
    /// </summary>
    internal sealed class AllowedDomainsRetriever : IAllowedDomainsRetriever
    {
        private const string WWW_PREFIX = "www.";

        private readonly SiteInfo site;


        /// <summary>
        /// Creates an instance of <see cref="AllowedDomainsRetriever"/> class.
        /// </summary>
        /// <param name="site">Site for which the allowed domains should be retrieved.</param>
        public AllowedDomainsRetriever(SiteInfo site)
        {
            this.site = site ?? throw new ArgumentNullException(nameof(site));
        }


        /// <summary>
        /// Retrieves allowed domains from/to which post messages can be received/sent on a client.
        /// </summary>
        public IEnumerable<string> Retrieve()
        {
            var domains = GetDomains();
            var urls = GetUrls(domains);

            return urls.Select(domain => domain.GetLeftPart(UriPartial.Authority))
                          .Distinct();
        }


        private IList<Uri> GetUrls(IList<string> domains)
        {
            var urls = new List<Uri>();

            var useSslOnly = SettingsKeyInfoProvider.GetBoolValue("CMSUseSSLForAdministrationInterface");
            if (!useSslOnly)
            {
                urls.AddRange(domains.Select(domain => new Uri($"http://{domain}")));
            }
            urls.AddRange(domains.Select(domain => new Uri($"https://{domain}")));

            return urls;
        }


        private List<string> GetDomains()
        {
            var domains = new List<string>();

            domains.Add(site.DomainName);
            domains.AddRange(GetAliases());

            var domainsWithWww = GetDomainsWithEnsuredWww(domains).ToList();
            var domainsWithoutWww = GetDomainsWithRemovedWww(domains).ToList();

            domains.AddRange(domainsWithWww);
            domains.AddRange(domainsWithoutWww);

            return domains;
        }


        private IEnumerable<string> GetAliases()
        {
            var aliases = site.SiteDomainAliases.Values.Cast<SiteDomainAliasInfo>();
            return aliases.Select(alias => alias.SiteDomainAliasName);
        }


        private IList<string> GetDomainsWithEnsuredWww(IList<string> domains)
        {
            return domains.Where(alias => !alias.StartsWith(WWW_PREFIX, StringComparison.InvariantCultureIgnoreCase))
                          .Select(alias => $"{WWW_PREFIX}{alias}")
                          .ToList();
        }


        private IList<string> GetDomainsWithRemovedWww(IList<string> domains)
        {
            return domains.Where(alias => alias.StartsWith(WWW_PREFIX, StringComparison.InvariantCultureIgnoreCase))
                          .Select(alias => alias.Substring(WWW_PREFIX.Length))
                          .ToList();
        }
    }
}
