using System;
using System.Collections.Generic;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Data class containing all fields required for asynchronous hit logging.
    /// </summary>
    public class LogHitParameters
    {
        /// <summary>
        /// Current request referrer.
        /// </summary>
        public string UrlReferrer
        {
            get;
            set;
        }


        /// <summary>
        /// Alias path to the document from which was the request made.
        /// </summary>
        public string NodeAliasPath
        {
            get;
            set;
        }


        /// <summary>
        /// Culture of the document from which was the request made.
        /// </summary>
        public string DocumentCultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the AB test to log visit for.
        /// </summary>
        public string ABTestName
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the currently selected AB test variant.
        /// </summary>
        public string ABTestVariantName
        {
            get;
            set;
        }


        /// <summary>
        /// Decides whether to log first visit or recurring visit for given AB test.
        /// </summary>
        public string ABTestFirstVisit
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the MV test combination to log visit for.
        /// </summary>
        public string MVTestCombinationName
        {
            get;
            set;
        }


        /// <summary>
        /// Wrapper around the other fields. This field is used as argument when raising log hit event.
        /// </summary>
        /// <remarks>
        /// Keys in dictionary do not match the fields name due to backward compatibility.
        /// </remarks>
        public Dictionary<string, string> PlainParameters
        {
            get
            {
                return new Dictionary<string, string>
                {
                    {"referrer", UrlReferrer},
                    {"pagenodealiaspath", NodeAliasPath},
                    {"pageculturecode", DocumentCultureCode},
                    {"ABTestName", ABTestName},
                    {"ABTestVariantName", ABTestVariantName},
                    {"ABTestFirstVisit", ABTestFirstVisit},
                    {"MVTestCombinationName", MVTestCombinationName}
                };
            }
        }
    }
}