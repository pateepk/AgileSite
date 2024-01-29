using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// Aggregated data container which joins the Column names from all data sources
    /// </summary>
    public class AggregatedDataContainer : AbstractDataContainer<AggregatedDataContainer>
    {
        /// <summary>
        /// List of containers
        /// </summary>
        private readonly List<IDataContainer> mContainers = new List<IDataContainer>();


        /// <summary>
        /// List of column names
        /// </summary>
        private List<string> mColumnNames;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sources">Sources of data</param>
        public AggregatedDataContainer(params IDataContainer[] sources)
        {
            mContainers.AddRange(sources);
        }


        /// <summary>
        /// Column names
        /// </summary>
        public override List<string> ColumnNames
        {
            get 
            {
                if (mColumnNames == null)
                {
                    mColumnNames = new List<string>();

                    foreach (var source in mContainers)
                    {
                        mColumnNames.AddRange(source.ColumnNames);
                    }
                }

                return mColumnNames;
            }
        }
        

        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetValue(string columnName, out object value)
        {
            foreach (var source in mContainers)
            {
                if (source.TryGetValue(columnName, out value))
                {
                    return true;
                }
            }

            value = null;
            return false;
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public override bool SetValue(string columnName, object value)
        {
            foreach (var source in mContainers)
            {
                if (source.SetValue(columnName, value))
                {
                    return true;
                }
            }

            return false;
       }
    }
}
