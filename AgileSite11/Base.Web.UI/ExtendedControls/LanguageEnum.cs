using CMS.Base;

namespace CMS.Base.Web.UI
{


    #region "Language enum"

    /// <summary>
    /// Programming language to be used by the syntax highlighting editor. 
    /// Determines the syntax rules and tokens to highlight.
    /// </summary>
    public enum LanguageEnum : int
    {
        /// <summary>
        /// Plaintext mode, no highlighting &amp; formatting is provided.
        /// </summary>
        Text = 0,


        /// <summary>
        /// HTML (XHTML).
        /// </summary>
        HTML = 1,


        /// <summary>
        /// CSS.
        /// </summary>
        CSS = 2,


        /// <summary>
        /// JavaScript.
        /// </summary>
        JavaScript = 3,


        /// <summary>
        /// XML.
        /// </summary>
        XML = 4,


        /// <summary>
        /// C#.
        /// </summary>
        CSharp = 5,


        /// <summary>
        /// SQL (T-SQL dialect).
        /// </summary>
        SQL = 6,


        /// <summary>
        /// Mixed mode that allows HTML, CSS and JavaScript in the same text segment.
        /// </summary>        
        HTMLMixed = 7,


        /// <summary>
        /// ASP.NET with macro support.
        /// </summary>
        ASPNET = 8,


        /// <summary>
        /// C# with CMS extensions.
        /// </summary>
        CMSSharp = 9,


        /// <summary>
        /// LESS.
        /// </summary>
        LESS = 10
    }

    #endregion


    #region "Language code"

    /// <summary>
    /// Language code.
    /// </summary>
    public static class LanguageCode
    {
        #region "Constants"

        /// <summary>
        /// Plaintext mode, no highlighting &amp; formatting is provided.
        /// </summary>
        public const int Text = 0;


        /// <summary>
        /// HTML (XHTML).
        /// </summary>
        public const int HTML = 1;


        /// <summary>
        /// CSS.
        /// </summary>
        public const int CSS = 2;


        /// <summary>
        /// JavaScript.
        /// </summary>
        public const int JavaScript = 3;


        /// <summary>
        /// XML.
        /// </summary>
        public const int XML = 4;


        /// <summary>
        /// C#.
        /// </summary>
        public const int CSharp = 5;


        /// <summary>
        /// SQL (T-SQL dialect).
        /// </summary>
        public const int SQL = 6;


        /// <summary>
        /// Mixed mode that allows HTML, CSS and JavaScript in the same text segment.
        /// </summary>        
        public const int HTMLMixed = 7;


        /// <summary>
        /// ASP.NET with macro support.
        /// </summary>
        public const int ASPNET = 8;


        /// <summary>
        /// C# with CMS extensions.
        /// </summary>
        public const int CMSSharp = 9;


        /// <summary>
        /// LESS.
        /// </summary>
        public const int LESS = 10;

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the language enumeration from the string value. Supports only languages
        /// </summary>
        /// <param name="language">Language</param>
        public static LanguageEnum GetLanguageEnumFromString(string language)
        {
            switch (language.ToLowerCSafe())
            {
                case "text":
                    return LanguageEnum.Text;


                case "html":
                    return LanguageEnum.HTML;


                case "css":
                    return LanguageEnum.CSS;


                case "javascript":
                    return LanguageEnum.JavaScript;


                case "xml":
                    return LanguageEnum.XML;


                case "csharp":
                    return LanguageEnum.CSharp;


                case "sql":
                    return LanguageEnum.SQL;


                case "htmlmixed":
                    return LanguageEnum.HTMLMixed;


                case "aspnet":
                    return LanguageEnum.ASPNET;


                case "cmssharp":
                    return LanguageEnum.CMSSharp;

                case "less":
                    return LanguageEnum.LESS;
            }

            return LanguageEnum.Text;
        }


        /// <summary>
        /// Returns the enumeration representation of the language.
        /// </summary>
        /// <param name="code">Language code</param>
        public static LanguageEnum ToEnum(int code)
        {
            switch (code)
            {
                case Text:
                    return LanguageEnum.Text;

                case HTML:
                    return LanguageEnum.HTML;

                case CSS:
                    return LanguageEnum.CSS;

                case JavaScript:
                    return LanguageEnum.JavaScript;

                case XML:
                    return LanguageEnum.XML;

                case CSharp:
                    return LanguageEnum.CSharp;

                case SQL:
                    return LanguageEnum.SQL;

                case HTMLMixed:
                    return LanguageEnum.HTMLMixed;

                case ASPNET:
                    return LanguageEnum.ASPNET;

                case CMSSharp:
                    return LanguageEnum.CMSSharp;

                case LESS:
                    return LanguageEnum.LESS;

                default:
                    return LanguageEnum.Text;
            }
        }


        /// <summary>
        /// Returns the language code from the enumeration value.
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static int FromEnum(LanguageEnum value)
        {
            switch (value)
            {
                case LanguageEnum.Text:
                    return Text;

                case LanguageEnum.HTML:
                    return HTML;

                case LanguageEnum.CSS:
                    return CSS;

                case LanguageEnum.JavaScript:
                    return JavaScript;

                case LanguageEnum.XML:
                    return XML;

                case LanguageEnum.CSharp:
                    return CSharp;

                case LanguageEnum.SQL:
                    return SQL;

                case LanguageEnum.HTMLMixed:
                    return HTMLMixed;

                case LanguageEnum.ASPNET:
                    return ASPNET;

                case LanguageEnum.CMSSharp:
                    return CMSSharp;

                case LanguageEnum.LESS:
                    return LESS;

                default:
                    return Text;
            }
        }

        #endregion
    }

    #endregion
}