using System;
using System.Linq;

using CMS.DataEngine;
using CMS.DocumentEngine.Internal;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class providing <see cref="AlternativeUrlInfo"/> management.
    /// </summary>
    public class AlternativeUrlInfoProvider : AbstractInfoProvider<AlternativeUrlInfo, AlternativeUrlInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="AlternativeUrlInfoProvider"/>.
        /// </summary>
        public AlternativeUrlInfoProvider()
            : base(AlternativeUrlInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="AlternativeUrlInfo"/> objects.
        /// </summary>
        public static ObjectQuery<AlternativeUrlInfo> GetAlternativeUrls()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="AlternativeUrlInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="AlternativeUrlInfo"/> ID.</param>
        public static AlternativeUrlInfo GetAlternativeUrlInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="AlternativeUrlInfo"/>.
        /// </summary>
        /// <param name="alternativeUrl"><see cref="AlternativeUrlInfo"/> to be set.</param>
        public static void SetAlternativeUrlInfo(AlternativeUrlInfo alternativeUrl)
        {
            ProviderObject.SetInfo(alternativeUrl);
        }


        /// <summary>
        /// Deletes specified <see cref="AlternativeUrlInfo"/>.
        /// </summary>
        /// <param name="alternativeUrl"><see cref="AlternativeUrlInfo"/> to be deleted.</param>
        public static void DeleteAlternativeUrlInfo(AlternativeUrlInfo alternativeUrl)
        {
            ProviderObject.DeleteInfo(alternativeUrl);
        }


        /// <summary>
        /// Deletes <see cref="AlternativeUrlInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="AlternativeUrlInfo"/> ID.</param>
        public static void DeleteAlternativeUrlInfo(int id)
        {
            AlternativeUrlInfo alternativeUrl = GetAlternativeUrlInfo(id);
            DeleteAlternativeUrlInfo(alternativeUrl);
        }


        /// <summary>
        /// Returns instance of <see cref="AlternativeUrlInfo"/> with defined given <paramref name="url"/> on given <paramref name="siteId" />.
        /// </summary>
        public static AlternativeUrlInfo GetAlternativeUrl(NormalizedAlternativeUrl url, int siteId)
        {
            return ProviderObject.GetAlternativeUrlInternal(url, siteId);
        }


        /// <summary>
        /// Returns instance of <see cref="AlternativeUrlInfo"/> with defined given <paramref name="url"/> on given <paramref name="siteId" />.
        /// </summary>
        protected virtual AlternativeUrlInfo GetAlternativeUrlInternal(NormalizedAlternativeUrl url, int siteId)
        {
            if (String.IsNullOrEmpty(url.NormalizedUrl))
            {
                return null;
            }

            return GetAlternativeUrls()
                  .OnSite(siteId)
                  .WhereEquals("AlternativeUrlUrl", url.NormalizedUrl)
                  .TopN(1)
                  .FirstOrDefault();
        }
        

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// If <paramref name="info"/> is in conflict with page's URL then <see cref="InvalidAlternativeUrlException"/> is thrown.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        public static void SetInfoCheckForConflictingPage(AlternativeUrlInfo info)
        {
            ProviderObject.SetInfoCore(info, true);
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(AlternativeUrlInfo info)
        {
            SetInfoCore(info, false);
        }


        private void SetInfoCore(AlternativeUrlInfo info, bool checkConflictingPage)
        {
            if (String.IsNullOrEmpty(info.AlternativeUrlUrl.NormalizedUrl))
            {
                throw new InvalidAlternativeUrlException(info);
            }

            if (!AlternativeUrlHelper.UrlMatchesConstraint(info))
            {
                throw new InvalidAlternativeUrlException(info);
            }

            var excludedUrl = AlternativeUrlHelper.GetConflictingExcludedUrl(info.AlternativeUrlUrl, info.AlternativeUrlSiteID);
            if (!String.IsNullOrEmpty(excludedUrl))
            {
                throw new InvalidAlternativeUrlException(info, excludedUrl);
            }

            var conflictingAlternativeUrl = AlternativeUrlHelper.GetConflictingAlternativeUrl(info);
            if (conflictingAlternativeUrl != null)
            {
                throw new InvalidAlternativeUrlException(info, conflictingAlternativeUrl);
            }

            if (checkConflictingPage)
            {
                var conflictingPage = AlternativeUrlHelper.GetConflictingPage(info);
                if (conflictingPage != null)
                {
                    throw new InvalidAlternativeUrlException(info, conflictingPage);
                }
            }

            base.SetInfo(info);
        }
    }
}