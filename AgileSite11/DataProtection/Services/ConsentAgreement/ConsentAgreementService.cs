using System;
using System.Collections.Generic;
using System.Data;

using CMS.ContactManagement;
using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DataProtection
{
    /// <summary>
    /// Service to manage consent agreements.
    /// </summary>
    internal sealed class ConsentAgreementService : IConsentAgreementService
    {
        private readonly IDateTimeNowService dateTimeNowService;

        /// <summary>
        /// Creates an instance of <see cref="ConsentAgreementService"/> class.
        /// </summary>
        /// <param name="dateTimeNowService">The DateTimeNowService which will be used for retrieving the current time.</param>
        public ConsentAgreementService(IDateTimeNowService dateTimeNowService = null)
        {
            this.dateTimeNowService = dateTimeNowService ?? Service.Resolve<IDateTimeNowService>();
        }


        /// <summary>
        /// Inserts an <see cref="ConsentAgreementInfo">agreement</see> of the given <see cref="ConsentInfo">consent</see> and the given <see cref="ContactInfo">contact</see>.
        /// </summary>
        /// <remarks>
        /// If an active agreement already exists then this active agreement is kept and its hash is updated with a one from the given consent.
        /// </remarks>
        /// <param name="contact">Contact</param>
        /// <param name="consent">Consent</param>
        /// <returns>Returns the agreement object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> is <c>null</c> -or <paramref name="consent"/> is <c>null</c></exception>
        public ConsentAgreementInfo Agree(ContactInfo contact, ConsentInfo consent)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }
            if (consent == null)
            {
                throw new ArgumentNullException(nameof(consent));
            }

            var consentAgreement = new ConsentAgreementInfo
            {
                ConsentAgreementContactID = contact.ContactID,
                ConsentAgreementConsentID = consent.ConsentID,
                ConsentAgreementTime = dateTimeNowService.GetDateTimeNow(),
                ConsentAgreementConsentHash = consent.ConsentHash,
                ConsentAgreementRevoked = false,
            };

            consentAgreement.Insert();
            ClearCache(contact.ContactID, consent.ConsentID);

            return consentAgreement;
        }


        /// <summary>
        /// Indicates whether the given <see cref="ContactInfo">contact</see> has agreed the specified <see cref="ConsentInfo">consent</see>.
        /// </summary>
        /// <param name="contact">Contact.</param>
        /// <param name="consent">Consent.</param>
        /// <returns>Returns <c>true</c> when the given contact agreed the specified consent and is still valid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> is <c>null</c> -or <paramref name="consent"/> is <c>null</c></exception>
        public bool IsAgreed(ContactInfo contact, ConsentInfo consent)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }
            if (consent == null)
            {
                throw new ArgumentNullException(nameof(consent));
            }

            string key = GetCacheKey(contact.ContactID, consent.ConsentID);

            return CacheHelper.Cache(
                cacheSettings =>
                {
                    var consentAgreement = GetLastConsentAgreement(contact.ContactID, consent.ConsentID);
                    return consentAgreement != null && !consentAgreement.ConsentAgreementRevoked;
                }, new CacheSettings(10, true, cacheItemNameParts: key));
        }


        /// <summary>
        /// Revokes an agreement of the given <see cref="ConsentInfo">consent</see> for the given <see cref="ContactInfo">contact</see>.
        /// </summary>
        /// <param name="contact">Contact</param>
        /// <param name="consent">Consent</param>
        /// <returns>Returns the agreement object that has been revoked.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> is <c>null</c> -or <paramref name="consent"/> is <c>null</c></exception>
        public ConsentAgreementInfo Revoke(ContactInfo contact, ConsentInfo consent)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }
            if (consent == null)
            {
                throw new ArgumentNullException(nameof(consent));
            }

            var consentAgreement = new ConsentAgreementInfo
            {
                ConsentAgreementContactID = contact.ContactID,
                ConsentAgreementConsentID = consent.ConsentID,
                ConsentAgreementTime = dateTimeNowService.GetDateTimeNow(),
                ConsentAgreementRevoked = true
            };

            consentAgreement.Insert();
            ClearCache(contact.ContactID, consent.ConsentID);

            DataProtectionEvents.RevokeConsentAgreement.StartEvent(new RevokeConsentAgreementEventArgs()
            {
                ConsentAgreement = consentAgreement,
                Consent = consent,
                Contact = contact
            });

            return consentAgreement;
        }


        /// <summary>
        /// Returns agreed consents for the given contact.
        /// </summary>
        /// <param name="contact">Contact.</param>
        /// <remarks>Only returns agreed consents, revoked consents are not included.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contact"/> is <c>null</c>.</exception>
        public IEnumerable<Consent> GetAgreedConsents(ContactInfo contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            var contactWhere = new WhereCondition().WhereEquals("ConsentAgreementContactID", contact.ContactID);

            return GetActiveAgreementsQuery(contactWhere, "ConsentAgreementConsentID")
                    .Source(s => s.Join<ConsentInfo>("SubData.ConsentAgreementConsentID", "ConsentID"))
                    .Source(s => s.LeftJoin<ConsentArchiveInfo>("SubData.ConsentAgreementConsentHash", "ConsentArchiveHash"))
                    .Select(row => ConstructConsent(row));
        }


        private Consent ConstructConsent(DataRow row)
        {
            var consentInfo = new ConsentInfo(row);
            var consentAgreementInfo = new ConsentAgreementInfo(row);
            var consentArchiveInfo = new ConsentArchiveInfo(row);

            string consentContent = consentArchiveInfo.ConsentArchiveID == 0 ?
                consentInfo.ConsentContent :
                consentArchiveInfo.ConsentArchiveContent;   

            return new Consent
            {
                Id = consentInfo.ConsentID,
                Name = consentInfo.ConsentName,
                DisplayName = consentInfo.ConsentDisplayName,
                Content = consentContent,
                Hash = consentAgreementInfo.ConsentAgreementConsentHash
            };
        }


        /// <summary>
        /// Gets an object query that returns contact IDs who agreed to the specified consent ad their agreement is still active (has not been revoked).
        /// </summary>
        internal static ObjectQuery<ConsentAgreementInfo> GetContactIDsWhoAgreed(ConsentInfo consent)
        {
            // Retrieve consent agreements only for the specified consent
            var where = new WhereCondition().WhereEquals("ConsentAgreementConsentID", consent.ConsentID);
            var idColumnName = "ConsentAgreementContactID";

            return GetActiveAgreementsQuery(where, idColumnName).Column(idColumnName);
        }


        /// <summary>
        /// Gets a query that returns only those agreements which have not been revoked.
        /// </summary>
        /// <param name="where">Specify the restricting conditions for the query</param>
        /// <param name="partitionByColumnName">Name of the column by which the order gets partitioned by.</param>
        /// <remarks>
        /// Note: If you need to restrict the scope of agreements (specific contact, specific consent ...), provide this restriction in the <paramref name="where" /> parameter.
        /// That way the generated query will use the restriction in the inner select, therefore will provide better performance.
        /// </remarks>
        private static ObjectQuery<ConsentAgreementInfo> GetActiveAgreementsQuery(WhereCondition where, string partitionByColumnName)
        {
            string LATEST_CONSENT_ACTION_ROW_NUMBER = "CMS_CA";

            return ConsentAgreementInfoProvider.GetConsentAgreements()
                            .Where(where)
                            .AddColumn(
                                new RowNumberColumn(LATEST_CONSENT_ACTION_ROW_NUMBER, "ConsentAgreementTime DESC")
                                {
                                    PartitionBy = partitionByColumnName
                                }
                            )
                            .AsNested()
                        .WhereEquals(LATEST_CONSENT_ACTION_ROW_NUMBER, 1)
                        .WhereEquals("ConsentAgreementRevoked", 0);
        }


        /// <summary>
        /// Gets a last consent agreement that has been created.
        /// </summary>
        /// <returns>Returns latest consent agreement object or <c>null</c> when a consent agreement is not found.</returns>
        internal static ConsentAgreementInfo GetLastConsentAgreement(int contactId, int consentId)
        {
            return ConsentAgreementInfoProvider.GetConsentAgreements()
                                               .WhereEquals("ConsentAgreementContactID", contactId)
                                               .WhereEquals("ConsentAgreementConsentID", consentId)
                                               .OrderByDescending("ConsentAgreementTime")
                                               .TopN(1)
                                               .FirstObject;
        }


        private static void ClearCache(int contactID, int consentID)
        {
            string key = GetCacheKey(contactID, consentID);
            CacheHelper.ClearCache(key);
        }


        private static string GetCacheKey(int contactID, int consentID)
        {
            return CacheHelper.GetCacheItemName(null, "ConsentAgreement", contactID, consentID).ToLowerInvariant();
        }
    }
}
