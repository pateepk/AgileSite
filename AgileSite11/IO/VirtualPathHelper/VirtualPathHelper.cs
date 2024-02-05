using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

using CMS.Core;
using CMS.Base;

namespace CMS.IO
{
    /// <summary>
    /// Virtual path provider helper.
    /// </summary>
    public class VirtualPathHelper
    {
        #region "Delegates"

        /// <summary>
        /// Delegate used for registered path callback method
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        public delegate IVirtualFileObject GetVirtualFileObjectHandler(string virtualPath);

        #endregion


        #region "Variables"

        // Collection of registered virtual paths
        private static readonly SortedList<string, GetVirtualFileObjectHandler> registeredPaths = new SortedList<string, GetVirtualFileObjectHandler>(StringComparer.OrdinalIgnoreCase);
        
        // List of physical folders processed by virtual path provider
        private static readonly SortedList<string, bool> managedCompilationPaths = new SortedList<string, bool>(StringComparer.OrdinalIgnoreCase);

        // Provider object instance.
        private static IVirtualPathProvider mProviderObject;
        
        // Provider assembly name.
        private static string mProviderAssemblyName;
        
        // Locker object
        private static readonly object lockObject = new object();
        
        // Indicates if the virtual path provider was already registered
        private static bool? mVirtualPathProviderRegistered;

        // URL parameter separator
        private static string mURLParametersSeparator = "---";
        
        // If true, the system is using virtual path provider.

        /// <summary>
        /// Version guid prefix used in control path
        /// </summary>
        public const string VERSION_GUID_PREFIX = "=vg=";

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether system is using virtual path provider.
        /// </summary>
        public static bool UsingVirtualPathProvider
        {
            get;
            set;
        }


        /// <summary>
        /// URL parameters separator.
        /// </summary>
        public static string URLParametersSeparator
        {
            get
            {
                return mURLParametersSeparator;
            }
            set
            {
                mURLParametersSeparator = value;
            }
        }


        /// <summary>
        /// Provider library assembly.
        /// </summary>
        internal static string ProviderAssemblyName
        {
            get
            {
                if (mProviderAssemblyName == null)
                {
                    string assemblyName = Convert.ToString(CoreServices.AppSettings["CMSVirtualPathProviderAssembly"]);

                    if (!String.IsNullOrEmpty(assemblyName))
                    {
                        mProviderAssemblyName = assemblyName;
                    }
                    else
                    {
                        mProviderAssemblyName = "CMS.VirtualPathProvider";
                    }
                }
                return mProviderAssemblyName;
            }
            set
            {
                mProviderAssemblyName = value;
            }
        }


        /// <summary>
        /// Returns the helper object.
        /// </summary>
        public static IVirtualPathProvider ProviderObject
        {
            get
            {
                if (mProviderObject == null)
                {
                    // Get the helper object
                    string fullName = ProviderAssemblyName + ".DbPathProvider";

                    mProviderObject = ClassHelper.GetClass<IVirtualPathProvider>(ProviderAssemblyName, fullName);
                }

                return mProviderObject;
            }
            internal set
            {
                mProviderObject = value;
            } 
        }

        #endregion


        #region "Virtual path methods"

        /// <summary>
        /// Register callback method for specified virtual path. 
        /// </summary>
        /// <remarks>Does not register path if virtual path provider is not correctly registered</remarks>
        /// <param name="relativePath">Relative path</param>
        /// <param name="getObjectCallback">GetVirtualFileObjectHandler callback method</param>
        /// <returns>Returns true if path was registered successfully</returns>
        public static bool RegisterVirtualPath(string relativePath, GetVirtualFileObjectHandler getObjectCallback)
        {
            GetVirtualFileObjectHandler fakeHandler = null;
            return RegisterVirtualPath(relativePath, getObjectCallback, false, ref fakeHandler);
        }


        /// <summary>
        /// Register callback method for specified virtual path
        /// </summary>
        /// <remarks>Does not register path if virtual path provider is not correctly registered</remarks>
        /// <param name="relativePath">Relative path</param>
        /// <param name="getObjectCallback">GetVirtualFileObjectHandler callback method</param>
        /// <param name="overrideExisting">Indicates whether current path should override existing</param>
        /// <param name="existingCallbackMethod">Returns existing callback method if current override the path registration</param>
        /// <returns>Returns true if path was registered successfully</returns>
        public static bool RegisterVirtualPath(string relativePath, GetVirtualFileObjectHandler getObjectCallback, bool overrideExisting, ref GetVirtualFileObjectHandler existingCallbackMethod)
        {
            if (!RegisterVirtualPathProvider())
            {
                // Do not register virtual path if virtual path provider is not registered
                return false;
            }

            return RegisterVirtualPathInternal(relativePath, getObjectCallback, overrideExisting, ref existingCallbackMethod);
        }


