using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace ssch.tools
{

    public class DataAccess
    {

        // hold current connection string.
        // if there will be more than one connection string, need to create a new one
        private string _connstring = string.Empty;

        public DataAccess(string connectionString)
        {
            _connstring = connectionString;
        }

        public DataSet ExecuteDataSet(SqlCommand command)
        {
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.SelectCommand.Connection = new SqlConnection(_connstring);

            DataSet ds = new DataSet();

            try
            {
                adapter.Fill(ds);
            }
            catch
            {
                // ErrorManager.logError(SV.ErrorMessages.ExecuteDataSet, e);
                ds = null;
            }
            finally
            {
                if (adapter.SelectCommand.Connection != null)
                { adapter.SelectCommand.Connection.Close(); }
            }

            return ds;
        }

        public DataTable ExecuteDataTable(SqlCommand command)
        {
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.SelectCommand.Connection = new SqlConnection(_connstring);

            DataTable table = new DataTable();

            try
            {
                adapter.Fill(table);
            }
            catch
            {
                // ErrorManager.logError(SV.ErrorMessages.ExecuteDataTable, e);
            }
            finally
            {
                if (adapter.SelectCommand.Connection != null)
                { adapter.SelectCommand.Connection.Close(); }
            }

            return table;
        }

        public void ExecuteNonQuery(SqlCommand command)
        {
            command.Connection = new SqlConnection(_connstring);

            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            catch
            {
                // ErrorManager.logError(SV.ErrorMessages.ExecuteNonQuery, e);
            }
            finally
            {
                if (null != command.Connection)
                {
                    command.Connection.Close();
                }
            }
        }

        public object ExecuteScalar(SqlCommand command)
        {
            object rtn = null;
            command.Connection = new SqlConnection(_connstring);

            try
            {
                command.Connection.Open();
                rtn = command.ExecuteScalar();
            }
            catch
            {
                // ErrorManager.logError(SV.ErrorMessages.ExecuteNonQuery, e);
            }
            finally
            {
                if (null != command.Connection)
                {
                    command.Connection.Close();
                }
            }

            return rtn;
        }

    }

}
