using System;

namespace CMS.SocialMarketing
{

    /// <summary>
    /// Represents a range of object identifiers on a Twitter timeline.
    /// </summary>
    internal struct TwitterIdentifierRange
    {
        /// <summary>
        /// Gets the range start (inclusive).
        /// </summary>
        public ulong From
        {
            get;
        }

        /// <summary>
        /// Gets the range end (inclusive).
        /// </summary>
        public ulong To
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the TimelineRange struct.
        /// </summary>
        /// <param name="from">The range start (inclusive).</param>
        /// <param name="to">The range end (inclusive).</param>
        public TwitterIdentifierRange(ulong from, ulong to)
        {
            From = from;
            To = to;
        }

        /// <summary>
        /// Converts this range into a string.
        /// </summary>
        /// <returns>A string that represents this range.</returns>
        public override string ToString()
        {
            return string.Concat(From, ";", To);
        }

        /// <summary>
        /// Converts the string representation of a range to an equivalent range.
        /// </summary>
        /// <param name="input">A string containing a range to convert.</param>
        /// <returns>A range whose value is represented by the specified string.</returns>
        public static TwitterIdentifierRange Parse(string input)
        {
            string[] tokens = input.Split(';');

            return new TwitterIdentifierRange(UInt64.Parse(tokens[0]), UInt64.Parse(tokens[1]));
        }

    }

}