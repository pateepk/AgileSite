using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;

using CMS.Base;
using CMS.Core;
using CMS.IO;

using ConversionFunction = System.Func<object, object, System.Globalization.CultureInfo, object>;

namespace CMS.Helpers
{
    /// <summary>
    /// Global class providing Validation methods.
    /// </summary>
    public class ValidationHelper
    {
        #region "Constants"

        /// <summary>
        /// App setting key for hash string salt. 
        /// </summary>
        public const string APP_SETTINGS_HASH_STRING_SALT = "CMSHashStringSalt";


        /// <summary>
        /// Maximum length of multiple e-mails
        /// </summary>
        /// <remarks>
        /// According to RFC 2822 line length must be no more than 998 characters long. 
        /// As multiple e-mail addresses are in one line, this length should not exceed.
        /// </remarks>
        public const int MULTIPLE_EMAILS_LENGTH = 998;


        /// <summary>
        /// Maximum length of e-mail 
        /// </summary>
        /// <remarks>According to RFC 5321 e-mail address should be no more than 254 characters long</remarks>
        public const int SINGLE_EMAIL_LENGTH = 254;


        // HEX values corresponding to the index in the array (0-16)
        private static readonly char[] HEX_VALUES = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };


        // Default e-mail separator
        private const string EMAIL_SEPARATOR = ";";

        #endregion


        #region "Variables"

        /// <summary>
        /// Contains set of string colors in LowerCSafe format.
        /// </summary>
        private static HashSet<string> mColorSet;


        /// <summary>
        /// Regular expression to match forbidden characters in file name.
        /// </summary>
        private static Regex mFileNameForbiddenCharRegExp;


        /// <summary>
        /// Regular expression to match the integer.
        /// </summary>
        private static Regex mIntRegExp;


        /// <summary>
        /// Regular expression to match the long.
        /// </summary>
        private static Regex mLongRegExp;


        /// <summary>
        /// Regular expression to match the identifier.
        /// </summary>
        private static Regex mIdentifierRegExp;


        /// <summary>
        /// Regular expression to match the codename.
        /// </summary>
        private static Regex mCodenameRegExp;


        /// <summary>
        /// Regular expression to match the codename with support of Unicode letters.
        /// </summary>
        private static Regex mUnicodeCodenameRegExp;


        /// <summary>
        /// Regular expression to match the username.
        /// </summary>
        private static Regex mUsernameRegExp;


        /// <summary>
        /// Regular expression to match the folder name.
        /// </summary>
        private static Regex mFolderRegExp;


        /// <summary>
        /// Regular expression to match the email.
        /// </summary>
        private static Regex mEmailRegExp;


        /// <summary>
        /// Regular expression to match the hexa color code expression.
        /// </summary>
        private static Regex mColorRegExp;


        /// <summary>
        /// Regular expression to match the URL expression.
        /// </summary>
        private static Regex mURLRegExp;


        /// <summary>
        /// Regular expression to match the GUID.
        /// </summary>
        private static Regex mGuidRegExp;


        /// <summary>
        /// Regular expression to match the U.S. Phone number.
        /// </summary>
        private static Regex mUsPhoneNumberRegExp;


        /// <summary>
        /// Regular expression to match client id.
        /// </summary>
        private static Regex mIsClientIdRegEx;


        /// <summary>
        /// Replacement for forbidden username characters.
        /// </summary>
        private static string mUserNameCharReplacement;


        /// <summary>
        /// Replacement for forbidden rolename characters.
        /// </summary>
        private static string mRoleNameCharReplacement;


        /// <summary>
        /// Indicates if user safe names should be used.
        /// </summary>
        private static bool? mUseSafeUserName;


        /// <summary>
        /// Indicates if role safe names should be used.
        /// </summary>
        private static bool? mUseSafeRoleName;


        /// <summary>
        /// Custom user name regular expression string from web.config for user name validation.
        /// </summary>
        private static string mCustomUsernameRegExpString;


        /// <summary>
        /// Custom email regular expression string from web.config for email validation.
        /// </summary>
        private static string mCustomEmailRegExpString;


        /// <summary>
        /// Salt used for hashing string in GetHashString() method.
        /// </summary>
        private static string mHashStringSalt;


        /// <summary>
        /// Dictionary of conversion functions for standard system data types
        /// </summary>
        private static Dictionary<Type, ConversionFunction> mConversionFunctions;


