using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.SalesForce
{

    internal sealed class ReplicationLog
    {

        #region "Private members"

        private List<string> mEntries;
        private bool mHasDatabaseErrors;
        private bool mHasSalesForceErrors;

        #endregion

        #region "Public properties"

        public IEnumerable<string> Entries
        {
            get
            {
                return mEntries.AsEnumerable();
            }
        }

        public bool HasDatabaseErrors
        {
            get
            {
                return mHasDatabaseErrors;
            }
        }

        public bool HasSalesForceErrors
        {
            get
            {
                return mHasSalesForceErrors;
            }
        }

        public bool HasErrors
        {
            get
            {
                return HasDatabaseErrors || HasSalesForceErrors;
            }
        }

        #endregion

        #region "Constructors"

        public ReplicationLog()
        {
            mEntries = new List<string>();
        }

        #endregion

        #region "Public methods"

        public void AppendDatabaseError(string format, params object[] args)
        {
            string message = String.Format(format, args);
            mEntries.Add(message);
            mHasDatabaseErrors = true;
        }

        public void AppendSalesForceError(IEntityCommandResult result, string format, params object[] args)
        {
            StringBuilder message = new StringBuilder();
            message.AppendFormat(format, args);
            foreach (Error error in result.Errors)
            {
                message.AppendLine().AppendFormat("  - {0} ({1})", error.Message, Enum.GetName(typeof(StatusCode), error.StatusCode));
            }
            mEntries.Add(message.ToString());
            mHasSalesForceErrors = true;
        }

        public void AppendInformation(string format, params object[] args)
        {
            string message = String.Format(format, args);
            mEntries.Add(message);
        }

        public void Clear()
        {
            mEntries.Clear();
            mHasDatabaseErrors = false;
            mHasSalesForceErrors = false;
        }

        #endregion

    }

}