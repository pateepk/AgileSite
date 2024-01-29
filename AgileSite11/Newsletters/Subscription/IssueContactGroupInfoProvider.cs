using System.Linq;

using CMS.ContactManagement;
using CMS.DataEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class providing IssueContactGroupInfo management.
    /// </summary>
    public class IssueContactGroupInfoProvider : AbstractInfoProvider<IssueContactGroupInfo, IssueContactGroupInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public IssueContactGroupInfoProvider()
            : base(IssueContactGroupInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the IssueContactGroupInfo objects.
        /// </summary>
        public static ObjectQuery<IssueContactGroupInfo> GetIssueContactGroups()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns IssueContactGroupInfo for given <see cref="IssueInfo"/> and <see cref="ContactGroupInfo"/>.
        /// </summary>
        /// <param name="issueID">IssueInfo ID</param>
        /// <param name="contactGroupID">ContactGroupInfo ID</param>
        public static IssueContactGroupInfo GetIssueContactGroupInfo(int issueID, int contactGroupID)
        {
            return ProviderObject.GetIssueContactGroupInfoInternal(issueID, contactGroupID);
        }


        /// <summary>
        /// Sets (updates or inserts) specified IssueContactGroupInfo.
        /// </summary>
        /// <param name="infoObj">IssueContactGroupInfo to be set</param>
        public static void SetIssueContactGroupInfo(IssueContactGroupInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified IssueContactGroupInfo.
        /// </summary>
        /// <param name="infoObj">IssueContactGroupInfo to be deleted</param>
        public static void DeleteIssueContactGroupInfo(IssueContactGroupInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes IssueContactGroupInfo for given <see cref="IssueInfo"/> and <see cref="ContactGroupInfo"/>.
        /// </summary>
        /// <param name="issueID">IssueInfo ID</param>
        /// <param name="contactGroupID">ContactGroupInfo ID</param>
        public static void DeleteIssueContactGroupInfo(int issueID, int contactGroupID)
        {
            ProviderObject.DeleteIssueContactGroupInfoInternal(issueID, contactGroupID);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns IssueContactGroupInfo for given <see cref="IssueInfo"/> and <see cref="ContactGroupInfo"/>.
        /// </summary>
        /// <param name="issueID">IssueInfo ID</param>
        /// <param name="contactGroupID">ContactGroupInfo ID</param>
        protected virtual IssueContactGroupInfo GetIssueContactGroupInfoInternal(int issueID, int contactGroupID)
        {
            return GetIssueContactGroups().WhereEquals("IssueID", issueID).WhereEquals("ContactGroupID", contactGroupID).TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Deletes IssueContactGroupInfo for given <see cref="IssueInfo"/> and <see cref="ContactGroupInfo"/>.
        /// </summary>
        /// <param name="issueID">IssueInfo ID</param>
        /// <param name="contactGroupID">ContactGroupInfo ID</param>      
        protected virtual void DeleteIssueContactGroupInfoInternal(int issueID, int contactGroupID)
        {
            IssueContactGroupInfo infoObj = ProviderObject.GetIssueContactGroupInfoInternal(issueID, contactGroupID);
            DeleteIssueContactGroupInfo(infoObj);
        }

        #endregion
    }
}