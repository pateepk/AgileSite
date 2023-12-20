using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Generator for unique A/B variant names.
    /// </summary>
    internal class VariantNameGenerator
    {
        private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string PREFIX = "Variant ";
        private const string ORIGINAL_SUFFIX = "original";

        private readonly Regex mRegex;


        /// <summary>
        /// Creates a new instance of <see cref="VariantNameGenerator"/>.
        /// </summary>
        public VariantNameGenerator()
        {
            mRegex = new Regex($"^{PREFIX}(?<character>[A-Z]+)( \\({ORIGINAL_SUFFIX}\\))?$");
        }


        /// <summary>
        /// Returns name of original variant.
        /// </summary>
        public string GetOriginalName()
        {
            return $"{PREFIX}{CHARS[0]} ({ORIGINAL_SUFFIX})";
        }


        /// <summary>
        /// Returns unique name in default format "Variant A" with character which is not used in any of given <paramref name="existingNames"/>.
        /// </summary>
        /// <param name="existingNames">Set of existing names</param>
        public string GetDefaultUniqueName(IEnumerable<string> existingNames)
        {
            var existingCharacters = existingNames.Select(i => mRegex.Match(i).Groups["character"]).Select(i => i.Value).ToList();
            if (!existingCharacters.Any(i => i.Length > 0))
            {
                return PREFIX + CHARS[0];
            }

            var lastExistingName = existingCharacters.OrderBy(i => i.Length).ThenBy(i => i).Last();
            var existingIndexes = lastExistingName.Reverse().Select(character => CHARS.IndexOf(character)).ToList();

            return PREFIX + GetName(existingIndexes);
        }


        /// <summary>
        /// Returns name based on given collection of indexes from <see cref="CHARS"/> constant.
        /// </summary>
        private static string GetName(ICollection<int> existingIndexes)
        {
            return existingIndexes.All(i => i < CHARS.Length - 1) 
                ? GetNameWithoutExtension(existingIndexes) 
                : GetNameWithExtension(existingIndexes);
        }


        /// <summary>
        /// Returns name based on given collection of indexes representing characters in <see cref="CHARS"/> constant.
        /// Name is not required to be extended as none index represents last character in <see cref="CHARS"/> constant.
        /// </summary>
        private static string GetNameWithoutExtension(IEnumerable<int> existingIndexes)
        {
            var result = "";
            var first = true;

            foreach (var index in existingIndexes)
            {
                var character = CHARS[index];
                if (first)
                {
                    character = CHARS[index + 1];
                    first = false;
                }

                result = character + result;
            }

            return result;
        }


        /// <summary>
        /// Returns name based on given collection of indexes representing characters in <see cref="CHARS"/> constant.
        /// Name is required to be extended because atleast one index represents last character in <see cref="CHARS"/> constant.
        /// </summary>
        private static string GetNameWithExtension(IEnumerable<int> existingIndexes)
        {
            var result = "";
            var additionalCharacterNeeded = false;

            foreach (var integer in existingIndexes)
            {
                if (integer < CHARS.Length - 1)
                {
                    result = CHARS[integer + 1] + result;
                    additionalCharacterNeeded = false;
                }
                else
                {
                    result = CHARS[0] + result;
                    additionalCharacterNeeded = true;
                }
            }

            if (additionalCharacterNeeded)
            {
                result += CHARS[0];
            }

            return result;
        }
    }
}
