using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Collection of the provider dictionaries.
    /// </summary>
    public class ProviderDictionaryCollection
    {
        private readonly Action<ProviderDictionaryCollection, object> loadCallback;

        private readonly object parameter;


        /// <summary>
        /// Default loading type of the items.
        /// </summary>
        internal LoadHashtableEnum LoadingType
        {
            get;
            private set;
        }


        /// <summary>
        /// Dictionary name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// ID dictionary.
        /// </summary>
        public ProviderInfoDictionary<int> ById
        {
            get;
            set;
        }


        /// <summary>
        /// CodeName dictionary.
        /// </summary>
        public ProviderInfoDictionary<string> ByCodeName
        {
            get;
            set;
        }


        /// <summary>
        /// FullName dictionary.
        /// </summary>
        public ProviderInfoDictionary<string> ByFullName
        {
            get;
            set;
        }


        /// <summary>
        /// Guid dictionary.
        /// </summary>
        public ProviderInfoDictionary<Guid> ByGuid
        {
            get;
            set;
        }


        /// <summary>
        /// Guid and site dictionary.
        /// </summary>
        public ProviderInfoDictionary<string> ByGuidAndSite
        {
            get;
            set;
        }


        /// <summary>
        /// String values dictionary.
        /// </summary>
        public ProviderDictionary<string, string> StringValues
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="loadingType">Loading type</param>
        /// <param name="loadCallback">Callback function with parameter to load the objects</param>
        /// <param name="parameter">Additional parameter passed to the <paramref name="loadCallback"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="objectType"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="loadCallback"/></exception>
        public ProviderDictionaryCollection(string objectType, LoadHashtableEnum loadingType, Action<ProviderDictionaryCollection, object> loadCallback, object parameter = null)
        {
            if (String.IsNullOrEmpty(objectType))
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            if (loadCallback == null)
            {
                throw new ArgumentNullException(nameof(loadCallback));
            }

            Name = objectType;
            this.parameter = parameter;
            LoadingType = loadingType;
            this.loadCallback = loadCallback;
        }


        /// <summary>
        /// Clears all the items.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        public void Clear(bool logTasks)
        {
            ById?.Clear(logTasks);
            ByCodeName?.Clear(logTasks);
            ByFullName?.Clear(logTasks);
            ByGuid?.Clear(logTasks);
            ByGuidAndSite?.Clear(logTasks);
            StringValues?.Clear(logTasks);
        }


        /// <summary>
        /// Loads the default items of the dictionaries.
        /// </summary>
        public void LoadDefaultItems()
        {
            if (LoadingType == LoadHashtableEnum.All)
            {
                loadCallback(this, parameter);
            }
        }


        /// <summary>
        /// Ensures loading of all the items. Keeps the original loading type.
        /// </summary>
        public void LoadAll()
        {
            LoadHashtableEnum originalValue = LoadHashtableEnum.None;
            try
            {
                originalValue = LoadingType;
                LoadingType = LoadHashtableEnum.All;
                loadCallback(this, parameter);
            }
            finally
            {
                LoadingType = originalValue;
            }
        }
    }
}