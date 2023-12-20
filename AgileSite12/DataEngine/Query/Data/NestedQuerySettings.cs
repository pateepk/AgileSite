namespace CMS.DataEngine
{
    /// <summary>
    /// Settings to define how a nested query should be created
    /// </summary>
    public class NestedQuerySettings
    {
        private bool mEnsureOrderByColumns = true;
        private bool mMovePagingToOuterQuery = true;
        private bool mUseSameDataSet = true;


        /// <summary>
        /// If true, order by columns are translated into output columns CMS_O1, CMS_O2 etc. so that the data is available in the outer query. Default true
        /// </summary>
        public bool EnsureOrderByColumns
        {
            get
            {
                return mEnsureOrderByColumns;
            }
            set
            {
                mEnsureOrderByColumns = value;
            }
        }


        /// <summary>
        /// If true, paging parameters are moved to the outer query. Default true 
        /// </summary>
        internal bool MovePagingToOuterQuery
        {
            get
            {
                return mMovePagingToOuterQuery;
            }
            set
            {
                mMovePagingToOuterQuery = value;
            }
        }


        /// <summary>
        /// If true, outer query uses the same DataSet as the inner query to provide the same type of result by default. Default true
        /// </summary>
        internal bool UseSameDataSet
        {
            get
            {
                return mUseSameDataSet;
            }
            set
            {
                mUseSameDataSet = value;
            }
        }
    }
}