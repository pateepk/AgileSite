using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using CMS.Base;
using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// HTML utility methods.
    /// </summary>
    public static class HTMLHelper
    {
        #region "Constants"

        /// <summary>
        /// HTML 5 document type.
        /// </summary>
        public const string DOCTYPE_HTML5 = "<!DOCTYPE html>";

        /// <summary>
        /// HTML break tag - "&lt;br /&gt;".
        /// </summary>
        public const string HTML_BREAK = "<br />";

        /// <summary>
        /// XMLNS attribute used in XHTML documents.
        /// </summary>
        public const string DEFAULT_XMLNS_ATTRIBUTE = @"xmlns=""http://www.w3.org/1999/xhtml""";

        /// <summary>
        /// Array of special characters 
        /// </summary>
        private static readonly string[] HTML_SPECIAL_CHARS = {
            "quot;", "apos;", "amp;", "lt;", "gt;", "nbsp;", "iexcl;", "cent;", "pound;", "curren;", "yen;", "brvbar;", "sect;", "uml;",
            "copy;", "ordf;", "laquo;", "not;", "shy;", "reg;", "macr;", "deg;", "plusmn;", "sup2;", "sup3;", "acute;", "micro;", "para;",
            "middot;", "cedil;", "sup1;", "ordm;", "raquo;", "frac14;", "frac12;", "frac34;", "iquest;", "times;", "divide;", "Agrave;",
            "Aacute;", "Acirc;", "Atilde;", "Auml;", "Aring;", "AElig;", "Ccedil;", "Egrave;", "Eacute;", "Ecirc;", "Euml;", "Igrave;",
            "Iacute;", "Icirc;", "Iuml;", "ETH;", "Ntilde;", "Ograve;", "Oacute;", "Ocirc;", "Otilde;", "Ouml;", "Oslash;", "Ugrave;",
            "Uacute;", "Ucirc;", "Uuml;", "Yacute;", "THORN;", "szlig;", "agrave;", "aacute;", "acirc;", "atilde;", "auml;", "aring;",
            "aelig;", "ccedil;", "egrave;", "eacute;", "ecirc;", "euml;", "igrave;", "iacute;", "icirc;", "iuml;", "eth;", "ntilde;",
            "ograve;", "oacute;", "ocirc;", "otilde;", "ouml;", "oslash;", "ugrave;", "uacute;", "ucirc;", "uuml;", "yacute;", "thorn;",
            "yuml;", "forall;", "part;", "exist;", "empty;", "nabla;", "isin;", "notin;", "ni;", "prod;", "sum;", "minus;", "lowast;",
            "radic;", "prop;", "infin;", "ang;", "and;", "or;", "cap;", "cup;", "int;", "there4;", "sim;", "cong;", "asymp;", "ne;", "equiv;",
            "le;", "ge;", "sub;", "sup;", "nsub;", "sube;", "supe;", "oplus;", "otimes;", "perp;", "sdot;", "Alpha;", "Beta;", "Gamma;",
            "Delta;", "Epsilon;", "Zeta;", "Eta;", "Theta;", "Iota;", "Kappa;", "Lambda;", "Mu;", "Nu;", "Xi;", "Omicron;", "Pi;", "Rho; ",
            "Sigma;", "Tau;", "Upsilon;", "Phi;", "Chi;", "Psi;", "Omega; ", "alpha;", "beta;", "gamma;", "delta;", "epsilon;", "zeta;",
            "eta;", "theta;", "iota;", "kappa;", "lambda;", "mu;", "nu;", "xi;", "omicron;", "pi;", "rho;", "sigmaf;", "sigma;", "tau;",
            "upsilon;", "phi;", "chi;", "psi;", "omega; ", "thetasym;", "upsih;", "piv;", "OElig;", "oelig;", "Scaron;", "scaron;", "Yuml;",
            "fnof;", "circ;", "tilde;", "ensp;", "emsp;", "thinsp;", "zwnj;", "zwj;", "lrm;", "rlm;", "ndash;", "mdash;", "lsquo;", "rsquo;",
            "sbquo;", "ldquo;", "rdquo;", "bdquo;", "dagger;", "Dagger;", "bull;", "hellip;", "permil;", "prime;", "Prime;", "lsaquo;",
            "rsaquo;", "oline;", "euro;", "trade;", "larr;", "uarr;", "rarr;", "darr;", "harr;", "crarr;", "lceil;", "rceil;", "lfloor;",
            "rfloor;", "loz;", "spades;", "clubs;", "hearts;", "diams;"
        };

        /// <summary>
        /// CSS class marker to disable conversion of TABLE to DIV
        /// </summary>
        public const string NO_DIVS = "_nodivs";

        /// <summary>
        /// CSS class marker to enable conversion of TABLE to DIV
        /// </summary>
        public const string DIVS = "_divs";

        #endregion


        #region "Variables"

        /// <summary>
        /// Conversion level for no conversion
        /// </summary>
        private const int NOT_CONVERT = -1;


        /// <summary>
        /// Use extension on postback?
        /// </summary>
        private static bool? mUseExtensionOnPostback;


        /// <summary>
        /// Hash table of regular expressions to resolve local URLs.
        /// </summary>
        private static readonly SafeDictionary<string, Regex> mLocalUrlRegEx = new SafeDictionary<string, Regex>();


        /// <summary>
        /// List of self-closing tags
        /// </summary>
        private static readonly HashSet<string> mSelfClosingTags = new HashSet<string> { "area", "base", "br", "col", "embed", "hr", "img", "input", "keygen", "link", "menuitem", "meta", "param", "source", "track", "wbr" };

        /// <summary>
        /// List of attributes converted to CSS class
        /// </summary>
        private static readonly HashSet<string> mConvertToCssClass = new HashSet<string> { "align", "valign", "cellpadding", "cellspacing", "rules", "border" };

        /// <summary>
        /// List of div attributes converted to CSS class (not valid for DIV tag)
        /// </summary>
        private static readonly HashSet<string> mDivConvertToCssClass = new HashSet<string> { "colspan", "rowspan", "span" };

        /// <summary>
        /// List of attributes convertible to CSS style.
        /// </summary>
        private static readonly HashSet<string> mAttributesConvertibleToInlineStyle = new HashSet<string> { "width", "height" };

        /// <summary>
        /// Collection of attribute names whose value represents unit of length.
        /// </summary>
        private static readonly HashSet<string> mPixelOrPercentageAttributes = new HashSet<string> { "width", "height" };

        #endregion


        #region "Regular expressions"

        /// <summary>
        /// Regular expression for HTML encoding.
        /// </summary>
        private static Regex mRegExHTMLEncode;


        /// <summary>
        /// Regular expression to fix the form action parameter.
        /// </summary>
        private static Regex mRegExFixFormAction;


        /// <summary>
        /// Regular expression to match form actions.
        /// </summary>
        private static Regex mRegExFixAJAXFormAction;


        /// <summary>
        /// Regular expression to match the form name.
        /// </summary>
        private static Regex mRegExFormName;


        /// <summary>
        /// Tag regular expression for any tags (including ASPX ones).
        /// </summary>
        private static Regex mRegExAnyTags;


        /// <summary>
        /// Regular expression to match the title with ID attribute.
        /// </summary>
        private static Regex mRegExTitleId;


        /// <summary>
        /// Regular expression to match the URL that needs to be fixed.
        /// </summary>
        private static Regex mRegExFixUrl;


        /// <summary>
        /// Regular expression indicates if string starts with ampersand numbered char.
        /// </summary>
        private static Regex mRegExStartsWithAmpNumber;


        /// <summary>
        /// Regular expression to match the view state tag.
        /// </summary>
        private static Regex mRegExFixViewState;


        /// <summary>
        /// Complete fix HTML regular expression (matches the HTML tag).
        /// </summary>
        private static Regex mRegExFixXHTML;


        /// <summary>
        /// Regular expression pattern for URLs in CSS rules (used for server-relative URLs).
        /// </summary>
        private static Regex mRegExResolveCSSUrl;


        /// <summary>
        /// Regular expression pattern for URLs in CSS rules (used for client-relative URLs).
        /// </summary>
        private static Regex mRegExResolveCSSClientUrl;


        ///<summary>
        /// Regular expression match for the scripts.
	    /// </summary>
        private static Regex mRegExRemoveScripts;


        /// <summary>
        /// Regular expression match for new lines and spaces between tags.
        /// </summary>
        private static Regex mRegExStripTagsSpaces;


        /// <summary>
        /// Regular expression match for the tags.
        /// </summary>
        private static Regex mRegExStripTagsTags;


        /// <summary>
        /// Regular expression match for the comments.
        /// </summary>
        private static Regex mRegExStripTagsComments;


        /// <summary>
        /// Regular expression match for the HTML entities.
        /// </summary>
        private static Regex mRegExStripTagsEntities;


        /// <summary>
        /// Regular expression for removing entire head tag.
        /// </summary>
        private static Regex mRegexHtmlToTextHead;


        /// <summary>
        /// Regular expression for removing entire style tag.
        /// </summary>
        private static Regex mRegexHtmlToTextStyle;


        /// <summary>
        /// Regular expression for removing entire script tag.
        /// </summary>
        private static Regex mRegexHtmlToTextScript;


        /// <summary>
        /// Regular expression for removing tags but keeps their content.
        /// </summary>
        private static Regex mRegexHtmlToTextTags;


        /// <summary>
        /// Regular expression for removing white spaces
        /// </summary>
        private static Regex mRegexHtmlToTextWhiteSpace;

        #endregion


        #region "Properties"

        /// <summary>
        /// Use extension on postback?
        /// </summary>
        internal static bool UseExtensionOnPostbackInternal
        {
            get
            {
                if (mUseExtensionOnPostback == null)
                {
                    mUseExtensionOnPostback = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseExtensionOnPostback"], false);
                }

                return mUseExtensionOnPostback.Value;
            }
            set
            {
                mUseExtensionOnPostback = value;
            }
        }

        #endregion


        #region "RegEx properties"

        /// <summary>
        /// Regular expression for removing entire head tag.
        /// </summary>
        public static Regex RegexHtmlToTextHead
        {
            get
            {
                return mRegexHtmlToTextHead ?? (mRegexHtmlToTextHead = RegexHelper.GetRegex("<head.*?</head>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled));
            }
            set
            {
                mRegexHtmlToTextHead = value;
            }
        }


        /// <summary>
        /// Regular expression for removing entire script tag.
        /// </summary>
        public static Regex RegexHtmlToTextStyle
        {
            get
            {
                return mRegexHtmlToTextStyle ?? (mRegexHtmlToTextStyle = RegexHelper.GetRegex("<style.*?</style>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled));
            }
            set
            {
                mRegexHtmlToTextStyle = value;
            }
        }


        /// <summary>
        /// Regular expression for removing entire script tag.
        /// </summary>
        public static Regex RegexHtmlToTextScript
        {
            get
            {
                return mRegexHtmlToTextScript ?? (mRegexHtmlToTextScript = RegexHelper.GetRegex("<script.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled));
            }
            set
            {
                mRegexHtmlToTextScript = value;
            }
        }


        /// <summary>
        /// Regular expression for removing tags but keeps their content.
        /// </summary>
        public static Regex RegexHtmlToTextTags
        {
            get
            {
                return mRegexHtmlToTextTags ?? (mRegexHtmlToTextTags = RegexHelper.GetRegex("<[^>]*>"));
            }
            set
            {
                mRegexHtmlToTextTags = value;
            }
        }


        /// <summary>
        /// Regular expression for removing white spaces
        /// </summary>
        public static Regex RegexHtmlToTextWhiteSpace
        {
            get
            {
                return mRegexHtmlToTextWhiteSpace ?? (mRegexHtmlToTextWhiteSpace = RegexHelper.GetRegex("\\s+", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled));
            }
            set
            {
                mRegexHtmlToTextWhiteSpace = value;
            }
        }


        /// <summary>
        /// Regular expression for HTML encoding.
        /// </summary>
        public static Regex RegExHTMLEncode
        {
            get
            {
                // Expression groups:                                                (1:encode                          ) (2:leave         )
                return mRegExHTMLEncode ?? (mRegExHTMLEncode = RegexHelper.GetRegex("((?:[^&]|(?:&(?!(?:\\w+|#\\d+);)))+)?(&(?:\\w+|#\\d+);)?"));
            }
            set
            {
                mRegExHTMLEncode = value;
            }
        }


        /// <summary>
        /// Regular expression to fix the form action parameter.
        /// </summary>
        public static Regex RegExFixFormAction
        {
            get
            {
                // Expression groups:                                                      (1:start    )(2:action )       (3:end)
                return mRegExFixFormAction ?? (mRegExFixFormAction = RegexHelper.GetRegex("(<form.*?\\s)(action=\")(?:.*?)(\")", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase));
            }
            set
            {
                mRegExFixFormAction = value;
            }
        }


        /// <summary>
        /// Regular expression to fix the form action parameter for AJAX request.
        /// </summary>
        public static Regex RegExFixAJAXFormAction
        {
            get
            {
                // Expression groups:                                                              (1:start      )(2:action               )           (3:end)
                return mRegExFixAJAXFormAction ?? (mRegExFixAJAXFormAction = RegexHelper.GetRegex("(\\|[^\\|]*\\|)(formAction\\|[^\\|]*\\|)(?:[^\\|]*)(\\|)", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase));
            }
            set
            {
                mRegExFixAJAXFormAction = value;
            }
        }


        /// <summary>
        /// Regular expression to match the form name.
        /// </summary>
        private static Regex RegExFormName
        {
            get
            {
                // Expression groups:                                            (1:start  )
                return mRegExFormName ?? (mRegExFormName = RegexHelper.GetRegex("(<form\\s+)(?:name=.*?\\s)", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase));
            }
        }


        /// <summary>
        /// Regular expression to match the title with ID attribute.
        /// </summary>
        private static Regex RegExTitleId
        {
            get
            {
                // Expression groups:                                          (1:start)              (2:end)
                return mRegExTitleId ?? (mRegExTitleId = RegexHelper.GetRegex("(<title)(?:\\s+id=.+?)(>)", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase));
            }
        }


        /// <summary>
        /// Regular expression to match the URL that needs to be fixed.
        /// </summary>
        private static Regex RegExFixUrl
        {
            get
            {
                // Expression groups: none
                return mRegExFixUrl ?? (mRegExFixUrl = RegexHelper.GetRegex("(?:\\sbackground=\"|\\ssrc=\"|\\shref=\"|[\\s|:]url\\()(?:[^&\"]*)(?:(&amp;|&)[^&\"]*)*", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase));
            }
        }


        /// <summary>
        /// Regular expression indicates if string starts with ampersand numbered char.
        /// </summary>
        private static Regex RegExStartsWithAmpNumber
        {
            get
            {
                // Expression groups: none
                return mRegExStartsWithAmpNumber ?? (mRegExStartsWithAmpNumber = RegexHelper.GetRegex("^#\\d+;"));
            }
        }


        /// <summary>
        /// Regular expression to match the view state tag.
        /// </summary>
        private static Regex RegExFixViewState
        {
            get
            {
                // Expression groups:                                                    (1:all                    )
                return mRegExFixViewState ?? (mRegExFixViewState = RegexHelper.GetRegex("(<input.*?__VIEWSTATE.*?/>)", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase));
            }
        }


        /// <summary>
        /// Tag regular expression for any tags (including ASPX ones).
        /// </summary>
        private static Regex RegExAnyTags
        {
            get
            {
                // Expression groups:                                          (1:tag                   )
                return mRegExAnyTags ?? (mRegExAnyTags = RegexHelper.GetRegex("(<(?:[^<]|(?:<%[^>]*>))*>)"));
            }
        }


        /// <summary>
        /// Complete fix HTML regular expression (matches the HTML tag).
        /// </summary>
        internal static Regex RegExFixXHTML
        {
            get
            {
                // Expression groups:                                                  (1:tg)       (2:attribute name                            )               (3:value                                                                 )            (3:  ) (3:=  )           (1:comnt                    ) (1:endcm                    )
                return mRegExFixXHTML ?? (mRegExFixXHTML = RegexHelper.GetRegex("<(?:/?(\\w+)(?:\\s+((?:\\w|[0-9\\-_])+(?:\\:(?:\\w|[0-9\\-_])+)?)(?:\\s*=(?!\\s)((?:\"(?:[^\"]|\\\\\")*\")|(?:\'(?:[^\']|\\\\\')*\')|(?:[^\\s/>]|/(?!>))*)|(?=[\\s/>])(?<3>)|(?<3>=)))*\\s*/?>|(?<1>!--(?:\\[[^\\]]*\\]>)?))|(?<1>(?:<!\\[[^\\]]*\\])?-->)"));
            }
        }


        /// <summary>
        /// Regular expression pattern for URLs in CSS rules (used for server-relative URLs).
        /// </summary>
        private static Regex RegExResolveCSSUrl
        {
            get
            {
                // Expression groups:                                                      (1:prefix    )
                return mRegExResolveCSSUrl ?? (mRegExResolveCSSUrl = RegexHelper.GetRegex("(url\\([\"']?)(?:~/)"));
            }
        }


        /// <summary>
        /// Regular expression pattern for URLs in CSS rules (used for client-relative URLs).
        /// </summary>
        private static Regex RegExResolveCSSClientUrl
        {
            get
            {
                return mRegExResolveCSSClientUrl ?? (mRegExResolveCSSClientUrl = RegexHelper.GetRegex(@"url\((?<quote>(\'|\"")?)\s*(?<url>.*?)\s*\k<quote>\)"));
            }
        }


        /// <summary>
        /// Regular expression match for the scripts.
        /// </summary>
        private static Regex RegExRemoveScripts
        {
            get
            {
                // Expression groups: none 
                return mRegExRemoveScripts ?? (mRegExRemoveScripts = RegexHelper.GetRegex(@"<script[ >](?:[^<]|<(?!/script))*</script>", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase));
            }
        }


        /// <summary>
        /// Regular expression match for new lines and spaces between tags.
        /// </summary>
        public static Regex RegExStripTagsSpaces
        {
            get
            {
                // Expression groups: none
                return mRegExStripTagsSpaces ?? (mRegExStripTagsSpaces = RegexHelper.GetRegex(@"(?:>)(?:\s)*(?:<)"));
            }
            set
            {
                mRegExStripTagsSpaces = value;
            }
        }


        /// <summary>
        /// Regular expression match for the tags in strip tags method.
        /// </summary>
        private static Regex RegExStripTagsTags
        {
            get
            {
                // Expression groups: none 
                return mRegExStripTagsTags ?? (mRegExStripTagsTags = RegexHelper.GetRegex(@"(?></?\w+)(?>(?:[^>'""]+|'[^']*'|""[^""]*"")*)>"));
            }
        }


        /// <summary>
        /// Regular expression match for the HTML comment tags in strip tags method.
        /// </summary>
        private static Regex RegExStripTagsComments
        {
            get
            {
                // Expression groups: none 
                return mRegExStripTagsComments ?? (mRegExStripTagsComments = RegexHelper.GetRegex(@"<!([\r\n\t ])*--.*?--([\r\n\t ])*>", RegexOptions.Singleline));
            }
        }


        /// <summary>
        /// Regular expression match for the HTML entities.
        /// </summary>
        private static Regex RegExStripTagsEntities
        {
            get
            {
                // Expression groups: none
                return mRegExStripTagsEntities ?? (mRegExStripTagsEntities = RegexHelper.GetRegex("&#x?[0-9]{2,4};|&quot;|&amp;|&nbsp;|&lt;|&gt;|&euro;|&copy;|&reg;|&permil;|&Dagger;|&dagger;|&lsaquo;|&rsaquo;|&bdquo;|&rdquo;|&ldquo;|&sbquo;|&rsquo;|&lsquo;|&mdash;|&ndash;|&rlm;|&lrm;|&zwj;|&zwnj;|&thinsp;|&emsp;|&ensp;|&tilde;|&circ;|&Yuml;|&scaron;|&Scaron;", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase));
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if rel="nofollow" should be used for users link in forums, blog comments or message boards
        /// </summary>
        /// <param name="siteName">Current site name</param>
        public static bool UseNoFollowForUsersLinks(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSUseNoFollowForUsersLinks"].ToBoolean(false);
        }


        /// <summary>
        /// Fixes RTL text. Adds special marks at the end (at the beginning) of the text if it ends (starts) with weak unicode symbol.
        /// </summary>
        /// <param name="text">Text to fix</param>
        public static string FixRTLText(string text)
        {
            string retval = "";
            string textTrimmed = text.Trim();
            if (textTrimmed.StartsWithCSafe("("))
            {
                retval += "&lrm;";
            }
            retval += text;
            if (textTrimmed.EndsWithCSafe(")") || textTrimmed.EndsWithCSafe("."))
            {
                retval += "&lrm;";
            }
            return retval;
        }


        /// <summary>
        /// Returns the HTML link element that links to a related document.
        /// </summary>
        /// <param name="url">URL pointing to external document.</param>
        /// <param name="type">(Optional) Specifies the MIME type of the linked document.</param>
        /// <param name="rel">(Optional) Relationship between the current document and the linked document.</param>
        /// <param name="media">(Optional) What device the linked document will be displayed on.</param>
        /// <param name="title">(Optional) Specifies extra information about an element.</param>
        /// <param name="sizes">(Optional) Specifies size of files. This attribute is defined in HTML5.</param>
        /// <returns>HTML link to the related document</returns>
        public static string GetLink(string url, string type, string rel, string media, string title, string sizes = null)
        {
            StringBuilder link = new StringBuilder();

            link.Append("<link");

            AppendAttribute(link, "href", URLHelper.ResolveUrl(url));
            AppendAttribute(link, "type", type);
            AppendAttribute(link, "rel", rel);
            AppendAttribute(link, "media", media);
            AppendAttribute(link, "title", title);
            AppendAttribute(link, "sizes", sizes);

            link.AppendLine("/>");

            return link.ToString();
        }


        /// <summary>
        /// Returns span (of specified CSS class) containing message.
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="className">CSS class</param>
        public static string SpanMsg(string msg, string className)
        {
            return String.Format("<span class=\"{0}\">{1}</span>", className, msg);
        }


        /// <summary>
        /// Unresolves relative URI if used in "src=" or "href=" or "background=" or "url(".
        /// </summary>
        /// <param name="mHtml">HTML code.</param>
        /// <param name="applicationPath">Application path.</param>
        /// <param name="pathModifier">Method that is called after each of the local path is unresolved. Input parameter format: "~/relativePath?query=.."</param>
        public static string UnResolveUrls(string mHtml, string applicationPath, Func<string, string> pathModifier = null)
        {
            if (String.IsNullOrEmpty(mHtml))
            {
                return mHtml;
            }

            // If application path not set, get from the request
            if (applicationPath == null)
            {
                applicationPath = SystemContext.ApplicationPath;
            }

            var linksConverter = new RichTextLinksConverter(applicationPath, pathModifier);
            return linksConverter.Unresolve(mHtml);
        }


        /// <summary>
        /// Resolves relative URLs in given HTML(string). 
        /// If CMSResolveAllUrls application settings is true it resolves all URLs.
        /// Otherwise it resolves URLs used in "src", "href", "background" and "url" attributes.
        /// </summary>
        /// <param name="mHtml">HTML code</param>
        /// <param name="applicationPath">Application path</param>
        /// <param name="canResolveAllUrls">Determines whether resolving of all URL can be used, <c>true</c> is used by default</param>
        public static string ResolveUrls(string mHtml, string applicationPath, bool canResolveAllUrls = true)
        {
            if (mHtml != null)
            {
                // If application path not set, get from the request
                if ((applicationPath == null) && (CMSHttpContext.Current != null))
                {
                    applicationPath = "~/";
                }
                applicationPath = URLHelper.ResolveUrl(applicationPath);

                applicationPath = applicationPath?.TrimEnd('/');

                bool virtualPrefix = VirtualContext.IsInitialized;

                if (!virtualPrefix && canResolveAllUrls)
                {
                    // Resolve fast with string builder in case no virtual prefix is present
                    int pathIndex = mHtml.IndexOf("~/", StringComparison.Ordinal);
                    if (pathIndex >= 1)
                    {
                        StringBuilder sb = new StringBuilder((int)(mHtml.Length * 1.1));

                        int lastIndex = 0;
                        while (pathIndex >= 1)
                        {
                            var openingSymbol = GetLastNonWhiteSpaceSymbol(mHtml, pathIndex - 1);
                            if ((openingSymbol == '(') || (openingSymbol == '"') || (openingSymbol == '\''))
                            {
                                // Add previous code
                                if (lastIndex < pathIndex)
                                {
                                    sb.Append(mHtml, lastIndex, pathIndex - lastIndex);
                                }

                                // Add application path and move to the next location
                                sb.Append(applicationPath);
                                lastIndex = pathIndex + 1;
                            }

                            pathIndex = mHtml.IndexOf("~/", pathIndex + 2, StringComparison.Ordinal);
                        }

                        // Add the rest of the code
                        if (lastIndex < mHtml.Length)
                        {
                            sb.Append(mHtml, lastIndex, mHtml.Length - lastIndex);
                        }

                        mHtml = sb.ToString();
                    }
                }
                else
                {
                    // Resolve only supported attributes
                    var supportedAttributesWithEqualSign = new[]
                    {
                        "href",
                        "src",
                        "background"
                        // url attribude is handled separately, as it uses brackets

                    };

                    int pathIndex = mHtml.IndexOf("~/", StringComparison.Ordinal);
                    if (pathIndex >= 1)
                    {
                        StringBuilder sb = new StringBuilder((int)(mHtml.Length * 1.1));

                        int lastIndex = 0;
                        while (pathIndex >= 1)
                        {
                            // Get the left substring - the one that contains the attribute name. Unprocessed part (starting from lastIndex) is enough
                            var leftSubstring = mHtml.Substring(lastIndex, pathIndex - lastIndex).TrimEnd();
                            var openingSymbol = leftSubstring[leftSubstring.Length - 1];

                            if ((openingSymbol == '"') || (openingSymbol == '\''))
                            {
                                leftSubstring = leftSubstring.TrimEnd(openingSymbol).TrimEnd();

                                if (leftSubstring.EndsWith("=", StringComparison.Ordinal))
                                {
                                    leftSubstring = leftSubstring.TrimEnd('=').TrimEnd();
                                    if (leftSubstring.EndsWithAny(StringComparison.OrdinalIgnoreCase, supportedAttributesWithEqualSign))
                                    {
                                        // Add previous code
                                        if (lastIndex < pathIndex)
                                        {
                                            sb.Append(mHtml, lastIndex, pathIndex - lastIndex);
                                        }

                                        // Add application path and move to the next location
                                        sb.Append(applicationPath);
                                        lastIndex = pathIndex + 1;
                                    }
                                }
                            }
                            else if (openingSymbol == '(')
                            {
                                leftSubstring = leftSubstring.TrimEnd(openingSymbol).TrimEnd();

                                if (leftSubstring.EndsWith("url", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Add previous code
                                    if (lastIndex < pathIndex)
                                    {
                                        sb.Append(mHtml, lastIndex, pathIndex - lastIndex);
                                    }

                                    // Add application path and move to the next location
                                    sb.Append(applicationPath);
                                    lastIndex = pathIndex + 1;
                                }
                            }

                            pathIndex = mHtml.IndexOf("~/", pathIndex + 2, StringComparison.Ordinal);
                        }

                        // Add the rest of the code
                        if (lastIndex < mHtml.Length)
                        {
                            sb.Append(mHtml, lastIndex, mHtml.Length - lastIndex);
                        }

                        mHtml = sb.ToString();
                    }
                }
            }

            return mHtml;
        }


        private static char GetLastNonWhiteSpaceSymbol(string mHtml, int index)
        {
            var openingSymbol = mHtml[index];

            int i = 1;
            while (char.IsWhiteSpace(openingSymbol) && i < index)
            {
                openingSymbol = mHtml[index - i];
                i++;
            }
            return openingSymbol;
        }


        /// <summary>
        /// Ensures that all local URLs have given prefix
        /// </summary>
        /// <param name="mHtml">HTML code</param>
        /// <param name="applicationPath">Application path</param>
        /// <param name="urlPrefix">URL prefix to ensure</param>
        /// <param name="urlModifier">Callback for URL modification, optional</param>
        public static string EnsureURLPrefixes(string mHtml, string applicationPath, string urlPrefix, URLHelper.PathModifierHandler urlModifier = null)
        {
            if (mHtml != null)
            {
                // Prepare the match 
                MatchEvaluator evaluator =
                    m => EnsureURLPrefix(m, applicationPath, urlPrefix, urlModifier);

                // Get regular expression
                Regex re = mLocalUrlRegEx[applicationPath];
                if (re == null)
                {
                    // Expression groups:      (1:name                                    (2: part                                     )(3:url                                       )(4:end)
                    re = RegexHelper.GetRegex("(\\ssrc=[\"']|\\shref=[\"']|\\surl\\([\"'])(https://[^/\"']*|http://[^/\"']*|//[^/\"']*|)(" + applicationPath.TrimEnd('/') + "/[^\"']*)([\"'])", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase);
                    mLocalUrlRegEx[applicationPath] = re;
                }

                // Process all URLs
                mHtml = re.Replace(mHtml, evaluator);
            }

            return mHtml;
        }


        /// <summary>
        /// Ensures the URL prefix in the given match
        /// </summary>
        /// <param name="m">RegEx match</param>
        /// <param name="applicationPath">Application path</param>
        /// <param name="urlPrefix">URL prefix</param>
        /// <param name="urlModifier">URL modifier</param>
        private static string EnsureURLPrefix(Match m, string applicationPath, string urlPrefix, URLHelper.PathModifierHandler urlModifier)
        {
            string name = m.Groups[1].ToString();
            string part = m.Groups[2].ToString();
            string url = m.Groups[3].ToString();
            string end = m.Groups[4].ToString();

            // Ensure the virtual prefix only if not present yet
            if (urlPrefix.StartsWith(VirtualContext.VirtualContextPrefix, StringComparison.Ordinal) && !VirtualContext.ContainsVirtualContextPrefix(url))
            {
                // Get current domain
                string domainName = URLHelper.GetDomainName(URLHelper.GetDomain(part));
                // Ensure prefix for local URLs only
                if (string.IsNullOrEmpty(part) || domainName.EqualsCSafe(RequestContext.CurrentDomain, true))
                {
                    url = URLHelper.EnsureURLPrefix(url, applicationPath, urlPrefix, urlModifier);
                }
            }

            return string.Format("{0}{1}{2}{3}", name, part, url, end);
        }


        /// <summary>
        /// Resolves application-relative URLs in CSS rules to absolute URLs (used with database style sheets).
        /// </summary>
        /// <param name="inputText">CSS text to search and re-base</param>
        /// <param name="applicationPath">Base URL to use when resolving application relative URLs</param>
        /// <returns>CSS rules containing absolute URLs to resources</returns>
        public static string ResolveCSSUrls(string inputText, string applicationPath)
        {
            if (inputText != null)
            {
                // If application path not set, get from the request
                if (applicationPath == null)
                {
                    applicationPath = SystemContext.ApplicationPath;
                }
                // Ensure empty spaces and trailing slash in application path
                applicationPath = applicationPath.TrimEnd('/').Replace(" ", "%20");

                inputText = RegExResolveCSSUrl.Replace(inputText, "$1" + applicationPath + "/");
            }

            return inputText;
        }


        /// <summary>
        /// Resolves client-relative URLs in CSS rules to absolute URLs (used with css files).
        /// </summary>
        /// <param name="inputText">CSS text to search and re-base</param>
        /// <param name="baseUrl">Base URL to use when resolving client relative URLs</param>
        /// <returns>CSS rules containing absolute URLs to resources</returns>
        public static string ResolveCSSClientUrls(string inputText, string baseUrl)
        {
            if (!string.IsNullOrEmpty(inputText) && !string.IsNullOrEmpty(baseUrl))
            {
                MatchEvaluator evaluator =
                    link => string.Format("url({1}{0}{1})", URLHelper.ResolveClientUrl(baseUrl, link.Groups["url"].ToString()), link.Groups["quote"]);

                inputText = RegExResolveCSSClientUrl.Replace(inputText, evaluator);
            }

            return inputText;
        }


        /// <summary>
        /// Remove HTML tags from text.
        /// </summary>
        /// <param name="htmlText">HTML text</param>
        /// <param name="replaceEntities">True if replace special entities</param>
        /// <param name="tagReplacement">Replacement for HTML tags - inner of tag is string "$2"</param>
        public static string StripTags(string htmlText, bool replaceEntities = true, string tagReplacement = "")
        {
            return StripTags(htmlText, replaceEntities, false, tagReplacement, "@", string.Empty);
        }


        /// <summary>
        /// Remove HTML tags from text.
        /// </summary>
        /// <param name="htmlText">HTML text</param>
        /// <param name="replaceEntities">True if replace special entities</param>
        /// <param name="replaceComments">Whether to replace HTML comments</param>
        /// <param name="tagReplacement">Replacement for HTML tags - inner of tag is string "$2"</param>
        /// <param name="entitiesReplacement">Replacement for HTML entities</param>
        /// <param name="commentsReplacement">Replacement for HTML comments</param>
        public static string StripTags(string htmlText, bool replaceEntities, bool replaceComments, string tagReplacement, string entitiesReplacement, string commentsReplacement)
        {
            if (string.IsNullOrEmpty(htmlText))
            {
                return htmlText;
            }

            // Optimization - If no HTML tags are present, no need to strip the tags
            if (MayContainTags(htmlText))
            {
                // Replace comments
                if (replaceComments && (commentsReplacement != null))
                {
                    htmlText = RemoveComments(htmlText, commentsReplacement);
                }

                // Remove the scripts first
                htmlText = RemoveScripts(htmlText);

                // Strip white spaces between tags
                htmlText = RegExStripTagsSpaces.Replace(htmlText, "><");

                // Strip tags
                if (tagReplacement != null)
                {
                    htmlText = RegExStripTagsTags.Replace(htmlText, tagReplacement);
                }
            }

            // Substitute special characters with replacement. Optimization, if no HTML entities are present, do not process
            if (replaceEntities && (entitiesReplacement != null) && MayContainEntities(htmlText))
            {
                htmlText = RegExStripTagsEntities.Replace(htmlText, entitiesReplacement);
            }

            return htmlText;
        }


        /// <summary>
        /// Remove HTML comments from given text.
        /// </summary>
        /// <param name="htmlText">HTML text</param>
        /// <param name="commentsReplacement">Replacement for HTML comments</param>
        public static string RemoveComments(string htmlText, string commentsReplacement = "")
        {
            if (string.IsNullOrEmpty(htmlText))
            {
                return htmlText;
            }

            return RegExStripTagsComments.Replace(htmlText, commentsReplacement);
        }


        /// <summary>
        /// Returns true if the given text may contain HTML entities
        /// </summary>
        /// <param name="text">Text to check</param>
        private static bool MayContainEntities(string text)
        {
            return (text.IndexOf(';') >= 0);
        }


        /// <summary>
        /// Returns true if the given text may contain HTML entities
        /// </summary>
        /// <param name="text">Text to check</param>
        private static bool MayContainTags(string text)
        {
            return (text.IndexOf('<') >= 0);
        }


        /// <summary>
        /// Converts HTML to the plain text (body part).
        /// </summary>
        /// <param name="htmlCode">HTML code</param>
        public static string HtmlToPlainText(string htmlCode)
        {
            // Remove new lines
            htmlCode = htmlCode.Replace("\n", " ");
            // Remove tab spaces
            htmlCode = htmlCode.Replace("\t", " ");
            // Remove head tag
            htmlCode = RegexHtmlToTextHead.Replace(htmlCode, " ");
            // Remove style tag
            htmlCode = RegexHtmlToTextStyle.Replace(htmlCode, " ");
            // Remove any JavaScript
            htmlCode = RegexHtmlToTextScript.Replace(htmlCode, " ");
            // Remove tags
            htmlCode = RegexHtmlToTextTags.Replace(htmlCode, " ");
            // Decode HTML entities
            htmlCode = HttpUtility.HtmlDecode(htmlCode);
            // Replace white spaces
            htmlCode = RegexHtmlToTextWhiteSpace.Replace(htmlCode, " ");

            // Return plain HTML code
            return htmlCode;
        }

           
        /// <summary>
        /// Removes the scripts from the given HTML text.
        /// </summary>
        /// <param name="htmlText">HTML text to process</param>
        public static string RemoveScripts(string htmlText)
        {
            if (String.IsNullOrWhiteSpace(htmlText))
            {
                return htmlText;
            }

            while (RegExRemoveScripts.IsMatch(htmlText))
            {
                htmlText = RegExRemoveScripts.Replace(htmlText, "");
            }

            return htmlText;
        }


        /// <summary>
        /// Ensures the HTML line endings (&lt;br /&gt;) in the given text.
        /// </summary>
        /// <param name="text">Text to process</param>
        public static string EnsureHtmlLineEndings(string text)
        {
            return TextHelper.EnsureLineEndings(text, HTML_BREAK);
        }


        /// <summary>
        /// Reformats the HTML code indentation.
        /// </summary>
        /// <param name="inputText">Input text</param>
        /// <param name="indentationString">Indentation string, "\t" is used by default</param>
        /// <param name="maxIndent">Maximum indentation level, <c>Int32.MaxValue</c> is used by default</param>
        public static string ReformatHTML(string inputText, string indentationString = "\t", int maxIndent = Int32.MaxValue)
        {
            // Add new lines for tags
            inputText = RegExAnyTags.Replace(inputText, "\n$0\n");

            int indent = 0;
            StringBuilder sb = new StringBuilder((int)(inputText.Length * 1.5));

            string[] lines = inputText.Split('\n');
            bool lastTag = false;
            bool lastEndTag = false;

            foreach (string line in lines)
            {
                string trimLine = line.Trim();
                if (trimLine != "")
                {
                    bool currentTag = trimLine.StartsWithCSafe("<");
                    bool currentEndTag = false;

                    // Decrease indentation if end tag
                    if (trimLine.StartsWithCSafe("</"))
                    {
                        currentEndTag = true;
                        currentTag = false;

                        indent--;
                        if (indent < 0)
                        {
                            // Indent all the content before
                            if (sb.Length > 0)
                            {
                                int i = sb.Length - 1;

                                while (i > 0)
                                {
                                    // Insert the indentation on each line
                                    if (sb[i] == '\n')
                                    {
                                        sb.Insert(i + 1, indentationString);
                                    }

                                    i--;
                                }

                                // Insert the indentation on the first line
                                sb.Insert(0, indentationString);
                            }

                            indent = 0;
                        }
                    }

                    if ((currentTag || currentEndTag) && (lastTag || lastEndTag) && !(currentEndTag && lastTag))
                    {
                        sb.NewLine();

                        // Create indentation
                        for (int i = 0; i < indent; i++)
                        {
                            sb.Append(indentationString);
                        }
                    }

                    // Increase indentation if start tag
                    if ((indent <= maxIndent) && trimLine.StartsWithCSafe("<") && !trimLine.StartsWithCSafe("<html", true) && !trimLine.StartsWithCSafe("<%") && !trimLine.StartsWithCSafe("<!") && !trimLine.StartsWithCSafe("</") && !trimLine.EndsWithCSafe("/>"))
                    {
                        indent++;
                    }

                    // Current tag is self closing
                    if (trimLine.EndsWithCSafe("/>"))
                    {
                        currentEndTag = true;
                        currentTag = false;
                    }

                    lastTag = currentTag;
                    lastEndTag = currentEndTag;

                    if (currentTag || currentEndTag)
                    {
                        sb.Append(trimLine);
                    }
                    else
                    {
                        sb.Append(line);
                    }
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Highlight HTML.
        /// </summary>
        /// <param name="inputHtml">Input HTML</param>
        public static string HighlightHTML(string inputHtml)
        {
            if (string.IsNullOrEmpty(inputHtml))
            {
                // Do not process empty HTML
                return inputHtml;
            }

            const string tagspan = "<span class=\"HTMLTag\">";
            const string tagcontentspan = "<span class=\"HTMLTagContent\">";
            const string propertyspan = "<span class=\"HTMLProperty\">";
            const string propertynamespan = "<span class=\"HTMLPropertyName\">";
            const string endspan = "</span>";

            // Highlight for open and closed tags
            inputHtml = inputHtml.Replace("<", "%%tagsspan%%" + HTMLEncode("<") + "%%endspan%%%%tagcontentspan%%");
            inputHtml = inputHtml.Replace("/>", "%%endspan%%%%tagsspan%%/" + HTMLEncode(">") + "%%endspan%%");
            inputHtml = inputHtml.Replace(">", "%%endspan%%%%tagsspan%%" + HTMLEncode(">") + "%%endspan%%");

            Regex regExPropertyName = RegexHelper.GetRegex("(\\s(\\w*|\\w*[\\S]*\\w*)\\s*(=))");
            Regex regExProperty = RegexHelper.GetRegex("(=\"(\\w*|[^\"]*|\\s*)\")");

            // Tag properties
            inputHtml = regExPropertyName.Replace(inputHtml, " %%propertynamespan%%$2%%endspan%%$3");
            inputHtml = regExProperty.Replace(inputHtml, "%%propertyspan%%$1%%endspan%%");

            // Replace macros for tags
            inputHtml = inputHtml.Replace("%%tagcontentspan%%", tagcontentspan);
            inputHtml = inputHtml.Replace("%%propertynamespan%%", propertynamespan);
            inputHtml = inputHtml.Replace("%%propertyspan%%", propertyspan);
            inputHtml = inputHtml.Replace("%%tagsspan%%", tagspan);
            inputHtml = inputHtml.Replace("%%endspan%%", endspan);

            return inputHtml;
        }


        /// <summary>
        /// Returns color from HTML string color definition.
        /// </summary>
        /// <param name="color">HTML color definition</param>
        /// <param name="defaultColor">Default color to return</param>
        public static Color GetSafeColorFromHtml(string color, Color defaultColor)
        {
            try
            {
                return ColorTranslator.FromHtml(color);
            }
            catch
            {
                return defaultColor;
            }
        }


        /// <summary>
        /// Gets the unique ID for the control, if some other control has the same base ID, adds the number at the end.
        /// </summary>
        /// <param name="baseId">Base control ID</param>
        public static string GetUniqueControlID(string baseId)
        {
            // Get the table of registered ID for current request
            Hashtable existingIds = WebLanguagesContext.CurrentHTMLUniqueIDs;

            if (existingIds[baseId] == null)
            {
                existingIds[baseId] = true;
                return baseId;
            }

            // Get unique ID
            string newId = baseId;
            int index = 1;
            while (existingIds.Contains(newId))
            {
                newId = baseId + index.ToString();
                index++;
            }

            existingIds[newId] = true;

            return newId;
        }

        /// <summary>
        /// Adds attribute to the html tag only if <paramref name="attributeValue" /> is not <c>null</c>.
        /// Attribute duplicates are not checked.
        /// </summary>
        /// <param name="link">Link to add attribute to</param>
        /// <param name="attributeName">Name of the attribute to add</param>
        /// <param name="attributeValue">Value of the attribute to add</param>
        private static void AppendAttribute(StringBuilder link, string attributeName, string attributeValue)
        {
            if (attributeValue == null)
            {
                return;
            }

            string htmlAttribute = EncodeForHtmlAttribute(attributeValue);
            link.AppendFormat(" {0}=\"{1}\"", attributeName, htmlAttribute);
        }

        #endregion


        #region "HTML Encoding"

        /// <summary>
        /// HTML decoding function. Returns a string converted into an HTML-decoded string.
        /// </summary>
        /// <param name="inputText">Input text to encode</param>
        public static string HTMLDecode(string inputText)
        {
            if (inputText == null)
            {
                return null;
            }

            return HttpUtility.HtmlDecode(inputText);
        }


        /// <summary>
        /// HTML encoding function. Returns a string converted into an HTML-encoded string.
        /// Function does not encode previously encoded HTML entity (avoiding double HTML escaping).
        /// </summary>
        /// <param name="inputText">Input text to encode</param>
        public static string HTMLEncode(string inputText)
        {
            if (String.IsNullOrEmpty(inputText))
            {
                return inputText;
            }

            // Optimization - If the input text doesn't contain entities, encode the whole text
            if (!MayContainEntities(inputText))
            {
                return HttpUtility.HtmlEncode(inputText);
            }

            // Encode only not yet encoded text
            return RegExHTMLEncode.Replace(inputText, EncodeMatch);
        }


        /// <summary>
        /// Matching function.
        /// </summary>
        /// <param name="m">Regular expression match</param>
        private static string EncodeMatch(Match m)
        {
            return HttpUtility.HtmlEncode(m.Groups[1].Value) + m.Groups[2].Value;
        }


        /// <summary>
        /// HTML encoding function. HTML encodes given text and replaces standard line breaks with "&lt;br /&gt;" tag.
        /// </summary>
        /// <param name="inputText">Input text to encode</param>
        public static string HTMLEncodeLineBreaks(string inputText)
        {
            return EnsureHtmlLineEndings(HTMLEncode(inputText));
        }


        /// <summary>
        /// Encodes string to be used as the HTML attribute value.
        /// </summary>
        /// <param name="inputText">Input text to encode</param>
        public static string EncodeForHtmlAttribute(string inputText)
        {
            if (inputText == null)
            {
                return null;
            }

            return HttpUtility.HtmlAttributeEncode(inputText);
        }


        /// <summary>
        /// Encodes string to be used in a HTML comment.
        /// </summary>
        /// <param name="inputText">Input text to encode</param>
        public static string EncodeForHtmlComment(string inputText)
        {
            if (inputText == null)
            {
                return null;
            }

            return inputText.Replace(@"--", @"__");
        }

        #endregion


        #region "XHTML fix methods"

        /// <summary>
        /// Fixes the form action attribute within given HTML code.
        /// </summary>
        /// <param name="sourceHtml">Source HTML code</param>
        /// <param name="extension">Current extension</param>
        /// <param name="standardForm">Fix standard form action, <c>true</c> is used by default</param>
        /// <param name="ajaxForm">Fix AJAX form action, <c>false</c> is used by default</param>
        /// <param name="rawUrl">Raw URL of current request, optional</param>
        public static string FixFormAction(string sourceHtml, string extension, bool standardForm = true, bool ajaxForm = false, string rawUrl = null)
        {
            if (CMSHttpContext.Current != null)
            {
                string requestUrl = GetFormAction(rawUrl);
                string result = sourceHtml;
                if (standardForm)
                {
                    // Look for form tag
                    int formIndex = result.IndexOfCSafe("<form ", true);
                    if (formIndex >= 0)
                    {
                        // Replace the action
                        requestUrl = HTMLEncode(requestUrl);
                        result = RegExFixFormAction.Replace(result, "$1$2" + CMSHttpContext.Current.Server.UrlPathEncode(requestUrl) + "$3", 1, formIndex);
                    }
                }
                if (ajaxForm)
                {
                    // Look for form tag
                    int formIndex = result.IndexOfCSafe("|formAction|", true);
                    if (formIndex >= 0)
                    {
                        formIndex -= 10;
                        if (formIndex < 0)
                        {
                            formIndex = 0;
                        }
                        result = RegExFixAJAXFormAction.Replace(result, "|" + requestUrl.Length + "|$2" + CMSHttpContext.Current.Server.UrlPathEncode(requestUrl) + "$3", 1, formIndex);
                    }
                }
                return result;
            }
            return sourceHtml;
        }


        /// <summary>
        /// Gets the form action for the current request.
        /// </summary>
        /// <param name="rawUrl">Raw URL</param>
        public static string GetFormAction(string rawUrl)
        {
            // Prepare request URL
            string requestUrl = !String.IsNullOrEmpty(rawUrl) ? rawUrl : URLHelper.UnResolveUrl(RequestContext.CurrentURL, SystemContext.ApplicationPath);

            // !!!PLEASE NOTE!!!:
            // If you change anything, all possible values must be tested 
            // in all supported IISs and App pools and .NET versions!!!!!
            //
            // Possible values:
            // "/"
            // "/?queryString"
            // "/ApliacationPath/"
            // "/ApplicationPath/?"
            // "/Home"
            // "/Home?"
            // "/Home/"
            // "/Home/?"
            // "/Home.aspx"
            // "/Home.aspx?"
            // "/sitePrefix"
            // "/sitePrefix/"
            // "/langPrefix"
            // "/langPrefix/"
            // "/sitePrefix/langPrefix"
            // "/sitePrefix/langPrefix/"

            // Change form action  if it is required
            if (UseExtensionOnPostbackInternal)
            {
                // Get URL without application path
                string url = URLHelper.RemoveQuery(requestUrl);
                string query = URLHelper.GetQuery(requestUrl);

                // Leave root URL as is
                if (url == "~/")
                {
                    requestUrl = url + query;
                }
                else
                {
                    url = url.TrimStart('~');
                    if (url.EndsWithCSafe("/"))
                    {
                        url = URLHelper.RemoveTrailingSlashFromURL(url);
                    }

                    // Get last slash index
                    int slashIndex = url.LastIndexOfCSafe("/");
                    // If slash index is somewhere in path, remove current extension and add default extension
                    if (slashIndex >= 0)
                    {
                        // Remove the extension
                        string urlWithoutExtension = URLHelper.RemoveExtension(url);

                        // if extension wasn't removed, check language and path prefixes
                        if (url.Length == urlWithoutExtension.Length)
                        {
                            // Indicates whether default page should be added at the end of current URL
                            bool addDefault = false;

                            // Check lang prefix
                            if (!String.IsNullOrEmpty(RequestContext.CurrentURLLangPrefix))
                            {
                                if (urlWithoutExtension.EndsWithCSafe("/" + RequestContext.CurrentURLLangPrefix.Trim('/'), true))
                                {
                                    addDefault = true;
                                }
                            }

                            // Check path prefix
                            if (!addDefault && !String.IsNullOrEmpty(RequestContext.CurrentURLPathPrefix))
                            {
                                if (urlWithoutExtension.EndsWithCSafe("/" + RequestContext.CurrentURLPathPrefix.Trim('/'), true))
                                {
                                    addDefault = true;
                                }
                            }

                            // Add default page
                            if (addDefault)
                            {
                                urlWithoutExtension += "/default";
                            }
                        }

                        url = urlWithoutExtension + ".aspx";
                    }

                    requestUrl = "~" + url + query;
                }
            }

            // Resolve URL
            requestUrl = URLHelper.ResolveUrl(requestUrl);

            // Return URL
            return requestUrl;
        }


        /// <summary>
        /// Removes form name tag from the given HTML code.
        /// </summary>
        /// <param name="sourceHtml">Source HTML code</param>
        public static string RemoveFormName(string sourceHtml)
        {
            return RegExFormName.Replace(sourceHtml, "$1", 1);
        }


        /// <summary>
        /// Removes the title ID attribute from the HTML code.
        /// </summary>
        /// <param name="sourceHtml">Source HTML</param>
        public static string RemoveTitleId(string sourceHtml)
        {
            return RegExTitleId.Replace(sourceHtml, "$1$2", 1);
        }


        /// <summary>
        /// Fixes the URL within given HTML code. The output code is HTML valid.
        /// 
        /// <![CDATA[
        /// It changes '&' to '&amp;' and backwards if needed.
        /// 
        /// '?test=1&test2=2' is changed to '?test=1&amp;test2=2'
        /// '&amp;#13;' is changed to '&#13;'.
        /// '&amp;lt;' is changed to '&lt;'.
        /// ]]>
        /// </summary>
        /// <param name="sourceHtml">Source HTML</param>
        public static string FixUrl(string sourceHtml)
        {
            return (sourceHtml != null) ? RegExFixUrl.Replace(sourceHtml, FixUrlEvaluator) : null;
        }


        /// <summary>
        /// Fixes the view state tag within given HTML code.
        /// </summary>
        /// <param name="sourceHtml">Source HTML code</param>
        public static string FixViewstate(string sourceHtml)
        {
            // Wrap the __VIEWSTATE tag in a div to pass validation
            return RegExFixViewState.Replace(sourceHtml, "<div style=\"display:none\">$1</div>", 1);
        }


        /// <summary>
        /// Converts the string representation to the ConvertTableEnum
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static ConvertTableEnum GetConvertTableEnum(string value)
        {
            if (value == null)
            {
                return ConvertTableEnum.None;
            }

            switch (value.ToLowerCSafe())
            {
                case "marked":
                    return ConvertTableEnum.Marked;

                case "all":
                    return ConvertTableEnum.All;

                default:
                    return ConvertTableEnum.None;
            }
        }


        /// <summary>
        /// Limits the string length.
        /// </summary>
        /// <param name="text">Text to trim</param>
        /// <param name="maxLength">Max length</param>
        /// <param name="padString">Trim character</param>
        /// <param name="wholeWords">If true, the text won't be cut in the middle of the word</param>
        public static string LimitLength(string text, int maxLength, string padString, bool wholeWords)
        {
            if (text == null)
            {
                return null;
            }

            if (maxLength <= 0)
            {
                return text;
            }

            if (padString == null)
            {
                padString = TextHelper.DEFAULT_ELLIPSIS;
            }

            // Trim the text
            int trimmedLength = maxLength - padString.Length;
            if (trimmedLength <= 0)
            {
                return padString;
            }

            if (text.Length > trimmedLength)
            {
                // Ensure the cut to the whole words
                if (wholeWords && Char.IsLetterOrDigit(text[trimmedLength]))
                {
                    // Go to the beginning of the word
                    while ((trimmedLength > 0) && Char.IsLetterOrDigit(text[trimmedLength - 1]))
                    {
                        trimmedLength--;
                    }

                    // Go to the end of the next word before (skip the white spaces and special chars)
                    while ((trimmedLength > 0) && !Char.IsLetterOrDigit(text[trimmedLength - 1]))
                    {
                        trimmedLength--;
                    }
                }

                text = text.Substring(0, trimmedLength);

                // Check if tag is broken
                int lastBracket = text.LastIndexOfAny(new[] { '<', '>' });
                if ((lastBracket >= 0) && (text[lastBracket] == '<'))
                {
                    // Remove broken tag
                    text = text.Substring(0, lastBracket);
                }
                text = text + padString;

                // Fix HTML and close tags
                var settings = new FixXHTMLSettings
                {
                    SelfClose = true
                };

                text = FixXHTML(text, settings, null);
            }
            return text;
        }


        /// <summary>
        /// Performs the HTML code fix to become valid XHTML. Method for complete fix solution.
        /// </summary>
        /// <param name="sourceHtml">Source HTML code</param>
        /// <param name="settings">Settings</param>
        /// <param name="applicationPath">Application path (for resolving URLs)</param>
        public static string FixXHTML(string sourceHtml, FixXHTMLSettings settings, string applicationPath)
        {
            if (String.IsNullOrEmpty(sourceHtml))
            {
                return sourceHtml;
            }

            // Check if something should be fixed
            if (settings.AnyEnabled)
            {
                if (settings.ResolveUrl)
                {
                    // If application path not set, get from the request
                    if (String.IsNullOrEmpty(applicationPath))
                    {
                        applicationPath = SystemContext.ApplicationPath;
                    }
                }

                Stack<string> openTags = new Stack<string>();

                // Parse the list of matched tags
                MatchCollection tags = RegExFixXHTML.Matches(sourceHtml);

                // Prepare string builders
                StringBuilder sb = new StringBuilder((int)(sourceHtml.Length * 1.1));

                StringBuilder tagSb = new StringBuilder();
                StringBuilder attrSb = new StringBuilder();
                StringBuilder classSb = new StringBuilder();
                StringBuilder styleSb = new StringBuilder();

                int lastIndex = 0;

                string lastOpenedSelfClosing = "";
                string lastOpenedSelfLowerTagName = "";

                // Processing flags
                bool processTags = true;
                bool keepProcessTags = false;

                bool closeBeforeNextOpen = false;
                bool insideScript = false;

                // Nesting levels and indentation
                int currentLevel = 0;
                int indent = 0;

                // Tag conversion level
                Stack<int> convertLevels = new Stack<int>();
                int convertLevel = NOT_CONVERT;
                if (settings.TableToDiv == ConvertTableEnum.All)
                {
                    convertLevel = currentLevel;
                }
                convertLevels.Push(convertLevel);

                // Process all tags
                foreach (Match m in tags)
                {
                    // Process the tag
                    string tag = m.ToString();

                    bool closeConditionalComment = tag.StartsWithCSafe("<![endif]", true);
                    bool endTag = tag.StartsWithCSafe("</") || closeConditionalComment;
                    bool selfClosed = tag.EndsWithCSafe("/>");
                    bool singleComment = tag.StartsWithCSafe("<!") && tag.EndsWithCSafe(">");

                    bool selfClosing = false;
                    bool scriptTag = false;

                    string tagName = m.Groups[1].ToString();
                    string lowerTagName = tagName.ToLowerCSafe();

                    // Default settings for the new line before tag
                    bool newLineBeforeTag = endTag;

                    if (IsSelfClosing(lowerTagName))
                    {
                        // Self closing tag
                        selfClosing = true;
                    }
                    else
                    {
                        switch (lowerTagName)
                        {
                            case "iframe":
                                newLineBeforeTag = false;
                                break;

                            // Disable processing for script tag
                            case "script":
                                if (!endTag)
                                {
                                    scriptTag = true;
                                    insideScript = !selfClosed; // Self closed script will expand, so result is not inside script
                                }
                                else
                                {
                                    if (keepProcessTags)
                                    {
                                        processTags = true;
                                    }
                                    insideScript = false;
                                }
                                newLineBeforeTag = false;
                                break;

                            // Disable processing for the comment tag
                            case "!--":
                                {
                                    keepProcessTags = !processTags;
                                    processTags = false;
                                }
                                break;

                            default:
                                if ((tag.StartsWithCSafe("<!--[") || tag.StartsWithCSafe("<![")) && tag.EndsWithCSafe("]>"))
                                {
                                    // Process conditional comments
                                    keepProcessTags = !processTags;
                                    processTags = false;
                                }
                                break;
                        }
                    }

                    // Skip the tag if inside script
                    if (insideScript && !scriptTag)
                    {
                        continue;
                    }

                    bool expandSelfClosing = false;

                    if (processTags && !singleComment)
                    {
                        // Increase the level for opening or self closed tag
                        if (!endTag)
                        {
                            currentLevel++;
                        }

                        // Process tag content if required
                        if (settings.ProcessTagContent)
                        {
                            // Rebuild the tag from scratch
                            tagSb.Length = 0;
                            tagSb.Append("<");
                            if (endTag)
                            {
                                tagSb.Append("/");
                            }

                            string extraCssClass = null;

                            // Get the tag name to render
                            string originalRenderTag = (settings.LowerCase ? lowerTagName : tagName);
                            string renderTag = originalRenderTag;

                            // Convert table tags to div tags
                            if (settings.TableToDiv != ConvertTableEnum.None)
                            {
                                ConvertTag(lowerTagName, ref extraCssClass, ref renderTag);
                            }

                            bool allowConvert = ((convertLevel >= 0) && (convertLevel <= currentLevel));

                            if (!endTag)
                            {
                                // Process tag attributes
                                attrSb.Length = 0;

                                bool? changeConvert = null;
                                ProcessTagAttributes(settings, applicationPath, attrSb, classSb, styleSb, m, scriptTag, renderTag, extraCssClass, allowConvert, ref changeConvert);

                                // If conversion of tags is set, configure it
                                if (changeConvert != null)
                                {
                                    // Back the previous convert level
                                    convertLevels.Push(convertLevel);

                                    if (changeConvert.Value)
                                    {
                                        convertLevel = currentLevel;
                                        allowConvert = true;
                                    }
                                    else
                                    {
                                        convertLevel = -currentLevel;
                                        allowConvert = false;
                                    }
                                }

                                // Restore the render tag to original if conversion is not allowed
                                if (!allowConvert)
                                {
                                    renderTag = originalRenderTag;
                                }
                                else
                                {
                                    lowerTagName = renderTag.ToLowerCSafe();
                                }

                                tagSb.Append(renderTag, attrSb.ToString());

                                // Close the tag
                                if (selfClosed)
                                {
                                    if (IsSelfClosing(renderTag))
                                    {
                                        tagSb.Append(" /");
                                    }
                                    else
                                    {
                                        selfClosed = false;
                                        selfClosing = false;

                                        expandSelfClosing = true;
                                    }
                                }
                            }
                            else
                            {
                                // Restore the render tag to original if conversion is not allowed
                                if (!allowConvert)
                                {
                                    renderTag = originalRenderTag;
                                }
                                else
                                {
                                    lowerTagName = renderTag.ToLowerCSafe();
                                }

                                // Render the end tag name
                                tagSb.Append(renderTag);
                            }

                            tagSb.Append('>');

                            if (expandSelfClosing)
                            {
                                tagSb.Append("</", renderTag, ">");
                            }

                            tag = tagSb.ToString();
                        }
                    }

                    if (settings.SelfClose)
                    {
                        if (processTags)
                        {
                            // If some self-closing tag is opened, process it
                            if (lastOpenedSelfClosing != "")
                            {
                                if (!endTag || (lowerTagName != lastOpenedSelfLowerTagName))
                                {
                                    // Self closing tag but not closed, close the tag
                                    lastOpenedSelfClosing = lastOpenedSelfClosing.TrimEnd('>', ' ') + " />";

                                    // Notify on fix
                                    if (SystemContext.DevelopmentMode)
                                    {
                                        sb.Append("<!--FIX SELFCLOSING " + lastOpenedSelfLowerTagName + "-->");
                                    }
                                }
                                else
                                {
                                    // Add open tag so current tag can properly close it
                                    openTags.Push(lastOpenedSelfLowerTagName);
                                }

                                if (settings.Indent)
                                {
                                    sb.NewLine();
                                    sb.AppendIndent(indent);
                                }
                                sb.Append(lastOpenedSelfClosing);

                                lastOpenedSelfClosing = "";
                            }

                            // Add the normal text before tag
                            if (lastIndex < m.Index)
                            {
                                if (settings.Indent)
                                {
                                    // Handle the indentation
                                    if (endTag && (lowerTagName == "script"))
                                    {
                                        // Ensure the indentation for script content
                                        string text = sourceHtml.Substring(lastIndex, m.Index - lastIndex);
                                        text = TextHelper.ReformatCode(text, indent);
                                        if (!String.IsNullOrEmpty(text))
                                        {
                                            sb.NewLine();
                                            sb.Append(text);

                                            newLineBeforeTag = true;
                                        }
                                    }
                                    else
                                    {
                                        // Add the indented text
                                        string text = sourceHtml.Substring(lastIndex, m.Index - lastIndex);
                                        string trimmedText = text.Trim();

                                        if (!String.IsNullOrEmpty(trimmedText))
                                        {
                                            if (AllowIndent(endTag, lowerTagName, text))
                                            {
                                                // Add indented trimmed text
                                                sb.NewLine();
                                                sb.AppendIndent(indent);

                                                newLineBeforeTag = true;

                                                sb.Append(trimmedText);
                                            }
                                            else
                                            {
                                                // Add original text without indent
                                                sb.Append(text);

                                                newLineBeforeTag = false;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Add normal way
                                    sb.Append(sourceHtml, lastIndex, m.Index - lastIndex);
                                }
                            }

                            if (endTag)
                            {
                                bool closed = false;

                                // If end tag, close open tags that do not match if one of them is matching
                                if (openTags.Contains(lowerTagName) || closeConditionalComment)
                                {
                                    while (openTags.Count > 0)
                                    {
                                        string lastOpen = openTags.Pop();

                                        // Decrease the processing level
                                        DecreaseLevel(convertLevels, ref currentLevel, ref convertLevel);

                                        // Last open is current, keep closed
                                        if (lastOpen == lowerTagName)
                                        {
                                            closed = true;
                                            break;
                                        }
                                        else if (closeConditionalComment && (lastOpen.StartsWithCSafe("!--[") || lastOpen.StartsWithCSafe("![")))
                                        {
                                            closed = true;
                                            break;
                                        }
                                        else
                                        {
                                            if (settings.Indent)
                                            {
                                                indent--;

                                                // Add closing tag with the given indent
                                                if (AllowIndent(endTag, lowerTagName, null))
                                                {
                                                    sb.NewLine();
                                                    sb.AppendIndent(indent);

                                                    newLineBeforeTag = true;
                                                }

                                                indent++;
                                            }

                                            // Last open is not current, add its closing tag
                                            sb.Append("</" + lastOpen + ">");

                                            // Notify on fix
                                            if (SystemContext.DevelopmentMode)
                                            {
                                                sb.Append("<!--FIX CLOSING " + lastOpen + "-->");
                                            }
                                        }
                                    }
                                }

                                // If not closed, open the tag before adding the closing
                                if (!closed)
                                {
                                    if (settings.Indent)
                                    {
                                        // Add opening tag with the given indent
                                        sb.NewLine();
                                        sb.AppendIndent(indent);

                                        indent++;
                                    }
                                    sb.Append("<" + lowerTagName + ">");

                                    // Notify on fix
                                    if (SystemContext.DevelopmentMode)
                                    {
                                        sb.Append("<!--FIX OPENING " + lowerTagName + "-->");
                                    }
                                }

                                closeBeforeNextOpen = false;
                            }
                            else
                            {
                                // Starting tag
                                if (selfClosing)
                                {
                                    if (!selfClosed)
                                    {
                                        // Postpone the execution to the next tag processing to see if the tag is properly closed or not
                                        lastOpenedSelfClosing = tag;
                                        lastOpenedSelfLowerTagName = lowerTagName;

                                        tag = "";
                                    }
                                }
                                else
                                {
                                    // Close the tag if required
                                    if (closeBeforeNextOpen)
                                    {
                                        if (settings.Indent)
                                        {
                                            indent--;

                                            // Add closing tag with the given indent
                                            sb.NewLine();
                                            sb.AppendIndent(indent);
                                        }

                                        string lastOpen = openTags.Pop();
                                        sb.Append("</" + lastOpen + ">");

                                        // Decrease the current level
                                        DecreaseLevel(convertLevels, ref currentLevel, ref convertLevel);

                                        closeBeforeNextOpen = false;
                                    }

                                    // Register opening tag
                                    if (!expandSelfClosing)
                                    {
                                        openTags.Push(lowerTagName);
                                    }
                                }
                            }

                            if (settings.Indent)
                            {
                                // Add the indented tag
                                if (endTag)
                                {
                                    indent--;
                                }

                                // Do not add the new line and indent in case of BR tag
                                if ((!endTag || newLineBeforeTag) && (lowerTagName != "br") && AllowIndent(endTag, lowerTagName, null))
                                {
                                    sb.NewLine();
                                    sb.AppendIndent(indent);
                                }
                                sb.Append(tag);

                                if (!endTag && !selfClosed && (lowerTagName != "html"))
                                {
                                    indent++;
                                }
                            }
                            else
                            {
                                // Add normal way
                                sb.Append(tag);
                            }
                            lastIndex = m.Index + m.Length;
                        }

                        switch (lowerTagName)
                        {
                            // Enable processing for end script tag
                            case "script":
                                if (keepProcessTags)
                                {
                                    processTags = endTag;
                                }
                                break;

                            // Tags which do not require end tag
                            case "option":
                                if (!endTag)
                                {
                                    closeBeforeNextOpen = true;
                                }
                                break;
                        }
                    }
                    else
                    {
                        // Add the normal text before tag
                        if (lastIndex < m.Index)
                        {
                            if (settings.Indent)
                            {
                                // Handle the indentation
                                if (endTag && (lowerTagName == "script"))
                                {
                                    // Ensure the indentation for script content
                                    string text = sourceHtml.Substring(lastIndex, m.Index - lastIndex);
                                    text = TextHelper.ReformatCode(text, indent);
                                    if (!String.IsNullOrEmpty(text))
                                    {
                                        sb.NewLine();
                                        sb.Append(text);

                                        newLineBeforeTag = true;
                                    }
                                }
                                else
                                {
                                    // Add the indented text
                                    string text = sourceHtml.Substring(lastIndex, m.Index - lastIndex).Trim();
                                    if (!String.IsNullOrEmpty(text))
                                    {
                                        sb.NewLine();
                                        sb.AppendIndent(indent);
                                        sb.Append(text);

                                        newLineBeforeTag = true;
                                    }
                                }
                            }
                            else
                            {
                                // Add normal way
                                sb.Append(sourceHtml, lastIndex, m.Index - lastIndex);
                            }
                        }

                        // Do not self-close tags - do not parse the structure
                        if (settings.Indent)
                        {
                            // Add the indented tag
                            if (endTag)
                            {
                                indent--;
                            }

                            // Do not add the new line and indent in case of BR tag
                            if ((!endTag || newLineBeforeTag) && (lowerTagName != "br") && AllowIndent(endTag, lowerTagName, null))
                            {
                                sb.NewLine();
                                sb.AppendIndent(indent);
                            }
                            sb.Append(tag);

                            if (!endTag && !selfClosed && (lowerTagName != "html"))
                            {
                                indent++;
                            }
                        }
                        else
                        {
                            // Add normal way
                            sb.Append(tag);
                        }

                        lastIndex = m.Index + m.Length;

                        if (endTag)
                        {
                            // Decrease the current level
                            DecreaseLevel(convertLevels, ref currentLevel, ref convertLevel);
                        }
                    }

                    switch (lowerTagName)
                    {
                        // Enable processing for end comment tag
                        case "-->":
                        case "<![endif]-->":
                            {
                                if (!keepProcessTags)
                                {
                                    processTags = true;
                                }
                                keepProcessTags = false;
                            }
                            break;
                    }
                }

                // If some self-closing tag is opened, process it
                if (lastOpenedSelfClosing != "")
                {
                    // Self closing tag but not closed, close the tag
                    lastOpenedSelfClosing = lastOpenedSelfClosing.TrimEnd('>', ' ') + " />";

                    // Notify on fix
                    if (SystemContext.DevelopmentMode)
                    {
                        sb.Append("<!--FIX SELFCLOSING " + lastOpenedSelfLowerTagName + "-->");
                    }

                    if (settings.Indent)
                    {
                        sb.NewLine();
                        sb.AppendIndent(indent);
                    }
                    sb.Append(lastOpenedSelfClosing);
                }

                // Add the rest of the HTML code
                if (lastIndex < sourceHtml.Length)
                {
                    if (settings.Indent)
                    {
                        // Add the next indented text
                        string text = sourceHtml.Substring(lastIndex).Trim();
                        if (!String.IsNullOrEmpty(text))
                        {
                            sb.NewLine();
                            sb.AppendIndent(indent);
                            sb.Append(text);
                        }
                    }
                    else
                    {
                        // Add normal way
                        sb.Append(sourceHtml, lastIndex, sourceHtml.Length - lastIndex);
                    }
                }

                // Close the rest of the tags
                while (openTags.Count > 0)
                {
                    string lastOpen = openTags.Pop();

                    // Decrease the processing level
                    DecreaseLevel(convertLevels, ref currentLevel, ref convertLevel);

                    if (settings.Indent)
                    {
                        indent--;

                        // Add closing tag with the given indent
                        if (AllowIndent(true, lastOpen, null))
                        {
                            sb.NewLine();
                            sb.AppendIndent(indent);
                        }
                    }

                    // Add closing tag
                    sb.Append("</" + lastOpen + ">");

                    // Notify on fix
                    if (SystemContext.DevelopmentMode)
                    {
                        sb.Append("<!--FIX CLOSING " + lastOpen + "-->");
                    }
                }

                return sb.ToString();
            }
            else
            {
                // Not not perform any actions
                return sourceHtml;
            }
        }


        /// <summary>
        /// Decreases the current level and resets conversion level if leaving out
        /// </summary>
        /// <param name="convertLevels">Stack of the backup of the levels</param>
        /// <param name="currentLevel">Current level</param>
        /// <param name="convertLevel">Conversion level</param>
        private static void DecreaseLevel(Stack<int> convertLevels, ref int currentLevel, ref int convertLevel)
        {
            if (currentLevel == Math.Abs(convertLevel))
            {
                if (convertLevels.Count > 0)
                {
                    // Restore the conversion level
                    convertLevel = convertLevels.Pop();
                }
                else
                {
                    // If no level backed up, do not convert
                    convertLevel = NOT_CONVERT;
                }
            }
            currentLevel--;
        }


        /// <summary>
        /// Converts the given tag to its appropriate alternative
        /// </summary>
        /// <param name="lowerTagName">Lowercase tag name</param>
        /// <param name="extraCssClass">Extra CSS class to add to the new tag</param>
        /// <param name="renderTag">Returning the tag to render in the place of the given tag</param>
        private static void ConvertTag(string lowerTagName, ref string extraCssClass, ref string renderTag)
        {
            if (lowerTagName.StartsWith("t", StringComparison.Ordinal))
            {
                switch (lowerTagName)
                {
                    case "table":
                    case "tr":
                    case "td":
                    case "th":
                    case "thead":
                    case "tbody":
                    case "tfoot":
                        {
                            // Convert tag to div and add extra CSS class
                            extraCssClass = lowerTagName;
                            renderTag = "div";

                            return;
                        }
                }
            }
            else if (lowerTagName.StartsWith("c", StringComparison.Ordinal))
            {
                switch (lowerTagName)
                {
                    case "caption":
                    case "col":
                    case "colgroup":
                        {
                            // Convert tag to div and add extra CSS class
                            extraCssClass = lowerTagName;
                            renderTag = "div";

                            return;
                        }
                }
            }
        }


        /// <summary>
        /// Processes the attributes within the given tag
        /// </summary>
        /// <param name="settings">Filter configuration</param>
        /// <param name="applicationPath">Application path for resolving filter</param>
        /// <param name="tagsb">Output string builder</param>
        /// <param name="classSb">Helper string builder for the class attribute</param>
        /// <param name="styleSb">Helper string builder for the style attribute</param>
        /// <param name="m">Tag matching object</param>
        /// <param name="scriptTag">Flag whether the tag is script tag</param>
        /// <param name="lowerTagName">Lower case tag name</param>
        /// <param name="extraCssClass">Extra CSS class to add to class attribute</param>
        /// <param name="allowConvert">Returning flag whether the tag is allowed to be converted or not</param>
        /// <param name="changeConvert">Returning flag whether the conversion should be enabled or not</param>
        private static void ProcessTagAttributes(FixXHTMLSettings settings, string applicationPath, StringBuilder tagsb, StringBuilder classSb, StringBuilder styleSb, Match m, bool scriptTag, string lowerTagName, string extraCssClass, bool allowConvert, ref bool? changeConvert)
        {
            // Add the parameters
            Group nameGroup = m.Groups[2];
            Group valueGroup = m.Groups[3];

            bool typePresent = false;

            bool attributes = settings.Attributes;
            bool toDiv = (settings.TableToDiv != ConvertTableEnum.None);
            bool mayModify = settings.MayModifyCssClass;

            bool handleHtml5 = (settings.HTML5 || toDiv);
            bool idRendered = false;

            bool classAtTheEnd = false;
            bool styleAtTheEnd = false;

            string extraCssStyle = null;
            string localExtraCssClass = null;

            bool convertDivAttributes = allowConvert && (lowerTagName == "div");

            // Process all attributes
            int paramCount = nameGroup.Captures.Count;
            for (int i = 0; i < paramCount; i++)
            {
                // Add the name
                string name = nameGroup.Captures[i].ToString();
                string lowerName = name.ToLowerCSafe();
                string value = valueGroup.Captures[i].ToString();

                // Handle the ID parameter
                bool isId = lowerName.EqualsCSafe("id");
                if (isId)
                {
                    // Skip duplicate ID
                    if (idRendered)
                    {
                        continue;
                    }
                    idRendered = true;
                }

                // Handle the obsolete HTML5 attributes
                if (handleHtml5)
                {
                    if (mConvertToCssClass.Contains(lowerName) || (convertDivAttributes && mDivConvertToCssClass.Contains(lowerName)))
                    {
                        // Trim the quotes from value
                        string cssValue = TrimQuotes(value).ToLowerCSafe();

                        // Convert the attribute to extra CSS class
                        if (String.IsNullOrEmpty(localExtraCssClass))
                        {
                            localExtraCssClass = String.Format("{0}_{1}", lowerName, cssValue);
                        }
                        else
                        {
                            localExtraCssClass += String.Format(" {0}_{1}", lowerName, cssValue);
                        }
                        continue;
                    }

                    if (mAttributesConvertibleToInlineStyle.Contains(lowerName))
                    {
                        // Trim the quotes from value
                        string styleValue = TrimQuotes(value);

                        if (mPixelOrPercentageAttributes.Contains(lowerName))
                        {
                            styleValue = GetLengthValueForCss(styleValue);
                        }

                        // Move to CSS style
                        if (!String.IsNullOrEmpty(styleValue))
                        {
                            // Ensure proper ending of the last style
                            if (!string.IsNullOrEmpty(extraCssStyle))
                            {
                                if (!extraCssStyle.TrimEnd().EndsWithCSafe(";"))
                                {
                                    extraCssStyle += ";";
                                }
                                extraCssStyle += " ";
                            }
                            extraCssStyle += String.Format("{0}: {1};", lowerName, styleValue);
                        }
                        continue;
                    }
                }

                StringBuilder attrSb = tagsb;
                bool keepOpen = false;

                // Leave the class as the last tag in case the filter may add extra class
                bool isClass = (lowerName == "class");
                if (mayModify)
                {
                    if (isClass)
                    {
                        // Use class string builder
                        classSb.Length = 0;
                        attrSb = classSb;
                        classAtTheEnd = true;
                        keepOpen = true;
                    }
                    else if (lowerName == "style")
                    {
                        // Use style string builder
                        styleSb.Length = 0;
                        attrSb = styleSb;
                        keepOpen = true;
                        styleAtTheEnd = true;
                    }
                }

                // Lowercase tag name
                attrSb.Append(" ", (settings.LowerCase ? lowerName : name));

                // Add the value
                if (value == "")
                {
                    // Explicit value for non-value items
                    if (attributes)
                    {
                        attrSb.Append("=\"", lowerName);
                        if (!keepOpen)
                        {
                            attrSb.Append('"');
                        }
                    }
                }
                else if (value == null)
                {
                    // Value removed on purpose, render empty value
                    attrSb.Append("=\"");
                    if (!keepOpen)
                    {
                        attrSb.Append('"');
                    }
                }
                else
                {
                    attrSb.Append('=');

                    // Check whether the tag and it's children are allowed to be converted
                    if (isClass && toDiv)
                    {
                        if (value.Contains(DIVS))
                        {
                            changeConvert = allowConvert = true;
                        }
                        else if (value.Contains(NO_DIVS))
                        {
                            changeConvert = allowConvert = false;
                        }
                    }

                    // Encode double quotes in attribute value if it was enclosed in single quotes, because switching to double quotes would break the attribute
                    bool encodeDoubleQuotes = value.StartsWith("'", StringComparison.Ordinal);
                   
                    // Trim the quotes from value
                    value = TrimQuotes(value);

                    if (encodeDoubleQuotes)
                    {
                        value = value.Replace("\"", "&quot;");
                    }

                    // Starting quotes
                    attrSb.Append('"');

                    if (attributes)
                    {
                        // Trim the value
                        value = value.Trim();
                    }

                    // Resolve value
                    if (settings.ResolveUrl && value.StartsWithCSafe("~/"))
                    {
                        if (applicationPath != "/")
                        {
                            // Web site is not in the wwwroot folder -> append appPath
                            attrSb.Append(applicationPath);
                        }

                        // Append relative URL without '~'
                        attrSb.Append(value.Substring(1));
                    }
                    else
                    {
                        attrSb.Append(value);
                    }

                    // Ending quotes
                    if (!keepOpen)
                    {
                        attrSb.Append('"');
                    }
                }

                // Catch the script parameters
                if (settings.Javascript && scriptTag)
                {
                    if (lowerName == "type")
                    {
                        typePresent = true;
                    }
                }
            }

            if (allowConvert)
            {
                // Add extra CSS class to the local extra CSS class
                if (String.IsNullOrEmpty(localExtraCssClass))
                {
                    localExtraCssClass = extraCssClass;
                }
                else
                {
                    localExtraCssClass = extraCssClass + " " + localExtraCssClass;
                }
            }

            // Add the extra attributes
            AddExtraAttribute(tagsb, classSb, localExtraCssClass, classAtTheEnd, "class");
            AddExtraAttribute(tagsb, styleSb, extraCssStyle, styleAtTheEnd, "style");

            // Add missing script parameters
            if (settings.Javascript && scriptTag)
            {
                if (!typePresent)
                {
                    tagsb.Append(" type=\"text/javascript\"");
                }
            }
        }


        /// <summary>
        /// Use the method to convert element attribute value that represents a length in pixels or percentage to CSS compliant value. If the value doesn't represent 
        /// a length then the original value is returned unmodified.
        /// </summary>
        /// <param name="rawValue">Element attribute value.</param>
        /// <returns>String representation of length including a unit.</returns>
        private static string GetLengthValueForCss(string rawValue)
        {
            if (rawValue.EndsWithAny(StringComparison.InvariantCultureIgnoreCase, "px", "%"))
            {
                return rawValue;
            }

            decimal parsedValue;

            if (Decimal.TryParse(rawValue, out parsedValue))
            {
                return parsedValue + "px";
            }

            return rawValue;
        }


        /// <summary>
        /// Adds the extra attributes to the end of the tag
        /// </summary>
        /// <param name="tagsb">Tag string builder</param>
        /// <param name="attrSb">Attribute string builder</param>
        /// <param name="extraValue">Extra value</param>
        /// <param name="attrAtTheEnd">Flag whether the attribute is moved to the end of the tag</param>
        /// <param name="attributeName">Attribute name</param>
        private static void AddExtraAttribute(StringBuilder tagsb, StringBuilder attrSb, string extraValue, bool attrAtTheEnd, string attributeName)
        {
            // Add class to the end
            if (attrAtTheEnd)
            {
                // Append the class
                tagsb.Append(attrSb);

                if (!String.IsNullOrEmpty(extraValue))
                {
                    tagsb.Append(" ", extraValue);
                }

                // Close the attribute
                tagsb.Append('"');
            }
            else if (!String.IsNullOrEmpty(extraValue))
            {
                // Class not generated, generate the extra class explicitly
                tagsb.Append(" ", attributeName, "=\"", extraValue, '\"');
            }
        }


        /// <summary>
        /// Trims the quotes from the given value
        /// </summary>
        /// <param name="value">Value to trim</param>
        private static string TrimQuotes(string value)
        {
            // Add value in quotes
            bool trimStart = false;
            bool trimEnd = false;

            if (value.StartsWithAny(StringComparison.CurrentCulture, "'", "\""))
            {
                trimStart = true;
            }
            if ((value.Length > 1) && value.EndsWithAny(StringComparison.CurrentCulture, "'", "\""))
            {
                trimEnd = true;
            }

            // Trim the value
            if (trimStart)
            {
                value = trimEnd ? value.Substring(1, value.Length - 2) : value.Substring(1);
            }
            else if (trimEnd)
            {
                value = value.Substring(0, value.Length - 1);
            }

            return value;
        }


        /// <summary>
        /// Returns true if the indentation is allowed under given parameters
        /// </summary>
        /// <param name="endTag">End tag flag</param>
        /// <param name="lowerTagName">Lower tag name</param>
        /// <param name="innerText">Inner tag text</param>
        private static bool AllowIndent(bool endTag, string lowerTagName, string innerText)
        {
            switch (lowerTagName)
            {
                case "textarea":
                case "pre":
                    // Exceptions
                    return !endTag;
            }

            // If the inner text is empty, handle normally
            if (String.IsNullOrEmpty(innerText))
            {
                return true;
            }

            // Text in between must be empty or contain white spaces to be indented
            if (!Char.IsWhiteSpace(innerText[0]) || !Char.IsWhiteSpace(innerText[innerText.Length - 1]))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns true if the tag is a self closing tag
        /// </summary>
        /// <param name="lowerTagName">Tag name</param>
        private static bool IsSelfClosing(string lowerTagName)
        {
            return mSelfClosingTags.Contains(lowerTagName);
        }


        /// <summary>
        /// Match evaluator for FixUrl regex replacement.
        /// </summary>
        /// <param name="match">Match</param>
        private static string FixUrlEvaluator(Match match)
        {
            string output = match.ToString();
            int shift = -match.Index;
            foreach (Capture capture in match.Groups[1].Captures)
            {
                string before = output.Substring(0, capture.Index + shift);
                string after = output.Substring(capture.Index + capture.Length + shift);
                bool urlFixed = false;

                if (RegExStartsWithAmpNumber.IsMatch(after))
                {
                    output = before + "&" + after;
                    urlFixed = true;
                    shift += 1 - capture.Length;
                }

                if (!urlFixed)
                {
                    foreach (string specialChar in HTML_SPECIAL_CHARS)
                    {
                        // Exclude & special chars from replacing
                        if (after.StartsWith(specialChar, StringComparison.Ordinal))
                        {
                            output = before + "&" + after;
                            urlFixed = true;
                            shift += 1 - capture.Length;
                        }
                    }
                }

                if (!urlFixed)
                {
                    shift = shift + 5 - capture.Length;
                    output = before + "&amp;" + after;
                }
            }

            return output;
        }

        #endregion
    }
}