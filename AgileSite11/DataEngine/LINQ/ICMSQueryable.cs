using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for queryable CMS objects
    /// </summary>
    public interface ICMSQueryable<out TObject> : ICMSQueryable, IOrderedQueryable<TObject>
    {
        /// <summary>
        /// Creates the child collection based on the given provider
        /// </summary>
        /// <param name="settings">Query parameters</param>
        ICMSQueryable<TObject> CreateChild(IDataQuerySettings settings);
    }


    /// <summary>
    /// Interface for queryable CMS objects
    /// </summary>
    public interface ICMSQueryable
    {
        /// <summary>
        /// Number of total items in the collection
        /// </summary>
        int Count
        {
            get;
        }


        /// <summary>
        /// Returns true if the given collection is offline (disconnected from the database)
        /// </summary>
        bool IsOffline
        {
            get;
        }


        /// <summary>
        /// Columns list of SQL order by expression.
        /// </summary>
        string OrderByColumns
        {
            get;
        }
    }
}
