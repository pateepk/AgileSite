using System;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class representing range of values.
    /// </summary>
    public class Range<T> where T : IComparable
    {
        #region "Variables"

        private readonly T from;
        private readonly T to;

        #endregion


        #region "Methods"

        /// <summary>
        /// Class representing range of values.
        /// </summary>
        /// <param name="endpoint1">One endpoint of given range</param>
        /// <param name="endpoint2">Second endpoint of given range</param>
        public Range(T endpoint1, T endpoint2)
        {
            if (endpoint1.CompareTo(endpoint2) < 0)
            {
                from = endpoint1;
                to = endpoint2;
            }
            else
            {
                from = endpoint2;
                to = endpoint1;
            }
        }


        /// <summary>
        /// Returns true if range intersects with another one.
        /// </summary>
        /// <param name="range">Second range to check for intersection</param>
        /// <returns>True if the two ranges intersects, false otherwise</returns>
        public bool IntersectsWith(Range<T> range)
        {
            if (range.from.CompareTo(from) == 0)
            {
                return true;
            }
            else if (range.from.CompareTo(from) > 0)
            {
                return range.from.CompareTo(to) < 0;
            }
            else
            {
                return range.to.CompareTo(from) > 0;
            }
        }

        #endregion
    }
}
