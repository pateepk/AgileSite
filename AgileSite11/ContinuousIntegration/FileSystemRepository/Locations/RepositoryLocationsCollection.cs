using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using CMS.DataEngine;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Class represents collection of file locations in the repository that are all used for deserialization of one object.
    /// </summary>
    /// <remarks>
    /// Serialization of common info object results in single location stored in the collection (path to its XML file).
    /// Collection might contain multiple <see cref="MainLocations"/> if the serialized object is wrapper (e.g. document).
    /// Once an object defines non-empty <see cref="ContinuousIntegrationSettings.SeparatedFields"/>, single main location will
    /// contain multiple locations for these separated will. Separated fields of a serialized info object can be found in
    /// <see cref="StructuredLocations"/> collection.
    /// </remarks>
    [DebuggerDisplay("Count = {mJoinedLocations.Count}")]
    [DebuggerTypeProxy(typeof(RepositoryLocationsCollectionDebugView))]
    public class RepositoryLocationsCollection : IEnumerable<string>, IEquatable<RepositoryLocationsCollection>, ICloneable
    {
        private readonly SortedSet<string> mJoinedLocations;
        private readonly Dictionary<string, StructuredLocation> mStructuredLocations;


        /// <summary>
        /// Collection of <see cref="StructuredLocation"/>s with main location of an object and
        /// set of all additional file locations, where other parts of the (main) object are stored.
        /// </summary>
        public IEnumerable<StructuredLocation> StructuredLocations
        {
            get
            {
                return mStructuredLocations.Values;
            }
        }


        /// <summary>
        /// Usually there is only one main location in this collection, unless stored object is
        /// a wrapper for multiple (wrapped) objects that exists separately.
        /// </summary>
        public IEnumerable<string> MainLocations
        {
            get
            {
                return mStructuredLocations.Keys;
            }
        }


        /// <summary>
        /// Creates a new instance of file locations collection.
        /// </summary>
        public RepositoryLocationsCollection()
            : this(Enumerable.Empty<string>())
        {
        }


        /// <summary>
        /// Creates a new instance of file locations collection with single <paramref name="location"/> within.
        /// </summary>
        /// <param name="location">File location.</param>
        public RepositoryLocationsCollection(string location)
            : this(new[] { location })
        {
        }


        /// <summary>
        /// Creates a new instance of file locations collection with given <paramref name="locations"/> within.
        /// </summary>
        /// <param name="locations">Enumeration of file locations.</param>
        public RepositoryLocationsCollection(IEnumerable<string> locations)
        {
            var locationList = locations.ToArray();
            mJoinedLocations = new SortedSet<string>(locationList, StringComparer.InvariantCultureIgnoreCase);
            mStructuredLocations = locationList.ToDictionary(mainLocation => mainLocation, mainLocation => new StructuredLocation(mainLocation), StringComparer.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Adds location to the collection.
        /// <para>If <paramref name="mainLocation"/> or <paramref name="additionalLocation"/> already exists, it is not added for second time.</para>
        /// <para>Single <paramref name="mainLocation"/> can have multiple different <paramref name="additionalLocation"/>s.</para>
        /// </summary>
        /// <param name="mainLocation">Location to add.</param>
        /// <param name="additionalLocation">Main <paramref name="mainLocation"/> can have additional files (for example separated field location is additional file to main object location).</param>
        /// <remarks>It is possible to add multiple different <paramref name="additionalLocation"/> by calling the <see cref="Add"/> method with same <paramref name="mainLocation"/> parameter multiple times.</remarks>
        public void Add(string mainLocation, string additionalLocation = null)
        {
            // Process main location
            mJoinedLocations.Add(mainLocation);            
            if (!mStructuredLocations.ContainsKey(mainLocation))
            {
                mStructuredLocations.Add(mainLocation, new StructuredLocation(mainLocation));
            }

            // Process additional location
            if (additionalLocation == null)
            {
                // Additional location not provided, no additional handling required
                return;
            }
            mJoinedLocations.Add(additionalLocation);
            mStructuredLocations[mainLocation].Add(additionalLocation);
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
                return mJoinedLocations.Aggregate(17, (current, item) => current * 23 + item.GetHashCode());
            }
        }


        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return Clone(x => x);
        }


        /// <summary>
        /// Creates a new object that is a copy of the current instance using <paramref name="locationTransform"/> function.
        /// </summary>
        /// <param name="locationTransform">Transformation function for location.</param>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <example>
        /// This code shows how to transform relative locations into absolute using <see cref="Clone(Func{string,string})"/> method.
        /// <code>
        /// // Obtain repository configuration
        /// FileSystemRepositoryConfiguration configuration = FileSystemRepositoryConfigurationBuilder.Build();
        /// 
        /// // Store relative paths for objects
        /// RepositoryLocationsCollection relativeLocations = new RepositoryLocationsCollection();
        /// /*...*/
        /// 
        /// // Obtain absolute paths from the relative ones
        /// RepositoryLocationsCollection absoluteLocations = relativeLocations.Close(relativePath => Path.Combine(configuration.RepositoryRootPath, relativePath));
        /// </code>
        /// </example>
        public object Clone(Func<string, string> locationTransform)
        {
            var clone = new RepositoryLocationsCollection();
            foreach (var structuredLocation in StructuredLocations)
            {
                var mainLocation = locationTransform(structuredLocation.MainLocation);
                clone.Add(mainLocation);
                foreach (var location in structuredLocation)
                {
                    clone.Add(mainLocation, locationTransform(location));
                }
            }

            return clone;
        }


        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(RepositoryLocationsCollection other)
        {
            return this.SequenceEqual(other);
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
            var other = obj as RepositoryLocationsCollection;
            return other != null && Equals(other);
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection of individual, unstructured (see <see cref="StructuredLocations"/>) locations.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection of individual, unstructured (see <see cref="StructuredLocations"/>) locations.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<string> GetEnumerator()
        {
            return mJoinedLocations.GetEnumerator();
        }
    }
}
