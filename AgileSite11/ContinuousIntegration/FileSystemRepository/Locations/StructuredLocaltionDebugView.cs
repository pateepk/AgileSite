using System;
using System.Diagnostics;
using System.Linq;


namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Debug view for <see cref="StructuredLocation"/>.
    /// </summary>
    internal class StructuredLocaltionDebugView
    {
        private readonly string mMainLocation;
        private readonly string[] mAdditionalLocations;


        /// <summary>
        /// Path to the main location of an object.
        /// </summary>
        public string MainLocation
        {
            get
            {
                return mMainLocation;
            }
        }


        /// <summary>
        /// Collection of paths of the additional locations of an object.
        /// </summary>
        [DebuggerDisplay("Count = {mAdditionalLocations.Length}")]
        public string[] AdditionalLocations
        {
            get
            {
                return mAdditionalLocations;
            }
        }

        /// <summary>
        /// Initializes a new instance of debug view for <see cref="StructuredLocaltionDebugView"/>.
        /// </summary>
        /// <param name="location">Structured location collection for which to initialize the debug view.</param>
        public StructuredLocaltionDebugView(StructuredLocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }

            mMainLocation = location.MainLocation;
            mAdditionalLocations = location.ToArray();
        }
    }
}