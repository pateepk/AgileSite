using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Methods for resolving macros within discussion posts.
    /// </summary>
    public class DiscussionMacroResolver : CoreMethods
    {
        #region "Variables"

        private bool mEnableBold = true;
        private bool mEnableItalics = true;
        private bool mEnableStrikeThrough = true;
        private bool mEnableUnderline = true;

        private bool mEnableCode = true;
        private bool mEnableQuote = true;
        private bool mEnableSize = true;
        private bool mEnableColor = true;

        private bool mEnableImage = true;
        private bool mEnableURL = true;

        private bool mEncodeText = true;
        private bool mConvertLineBreaksToHTML = true;
        private bool mResolveToPlainText = false;

        private int mMaxImageSideSize = 600;
        private int mMaxTextSize = 20;
        private string mTextSizeUnit = "pt";

        private string mQuotePostText = string.Empty;

        private static Regex mRegExBBTagExpression = null;
        private static Regex mRegExBBTag = null;

        #endregion


        #region "Regex Properties"

        /// <summary>
        /// Regular expression for the parsed BB tag expression.
        /// </summary>
        public static Regex RegExBBTagExpression
        {
            get
            {
                if (mRegExBBTagExpression == null)
                {
                    // Expression groups:                                      (1:tg)        (2:pm )            (1:tg    )(2:pm)         (3:text)
                    mRegExBBTagExpression = RegexHelper.GetRegex("\\[\\s*(?:(?:(\\w+)\\s*(?:=([^]]*))?)|(?:/\\s*(?<1>\\w+)(?<2>)))\\s*\\]([^\\[]*)");
                }

                return mRegExBBTagExpression;
            }
            set
            {
                mRegExBBTagExpression = value;
            }
        }


        /// <summary>
        /// Regular expression for the BB tag.
        /// </summary>
        public static Regex RegExBBTag
        {
            get
            {
                if (mRegExBBTag == null)
                {
                    // Expression groups: none                                   
                    mRegExBBTag = RegexHelper.GetRegex("\\[\\s*(?:(?:\\w+\\s*(?:=[^]]*)?)|(?:/\\s*\\w+))\\s*\\]");
                }

                return mRegExBBTag;
            }
            set
            {
                mRegExBBTag = value;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the value that indicates whether rel="nofollow" should be used for resolved links
        /// </summary>
        public bool UseNoFollowForLinks
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates whether strikeThrough macros are enabled.
        /// </summary>
        public bool EnableStrikeThrough
        {
            get
            {
                return mEnableStrikeThrough;
            }
            set
            {
                mEnableStrikeThrough = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether underline macros are enabled.
        /// </summary>
        public bool EnableUnderline
        {
            get
            {
                return mEnableUnderline;
            }
            set
            {
                mEnableUnderline = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether italics macros are enabled.
        /// </summary>
        public bool EnableItalics
        {
            get
            {
                return mEnableItalics;
            }
            set
            {
                mEnableItalics = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether bold macros are enabled.
        /// </summary>
        public bool EnableBold
        {
            get
            {
                return mEnableBold;
            }
            set
            {
                mEnableBold = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether code macros are enabled.
        /// </summary>
        public bool EnableCode
        {
            get
            {
                return mEnableCode;
            }
            set
            {
                mEnableCode = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether color macros are enabled.
        /// </summary>
        public bool EnableColor
        {
            get
            {
                return mEnableColor;
            }
            set
            {
                mEnableColor = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether size macros are enabled.
        /// </summary>
        public bool EnableSize
        {
            get
            {
                return mEnableSize;
            }
            set
            {
                mEnableSize = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether URL macros are enabled.
        /// </summary>
        public bool EnableURL
        {
            get
            {
                return mEnableURL;
            }
            set
            {
                mEnableURL = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether Quote macros are enabled.
        /// </summary>
        public bool EnableQuote
        {
            get
            {
                return mEnableQuote;
            }
            set
            {
                mEnableQuote = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether Image macros are enabled.
        /// </summary>
        public bool EnableImage
        {
            get
            {
                return mEnableImage;
            }
            set
            {
                mEnableImage = value;
            }
        }


        /// <summary>
        /// Gets or sets max side size of image.
        /// </summary>
        public int MaxImageSideSize
        {
            get
            {
                return mMaxImageSideSize;
            }
            set
            {
                mMaxImageSideSize = value;
            }
        }


        /// <summary>
        /// Gets or sets max text size of image.
        /// </summary>
        public int MaxTextSize
        {
            get
            {
                return mMaxTextSize;
            }
            set
            {
                mMaxTextSize = value;
            }
        }


        /// <summary>
        /// Gets or sets the text size unit which should be used.
        /// </summary>
        public string TextSizeUnit
        {
            get
            {
                return mTextSizeUnit;
            }
            set
            {
                mTextSizeUnit = value;
            }
        }


        /// <summary>
        /// Gets or sets the text which should be displayed in quote area after user name.
        /// </summary>
        public string QuotePostText
        {
            get
            {
                return mQuotePostText;
            }
            set
            {
                mQuotePostText = value;
            }
        }


        /// <summary>
        /// If true, the text outside of the macros is encoded.
        /// </summary>
        public bool EncodeText
        {
            get
            {
                return mEncodeText;
            }
            set
            {
                mEncodeText = value;
            }
        }


        /// <summary>
        /// If true, the line breaks are converted to HTML br tags.
        /// </summary>
        public bool ConvertLineBreaksToHTML
        {
            get
            {
                return mConvertLineBreaksToHTML;
            }
            set
            {
                mConvertLineBreaksToHTML = value;
            }
        }


        /// <summary>
        /// If true, the tags are resolved to the plain text version.
        /// </summary>
        public bool ResolveToPlainText
        {
            get
            {
                return mResolveToPlainText;
            }
            set
            {
                mResolveToPlainText = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Discussion macro helper constructor.
        /// </summary>
        public DiscussionMacroResolver()
        {
        }


        /// <summary>
        /// Discussion macro helper constructor.
        /// </summary>
        /// <param name="enableImage">Indicates whether image macro is enabled</param>
        /// <param name="enableQuote">Indicates whether quote macro is enabled</param>
        /// <param name="enableUrl">Indicates ehether URL macro is enabled</param>
        /// <param name="maxSideSize">Sets  image maxsidesize</param>
        /// <param name="quotePostText">Sets the text which should be displayed in quote area after user name</param>
        /// <param name="invalidateJavaScript">Sets whether JavaScript should be invalidated</param>
        public DiscussionMacroResolver(bool enableImage, bool enableQuote, bool enableUrl, int maxSideSize, string quotePostText, bool invalidateJavaScript)
        {
            EnableImage = enableImage;
            EnableQuote = enableQuote;
            EnableURL = enableUrl;

            MaxImageSideSize = maxSideSize;
            QuotePostText = quotePostText;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns the forum discussion bold tag.
        /// </summary>
        /// <param name="text">Text within the tag</param>
        public static string GetBold(string text)
        {
            return "[b]" + text + "[/b]";
        }


        /// <summary>
        /// Returns the forum discussion italics tag.
        /// </summary>
        /// <param name="text">Text within the tag</param>
        public static string GetItalics(string text)
        {
            return "[i]" + text + "[/i]";
        }


        /// <summary>
        /// Returns the forum discussion underline tag.
        /// </summary>
        /// <param name="text">Text within the tag</param>
        public static string GetUnderline(string text)
        {
            return "[u]" + text + "[/u]";
        }


        /// <summary>
        /// Returns the forum discussion strike through tag.
        /// </summary>
        /// <param name="text">Text within the tag</param>
        public static string GetStrikeThrough(string text)
        {
            return "[s]" + text + "[/s]";
        }


        /// <summary>
        /// Returns the forum discussion URL tag.
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="text">Text within the tag</param>
        public static string GetURL(string url, string text)
        {
            string result = "[url";

            if (url != null)
            {
                if (text == null)
                {
                    text = url;
                }
                else
                {
                    result += "=" + url;
                }
            }
            result += "]" + text + "[/url]";

            return result;
        }


        /// <summary>
        /// Returns the forum discussion image tag.
        /// </summary>
        /// <param name="url">Image URL</param>
        public static string GetImage(string url)
        {
            return "[img]" + url + "[/img]";
        }


        /// <summary>
        /// Returns the forum discussion color tag.
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="text">Text within the tag</param>
        public static string GetColor(string color, string text)
        {
            return "[color=" + color + "]" + text + "[/color]";
        }


        /// <summary>
        /// Returns the forum discussion size tag.
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="text">Text within the tag</param>
        public static string GetSize(int size, string text)
        {
            return "[size=" + size.ToString() + "]" + text + "[/size]";
        }


        /// <summary>
        /// Returns the forum discussion Quote tag.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="text">Text within the tag</param>
        public static string GetQuote(string userName, string text)
        {
            string result = "[quote";

            if (!String.IsNullOrEmpty(userName))
            {
                result += "=" + userName;
            }

            result += "]" + text + "[/quote]";

            return result;
        }


        /// <summary>
        /// Remove all BB tags from the text.
        /// </summary>
        /// <param name="sourceText">Text containing BB macros to be removed</param>
        public static string RemoveTags(string sourceText)
        {
            if (sourceText == null)
            {
                return null;
            }

            return RegExBBTag.Replace(sourceText, "");
        }


        /// <summary>
        /// Resolves BB macros in a given text.
        /// </summary>
        /// <param name="sourceText">Text to resolve</param>
        public string ResolveMacros(string sourceText)
        {
            return ResolveMacros(sourceText, null);
        }


        /// <summary>
        /// Resolves BB macros in a given text.
        /// </summary>
        /// <param name="sourceText">Text to resolve</param>
        /// <param name="domainName">Domain name for URL resolving</param>
        public string ResolveMacros(string sourceText, string domainName)
        {
            if (sourceText == null)
            {
                return null;
            }

            // Get all tags
            MatchCollection tags = RegExBBTagExpression.Matches(sourceText);
            if (tags.Count == 0)
            {
                if (EncodeText || ConvertLineBreaksToHTML)
                {
                    sourceText = ProcessText(sourceText);
                }

                return sourceText;
            }

            Stack<string> openTags = new Stack<string>();
            StringBuilder sb = new StringBuilder((int)(sourceText.Length * 1.1));

            int lastIndex = 0;
            string automaticText = null;

            // Process all tags
            foreach (Match m in tags)
            {
                // Process the tag
                string tag = m.ToString();

                bool endTag = tag.StartsWithCSafe("[/");

                string tagName = m.Groups[1].ToString();
                string lowerTagName = tagName.ToLowerCSafe();

                string parameter = m.Groups[2].ToString();
                string text = m.Groups[3].ToString();

                string renderTag = null;
                string renderText = null;

                // Clear automatic text if there is an inherited tag
                if (!endTag)
                {
                    automaticText = null;
                }

                if (ResolveToPlainText)
                {
                    if (!endTag)
                    {
                        renderTag = "";
                        renderText = text;

                        // Process the tags
                        switch (lowerTagName)
                        {
                            // URL tag
                            case "url":
                                {
                                    if (string.IsNullOrEmpty(parameter))
                                    {
                                        parameter = text;
                                    }

                                    renderText = "(" + string.Format(GetString("discussionmacrohelper.plaintextlink"), parameter);
                                    if ((text != "") && (text != parameter))
                                    {
                                        renderText += " : " + text;
                                    }
                                    else
                                    {
                                        renderText += " ";
                                    }
                                }
                                break;

                            // Image tag
                            case "img":
                                {
                                    renderText = "(" + GetString("discussionmacrohelper.plaintextimage") + text;
                                }
                                break;

                            // Quote tag
                            case "quote":
                                {
                                    if (!String.IsNullOrEmpty(parameter))
                                    {
                                        string cited = parameter;
                                        cited = string.Format(GetString("discussionmacrohelper.plaintextcite"), cited);

                                        renderText = cited;
                                    }
                                    else
                                    {
                                        renderText = "";
                                    }

                                    renderText = "\r\n" + renderText + "\"" + text;
                                }
                                break;
                            // All others render as given
                            default:
                                renderTag = null;
                                break;
                        }
                    }
                    else
                    {
                        // Render all text after the end tag
                        renderText = text;
                    }
                }
                else
                {
                    // Process the tags
                    switch (lowerTagName)
                    {
                        // Bold
                        case "b":
                        case "strong":
                            if (EnableBold)
                            {
                                if (!endTag)
                                {
                                    renderTag = "<strong class=\"BBStrong\">";
                                }
                                renderText = text;
                            }
                            break;

                        // Italics
                        case "i":
                        case "em":
                            if (EnableItalics)
                            {
                                if (!endTag)
                                {
                                    renderTag = "<em class=\"BBItalics\">";
                                }
                                renderText = text;
                            }
                            break;

                        // Underlined
                        case "u":
                            if (EnableUnderline)
                            {
                                if (!endTag)
                                {
                                    renderTag = "<span class=\"BBUnderline\" style=\"text-decoration: underline;\">";
                                }
                                renderText = text;
                            }
                            break;

                        // Strikethrough
                        case "s":
                            if (EnableStrikeThrough)
                            {
                                if (!endTag)
                                {
                                    renderTag = "<s class=\"BBStrike\">";
                                }
                                renderText = text;
                            }
                            break;

                        // URL
                        case "url":
                            if (EnableURL)
                            {
                                renderText = text;

                                if (!endTag)
                                {
                                    // Get url from the parameter or text
                                    string url = parameter;
                                    if (String.IsNullOrEmpty(url))
                                    {
                                        url = text;
                                    }
                                    else if (String.IsNullOrEmpty(text))
                                    {
                                        automaticText = url.TrimStart('?', '#', '~');
                                        //text = url;
                                    }

                                    if (!URLHelper.ContainsProtocol(url) && !url.StartsWithCSafe("~") && !url.StartsWithCSafe("/") && !url.StartsWithCSafe(".") && !url.StartsWithCSafe("?") && !url.StartsWithCSafe("#"))
                                    {
                                        url = "http://" + url;
                                    }

                                    // Validate the URL
                                    if (IsURL(url))
                                    {
                                        url = URLHelper.GetAbsoluteUrl(url, domainName);
                                        url = HTMLHelper.HTMLEncode(url);
                                        renderTag = "<a href=\"" + url + "\" class=\"BBLink\"" + (UseNoFollowForLinks ? " rel=\"nofollow\" " : "") + ">";
                                    }
                                    else
                                    {
                                        renderText = null;
                                        automaticText = null;
                                    }
                                }
                            }
                            break;

                        // Quotation
                        case "quote":
                            if (EnableQuote)
                            {
                                if (!endTag)
                                {
                                    renderTag = "<blockquote class=\"BBQuote\"><div>";
                                    if (!String.IsNullOrEmpty(parameter))
                                    {
                                        string cited = ProcessText(parameter);
                                        cited = string.Format(GetString("discussionmacrohelper.cite"), cited);

                                        renderTag += "<cite>" + cited + "</cite> ";
                                    }
                                }

                                renderText = text;
                            }
                            break;

                        // Image
                        case "img":
                            if (EnableImage)
                            {
                                if (endTag)
                                {
                                    renderText = text;
                                }
                                else
                                {
                                    // Parameter is the size (optional)
                                    string size = parameter;

                                    int maxAllowedSideSize = MaxImageSideSize;

                                    int xIndex = size.IndexOfCSafe("x", true);
                                    int width = 0;
                                    int height = 0;
                                    int maxSideSize = 0;
                                    if (xIndex >= 0)
                                    {
                                        // Width / height
                                        width = ValidationHelper.GetInteger(size.Substring(0, xIndex), 0);
                                        height = ValidationHelper.GetInteger(size.Substring(xIndex + 1), 0);

                                        // Ensure the max side size of the image
                                        int[] newSize = ImageHelper.EnsureImageDimensions(0, 0, maxAllowedSideSize, width, height);
                                        width = newSize[0];
                                        height = newSize[1];
                                    }
                                    else
                                    {
                                        // Max side size
                                        maxSideSize = ValidationHelper.GetInteger(size, 0);
                                        if (((maxSideSize > 0) && (maxAllowedSideSize > 0) && (maxSideSize > maxAllowedSideSize)) || ((maxSideSize == 0) && (maxAllowedSideSize > 0)))
                                        {
                                            // Ensure the max side size of the image
                                            maxSideSize = maxAllowedSideSize;
                                        }
                                    }

                                    // Prepare the style (size)
                                    string style = GetMaxWidthHeightExpression(width, height, maxSideSize);
                                    if (!String.IsNullOrEmpty(style))
                                    {
                                        style = " style=\"" + style + "\"";
                                    }

                                    // Check URL
                                    string url = text;
                                    if (IsURL(url))
                                    {
                                        url = URLHelper.GetAbsoluteUrl(url, domainName);
                                        url = HTMLHelper.HTMLEncode(url);
                                        renderTag = "<img class=\"BBImage\" src=\"" + url + "\" alt=\"" + GetString("forum.discussion.image") + "\"" + style + ">";
                                    }
                                }
                            }
                            break;

                        // Code
                        case "code":
                            if (EnableCode)
                            {
                                if (!endTag)
                                {
                                    renderTag = "<pre class=\"BBCode\">";
                                }
                                renderText = text;
                            }
                            break;

                        // Color
                        case "color":
                            if (EnableColor)
                            {
                                if (!endTag)
                                {
                                    // Parameter is the color (required)
                                    string color = parameter;
                                    bool valid = false;
                                    if (ValidationHelper.IsColor(color))
                                    {
                                        valid = true;
                                    }
                                    else if (!color.StartsWithCSafe("#"))
                                    {
                                        // Check color with missing #
                                        color = "#" + color;
                                        if (ValidationHelper.IsColor(color))
                                        {
                                            valid = true;
                                        }
                                    }

                                    if (valid)
                                    {
                                        renderTag = "<span class=\"BBColor\" style=\"color: " + color + "\">";
                                    }
                                }
                                renderText = text;
                            }
                            break;

                        // Size
                        case "size":
                            if (EnableSize)
                            {
                                if (!endTag)
                                {
                                    // Parameter is the size in pixels (required)
                                    int size = ValidationHelper.GetInteger(parameter, 0);
                                    if (size > 0)
                                    {
                                        if (size > MaxTextSize)
                                        {
                                            size = MaxTextSize;
                                        }
                                        renderTag = "<span class=\"BBSize\" style=\"font-size: " + size + "px\">";
                                    }
                                }
                                renderText = text;
                            }
                            break;
                    }
                }

                // Add the normal text before tag
                if (lastIndex < m.Index)
                {
                    string notProcessedText = sourceText.Substring(lastIndex, m.Index - lastIndex);
                    notProcessedText = ProcessText(notProcessedText);
                    sb.Append(notProcessedText);
                }

                bool rendered = false;
                if (renderTag != null)
                {
                    // New valid open starting tag
                    openTags.Push(lowerTagName);

                    // Render the tag and text if any
                    sb.Append(renderTag);
                    renderText = ProcessText(renderText);
                    sb.Append(renderText);

                    rendered = true;
                }
                else if (!endTag)
                {
                    // Starting tag, not valid
                    openTags.Push(lowerTagName + " ");
                }
                else
                {
                    if (endTag)
                    {
                        // Add automatic closing tag content
                        if (automaticText != null)
                        {
                            sb.Append(automaticText);
                            automaticText = null;
                        }

                        // End tag, close all open tags until current is found
                        while ((openTags.Count > 0) && (openTags.Peek().Trim() != lowerTagName))
                        {
                            string closeTag = openTags.Pop();

                            // Close only valid tags
                            if (!closeTag.EndsWithCSafe(" "))
                            {
                                closeTag = GetClosingTag(closeTag);
                                sb.Append(closeTag);
                            }
                        }

                        // Close current
                        if ((openTags.Count > 0) && (openTags.Peek().Trim() == lowerTagName))
                        {
                            string closeTag = openTags.Pop();

                            // Close only when valid
                            if (!closeTag.EndsWithCSafe(" "))
                            {
                                closeTag = GetClosingTag(lowerTagName);
                                if (closeTag != null)
                                {
                                    sb.Append(closeTag);
                                    renderText = ProcessText(renderText);
                                    sb.Append(renderText);
                                    rendered = true;
                                }
                            }
                        }
                    }
                }

                if (!rendered)
                {
                    // Not renderable, render tag as regular text
                    renderText = tag;
                    renderText = ProcessText(renderText);
                    sb.Append(renderText);
                }

                lastIndex = m.Index + m.Length;
            }

            // Close all open tags
            while (openTags.Count > 0)
            {
                string closeTag = openTags.Pop();
                closeTag = GetClosingTag(closeTag);
                sb.Append(closeTag);
            }

            // Add the rest of the text
            if (lastIndex < sourceText.Length)
            {
                string renderText = sourceText.Substring(lastIndex);
                renderText = ProcessText(renderText);
                sb.Append(renderText);
            }

            return sb.ToString();
        }


        /// <summary>
        /// Processes the standard text.
        /// </summary>
        /// <param name="text">Text to process</param>
        protected string ProcessText(string text)
        {
            if (ResolveToPlainText)
            {
                return text;
            }

            if (EncodeText)
            {
                text = HTMLHelper.HTMLEncode(text);
            }

            if (ConvertLineBreaksToHTML)
            {
                text = HTMLHelper.EnsureHtmlLineEndings(text);
            }

            return text;
        }


        /// <summary>
        /// Gets the closing tag for specified BB tag.
        /// </summary>
        /// <param name="tagName">Tag name</param>
        protected string GetClosingTag(string tagName)
        {
            if (ResolveToPlainText)
            {
                // Process the tags
                switch (tagName)
                {
                    // URL
                    case "url":
                    case "img":
                        return ")";

                    // Quoation
                    case "quote":
                        return "\"\r\n";

                    // Other
                    default:
                        return "";
                }
            }
            else
            {
                // Process the tags
                switch (tagName)
                {
                    // Bold
                    case "b":
                    case "strong":
                        return "</strong>";

                    // Italics
                    case "i":
                    case "em":
                        return "</em>";

                    // Strikethrough
                    case "s":
                        return "</s>";

                    // URL
                    case "url":
                        return "</a>";

                    // Quoation
                    case "quote":
                        return "</div></blockquote>";

                    // Image
                    case "img":
                        return "</img>";

                    // Code
                    case "code":
                        return "</pre>";

                    // Color, Size, Underlined
                    case "color":
                    case "size":
                    case "u":
                        return "</span>";
                }
            }

            return null;
        }


        /// <summary>
        /// Returns true if BBcode is enabled for given actions set (for DB column with stored enabled actions).
        /// </summary>
        /// <param name="enabledActions">Set of enabled actions</param>
        /// <param name="action">Dsicussion action</param>
        public static bool IsBBCodeEnabled(int enabledActions, DiscussionActionEnum action)
        {
            return ((enabledActions >> Convert.ToInt32(action)) % 2 == 1);
        }


        /// <summary>
        /// Sets enabled BBcode (for DB column with stored enabled actions).
        /// </summary>
        /// <param name="currentActions">Current enabled actions</param>
        /// <param name="action">Discussion action to enable</param>
        /// <param name="enable">Indicates if the BBcode should be allowed</param>
        public static int SetBBCode(int currentActions, DiscussionActionEnum action, bool enable)
        {
            // BBCode action is not already allowed -> allow
            if (!IsBBCodeEnabled(currentActions, action) && enable)
            {
                currentActions += Convert.ToInt32(Math.Pow(2, Convert.ToInt32(action)));
            }

            // BBCode action is not already denied -> deny
            if (IsBBCodeEnabled(currentActions, action) && !enable)
            {
                currentActions -= Convert.ToInt32(Math.Pow(2, Convert.ToInt32(action)));
            }

            return currentActions;
        }


        /// <summary>
        /// Returns true if the given string is URL, decodes the string before the validation.
        /// </summary>
        /// <param name="url">URL to check</param>
        public static bool IsURL(string url)
        {
            if (url == null)
            {
                return false;
            }

            // Decode the URL
            url = HttpUtility.HtmlDecode(url);

            // Check validity
            return ValidationHelper.IsURL(url);
        }


        /// <summary>
        /// Gets the CSS expression to limit the max width of the element.
        /// </summary>
        /// <param name="width">Max width</param>
        /// <param name="height">Max height</param>
        /// <param name="maxSideSize">Max side size</param>
        private string GetMaxWidthHeightExpression(int width, int height, int maxSideSize)
        {
            if (maxSideSize > 0)
            {
                width = maxSideSize;
                height = maxSideSize;
            }

            // Create the expression
            string result = GetMaxWidthExpression(width) + " " + GetMaxHeightExpression(height);
            return result.Trim();
        }


        /// <summary>
        /// Gets the CSS expression to limit the max width of the element.
        /// </summary>
        /// <param name="width">Max width</param>
        private string GetMaxWidthExpression(int width)
        {
            if (width <= 0)
            {
                return null;
            }

            return "max-width: " + width + "px;";
        }


        /// <summary>
        /// Gets the CSS expression to limit the max Height of the element.
        /// </summary>
        /// <param name="height">Max Height</param>
        private string GetMaxHeightExpression(int height)
        {
            if (height <= 0)
            {
                return null;
            }

            return "max-height: " + height + "px;";
        }


        #endregion
    }
}