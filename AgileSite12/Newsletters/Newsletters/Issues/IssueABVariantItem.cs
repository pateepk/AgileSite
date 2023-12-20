using System;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class contains issue ID and variant name pair.
    /// </summary>
    public class IssueABVariantItem
    {
        /// <summary>
        /// Issue ID.
        /// </summary>
        public int IssueID
        {
            get;
            set;
        }


        /// <summary>
        /// Variant name.
        /// </summary>
        public string IssueVariantName
        {
            get;
            set;
        }


        /// <summary>
        /// Flag indicates the winner
        /// </summary>
        public bool IsWinner
        {
            get;
            set;
        }


        /// <summary>
        /// Issue status
        /// </summary>
        public IssueStatusEnum IssueStatus
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public IssueABVariantItem()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="issueId">Issue ID</param>
        /// <param name="variantName">Variant name</param>
        /// <param name="isWinner">Winner</param>
        /// <param name="issueStatus">Issue status</param>
        public IssueABVariantItem(int issueId, string variantName, bool isWinner, IssueStatusEnum issueStatus)
        {
            IssueID = issueId;
            IssueVariantName = variantName;
            IsWinner = isWinner;
            IssueStatus = issueStatus;            
        }
    }
}
