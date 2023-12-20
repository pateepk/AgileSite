using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.MacroEngine;
using CMS.OutputFilter;
using CMS.PortalEngine;
using CMS.Routing.Web;
using CMS.SiteProvider;
using CMS.UIControls;

using HttpCacheability = System.Web.HttpCacheability;
using HttpCacheRevalidation = System.Web.HttpCacheRevalidation;

[assembly: RegisterHttpHandler("CMSPages/GetResource.ashx", typeof(GetResourceHandler), Order = 1)]

namespace CMS.UIControls
{
    /// <summary>
    /// Handler that serves minified and compressed resources.
    /// </summary>
    internal class GetResourceHandler : IHttpHandler
    {
        #region "Constants"

        /// <summary>
        /// List of the allowed general file extensions
        /// </summary>
        private static readonly HashSet<string> mAllowedFileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".woff", ".eot", ".svg", ".ttf" };


        /// <summary>
        /// Supported querystring argument used to identify javascript files.
        /// </summary>
        private const string JS_FILE_ARGUMENT = "scriptfile";


        /// <summary>
        /// Supported querystring argument used to identify QR code
        /// </summary>
        private const string QR_CODE_ARGUMENT = "qrcode";


        /// <summary>
        /// Supported querystring argument used to identify image files.
        /// </summary>
        private const string IMAGE_FILE_ARGUMENT = "image";


        /// <summary>
        /// Supported querystring argument used to identify general files
        /// </summary>
        private const string FILE_ARGUMENT = "file";


        /// <summary>
        /// Supported querystring argument used to identify newsletter stylesheets stored in a database.
        /// </summary>
        private const string NEWSLETTERCSS_DATABASE_ARGUMENT = "newslettertemplatename";


        /// <summary>
        /// Supported querystring argument used to identify JavaScript module.
        /// </summary>
        private const string SCRIPT_MODULE_ARGUMENT = "scriptmodule";


        /// <summary>
        /// Extension of a CSS file.
        /// </summary>
        private const string CSS_FILE_EXTENSION = ".css";


        /// <summary>
        /// Extension of JS file.
        /// </summary>
        private const string JS_FILE_EXTENSION = ".js";


        /// <summary>
        /// Extension of image file.
        /// </summary>
        private const string IMAGE_FILE_EXTENSION = "##IMAGE##";


        /// <summary>
        /// Extension of general file.
        /// </summary>
        private const string FILE_EXTENSION = "##FILE##";


        /// <summary>
        /// Name of the application settings key. Determines whether relative CSS URLs are resolved to absolute URLs (=TRUE) or not (=FALSE).
        /// Default value is set to False.
        /// </summary>
        private const string SETTINGS_USE_ABSOLUTE_CSS_CLIENT_URLS = "CMSUseAbsoluteCSSClientURLs";

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets a value indicating whether another request can use the IHttpHandler instance.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Site name.
        /// </summary>
        private static string SiteName
        {
            get
            {
                return ConnectionHelper.ConnectionAvailable ? SiteContext.CurrentSiteName : string.Empty;
            }
        }


        /// <summary>
        /// Gets the number of minutes the resource will be cached on the server.
        /// </summary>
        private static int CacheMinutes
        {
            get
            {
                if (ConnectionHelper.ConnectionAvailable)
                {
                    return CacheHelper.CacheImageMinutes(SiteName);
                }
                else
                {
                    return 0;
                }
            }
        }


        /// <summary>
        /// Gets the number of minutes the resource will be cached on the client.
        /// </summary>
        private static int ClientCacheMinutes
        {
            get
            {
                if (ConnectionHelper.ConnectionAvailable)
                {
                    return CacheHelper.ClientCacheMinutes(SiteName);
                }
                else
                {
                    return 0;
                }
            }
        }


        /// <summary>
        /// Gets if the client caching is enabled.
        /// </summary>
        private static bool ClientCacheEnabled
        {
            get
            {
                return CacheHelper.ClientCacheRequested;
            }
        }


        /// <summary>
        /// Gets if the client cache should be revalidated to ensure all data is up-to-date.
        /// </summary>
        private static bool RevalidateClientCache
        {
            get
            {
                return !ConnectionHelper.ConnectionAvailable || CacheHelper.RevalidateClientCache(SiteName);
            }
        }


        /// <summary>
        /// Gets the number of minutes large files (those over maximum allowed size) will be cached on the server.
        /// </summary>
        private static int PhysicalFilesCacheMinutes
        {
            get
            {
                return CacheHelper.PhysicalFilesCacheMinutes;
            }
        }


