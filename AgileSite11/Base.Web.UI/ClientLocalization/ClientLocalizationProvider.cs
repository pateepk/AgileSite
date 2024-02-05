using System;
using System.Collections.Generic;
using System.ComponentModel;

using CMS.Core;
using CMS.Helpers;
using CMS.IO;

using Newtonsoft.Json;

namespace CMS.Base.Web.UI.Internal
{
    /// <summary>
    /// Provides method for obtaining all the localized strings required by given module.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ClientLocalizationProvider : IClientLocalizationProvider
    {
        private const string MODULES_PATH = @"CMSScripts\CMSModules";
        private const string LOCALIZATION_EXTENSION = "localization.json";
        private readonly ILocalizationService mLocalizationService;
        private readonly AbstractStorageProvider mAbstractStorageProvider;
        

        /// <summary>
        /// Instantiates new instance of <see cref="ClientLocalizationProvider" />.
        /// </summary>
        /// <param name="localizationService">Provides method for string localization</param>
        public ClientLocalizationProvider(ILocalizationService localizationService)
            : this(localizationService, AbstractStorageProvider.DefaultProvider)
        {
            mLocalizationService = localizationService;
        }


        /// <summary>
        /// Instantiates new instance of <see cref="ClientLocalizationProvider"/>.
        /// </summary>
        /// <param name="localizationService">Provides method for string localization</param>
        /// <param name="abstractStorageProvider">Abstraction of the file storage</param>
        internal ClientLocalizationProvider(ILocalizationService localizationService, AbstractStorageProvider abstractStorageProvider)
        {
            mLocalizationService = localizationService;
            mAbstractStorageProvider = abstractStorageProvider;
        }


        /// <summary>
        /// Resolves all localization string for the given <paramref name="moduleName" />. This method assumes there exists
        /// localization file in the module folder.
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <exception cref="ArgumentException"><paramref name="moduleName" /> is <c>null</c> or empty</exception>
        /// <returns>Collection of all strings obtained from the localization file localized to the current preferred UI culture</returns>
        public IDictionary<string, string> GetClientLocalization(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                throw new ArgumentException("Argument is null or empty", nameof(moduleName));
            }
            
            if (SystemContext.DevelopmentMode)
            {
                return BuildResourceDictionary(moduleName);
            }

            return CacheHelper.Cache(
                () => BuildResourceDictionary(moduleName), 
                new CacheSettings(1440, "ClientLocalizationProvider", moduleName, CacheHelper.GetCultureCacheKey(CultureHelper.PreferredUICultureCode))
            );
        }


        /// <summary>
        /// Builds resource dictionary containing all the resource strings and their translations for the given <paramref name="moduleName"/>.
        /// </summary>
        /// <param name="moduleName">Name of the module used for the resource file identification</param>
        /// <returns>Resource dictionary containing all the resource strings and their translations</returns>
        private Dictionary<string, string> BuildResourceDictionary(string moduleName)
        {
            var localizationFilePath = GetLocalizationFilePath(moduleName);
            var result = new Dictionary<string, string>();

            using (var streamReader = mAbstractStorageProvider.FileProviderObject.OpenText(localizationFilePath))
            {
                foreach (var resourceString in JsonConvert.DeserializeObject<string[]>(streamReader.ReadToEnd()))
                {
                    if (!result.ContainsKey(resourceString))
                    {
                        result.Add(resourceString, mLocalizationService.GetString(resourceString, CultureHelper.PreferredUICultureCode));
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Gets path of the localization file specified for the given <paramref name="moduleName" />.
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <returns>Location of the localization file</returns>
        private string GetLocalizationFilePath(string moduleName)
        {
            return $"{Path.Combine(SystemContext.WebApplicationPhysicalPath, MODULES_PATH, moduleName)}.{LOCALIZATION_EXTENSION}";
        }
    }
}