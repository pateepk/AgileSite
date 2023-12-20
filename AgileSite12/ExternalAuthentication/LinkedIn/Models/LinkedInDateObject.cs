using System;
using System.Runtime.Serialization;

using CMS.Helpers;

namespace CMS.ExternalAuthentication.LinkedIn
{
    /// <summary>
    /// Represents date according to LinkedIn API.
    /// </summary>
    [Serializable, DataContract]
    public class LinkedInDateObject
    {
        /// <summary>
        /// Day represented in integer. Valid range from 1 to 31 depending on month.
        /// </summary>
        [DataMember(Name = "day")]
        public int Day
        {
            get;
            set;
        }


        /// <summary>
        /// Month represented in integer. Valid range from 1 to 12.
        /// </summary>
        [DataMember(Name = "month")]
        public int Month
        {
            get;
            set;
        }


        /// <summary>
        /// Year represented in integer.
        /// </summary>
        [DataMember(Name = "year")]
        public int Year
        {
            get;
            set;
        }


        /// <summary>
        /// Converts the object to the nullable DateTime. Returns null if object represents incorrect date.
        /// </summary>
        /// <returns>DateTime object created from <see cref="Year"/>, <see cref="Month"/> and <see cref="Day"/>. If those values do not form a correct date, <see cref="DateTimeHelper.ZERO_TIME"/> is returned.</returns>
        public DateTime ToDateTime()
        {
            try
            {
                return new DateTime(Year, Month, Day);
            }
            catch (ArgumentOutOfRangeException)
            {
                return DateTimeHelper.ZERO_TIME;
            }
        }
    }
}
