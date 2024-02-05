using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Container for scores together with amount of points certain contact reached.
    /// </summary>
    public class ContactWithScoreValueCollection : IEnumerable<ContactWithScoreValue>
    {
        private List<ContactWithScoreValue> mInternalData;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scoreIDsWithPoints">Initial data</param>
        /// <exception cref="ArgumentNullException"><paramref name="scoreIDsWithPoints"/> is null</exception>
        public ContactWithScoreValueCollection(IEnumerable<ContactWithScoreValue> scoreIDsWithPoints)
        {
            if (scoreIDsWithPoints == null)
            {
                throw new ArgumentNullException("scoreIDsWithPoints");
            }

            mInternalData = scoreIDsWithPoints.ToList();
        }


        /// <summary>
        /// Removes items from the collection based on the given list of score IDs.
        /// </summary>
        /// <param name="scoreIDs">ScoreIDs used to remove items from the collection</param>
        /// <exception cref="ArgumentNullException"><paramref name="scoreIDs"/> is null</exception>
        public void RemoveItemsWithScoreId(IEnumerable<int> scoreIDs)
        {
            if (scoreIDs == null)
            {
                throw new ArgumentNullException("scoreIDs");
            }

            var set = new HashSet<int>(scoreIDs);
            var scoresToRemove = mInternalData.Where(ScoreIDWithValue => set.Contains(ScoreIDWithValue.ScoreID));
            mInternalData = mInternalData.Except(scoresToRemove).ToList();
        }


        /// <summary>
        /// Subtracts subtrahend from the collection.
        /// </summary>
        /// <param name="subtrahend">Collection to be subtracted from the current collection</param>
        /// <exception cref="ArgumentNullException"><paramref name="subtrahend"/> is null</exception>
        public void RelativeComplement(ContactWithScoreValueCollection subtrahend)
        {
            if (subtrahend == null)
            {
                throw new ArgumentNullException("subtrahend");
            }

            mInternalData = mInternalData.Except(subtrahend).ToList();
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator of KeyValuePair of int and int where key is ScoreID and value is number of points.
        /// </returns>
        public IEnumerator<ContactWithScoreValue> GetEnumerator()
        {
            return mInternalData.GetEnumerator();
        }


        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An enumerator of KeyValuePair of int and int where key is ScoreID and value is number of points.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}