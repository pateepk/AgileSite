using System;
using System.Linq;
using System.Text;

namespace CMS.SocialMarketing
{
     /// <summary>
    /// Represents information about a configuration of Twitter.
    /// </summary>
    public class TwitterConfiguration
    {

        #region "Public properties"

        /// <summary>
        /// Length of the URL shortened by Twitter's shortener t.co.
        /// </summary>
        public int ShortUrlLength;


        /// <summary>
        /// Length of the URL shortened by Twitter's shortener t.co if https protocol is used.
        /// </summary>
        public int ShortUrlLengthHttps;

        #endregion


        #region "Private methods"

        /// <summary>
        /// Initializes a new instance of the TwitterConfiguration with default values.
        /// </summary>
        public TwitterConfiguration()
        {
            ShortUrlLength = 22;
            ShortUrlLengthHttps = 23;
        }

        #endregion
    
    }
}
