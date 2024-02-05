using System;
using System.Transactions;

using CMS.Core;
using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Transaction scope. Ensures that all queries run within single transaction.
    /// </summary>
    public class CMSTransactionScope : Trackable<CMSTransactionScope>, ITransactionScope, INotCopyThreadItem
    {
        #region "Variables"

        /// <summary>
        /// If true, .NET transaction scope is used instead of standard transactions.
        /// </summary>
        protected static bool? mUseTransactionScope = null;

        /// <summary>
        /// Transaction scope.
        /// </summary>
        protected TransactionScope mTransactionScope = null;

        /// <summary>
        /// Inner connection scope.
        /// </summary>
        protected CMSConnectionScope mConnectionScope = null;

        /// <summary>
        /// This scope closes the connection.
        /// </summary>
        protected bool mCloseConnection = false;

        /// <summary>
        /// This scope commits the transaction.
        /// </summary>
        protected bool mCommitTransaction = false;

        /// <summary>
        /// This scope should rollback the transaction in case of errors.
        /// </summary>
        protected bool mRollbackTransaction = false;

        #endregion


        #region "Properties"

        /// <summary>
        /// Connection of the current scope.
        /// </summary>
        public IDataConnection Connection
        {
            get
            {
                return mConnectionScope.Connection;
            }
        }


        /// <summary>
        /// If true, .NET transaction scope is used instead of standard transactions.
        /// </summary>
        public static bool UseTransactionScope
        {
            get
            {
                if (mUseTransactionScope == null)
                {
                    mUseTransactionScope = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseTransactionScope"], false);
                }

                return mUseTransactionScope.Value;
            }
            set
            {
                mUseTransactionScope = value;
            }
        }


        /// <summary>
        /// Returns true if the current code executes in a scope of open transaction
        /// </summary>
        public static bool IsInTransaction
        {
            get
            {
                // Get current connection scope
                var conn = ConnectionContext.CurrentConnectionScope;
                if (conn != null)
                {
                    // Check if the connection is in open transaction
                    return conn.Connection.IsTransaction();
                }

                return false;
            }
        }

        #endregion


        #region "Methods"
        
        /// <summary>
        /// Constructor. Opens the connection and begins the transaction.
        /// </summary>
        /// <param name="conn">Connection to use within all database operations</param>
        public CMSTransactionScope(IDataConnection conn = null)
        {
            // Use the transaction scope for default data provider
            if (UseTransactionScope)
            {
                mTransactionScope = new TransactionScope();
            }

            // Create connection scope with open connection
            mConnectionScope = new CMSConnectionScope(conn).Open();

            // Start transaction if necessary
            if (!UseTransactionScope)
            {
                // Use standard commit / rollback
                if (!Connection.IsTransaction())
                {
                    Connection.BeginTransaction();

                    mCommitTransaction = true;
                    mRollbackTransaction = true;
                }
            }
        }


        /// <summary>
        /// Disposes the object.
        /// </summary>
        public override void Dispose()
        {
            // Use standard commit / rollback
            if (!UseTransactionScope)
            {
                // Rollback the transaction if necessary
                if (mRollbackTransaction)
                {
                    mRollbackTransaction = false;

                    try
                    {
                        // Rollback transaction only if connection is open
                        if (Connection.IsOpen())
                        {
                            Connection.RollbackTransaction();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Connection is no longer usable, attempt to recover the connection from error
                        ConnectionContext.CurrentConnectionScope.ResetConnection();

                        // Log error to event log
                        CoreServices.EventLog.LogException("Transaction", "ROLLBACK", ex);
                    }
                }
                else if (mCommitTransaction)
                {
                    mCommitTransaction = false;
                    Connection.CommitTransaction();
                }
            }

            // Dispose the connection scope
            mConnectionScope.Dispose();
            mConnectionScope = null;

            // Dispose the transaction scope
            if (UseTransactionScope)
            {
                mTransactionScope.Dispose();
                mTransactionScope = null;
            }

            base.Dispose();
        }


        /// <summary>
        /// Commits the transaction.
        /// </summary>
        public void Commit()
        {
            if (UseTransactionScope)
            {
                // Complete the scope
                mTransactionScope.Complete();
            }
            else
            {
                mRollbackTransaction = false;

                // Commit the transaction if necessary
                if (mCommitTransaction)
                {
                    mCommitTransaction = false;
                    Connection.CommitTransaction();
                }
            }
        }


        /// <summary>
        /// Commits current transaction and begins new transaction.
        /// </summary>
        public void CommitAndBeginNew()
        {
            if (UseTransactionScope)
            {
                // Complete current transaction scope
                mTransactionScope.Complete();
                mTransactionScope.Dispose();

                // Open new transaction scope
                mTransactionScope = new TransactionScope();
            }
            else
            {
                // Use standard commit / rollback
                if (mCommitTransaction)
                {
                    mCommitTransaction = false;
                    mRollbackTransaction = false;

                    // Commit
                    Connection.CommitTransaction();

                    // Begin new transaction
                    Connection.BeginTransaction();
                    mCommitTransaction = true;
                    mRollbackTransaction = true;
                }
            }
        }
        
        #endregion
    }
}