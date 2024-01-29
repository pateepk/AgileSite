using System;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class for Change contact account action
    /// </summary>
    public class ContactAccountAction : ContactAutomationAction
    {
        #region "Variables"

        private AccountInfo mAccount = null;
        private bool mAccountProcessed = false;
        private ContactRoleInfo mRole = null;
        private bool mRoleProcessed = false;

        #endregion


        #region "Parameters"

        /// <summary>
        /// Account identifier.
        /// </summary>
        public virtual Guid AccountGUID
        {
            get
            {
                return GetResolvedParameter<Guid>("Account", Guid.Empty);
            }
        }


        /// <summary>
        /// Contact role identifier.
        /// </summary>
        public virtual string RoleName
        {
            get
            {
                return GetResolvedParameter<string>("RoleName", string.Empty);
            }
        }


        /// <summary>
        /// Gets current action - 0 for ADD TO, 1 for REMOVE FROM account.
        /// </summary>
        public virtual int ContactAction
        {
            get
            {
                return GetResolvedParameter<int>("ContactAction", 0);
            }
        }

        #endregion


        #region "Properties"


        /// <summary>
        /// Role info object
        /// Null value possible
        /// </summary>
        protected virtual ContactRoleInfo Role
        {
            get
            {
                if ((mRole == null) && !String.IsNullOrEmpty(RoleName) && !mRoleProcessed)
                {
                    mRoleProcessed = true;
                    // Get contact role
                    mRole = ContactRoleInfoProvider.GetContactRoleInfo(RoleName);
                }
                return mRole;
            }
        }


        /// <summary>
        /// ID of role
        /// </summary>
        protected virtual int RoleID
        {
            get
            {
                return (Role != null) ? Role.ContactRoleID : 0;
            }
        }


        /// <summary>
        /// Account info object
        /// </summary>
        protected virtual AccountInfo Account
        {
            get
            {
                if ((mAccount == null) && !mAccountProcessed)
                {
                    mAccountProcessed = true;
                    mAccount = AccountInfoProvider.GetAccountInfo(AccountGUID);
                }
                return mAccount;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Inserts or updated relation between contact and account
        /// </summary>
        protected void SetAccountContactRelation()
        {
            AccountContactInfo relation = AccountContactInfoProvider.GetAccountContactInfo(Account.AccountID, Contact.ContactID);
            if (relation == null)
            {
                // Add contact to account
                AccountHelper.CreateAccountContactRelation(Account.AccountID, Contact.ContactID, RoleID);
            }
            else
            {
                // Update relation between contact and account
                relation.ContactRoleID = RoleID;
                AccountContactInfoProvider.SetAccountContactInfo(relation);
            }
        }

        #endregion


        /// <summary>
        /// Executes current action
        /// </summary>
        public override void Execute()
        {
            if ((Contact == null) || (Account == null))
            {
                return;
            }

            if (ContactAction == 1)
            {
                // Remove contact from account
                AccountContactInfoProvider.DeleteAccountContactInfo(Account.AccountID, Contact.ContactID);
            }
            else
            {
                // Create/update relation
                SetAccountContactRelation();
            }
        }
    }
}
