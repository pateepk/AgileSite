using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides SQL hints
    /// </summary>
    public class SqlHints
    {
        /// <summary>
        /// Specifies that any indexed views are not expanded to access underlying tables when the query optimizer processes the query. The query optimizer treats the view like a table with clustered index. NOEXPAND applies only to indexed views.
        /// </summary>
        public const string NOEXPAND = "NOEXPAND";

        
        /// <summary>
        /// Specifies that the query can read the uncommitted data and ignores the locks.
        /// </summary>
        public const string NOLOCK = "NOLOCK";

        
        /// <summary>
        /// Gets the table hints for the query 
        /// </summary>
        /// <param name="hints">List of hints to include</param>
        public static string GetTableHints(params string[] hints)
        {
            if ((hints != null) && (hints.Length > 0))
            {
                return "WITH (" + hints.Join(", ") + ")";
            }

            return null;
        }
    }
}
