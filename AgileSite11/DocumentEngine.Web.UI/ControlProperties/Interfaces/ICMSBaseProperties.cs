using System;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Base properties interface.
    /// </summary>
    public interface ICMSBaseProperties
    {
        /// <summary>
        /// Stop processing.
        /// </summary>
        /// <remarks>
        /// Stop processing. Use in webparts design mode.
        /// </remarks>
        bool StopProcessing
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the cache item the control will use.
        /// </summary>
        /// <remarks>
        /// By setting this name dynamically, you can achieve caching based on URL parameter or some other variable - simply put the value of the parameter to the CacheItemName property. If no value is set, the control stores its content to the item named "URL|ControlID".
        /// </remarks>
        string CacheItemName
        {
            get;
            set;
        }


        /// <summary>
        /// Cache dependencies, each cache dependency on a new line.
        /// </summary>
        string CacheDependencies
        {
            get;
            set;
        }

        /// <summary>
        /// Number of minutes the retrieved content is cached for. Zero indicates that the content will not be cached.
        /// </summary>
        /// <remarks>
        /// This parameter allows you to set up caching of content so that it's not retrieved from the database each time a user requests the page.
        /// </remarks>
        int CacheMinutes
        {
            get;
            set;
        }


        /// <summary>
        /// Property to set and get the SiteName.
        /// </summary>
        string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Property to set and get the WhereCondition.
        /// </summary>
        string WhereCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Property to set and get the OrderBy.
        /// </summary>
        string OrderBy
        {
            get;
            set;
        }


        /// <summary>
        /// Select top N rows.
        /// </summary>
        int TopN
        {
            get;
            set;
        }


        /// <summary>
        /// Columns to select, null or empty returns all columns.
        /// </summary>
        string SelectedColumns
        {
            get;
            set;
        }
    }
}