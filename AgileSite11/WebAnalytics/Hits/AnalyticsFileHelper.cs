using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.Helpers;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides methods to handle web analytics files.
    /// </summary>
    internal static class AnalyticsFileHelper
    {
        private const char REPLACEMENT_CHARACTER = '-';

        private static string mFileNameInstanceIdentifier;
        private static IEnumerable<char> mInvalidCharacters;


        /// <summary>
        /// Gets regular expression for time format HHmm.
        /// </summary>
        private static Regex TimeRegex => RegexHelper.GetRegex(@"^\d\d\d\d$");


        /// <summary>
        /// Gets file name instance identifier inferred from <see cref="SystemContext.InstanceName"/>.
        /// </summary>
        private static string FileNameInstanceIdentifier
        {
            get
            {
                return mFileNameInstanceIdentifier ?? (mFileNameInstanceIdentifier = GetFileNameInstanceIdentifier(SystemContext.InstanceName));
            }
        }


        /// <summary>
        /// Gets invalid characters for file name.
        /// </summary>
        private static IEnumerable<char> InvalidCharacters
        {
            get
            {
                if (mInvalidCharacters == null)
                {
#pragma warning disable BH1014
                    var invalidCharacters = Path.GetInvalidFileNameChars().ToList();
                    invalidCharacters.Add('_');
#pragma warning disable BH1014

                    mInvalidCharacters = invalidCharacters;
                }

                return mInvalidCharacters;
            }
        }


        /// <summary>
        /// Returns name of the file base on given parameters and also on (non)shared location of the <see cref="HitLogProvider.LogDirectory"/>.
        /// </summary>
        public static string GetFileName(string codeName, DateTime dateTime)
        {
            var strNow = dateTime.ToString("yyMMdd_HHmm", CultureInfo.InvariantCulture);
            var fileName = IO.StorageHelper.IsSharedStorage(HitLogProvider.LogDirectory)
                ? $"{codeName}_{strNow}_{FileNameInstanceIdentifier}.log"
                : $"{codeName}_{strNow}.log";

            return fileName;
        }


        /// <summary>
        /// Gets <paramref name="instanceIdentifier"/> converted to a representation which can be used in a file name.
        /// </summary>
        internal static string GetFileNameInstanceIdentifier(string instanceIdentifier)
        {
            string validFileName = instanceIdentifier;
            foreach (var character in InvalidCharacters)
            {
                validFileName = validFileName.Replace(character, REPLACEMENT_CHARACTER);
            }

            return validFileName;
        }


        /// <summary>
        /// Removes machine name from file name.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public static string RemoveMachineName(string fileName)
        {
            fileName = fileName.ToLowerInvariant();

            // Find last underscore, part after that could be time in format HHmm or machine name.
            var lastUnderscore = fileName.LastIndexOf("_", StringComparison.Ordinal) + 1;
            var dateTimeStr = fileName.Substring(lastUnderscore).Replace(".log", string.Empty);

            // Try to find out if its time or machine name            
            if (!TimeRegex.IsMatch(dateTimeStr))
            {
                // Given part is not time, remove it
                fileName = fileName.Replace("_" + dateTimeStr, string.Empty);
            }

            return fileName;
        }
    }
}
