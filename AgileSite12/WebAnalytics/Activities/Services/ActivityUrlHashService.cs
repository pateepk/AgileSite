using System;
using System.Data.HashFunction.CRC;
using System.Text;

using CMS.WebAnalytics.Internal;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides hash computation for activity URLs using CRC.
    /// </summary>
    internal class ActivityUrlHashService : IActivityUrlHashService
    {
        private readonly IActivityUrlPreprocessor mUrlPreprocessor;
        private readonly ICRC crc = CRCFactory.Instance.Create(CRCConfig.CRC64);


        /// <summary>
        /// Initializes the new instance of the <see cref="ActivityUrlHashService"/> service.
        /// </summary>
        /// <param name="urlPreprocessor">Activity URL preprocessor</param>
        public ActivityUrlHashService(IActivityUrlPreprocessor urlPreprocessor)
        {
            mUrlPreprocessor = urlPreprocessor;
        }


        /// <summary>
        /// Computes a hash for the specified activity URL using CRC.
        /// URL is first processed by <see cref="IActivityUrlPreprocessor"/> specified in the constructor.
        /// </summary>
        /// <param name="activityUrl">Activity URL to compute hash for.</param>
        /// <returns>Hash corresponding to <paramref name="activityUrl"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="activityUrl"/> is null.</exception>
        public long GetActivityUrlHash(string activityUrl)
        {
            if (activityUrl == null)
            {
                throw new ArgumentNullException(nameof(activityUrl));
            }

            var preprocessedUrl = mUrlPreprocessor.PreprocessActivityUrl(activityUrl);

            return ComputeHash(preprocessedUrl);
        }


        private long ComputeHash(string input)
        {
            var dataToHash = Encoding.UTF8.GetBytes(input);
            return BitConverter.ToInt64(crc.ComputeHash(dataToHash).Hash, 0);
        }
    }
}
