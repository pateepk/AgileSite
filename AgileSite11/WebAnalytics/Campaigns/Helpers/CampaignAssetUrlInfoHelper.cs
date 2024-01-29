using System;

using CMS.Base;
using CMS.SiteProvider;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class providing helper methods for <see cref="CampaignAssetUrlInfo"/>.
    /// </summary>
    public class CampaignAssetUrlInfoHelper : AbstractHelper<CampaignAssetUrlInfoHelper>
    {
        #region "Public Methods"

        /// <summary>
        /// Creates valid URL target that is <see cref="Uri.AbsolutePath"/> without scheme, port, domain, query strings, 
        /// fragments and trailing slash at the end.
        /// If the <paramref name="url"/> is already normalized, then it is returned.
        /// Removes <see cref="SiteInfo.SitePresentationURL"/> from the <paramref name="url"/>.
        /// </summary>
        /// <param name="url">URL to be changed to correct format.</param>
        /// <param name="site">Is value from <see cref="SiteInfo.SitePresentationURL"/> of the content only site.</param>
        /// <returns>Returns valid URL for <see cref="CampaignAssetUrlInfo.CampaignAssetUrlTarget"/> property from given URL.</returns>
        /// <example>
        /// <para>'http://your-domain.com/landing_page/' is not normalized.</para>
        /// <para>String.Empty is not normalized.</para>
        /// <para>'/' is normalized.</para>
        /// <para>'landing/page' is not normalized.</para>
        /// <para>'/landing/page' is normalized.</para>
        /// <para>'/landing/page/' is not normalized.</para>
        /// </example>
        public static string NormalizeAssetUrlTarget(string url, SiteInfo site)
        {
            return HelperObject.NormalizeAssetUrlTargetInternal(url, site);
        }


        /// <summary>
        /// Returns true if <paramref name="url"/> is <see cref="Uri.AbsolutePath"/> without scheme, port, domain, query strings, 
        /// fragments and trailing slash at the end.
        /// If true, value of <paramref name="url"/> is valid for setting in <see cref="CampaignAssetUrlInfo.CampaignAssetUrlTarget"/>.
        /// </summary>
        /// <param name="url">URL</param>
        /// <example>
        /// <para>'http://your-domain.com/landing_page/' is not normalized.</para>
        /// <para>String.Empty is not normalized.</para>
        /// <para>'/' is normalized.</para>
        /// <para>'landing/page' is not normalized.</para>
        /// <para>'/landing/page' is normalized.</para>
        /// <para>'/landing/page/' is not normalized.</para>
        /// </example>
        public static bool IsNormalizedUrlTarget(string url)
        {
            return HelperObject.IsNormalizedUrlTargetInternal(url);
        }


        /// <summary>
        /// Returns <see cref="Uri"/> created from <paramref name="site"/>'s <see cref="SiteInfo.SitePresentationURL"/> 
        /// and <paramref name="urlAsset"/>'s <see cref="CampaignAssetUrlInfo.CampaignAssetUrlTarget"/>.
        /// </summary>
        /// <param name="urlAsset">
        /// <see cref="CampaignAssetUrlInfo"/> to get <see cref="CampaignAssetUrlInfo.CampaignAssetUrlTarget"/>.
        /// </param>
        /// <param name="site">
        /// <see cref="SiteInfo"/> to generate URL, if not supplied, then <see cref="SiteContext.CurrentSite"/> is used.
        /// </param>
        /// <exception cref="UriFormatException">
        /// If <paramref name="urlAsset"/>'s <see cref="CampaignAssetUrlInfo.CampaignAssetUrlTarget"/> is not normalized
        /// or <paramref name="site"/>'s <see cref="SiteInfo.SitePresentationURL"/> is not correctly set.
        /// </exception>
        public static Uri GetCampaignAssetUrlInfoFullUri(CampaignAssetUrlInfo urlAsset, SiteInfo site = null)
        {
            return HelperObject.GetCampaignAssetUrlInfoFullUriInternal(urlAsset, site);
        }

        #endregion


        #region "Virtual Methods"

        /// <summary>
        /// Creates valid URL target that is relative path from the website root without scheme, port, domain, query strings, 
        /// fragments and trailing slash at the end.
        /// If the <paramref name="url"/> is already normalized format <see cref="IsNormalizedUrlTarget(string)"/>, then it is returned.
        /// </summary>
        /// <param name="url">URL to be changed to correct format.</param>
        /// <param name="site">
        /// <see cref="SiteInfo"/> that has <see cref="SiteInfo.SiteIsContentOnly"/> set to true with correctly set <see cref="SiteInfo.SitePresentationURL"/>.
        /// Valid URL target will be created as relative path from <see cref="SiteInfo.SitePresentationURL"/>.
        /// </param>
        /// <returns>
        /// Returns valid URL for <see cref="CampaignAssetUrlInfo.CampaignAssetUrlTarget"/> property from given URL
        /// or returns empty string when <paramref name="url"/> can't be normalized.
        /// </returns>
        /// <example>
        /// <para> 
        /// If site's presentation url 'http://your-domain.com/virt_dir/' and <paramref name="url"/> is 'http://your-domain.com/virt_dir/some_page',
        /// then returned URL is '/some_page'.
        /// </para>
        /// <para>'http://your-domain.com/landing_page/' is not normalized.</para>
        /// <para>String.Empty is not normalized.</para>
        /// <para>'/' is normalized.</para>
        /// <para>'landing/page' is not normalized.</para>
        /// <para>'/landing/page' is normalized.</para>
        /// <para>'/landing/page/' is not normalized.</para>
        /// </example>
        /// <exception cref="UriFormatException">Given <paramref name="url"/> is an invalid Uniform Resource Identifier (URI).</exception>
        /// <exception cref="ArgumentException">Site is not content only or does not contain correct <see cref="SiteInfo.SitePresentationURL"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// When given <paramref name="url"/> does not belong to given <paramref name="site"/> (<paramref name="url"/> is checked against site's <see cref="SiteInfo.SitePresentationURL"/>).
        /// </exception>
        /// <seealso cref="CampaignAssetUrlInfo.CampaignAssetUrlTarget"/>
        protected virtual string NormalizeAssetUrlTargetInternal(string url, SiteInfo site = null)
        {
            // If url is normalized, then it is returned
            if (IsNormalizedUrlTarget(url))
            {
                return url;
            }

            // Create Uris out of params
            Uri urlToBeNormalized = new Uri(url, UriKind.Absolute);
            Uri sitePresentationUrl = CreateSitePresentationUri(site);

            // Check if Uris belongs to the same site
            if (!UrlsBelongsToSameSite(urlToBeNormalized, sitePresentationUrl))
            {
                throw new InvalidOperationException("Given url does not belong to given site.");
            }

            return GetUrlRelativePathFromSitePresentationUrl(urlToBeNormalized, sitePresentationUrl);
        }


        /// <summary>
        /// Returns true if <paramref name="url"/> is <see cref="Uri.AbsolutePath"/> without scheme, port, domain, query strings, 
        /// fragments and trailing slash at the end.
        /// If true, value of <paramref name="url"/> is valid for setting in <see cref="CampaignAssetUrlInfo.CampaignAssetUrlTarget"/>.
        /// </summary>
        /// <param name="url">URL</param>
        /// <example>
        /// <para>'http://your-domain.com/landing_page/' is not normalized.</para>
        /// <para>String.Empty is not normalized.</para>
        /// <para>'/' is normalized.</para>
        /// <para>'landing/page' is not normalized.</para>
        /// <para>'/landing/page' is normalized.</para>
        /// <para>'/landing/page/' is not normalized.</para>
        /// </example>
        protected virtual bool IsNormalizedUrlTargetInternal(string url)
        {
            // Uri's AbsolutePath can not be retrieved when 
            Uri dummyDomain = new Uri("http://localhost/");
        
            Uri uri = null; 
            if(Uri.IsWellFormedUriString(url, UriKind.Relative) && Uri.TryCreate(dummyDomain, url, out uri))
            {
                var absolutePath = uri.AbsolutePath;
                if (absolutePath.Length > 1)
                {
                    absolutePath = absolutePath.TrimEnd('/');
                }

                return absolutePath.EqualsCSafe(url, true);
            }

            return false;
        }


        /// <summary>
        /// Returns <see cref="Uri"/> created from <paramref name="site"/>'s <see cref="SiteInfo.SitePresentationURL"/> 
        /// and <paramref name="urlAsset"/>'s <see cref="CampaignAssetUrlInfo.CampaignAssetUrlTarget"/>.
        /// </summary>
        /// <param name="urlAsset">
        /// <see cref="CampaignAssetUrlInfo"/> to get <see cref="CampaignAssetUrlInfo.CampaignAssetUrlTarget"/>.
        /// </param>
        /// <param name="site">
        /// <see cref="SiteInfo"/> to generate URL, if not supplied, then <see cref="SiteContext.CurrentSite"/> is used.
        /// </param>
        /// <exception cref="UriFormatException">
        /// If <paramref name="urlAsset"/>'s <see cref="CampaignAssetUrlInfo.CampaignAssetUrlTarget"/> is not normalized
        /// or <paramref name="site"/>'s <see cref="SiteInfo.SitePresentationURL"/> is not correctly set.
        /// </exception>
        protected virtual Uri GetCampaignAssetUrlInfoFullUriInternal(CampaignAssetUrlInfo urlAsset, SiteInfo site = null)
        {
            if (urlAsset == null)
            {
                throw new ArgumentNullException("urlAsset");
            }

            site = site ?? SiteContext.CurrentSite;
            var sitePresentationUrl = new Uri(site.SitePresentationURL);

            // Get application path from site presentation URL
            var applicationPath = sitePresentationUrl.AbsolutePath.TrimEnd('/');
            var campaignAssetUrlTarget = new Uri(urlAsset.CampaignAssetUrlTarget.TrimEnd('/'), UriKind.Relative);
            var relativePath = applicationPath + campaignAssetUrlTarget;
            
            // If site presentationUrl is http://domain/CMS and relativePath /CMS/Test then correct URL http://domain/CMS/Test is returned
            return new Uri(sitePresentationUrl, relativePath);
        }

        #endregion


        #region "Private Methods"

        /// <summary>
        /// Creates uri out of the given <paramref name="site"/>'s <see cref="SiteInfo.SitePresentationURL"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Site is not content only or has incorrectly set SitePresentationURL.</exception>
        private Uri CreateSitePresentationUri(SiteInfo site)
        {
            Uri sitePresentationUrl = null;
            if (!site.SiteIsContentOnly || !Uri.TryCreate(site.SitePresentationURL, UriKind.Absolute, out sitePresentationUrl))
            {
                throw new ArgumentException("Site is not content only or has incorrectly set SitePresentationURL.");
            }

            return sitePresentationUrl;
        }


        /// <summary>
        /// Takes url to be normalized and site's presentation url.
        /// Returns relative path of <paramref name="url"/> from <paramref name="sitePresentationUrl"/> in the form of <see cref="Uri.AbsolutePath"/>.
        /// </summary>
        private string GetUrlRelativePathFromSitePresentationUrl(Uri url, Uri sitePresentationUrl)
        {
            var relativeUrl = url.AbsolutePath.TrimEnd('/').Substring(sitePresentationUrl.AbsolutePath.TrimEnd('/').Length);
            return string.IsNullOrEmpty(relativeUrl) ? "/" : relativeUrl;
        }


        /// <summary>
        /// Returns true if both <paramref name="url"/> belongs to site with <paramref name="sitePresentationUrl"/>.
        /// </summary>
        private bool UrlsBelongsToSameSite(Uri url, Uri sitePresentationUrl)
        {
            string sitePresentationHostName = GetHostWithoutWWW(sitePresentationUrl);
            string urlHostName = GetHostWithoutWWW(url);

            // AbsolutePath for site should end with / if the site's path will be /Virt_dir and uri's path
            // will be /Virt_dir_cms, they could match even though they are different
            string siteAbsolutePath = sitePresentationUrl.AbsolutePath.TrimEnd('/') + "/";
            string uriAbsolutePath = url.AbsolutePath.TrimEnd('/') + "/";

            if ((sitePresentationHostName.EqualsCSafe(urlHostName, StringComparison.InvariantCultureIgnoreCase)) &&
                (uriAbsolutePath.StartsWithCSafe(siteAbsolutePath, true)))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns host name without www prefix.
        /// </summary>
        private string GetHostWithoutWWW(Uri sitePresentationUrl)
        {
            return sitePresentationUrl.Host.StartsWith("www.", StringComparison.InvariantCultureIgnoreCase) ? sitePresentationUrl.Host.Remove(0, 4) : sitePresentationUrl.Host;
        }

        #endregion
    }
}
