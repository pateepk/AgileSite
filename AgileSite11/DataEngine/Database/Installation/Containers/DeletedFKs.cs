namespace CMS.DataEngine
{
    /// <summary>
    /// Class to define FKs which were removed during DB separation. Is used in DB join.
    /// </summary>
    public class DeletedFKs
    {
        /// <summary>
        /// Table with foreign key.
        /// </summary>
        public string ReferencingTable { get; set; }


        /// <summary>
        /// Name of the foreign key.
        /// </summary>
        public string FKName { get; set; }


        /// <summary>
        /// Foreign key column.
        /// </summary>
        public string ReferencingColumn { get; set; }


        /// <summary>
        /// Table with primary key.
        /// </summary>
        public string PKTable { get; set; }


        /// <summary>
        /// Primary key column.
        /// </summary>
        public string PKColumn { get; set; }
    }
}
