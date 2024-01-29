using System;
using System.Text.RegularExpressions;

using CMS.Helpers;

namespace CMS.SocialMarketing
{

    /// <summary>
    /// Provides methods for replacing URLs in a text.
    /// </summary>
    public sealed class URLParser
    {

        #region "Variables"

        /// <summary>
        /// The URL regular expression.
        /// </summary>
        private Regex mURLRegex = null;

        #endregion


        #region "Private properties"

        /// <summary>
        /// Gets the URL regular expression.
        /// </summary>
        private Regex URLRegex
        {
            get
            {
                if (mURLRegex == null)
                {
                    mURLRegex = RegexHelper.GetRegex(VALID_URL_PATTERN_STRING, RegexOptions.IgnoreCase);
                }
                return mURLRegex;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// In a specified input string, replaces all URLs with a string returned by an evaluator.
        /// </summary>
        /// <param name="input">The string to search for URLs.</param>
        /// <param name="evaluator">A custom method that examines each match and returns a replacement string.</param>
        /// <returns>A new string that is identical to the input string, except that a replacement string takes the place of each matched URL.</returns>
        public string Replace(string input, Func<URLParserMatch, string> evaluator)
        {
            return URLRegex.Replace(input, x => EvaluateMatch(x, evaluator));
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Examines a matched URL and returns a replacement string.
        /// </summary>
        /// <param name="match">An URL match.</param>
        /// <param name="evaluator">A custom method that examines an URL match and returns a replacement string.</param>
        /// <returns>A replacement string.</returns>
        private string EvaluateMatch(Match match, Func<URLParserMatch, string> evaluator)
        {
            URLParserMatch parserMatch = new URLParserMatch
            {
                URL = match.Value,
                Protocol = match.Groups[URL_PROTOCOL_GROUPNAME].Value,
                Domain = match.Groups[URL_DOMAIN_GROUPNAME].Value
            };

            return evaluator(parserMatch);
        }

        #endregion


        #region "URL regular expression constants"

        /// <summary>
        /// The regular expression group name of the matched domain.
        /// </summary>
        private const String URL_DOMAIN_GROUPNAME = "domain";

        /// <summary>
        /// The regular expression group name of the matched protocol.
        /// </summary>
        private const String URL_PROTOCOL_GROUPNAME = "proto";

        private const String LATIN_ACCENTS_CHARS =
            "\\u00c0-\\u00d6\\u00d8-\\u00f6\\u00f8-\\u00ff" +                                           // Latin-1
            "\\u0100-\\u024f" +                                                                         // Latin Extended A and B
            "\\u0253\\u0254\\u0256\\u0257\\u0259\\u025b\\u0263\\u0268\\u026f\\u0272\\u0289\\u028b" +    // IPA Extensions
            "\\u02bb" +                                                                                 // Hawaiian
            "\\u0300-\\u036f" +                                                                         // Combining diacritics
            "\\u1e00-\\u1eff";                                                                          // Latin Extended Additional (mostly for Vietnamese)

        /// <summary>
        /// Chars allowed to preced URL as a separator
        /// </summary>
        private const String URL_VALID_PRECEEDING_CHARS = "(?<=[,\"'(\\s]|^)";

        private const String URL_END_LOOKAHEAD = "(?=(?:[\\s,.!)\"']|$))";

        private const String URL_VALID_CHARS = "\\w" + LATIN_ACCENTS_CHARS + "";
        private const String URL_VALID_SUBDOMAIN = "(?:(?:[" + URL_VALID_CHARS + "][-" + URL_VALID_CHARS + "_]*)?[" + URL_VALID_CHARS + "]\\.)";
        private const String URL_VALID_DOMAIN_NAME = "(?:(?:[" + URL_VALID_CHARS + "][-" + URL_VALID_CHARS + "]*)?[" + URL_VALID_CHARS + "]\\.)";

        /// <summary>
        ///  Any non-space, non-punctuation characters. \p{Z} = any kind of whitespace or invisible separator.
        /// </summary>
        private const String URL_VALID_UNICODE_CHARS = "[.[^\\p{P}\\s\\p{Z}]]";

        /// <summary>
        /// Global top level domains
        /// </summary>
        private const String URL_VALID_GTLD = "(?:(?:aero|asia|biz|cat|com|coop|edu|gov|info|int|jobs|mil|mobi|museum|name|net|org|pro|tel|travel|xxx)(?=\\W|$))";

        /// <summary>
        /// Top level domains
        /// </summary>
        private const String URL_VALID_CCTLD =
            "(?:(?:ac|ad|ae|af|ag|ai|al|am|an|ao|aq|ar|as|at|au|aw|ax|az|ba|bb|bd|be|bf|bg|bh|bi|bj|bm|bn|bo|br|bs|bt|" +
            "bv|bw|by|bz|ca|cc|cd|cf|cg|ch|ci|ck|cl|cm|cn|co|cr|cs|cu|cv|cx|cy|cz|dd|de|dj|dk|dm|do|dz|ec|ee|eg|eh|" +
            "er|es|et|eu|fi|fj|fk|fm|fo|fr|ga|gb|gd|ge|gf|gg|gh|gi|gl|gm|gn|gp|gq|gr|gs|gt|gu|gw|gy|hk|hm|hn|hr|ht|" +
            "hu|id|ie|il|im|in|io|iq|ir|is|it|je|jm|jo|jp|ke|kg|kh|ki|km|kn|kp|kr|kw|ky|kz|la|lb|lc|li|lk|lr|ls|lt|" +
            "lu|lv|ly|ma|mc|md|me|mg|mh|mk|ml|mm|mn|mo|mp|mq|mr|ms|mt|mu|mv|mw|mx|my|mz|na|nc|ne|nf|ng|ni|nl|no|np|" +
            "nr|nu|nz|om|pa|pe|pf|pg|ph|pk|pl|pm|pn|pr|ps|pt|pw|py|qa|re|ro|rs|ru|rw|sa|sb|sc|sd|se|sg|sh|si|sj|sk|" +
            "sl|sm|sn|so|sr|ss|st|su|sv|sx|sy|sz|tc|td|tf|tg|th|tj|tk|tl|tm|tn|to|tp|tr|tt|tv|tw|tz|ua|ug|uk|us|uy|uz|" +
            "va|vc|ve|vg|vi|vn|vu|wf|ws|ye|yt|za|zm|zw)(?=\\W|$))";

        /// <summary>
        /// PunyCode is used to encode Unicode characters in URLs into chars allowed in URLs
        /// </summary>
        private const String URL_PUNYCODE = "(?:xn--[0-9a-z]+)";

        /// <summary>
        /// One part of IPv4
        /// </summary>
        private const String URL_IPV4_VALID_OCTET = "(?:2[0-4][0-9]|25[0-5]|[01]?[0-9]{1,2})";

        /// <summary>
        /// IPv4 address
        /// </summary>
        private const String URL_IPV4_VALID_ADDRESS = URL_IPV4_VALID_OCTET + "(?:\\." + URL_IPV4_VALID_OCTET + "){3}";

        /// <summary>
        /// One segmentf of IPv6 address in the 4-hexadecimal-digit-format
        /// </summary>
        private const String URL_IPV6_VALID_SEGMENT = "[a-f0-9]{1,4}";

        /// <summary>
        /// IPv6 in whatever format allowed by RFC
        /// </summary>
        private const String URL_IPV6_VALID_ADDRESS = "(?:" +
            "(?:(?:" + URL_IPV6_VALID_SEGMENT + ":){7}(?:" + URL_IPV6_VALID_SEGMENT + "|:))|" +
            "(?:(?:" + URL_IPV6_VALID_SEGMENT + ":){6}(?::|(?:" + URL_IPV4_VALID_OCTET + "(?:\\." + URL_IPV4_VALID_OCTET + "){3})|(?::" + URL_IPV6_VALID_SEGMENT + ")))|" +
            "(?:(?:" + URL_IPV6_VALID_SEGMENT + ":){5}(?:(?::(?:" + URL_IPV4_VALID_ADDRESS + ")?)|(?:(?::" + URL_IPV6_VALID_SEGMENT + "){1,2})))|" +
            "(?:(?:" + URL_IPV6_VALID_SEGMENT + ":){4}(?::" + URL_IPV6_VALID_SEGMENT + "){0,1}(?:(?::(?:" + URL_IPV4_VALID_ADDRESS + ")?)|(?:(?::" + URL_IPV6_VALID_SEGMENT + "){1,2})))|" +
            "(?:(?:" + URL_IPV6_VALID_SEGMENT + ":){3}(?::" + URL_IPV6_VALID_SEGMENT + "){0,2}(?:(?::(?:" + URL_IPV4_VALID_ADDRESS + ")?)|(?:(?::" + URL_IPV6_VALID_SEGMENT + "){1,2})))|" +
            "(?:(?:" + URL_IPV6_VALID_SEGMENT + ":){2}(?::" + URL_IPV6_VALID_SEGMENT + "){0,3}(?:(?::(?:" + URL_IPV4_VALID_ADDRESS + ")?)|(?:(?::" + URL_IPV6_VALID_SEGMENT + "){1,2})))|" +
            "(?:(?:" + URL_IPV6_VALID_SEGMENT + ":)(?::" + URL_IPV6_VALID_SEGMENT + "){0,4}(?:(?::(?:" + URL_IPV4_VALID_ADDRESS + ")?)|(?:(?::" + URL_IPV6_VALID_SEGMENT + "){1,2})))|" +
            "(?::(?::" + URL_IPV6_VALID_SEGMENT + "){0,5}(?:(?::(?:" + URL_IPV4_VALID_ADDRESS + ")?)|(?:(?::" + URL_IPV6_VALID_SEGMENT + "){1,2})))|" +
            "(?:(?:" + URL_IPV4_VALID_ADDRESS + "))" +
        ")";

        /// <summary>
        /// Valid domain name as a whole.
        /// </summary>
        private const String URL_VALID_DOMAIN =
          "(?:" +                                                   // subdomains + domain + TLD
              URL_VALID_SUBDOMAIN + "+" + URL_VALID_DOMAIN_NAME +   // e.g. www.twitter.com, foo.co.jp, bar.co.uk
              "(?:" + URL_VALID_GTLD + "|" + URL_VALID_CCTLD + "|" + URL_PUNYCODE + ")" +
            ")" +
          "|(?:" +                                                  // domain + gTLD
            URL_VALID_DOMAIN_NAME +                                 // e.g. twitter.com
            "(?:" + URL_VALID_GTLD + "|" + URL_PUNYCODE + ")" +
          ")" +
          "|(?:" + "(?<=" + URL_VALID_PROTOCOL_NAME + "://)" +
            "(?:" +
              "(?:" + URL_VALID_DOMAIN_NAME + URL_VALID_CCTLD + ")" +  // protocol + domain + ccTLD
              "|(?:" +
                URL_VALID_UNICODE_CHARS + "+\\." +                     // protocol + unicode domain + TLD
                "(?:" + URL_VALID_GTLD + "|" + URL_VALID_CCTLD + ")" +
              ")" +
            ")" +
          ")" +
          "|(?:" +                                                  // domain + ccTLD + '/'
            URL_VALID_DOMAIN_NAME + URL_VALID_CCTLD + "(?=/)" +     // e.g. t.co/
          ")" +
          "|(?:" +
            URL_IPV4_VALID_ADDRESS +
          ")";

        private const String URL_VALID_PORT_NUMBER = "\\d{1,}";
        private const String URL_VALID_GENERAL_PATH_CHARS = "[^\\(\\)\\s\\?]";

        /// <summary>
        /// Allow URL paths to contain balanced parens
        /// 1. Used in Wikipedia URLs like /Primer_(film)
        /// 2. Used in IIS sessions like /S(dfd346)/
        /// </summary>
        private const String URL_BALANCED_PARENS = "\\(" + URL_VALID_GENERAL_PATH_CHARS + "+\\)";

        /// <summary>
        /// Valid end-of-path chracters (so /foo. does not gobble the period).
        /// Allow <![CDATA[=&#]]> for empty URL parameters and other URL-join artifacts
        /// </summary>
        private const String URL_VALID_PATH_ENDING_CHARS = "[^\\(\\)\\s\\?!*';:,.$%\\[\\]~|&@]|(?:" + URL_BALANCED_PARENS + ")";

        private const String URL_VALID_PATH = "(?:" +
          "(?:" + 
            URL_VALID_GENERAL_PATH_CHARS + "*" +
            "(?:" + URL_BALANCED_PARENS + URL_VALID_GENERAL_PATH_CHARS + "*)*" +
            URL_VALID_PATH_ENDING_CHARS +
          ")|(?:@" + URL_VALID_GENERAL_PATH_CHARS + "+/)" +
        ")";

        private const String URL_VALID_USER_NAME = "(?:(?:[" + URL_VALID_CHARS + "][-" + URL_VALID_CHARS + "]*)?[" + URL_VALID_CHARS + "])";
        private const String URL_VALID_PASSWORD = "(?:(?:[" + URL_VALID_CHARS + "][-" + URL_VALID_CHARS + "]*)?[" + URL_VALID_CHARS + "])";
        private const String URL_BEGIN_LOOKAHEAD = URL_VALID_PRECEEDING_CHARS;
        private const String URL_VALID_PROTOCOL_NAME = "(?:https?|s?ftp|ftps?)";
        private const String URL_VALID_URL_QUERY_CHARS = "[-a-z0-9!?*'();:&=+$/%#\\[\\]_.,~|@]";
        private const String URL_VALID_URL_QUERY_ENDING_CHARS = "[a-z0-9_&=#/]";

        /// <summary>
        /// The whole URL recognition pattern.
        /// </summary>
        private const String VALID_URL_PATTERN_STRING =
            URL_BEGIN_LOOKAHEAD +
            "(?:(?#<complete>)" +                                                               // $1 total match
            "(?:(?#<URL>)" +                                                                    // $3 URL
                "(?<" + URL_PROTOCOL_GROUPNAME + ">" + URL_VALID_PROTOCOL_NAME + "://)?" +      // $4 Protocol (optional)
                "(?:" + URL_VALID_USER_NAME + "(?::" + URL_VALID_PASSWORD + ")?@)?" +           // username:password@ (optional)
                "(?:" +                                                                         // Adding Ipv6 with port
                    "(?<" + URL_DOMAIN_GROUPNAME + ">" + URL_VALID_DOMAIN + ")" +               // $5 Domain(s)
                    "(?::(?:(?#<port>)" + URL_VALID_PORT_NUMBER + "))?" +                       // $6 Port number (optional)
                "|" +
                    "(?:\\[" + URL_IPV6_VALID_ADDRESS + "\\]:" + URL_VALID_PORT_NUMBER + ")" +  // IPv6 + Port
                "|" +
                    "(?:" + URL_IPV6_VALID_ADDRESS + ")" +
                ")" +
                "(?:(?#<path>)/" +
                  URL_VALID_PATH + "*" +
                ")?" +                                                                          //  $7 URL Path and anchor
                "(?:(?#<query>)\\?" + URL_VALID_URL_QUERY_CHARS + "*" +                         //  $8 Query String
                        URL_VALID_URL_QUERY_ENDING_CHARS + ")?" +
              ")" +
            ")" +
            URL_END_LOOKAHEAD;

        #endregion

    }

}