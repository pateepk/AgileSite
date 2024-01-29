using System;
using System.Collections.Generic;

using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.Localization
{
    using ResourcesDictionary = StringSafeDictionary<string>;

    /// <summary>
    /// String resource manager for use with the .resx files.
    /// </summary>
    public class FileResourceManager
    {
        #region "Variables"

        /// <summary>
        /// If true, all resource strings are loaded on the Resource manager initialization.
        /// </summary>
        private static readonly BoolAppSetting mLoadAllStrings = new BoolAppSetting("CMSLoadAllResourceStrings", true);


        /// <summary>
        /// True if all readers are completed.
        /// </summary>
        private bool mAllReadersCompleted;


        /// <summary>
        /// Contains resources pre-loaded in memory.
        /// </summary>
        private ResourcesDictionary mResources;


        /// <summary>
        /// Array of the resource readers.
        /// </summary>
        private readonly List<FileResourceReader> mReaders = new List<FileResourceReader>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Loaded resources
        /// </summary>
        public virtual ResourcesDictionary Resources
        {
            get
            {
                return LockHelper.Ensure(ref mResources, LoadStrings, this);
            }
        }


        /// <summary>
        /// Resource files.
        /// </summary>
        public virtual string[] ResourceFiles
        {
            get;
            private set;
        }


        /// <summary>
        /// Manager culture.
        /// </summary>
        public virtual string Culture
        {
            get;
            private set;
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor
        /// </summary>
        public FileResourceManager()
            : this("", "")
        {
        }


        /// <summary>
        /// Constructor, creates a new instance of the FileResourceManager object.
        /// </summary>
        /// <param name="resourceFile">Resource file to use</param>
        /// <param name="culture">Culture of resource file</param>
        public FileResourceManager(string resourceFile, string culture)
        {
            if (culture == null)
            {
                return;
            }

            ResourceFiles = resourceFile.Split(new [] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            Culture = culture;
        }


        /// <summary>
        /// Loads the resource strings
        /// </summary>
        private ResourcesDictionary LoadStrings()
        {
            // Load the files
            var files = ResourceFiles;
            if (files != null)
            {
                if (mLoadAllStrings)
                {
                    // Load all the strings
                    return LoadStrings(files, Culture);
                }
                else
                {
                    // Load only the readers for later use
                    LoadFiles(files, Culture);
                }
            }

            return new ResourcesDictionary();
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns resource file relative path based on given culture.
        /// </summary>
        /// <param name="uiCulture">Culture for resource file; default UI culture resource file is returned if null or empty</param>
        public static string GetResFilename(string uiCulture)
        {
            if (uiCulture == null || uiCulture.EqualsCSafe(ResourceStringInfoProvider.DefaultUICulture, StringComparison.InvariantCultureIgnoreCase))
            {
                uiCulture = String.Empty;
            }
            else
            {
                uiCulture += ".";
            }

            return String.Format("~\\CMSResources\\CMS.{0}resx", uiCulture);
        }


        /// <summary>
        /// Returns the resource string of currently loaded culture/language.
        /// </summary>
        /// <param name="key">Resource string key</param>
        public virtual string GetString(string key)
        {
            if (String.IsNullOrEmpty(key))
            {
                return null;
            }

            string result = Resources[key];

            if ((result == null) && !mAllReadersCompleted && !mLoadAllStrings)
            {
                // Try to get from the resource file
                result = ReadString(key);
            }

            return result;
        }


        /// <summary>
        /// Sets the resource string of currently loaded culture/language.
        /// </summary>
        /// <param name="key">Resource string key</param>
        /// <param name="value">Resource string value</param>
        public virtual void SetString(string key, string value)
        {
            if (key == null)
            {
                return;
            }
            
            Resources[key] = value;
        }


        /// <summary>
        /// Remove resource string from currently loaded culture.
        /// </summary>
        public virtual void DeleteString(string key)
        {
            if (key == null)
            {
                return;
            }

            Resources.Remove(key);
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Loads the resource file from the specified path.
        /// </summary>
        /// <param name="resourceFiles">Files to load</param>
        /// <param name="culture">File culture</param>
        protected void LoadFiles(string[] resourceFiles, string culture)
        {
            // Load all files as resource readers
            foreach (string file in resourceFiles)
            {
                if ((file.Trim() != String.Empty) && File.Exists(file))
                {
                    var reader = new FileResourceReader(file);

                    mReaders.Add(reader);
                }
            }
        }


        /// <summary>
        /// Loads the resource file from the specified path.
        /// </summary>
        /// <param name="resourceFiles">Files to load</param>
        /// <param name="culture">File culture</param>
        protected ResourcesDictionary LoadStrings(string[] resourceFiles, string culture)
        {
            var resources = new ResourcesDictionary();

            foreach (string file in resourceFiles)
            {
                // Load resource
                if ((file.Trim() != String.Empty) && File.Exists(file))
                {
                    var reader = new FileResourceReader(file);

                    string name = null;
                    string value = null;

                    // Get next string
                    while (reader.GetNextString(ref name, ref value))
                    {
                        resources[name] = value;
                    }

                    mAllReadersCompleted = true;
                }
            }

            return resources;
        }


        /// <summary>
        /// Reads the string from the resource readers.
        /// </summary>
        /// <param name="key">String key</param>
        protected string ReadString(string key)
        {
            if (mAllReadersCompleted)
            {
                return null;
            }

            bool allCompleted = true;

            var resources = Resources;

            // Go through all readers
            foreach (var reader in mReaders)
            {
                if (!reader.IsCompleted)
                {
                    allCompleted = false;

                    string name = null;
                    string value = null;

                    // Get next string
                    while (reader.GetNextString(ref name, ref value))
                    {
                        resources[name] = value;
                        
                        // If matches with requested key, return the value
                        if (name.EqualsCSafe(key, true))
                        {
                            return value;
                        }
                    }
                }
            }

            mAllReadersCompleted = allCompleted;

            return null;
        }

        #endregion
    }
}