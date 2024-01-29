using System;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Utility class for Azure Search fields.
    /// </summary>
    public class NamingHelper : AbstractHelper<NamingHelper>
    {
        private const string SYSTEM_FIELD_NAME_PREFIX = "sys";


        /// <summary>
        /// Gets field name which is valid to be used for Azure Search index.
        /// </summary>
        /// <param name="fieldName">Field name to be renamed so it fulfills Azure Search naming conventions.</param>
        /// <remarks>
        /// <para>
        /// Azure Search has naming rules (https://docs.microsoft.com/en-us/rest/api/searchservice/naming-rules) which field names must follow.
        /// Remarkably
        /// <list type="bullet">
        ///     <item><description>Allowed characters are letters, numbers and dashes ('_').</description></item>
        ///     <item><description>First character must be a letter.</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Since system fields start with a dash (e.g. <see cref="SearchFieldsConstants.ID"/>), the default implementation prefixes dash starting name of column by string "sys".
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldName"/> is null.</exception>
        public static string GetValidFieldName(string fieldName)
        {
            return HelperObject.GetValidFieldNameInternal(fieldName);
        }


        /// <summary>
        /// Gets document key which is valid to be used for Azure Search document.
        /// </summary>
        /// <param name="documentKey">Document key to be renamed so it fulfills Azure Search naming conventions.</param>
        /// <returns>Returns document key where semicolons are replaced by dashes.</returns>
        /// <remarks>
        /// <para>
        /// Azure Search has naming rules (https://docs.microsoft.com/en-us/rest/api/searchservice/naming-rules) which document keys must follow.
        /// </para>
        /// <para>
        /// Since the system uses semicolon (';') as a delimiter for compound identifiers, the default implementation replaces all semicolons by dashes ('-').
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="documentKey"/> is null.</exception>
        public static string GetValidDocumentKey(string documentKey)
        {
            return HelperObject.GetValidDocumentKeyInternal(documentKey);
        }


        /// <summary>
        /// Gets valid index name for Azure Search service.
        /// </summary>
        /// <param name="indexName">Index name to be renamed so it fulfills Azure Search naming conventions.</param>
        /// <remarks>
        /// Azure Search has naming rules (https://docs.microsoft.com/en-us/rest/api/searchservice/naming-rules) which index names must follow.
        /// </remarks>
        /// <returns>Returns string that has first character a letter or number and dots are replaced with dashes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="indexName"/> is null.</exception>
        public static string GetValidIndexName(string indexName)
        {
            return HelperObject.GetValidIndexNameInternal(indexName);
        }


        /// <summary>
        /// Gets field name which is valid to be used for Azure Search index.
        /// </summary>
        /// <param name="fieldName">Field name to be renamed so it fulfills Azure Search naming conventions.</param>
        /// <remarks>
        /// <para>
        /// Azure Search has naming rules (https://docs.microsoft.com/en-us/rest/api/searchservice/naming-rules) which field names must follow.
        /// Remarkably
        /// <list type="bullet">
        ///     <item><description>Allowed characters are letters, numbers and dashes ('_').</description></item>
        ///     <item><description>First character must be a letter.</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Since system fields start with a dash (e.g. <see cref="SearchFieldsConstants.ID"/>), the default implementation prefixes dash starting name of column by string "sys".
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldName"/> is null.</exception>
        protected virtual string GetValidFieldNameInternal(string fieldName)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException(nameof(fieldName), "Azure Search field name cannot be null.");
            }

            if (fieldName.StartsWith("_", StringComparison.Ordinal))
            {
                fieldName = SYSTEM_FIELD_NAME_PREFIX + fieldName;
            }
            return fieldName.ToLowerInvariant();
        }


        /// <summary>
        /// Gets document key which is valid to be used for Azure Search document.
        /// </summary>
        /// <param name="documentKey">Document key to be renamed so it fulfills Azure Search naming conventions.</param>
        /// <returns>Returns document key where semicolons are replaced by dashes.</returns>
        /// <remarks>
        /// <para>
        /// Azure Search has naming rules (https://docs.microsoft.com/en-us/rest/api/searchservice/naming-rules) which document keys must follow.
        /// </para>
        /// <para>
        /// Since the system uses semicolon (';') as a delimiter for compound identifiers, the default implementation replaces all semicolons by dashes ('-').
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="documentKey"/> is null.</exception>
        protected virtual string GetValidDocumentKeyInternal(string documentKey)
        {
            if (documentKey == null)
            {
                throw new ArgumentNullException(nameof(documentKey), "Azure Search document key cannot be null.");
            }

            return documentKey.Replace(';', '-');
        }


        /// <summary>
        /// Gets valid index name for Azure Search service.
        /// </summary>
        /// <param name="indexName">Index name to be renamed so it fulfills Azure Search naming conventions.</param>
        /// <remarks>
        /// Azure Search has naming rules (https://docs.microsoft.com/en-us/rest/api/searchservice/naming-rules) which index names must follow.
        /// </remarks>
        /// <returns>Returns string that has first character a letter or number and dots are replaced with dashes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="indexName"/> is null.</exception>
        private string GetValidIndexNameInternal(string indexName)
        {
            if (indexName == null)
            {
                throw new ArgumentNullException(nameof(indexName), "Azure Search index name cannot be null.");
            }

            CMSRegex rgx = new CMSRegex("[^a-z0-9 -]");
            return rgx.Replace(indexName.ToLowerInvariant().Replace('.', '-'), "").TrimStart(new char[] { '-' });
        }
    }
}
