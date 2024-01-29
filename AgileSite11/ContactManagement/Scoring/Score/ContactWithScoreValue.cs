namespace CMS.ContactManagement
{
    /// <summary>
    /// Represents number of points contact has in certain score.
    /// </summary>
    public class ContactWithScoreValue
    {
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
                return (ScoreID * 397) ^ ContactID;
            }
        }


        /// <summary>
        /// ID of the score the points belong to.
        /// </summary>
        public int ScoreID
        {
            get;
            set;
        }


        /// <summary>
        /// Number of points contact reached.
        /// </summary>
        public int ScoreValue
        {
            get;
            set;
        }


        /// <summary>
        /// ID of the contact.
        /// </summary>
        public int ContactID
        {
            get;
            set;
        }


        /// <summary>
        /// ID of the site.
        /// </summary>
        public int SiteID
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            var typedObj = (ContactWithScoreValue)obj;

            return (ScoreID == typedObj.ScoreID) && (ContactID == typedObj.ContactID);
        }
    }
}