using System;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Query based controls interface definition.
    /// </summary>
    public interface ICMSQueryProperties : ICMSBaseProperties
    {
        /// <summary>
        /// Query name in format application.class.query.
        /// </summary>
        string QueryName
        {
            get;
            set;
        }


        /// <summary>
        /// Number of items per page.
        /// </summary>
        int PageSize
        {
            get;
            set;
        }


        /// <summary>
        /// Select top N rows.
        /// </summary>
        int SelectTopN
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates whether cache minutes must be set manually (Cache minutes value is independent on view mode and cache settings)
        /// </summary>
        bool ForceCacheMinutes
        {
            get;
            set;
        }
    }
}