using System.Collections.Generic;
using System.Data.SqlClient;

namespace CMS.DataEngine
{
    /// <summary>
    /// Configuration class for <see cref="AbstractDataConnection.BulkInsert"/>.
    /// </summary>
    public class BulkInsertSettings
    {
        /// <summary>
        /// Gets or sets a collection of column mappings.
        /// Column mappings define the relationships between columns in the data source and columns in the destination.
        /// </summary>
        public IDictionary<string, string> Mappings
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a bitwise flag that specifies one or more options to use with an instance of System.Data.SqlClient.SqlBulkCopy.
        /// </summary>
        public SqlBulkCopyOptions Options
        {
            get;
            set;
        }


        /// <summary>
        /// Number of seconds for the operation to complete before it times out. The default is 30 seconds. A value of 0 indicates no limit; the bulk copy will wait indefinitely.
        /// </summary>
        public int BulkCopyTimeout
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets whether to keep identity of inserted data.
        /// </summary>
        public bool KeepIdentity
        {
            get
            {
                return Options.HasFlag(SqlBulkCopyOptions.KeepIdentity);
            }
            set
            {
                if (value)
                {
                    Options |= SqlBulkCopyOptions.KeepIdentity;
                }
                else
                {
                    Options &= ~SqlBulkCopyOptions.KeepIdentity;
                }
            }
        }


        /// <summary>
        /// Default constructor.
        /// </summary>
        public BulkInsertSettings()
        {
            BulkCopyTimeout = 30;
        }
    }
}
