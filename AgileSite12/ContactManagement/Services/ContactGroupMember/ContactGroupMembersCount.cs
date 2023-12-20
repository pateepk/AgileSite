namespace CMS.ContactManagement
{ 
    /// <summary>
    /// Class to link contact group with its count of members
    /// </summary>
    public class ContactGroupMembersCount
    {
        /// <summary>
        /// Contact group ID
        /// </summary>
        public int ContactGroupID
        {
            get;
            set;
        }


        /// <summary>
        /// Count of members
        /// </summary>
        public int MembersCount
        {
            get;
            set;
        }
    }
}
