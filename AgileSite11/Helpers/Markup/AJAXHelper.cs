using System;
using System.Collections;
using System.Reflection;
using System.Text;

using CMS.Core;
using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// AJAX helper.
    /// </summary>
    public static class AJAXHelper
    {
        #region "Variables"

        /// <summary>
        /// Type of response part containing script path
        /// </summary>
        private const string SCRIPT_PATH_TYPE = "ScriptPath";

        /// <summary>
        /// Length field index.
        /// </summary>
        public const int FIELD_LENGTH = 0;

        /// <summary>
        /// Type field index.
        /// </summary>
        public const int FIELD_TYPE = 1;

        /// <summary>
        /// ID field index.
        /// </summary>
        public const int FIELD_ID = 2;

        /// <summary>
        /// Content field index.
        /// </summary>
        public const int FIELD_CONTENT = 3;

        /// <summary>
        /// Field separator.
        /// </summary>
        public const string SEPARATOR = "|";

        /// <summary>
        /// AjaxToolkitVersion.
        /// </summary>
        private static int mAjaxToolkitVersion = 0;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns major verion of loaded ajaxtoolkit.
        /// </summary>
        [Obsolete("Use custom code instead.")]
        public static int AjaxToolkitVersion
        {
            get
            {
                if (mAjaxToolkitVersion == 0)
                {
                    try
                    {
                        // Try load AjaxControlToolkit assembly
                        Assembly ajaxAssembly = ClassHelper.GetAssembly("AjaxControlToolkit");
                        AssemblyName ajaxAssemblyName = ajaxAssembly.GetName();

                        // Get AjaxControlToolkit major version
                        mAjaxToolkitVersion = ajaxAssemblyName.Version.Major;
                    }
                    catch
                    {
                    }
                }
                return mAjaxToolkitVersion;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the next field from the AJAX response (all text until next separator).
        /// </summary>
        /// <param name="response">Response string</param>
        /// <param name="start">Start index</param>
        public static string GetNextField(string response, int start)
        {
            if (start >= response.Length)
            {
                return null;
            }

            int separatorIndex = response.IndexOfCSafe(SEPARATOR, start);
            if (separatorIndex < 0)
            {
                return null;
            }

            return response.Substring(start, separatorIndex - start);
        }


        /// <summary>
        /// Parses the AJAX response returning the list of string arrays [length, type, id, content].
        /// </summary>
        /// <param name="response">Response string</param>
        public static ArrayList ParseResponse(string response)
        {
            ArrayList list = new ArrayList();

            int index = 0;
            while (true)
            {
                // Length field
                string lengthString = GetNextField(response, index);
                if (lengthString == null)
                {
                    break;
                }
                index += (lengthString.Length + 1);
                int length = Convert.ToInt32(lengthString);

                // Type field
                string type = GetNextField(response, index);
                index += (type.Length + 1);

                // ID field
                string id = GetNextField(response, index);
                index += (id.Length + 1);

                // Content
                string content = response.Substring(index, length);
                index += (length + 1);

                list.Add(new string[] { lengthString, type, id, content });
            }

            return list;
        }


        /// <summary>
        /// Builds the response string from given response array.
        /// </summary>
        /// <param name="response">Response array</param>
        /// <param name="expectedSize">Expected size of the response in bytes</param>
        public static string BuildResponse(ArrayList response, int expectedSize)
        {
            // Prepare the string builder
            StringBuilder sb = null;
            if (expectedSize <= 0)
            {
                sb = new StringBuilder();
            }
            else
            {
                sb = new StringBuilder(expectedSize);
            }

            foreach (string[] fields in response)
            {
                fields[FIELD_LENGTH] = fields[FIELD_CONTENT].Length.ToString();
                sb.Append(fields[FIELD_LENGTH]);

                sb.Append(SEPARATOR);
                sb.Append(fields[FIELD_TYPE]);
                sb.Append(SEPARATOR);
                sb.Append(fields[FIELD_ID]);
                sb.Append(SEPARATOR);
                sb.Append(fields[FIELD_CONTENT]);
                sb.Append(SEPARATOR);
            }

            return sb.ToString();
        }


        /// <summary>
        /// Resolves URLs in the response array content.
        /// </summary>
        /// <param name="response">Response array</param>
        /// <param name="applicationPath">Application path</param>
        public static void ResolveURLs(ArrayList response, string applicationPath)
        {
            ResolveURLs(response, applicationPath, true);
        }


        /// <summary>
        /// Resolves URLs in the response array content.
        /// </summary>
        /// <param name="response">Response array</param>
        /// <param name="applicationPath">Application path</param>
        /// <param name="canResolveAllUrls">Determines whether resolving of all url can be used</param>
        public static void ResolveURLs(ArrayList response, string applicationPath, bool canResolveAllUrls)
        {
            foreach (string[] fields in response)
            {
                fields[FIELD_CONTENT] = HTMLHelper.ResolveUrls(fields[FIELD_CONTENT], applicationPath, canResolveAllUrls);
            }
        }


        /// <summary>
        /// Resolves URLs in the response array content.
        /// </summary>
        /// <param name="response">Response string</param>
        /// <param name="applicationPath">Application path</param>
        public static string ResolveURLs(string response, string applicationPath)
        {
            return ResolveURLs(response, applicationPath, true);
        }


        /// <summary>
        /// Resolves URLs in the response array content.
        /// </summary>
        /// <param name="response">Response string</param>
        /// <param name="applicationPath">Application path</param>
        /// <param name="canResolveAllUrls">Determines whether resolving of all url can be used</param>
        public static string ResolveURLs(string response, string applicationPath, bool canResolveAllUrls)
        {
            try
            {
                // Parse the response and resolve URLs
                ArrayList list = ParseResponse(response);

                ResolveURLs(list, applicationPath, canResolveAllUrls);

                return BuildResponse(list, (int)(response.Length * 1.1));
            }
            catch (Exception ex)
            {
                // In case of error, get original response
                CoreServices.EventLog.LogException("AJAXHelper", "ResolveURLs", ex);

                return response;
            }
        }


        /// <summary>
        /// Ensures that all local URLs have given prefix
        /// </summary>
        /// <param name="response">Html code</param>
        /// <param name="applicationPath">Application path</param>
        /// <param name="urlPrefix">URL prefix to ensure</param>
        public static void EnsureURLPrefixes(ArrayList response, string applicationPath, string urlPrefix)
        {
            EnsureURLPrefixes(response, applicationPath, urlPrefix, null);
        }


        /// <summary>
        /// Ensures that all local URLs have given prefix
        /// </summary>
        /// <param name="response">Html code</param>
        /// <param name="applicationPath">Application path</param>
        /// <param name="urlPrefix">URL prefix to ensure</param>
        /// <param name="urlModifier">URL modifier to apply</param>
        public static void EnsureURLPrefixes(ArrayList response, string applicationPath, string urlPrefix, URLHelper.PathModifierHandler urlModifier)
        {
            foreach (string[] fields in response)
            {
                // Ensure prefix for script path parts
                if (fields[FIELD_ID] == SCRIPT_PATH_TYPE)
                {
                    var url = fields[FIELD_CONTENT];

                    // Ensure the virtual prefix only if not present yet
                    if (urlPrefix.StartsWith(VirtualContext.VirtualContextPrefix, StringComparison.Ordinal) && !VirtualContext.ContainsVirtualContextPrefix(url))
                    {
                        // Ensure prefix for local URLs only
                        fields[FIELD_CONTENT] = URLHelper.EnsureURLPrefix(url, applicationPath, urlPrefix, urlModifier);
                    }
                }
                // Ensure prefixes in rest of response
                else
                {
                    fields[FIELD_CONTENT] = HTMLHelper.EnsureURLPrefixes(fields[FIELD_CONTENT], applicationPath, urlPrefix, urlModifier);
                }
            }
        }


        /// <summary>
        /// Ensures that all local URLs have given prefix
        /// </summary>
        /// <param name="response">Html code</param>
        /// <param name="applicationPath">Application path</param>
        /// <param name="urlPrefix">URL prefix to ensure</param>
        public static string EnsureURLPrefixes(string response, string applicationPath, string urlPrefix)
        {
            return EnsureURLPrefixes(response, applicationPath, urlPrefix, null);
        }


        /// <summary>
        /// Ensures that all local URLs have given prefix
        /// </summary>
        /// <param name="response">Html code</param>
        /// <param name="applicationPath">Application path</param>
        /// <param name="urlPrefix">URL prefix to ensure</param>
        /// <param name="urlModifier">URL modifier to apply</param>
        public static string EnsureURLPrefixes(string response, string applicationPath, string urlPrefix, URLHelper.PathModifierHandler urlModifier)
        {
            try
            {
                // Parse the response and resolve URLs
                ArrayList list = ParseResponse(response);

                EnsureURLPrefixes(list, applicationPath, urlPrefix, urlModifier);

                return BuildResponse(list, (int)(response.Length * 1.1));
            }
            catch (Exception ex)
            {
                // In case of error, get original response
                CoreServices.EventLog.LogException("AJAXHelper", "EnsureURLPrefixes", ex);

                return response;
            }
        }

        #endregion
    }
}