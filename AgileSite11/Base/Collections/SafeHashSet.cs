using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Thread-safe variant of a generic hash set
    /// </summary>
    /// <remarks>Even for reference types, null is not supported value in the safe hash set.</remarks>
    public class SafeHashSet<T> : ISet<T>, IEnumerable<T>, IEnumerable
    {
        private readonly ConcurrentDictionary<T, bool> mValues;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comparer">Equality comparer</param>
        public SafeHashSet(IEqualityComparer<T> comparer = null)
        {
            mValues = comparer != null
                ? new ConcurrentDictionary<T, bool>(comparer)
                : new ConcurrentDictionary<T, bool>();
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="collection">Source collection</param>
        /// <param name="comparer">Equality comparer</param>
        public SafeHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer = null)
        {
            var dictionary = collection.ToDictionary(x => x, _ => true);
            mValues = comparer != null
                ? new ConcurrentDictionary<T, bool>(dictionary, comparer)
                : new ConcurrentDictionary<T, bool>(dictionary);
        }


        /// <summary>
        /// Returns true if the hash set contains the given value
        /// </summary>
        /// <param name="item">Value to check</param>
        /// <exception cref="ArgumentNullException">If <paramref name="item" /> is null.</exception>
        public bool Contains(T item)
        {
            return mValues.ContainsKey(item);
        }


        /// <summary>
        /// Adds the item to the hash set
        /// </summary>
        /// <param name="item">Value to add</param>
        /// <returns>true if item added, false otherwise</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="item" /> is null.</exception>
        public bool Add(T item)
        {
            return mValues.TryAdd(item, true);
        }


        /// <summary>
        /// Adds the item to the hash set
        /// </summary>
        /// <param name="item">Value to add</param>
        /// <exception cref="ArgumentNullException">If <paramref name="item" /> is null.</exception>
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }


        /// <summary>
        /// Removes the value from the hash set
        /// </summary>
        /// <param name="item">Value to remove</param>
        /// <exception cref="ArgumentNullException">If <paramref name="item" /> is null.</exception>
        public bool Remove(T item)
        {
            bool val;
            return mValues.TryRemove(item, out val);
        }


        /// <summary>
        /// Removes all elements in the specified collection from the current <see cref="SafeHashSet{T}" /> object.
        /// </summary>
        /// <param name="other">The collection of items to remove from the <see cref="SafeHashSet{T}" /> object.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="other" /> is null.</exception>
        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            foreach (var oneOther in other)
            {
                Remove(oneOther);
            }
        }


        /// <summary>
        /// Modifies the current <see cref="SafeHashSet{T}" /> object to contain only elements that are present in that object and in the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="SafeHashSet{T}" /> object.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="other" /> is null.</exception>
        public void IntersectWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            var toRemove = mValues.Where(value => !other.Contains(value.Key)).Select(value => value.Key);
            ExceptWith(toRemove);
        }


        /// <summary>
        /// Determines whether a <see cref="SafeHashSet{T}" /> object is a proper subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="SafeHashSet{T}" /> object.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="other" /> is null.</exception>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            IEnumerable<T> otherEnumerated = other as T[] ?? other.ToArray();
            return IsSubsetOf(otherEnumerated) && (Count < otherEnumerated.Count());
        }


        /// <summary>
        /// Determines whether a <see cref="SafeHashSet{T}" /> object is a proper superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="SafeHashSet{T}" /> object.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="other" /> is null.</exception>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            IEnumerable<T> otherEnumerated = other as T[] ?? other.ToArray();
            return IsSupersetOf(otherEnumerated) && otherEnumerated.Count() < Count;
        }


        /// <summary>
        /// Determines whether a <see cref="SafeHashSet{T}" /> object is a subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="SafeHashSet{T}" /> object.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="other" /> is null.</exception>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return other.Where(Contains).SequenceEqual(this);
        }


        /// <summary>
        /// Determines whether a <see cref="SafeHashSet{T}" /> object is a superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="SafeHashSet{T}" /> object.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="other" /> is null.</exception>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            IEnumerable<T> otherEnumerated = other as T[] ?? other.ToArray();
            return !otherEnumerated.Any() || this.Where(otherEnumerated.Contains).SequenceEqual(otherEnumerated);
        }


        /// <summary>
        /// Determines whether the current <see cref="SafeHashSet{T}" /> object and a specified collection share common elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="SafeHashSet{T}" /> object.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="other" /> is null.</exception>
        public bool Overlaps(IEnumerable<T> other)
        {
            return other.Where(Contains).Any();
        }


        /// <summary>
        /// Determines whether a <see cref="SafeHashSet{T}" /> object and the specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="SafeHashSet{T}" /> object.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="other" /> is null.</exception>
        public bool SetEquals(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            var otherEnumerated = other as T[] ?? other.ToArray();
            if (!otherEnumerated.Any() && !this.Any())
            {
                return true;
            }

            return this.OrderBy(x => x).SequenceEqual(otherEnumerated.OrderBy(x => x));
        }


        /// <summary>
        /// Modifies the current <see cref="SafeHashSet{T}" /> object to contain only elements that are present either in that object or in the specified collection, but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="SafeHashSet{T}" /> object.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="other" /> is null.</exception>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            var toRemove = mValues.Where(value => other.Contains(value.Key)).Select(value => value.Key).ToArray();
            ExceptWith(toRemove);

            UnionWith(other.Where(item => !toRemove.Contains(item)));
        }


        /// <summary>
        /// Modifies the current <see cref="SafeHashSet{T}" /> object to contain all elements that are present in itself, the specified collection, or both.
        /// </summary>
        /// <param name="other">The collection to merge into the current <see cref="SafeHashSet{T}" /> object.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="other" /> is null.</exception>
        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            foreach (var oneOther in other)
            {
                mValues[oneOther] = true;
            }
        }


        /// <summary>
        /// Removes all elements from a <see cref="SafeHashSet{T}" /> object.
        /// </summary>
        public void Clear()
        {
            mValues.Clear();
        }


        /// <summary>
        /// Copies the elements of a <see cref="SafeHashSet{T}" /> object to an array, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="SafeHashSet{T}" /> object. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="array" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="arrayIndex" /> is less than the lower bound of <paramref name="array" />.</exception>
        /// <exception cref="ArgumentException">If the number of elements in set is greater than the available number of elements from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            mValues.Keys.CopyTo(array, arrayIndex);
        }


        /// <summary>
        /// Returns the number of elements in a sequence.
        /// </summary>
        public int Count
        {
            get
            {
                return mValues.Count;
            }
        }


        /// <summary>
        /// Gets a value indicating whether a collection is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return mValues.Keys.IsReadOnly;
            }
        }


        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return mValues.Keys.GetEnumerator();
        }


        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return mValues.Keys.GetEnumerator();
        }
    }
}