        /// <summary>
        /// Register callback method for specified virtual path
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <param name="getObjectCallback">GetVirtualFileObjectHandler callback method</param>
        /// <param name="overrideExisting">Indicates whether current path should override existing</param>
        /// <param name="existingCallbackMethod">Returns existing callback method if current override the path registration</param>
        /// <returns>Returns true if path was registered successfully</returns>
        internal static bool RegisterVirtualPathInternal(string relativePath, GetVirtualFileObjectHandler getObjectCallback, bool overrideExisting, ref GetVirtualFileObjectHandler existingCallbackMethod)
        {
            if (registeredPaths.ContainsKey(relativePath))
            {
                if (overrideExisting)
                {
                    existingCallbackMethod = registeredPaths[relativePath];
                    registeredPaths.Remove(relativePath);
                }
                else
                {
                    return false;
                }
            }

            registeredPaths.Add(relativePath, getObjectCallback);
            return true;
        }


        /// <summary>
        /// Returns IVirtualFileObject for specified virtual path
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        public static IVirtualFileObject GetVirtualFileObject(string virtualPath)
        {
            virtualPath = VirtualPathUtility.ToAppRelative(virtualPath, SystemContext.ApplicationPath);

            // Loop in reverse order, longer path have higher priority
            for (int i = registeredPaths.Count - 1; i >= 0; --i)
            {
                if (virtualPath.StartsWith(registeredPaths.Keys[i], StringComparison.OrdinalIgnoreCase))
                {
                    IVirtualFileObject virtFile = registeredPaths.Values[i].Invoke(virtualPath);
                    if (virtFile != null)
                    {
                        return virtFile;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Returns virtual relative for required parameters
        /// </summary>
        /// <param name="codeName">Object code name</param>
        /// <param name="extension">Required extension starting with dot character</param>
        /// <param name="directory">Starting directory</param>
        /// <param name="prefix">Optional codename prefix. Prefix is delimited by "=" directory.</param>
        /// <param name="versionGuid">Version GUID value</param>
        public static string GetVirtualFileRelativePath(string codeName, string extension, string directory, string prefix, string versionGuid)
        {
            if (string.IsNullOrEmpty(codeName))
            {
                return null;
            }

            if (!String.IsNullOrEmpty(versionGuid))
            {
                versionGuid = VERSION_GUID_PREFIX + versionGuid + "/";
            }

            if (!String.IsNullOrEmpty(prefix))
            {
                prefix += "/";
            }

            // Build URL
            string url = directory + "/" + versionGuid + prefix + codeName + extension;

            // Resolve the URL
            if (url.StartsWith("~/", StringComparison.Ordinal))
            {
                return SystemContext.ApplicationPath.TrimEnd('/') + url.TrimStart('~');
            }

            return url;
        }


        /// <summary>
        /// Returns virtual object name and prefixes if available
        /// </summary>
        /// <param name="relativeUrl">Current relative URL</param>
        /// <param name="relatedDirPath">Required relative directory path</param>
        /// <param name="prefixes">List of prefixes</param>
        public static string GetVirtualObjectName(string relativeUrl, string relatedDirPath, ref List<string> prefixes)
        {
            // gets the path behind specified directory
            string url = relativeUrl.Remove(0, relatedDirPath.Length + 1);

            // Remove version GUID if is in path
            if (url.StartsWith(VERSION_GUID_PREFIX, StringComparison.Ordinal))
            {
                url = RemoveFirstPart(url, "/");
            }

            // Try get prefixes
            string[] paths = url.Split('/');
            for (int i = 0; i < paths.Length - 1; i++)
            {
                if (prefixes == null)
                {
                    prefixes = new List<string>();
                }

                prefixes.Add(paths[i].TrimStart('.'));
            }

            // Set code name
            url = paths[paths.Length - 1];

            // return file name from the specified path without extension
            return Path.GetFileNameWithoutExtension(url);
        }

        #endregion


        #region "Register/Init Methods"

        /// <summary>
        /// Creates the virtual path provider object and registers the object within the system.
        /// </summary>
        public static bool RegisterVirtualPathProvider()
        {
            if (mVirtualPathProviderRegistered.HasValue)
            {
                return mVirtualPathProviderRegistered.Value;
            }

            if (!CoreServices.AppSettings["CMSUseVirtualPathProvider"].ToBoolean(true))
            {
                return false;
            }

            lock (lockObject)
            {
                if (mVirtualPathProviderRegistered.HasValue)
                {
                    return mVirtualPathProviderRegistered.Value;
                }

                try
                {
                    var provider = ProviderObject;
                    if (provider != null)
                    {
                        // Register the provider if exists
                        provider.Register();

                        mVirtualPathProviderRegistered = true;
                    }
                    else
                    {
                        mVirtualPathProviderRegistered = false;
                    }
                }
                catch (Exception)
                {
                    mVirtualPathProviderRegistered = false;
                }
            }

            return mVirtualPathProviderRegistered.Value;
        }

        #endregion


        #region "Physical directory methods"

        /// <summary>
        /// Registers path to the part of the system which should be managed for single file or batch compilation
        /// </summary>
        /// <remarks>Does not register path if virtual path provider is not correctly registered</remarks>
        /// <param name="relativePath">Relative path of the folder</param>
        /// <param name="provideFolderContent">If true, the folder content is provided for this path to allow batch compilation</param>
        public static void RegisterManagedCompilationPath(string relativePath, bool provideFolderContent = false)
        {
            if (!RegisterVirtualPathProvider())
            {
                return;
            }

            RegisterManagerCompilationPathInternal(relativePath, provideFolderContent);
        }


        /// <summary>
        /// Registers path to the part of the system which should be managed for single file or batch compilation
        /// </summary>
        /// <param name="relativePath">Relative path of the folder</param>
        /// <param name="provideFolderContent">If true, the folder content is provided for this path to allow batch compilation</param>
        internal static void RegisterManagerCompilationPathInternal(string relativePath, bool provideFolderContent)
        {
            if (!managedCompilationPaths.ContainsKey(relativePath))
            {
                managedCompilationPaths.Add(relativePath, provideFolderContent);
            }
        }


        /// <summary>
        /// Returns true if current path is registered for single file compilation
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        public static bool IsSingleFileCompilationPath(string virtualPath)
        {
            // Get relative path
            string relativePath = VirtualPathUtility.ToAppRelative(virtualPath, SystemContext.ApplicationPath);
            // Loop in reverse order, longer path have higher priority
            for (int i = managedCompilationPaths.Count - 1; i >= 0; --i)
            {
                if (relativePath.StartsWith(managedCompilationPaths.Keys[i], StringComparison.OrdinalIgnoreCase))
                {
                    // Exclude */App_* folders, e.g: /App_LocalResources etc.
                    if (virtualPath.IndexOf("/app_", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return false;
                    }

                    return !managedCompilationPaths.Values[i];
                }
            }
            return false;
        }

        #endregion


        #region "General helper methods"

        /// <summary>
        /// Returns the string with first selector removed.
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="delimiter">Selector delimiter</param>
        internal static string RemoveFirstPart(string expression, string delimiter)
        {
            // Get the index of delimiter
            int delimIndex = expression.IndexOf(delimiter, StringComparison.Ordinal);
            if (delimIndex < 0)
            {
                // No delimiter, delete all
                return "";
            }
            else
            {
                // Remove the part before the delimiter and the delimiter itself
                return expression.Substring(delimIndex + delimiter.Length);
            }
        }


        /// <summary>
        /// Removes the default layout directives from the layout code.
        /// </summary>
        /// <param name="code">Code to process</param>
        /// <param name="defaultDirectives">Default directives to be removed</param>
        public static string RemoveDirectives(string code, string[] defaultDirectives)
        {
            string line;

            StringBuilder sb = new StringBuilder();
            StringReader sr = StringReader.New(code);
            while ((line = sr.ReadLine()) != null)
            {
                bool skip = false;
                foreach (var item in defaultDirectives)
                {
                    if (line.Equals(item, StringComparison.InvariantCultureIgnoreCase))
                    {
                        skip = true;
                        break;
                    }
                }
                if (!skip)
                {
                    sb.AppendLine(line);
                }
            }

            return sb.ToString().TrimEnd(new[] { '\r', '\n' });
        }

        #endregion
    }
}