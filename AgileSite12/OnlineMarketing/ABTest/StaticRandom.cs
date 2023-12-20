using System;
using System.Threading;

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Class that provides <see cref="Random"/> generator from thread local storage. This class is preferred to use, because <see cref="Random"/> class is not
    /// thread-safe.
    /// </summary>
    public class StaticRandom
    {
        private static readonly ThreadLocal<Random> ThreadLocalRandom = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        /// <summary>
        /// Gets instance for generator. Differs for each thread.
        /// </summary>
        private static Random Instance
        {
            get
            {
                return ThreadLocalRandom.Value;
            }
        }


        /// <summary>
        /// Returns a nonnegative random number less than the specified maximum.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero, and less than <paramref name="maxValue"/>; that is, the range of return values ordinarily includes zero but not <paramref name="maxValue"/>. However, if <paramref name="maxValue"/> equals zero, <paramref name="maxValue"/> is returned.
        /// </returns>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue"/> must be greater than or equal to zero. </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than zero. </exception>
        public static int Next(int maxValue)
        {
            return Instance.Next(maxValue);
        }


        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/> but not <paramref name="maxValue"/>. If <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.
        /// </returns>
        /// <param name="minValue">The inclusive lower bound of the random number returned. </param><param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>. </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>. </exception>
        public static int Next(int minValue, int maxValue)
        {
            return Instance.Next(minValue, maxValue);
        }
    }
}
