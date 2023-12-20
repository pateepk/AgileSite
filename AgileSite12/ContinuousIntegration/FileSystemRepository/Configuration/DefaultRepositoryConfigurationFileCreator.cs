using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;
using CMS.IO;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Class provides functionality to create default Continuous Integration repository configuration stored in file system.
    /// </summary>
    internal static class DefaultRepositoryConfigurationFileCreator
    {
        private const string SETTINGS_WARNING = @"<!-- Settings may contain sensitive data and are excluded from continuous integration by default.
         Only remove the exclusion if you agree to make all setting values available within the file system used by the application
         and any connected source control systems. -->";


        /// <summary>
        /// Contains object types excluded from repository by default
        /// </summary>
        private static readonly Lazy<HashSet<string>> DefaultExcludedObjectTypes = new Lazy<HashSet<string>>(() =>
            new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                SettingsKeyInfo.OBJECT_TYPE
            });


        /// <summary>
        /// Stores initial configuration file for the repository.
        /// </summary>
        /// <param name="mainObjectTypeProvider">Main object types provider.</param>
        /// <param name="path">Path to the configuration file.</param>
        /// <remarks>
        /// The initial configuration does not limit storing of object types except for types explicitly mentioned in <see cref="DefaultExcludedObjectTypes"/>.
        /// <para>The file contains comments with hints for configuration.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="mainObjectTypeProvider"/></exception>
        public static void StoreInitial(IMainObjectTypeProvider mainObjectTypeProvider, string path)
        {
            if (mainObjectTypeProvider == null)
            {
                throw new ArgumentNullException(nameof(mainObjectTypeProvider));
            }

            var encoding = EncodingConfiguration.Encoding;
            var supportedMainObjectTypes = mainObjectTypeProvider.GetObjectTypes().OrderBy(it => it);
            var sampleConfigXml = GetInitialConfigurationXml(supportedMainObjectTypes);
            var fileContent = String.Format(@"<?xml version=""1.0"" encoding=""{0}""?>{1}", encoding.HeaderName, sampleConfigXml);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (var sw = StreamWriter.New(path, false, encoding))
            {
                sw.Write(fileContent);
            }
        }


        /// <summary>
        /// Gets sample configuration XML <paramref name="supportedMainObjectTypes"/> for reference in a comment.
        /// </summary>
        /// <param name="supportedMainObjectTypes">Enumeration of supported main object types to be included as a reference in an XML comment.</param>
        /// <returns>Sample configuration XML.</returns>
        private static string GetInitialConfigurationXml(IEnumerable<string> supportedMainObjectTypes)
        {
            const string sampleConfigFileContent = @"
<{0} xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <!-- After changing the included or excluded object types, you need to run serialization for all objects to bring 
       the CIRepository folder into the required state. Open the 'Continuous integration' application in the Kentico administration interface
       and click 'Serialize all objects'. Always synchronize the repository.config settings along with the other content of the CIRepository folder. -->

  <!-- The following main object types can be used in the {1} and {2} definition:
         {3} -->

  <!-- If one or more main object types are specified, continuous integration only processes objects of the given type and their child types. 
       All supported object types are included if empty. -->
  <{1}>
    <!-- <{4}>ObjectTypeA</{4}> -->
    <!-- <{4}>ObjectTypeB</{4}> -->
  </{1}>

  <!-- Continuous integration processes all included object types except for the listed types. -->
  <{2}>
    {5}
    <!-- <{4}>ObjectTypeX</{4}> -->
    <!-- <{4}>ObjectTypeY</{4}> -->
  </{2}>

  <!-- You can exclude objects from processing by specifying excluded code names for any object type. 
       Use the % wildcard at the start or end of the values to exclude multiple objects based on code name prefixes or suffixes.
       You can add multiple code name values separated by semicolons for each object type. -->
  <{6}>
    <!-- <{7} ObjectType=""ObjectTypeA"">ExcludedCodeNameA;ExcludedCodeNameB</{7}> -->
    <!-- <{7} ObjectType=""ObjectTypeB"">ExcludedCodeNamePrefix%;%ExcludedCodeNameSuffix</{7}> -->
  </{6}>

</{0}>";
            var supportedMainObjectTypesString = GetSupportedMainObjectTypesString(supportedMainObjectTypes);
            var excludedObjectTypesString = GetDefaultExcludedTypesString();

            string res = String.Format(sampleConfigFileContent,
                RepositoryConfigurationFile.REPOSITORY_CONFIGURATION_ELEMENT_NAME, RepositoryConfigurationFile.INCLUDED_OBJECT_TYPES_ELEMENT_NAME,
                RepositoryConfigurationFile.EXCLUDED_OBJECT_TYPES_ELEMENT_NAME, supportedMainObjectTypesString,
                RepositoryConfigurationFile.OBJECT_TYPE_ELEMENT_NAME, excludedObjectTypesString, RepositoryConfigurationFile.OBJECT_FILTERS_ELEMENT_NAME,
                RepositoryConfigurationFile.OBJECT_EXCLUDED_CODE_NAMES_ELEMENT_NAME);

            return res;
        }


        /// <summary>
        /// Returns part of the configuration file with all supported object types
        /// </summary>
        /// <param name="supportedMainObjectTypes">Enumeration of supported main object types to be included as a reference in an XML comment.</param>
        /// <returns>Configuration part with supported object types as a string</returns>
        private static string GetSupportedMainObjectTypesString(IEnumerable<string> supportedMainObjectTypes)
        {
            const int objectTypesIndent = 9;
            const int objectTypesPerLine = 5;

            int i = 0;
            var supportedMainObjectTypesStr = supportedMainObjectTypes.Aggregate(new StringBuilder(), (sb, s) =>
            {
                if (sb.Length == 0)
                {
                    sb.Append(s);
                }
                else if (i % objectTypesPerLine == 0)
                {
                    sb.AppendLine(",").Append(' ', objectTypesIndent).Append(s);
                }
                else
                {
                    sb.Append(", ").Append(s);
                }

                ++i;

                return sb;
            });

            return supportedMainObjectTypesStr.ToString();
        }


        /// <summary>
        /// Returns part of the configuration file with excluded object types
        /// </summary>
        /// <returns>Configuration part with excluded object types as a string</returns>
        private static string GetDefaultExcludedTypesString()
        {
            if (!DefaultExcludedObjectTypes.Value.Any())
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(SETTINGS_WARNING);

            foreach (var defaultExcludedObjectType in DefaultExcludedObjectTypes.Value)
            {
                builder.Append(Environment.NewLine);
                builder.AppendFormat("    <{0}>{1}</{0}>", RepositoryConfigurationFile.OBJECT_TYPE_ELEMENT_NAME, defaultExcludedObjectType);
            }

            return builder.ToString();
        }
    }
}
