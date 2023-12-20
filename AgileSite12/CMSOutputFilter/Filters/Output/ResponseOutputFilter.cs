using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Helpers;
using CMS.PortalEngine;
using CMS.Base;
using CMS.SiteProvider;
using CMS.DataEngine;
using CMS.DocumentEngine;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Fixes bugs in ASP.NET related to XHTML support and URL Rewriting.
    /// </summary>
    public class ResponseOutputFilter : Stream
    {
        #region "Events"

        /// <summary>
        /// Custom substitution event.
        /// </summary>
        public static event EventHandler<SubstitutionEventArgs> OnResolveSubstitution;


        /// <summary>
        /// Output filter handler.
        /// </summary>
        /// <param name="filter">Filter that has been created</param>
        public delegate void OutputFilterHandler(ResponseOutputFilter filter);


        /// <summary>
        /// Custom filter handler definition.
        /// </summary>
        /// <param name="filter">Output filter object</param>
        /// <param name="finalHtml">Final HTML code of the page to filter</param>
        public delegate void CustomFilterHandler(ResponseOutputFilter filter, ref string finalHtml);


        /// <summary>
        /// Fires before the filtering occurs.
        /// </summary>
        public event CustomFilterHandler OnBeforeFiltering;


        /// <summary>
        /// Fires after the filtering occurs.
        /// </summary>
        public event CustomFilterHandler OnAfterFiltering;


        /// <summary>
        /// Fires after the filter has been created.
        /// </summary>
        public static event OutputFilterHandler OnFilterCreated;

        #endregion


        #region "Variables"

        /// <summary>
        /// Default output filter capacity.
        /// </summary>
        private static int? mDefaultFilterCapacity;


        /// <summary>
        /// When true, the request is completed, when false, the Request.End is called.
        /// </summary>
        private static bool? mOutputFilterEndRequest;


        /// <summary>
        /// Response stream.
        /// </summary>
        private readonly Stream mResponseStream;


        /// <summary>
        /// Response HTML.
        /// </summary>
        private MemoryStream responseHtml;


        /// <summary>
        /// Encoding for the current filter
        /// </summary>
        private Encoding mEncoding;


        /// <summary>
        /// Substitution pattern used for searching output cache substitutions.
        /// </summary>
        private static readonly Regex SubstitutionPattern = new Regex(@"{~(.+?)~}", RegexOptions.Compiled);

        #endregion


        #region "Public properties"

        /// <summary>
        /// Default capacity for the output filter.
        /// </summary>
        public static int DefaultFilterCapacity
        {
            get
            {
                if (mDefaultFilterCapacity == null)
                {
                    mDefaultFilterCapacity = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSDefaultOutputFilterCapacity"], 32768);
                }

                return mDefaultFilterCapacity.Value;
            }
            set
            {
                mDefaultFilterCapacity = value;
            }
        }


        /// <summary>
        /// When true, the request is completed, when false, the Request.End is called.
        /// </summary>
        public static bool OutputFilterEndRequest
        {
            get
            {
                if (mOutputFilterEndRequest == null)
                {
                    mOutputFilterEndRequest = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSOutputFilterEndRequest"], true);
                }

                return mOutputFilterEndRequest.Value;
            }
        }


        /// <summary>
        /// If true, the filter is applied to the response.
        /// </summary>
        public bool ApplyFilter
        {
            get;
            set;
        } = true;


        /// <summary>
        /// If true, the filter logs the output to the file.
        /// </summary>
        public bool LogToFile
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the filter logs the output to the debug.
        /// </summary>
        public bool LogToDebug
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the filter uses GZip compression on output.
        /// </summary>
        public bool UseGZip
        {
            get;
            set;
        }


        /// <summary>
        /// Current response encoding
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                return mEncoding ?? (mEncoding = CMSHttpContext.Current.Response.ContentEncoding);
            }
            set
            {
                mEncoding = value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets string array of excluded Resolve filter urls.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string[] GetExcludedXHTMLResolveUrls(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSExcludedResolveFilterURLs").Split(';');
        }


        /// <summary>
        /// Gets string array of excluded Form filter urls.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string[] GetExcludedFormFilterURLs(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSExcludedFormFilterURLs").Split(';');
        }


        /// <summary>
        /// Gets string array of excluded XHTML filter urls.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string[] GetExcludedXHTMLFilterURLs(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSExcludedXHTMLFilterURLs").Split(';');
        }


        /// <summary>
        /// Gets string array of excluded XHTML Javascript filter urls.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string[] GetExcludedJavascriptFilterURLs(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSExcludedJavascriptFilterURLs").Split(';');
        }


        /// <summary>
        /// Gets string array of excluded XHTML LowerCase filter urls.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string[] GetExcludedLowerCaseFilterURLs(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSExcludedLowerCaseFilterURLs").Split(';');
        }


        /// <summary>
        /// Gets string array of excluded XHTML Attributes filter urls.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string[] GetExcludedAttributesFilterURLs(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSExcludedAttributesFilterURLs").Split(';');
        }


        /// <summary>
        /// Gets string array of excluded XHTML Self close filter urls.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string[] GetExcludedSelfCloseFilterURLs(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSExcludedSelfCloseFilterURLs").Split(';');
        }


        /// <summary>
        /// Gets string array of excluded HTML5 filter urls.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string[] GetExcludedHTML5FilterURLs(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSExcludedHTML5FilterURLs").Split(';');
        }


        /// <summary>
        /// Returns true if the indentation of the output is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool IndentOutputHtml(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSIndentOutputHtml");
        }


        /// <summary>
        /// Returns true if the conversion of TABLE tags to DIV tags is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static ConvertTableEnum ConvertTablesToDivs(string siteName)
        {
            // Get the settings
            string value = SettingsKeyInfoProvider.GetValue(siteName + ".CMSConvertTablesToDivs");

            return HTMLHelper.GetConvertTableEnum(value);
        }


        /// <summary>
        /// Gets the flag whether the view state should be moved to the end of the page for SEO purposes.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool MoveViewStateToEnd(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSMoveViewStateToEnd");
        }


        /// <summary>
        /// Creates new instance of output filter.
        /// </summary>
        public static ResponseOutputFilter CreateOutputFilter()
        {
            var response = CMSHttpContext.Current.Response;
            ResponseOutputFilter result = new ResponseOutputFilter(response.Filter);

            // Fire OnCreated event
            OnFilterCreated?.Invoke(result);

            return result;
        }


        /// <summary>
        /// Ensures that current request contains the output filter.
        /// </summary>
        public static ResponseOutputFilter EnsureOutputFilter()
        {
            ResponseOutputFilter filter = OutputFilterContext.CurrentFilter;
            if (filter == null)
            {
                filter = CreateOutputFilter();
                OutputFilterContext.CurrentFilter = filter;
            }

            return filter;
        }

        #endregion


        #region "Filter properties"

        /// <summary>
        ///
        /// </summary>
        public override bool CanRead => true;


        ///  <summary></summary>
        public override bool CanSeek => true;


        /// <summary>
        ///
        /// </summary>
        public override bool CanWrite => true;


        /// <summary>
        ///
        /// </summary>
        public override long Length => 0;


        /// <summary>
        ///
        /// </summary>
        public override long Position
        {
            get;
            set;
        }

        #endregion


        #region "Filter overrides"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inputStream">Stream of rendered HTML code</param>
        public ResponseOutputFilter(Stream inputStream)
        {
            LogToFile = false;
            mResponseStream = inputStream;

            responseHtml = new MemoryStream(DefaultFilterCapacity);
        }


        /// <summary>
        /// Closes the OutputFilter.
        /// </summary>
        public override void Close()
        {
            // Prepare the output
            string finalHtml = FilterResponse();
            OutputData output = new OutputData(finalHtml, UseGZip, Encoding);

            // Save the data to cache
            OutputHelper.SaveOutputToCache(output);

            // Write output to stream
            output.WriteOutputToStream(mResponseStream, false, ApplyFilter);

            // Log the output to the file
            OutputDebug.LogOutput(output, LogToFile, LogToDebug);

            mResponseStream.Close();

            // Throw out the response string object, it is not needed anymore
            responseHtml.Dispose();
            responseHtml = null;
        }


        /// <summary>
        /// Flushes the response stream.
        /// </summary>
        public override void Flush()
        {
            mResponseStream.Flush();
        }


        /// <summary>
        /// Moves the response stream position.
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return mResponseStream.Seek(offset, origin);
        }


        /// <summary>
        /// Sets the stream length.
        /// </summary>
        public override void SetLength(long length)
        {
            mResponseStream.SetLength(length);
        }


        /// <summary>
        /// Reads from the response stream.
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return mResponseStream.Read(buffer, offset, count);
        }

        #endregion


        #region "Modifications"

        /// <summary>
        /// Handles the write event.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            responseHtml?.Write(buffer, offset, count);
        }


        /// <summary>
        /// Gets document's relative path used in output filters.
        /// </summary>
        public static String GetFilterRelativePath()
        {
            string relativePath = RequestContext.CurrentRelativePath;

            // Manage 'getdoc' urls
            if (PortalContext.ViewMode != ViewModeEnum.LiveSite)
            {
                TreeNode tn = DocumentContext.CurrentDocument;
                if (tn != null)
                {
                    relativePath = DocumentURLProvider.GetUrl(DocumentContext.OriginalAliasPath, tn.DocumentUrlPath);

                    // Remove '~'
                    if (!String.IsNullOrEmpty(relativePath) && relativePath.StartsWith("~", StringComparison.Ordinal))
                    {
                        relativePath = relativePath.Substring(1);
                    }
                }
            }

            return relativePath;
        }

        /// <summary>
        /// Applies the output filter to the output data.
        /// </summary>
        /// <returns>Returns the filtered data</returns>
        private string FilterResponse()
        {
            // Convert buffer to HTML
            byte[] buffer = responseHtml.GetBuffer();

            string finalHtml = Encoding.GetString(buffer, 0, (int)responseHtml.Length);

            return FilterResponse(finalHtml);
        }


        /// <summary>
        /// Applies the output filter to the output data.
        /// </summary>
        /// <param name="finalHtml">Input HTML code</param>
        /// <returns>Returns the filtered data</returns>
        public string FilterResponse(string finalHtml)
        {
            // Get application path for resolving
            string appPath = SystemContext.ApplicationPath;

            var contentType = CMSHttpContext.Current.Response.ContentType;
            bool isHtml = string.Equals(contentType, "text/html", StringComparison.OrdinalIgnoreCase);
            bool isPlainText = string.Equals(contentType, "text/plain", StringComparison.OrdinalIgnoreCase);


            if (ApplyFilter && !RequestHelper.IsCallback())
            {
                RequestDebug.LogRequestOperation("FilterResponse", DataHelper.GetSizeString(finalHtml.Length), 0);

                // Custom filtering before default filters
                OnBeforeFiltering?.Invoke(this, ref finalHtml);

                string relativePath = GetFilterRelativePath();
                string siteName = SiteContext.CurrentSiteName;

                bool resolve = !URLHelper.IsExcluded(relativePath, GetExcludedXHTMLResolveUrls(siteName));
                bool resolved = false;

                // Fix XHTML for html response
                if (isHtml && !URLHelper.IsExcluded(relativePath, GetExcludedXHTMLFilterURLs(siteName)))
                {
                    // Get the settings for current page
                    bool fixJavascript = !URLHelper.IsExcluded(relativePath, GetExcludedJavascriptFilterURLs(siteName));
                    bool lowerCase = !URLHelper.IsExcluded(relativePath, GetExcludedLowerCaseFilterURLs(siteName));
                    bool attributes = !URLHelper.IsExcluded(relativePath, GetExcludedAttributesFilterURLs(siteName));
                    bool selfCloseTags = !URLHelper.IsExcluded(relativePath, GetExcludedSelfCloseFilterURLs(siteName));
                    bool html5 = !URLHelper.IsExcluded(relativePath, GetExcludedHTML5FilterURLs(siteName));

                    bool indent = IndentOutputHtml(siteName);
                    ConvertTableEnum tableToDiv = ConvertTablesToDivs(siteName);

                    // Fix XHTML with single method (single pass)
                    FixXHTMLSettings settings = new FixXHTMLSettings
                    {
                        LowerCase = lowerCase,
                        Attributes = attributes,
                        SelfClose = selfCloseTags,
                        ResolveUrl = resolve,
                        Javascript = fixJavascript,
                        Indent = indent,
                        TableToDiv = tableToDiv,
                        HTML5 = html5
                    };

                    finalHtml = HTMLHelper.FixXHTML(finalHtml, settings, appPath);
                    resolved = true;
                }

                // Resolve url in src= and href= and background=
                if (resolve && !resolved)
                {
                    if (isHtml)
                    {
                        // Resolve URLs in HTML
                        finalHtml = HTMLHelper.ResolveUrls(finalHtml, appPath, OutputFilterContext.CanResolveAllUrls);
                    }
                    else if (isPlainText && RequestHelper.IsAJAXRequest())
                    {
                        // Resolve URLs in AJAX
                        finalHtml = AJAXHelper.ResolveURLs(finalHtml, appPath, OutputFilterContext.CanResolveAllUrls);
                    }
                }

                // Move the view state to the end
                bool moveViewState = MoveViewStateToEnd(siteName);
                if (moveViewState)
                {
                    // Do not move view state Max field length is not set
                    System.Web.UI.Page page = CMSHttpContext.Current.Handler as System.Web.UI.Page;
                    if ((page != null) && (page.MaxPageStateFieldLength == -1))
                    {
                        // Locate the ViewState
                        int start = finalHtml.IndexOf("<input type=\"hidden\" name=\"__VIEWSTATE\"", StringComparison.OrdinalIgnoreCase);
                        if (start > -1)
                        {
                            // End of the ViewState
                            int end = finalHtml.IndexOf("/>", start, StringComparison.OrdinalIgnoreCase) + 2;
                            int formEnd = finalHtml.LastIndexOf("</form>", StringComparison.OrdinalIgnoreCase);

                            // Add the callback fix
                            const string CALLBACK_FIX = @"<script type=""text/javascript"">
      //<![CDATA[
      if (window.WebForm_InitCallback) {
        __theFormPostData = '';
        __theFormPostCollection = new Array();
        window.WebForm_InitCallback();
      }
      //]]>
    </script>
  ";

                            // Build the new HTML
                            StringBuilder sb = new StringBuilder(finalHtml.Length + CALLBACK_FIX.Length + 4);
                            sb.Append(finalHtml, 0, start);
                            sb.Append(finalHtml, end, formEnd - end);
                            sb.Append("  ");
                            sb.Append(finalHtml, start, end - start);
                            sb.Append("\r\n  ");
                            sb.Append(CALLBACK_FIX);
                            sb.Append(finalHtml, formEnd, finalHtml.Length - formEnd);

                            finalHtml = sb.ToString();
                        }
                    }
                }

                // Custom filtering before default filters
                OnAfterFiltering?.Invoke(this, ref finalHtml);
            }

            // Ensure the virtual prefixes
            if (VirtualContext.IsInitialized)
            {
                // Ensure custom prefix for preview link URLs
                URLHelper.PathModifierHandler previewHash = null;
                if (VirtualContext.IsPreviewLinkInitialized)
                {
                    previewHash = VirtualContext.AddPathHash;
                }

                if (isHtml)
                {
                    // Resolve URLs in HTML
                    finalHtml = HTMLHelper.EnsureURLPrefixes(finalHtml, appPath, VirtualContext.CurrentURLPrefix, previewHash);
                }
                else if (isPlainText && RequestHelper.IsAJAXRequest())
                {
                    // Resolve URLs in AJAX
                    finalHtml = AJAXHelper.EnsureURLPrefixes(finalHtml, appPath, VirtualContext.CurrentURLPrefix, previewHash);
                }
            }

            return finalHtml;
        }


        /// <summary>
        /// Resolves the substitutions in the given output HTML.
        /// </summary>
        /// <param name="html">Output HTML</param>
        /// <returns>Returns true if something was resolved</returns>
        public static bool ResolveSubstitutions(ref string html)
        {
            if (String.IsNullOrEmpty(html))
            {
                return false;
            }

            // Prepare the string builder (reserve 10% of the space for substitutions)
            StringBuilder resolvedHtml = new StringBuilder(html.Length * 110 / 100);

            int lastIndex = 0;
            bool somethingResolved = false;

            Match m = SubstitutionPattern.Match(html);
            while (m.Success)
            {
                // Append the content in between
                resolvedHtml.Append(html, lastIndex, m.Index - lastIndex);

                // Get the expression
                string expression = m.Groups[1].Value;

                // Resolve the substitution
                bool match;
                string resolved = ResolveSubstitutionInternal(expression, out match);
                if (match)
                {
                    // Append the result
                    somethingResolved = true;
                    resolvedHtml.Append(resolved);
                }
                else
                {
                    // Keep the expression
                    resolvedHtml.Append(m.Groups[0].Value);
                }

                lastIndex = m.Index + m.Length;
                m = m.NextMatch();
            }

            // Append the content in between
            resolvedHtml.Append(html, lastIndex, html.Length - lastIndex);

            // Get the data
            html = resolvedHtml.ToString();

            // Return flag whether something was resolved
            return somethingResolved;
        }


        /// <summary>
        /// Resolves the substitution.
        /// </summary>
        /// <param name="expression">Expression to resolve</param>
        /// <param name="match">Returns true if matched</param>
        private static string ResolveSubstitutionInternal(string expression, out bool match)
        {
            if (OnResolveSubstitution != null)
            {
                var eventArgs = new SubstitutionEventArgs
                {
                    Expression = expression
                };

                // Resolve the substitution via external handler
                OnResolveSubstitution(null, eventArgs);

                match = eventArgs.Match;
                return eventArgs.Result;
            }

            match = false;
            return null;
        }

        #endregion
    }
}