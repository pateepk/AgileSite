using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class providing ABTestInfo management.
    /// </summary>
    public class ABTestInfoProvider : AbstractInfoProvider<ABTestInfo, ABTestInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public ABTestInfoProvider()
            : base(ABTestInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// States indexed by state code.
        /// </summary>
        private static CMSStatic<ProviderInfoDictionary<int>> mABTestInfosByIssueID = new CMSStatic<ProviderInfoDictionary<int>>();


        /// <summary>
        /// Table lock for loading.
        /// </summary>
        private static object tableLock = new object();


        /// <summary>
        /// AB tests indexed by parent issue ID.
        /// </summary>
        private static ProviderInfoDictionary<int> ABTestInfosByIssueID
        {
            get
            {
                return mABTestInfosByIssueID;
            }
            set
            {
                mABTestInfosByIssueID.Value = value;
            }
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ABTestInfo objects.
        /// </summary>
        public static ObjectQuery<ABTestInfo> GetABTests()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns abtest with specified ID.
        /// </summary>
        /// <param name="abtestId">Abtest ID.</param>        
        public static ABTestInfo GetABTestInfo(int abtestId)
        {
            return ProviderObject.GetInfoById(abtestId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified abtest.
        /// </summary>
        /// <param name="abtestObj">Abtest to be set.</param>
        public static void SetABTestInfo(ABTestInfo abtestObj)
        {
            ProviderObject.SetInfo(abtestObj);
        }


        /// <summary>
        /// Deletes specified abtest.
        /// </summary>
        /// <param name="abtestObj">Abtest to be deleted.</param>
        public static void DeleteABTestInfo(ABTestInfo abtestObj)
        {
            ProviderObject.DeleteInfo(abtestObj);
        }


        /// <summary>
        /// Deletes abtest with specified ID.
        /// </summary>
        /// <param name="abtestId">Abtest ID.</param>
        public static void DeleteABTestInfo(int abtestId)
        {
            ABTestInfo abtestObj = GetABTestInfo(abtestId);
            DeleteABTestInfo(abtestObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns ABTest info for the given issue (parent issue).
        /// </summary>
        /// <param name="issueId">Parent issue ID</param>        
        public static ABTestInfo GetABTestInfoForIssue(int issueId)
        {
            return ProviderObject.GetABTestInfoForIssueInternal(issueId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ABTestInfo info)
        {
            if (info != null)
            {
                // Load hashtables
                LoadTests();

                // Save changes
                base.SetInfo(info);

                // Update hashtables
                ABTestInfosByIssueID.Update(info.TestIssueID, info);
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ABTestInfo info)
        {
            if (info != null)
            {
                // Load hashtables
                LoadTests();

                // Delete info
                base.DeleteInfo(info);

                // Update hashtables
                ABTestInfosByIssueID.Delete(info.TestIssueID);
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            // Clear AB tests
            lock (tableLock)
            {
                if (ABTestInfosByIssueID != null)
                {
                    ABTestInfosByIssueID.Clear(logTasks);
                }
            }
        }


        /// <summary>
        /// Returns ABTest info for the given issue (parent issue).
        /// </summary>
        /// <param name="issueId">Parent issue ID</param>        
        public virtual ABTestInfo GetABTestInfoForIssueInternal(int issueId)
        {
            ABTestInfo test = null;

            if (issueId > 0)
            {
                // Load hashtable
                LoadTests();

                // Try to get test from hashtable
                test = (ABTestInfo)ABTestInfosByIssueID[issueId];
                if (test == null)
                {
                    // Get test from DB when not found
                    test = GetObjectQuery().WhereEquals("TestIssueID", issueId).TopN(1).FirstOrDefault();
                    if (test != null)
                    {
                        // Update hashtables
                        ABTestInfosByIssueID.Update(test.TestIssueID, test);
                    }
                }
            }

            return test;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Loads tests to hashtable.
        /// </summary>
        private static void LoadTests()
        {
            if (ProviderHelper.LoadTables(ABTestInfosByIssueID))
            {
                lock (tableLock)
                {
                    if (ProviderHelper.LoadTables(ABTestInfosByIssueID))
                    {
                        // Prepare the tables
                        var tempTests = new ProviderInfoDictionary<int>(ABTestInfo.OBJECT_TYPE, "TestIssueID");

                        // Load the data
                        if (ProviderHelper.LoadHashTables(ABTestInfo.OBJECT_TYPE, LoadHashtableEnum.All) != LoadHashtableEnum.None)
                        {
                            var tests = GetABTests();
                            // Add tests to hashtable
                            foreach (var test in tests)
                            {
                                tempTests[test.TestIssueID] = test;
                            }
                        }

                        ABTestInfosByIssueID = tempTests;
                    }
                }
            }
        }

        #endregion
    }
}