        /// <summary>
        /// File names which cannot be used on Windows (regardless of extension).
        /// </summary>
        internal static readonly HashSet<string> RESERVED_FILE_SYSTEM_NAMES = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Based on https://msdn.microsoft.com/en-us/library/windows/desktop/aa365247(v=vs.85).aspx#naming_conventions
            "con", "prn", "aux", "nul",
            "com0", "com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8", "com9",
            "lpt0", "lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9"
        };

        #endregion


        #region "Private properties"

        /// <summary>
        /// Contains set of string colors in LowerCSafe format.
        /// </summary>
        private static HashSet<string> ColorSet
        {
            get
            {
                if (mColorSet == null)
                {
                    mColorSet = new HashSet<string>(Enum.GetNames(typeof(KnownColor)).Select(color => color.ToLowerInvariant()));
                }

                return mColorSet;
            }
        }


        /// <summary>
        /// Replacement for forbidden username characters.
        /// </summary>
        private static string UserNameCharReplacement
        {
            get
            {
                return mUserNameCharReplacement ?? (mUserNameCharReplacement = SettingsHelper.AppSettings["CMSForbiddenUserNameCharactersReplacement"]);
            }
        }


        /// <summary>
        /// Replacement for forbidden Rolename characters.
        /// </summary>
        private static string RoleNameCharReplacement
        {
            get
            {
                return mRoleNameCharReplacement ?? (mRoleNameCharReplacement = SettingsHelper.AppSettings["CMSForbiddenRoleNameCharactersReplacement"]);
            }
        }


        /// <summary>
        /// Dictionary of conversion functions in format DataType => ConversionFunction
        /// </summary>
        private static Dictionary<Type, ConversionFunction> ConversionFunctions
        {
            get
            {
                return mConversionFunctions ?? (mConversionFunctions = EnsureConversionFunctions());
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns salt used for hashing string in GetHashString() method.
        /// </summary>
        public static string HashStringSalt
        {
            get
            {
                if (mHashStringSalt == null)
                {
                    // Get the default salt
                    string salt = GetDefaultHashStringSalt();
                    if (string.IsNullOrEmpty(salt))
                    {
                        throw new ArgumentException("[ValidationHelper.HashStringSalt]: Salt cannot be empty. Either " + APP_SETTINGS_HASH_STRING_SALT + " or connection string must be specified in web.config.");
                    }

                    mHashStringSalt = salt;
                }

                return mHashStringSalt;
            }
            set
            {
                mHashStringSalt = value;
            }
        }


        /// <summary>
        /// Gets the default hash string salt
        /// </summary>
        public static string GetDefaultHashStringSalt()
        {
            // Get the salt from configuration
            string salt = SettingsHelper.AppSettings[APP_SETTINGS_HASH_STRING_SALT];
            if (string.IsNullOrEmpty(salt))
            {
                salt = CoreServices.ConnectionStrings.DefaultConnectionString;
            }
            return salt;
        }


        /// <summary>
        /// Regular expression to match forbidden characters in file name.
        /// </summary>
        public static Regex FileNameForbiddenCharRegExp
        {
            get
            {
                return mFileNameForbiddenCharRegExp ?? (mFileNameForbiddenCharRegExp = RegexHelper.GetRegex("[\\|\\\\\\?\\*<\":>/]"));
            }
        }


        /// <summary>
        /// Indicates whether to use safe or normal user names.
        /// </summary>
        public static bool UseSafeUserName
        {
            get
            {
                if (mUseSafeUserName == null)
                {
                    mUseSafeUserName = GetBoolean(SettingsHelper.AppSettings["CMSEnsureSafeUserNames"], true);
                }
                return mUseSafeUserName.Value;
            }
            set
            {
                mUseSafeUserName = value;
            }
        }


        /// <summary>
        /// Indicates whether to use safe or normal role names.
        /// </summary>
        public static bool UseSafeRoleName
        {
            get
            {
                if (mUseSafeRoleName == null)
                {
                    mUseSafeRoleName = GetBoolean(SettingsHelper.AppSettings["CMSEnsureSafeRoleNames"], true);
                }
                return mUseSafeRoleName.Value;
            }
            set
            {
                mUseSafeRoleName = value;
            }
        }


        /// <summary>
        /// Integer regular expression.
        /// </summary>
        public static Regex IntRegExp
        {
            get
            {
                return mIntRegExp ?? (mIntRegExp = RegexHelper.GetRegex("^(?:\\+|-)?1?\\d{1,9}$"));
            }
        }


        /// <summary>
        /// Long regular expression.
        /// </summary>
        public static Regex LongRegExp
        {
            get
            {
                return mLongRegExp ?? (mLongRegExp = RegexHelper.GetRegex("^(?:\\+|-)?1?\\d{1,18}$"));
            }
        }


        /// <summary>
        /// Identifier regular expression.
        /// </summary>
        public static Regex IdentifierRegExp
        {
            get
            {
                return mIdentifierRegExp ?? (mIdentifierRegExp = RegexHelper.GetRegex("^[A-Za-z_][A-Za-z0-9_]*$"));
            }
        }


        /// <summary>
        /// Codename regular expression.
        /// </summary>
        public static Regex CodenameRegExp
        {
            get
            {
                return mCodenameRegExp ?? (mCodenameRegExp = RegexHelper.GetRegex("^(?:[A-Za-z0-9_\\-]+)(?:\\.[A-Za-z0-9_\\-]+)*$"));
            }
        }


        /// <summary>
        /// Codename regular expression with support of Unicode letters.
        /// </summary>
        public static Regex UnicodeCodenameRegExp
        {
            get
            {
                return mUnicodeCodenameRegExp ?? (mUnicodeCodenameRegExp = RegexHelper.GetRegex("^(?:[\\p{L}0-9_\\-]+)(?:\\.[\\p{L}0-9_\\-]+)*$"));
            }
        }


        /// <summary>
        /// Gets or sets the regular expression for client id validation.
        /// </summary>
        public static Regex ClientIDRexExp
        {
            get
            {
                return mIsClientIdRegEx ?? (mIsClientIdRegEx = RegexHelper.GetRegex("^\\w+$"));
            }
        }


        /// <summary>
        /// Custom user name regular expression string from web.config for user name validation.
        /// </summary>
        public static string CustomUsernameRegExpString
        {
            get
            {
                return mCustomUsernameRegExpString ?? (mCustomUsernameRegExpString = GetString(SettingsHelper.AppSettings["CMSUserValidationRegex"], null));
            }
        }


        /// <summary>
        /// Username regular expression.
        /// </summary>
        public static Regex UsernameRegExp
        {
            get
            {
                if (mUsernameRegExp == null)
                {
                    string defaultRegEx = (UseSafeUserName ? "^[a-zA-Z0-9_\\-\\.@]+$" : "^[a-zA-Z0-9_\\-\\.\\\\@!#$%&'*+/=?^_`{|}~]+$");
                    string regex = CustomUsernameRegExpString ?? defaultRegEx;
                    mUsernameRegExp = RegexHelper.GetRegex(regex);
                }
                return mUsernameRegExp;
            }
        }


        /// <summary>
        /// Folder regular expression.
        /// </summary>
        public static Regex FolderRegExp
        {
            get
            {
                return mFolderRegExp ?? (mFolderRegExp = RegexHelper.GetRegex(@"^([^/:*?<>{}\\\\""|%&+]*)+$"));
            }
        }


        /// <summary>
        /// Custom email regular expression string from web.config for email validation.
        /// </summary>
        public static string CustomEmailRegExpString
        {
            get
            {
                return mCustomEmailRegExpString ?? (mCustomEmailRegExpString = GetString(SettingsHelper.AppSettings["CMSEmailValidationRegex"], null));
            }
        }


        /// <summary>
        /// Gets the regular expression used to validate e-mail addresses.
        /// </summary>
        public static Regex EmailRegExp
        {
            get
            {
                if (mEmailRegExp == null)
                {
                    string defaultRegExp = CustomEmailRegExpString ?? @"^[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?$";
                    // Expression groups: none
                    mEmailRegExp = RegexHelper.GetRegex(defaultRegExp);
                }
                return mEmailRegExp;
            }
        }


        /// <summary>
        /// Hexa color code regular expression.
        /// </summary>
        public static Regex ColorRegExp
        {
            get
            {
                return mColorRegExp ?? (mColorRegExp = RegexHelper.GetRegex("^(?:#[0-9a-fA-F]{6}|#[0-9a-fA-F]{3})$"));
            }
        }


        /// <summary>
        /// URL regular expression.
        /// </summary>
        public static Regex URLRegExp
        {
            get
            {
                return mURLRegExp ?? (mURLRegExp = RegexHelper.GetRegex("^(?:(?#Protocol)(?:(?:ht|f)tp(?:s?)\\:\\/\\/|(?=www\\.|\\/|~\\/|\\.\\.))(?#Username:Password)(?:\\w+:\\w+@)?(?#Subdomains)(?:(?:[-\\w]+\\.)*(?#TopLevel Domains)(?:[-\\w]+))(?#Port)(?::[\\d]{1,5})?|~|\\.\\.)?(?#Directories)(?:(?:(?:\\/(?:[-\\w~!$+|.,=\\(\\)@]|%[a-fA-F0-9]{2})+)+|\\/)+|\\?|#)?(?#Query)(?:(?:\\?(?:[-\\w~!$+|.,*:]|%[a-fA-F0-9]{2})+(?:=(?:[-\\w~!$+|.,*:=;/]|%[a-fA-F0-9]{2})*)?)(?:&(?:[-\\w~!$+|.,*:]|%[a-fA-F0-9]{2})+(?:=(?:[-\\w~!$+|.,*:=/]|%[a-fA-F0-9]{2})*)?)*)*(?#Anchor)(?:#.*)?$", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase));
            }
        }


        /// <summary>
        /// GUID regular expression.
        /// </summary>
        public static Regex GuidRegExp
        {
            get
            {
                return mGuidRegExp ?? (mGuidRegExp = RegexHelper.GetRegex("^[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}$", RegexHelper.DefaultOptions));
            }
        }


        /// <summary>
        /// U.S. Phone number regular expression.
        /// </summary>
        public static Regex UsPhoneNumberRegExp
        {
            get
            {
                return mUsPhoneNumberRegExp ?? (mUsPhoneNumberRegExp = RegexHelper.GetRegex(@"\+?1?[-\s.]?\(?(\d{3})\)?[-\s.]?(\d{3})[-\s.]?(\d{4})", RegexHelper.DefaultOptions));
            }
        }


        #endregion


        #region "Methods"

        /// <summary>
        /// Removes forbidden characters from given xml string.
        /// </summary>
        /// <param name="xml">XML to sanitize</param>
        public static string GetSafeXML(string xml)
        {
            var sanitizedXml = new StringBuilder();

            foreach (char c in xml)
            {
                if (IsAllowedXMLCharacter(c))
                {
                    sanitizedXml.Append(c);
                }
            }

            return sanitizedXml.ToString();
        }


        /// <summary>
        /// Method used to obtain original text formatted to UTF-8 and encoded in base64
        /// </summary>
        /// <param name="data">Base64 encoded text</param>
        public static string Base64Decode(string data)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(data));
            }
            catch (Exception e)
            {
                throw new Exception("[ValidationHelper.Base64Decode]: Error occurred during decoding", e);
            }
        }


        /// <summary>
        /// Gets the culture info of default culture.
        /// </summary>
        private static CultureInfo GetDefaultCultureInfo()
        {
            return Thread.CurrentThread.CurrentCulture;
        }


        /// <summary>
        /// Gets the culture info for the given culture.
        /// </summary>
        /// <param name="culture">Culture to get</param>
        private static CultureInfo GetCultureInfo(ref string culture)
        {
            // Get the culture info
            if (culture == null)
            {
                CultureInfo defaultCulture = GetDefaultCultureInfo();
                culture = defaultCulture.Name;
                
                return defaultCulture;
            }
            
            return CultureHelper.GetCultureInfo(culture);
        }


        /// <summary>
        /// Gets string from byte array.
        /// </summary>
        /// <param name="hashBytes">Hash</param>
        /// <returns>Text interpretation of hash</returns>
        public static string GetStringFromHash(byte[] hashBytes)
        {
            int l = hashBytes.Length;
            int i = 0;

            char[] result = new char[l * 2];

            for (int si = 0; si < l; si++)
            {
                byte b = hashBytes[si];

                result[i++] = HEX_VALUES[b / 16];
                result[i++] = HEX_VALUES[b % 16];
            }

            return new String(result);
        }

        #endregion


        #region "Validation methods"

        /// <summary>
        /// Returns true if the object representation matches the Boolean type.
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsBoolean(object value)
        {
            if (IsNull(value))
            {
                return false;
            }

            if (value is bool)
            {
                return true;
            }

            switch (value.ToString().ToLowerInvariant())
            {
                case "0":
                case "1":
                case "true":
                case "false":
                    return true;

                default:
                    return false;
            }
        }


        /// <summary>
        /// Returns true if the object representation matches the Integer type.
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsInteger(object value)
        {
            if (IsNull(value))
            {
                return false;
            }

            if (value is int)
            {
                return true;
            }

            int v;

            return Int32.TryParse(Convert.ToString(value), out v);
        }


        /// <summary>
        /// Returns true if the object representation matches the Long type.
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsLong(object value)
        {
            if (IsNull(value))
            {
                return false;
            }

            if (value is long)
            {
                return true;
            }

            long v;

            return long.TryParse(value.ToString(), out v);
        }


        /// <summary>
        /// Returns true if the object representation matches the positive number.
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="culture">Culture to check</param>
        public static bool IsPositiveNumber(object value, string culture = null)
        {
            if (IsNull(value))
            {
                return false;
            }

            // Process double or integer value, with fallback to decimal
            var number = GetDouble(value, -1.0d, culture);
            return (number >= 0.0d) || (GetDecimal(value, -1.0M, culture) >= Decimal.Zero);
        }


        /// <summary>
        /// Returns true if value is higher than or equal to minimum and lesser than or equal to maximum.
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="value">Value to check</param>
        public static bool IsInRange(int min, int max, int value)
        {
            return ((value >= min) && (value <= max));
        }


        /// <summary>
        /// Returns true if value is higher than or equal to minimum and lesser than or equal to maximum.
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="value">Value to check</param>
        public static bool IsInRange(double min, double max, double value)
        {
            return ((value >= min) && (value <= max));
        }


        /// <summary>
        /// Returns true if value is higher than or equal to minimum and lesser than or equal to maximum.
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="value">Value to check</param>
        public static bool IsInRange(decimal min, decimal max, decimal value)
        {
            return ((value >= min) && (value <= max));
        }


        /// <summary>
        /// Returns true if the object representation matches the Double type.
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="culture">Optional culture code. If null, culture of current thread is used.</param>
        public static bool IsDouble(object value, string culture = null)
        {
            if (IsNull(value))
            {
                return false;
            }

            if (value is double)
            {
                return true;
            }

            var ci = GetCultureInfo(ref culture);
            var strignValue = Convert.ToString(value, ci);

            double result;
            var parseResult = Double.TryParse(strignValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands, ci, out result);

            return parseResult && !IsUnsupportedDouble(strignValue, ci);
        }


        /// <summary>
        /// Indicates whether input string is double format not supported in previous versions.
        /// </summary>
        /// <remarks>
        /// Previous version did not support following format (+-){numbers}{group separator}{number}. This format should be supported but we can't change this behavior within hotfix.
        /// </remarks>
        private static bool IsUnsupportedDouble(string value, CultureInfo cultureInfo)
        {
            if (String.IsNullOrEmpty(cultureInfo.NumberFormat.NumberGroupSeparator))
            {
                return false;
            }

            return RegexHelper.GetRegex($@"^(?:\+|-)?\d+\{cultureInfo.NumberFormat.NumberGroupSeparator}\d$").IsMatch(value);
        }


        /// <summary>
        /// Returns true if the object representation matches the Double type in English culture.
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsDoubleSystem(object value)
        {
            return IsDouble(value, CultureHelper.EnglishCulture.TwoLetterISOLanguageName);
        }


        /// <summary>
        /// Returns true if the object representation matches the decimal type.
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="culture">Culture code</param>
        /// <param name="precision">Maximum number of digits</param>
        /// <param name="scale">Maximum number of decimal places</param>
        public static bool IsDecimal(object value, string culture = null, int precision = 0, int scale = 0)
        {
            if (IsNull(value))
            {
                return false;
            }

            if (value is decimal && precision <= 0)
            {
                return true;
            }

            // Load given or default culture
            CultureInfo usedCulture = GetCultureInfo(ref culture);

            decimal result;
            if (decimal.TryParse(Convert.ToString(value, usedCulture), NumberStyles.Number, usedCulture, out result))
            {
                if (precision > 0)
                {
                    // Get SQL decimal
                    var sqlDecimal = new SqlDecimal(result);

                    // Check number of decimal places and number of digits
                    if ((sqlDecimal.Scale > scale) || (sqlDecimal.Precision - sqlDecimal.Scale > precision - scale))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns true if the object representation matches the TimeSpan type.
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="culture">Culture for conversion</param>
        public static bool IsTimeSpan(object value, CultureInfo culture = null)
        {
            if (IsNull(value))
            {
                return false;
            }

            if (value is TimeSpan)
            {
                return true;
            }

            TimeSpan result;
            if (culture != null)
            {
                return TimeSpan.TryParse(value.ToString(), culture, out result);
            }
            else
            {
                return TimeSpan.TryParse(value.ToString(), out result);
            }
        }


        /// <summary>
        /// Returns true if the object representation matches the Guid type.
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsGuid(object value)
        {
            if (IsNull(value))
            {
                return false;
            }

            if (value is Guid)
            {
                return true;
            }

            // Fast validation
            string stringGuid = value.ToString();
            if (stringGuid.Length != 36)
            {
                return false;
            }

            Guid dummyGuid;
            return Guid.TryParse(stringGuid, out dummyGuid);
        }


        /// <summary>
        /// Returns true if the object representation matches the IdentifierRegExp.
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsIdentifier(object value)
        {
            return !IsNull(value) && IdentifierRegExp.IsMatch(value.ToString());
        }


        /// <summary>
        /// Returns true if the object representation matches the date time format.
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="culture">Culture for conversion</param>
        public static bool IsDateTime(object value, CultureInfo culture = null)
        {
            if (IsNull(value))
            {
                return false;
            }

            if (value is DateTime)
            {
                return true;
            }

            DateTime result;
            if (culture != null)
            {
                return DateTime.TryParse(value.ToString(), culture.DateTimeFormat, DateTimeStyles.None, out result);
            }
            else
            {
                return DateTime.TryParse(value.ToString(), out result);
            }
        }


        /// <summary>
        /// Returns true if the object representation matches the User name.
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsUserName(object value)
        {
            return !IsNull(value) && UsernameRegExp.IsMatch(value.ToString());
        }


        /// <summary>
        /// Returns true if the object representation matches the Code name.
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="useUnicode">If true, unicode letters are allowed in the codename, otherwise only a-Z are allowed</param>        
        public static bool IsCodeName(object value, bool useUnicode = false)
        {
            if (IsNull(value))
            {
                return false;
            }

            if (useUnicode)
            {
                return UnicodeCodenameRegExp.IsMatch(value.ToString());
            }

            return CodenameRegExp.IsMatch(value.ToString());
        }


        /// <summary>
        /// Returns true if the object representation matches full name form ([namespace].[classname].[identifier]).
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsFullName(object value)
        {
            if (IsNull(value))
            {
                return false;
            }

            string[] parts = value.ToString().Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            return ((parts.Length == 3) && (IsIdentifier(parts[0])) && (IsIdentifier(parts[1])) && (IsIdentifier(parts[2])));
        }


        /// <summary>
        /// Returns true if the object representation matches the criteria for valid client id value.
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsValidClientID(object value)
        {
            if (IsNull(value))
            {
                return false;
            }

            return ClientIDRexExp.IsMatch(Convert.ToString(value));
        }


        /// <summary>
        /// Returns true if the object representation matches given regular expression.
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="regExp">Regular expression</param>
        public static bool IsRegularExp(object value, string regExp)
        {
            if (IsNull(value) || string.IsNullOrEmpty(regExp))
            {
                return false;
            }

            // Ensure proper form of regular expression
            if (!regExp.StartsWith("^", StringComparison.Ordinal))
            {
                regExp = "^" + regExp;
            }
            if (!regExp.EndsWith("$", StringComparison.Ordinal))
            {
                regExp += "$";
            }

            // Create new regular expression
            Regex regularExp = RegexHelper.GetRegex(regExp, RegexOptions.None);

            return regularExp.IsMatch(value.ToString());
        }


        /// <summary>
        /// Returns true if the object representation matches the file name.
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsFileName(object value)
        {
            string filename = GetString(value, null);

            return !String.IsNullOrEmpty(filename) && !FileNameForbiddenCharRegExp.IsMatch(filename) && !RESERVED_FILE_SYSTEM_NAMES.Contains(Path.GetFileNameWithoutExtension(filename));
        }


        /// <summary>
        /// Returns true if the object representation matches the folder name.
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsFolderName(object value)
        {
            if (IsNull(value))
            {
                return false;
            }

            string valueStr = value.ToString().ToLowerInvariant();
            if (String.IsNullOrWhiteSpace(valueStr))
            {
                return false;
            }

            if (IsSpecialFolderName(valueStr))
            {
                return false;
            }

            return FolderRegExp.IsMatch(valueStr);
        }


        /// <summary>
        /// Returns true if the file name is restricted.
        /// </summary>
        /// <param name="name">File name to check</param>
        public static bool IsSpecialFileName(string name)
        {
            return IsSpecialFolderName(name);
        }


        /// <summary>
        /// Returns true if the folder name is restricted.
        /// </summary>
        /// <param name="name">Folder name to check</param>
        public static bool IsSpecialFolderName(string name)
        {
            return RESERVED_FILE_SYSTEM_NAMES.Contains(name) || (FindReservedFileSystemNamePrefix(name) != null);
        }


        /// <summary>
        /// Searches for occurrence of one of <see cref="RESERVED_FILE_SYSTEM_NAMES"/> as a prefix in <paramref name="name"/>, delimited by a '.' (dot) character.
        /// If such prefix is found, its value is returned (without the delimiter).
        /// </summary>
        /// <param name="name">Name which to search for reserved prefixes.</param>
        /// <returns>Reserved prefix found in <paramref name="name"/>, or null if no matching prefix is found.</returns>
        internal static string FindReservedFileSystemNamePrefix(string name)
        {
            return RESERVED_FILE_SYSTEM_NAMES.FirstOrDefault(prefix => (name.Length > prefix.Length) && name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && (name[prefix.Length] == '.'));
        }


        /// <summary>
        /// Returns <c>true</c> if the object representation matches the Email.
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="checkLength">if <c>true</c> checks whether e-mail is no longer than <see cref="SINGLE_EMAIL_LENGTH"/></param>
        public static bool IsEmail(object value, bool checkLength = false)
        {
            return !IsNull(value) && IsEmailInternal(value.ToString(), checkLength);
        }


        /// <summary>
        /// Returns true if the object representation matches the U.S. Phone number.
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsUsPhoneNumber(object value)
        {
            if (IsNull(value))
            {
                return false;
            }

            string valueStr = value.ToString();
            if (String.IsNullOrWhiteSpace(valueStr))
            {
                return false;
            }

            return UsPhoneNumberRegExp.IsMatch(valueStr);
        }


        /// <summary>
        /// Returns true if fromDate precedes toDate.
        /// </summary>
        /// <param name="fromDate">Start time</param>
        /// <param name="toDate">End time</param>
        /// <param name="nullFriendly">Tolerate nil dates</param>
        public static bool IsIntervalValid(DateTime fromDate, DateTime toDate, bool nullFriendly)
        {
            fromDate = GetDateTime(fromDate, DateTimeHelper.ZERO_TIME);
            toDate = GetDateTime(toDate, DateTimeHelper.ZERO_TIME);
            if (nullFriendly)
            {
                return fromDate == DateTimeHelper.ZERO_TIME || toDate == DateTimeHelper.ZERO_TIME || fromDate <= toDate;
            }
            return fromDate <= toDate;
        }


        /// <summary>
        /// Returns true if the object representation matches the Email list separated by specified character (default separator is semicolon).
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="separator">String that delimits the addresses</param>
        /// <param name="checkLength">if <c>true</c> checks whether <paramref name="value"/> length is no longer than <see cref="MULTIPLE_EMAILS_LENGTH"/>and each individual e-mail is no longer than <see cref="SINGLE_EMAIL_LENGTH"/></param>
        public static bool AreEmails(object value, string separator = null, bool checkLength = false)
        {
            return !IsNull(value) && AreEmailsInternal(value.ToString(), separator, checkLength);
        }


        /// <summary>
        /// Returns true if the object representation matches the hexa color code expression or value is an existing string representation of color.
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsColor(object value)
        {
            if (IsNull(value))
            {
                return false;
            }

            var color = value.ToString().ToLowerInvariant();

            // Check string name of color
            if (!color.StartsWith("#", StringComparison.Ordinal))
            {
                return ColorSet.Contains(color);
            }

            // HEXA color code validation
            return ColorRegExp.IsMatch(color);
        }


        /// <summary>
        /// Returns true if the given URL is valid.
        /// </summary>
        /// <param name="value">URL to check</param>
        public static bool IsURL(object value)
        {
            if (IsNull(value))
            {
                return false;
            }

            string valueStr = value.ToString();
            if (String.IsNullOrWhiteSpace(valueStr))
            {
                return false;
            }

            string url = HttpUtility.HtmlDecode(valueStr);
            return URLRegExp.IsMatch(url);
        }


        /// <summary>
        /// Returns true if the character is within range of allowed characters.
        /// </summary>
        /// <param name="character">Character to check</param>
        public static bool IsAllowedXMLCharacter(int character)
        {
            return (character == 0x9) || (character == 0xA) || (character == 0xD) ||
                   ((character >= 0x20) && (character <= 0xD7FF)) ||
                   ((character >= 0xE000) && (character <= 0xFFFD)) ||
                   ((character >= 0x10000) && (character <= 0x10FFFF));
        }


        /// <summary>
        /// Returns true if value is of type.
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="value">Value</param>
        public static bool IsType(Type type, object value)
        {
            if (type == typeof(string))
            {
                return value is string;
            }
            else if (type == typeof(bool))
            {
                return IsBoolean(value);
            }
            else if (type == typeof(int))
            {
                return IsInteger(value);
            }
            else if (type == typeof(long))
            {
                return IsLong(value);
            }
            else if (type == typeof(double))
            {
                return IsDouble(value);
            }
            else if (type == typeof(Decimal))
            {
                return IsDecimal(value);
            }
            else if (type == typeof(DateTime))
            {
                return IsDateTime(value);
            }
            else if (type == typeof(Guid))
            {
                return IsGuid(value);
            }

            return false;
        }


        /// <summary>
        /// Returns <c>true</c> if <paramref name="email"/> is not empty and has expected length and is in correct format.
        /// </summary>
        private static bool IsEmailInternal(string email, bool checkLength)
        {
            return !String.IsNullOrWhiteSpace(email)
                   && (!checkLength || email.Length <= SINGLE_EMAIL_LENGTH)
                   && EmailRegExp.IsMatch(email);
        }


        /// <summary>
        /// Returns <c>true</c> if <paramref name="emails"/> is not empty, has correct length and contains valid e-mails.
        /// </summary>
        private static bool AreEmailsInternal(string emails, string separator, bool checkLength)
        {
            if (String.IsNullOrWhiteSpace(emails))
            {
                return false;
            }

            if (String.IsNullOrEmpty(separator))
            {
                separator = EMAIL_SEPARATOR;
            }

            var splittedEmails = emails.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            // Validate the emails
            return (!checkLength || emails.Length <= MULTIPLE_EMAILS_LENGTH)
                   && splittedEmails.Length > 0
                   && splittedEmails.All(e => IsEmail(e.Trim(), checkLength));
        }

        #endregion


        #region "Conversion methods"

        /// <summary>
        /// Returns the boolean representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture used for conversion</param>
        public static bool GetBoolean(object value, bool defaultValue, CultureInfo culture = null)
        {
            if (IsNull(value))
            {
                return defaultValue;
            }

            if (value is bool)
            {
                return (bool)value;
            }

            var s = value as string;
            if (s != null)
            {
                return GetBooleanFromString(s, defaultValue).Value;
            }

            try
            {
                return Convert.ToBoolean(value, culture);
            }
            catch
            {
                // Suppress convert error and return default value
            }

            return defaultValue;
        }


        /// <summary>
        /// Gets the boolean value from string value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="defaultValue">Default value</param>
        private static bool? GetBooleanFromString(string value, bool? defaultValue)
        {
            if (IsNull(value))
            {
                return defaultValue;
            }

            switch (value.ToLowerInvariant())
            {
                case "true":
                case "1":
                    return true;

                case "false":
                case "0":
                    return false;

                default:
                    return defaultValue;
            }
        }


        /// <summary>
        /// Returns the boolean representation of an object or default value if a conversion doesn't exist.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="defaultValue">The default value to substitute</param>
        /// <returns>Successful conversion of value to boolean or a default value.</returns>
        /// <remarks>
        /// This method allows to use <c>null</c> as a default and return value, usable in cases
        /// where unknown value (3-state logic) has to behave differently under certain conditions.
        /// </remarks>
        public static bool? GetNullableBoolean(object value, bool? defaultValue)
        {
            // Null
            if (IsNull(value))
            {
                return defaultValue;
            }

            // Boolean
            if (value is bool)
            {
                return (bool)value;
            }

            // String
            var s = value as string;
            if (s != null)
            {
                return GetBooleanFromString(s, defaultValue);
            }

            return defaultValue;
        }


        /// <summary>
        /// Returns the color representation of a query parameter or default value
        /// if parameter is not a valid color.
        /// </summary>
        /// <param name="name">Color name in HTML format</param>
        /// <param name="defaultValue">Default value</param>
        public static Color GetColor(string name, Color defaultValue)
        {
            if (String.IsNullOrEmpty(name))
            {
                return defaultValue;
            }

            try
            {
                // Convert HTML representation to color
                return ColorTranslator.FromHtml(name);
            }
            catch
            {
                return defaultValue;
            }
        }


        /// <summary>
        /// Returns the integer representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture used for conversion</param>
        public static int GetInteger(object value, int defaultValue, CultureInfo culture = null)
        {
            if (value is int)
            {
                return (int)value;
            }

            int v;
            if (Int32.TryParse(Convert.ToString(value), out v))
            {
                return v;
            }

            return defaultValue;
        }


        /// <summary>
        /// Returns array of integer representations of supplied values or default value when not an int.
        /// </summary>
        /// <param name="values">Values to be converted</param>
        /// <param name="defaultValue">Default value</param>
        public static int[] GetIntegers(object[] values, int defaultValue)
        {
            // Do not process 
            if (values == null)
            {
                return null;
            }

            // Init int list
            var list = new List<int>();

            foreach (object value in values)
            {
                list.Add(GetInteger(value, defaultValue));
            }

            // Convert to int array
            return list.ToArray();
        }


        /// <summary>
        /// Returns the long representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture used for conversion</param>
        public static long GetLong(object value, long defaultValue, CultureInfo culture = null)
        {
            if (!IsLong(value))
            {
                return defaultValue;
            }
            else if (value is long)
            {
                return (long)value;
            }
            else
            {
                return Convert.ToInt64(value);
            }
        }


        /// <summary>
        /// Returns the integer representation of an object or default value if not.
        /// Consumes all exceptions.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public static int GetSafeInteger(object value, int defaultValue)
        {
            try
            {
                return GetInteger(value, defaultValue);
            }
            catch
            {
                return defaultValue;
            }
        }


        /// <summary>
        /// Returns the double representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture code</param>
        public static double GetDouble(object value, double defaultValue, string culture)
        {
            if (culture != null)
            {
                // Get with specific culture
                CultureInfo ci = CultureHelper.GetCultureInfo(culture);
                return GetDouble(value, defaultValue, ci);
            }
            else
            {
                // Get with default culture
                return GetDouble(value, defaultValue);
            }
        }


        /// <summary>
        /// Returns the double representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture info</param>
        public static double GetDouble(object value, double defaultValue, CultureInfo culture = null)
        {
            if (((culture == null) && !IsDouble(value)) || ((culture != null) && !IsDouble(value, culture.Name)))
            {
                return defaultValue;
            }
            else
            {
                try
                {
                    if (culture != null)
                    {
                        // Get with specific culture
                        return Convert.ToDouble(value, culture);
                    }
                    else
                    {

                        // Get with default culture
                        return Convert.ToDouble(value);

                    }
                }
                catch
                {
                    return defaultValue;
                }
            }
        }


        /// <summary>
        /// Converts the given object to a decimal value
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public static decimal GetDecimalSystem(object value, decimal defaultValue)
        {
            return GetDecimal(value, defaultValue, CultureHelper.EnglishCulture);
        }


        /// <summary>
        /// Returns the decimal representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture code</param>
        public static decimal GetDecimal(object value, decimal defaultValue, string culture)
        {
            return GetDecimal(value, defaultValue, GetCultureInfo(ref culture));
        }


        /// <summary>
        /// Returns the decimal representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture for the conversion</param>
        public static decimal GetDecimal(object value, decimal defaultValue, CultureInfo culture = null)
        {
            if (value == null)
            {
                return defaultValue;
            }

            if (value is decimal)
            {
                return (decimal)value;
            }

            CultureInfo usedCulture = culture ?? GetDefaultCultureInfo();

            decimal result;
            if (decimal.TryParse(Convert.ToString(value, usedCulture), NumberStyles.Number, usedCulture.NumberFormat, out result))
            {
                return result;
            }

            return defaultValue;
        }


        /// <summary>
        /// Returns the double representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture used for conversion</param>
        public static float GetFloat(object value, float defaultValue, CultureInfo culture = null)
        {
            return (float)GetDouble(value, defaultValue, culture);
        }


        /// <summary>
        /// Returns the double representation of an object (using English representation of floating point numbers) or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public static double GetDoubleSystem(object value, double defaultValue)
        {
            return GetDouble(value, defaultValue, CultureHelper.EnglishCulture);
        }


        /// <summary>
        /// Returns the GUID representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture used for conversion</param>
        public static Guid GetGuid(object value, Guid defaultValue, CultureInfo culture = null)
        {
            if (!IsGuid(value))
            {
                return defaultValue;
            }
            else if (value is Guid)
            {
                return (Guid)value;
            }
            else
            {
                return new Guid(value.ToString());
            }
        }


        /// <summary>
        /// Returns the string representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture</param>
        public static string GetString(object value, string defaultValue, string culture)
        {
            CultureInfo ci = (culture != null) ? CultureHelper.GetCultureInfo(culture) : null;

            return GetString(value, defaultValue, ci);
        }


        /// <summary>
        /// Returns the string representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture</param>
        public static string GetString(object value, string defaultValue, CultureInfo culture = null)
        {
            var strValue = value as string;
            if (strValue != null)
            {
                return strValue;
            }

            if (IsNull(value))
            {
                return defaultValue;
            }

            if (culture != null)
            {
                // Get with specific culture
                return Convert.ToString(value, culture);
            }

            // Get with default culture
            return Convert.ToString(value);
        }


        /// <summary>
        /// Returns the string representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture</param>
        /// <param name="format">Formatting string</param>
        public static string GetString(object value, string defaultValue, string culture, string format)
        {
            if (format != null)
            {
                // Load default value
                if (IsNull(value))
                {
                    value = defaultValue;
                }

                // Get by specified format
                CultureInfo ci = GetCultureInfo(ref culture);
                return String.Format(ci, format, value);
            }
            else
            {
                // Get as standard expression
                return GetString(value, defaultValue, culture);
            }
        }


        /// <summary>
        /// Returns the byte[] representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture used for conversion</param>
        public static byte[] GetBinary(object value, byte[] defaultValue, CultureInfo culture = null)
        {
            if (IsNull(value))
            {
                return defaultValue;
            }

            try
            {
                return (byte[])value;
            }
            catch
            {
                return defaultValue;
            }
        }


        /// <summary>
        /// Returns the DateTime representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture to use for processing of the string</param>
        public static DateTime GetDateTime(object value, DateTime defaultValue, string culture)
        {
            if (culture != null)
            {
                // Get with specific culture
                CultureInfo ci = CultureHelper.GetCultureInfo(culture);
                return GetDateTime(value, defaultValue, ci);
            }
            else
            {
                // Get with default culture
                return GetDateTime(value, defaultValue);
            }
        }


        /// <summary>
        /// Returns the DateTime representation of an object (using English representation of date and time) or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public static DateTime GetDateTimeSystem(object value, DateTime defaultValue)
        {
            return GetDateTime(value, defaultValue, CultureHelper.EnglishCulture);
        }


        /// <summary>
        /// Returns the DateTime representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture to convert</param>
        public static DateTime GetDateTime(object value, DateTime defaultValue, CultureInfo culture = null)
        {
            if (IsNull(value))
            {
                return defaultValue;
            }

            if (value is DateTime)
            {
                return (DateTime)value;
            }

            try
            {
                if (culture != null)
                {
                    return DateTime.Parse(value.ToString(), culture.DateTimeFormat);
                }

                return DateTime.Parse(value.ToString());
            }
            catch
            {
                return defaultValue;
            }
        }


        /// <summary>
        /// Returns the DateTime representation of an object (using English representation of date and time) or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public static DateTime GetDateSystem(object value, DateTime defaultValue)
        {
            return GetDate(value, defaultValue, CultureHelper.EnglishCulture);
        }


        /// <summary>
        /// Returns the DateTime representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public static DateTime GetDate(object value, DateTime defaultValue)
        {
            return GetDate(value, defaultValue, null);
        }


        /// <summary>
        /// Returns the DateTime representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture to use</param>
        public static DateTime GetDate(object value, DateTime defaultValue, CultureInfo culture)
        {
            return GetDateTime(value, defaultValue, culture).Date;
        }


        /// <summary>
        /// Returns the TimeSpan representation of an object (using English representation of date and time) or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public static TimeSpan GetTimeSpanSystem(object value, TimeSpan defaultValue)
        {
            return GetTimeSpan(value, defaultValue, CultureHelper.EnglishCulture);
        }


        /// <summary>
        /// Returns the TimeSpan representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture for the conversion</param>
        public static TimeSpan GetTimeSpan(object value, TimeSpan defaultValue, string culture)
        {
            if (culture != null)
            {
                // Get with specific culture
                CultureInfo ci = CultureHelper.GetCultureInfo(culture);
                return GetTimeSpan(value, defaultValue, ci);
            }

            // Get with default culture
            return GetTimeSpan(value, defaultValue);
        }


        /// <summary>
        /// Returns the TimeSpan representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture for the conversion</param>
        public static TimeSpan GetTimeSpan(object value, TimeSpan defaultValue, CultureInfo culture = null)
        {
            // No value - return default value
            if (value == null)
            {
                return defaultValue;
            }

            // Already time span
            if (value is TimeSpan)
            {
                return (TimeSpan)value;
            }

            TimeSpan result;

            if (IsTimeSpan(value, culture))
            {
                if (culture != null)
                {
                    result = TimeSpan.Parse(value.ToString(), culture.DateTimeFormat);
                }
                else
                {
                    result = TimeSpan.Parse(value.ToString());
                }
            }
            else
            {
                try
                {
                    // Try to parse value recieved from XML data (XML serialization uses special format)
                    result = XmlConvert.ToTimeSpan(value.ToString());
                }
                catch (FormatException)
                {
                    return defaultValue;
                }
            }

            return result;
        }


        /// <summary>
        /// Returns the given string if it matches the criteria for valid client ID value.
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="defaultValue">Value to return in case given value is not a valid client ID</param>
        public static string GetControlClientId(object value, string defaultValue = "")
        {
            return IsValidClientID(value) ? Convert.ToString(value) : defaultValue;
        }


        /// <summary>
        /// Converts the value to specified type.
        /// </summary>
        /// <typeparam name="ReturnType">Result type</typeparam>
        /// <param name="value">Value to convert</param>
        public static ReturnType GetValue<ReturnType>(object value)
        {
            return GetValue(value, default(ReturnType));
        }


        /// <summary>
        /// Converts the value to specified type. If the value is null, default value is used.
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture</param>
        public static T GetValue<T>(object value, T defaultValue, CultureInfo culture = null)
        {
            // Default value for nulls
            if ((value == null) || (value == DBNull.Value))
            {
                return defaultValue;
            }

            ConversionFunction conversion;
            ConversionFunctions.TryGetValue(typeof(T), out conversion);
            if (conversion != null)
            {
                return (T)conversion(value, defaultValue, culture);
            }

            // General conversion for other types
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }


        /// <summary>
        /// Registers system type conversion methods
        /// </summary>
        private static Dictionary<Type, ConversionFunction> EnsureConversionFunctions()
        {
            var functions = new Dictionary<Type, ConversionFunction>();

            AddConversion<string>(functions, GetString);
            AddConversion<int>(functions, GetInteger);
            AddConversion<double>(functions, GetDouble);
            AddConversion<bool>(functions, GetBoolean);
            AddConversion<DateTime>(functions, GetDateTime);
            AddConversion<Guid>(functions, GetGuid);
            AddConversion<TimeSpan>(functions, GetTimeSpan);
            AddConversion<byte[]>(functions, GetBinary);
            AddConversion<long>(functions, GetLong);
            AddConversion<decimal>(functions, GetDecimal);
            AddConversion<float>(functions, GetFloat);

            return functions;
        }


        /// <summary>
        /// Registers conversion method for given data type
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="functions">Dictionary of conversion functions</param>
        /// <param name="conversion">Conversion method</param>
        private static void AddConversion<T>(Dictionary<Type, ConversionFunction> functions, Func<object, T, CultureInfo, T> conversion)
        {
            functions.Add(typeof(T), (value, defaultValue, culture) => conversion(value, (T)defaultValue, culture));
        }


        /// <summary>
        /// Converts HResult (ErrorCode) from exception to format comparable with error codes in hex format.
        /// </summary>
        /// <param name="code">Code to convert</param>
        /// <returns>Code in comparable format (0x800ABCDE)</returns>
        public static long GetHResult(int code)
        {
            return code & 0xFFFFFFFF;
        }

        #endregion


        #region "Name methods"

        /// <summary>
        /// Gets the code name created from the given display name.
        /// </summary>
        /// <param name="name">Display name</param>
        /// <param name="prefix">Prefix of the display name</param>
        /// <param name="suffix">Suffix of the display name</param>
        public static string GetCodeName(string name, string prefix, string suffix)
        {
            return GetCodeName(prefix + name + suffix);
        }


        /// <summary>
        /// Gets the code name created from the given string.
        /// </summary>
        /// <param name="name">Display name</param>
        /// <param name="maxLength">Maximal length of the codename</param>
        public static string GetCodeName(object name, int maxLength)
        {
            return GetCodeName(name, null, maxLength);
        }


        /// <summary>
        /// Gets the code name created from the given string.
        /// </summary>
        /// <param name="name">Display name</param>
        /// <param name="replacement">Replacement string for invalid characters</param>
        /// <param name="maxLength">Maximal length of the codename</param>
        /// <param name="useUnicode">If true, Unicode letters are allowed in the codename, otherwise only a-Z are allowed</param>
        /// <param name="removeDiacritics">If true, diacritics is removed from Latin characters</param>
        /// <param name="allowedCharacters">Characters which should be allowed within the current name</param>
        /// <param name="useCamelCase">If true, camel case is used for the code name</param>
        public static string GetCodeName(object name, string replacement = null, int maxLength = 0, bool useUnicode = true, bool removeDiacritics = true, string allowedCharacters = null, bool useCamelCase = true)
        {
            string stringName = GetString(name, "");
            if (string.IsNullOrEmpty(stringName))
            {
                return "";
            }

            if (replacement == null)
            {
                replacement = "_";
            }

            // Add prefix if defined
            string prefix = CoreServices.Settings["CMSCodeNamePrefix"].ToString("");
            if (!string.IsNullOrEmpty(prefix))
            {
                stringName = prefix + stringName;
            }

            // Remove Latin diacritics if enabled
            if (removeDiacritics)
            {
                stringName = TextHelper.RemoveDiacritics(stringName);
            }

            // Make camel case if required
            if (useCamelCase)
            {
                StringBuilder sb = new StringBuilder();

                // Split to words and join with first letter to upper
                string[] words = stringName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var w in words)
                {
                    // Make first letter to Upper
                    sb.Append(Char.ToUpperInvariant(w[0]), w.Substring(1));
                }

                stringName = sb.ToString();
            }

            // Replace invalid characters
            stringName = Regex.Replace(stringName, "[^" + (useUnicode ? "\\p{L}" : "a-zA-Z") + "0-9_.-" + allowedCharacters + "]+", replacement);

            // Trim codename length
            if ((maxLength > 0) && (stringName.Length > maxLength))
            {
                stringName = stringName.Substring(0, maxLength);
            }

            // Replace duplicated dots
            stringName = Regex.Replace(stringName, @"\.+", ".");

            // Replace dots at start and end
            stringName = stringName.Trim('.', '_');

            return stringName;
        }


        /// <summary>
        /// Gets safe version of username.
        /// </summary>
        /// <param name="userName">Username</param>
        /// <param name="siteName">Name of site</param>
        public static string GetSafeUserName(string userName, string siteName)
        {
            char defaultReplacement = URLHelper.ForbiddenCharactersReplacement(siteName);

            // Load replacement from web.config
            string replacement = GetString(UserNameCharReplacement, defaultReplacement.ToString());

            // Return safe user name
            return GetCodeName(userName, replacement, 0, false, true, "@");
        }


        /// <summary>
        /// Gets safe version of rolename.
        /// </summary>
        /// <param name="roleName">Name of role</param>
        /// <param name="siteName">Name of site</param>
        /// <returns>Safe version of role name</returns>
        public static string GetSafeRoleName(string roleName, string siteName)
        {
            char defaultReplacement = URLHelper.ForbiddenCharactersReplacement(siteName);
            // Load replacement from web.config
            string replacement = GetString(RoleNameCharReplacement, defaultReplacement.ToString());
            // Return safe role name
            return GetCodeName(roleName, replacement);
        }


        /// <summary>
        /// Gets the language created from the given string.
        /// </summary>
        /// <param name="lang">Language code</param>
        /// <param name="replacement">Replacement string for invalid characters</param>
        public static string GetLanguage(object lang, string replacement)
        {
            if (replacement == null)
            {
                replacement = "_";
            }
            string stringName = GetString(lang, "");
            stringName = Regex.Replace(stringName, "[^a-zA-Z\\-]+", replacement);
            return stringName;
        }


        /// <summary>
        /// Gets the identifier created from the given name
        /// </summary>
        /// <param name="name">Display name</param>
        public static string GetIdentifier(object name)
        {
            return GetIdentifier(name, null);
        }


        /// <summary>
        /// Gets the identifier created from the given name
        /// </summary>
        /// <param name="name">Display name</param>
        /// <param name="replacement">Replacement string for invalid characters</param>
        public static string GetIdentifier(object name, string replacement)
        {
            if (replacement == null)
            {
                replacement = "_";
            }
            string stringName = GetString(name, "");
            stringName = Regex.Replace(stringName, "[^a-zA-Z0-9_]", replacement);

            return stringName;
        }


        /// <summary>
        /// Get safe file name in which forbidden characters are replaced with specified replacement
        /// </summary>
        /// <param name="value">File name</param>
        /// <param name="replacement">Replacement string</param>
        public static string GetSafeFileName(string value, string replacement = null)
        {
            if (!String.IsNullOrEmpty(value))
            {
                // Set default replacement character if not set
                if (String.IsNullOrEmpty(replacement))
                {
                    replacement = "_";
                }

                value = FileNameForbiddenCharRegExp.Replace(value, replacement);


                // Limit max filename length to 255
                if (value.Length > 255)
                {
                    string ext = Path.GetExtension(value);
                    value = value.Substring(0, 255 - ext.Length) + ext;
                }

                return value;
            }

            return String.Empty;
        }

        #endregion


        #region "Hash methods"

        /// <summary>
        /// Gets the SHA2 hash for the given value (salt unique to certain user session and database is added).
        /// </summary>
        /// <param name="value">Value to hash</param>
        /// <param name="settings">Hash settings</param>
        public static string GetHashString(string value, HashSettings settings = null)
        {
            settings = settings ?? new HashSettings();

            var customSalt = settings.CustomSalt;

            if (string.IsNullOrEmpty(customSalt))
            {
                customSalt = HashStringSalt;
            }

            // Prepare the value
            string valHash = (value?.ToLowerInvariant() ?? String.Empty) + customSalt + settings.HashSalt;

            // Calculate the hash
            return SecurityHelper.GetSHA2Hash(valHash);
        }


        /// <summary>
        /// Validates the hash for the given value.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="hash">Hash</param>
        /// <param name="settings">Hash settings</param>
        public static bool ValidateHash(string value, string hash, HashSettings settings = null)
        {
            settings = settings ?? new HashSettings();

            var result = ValidateHashInternal(value, hash, settings);

            // Log the operation
            SecurityDebug.LogSecurityOperation(null, "ValidateHash", null, value, result, null);

            // If hash is invalid redirect current request
            if (!result && settings.Redirect)
            {
                RedirectToBadHash();
            }

            return result;
        }


        /// <summary>
        /// Redirects to the bad hash information page
        /// </summary>
        private static void RedirectToBadHash()
        {
            URLHelper.Redirect(AdministrationUrlHelper.GetAccessDeniedUrl("dialogs.badhashtext"));
        }


        /// <summary>
        /// Validates the hash for the given value.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="hash">Hash</param>
        /// <param name="settings">Hash settings</param>
        private static bool ValidateHashInternal(string value, string hash, HashSettings settings)
        {
            string required = GetHashString(value, settings);

            return String.Equals(required, hash, StringComparison.Ordinal);
        }


        /// <summary>
        /// Validates the hashes for the given values.
        /// </summary>
        /// <param name="values">Values</param>
        /// <param name="hashes">Hashes</param>
        /// <param name="settings">Hash settings</param>
        /// <exception cref="ArgumentNullException">If either values or hashes are null.</exception>
        /// <exception cref="ArgumentException">If number of hashes does not match the number of values.</exception>
        public static bool ValidateHashes(IList<string> values, IList<string> hashes, HashSettings settings = null)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (hashes == null)
            {
                throw new ArgumentNullException(nameof(hashes));
            }

            if (values.Count != hashes.Count)
            {
                throw new ArgumentException("Number of hashes must match the number of values.", nameof(hashes));
            }

            settings = settings ?? new HashSettings();

            bool result = true;

            // Validate all hashes
            for (int i = 0; i < values.Count; i++)
            {
                var value = values[i];
                var hash = hashes[i];

                if (!ValidateHashInternal(value, hash, settings))
                {
                    result = false;
                    break;
                }
            }

            // Log the operation
            SecurityDebug.LogSecurityOperation(null, "ValidateHashes", null, values.Join("\n"), result, null);

            // If hash is invalid redirect current request
            if (!result && settings.Redirect)
            {
                RedirectToBadHash();
            }

            return result;
        }

        #endregion


        #region "Load methods"

        /// <summary>
        /// Attempts to load the given new value as a string to the result. Returns true if the conversion of the new value was successful, if not, returns false and keeps the value the same.
        /// </summary>
        /// <param name="value">Value to load</param>
        /// <param name="newValue">New value</param>
        public static bool LoadString(ref string value, object newValue)
        {
            return LoadValue(ref value, newValue, GetString, null);
        }


        /// <summary>
        /// Attempts to load the given new value as an integer to the result. Returns true if the conversion of the new value was successful, if not, returns false and keeps the value the same.
        /// </summary>
        /// <param name="value">Value to load</param>
        /// <param name="newValue">New value</param>
        public static bool LoadInteger(ref int value, object newValue)
        {
            return LoadValue(ref value, newValue, GetInteger, Int32.MinValue);
        }


        /// <summary>
        /// Attempts to load the given new value as a long to the result. Returns true if the conversion of the new value was successful, if not, returns false and keeps the value the same.
        /// </summary>
        /// <param name="value">Value to load</param>
        /// <param name="newValue">New value</param>
        public static bool LoadLong(ref long value, object newValue)
        {
            return LoadValue(ref value, newValue, GetLong, Int64.MinValue);
        }


        /// <summary>
        /// Attempts to load the given new value as a double to the result. Returns true if the conversion of the new value was successful, if not, returns false and keeps the value the same.
        /// </summary>
        /// <param name="value">Value to load</param>
        /// <param name="newValue">New value</param>
        public static bool LoadDouble(ref double value, object newValue)
        {
            return LoadValue(ref value, newValue, GetDouble, Double.MinValue);
        }


        /// <summary>
        /// Attempts to load the given new value as a double to the result (it uses en-us culture). Returns true if the conversion of the new value was successful, if not, returns false and keeps the value the same.
        /// </summary>
        /// <param name="value">Value to load</param>
        /// <param name="newValue">New value</param>
        public static bool LoadDoubleSystem(ref double value, object newValue)
        {
            return LoadValue(ref value, newValue, GetDoubleSystem, Double.MinValue);
        }


        /// <summary>
        /// Attempts to load the given new value as a decimal to the result. Returns true if the conversion of the new value was successful, if not, returns false and keeps the value the same.
        /// </summary>
        /// <param name="value">Value to load</param>
        /// <param name="newValue">New value</param>
        internal static bool LoadDecimal(ref decimal value, object newValue)
        {
            return LoadValue(ref value, newValue, GetDecimal, decimal.MinValue);
        }


        /// <summary>
        /// Attempts to load the given new value as a decimal to the result (it uses en-us culture). Returns true if the conversion of the new value was successful, if not, returns false and keeps the value the same.
        /// </summary>
        /// <param name="value">Value to load</param>
        /// <param name="newValue">New value</param>
        internal static bool LoadDecimalSystem(ref decimal value, object newValue)
        {
            return LoadValue(ref value, newValue, GetDecimalSystem, decimal.MinValue);
        }


        /// <summary>
        /// Attempts to load the given new value as a DateTime to the result. Returns true if the conversion of the new value was successful, if not, returns false and keeps the value the same.
        /// </summary>
        /// <param name="value">Value to load</param>
        /// <param name="newValue">New value</param>
        public static bool LoadDateTime(ref DateTime value, object newValue)
        {
            return LoadValue(ref value, newValue, GetDateTime, DateTime.MinValue);
        }


        /// <summary>
        /// Attempts to load the given new value as a DateTime to the result (it uses en-us culture). Returns true if the conversion of the new value was successful, if not, returns false and keeps the value the same.
        /// </summary>
        /// <param name="value">Value to load</param>
        /// <param name="newValue">New value</param>
        public static bool LoadDateTimeSystem(ref DateTime value, object newValue)
        {
            return LoadValue(ref value, newValue, GetDateTimeSystem, DateTime.MinValue);
        }


        /// <summary>
        /// Attempts to load the given new value as a TimeSpan to the result (it uses en-us culture). Returns true if the conversion of the new value was successful, if not, returns false and keeps the value the same.
        /// </summary>
        /// <param name="value">Value to load</param>
        /// <param name="newValue">New value</param>
        internal static bool LoadTimeSpanSystem(ref TimeSpan value, object newValue)
        {
            return LoadValue(ref value, newValue, GetTimeSpanSystem, TimeSpan.Zero);
        }


        /// <summary>
        /// Attempts to load the given new value as a Guid to the result. Returns true if the conversion of the new value was successful, if not, returns false and keeps the value the same.
        /// </summary>
        /// <param name="value">Value to load</param>
        /// <param name="newValue">New value</param>
        public static bool LoadGuid(ref Guid value, object newValue)
        {
            return LoadValue(ref value, newValue, GetGuid, Guid.Empty);
        }


        /// <summary>
        /// Attempts to load the given new value as a bool to the result. Returns true if the conversion of the new value was successful, if not, returns false and keeps the value the same.
        /// </summary>
        /// <param name="value">Value to load</param>
        /// <param name="newValue">New value</param>
        public static bool LoadBoolean(ref bool value, object newValue)
        {
            bool? newVal = null;

            var result = LoadValue(ref newVal, newValue, GetNullableBoolean, null);
            if (newVal != null)
            {
                value = newVal.Value;
            }

            return result;
        }


        /// <summary>
        /// Attempts to load the given new value as a specific type to the result. Returns true if the conversion of the new value was successful, if not, returns false and keeps the value the same.
        /// </summary>
        /// <param name="value">Value to load</param>
        /// <param name="newValue">New value</param>
        /// <param name="conversion">Conversion method</param>
        /// <param name="specialValue">Special value to detect invalid conversion</param>
        private static bool LoadValue<TValue>(ref TValue value, object newValue, Func<object, TValue, TValue> conversion, TValue specialValue)
        {
            return LoadValue(ref value, newValue, (o, v, c) => conversion(o, v), specialValue);
        }


        /// <summary>
        /// Attempts to load the given new value as a specific type to the result. Returns true if the conversion of the new value was successful, if not, returns false and keeps the value the same.
        /// </summary>
        /// <param name="value">Value to load</param>
        /// <param name="newValue">New value</param>
        /// <param name="conversion">Conversion method</param>
        /// <param name="specialValue">Special value to detect invalid conversion</param>
        private static bool LoadValue<TValue>(ref TValue value, object newValue, Func<object, TValue, CultureInfo, TValue> conversion, TValue specialValue)
        {
            // If new value is null, assign directly
            if (IsNull(newValue))
            {
                value = default(TValue);
                return true;
            }

            // Direct cast if value matches target type
            if (newValue is TValue)
            {
                value = (TValue)newValue;
                return true;
            }

            // Try to convert the value
            TValue typedValue = conversion(newValue, specialValue, null);
            if (!typedValue.Equals(specialValue))
            {
                value = typedValue;
                return true;
            }

            // Conversion not successful
            return false;
        }


        /// <summary>
        /// Returns true if the given value is considered NULL value
        /// </summary>
        /// <param name="value">Value to check</param>
        private static bool IsNull(object value)
        {
            return (value == null) || (value == DBNull.Value);
        }

        #endregion
    }
}