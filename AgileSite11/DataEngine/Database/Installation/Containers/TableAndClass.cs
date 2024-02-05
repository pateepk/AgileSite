namespace CMS.DataEngine
{
    /// <summary>
    /// Defines table with appropriate data class info.
    /// </summary>
    internal class TableAndClass
    {
        /// <summary>
        /// Table name.
        /// </summary>
        public string TableName { get; set; }


        /// <summary>
        /// Data class info.
        /// </summary>
        public DataClassInfo ClassInfo { get; set; }
    }
}
