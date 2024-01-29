using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.DataProtection
{
    /// <summary>
    /// Class providing <see cref="ConsentAgreementInfo"/> management.
    /// </summary>
    public class ConsentAgreementInfoProvider : AbstractInfoProvider<ConsentAgreementInfo, ConsentAgreementInfoProvider>
    {
        /// <summary>
        /// Returns all <see cref="ConsentAgreementInfo"/> bindings.
        /// </summary>
        public static ObjectQuery<ConsentAgreementInfo> GetConsentAgreements()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets specified <see cref="ConsentAgreementInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="ConsentAgreementInfo"/> to set.</param>
        public static void SetConsentAgreementInfo(ConsentAgreementInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="ConsentAgreementInfo"/> binding.
        /// </summary>
        /// <param name="infoObj"><see cref="ConsentAgreementInfo"/> object.</param>
        public static void DeleteConsentAgreementInfo(ConsentAgreementInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Moves all consent agreements from the contact identified by given <paramref name="sourceContactID"/> to the contact identified by <paramref name="targetContactID"/>.
        /// </summary>
        /// <remarks>
        /// This method should be used only in the merging process. Note that there is no consistency check on whether the contacts with given IDs exist or not (nor is the 
        /// foreign key check in DB). Caller of this method should perform all the necessary checks prior to the method invocation.
        /// </remarks>
        /// <param name="sourceContactID">Identifier of the contact the activities are moved from</param>
        /// <param name="targetContactID">Identifier of the contact the activities are moved to</param>
        internal static void BulkMoveConsentAgreements(int sourceContactID, int targetContactID)
        {
            var updateDictionary = new Dictionary<string, object>
            {
                {"ConsentAgreementContactID", targetContactID}
            };

            var whereCondition = new WhereCondition().WhereEquals("ConsentAgreementContactID", sourceContactID);

            ProviderObject.UpdateData(whereCondition, updateDictionary);
        }

    }
}