        /// <summary>
        /// Gets if Not Found response should be used when the resource cannot be located or if the request should terminate normally.
        /// </summary>
        private static bool Throw404WhenNotFound
        {
            get
            {
                return true;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Processes the incoming HTTP request that and returns the specified stylesheets.
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        public void ProcessRequest(HttpContext context)
        {
            // Disable debugging
            if (!DebugHelper.DebugResources)
            {
                // Disable the debugging
                DebugHelper.DisableDebug();

                OutputFilterContext.LogCurrentOutputToFile = false;
            }

            // When no parameters are specified, simply end the response
            if (!context.Request.QueryString.HasKeys())
            {
                SendNoContent(context);
            }

            // Process JS file request
            if (QueryHelper.Contains(JS_FILE_ARGUMENT))
            {
                ProcessJSFileRequest(context);
                return;
            }

            // Process QR code request
            if (QueryHelper.Contains(QR_CODE_ARGUMENT))
            {
                ProcessQRCodeRequest(context);
                return;
            }

            // Process image file request
            if (QueryHelper.Contains(IMAGE_FILE_ARGUMENT))
            {
                ProcessImageFileRequest(context);
                return;
            }

            // Process general file request
            if (QueryHelper.Contains(FILE_ARGUMENT))
            {
                ProcessFileRequest(context);
                return;
            }

            // Process general file request
            if (QueryHelper.Contains(SCRIPT_MODULE_ARGUMENT))
            {
                ProcessScriptModuleRequest(context, QueryHelper.GetString(SCRIPT_MODULE_ARGUMENT, null));
                return;
            }

            // Transfer to newsletter CSS
            string newsletterTemplateName = QueryHelper.GetString(NEWSLETTERCSS_DATABASE_ARGUMENT, "");
            if (!String.IsNullOrEmpty(newsletterTemplateName))
            {
                var newsletterCSSHandler = new GetNewsletterCSSHandler();
                newsletterCSSHandler.ProcessRequest(context);
                return;
            }

            // Load the settings
            var settings = new CMSCssSettings();
            settings.LoadFromQueryString();

            // Process the request
            ProcessRequest(context, settings);
        }


        /// <summary>
        /// Processes the given request.
        /// </summary>
        /// <param name="context">Http context</param>
        /// <param name="settings">CSS Settings</param>
        private static void ProcessRequest(HttpContext context, CMSCssSettings settings)
        {
            // Get cache setting for physical files
            int cacheMinutes = PhysicalFilesCacheMinutes;
            int clientCacheMinutes = cacheMinutes;

            bool hasVirtualContent = settings.HasVirtualContent();
            if (hasVirtualContent)
            {
                // Use specific cache settings if DB resources are requested
                cacheMinutes = CacheMinutes;
                clientCacheMinutes = ClientCacheMinutes;
            }

            // Try to get data from cache (or store them if not found)
            var resource = CacheHelper.Cache(
                cs => LoadResource(settings, cs),
                new CacheSettings(cacheMinutes, "getresource", SiteContext.CurrentSiteName, RequestContext.CurrentDomain, QueryHelper.GetParameterString(), RequestContext.IsSSL)
            );


            // Send response if there's something to send
            if (resource != null)
            {
                bool allowCache = (!hasVirtualContent || ClientCacheEnabled) && CacheHelper.AlwaysCacheResources;
                SendResponse(context, resource, allowCache, CssLinkHelper.StylesheetMinificationEnabled, clientCacheMinutes, false);
            }
            else
            {
                SendNotFoundResponse();
            }
        }


        /// <summary>
        /// Loads the resource from database
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="cs">Cached section</param>
        private static CMSOutputResource LoadResource(CMSCssSettings settings, CacheSettings cs)
        {
            // Load the data
            var result = GetResource(settings, cs.Cached);

            // Cache the file
            if ((result != null) && cs.Cached)
            {
                cs.CacheDependency = result.CacheDependency;
            }

            return result;
        }

        #endregion


        #region "General methods"

        /// <summary>
        /// Combines the given list of resources into a single resource
        /// </summary>
        /// <param name="resources">Resources to combine</param>
        private static CMSOutputResource CombineResources(IEnumerable<CMSOutputResource> resources)
        {
            StringBuilder data = new StringBuilder();
            StringBuilder etag = new StringBuilder();

            DateTime lastModified = DateTimeHelper.ZERO_TIME;
            List<string> files = new List<string>();

            int count = 0;
            bool atLeastOne = false;

            string fileName = null;

            // Build single resource
            foreach (CMSOutputResource resource in resources)
            {
                if (resource != null)
                {
                    atLeastOne = true;
                    string newData = resource.Data;

                    // Join the data into a single string
                    if ((data.Length > 0) && !String.IsNullOrEmpty(newData))
                    {
                        // Trim the charset
                        newData = CSSWrapper.TrimCharset(newData);
                        if (String.IsNullOrEmpty(newData))
                        {
                            continue;
                        }

                        data.AppendLine();
                        data.AppendLine();
                    }

                    data.Append(newData);

                    // Join e-tags
                    if (etag.Length > 0)
                    {
                        etag.Append('|');
                    }
                    // Remove quotes from appended ETag
                    etag.Append(resource.Etag.Trim(new[] { '"' }));

                    // Remember the largest last modified
                    if (resource.LastModified > lastModified)
                    {
                        lastModified = resource.LastModified;
                    }

                    files.AddRange(resource.ComponentFiles);

                    fileName = resource.Name + ".css";
                    if (!string.IsNullOrEmpty(resource.FileName))
                    {
                        fileName = resource.FileName;
                    }
                }

                count++;
            }

            if (!atLeastOne)
            {
                return null;
            }

            // Build the result
            CMSOutputResource result = new CMSOutputResource
            {
                Data = data.ToString(),
                Etag = etag.ToString(),
                LastModified = lastModified,
                ComponentFiles = files,
                Extension = ".css"
            };

            // Set the file name
            result.FileName = (count == 1) ? ValidationHelper.GetSafeFileName(fileName) : "components.css";

            return result;
        }


        /// <summary>
        /// Sends a response containing the requested data.
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        /// <param name="resource">Container with the data to serve</param>
        /// <param name="allowCache">True, if client caching is enabled, otherwise false</param>
        /// <param name="minificationEnabled">True, if the data can be served minified, otherwise false</param>
        /// <param name="clientCacheMinutes">Number of minutes after which the content in the client cache expires</param>
        /// <param name="staticContent">If true, the content is static</param>
        private static void SendResponse(HttpContext context, CMSOutputResource resource, bool allowCache, bool minificationEnabled, int clientCacheMinutes, bool staticContent)
        {
            // Set client cache revalidation
            SetRevalidation(context, staticContent);

            // Let client use data cached in browser if versions match and there was no change in data
            if (allowCache && IsResourceUnchanged(resource))
            {
                context.Response.ContentType = resource.ContentType;
                SendNotModifiedResponse(context, resource.LastModified, resource.Etag, clientCacheMinutes, true);
            }
            else
            {
                byte[] content;

                if (resource.Data == null)
                {
                    // Binary content
                    content = resource.BinaryData;
                }
                else
                {
                    // Text content
                    string contentCoding;
                    content = GetOutputData(resource, minificationEnabled, out contentCoding);

                    if (contentCoding != ContentCodingEnum.IDENTITY)
                    {
                        context.Response.AppendHeader("Content-Encoding", contentCoding);
                        context.Response.AppendHeader("Vary", "Accept-Encoding");
                        RequestContext.ResponseIsCompressed = true;
                    }
                }

                if (content == null)
                {
                    // 404 if no content found
                    RequestHelper.Respond404();
                    return;
                }

                context.Response.AppendHeader("Content-Length", content.Length.ToString());

                // Set client caching
                if (allowCache)
                {
                    SetClientCaching(context, true, resource.LastModified, resource.Etag, clientCacheMinutes);
                }

                context.Response.ContentType = resource.ContentType;

                // Set the file name and extension if defined
                if (!String.IsNullOrEmpty(resource.FileName) && !String.IsNullOrEmpty(resource.Extension))
                {
                    HTTPHelper.SetFileDisposition(resource.FileName, resource.Extension);
                }

                // Do not send output if there's none
                if (content.Length > 0)
                {
                    context.Response.OutputStream.Write(content, 0, content.Length);
                }
            }
        }


        /// <summary>
        /// Send a Not Found response when the requested data was not located successfully.
        /// </summary>
        private static void SendNotFoundResponse()
        {
            if (Throw404WhenNotFound)
            {
                RequestDebug.LogRequestOperation("404NotFound", RequestContext.URL.ToString(), 1);
                RequestHelper.Respond404();
            }
            else
            {
                RequestHelper.EndResponse();
            }
        }


        /// <summary>
        /// Sends the Not Modified response when the data on the client matches those on the server.
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        /// <param name="lastModified">Timestamp for the last modification of the data</param>
        /// <param name="etag">Etag used to identify the resources</param>
        /// <param name="clientCacheMinutes">Number of minutes after which the content in the client cache expires</param>
        /// <param name="publicCache">True, if the data can be cached by cache servers on the way, false if only by requesting client</param>
        private static void SendNotModifiedResponse(HttpContext context, DateTime lastModified, string etag, int clientCacheMinutes, bool publicCache)
        {
            // Set the status to Not modified
            context.Response.StatusCode = (int)HttpStatusCode.NotModified;
            context.Response.Cache.SetETag(etag);
            context.Response.Cache.SetCacheability(publicCache ? HttpCacheability.Public : HttpCacheability.ServerAndPrivate);

            DateTime expires = DateTime.Now.AddMinutes(clientCacheMinutes);

            // No not allow time in future
            if (lastModified >= DateTime.Now)
            {
                lastModified = DateTime.Now.AddSeconds(-1);
            }

            context.Response.Cache.SetLastModified(lastModified);
            context.Response.Cache.SetExpires(expires);

            RequestDebug.LogRequestOperation("304NotModified", etag, 1);
            RequestHelper.EndResponse();
        }


        /// <summary>
        /// Sends the No Content response when there was no data specified in request.
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        private static void SendNoContent(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NoContent;

            RequestDebug.LogRequestOperation("204NoContent", string.Empty, 1);
            RequestHelper.EndResponse();
        }


        /// <summary>
        /// Sets the client cache revalidation.
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        /// <param name="staticContent">If true, the content is static</param>
        private static void SetRevalidation(HttpContext context, bool staticContent)
        {
            if (!staticContent && RevalidateClientCache)
            {
                context.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            }
            else
            {
                context.Response.Cache.SetRevalidation(HttpCacheRevalidation.None);
            }
        }


        /// <summary>
        /// Sets the client caching.
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        /// <param name="allowCache">If true, the caching is allowed</param>
        /// <param name="lastModified">Timestamp for the last modification of the data</param>
        /// <param name="etag">Etag used to identify the resources</param>
        /// <param name="clientCacheMinutes">Number of minutes after which the content in the client cache expires</param>
        private static void SetClientCaching(HttpContext context, bool allowCache, DateTime lastModified, string etag, int clientCacheMinutes)
        {
            DateTime expires = allowCache ? DateTime.Now.AddMinutes(clientCacheMinutes) : DateTime.Now;

            // No not allow time in future
            if (lastModified >= DateTime.Now)
            {
                lastModified = DateTime.Now.AddSeconds(-1);
            }

            context.Response.Cache.SetLastModified(lastModified);
            context.Response.Cache.SetExpires(expires);

            context.Response.Cache.SetETag(etag);

            context.Response.Cache.SetCacheability(HttpCacheability.Public);
        }


        /// <summary>
        /// Checks if resource in the client cache matches the server version.
        /// </summary>
        /// <param name="resource">Resource to check</param>
        /// <returns>true, if resource is unchanged, otherwise false</returns>
        private static bool IsResourceUnchanged(CMSOutputResource resource)
        {
            // Determine the last modified date and etag sent from the browser
            string currentETag = RequestHelper.GetHeader("If-None-Match", string.Empty);
            string ifModified = RequestHelper.GetHeader("If-Modified-Since", string.Empty);

            // If resources match, compare last modification timestamps
            if ((ifModified != string.Empty) && (currentETag == resource.Etag))
            {
                // Get first part of header (colons can delimit additional data)
                DateTime modifiedStamp;
                if (DateTime.TryParse(ifModified.Split(";".ToCharArray())[0], out modifiedStamp))
                {
                    return (resource.LastModified.ToUniversalTime() <= modifiedStamp.ToUniversalTime().AddSeconds(1));
                }
            }

            return false;
        }


        /// <summary>
        /// Checks if this is a revalidation request for a physical file that did not change since the last time.
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        /// <param name="path">Full physical path to the file</param>
        private static void CheckRevalidation(HttpContext context, string path)
        {
            FileInfo fileInfo = FileInfo.New(path);

            if (fileInfo == null)
            {
                return;
            }

            var lastModified = fileInfo.LastWriteTime;

            // Virtual resource, used only to check if revalidation can be short-circuited
            CMSOutputResource fileResource = new CMSOutputResource
            {
                Etag = "file|" + lastModified,
                LastModified = lastModified
            };

            if (IsResourceUnchanged(fileResource))
            {
                SendNotModifiedResponse(context, fileResource.LastModified, fileResource.Etag, PhysicalFilesCacheMinutes, true);
            }
        }


        /// <summary>
        /// Reads a file in the given path.
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="fileExtension">File extension to check against</param>
        /// <returns>Content of the file, or null if it does not exist.</returns>
        private static byte[] ReadBinaryFile(string path, string fileExtension)
        {
            // Return null if file doesn't exist or is not supported
            if (!File.Exists(path) || (Path.GetExtension(path) != fileExtension))
            {
                return null;
            }

            // Try to read the contents of the file
            try
            {
                return File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("GetResourceHandler", "READBINARYFILE", ex);
                return null;
            }
        }


        /// <summary>
        /// Reads a file in the given path.
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="fileExtension">File extension to check against</param>
        /// <returns>Content of the file, or null if it does not exist.</returns>
        private static string ReadFile(string path, string fileExtension)
        {
            // Return null if file doesn't exist or is not supported
            if (!File.Exists(path) || (Path.GetExtension(path) != fileExtension))
            {
                return null;
            }

            // Try to read the contents of the file
            try
            {
                return File.ReadAllText(path);
            }
            catch
            {
                return string.Empty;
            }
        }


        /// <summary>
        /// Compresses a given text.
        /// </summary>
        /// <param name="resource">Text to compress</param>
        /// <returns>Compressed text</returns>
        private static byte[] Compress(string resource)
        {
            byte[] compressedBuffer;

            // Uses in-memory deflate stream to compress the resource
            using (var memory = new System.IO.MemoryStream())
            {
                using (var compressor = new DeflateStream(memory, CompressionMode.Compress))
                {
                    using (var writer = StreamWriter.New(compressor))
                    {
                        writer.Write(resource);
                    }
                }
                compressedBuffer = memory.ToArray();
            }

            return compressedBuffer;
        }


        /// <summary>
        /// Minify supplied source according to settings.
        /// </summary>
        /// <param name="resource">Resource to minify</param>
        /// <param name="minifier">Minifier to use when creating minified version of the data</param>
        /// <param name="minificationEnabled">True, if the data should be minified, otherwise false</param>
        /// <param name="compressionEnabled">True, if data should be compressed, otherwise false</param>
        private static void MinifyResource(CMSOutputResource resource, IResourceMinifier minifier, bool minificationEnabled, bool compressionEnabled)
        {
            if (resource == null)
            {
                return;
            }

            // Set up the settings
            if (minificationEnabled && (minifier != null))
            {
                resource.MinifiedData = minifier.Minify(resource.Data);
            }

            // Check whether the compression is enabled
            if (compressionEnabled && ConnectionHelper.ConnectionAvailable)
            {
                compressionEnabled &= RequestHelper.AllowResourceCompression;
            }

            // Compress
            if (compressionEnabled)
            {
                resource.CompressedData = Compress(resource.Data);
            }

            // Compress and minify
            if (minificationEnabled && compressionEnabled)
            {
                resource.MinifiedCompressedData = Compress(resource.MinifiedData);
            }
        }


        /// <summary>
        /// Returns the data which will be served to client depending on minification and compression settings.
        /// </summary>
        /// <param name="resource">Data container with the data to serve</param>
        /// <param name="minificationEnabled">True, if the data should be minified, otherwise false</param>
        /// <param name="contentCoding">The content coding to use when sending a response</param>
        /// <returns>Data to serve in a form of a byte block</returns>
        private static byte[] GetOutputData(CMSOutputResource resource, bool minificationEnabled, out string contentCoding)
        {
            // minification must be allowed by the server and minified data must be available
            bool minified = minificationEnabled && resource.ContainsMinifiedData;

            // compression must be allowed by server, supported by client and compressed data must be available
            bool allowCompression = false;
            if (ConnectionHelper.ConnectionAvailable)
            {
                allowCompression = RequestHelper.AllowResourceCompression;
            }

            bool compressed = allowCompression && RequestHelper.IsGZipSupported() && resource.ContainsCompressedData;

            // Set default content encoding
            contentCoding = ContentCodingEnum.IDENTITY;

            // Get the proper version of resource to serve based on the settings
            if (!minified && !compressed)
            {
                return Encoding.UTF8.GetBytes(resource.Data);
            }
            else if (minificationEnabled && !compressed)
            {
                return Encoding.UTF8.GetBytes(resource.MinifiedData);
            }
            else if (!minified)
            {
                contentCoding = ContentCodingEnum.DEFLATE;
                return resource.CompressedData;
            }
            else
            {
                contentCoding = ContentCodingEnum.DEFLATE;
                return resource.MinifiedCompressedData;
            }
        }

        #endregion


        #region "Specialized methods"

        /// <summary>
        /// Processes a request for a QR code
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        private static void ProcessQRCodeRequest(HttpContext context)
        {
            // Validate the request hash
            var hashSettings = new HashSettings("")
            {
                Redirect = false
            };

            if (!QueryHelper.ValidateHash("hash", null, hashSettings))
            {
                RequestHelper.Respond403();
            }

            string code = QueryHelper.GetString(QR_CODE_ARGUMENT, string.Empty);

            string correction = QueryHelper.GetString("ec", "M");
            string encoding = QueryHelper.GetString("e", "B");

            int size = QueryHelper.GetInteger("s", 4);
            int version = QueryHelper.GetInteger("v", 4);
            int maxSideSize = QueryHelper.GetInteger("maxsidesize", 0);

            Color fgColor = QueryHelper.GetColor("fc", Color.Black);
            Color bgColor = QueryHelper.GetColor("bc", Color.White);

            CMSOutputResource resource = null;

            // Try to get data from cache (or store them if not found)
            using (var cs = new CachedSection<CMSOutputResource>(ref resource, PhysicalFilesCacheMinutes, true, null, "getresource|qrcode|", code, encoding, size, version, correction, maxSideSize, fgColor, bgColor, RequestContext.IsSSL))
            {
                // Not found in cache; load the data
                if (cs.LoadData)
                {
                    // Prepare the QR code response
                    var settings = new QRCodeSettings(encoding, size, version)
                    {
                        Correction = correction,
                        BgColor = bgColor,
                        FgColor = fgColor
                    };

                    resource = GetQRCode(code, settings, maxSideSize);
                    cs.Data = resource;
                }
            }

            // Send response if there's something to send
            if (resource != null)
            {
                bool allowCache = CacheHelper.AlwaysCacheResources;

                SendResponse(context, resource, allowCache, false, PhysicalFilesCacheMinutes, true);
            }
            else
            {
                SendNotFoundResponse();
            }
        }


        /// <summary>
        /// Processes a request for a general file identified by its URL.
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        private static void ProcessFileRequest(HttpContext context)
        {
            // Process physical file
            ProcessFileRequest(context, FILE_ARGUMENT, FILE_EXTENSION, false);
        }


        /// <summary>
        /// Processes a request for a image file identified by its URL.
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        private static void ProcessImageFileRequest(HttpContext context)
        {
            // Get path to the resource file, resolve it in case it's virtual and map to physical path        
            string path = QueryHelper.GetString(IMAGE_FILE_ARGUMENT, string.Empty);

            // Trim any potential query from path
            int queryIndex = path.IndexOf("?", StringComparison.Ordinal);
            if (queryIndex >= 0)
            {
                path = path.Substring(0, queryIndex);
            }

            if (!path.StartsWith("/", StringComparison.Ordinal) && !path.StartsWith("~/", StringComparison.Ordinal))
            {
                // Map path to the default folder and a real file
                path = StorageHelper.DEFAULT_IMAGES_PATH + path;

                // If file not found in physical folder, try to map the file
                if (!File.ExistsRelative(path))
                {
                    Path.GetMappedPath(ref path);
                }
            }

            bool useCache = QueryHelper.GetString("chset", null) == null;

            // Process physical file
            ProcessPhysicalFileRequest(context, path, IMAGE_FILE_EXTENSION, false, useCache);
        }


        /// <summary>
        /// Processes a request for a JavaScript file identified by its URL.
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        private static void ProcessJSFileRequest(HttpContext context)
        {
            // Check whether to use minification
            bool useMinification = false;
            if (ConnectionHelper.ConnectionAvailable)
            {
                useMinification = ScriptHelper.ScriptMinificationEnabled;
            }

            ProcessFileRequest(context, JS_FILE_ARGUMENT, JS_FILE_EXTENSION, useMinification);
        }


        /// <summary>
        /// Processes a request for a file.
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        /// <param name="queryArgument">Name of the argument whose value specifies the location of the data</param>
        /// <param name="fileExtension">File extension to check against (to prevent serving unauthorized content)</param>
        /// <param name="minificationEnabled">True, if the data should be minified, otherwise false</param>
        private static void ProcessFileRequest(HttpContext context, string queryArgument, string fileExtension, bool minificationEnabled)
        {
            // Get path to the resource file, resolve it in case it's virtual and map to physical path        
            string path = QueryHelper.GetString(queryArgument, string.Empty);

            // Process physical file
            ProcessPhysicalFileRequest(context, path, fileExtension, minificationEnabled, true);
        }


        /// <summary>
        /// Processes a request for a file.
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        /// <param name="path">File path (virtual or absolute)</param>
        /// <param name="fileExtension">File extension to check against (to prevent serving unauthorized content)</param>
        /// <param name="minificationEnabled">True, if the data should be minified, otherwise false</param>
        /// <param name="useCache">If true, cache is allowed to be used</param>
        private static void ProcessPhysicalFileRequest(HttpContext context, string path, string fileExtension, bool minificationEnabled, bool useCache)
        {
            // Get physical path
            string physicalPath = URLHelper.GetPhysicalPath(URLHelper.GetVirtualPath(path));

            // If this is revalidation request, try quick revalidation check before reading the file
            CheckRevalidation(context, physicalPath);

            CMSOutputResource resource = null;

            // Try to get data from cache (or store them if not found)
            using (var cs = new CachedSection<CMSOutputResource>(ref resource, PhysicalFilesCacheMinutes, useCache, null, "getresource", physicalPath, RequestContext.IsSSL))
            {
                // Not found in cache; load the data
                if (cs.LoadData)
                {
                    // Check the image extension
                    switch (fileExtension)
                    {
                        case IMAGE_FILE_EXTENSION:
                        case FILE_EXTENSION:
                            {
                                // Extension sets
                                string fileExt = Path.GetExtension(physicalPath);

                                if ((fileExtension == IMAGE_FILE_EXTENSION) && ImageHelper.IsImage(fileExt))
                                {
                                    // Image file
                                    fileExtension = fileExt;
                                }
                                else if ((fileExtension == FILE_EXTENSION) && mAllowedFileExtensions.Contains(fileExt))
                                {
                                    // General file
                                    fileExtension = fileExt;
                                }

                                resource = GetFile(path, fileExtension, false, true);
                            }
                            break;

                        default:
                            {
                                // Retrieve the file resource, rebase client URLs and wrap it in output container
                                resource = GetFile(path, fileExtension, false, false);

                                var jsMinifier = Service.Resolve<IJavaScriptMinifier>();

                                MinifyResource(resource, jsMinifier, minificationEnabled, true);
                            }
                            break;

                    }

                    // Cache the file
                    if ((resource != null) && cs.Cached)
                    {
                        physicalPath = StorageHelper.GetRealFilePath(physicalPath);

                        if (File.Exists(physicalPath))
                        {
                            cs.CacheDependency = CacheHelper.GetFileCacheDependency(physicalPath);
                        }
                    }

                    cs.Data = resource;
                }
            }

            // Send response if there's something to send
            if (resource != null)
            {
                bool allowCache = CacheHelper.AlwaysCacheResources;

                SendResponse(context, resource, allowCache, minificationEnabled, PhysicalFilesCacheMinutes, true);
            }
            else
            {
                SendNotFoundResponse();
            }
        }


        /// <summary>
        /// Resolves CSS client URLs in CSS text.
        /// </summary>
        /// <param name="inputText">CSS text to resolve.</param>
        /// <param name="baseUrl">Base URL to use when resolving client relative URLs</param>
        private static string ResolveCSSClientUrls(string inputText, string baseUrl)
        {
            if (ValidationHelper.GetBoolean(SettingsHelper.AppSettings[SETTINGS_USE_ABSOLUTE_CSS_CLIENT_URLS], false))
            {
                baseUrl = URLHelper.GetAbsoluteUrl(baseUrl);
            }
            else
            {
                baseUrl = UrlResolver.ResolveUrl(baseUrl);
            }

            return HTMLHelper.ResolveCSSClientUrls(inputText, baseUrl);
        }


        /// <summary>
        /// Retrieves the specified resources and wraps them in an data container.
        /// </summary>
        /// <param name="settings">CSS settings</param>
        /// <param name="cached">If true, the result will be cached</param>
        /// <returns>The data container with the resulting stylesheet data</returns>
        private static CMSOutputResource GetResource(CMSCssSettings settings, bool cached)
        {
            var resources = new List<CMSOutputResource>();

            resources.AddRange(settings.Files.Select(filePath => GetFile(filePath, CSS_FILE_EXTENSION, true, false)));
            resources.AddRange(settings.Stylesheets.Select(stylesheetName => GetStylesheet(stylesheetName, settings.ReturnAsDynamic)));

            resources.AddRange(settings.Containers.Select(GetContainerResource));
            resources.AddRange(settings.WebParts.Select(GetWebPartResource));
            resources.AddRange(settings.Templates.Select(GetTemplateResource));
            resources.AddRange(settings.Layouts.Select(GetLayoutResource));
            resources.AddRange(settings.DeviceLayouts.Select(GetDeviceLayoutResource));
            resources.AddRange(settings.Transformations.Select(GetTransformationResource));
            resources.AddRange(settings.WebPartLayouts.Select(GetWebPartLayoutResource));

            // Combine to a single output
            var result = CombineResources(resources);
            if (result == null)
            {
                return null;
            }

            settings.ComponentFiles.AddRange(result.ComponentFiles);
            result.ContentType = MimeTypeHelper.GetMimetype(CSS_FILE_EXTENSION, "text/css") + "; charset=utf-8";

            // Resolve the macros
            if (CSSWrapper.ResolveMacrosInCSS)
            {
                var context = new MacroSettings
                {
                    TrackCacheDependencies = cached
                };

                if (cached)
                {
                    // Add the default dependencies
                    context.AddCacheDependencies(settings.GetCacheDependencies());
                    context.AddFileCacheDependencies(settings.GetFileCacheDependencies());
                }

                result.Data = MacroResolver.Resolve(result.Data, context);

                if (cached)
                {
                    // Add cache dependency
                    result.CacheDependency = CacheHelper.GetCacheDependency(context.FileCacheDependencies, context.CacheDependencies);
                }
            }
            else if (cached)
            {
                // Only the cache dependency from settings
                result.CacheDependency = settings.GetCacheDependency();
            }

            if (settings.ResolveCSSClientUrls)
            {
                result.Data = ResolveCSSClientUrls(result.Data, "~/CMSPages/GetResource.ashx");
            }

            // Minify
            var cssMinifier = Service.Resolve<ICssMinifier>();

            MinifyResource(result, cssMinifier, CssLinkHelper.StylesheetMinificationEnabled && settings.EnableMinification, settings.EnableCompression);

            return result;
        }


        /// <summary>
        /// Retrieves the stylesheet from file
        /// </summary>
        /// <param name="path">File path (virtual or absolute)</param>
        /// <param name="extension">File extension</param>
        /// <param name="resolveCSSUrls">If true, the CSS URLs are resolved in the output</param>
        /// <param name="binary">If true, the file is a binary file</param>
        /// <returns>The stylesheet data (plain version only), or null if it does not exist.</returns>    
        private static CMSOutputResource GetFile(string path, string extension, bool resolveCSSUrls, bool binary)
        {
            string relativePath = URLHelper.GetVirtualPath(path);
            string physicalPath = URLHelper.GetPhysicalPath(relativePath);

            // Do not allow to get files from outside the web application physical path
            if (!physicalPath.StartsWith(SystemContext.WebApplicationPhysicalPath, StringComparison.InvariantCultureIgnoreCase))
            {
                RequestHelper.Respond404();
            }

            // Get the file content
            string fileContent = null;
            byte[] binaryData = null;

            if (binary)
            {
                // Binary file
                binaryData = ReadBinaryFile(physicalPath, extension);
            }
            else
            {
                // Text file
                fileContent = ReadFile(physicalPath, extension);
                if (resolveCSSUrls)
                {
                    fileContent = ResolveCSSClientUrls(fileContent, relativePath);
                }
            }

            FileInfo fileInfo = FileInfo.New(physicalPath);

            if ((fileInfo == null) || ((binaryData == null) && (fileContent == null)))
            {
                return null;
            }

            var lastModified = fileInfo.LastWriteTime;

            // Build the result
            var resource = new CMSOutputResource
            {
                Data = fileContent,
                BinaryData = binaryData,
                Name = UrlResolver.ResolveUrl(relativePath),
                Etag = "file|" + lastModified,
                LastModified = lastModified,
                FileName = Path.GetFileName(physicalPath),
                Extension = extension
            };

            extension = extension ?? String.Empty;

            switch (extension.ToLowerInvariant())
            {
                case CSS_FILE_EXTENSION:
                    resource.ContentType = "text/css; charset=utf-8";
                    break;

                case JS_FILE_EXTENSION:
                    resource.ContentType = "application/x-javascript";

                    // Resolve macros in js files to support RequireJS config file in file system
                    if (QueryHelper.GetBoolean("resolvemacros", false))
                    {
                        resource.Data = MacroResolver.Resolve(resource.Data);
                    }
                    break;

                default:
                    resource.ContentType = MimeTypeHelper.GetMimetype(extension);
                    break;
            }

            return resource;
        }


        /// <summary>
        /// Generates the QR code
        /// </summary>
        /// <param name="code">Code to generate by the QR code</param>
        /// <param name="settings"></param>
        /// <param name="maxSideSize">Maximum size of the code in pixels, the code will be resized if larger than this size</param>
        private static CMSOutputResource GetQRCode(string code, QRCodeSettings settings, int maxSideSize)
        {
            try
            {
                // Generate the code
                var generator = Service.Resolve<IQRCodeGenerator>();

                var image = generator.GenerateQRCode(code, settings);

                byte[] bytes = ImageHelper.GetBytes(image, ImageFormat.Png);

                // Resize to a proper size
                if (maxSideSize > 0)
                {
                    // Resize the image by image helper to a proper size
                    ImageHelper ih = new ImageHelper(bytes);
                    image = ih.GetResizedImage(maxSideSize);

                    bytes = ImageHelper.GetBytes(image, ImageFormat.Png);
                }

                // Build the result
                var resource = new CMSOutputResource
                {
                    BinaryData = bytes,
                    Name = code,
                    Etag = "QRCode",
                    LastModified = new DateTime(2012, 1, 1),
                    ContentType = MimeTypeHelper.GetMimetype(".png"),
                    FileName = "QRCode.png",
                    Extension = ".png"
                };

                return resource;
            }
            catch (Exception ex)
            {
                // Report too long text
                string message = "[GetResource.GetQRCode]: Failed to generate QR code with text '" + code + "', this may be caused by the text being too long. Original message: " + ex.Message;

                var newEx = new Exception(message, ex);

                EventLogProvider.LogException("GetResource", "QRCODE", newEx);

                throw newEx;
            }
        }


        /// <summary>
        /// Retrieve the stylesheet either from the database or file if checked out.
        /// </summary>
        /// <param name="name">Stylesheet name</param>
        /// <param name="dynamic">Indicates whether stylesheet code is returned in its dynamic nature (if it uses CSS preprocessor)</param>
        /// <returns>The stylesheet data (plain version only)</returns>    
        private static CMSOutputResource GetStylesheet(string name, bool dynamic)
        {
            var stylesheetInfo = CssStylesheetInfoProvider.GetCssStylesheetInfo(name);
            if (stylesheetInfo == null)
            {
                return null;
            }

            // Get last modified date with dependency on external storage
            DateTime lastModified = stylesheetInfo.StylesheetLastModified;
            FileInfo fi = stylesheetInfo.GetFileInfo(CssStylesheetInfo.EXTERNAL_COLUMN_CSS);
            if ((fi != null) && fi.Exists)
            {
                lastModified = fi.LastWriteTime.ToUniversalTime();
            }

            string stylesheetName = stylesheetInfo.StylesheetName;

            string data = !stylesheetInfo.IsPlainCss() && dynamic ? stylesheetInfo.StylesheetDynamicCode : stylesheetInfo.StylesheetText;

            // Build the output
            var resource = new CMSOutputResource
            {
                Data = HTMLHelper.ResolveCSSUrls(data, SystemContext.ApplicationPath),
                Name = stylesheetName,
                LastModified = lastModified,
                Etag = "cssstylesheet|" + stylesheetInfo.StylesheetVersionGUID,
                FileName = stylesheetName + ".css",
                Extension = ".css"
            };

            // Add file dependency on component css file
            if ((fi != null) && fi.Exists)
            {
                resource.ComponentFiles.Add(stylesheetInfo.Generalized.GetVirtualFileRelativePath(CssStylesheetInfo.EXTERNAL_COLUMN_CSS));
            }

            return resource;
        }


        /// <summary>
        /// Retrieves the stylesheets of the web part container from the database.
        /// </summary>
        /// <param name="id">Container ID</param>
        /// <returns>The stylesheet data (plain version only)</returns>
        private static CMSOutputResource GetContainerResource(int id)
        {
            var containerInfo = WebPartContainerInfoProvider.GetWebPartContainerInfo(id);
            if (containerInfo == null)
            {
                return null;
            }

            DateTime lastModified = containerInfo.ContainerLastModified;

            return new CMSOutputResource
            {
                Data = HTMLHelper.ResolveCSSUrls(containerInfo.ContainerCSS, SystemContext.ApplicationPath),
                LastModified = lastModified,
                Etag = "webpartcontainer|" + lastModified,
                FileName = ValidationHelper.GetSafeFileName(containerInfo.ContainerName),
                Extension = ".css"
            };
        }


        /// <summary>
        /// Retrieves the stylesheets of the web part layout from the database.
        /// </summary>
        /// <param name="id">Layout ID</param>
        /// <returns>The stylesheet data (plain version only)</returns>
        private static CMSOutputResource GetWebPartLayoutResource(int id)
        {
            var layoutInfo = WebPartLayoutInfoProvider.GetWebPartLayoutInfo(id);
            if (layoutInfo == null)
            {
                return null;
            }

            // Get last modified date with dependency on external storage
            DateTime lastModified = layoutInfo.WebPartLayoutLastModified;
            FileInfo fi = layoutInfo.GetFileInfo(WebPartLayoutInfo.EXTERNAL_COLUMN_CSS);
            if (fi != null)
            {
                lastModified = fi.LastWriteTime.ToUniversalTime();
            }

            // Build the result
            var resource = new CMSOutputResource
            {
                Data = HTMLHelper.ResolveCSSUrls(layoutInfo.WebPartLayoutCSS, SystemContext.ApplicationPath),
                LastModified = lastModified,
                Etag = "webpartlayout|" + layoutInfo.WebPartLayoutVersionGUID,
                FileName = ValidationHelper.GetSafeFileName(layoutInfo.WebPartLayoutFullName) + ".css",
                Extension = ".css"
            };

            if (fi != null)
            {
                resource.ComponentFiles.Add(layoutInfo.Generalized.GetVirtualFileRelativePath(WebPartLayoutInfo.EXTERNAL_COLUMN_CSS));
            }

            return resource;
        }


        /// <summary>
        /// Retrieves the stylesheets of the page template from the database.
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>The stylesheet data (plain version only)</returns>
        private static CMSOutputResource GetTemplateResource(int id)
        {
            var templateInfo = PageTemplateInfoProvider.GetPageTemplateInfo(id);
            if (templateInfo == null)
            {
                return null;
            }

            // Get last modified date with dependency on external storage
            DateTime lastModified = templateInfo.PageTemplateLastModified;
            FileInfo fi = templateInfo.GetFileInfo(PageTemplateInfo.EXTERNAL_COLUMN_CSS);
            if (fi != null)
            {
                lastModified = fi.LastWriteTime.ToUniversalTime();
            }

            // Build the result
            var resource = new CMSOutputResource
            {
                Data = HTMLHelper.ResolveCSSUrls(templateInfo.PageTemplateCSS, SystemContext.ApplicationPath),
                LastModified = lastModified,
                Etag = "template|" + templateInfo.PageTemplateVersionGUID,
                FileName = ValidationHelper.GetSafeFileName(templateInfo.CodeName),
                Extension = ".css"
            };

            // Add file dependency on component css file
            if (fi != null)
            {
                resource.ComponentFiles.Add(templateInfo.Generalized.GetVirtualFileRelativePath(PageTemplateInfo.EXTERNAL_COLUMN_CSS));
            }

            return resource;
        }


        /// <summary>
        /// Retrieves the stylesheets of the layout from the database.
        /// </summary>
        /// <param name="id">Layout ID</param>
        /// <returns>The stylesheet data (plain version only)</returns>
        private static CMSOutputResource GetLayoutResource(int id)
        {
            var layoutInfo = LayoutInfoProvider.GetLayoutInfo(id);
            if (layoutInfo == null)
            {
                return null;
            }

            // Get last modified date with dependency on external storage
            DateTime lastModified = layoutInfo.LayoutLastModified;
            FileInfo fi = layoutInfo.GetFileInfo(LayoutInfo.EXTERNAL_COLUMN_CSS);
            if (fi != null)
            {
                lastModified = fi.LastWriteTime.ToUniversalTime();
            }

            // Build the result
            var resource = new CMSOutputResource
            {
                Data = HTMLHelper.ResolveCSSUrls(layoutInfo.LayoutCSS, SystemContext.ApplicationPath),
                LastModified = lastModified,
                Etag = "layout|" + layoutInfo.LayoutVersionGUID,
                FileName = ValidationHelper.GetSafeFileName(layoutInfo.LayoutCodeName),
                Extension = ".css"
            };

            // Add file dependency on component css file
            if (fi != null)
            {
                resource.ComponentFiles.Add(layoutInfo.Generalized.GetVirtualFileRelativePath(LayoutInfo.EXTERNAL_COLUMN_CSS));
            }

            return resource;
        }


        /// <summary>
        /// Retrieves the stylesheets of the device layout from the database.
        /// </summary>
        /// <param name="id">Device layout ID</param>
        /// <returns>The stylesheet data (plain version only)</returns>
        private static CMSOutputResource GetDeviceLayoutResource(int id)
        {
            var layoutInfo = PageTemplateDeviceLayoutInfoProvider.GetTemplateDeviceLayoutInfo(id);
            if (layoutInfo == null)
            {
                return null;
            }

            // Get last modified date with dependency on external storage
            DateTime lastModified = layoutInfo.LayoutLastModified;
            FileInfo fi = layoutInfo.GetFileInfo(PageTemplateDeviceLayoutInfo.EXTERNAL_COLUMN_CSS);
            if (fi != null)
            {
                lastModified = fi.LastWriteTime.ToUniversalTime();
            }

            // Build the result
            var resource = new CMSOutputResource
            {
                Data = HTMLHelper.ResolveCSSUrls(layoutInfo.LayoutCSS, SystemContext.ApplicationPath),
                LastModified = lastModified,
                Etag = "layout|" + layoutInfo.LayoutVersionGUID,
                FileName = Convert.ToString(layoutInfo.LayoutID),
                Extension = ".css"
            };

            // Add file dependency on component css file
            if (fi != null)
            {
                resource.ComponentFiles.Add(layoutInfo.Generalized.GetVirtualFileRelativePath(PageTemplateDeviceLayoutInfo.EXTERNAL_COLUMN_CSS));
            }

            return resource;
        }


        /// <summary>
        /// Retrieves the stylesheet of the web part from the database.
        /// </summary>
        /// <param name="id">Web part ID</param>
        /// <returns>The stylesheet data (plain version only)</returns>
        private static CMSOutputResource GetWebPartResource(int id)
        {
            var webPartInfo = WebPartInfoProvider.GetWebPartInfo(id);
            if (webPartInfo == null)
            {
                return null;
            }

            // Build the result
            var resource = new CMSOutputResource
            {
                Data = HTMLHelper.ResolveCSSUrls(webPartInfo.WebPartCSS, SystemContext.ApplicationPath),
                LastModified = webPartInfo.WebPartLastModified,
                Etag = "webpart|" + webPartInfo.WebPartName,
                FileName = ValidationHelper.GetSafeFileName(webPartInfo.WebPartName) + ".css",
                Extension = ".css"
            };

            return resource;
        }


        /// <summary>
        /// Retrieves the stylesheets of the web part layout from the database.
        /// </summary>
        /// <param name="id">Layout ID</param>
        /// <returns>The stylesheet data (plain version only)</returns>
        private static CMSOutputResource GetTransformationResource(int id)
        {
            var transformationInfo = TransformationInfoProvider.GetTransformation(id);
            if (transformationInfo == null)
            {
                return null;
            }

            // Get last modified date with dependency on external storage
            DateTime lastModified = transformationInfo.TransformationLastModified;
            FileInfo fi = transformationInfo.GetFileInfo(TransformationInfo.EXTERNAL_COLUMN_CSS);
            if (fi != null)
            {
                lastModified = fi.LastWriteTime.ToUniversalTime();
            }

            // Build the result
            var resource = new CMSOutputResource
            {
                Data = HTMLHelper.ResolveCSSUrls(transformationInfo.TransformationCSS, SystemContext.ApplicationPath),
                LastModified = lastModified,
                Etag = "transformation|" + transformationInfo.TransformationVersionGUID,
                FileName = ValidationHelper.GetSafeFileName(transformationInfo.TransformationFullName) + ".css",
                Extension = ".css"
            };

            // Add file dependency on component css file
            if (fi != null)
            {
                resource.ComponentFiles.Add(transformationInfo.Generalized.GetVirtualFileRelativePath(TransformationInfo.EXTERNAL_COLUMN_CSS));
            }

            return resource;
        }


        /// <summary>
        /// Processes a request for a JavaScript module file.
        /// </summary>
        /// <remarks>
        /// JavaScript module is identified by the <paramref name="scriptModuleIdentifier"/> parameter that follows the {CMSModuleName}/{JSModuleNamespace}/{JSModuleName}.js pattern.
        /// Namespaces are optional and can be nested. Script modules are loaded from the ~/CMSScripts/CMSModules/{CMSModuleName} folder.
        /// If the module identifier is not valid, the HTTP status code is set to 400 (Bad request).
        /// </remarks>
        /// <param name="context">The HTTP context to process.</param>
        /// <param name="scriptModuleIdentifier">The identifier of the JavaScript module.</param>
        private static void ProcessScriptModuleRequest(HttpContext context, string scriptModuleIdentifier)
        {
            if (string.IsNullOrWhiteSpace(scriptModuleIdentifier))
            {
                RequestHelper.Respond400("The JavaScript module identifier is missing.");
                return;
            }

            Regex regex = RegexHelper.GetRegex(@"^/(?<CmsModuleName>[\w\.]+)/((?<ScriptModuleNamePart>\w+)/)*(?<ScriptModuleNamePart>\w+)\.js$", RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            Match match = regex.Match(scriptModuleIdentifier);
            if (!match.Success)
            {
                RequestHelper.Respond400("Invalid JavaScript module identifier. Please check whether the identifier follows the {CMSModuleName}/{JSModuleNamespace}/{JSModuleName}.js pattern. Namespaces are optional and can be nested.");
                return;
            }

            string cmsModuleName = match.Groups["CmsModuleName"].Value;
            IEnumerable<string> scriptModuleNameParts = match.Groups["ScriptModuleNamePart"].Captures.Cast<Capture>().Select(x => x.Value);

            string scriptModuleVirtualPath = String.Format("~/CMSScripts/CMSModules/{0}/{1}.js", cmsModuleName, String.Join("/", scriptModuleNameParts));

            bool useMinification = ScriptHelper.ScriptMinificationEnabled;
            ProcessPhysicalFileRequest(context, scriptModuleVirtualPath, JS_FILE_EXTENSION, useMinification, true);
        }

        #endregion
    }
}
