using System.Data;
using System.Text;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class to provide staging task parameters.
    /// </summary>
    public class TaskParameters
    {
        #region "Variables"

        /// <summary>
        /// Task paramaeters table.
        /// </summary>
        private DataTable mParametersTable = null;


        /// <summary>
        /// Key for request stock helper caching.
        /// </summary>
        private string mReguestStockKey = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns task parameter table.
        /// </summary>
        public DataTable ParametersTable
        {
            get
            {
                return mParametersTable ?? (mParametersTable = GetEmptyTable());
            }
        }


        /// <summary>
        /// Returns key for request stock helper caching.
        /// </summary>
        public virtual string RequestStockKey
        {
            get
            {
                if (mReguestStockKey == null)
                {
                    StringBuilder sb = new StringBuilder();
                    if (ParametersTable.Rows.Count > 0)
                    {
                        // Sort table (to ensure same tables that are ordered differently are recognized as equal)
                        ParametersTable.DefaultView.Sort = "Key ASC";
                        foreach (DataRow row in ParametersTable.Rows)
                        {
                            string key = ValidationHelper.GetString(row["Key"], null);
                            string value = ValidationHelper.GetString(row["Value"], null);
                            if ((key != null) && (value != null))
                            {
                                // Append only initialized keys with values
                                sb.Append(key + "=" + value + ",");
                            }
                        }
                    }
                    mReguestStockKey = sb.ToString().TrimEnd(',');
                }
                return mReguestStockKey;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TaskParameters()
        {
        }


        /// <summary>
        /// Constructor with ParametersTable initialization.
        /// </summary>
        /// <param name="dt">DataTable used for task ParameterTable initialization</param>
        public TaskParameters(DataTable dt)
        {
            mParametersTable = dt;
        }


        /// <summary>
        /// Sets the parameter value under specified key for further use.
        /// </summary>
        /// <param name="key">Identification key</param>
        /// <param name="value">Value to be stored</param>
        public void SetParameter(string key, object value)
        {
            DataRow dr = GetRecord(key);
            if (dr != null)
            {
                dr["Value"] = value;
            }
            else
            {
                DataRow drNew = ParametersTable.NewRow();
                drNew["Key"] = key;
                drNew["Value"] = value;
                ParametersTable.Rows.Add(drNew);
            }
        }


        /// <summary>
        /// Gets the parameter value with given key.
        /// </summary>
        /// <param name="key">Identification key</param>
        /// <returns>Stored value</returns>
        public object GetParameter(string key)
        {
            DataRow dr = GetRecord(key);
            if (dr != null)
            {
                return dr["Value"];
            }
            return null;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns empty translation table.
        /// </summary>
        private DataTable GetEmptyTable()
        {
            DataTable dt = new DataTable();
            EnsureColumns(dt);
            dt.TableName = "TaskParameters";

            return dt;
        }


        /// <summary>
        /// Ensures the columns in the translation table.
        /// </summary>
        /// <param name="dt">Translation table</param>
        private void EnsureColumns(DataTable dt)
        {
            // Ensure the table columns
            if (!dt.Columns.Contains("Key"))
            {
                dt.Columns.Add(new DataColumn("Key", typeof(string)));
            }
            if (!dt.Columns.Contains("Value"))
            {
                dt.Columns.Add(new DataColumn("Value", typeof(object)));
            }
        }


        /// <summary>
        /// Gets the record by key.
        /// </summary>
        /// <param name="key">Identification key</param>
        /// <returns>Particular DataRow</returns>
        private DataRow GetRecord(string key)
        {
            // Create where and select particular parameter row
            string where = "Key = '" + SqlHelper.GetSafeQueryString(key, false) + "'";
            DataRow[] rows = ParametersTable.Select(where);

            // Return found row or null
            if ((rows == null) || (rows.Length <= 0))
            {
                return null;
            }
            else
            {
                return rows[0];
            }
        }

        #endregion
    }
}