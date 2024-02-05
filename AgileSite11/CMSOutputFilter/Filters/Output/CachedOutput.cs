using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Security;

using CMS.Helpers;
using CMS.Base;
using CMS.DocumentEngine;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Cached output container.
    /// </summary>
    [Serializable]
    public class CachedOutput : IDataContainer, ISerializable
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets the A/B or Multivariate test cookie name
        /// </summary>
        public string TestCookieName
        {
            get;
            set;
        }


        /// <summary>
        /// Output data.
        /// </summary>
        public OutputData OutputData
        {
            get;
            set;
        }


        /// <summary>
        /// Document page info.
        /// </summary>
        public PageInfo CachePageInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Alias path.
        /// </summary>
        public string AliasPath
        {
            get;
            set;
        }


        /// <summary>
        /// Original alias path.
        /// </summary>
        public string OriginalAliasPath
        {
            get;
            set;
        }


        /// <summary>
        /// URL rewriting status.
        /// </summary>
        public RequestStatusEnum Status
        {
            get;
            set;
        }


        /// <summary>
        /// Headers collection.
        /// </summary>
        [XmlIgnore]
        public NameValueCollection Headers
        {
            get;
            set;
        }


        /// <summary>
        /// Document conversion name - Reflects the "DocumentTrackConversionName" data column.
        /// </summary>
        public string DocumentTrackConversionName
        {
            get;
            set;
        }


        /// <summary>
        /// Document conversion value - Reflects the "DocumentConversionValue" data column.
        /// </summary>
        public string DocumentConversionValue
        {
            get;
            set;
        }


        /// <summary>
        /// Site name
        /// </summary>
        public string SiteName 
        {
            get; 
            set; 
        }


        /// <summary>
        /// Gets or sets the HTTP status code of the output returned to the client.
        /// </summary>
        public int HttpStatusCode
        {
            get;
            set;
        }

        #endregion


        #region "Cache properties"

        /// <summary>
        /// Cache key
        /// </summary>
        public string CacheKey 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Cache minutes
        /// </summary>
        public int CacheMinutes 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Cache item expiration
        /// </summary>
        public DateTime Expiration 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Cache in file system
        /// </summary>
        public bool CacheInFileSystem 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// List of the item cache dependencies
        /// </summary>
        public CacheDependencyList CacheDependencies 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// URL address of cached page - contains final URL address which reflects rewrite settings
        /// </summary>
        public string URL
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// CMSCachedOutput constructor
        /// </summary>
        public CachedOutput()
        {
        }


        /// <summary>
        /// Copies the cache settings to the target object
        /// </summary>
        /// <param name="target">Target object</param>
        public void CopyCacheSettingsTo(CachedOutput target)
        {
            target.SiteName = SiteName;
            target.CacheMinutes = CacheMinutes;
            target.CacheKey = CacheKey;
            target.Expiration = Expiration;
            target.CacheInFileSystem = CacheInFileSystem;
            target.URL = URL;
        }

        #endregion


        #region "IDataContainer Members"

        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                SetValue(columnName, value);
            }
        }


        /// <summary>
        /// Returns the column names.
        /// </summary>
        [XmlIgnore]
        public List<string> ColumnNames
        {
            get
            {
                return new List<string>
                    {
                        "AliasPath",
                        "Status",
                        "HasSubstitutions",
                        "Html",
                        "GZip"
                    };
            }
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public bool TryGetValue(string columnName, out object value)
        {
            switch (columnName.ToLowerInvariant())
            {
                case "aliaspath":
                    value = AliasPath;
                    return true;

                case "status":
                    value = Status;
                    return true;

                case "hassubstitutions":
                    value = OutputData.HasSubstitutions;
                    return true;

                case "html":
                    value = OutputData.Html;
                    return true;

                case "gzip":
                    value = OutputData.GZip;
                    return true;
            }

            value = null;
            return false;
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            object value;
            TryGetValue(columnName, out value);

            return value;
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public bool SetValue(string columnName, object value)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region "ISerializable Members"
        
        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization inf</param>
        /// <param name="context">Streaming context</param>
        public CachedOutput(SerializationInfo info, StreamingContext context)
        {
            CachePageInfo = (PageInfo)info.GetValue("CachePageInfo", typeof(PageInfo));
        }


        /// <summary>
        /// Object serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("CachePageInfo", CachePageInfo);
        }

        #endregion
    }
}