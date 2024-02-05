
namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents a URL extracted by <see cref="URLParser"/>
    /// </summary>
    public sealed class URLParserMatch
    {
        /// <summary>
        /// Whole URL
        /// </summary>
        public string URL
        {
            get;
            set;
        }

        /// <summary>
        /// Protocol
        /// </summary>
        public string Protocol
        {
            get;
            set;
        }

        /// <summary>
        /// Domain
        /// </summary>
        public string Domain
        {
            get;
            set;
        }

    }

}