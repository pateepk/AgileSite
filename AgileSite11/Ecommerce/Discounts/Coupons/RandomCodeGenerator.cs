using System;
using System.Collections.Generic;
using System.Text;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class generating random coupon codes according to specified pattern.
    /// </summary>
    public class RandomCodeGenerator
    {
        private readonly HashSet<string> mGeneratedCodes = new HashSet<string>(ECommerceHelper.CouponCodeComparer);
        private readonly Random mRandomNumber = new Random();

        private readonly ICodeUniquenessChecker mChecker;


        /// <summary>
        /// Pattern of generated codes. It is set via constructor.
        /// </summary>
        private readonly string mPattern;

        /// <summary>
        /// Prefix of codes. This string will be added at the beginning of all generated codes.
        /// </summary>
        private readonly string mPrefix;

        /// <summary>
        /// Characters A-Z.
        /// </summary>
        protected const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// Digits 0-9.
        /// </summary>
        protected const string DIGITS = "0123456789";

        /// <summary>
        /// Symbols A-Z and 0-9.
        /// </summary>
        protected const string SYMBOLS = CHARS + DIGITS;


        /// <summary>
        /// Initializes new instance of RandomCodeGenerator generating random codes starting with prefix. Format of codes is specified by codePattern
        /// parameter.
        /// </summary>
        /// <param name="checker">Ensures generated code uniqueness.</param>
        /// <param name="codePattern">Pattern for codes. Hash # stands for random digit, dollar $ stands for random character and asterisk for any alphanumeric symbol.
        /// Other characters are copied to output.</param>
        /// <param name="codePrefix">Every generated code will start with this prefix.</param>
        public RandomCodeGenerator(ICodeUniquenessChecker checker, string codePattern, string codePrefix = "")
        {
            if (checker == null)
            {
                throw new ArgumentNullException(nameof(checker));
            }

            mChecker = checker;
            mPattern = codePattern;
            mPrefix = codePrefix ?? string.Empty;
        }


        /// <summary>
        /// Generates one new unique code. Throws exception when not able to generate unique code within 10 attempts.
        /// </summary>
        public virtual string GenerateCode()
        {
            for (var i = 0; i < 10; i++)
            {
                var code = TryGenerateCode();

                if (CodeIsUnique(code))
                {
                    RememberCode(code);

                    return code;
                }
            }

            throw new TimeoutException($"Hard to find a unique code with given pattern ({mPattern}).");
        }


        /// <summary>
        /// Generates one random code.
        /// </summary>
        protected string TryGenerateCode()
        {
            var builder = new StringBuilder(mPrefix, mPrefix.Length + mPattern.Length);

            foreach (var charType in mPattern)
            {
                char newChar;
                switch (charType)
                {
                    case '#':
                        newChar = GetRandomDigit();
                        break;

                    case '$':
                        newChar = GetRandomChar();
                        break;

                    case '*':
                        newChar = GetRandomSymbol();
                        break;

                    default:
                        newChar = GetSymbolForType(charType);
                        break;
                }

                builder.Append(newChar);
            }

            return builder.ToString();
        }


        #region "Code uniquenes checking"

        /// <summary>
        /// Remembers given code as already generated.
        /// </summary>
        /// <param name="code">Code to be remembered.</param>
        protected virtual void RememberCode(string code)
        {
            mGeneratedCodes.Add(code);
        }


        /// <summary>
        /// Checks if given code is unique - not generated yet.
        /// </summary>
        /// <param name="code">Code to be checked.</param>
        protected virtual bool CodeIsUnique(string code)
        {
            return !mGeneratedCodes.Contains(code) && mChecker.IsUnique(code);
        }

        #endregion


        #region "Random characters generators"

        /// <summary>
        /// Returns random number from 0 to max (exclusive).
        /// </summary>
        /// <param name="max">Exclusive upper bound of the random number returned.</param>
        protected virtual int GetRandomNumber(int max)
        {
            return mRandomNumber.Next(0, max);
        }


        /// <summary>
        /// Returns random digit 0-9.
        /// </summary>
        protected virtual char GetRandomDigit()
        {
            return DIGITS[GetRandomNumber(10)];
        }


        /// <summary>
        /// Returns random character A-Z.
        /// </summary>
        protected virtual char GetRandomChar()
        {
            return CHARS[GetRandomNumber(CHARS.Length)];
        }


        /// <summary>
        /// Returns random symbol 0-9 or A-Z.
        /// </summary>
        protected virtual char GetRandomSymbol()
        {
            return SYMBOLS[GetRandomNumber(SYMBOLS.Length)];
        }


        /// <summary>
        /// Returns charType. Override this method to add custom character sets.
        /// </summary>
        /// <param name="charType">Character type to generate random character for.</param>
        protected virtual char GetSymbolForType(char charType)
        {
            return charType;
        }

        #endregion
    }
}
