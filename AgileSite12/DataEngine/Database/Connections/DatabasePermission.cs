namespace CMS.DataEngine
{
    /// <summary>
    /// Database permissions
    /// </summary>
    public class DatabasePermission
    {
        /// <summary>
        /// Modify database - create tables, functions etc.
        /// </summary>
        public static string ModifyDatabase
        {
            get
            {
                return "CONTROL SERVER";
            }
        }


        /// <summary>
        /// Create new database.
        /// </summary>
        public static string CreateDatabase
        {
            get
            {
                return "CREATE ANY DATABASE";
            }
        }
    }
}
