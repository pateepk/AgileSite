using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Search
{
    /// <summary>
    /// Represents one search result.
    /// </summary>
    public sealed class SearchResultItem : IDataContainer
    {
        private BaseInfo mData;
        private List<string> mColumnNames;


        private readonly Dictionary<string, Func<SearchResultItem, object>> valueGetters = new Dictionary<string, Func<SearchResultItem, object>>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "Id", (item) => item.Id },
            { "Type", (item) => item.Type },
            { "Score", (item) => item.Score },
            { "Position", (item) => item.Position },
            { "AbsScore", (item) => item.AbsScore },
            { "MaxScore", (item) => item.MaxScore },
            { "Index", (item) => item.Index },
            { "Title", (item) => item.Title },
            { "Content", (item) => item.Content },
            { "Created", (item) => item.Created },
            { "Image", (item) => item.Image },
            { "DocumentExtensions", (item) => item.DocumentExtensions }
        };


        private readonly Dictionary<string, Action<SearchResultItem, object>> valueSetters = new Dictionary<string, Action<SearchResultItem, object>>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "Id", (item, value) => item.Id = ValidationHelper.GetString(value, String.Empty) },
            { "Type", (item, value) => item.Type = ValidationHelper.GetString(value, String.Empty) },
            { "Score", (item, value) => item.Score = ValidationHelper.GetDouble(value, 0) },
            { "Position", (item, value) => item.Position = ValidationHelper.GetInteger(value, 0) },
            { "AbsScore", (item, value) => item.AbsScore = ValidationHelper.GetFloat(value, 0) },
            { "MaxScore", (item, value) => item.MaxScore = ValidationHelper.GetFloat(value, 0) },
            { "Index", (item, value) => item.Index = ValidationHelper.GetString(value, String.Empty) },
            { "Title", (item, value) => item.Title = ValidationHelper.GetString(value, String.Empty) },
            { "Content", (item, value) => item.Content = ValidationHelper.GetString(value, String.Empty) },
            { "Created", (item, value) => item.Created = ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME, CultureHelper.EnglishCulture) },
            { "Image", (item, value) => item.Image = ValidationHelper.GetString(value, String.Empty) },
            { "DocumentExtensions", (item, value) => item.DocumentExtensions = ValidationHelper.GetString(value, String.Empty) }
        };


        /// <summary>
        /// Gets <see cref="SearchResult"/> a search item belongs to.
        /// </summary>
        public SearchResult Result
        {
            get;
        }


        /// <summary>
        /// Gets or sets the identifier of a result item.
        /// </summary>
        public string Id
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the type of an object where the search item was found in (i.e. cms.document, cms.customtable).
        /// </summary>
        public string Type
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the score of a result item.
        /// </summary>
        public double Score
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the position of a result item.
        /// </summary>
        public int Position
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the absolute score of a result item.
        /// </summary>
        public float AbsScore
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the maximum score of a result item.
        /// </summary>
        public float MaxScore
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the index name of a result item.
        /// </summary>
        public string Index
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the title of the search item. Contains data from the field configured as 'Title field' in search index configuration.
        /// </summary>
        public string Title
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the content of the search item. Contains data from the field configured as 'Content field' in search index configuration.
        /// </summary>
        public string Content
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the date associated with search item. Contains data from the field configured as 'Date field' in search index configuration.
        /// If no date information found, contains <see cref="System.DateTime.MinValue"/>.
        /// </summary>
        public DateTime Created
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the image of a result item.
        /// </summary>
        public string Image
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the document extensions of a result item.
        /// </summary>
        public string DocumentExtensions
        {
            get;
            set;
        }


        /// <summary>
        /// Info object to get type specific data.
        /// </summary>
        /// <seealso cref="ResultData"/>
        public BaseInfo Data
        {
            get
            {
                return mData ?? (mData = GetDataObject(Id, Type));
            }
        }


        /// <summary>
        /// Gets or sets the source search document of this result item.
        /// </summary>
        public ILuceneSearchDocument SearchDocument
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the raw result data.
        /// </summary>
        public DataRow ResultData
        {
            get;
            set;
        }


        /// <summary>
        /// Gets a list of names of available columns.
        /// </summary>
        List<string> IDataContainer.ColumnNames
        {
            get
            {
                return mColumnNames ?? (mColumnNames = valueGetters.Keys.ToList());
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResultItem"/> class with parent search result set to <paramref name="result"/>.
        /// </summary>
        /// <param name="result">Search result the item belongs to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
        public SearchResultItem(SearchResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Result = result;
        }


        /// <summary>
        /// Returns column value for current search result item. The value is obtained from <see cref="ResultData"/>,
        /// if such column is present, or <see cref="SearchDocument"/>.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetSearchValue(string columnName)
        {
            // Check whether data row exists and contains required column
            var dr = ResultData;
            if ((dr != null) && (dr.Table.Columns.Contains(columnName)))
            {
                return dr[columnName];
            }

            var doc = SearchDocument;
            if (doc != null)
            {
                return doc.Get(columnName.ToLowerInvariant());
            }

            // Return nothing by default
            return null;
        }


        /// <summary>
        /// Returns the relative image URL associated with search item. The image URL is based on data from the field configured as 'Image field' (<see cref="Image"/>) in search index configuration.
        /// </summary>
        /// <returns>An URL associated with this search item.</returns>
        public string GetImageUrl()
        {
            return SearchIndexers.GetIndexer(Type)?.GetSearchImageUrl(this);
        }


        private BaseInfo GetDataObject(string id, string objectType)
        {
            return ModuleManager.GetObject(new LoadDataSettings(ResultData, objectType));
        }


        /// <summary>
        /// Gets or sets the value of a property named <paramref name="columnName"/>.
        /// </summary>
        /// <param name="columnName">Name of column to be get or set.</param>
        object ISimpleDataContainer.this[string columnName]
        {
            get
            {
                return ((ISimpleDataContainer)this).GetValue(columnName);
            }
            set
            {
                ((ISimpleDataContainer)this).SetValue(columnName, value);
            }
        }


        /// <summary>
        /// Returns the value of a property named <paramref name="columnName"/>.
        /// </summary>
        /// <param name="columnName">Name of column whose value is to be returned.</param>
        object ISimpleDataContainer.GetValue(string columnName)
        {
            Func<SearchResultItem, object> getter = null;
            if (valueGetters.TryGetValue(columnName, out getter))
            {
                return getter(this);
            }
            return null;
        }


        /// <summary>
        /// Sets the value of a property named <paramref name="columnName"/>.
        /// </summary>
        /// <param name="columnName">Name of column whose value is to be set.</param>
        /// <param name="value">Value for the column.</param>
        bool ISimpleDataContainer.SetValue(string columnName, object value)
        {
            Action<SearchResultItem, object> setter = null;
            if (valueSetters.TryGetValue(columnName, out setter))
            {
                setter(this, value);

                return true;
            }
            return false;
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present).</returns>
        bool IDataContainer.TryGetValue(string columnName, out object value)
        {
            value = null;
            Func<SearchResultItem, object> getter = null;
            if (valueGetters.TryGetValue(columnName, out getter))
            {
                value = getter(this);

                return true;
            }
            return false;
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        bool IDataContainer.ContainsColumn(string columnName)
        {
            return valueGetters.ContainsKey(columnName);
        }
    }
}