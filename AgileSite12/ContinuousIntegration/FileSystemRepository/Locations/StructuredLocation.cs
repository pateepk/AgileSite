using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Collection of additional locations associated with object stored in <see cref="MainLocation"/>.
    /// </summary>
    [DebuggerDisplay("MainLocation = {MainLocation}, AdditionalLocations = {Count}")]
    [DebuggerTypeProxy(typeof(StructuredLocaltionDebugView))]
    public class StructuredLocation : SortedSet<string>, IEquatable<StructuredLocation>
    {
        /// <summary>
        /// Path to the XML where the majority of an object is stored.
        /// This paths identifies to what object additional locations belong to.
        /// </summary>
        public string MainLocation
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates empty new instance of <see cref="StructuredLocation"/> for provided <paramref name="mainLocation"/>.
        /// </summary>
        /// <param name="mainLocation">Paths that identifies to what object additional locations belong to.</param>
        public StructuredLocation(string mainLocation)
            : base(StringComparer.InvariantCultureIgnoreCase)
        {
            MainLocation = mainLocation;
        }



        /// <summary>
        /// Creates new instance of <see cref="StructuredLocation"/> for provided <paramref name="mainLocation"/>
        /// and fills it with additional locations from provided <paramref name="additionalLocations"/>.
        /// </summary>
        /// <param name="mainLocation">Paths that identifies to what object additional locations belong to.</param>
        /// <param name="additionalLocations">Collection of paths to additional parts of the object stored in the <paramref name="mainLocation"/>.</param>
        public StructuredLocation(string mainLocation, IEnumerable<string> additionalLocations)
            : base(additionalLocations, StringComparer.InvariantCultureIgnoreCase)
        {
            MainLocation = mainLocation;
        }


        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(StructuredLocation other)
        {
            return MainLocation.Equals(other.MainLocation, StringComparison.OrdinalIgnoreCase) && this.SequenceEqual(other);
        }


        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            var other = obj as StructuredLocation;
            return (other != null) && Equals(other);
        }


        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return MainLocation.GetHashCode() + this.Aggregate(4, (current, item) => current * 46 + item.GetHashCode());
            }
        }
    }
}
