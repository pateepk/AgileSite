using System.Collections;

using CMS.Helpers;
using CMS.Base;

namespace CMS.Forums
{
    /// <summary>
    /// Class providing support for thread view counting.
    /// </summary>
    public static class ThreadViewCounter
    {
        /// <summary>
        /// Hashtable with ThreadID-ViewNumber values.
        /// </summary>
        public static Hashtable ThreadViewCount = new Hashtable();


        /// <summary>
        /// Number of threads in the hashtable which causes automatic update in DB.
        /// </summary>
        public static int MaxRecords = 100;


        /// <summary>
        /// Increments the number of views of specified post.
        /// </summary>
        /// <param name="postId">ID of the post the views of which are incremented</param>
        public static void LogThreadView(int postId)
        {
            bool saveDirectlyToDB = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSSaveThreadViewsDirectToDB"], false);

            if (saveDirectlyToDB)
            {
                ForumPostInfoProvider.IncrementPostViews(postId, 1);
            }
            else
            {
                ThreadViewCount[postId] = ValidationHelper.GetInteger(ThreadViewCount[postId], 0) + 1;
                if (ThreadViewCount.Count == MaxRecords)
                {
                    SaveViewCounts();
                }
            }
        }


        /// <summary>
        /// Saves the content of the hashtable to DB.
        /// </summary>
        public static void SaveViewCounts()
        {
            // Clone existing table to temporary and clear the main table
            Hashtable tempTable = new Hashtable();
            lock (ThreadViewCount)
            {
                foreach (DictionaryEntry entry in ThreadViewCount)
                {
                    tempTable[entry.Key] = entry.Value;
                }
                ThreadViewCount.Clear();
            }

            // Update the DB records
            foreach (DictionaryEntry entry in tempTable)
            {
                ForumPostInfoProvider.IncrementPostViews((int)entry.Key, (int)entry.Value);
            }
        }
    }
}