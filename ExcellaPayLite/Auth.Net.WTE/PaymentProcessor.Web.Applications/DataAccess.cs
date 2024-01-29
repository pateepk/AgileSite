using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data;
    using System.Data.SqlClient;

    public class DataAccess
    {

        // hold current connection string.
        // if there will be more than one connection string, need to create a new one
        private string _connstring = string.Empty;

        public DataAccess()
        {
            _connstring = AppSettings.connString();
        }

        public DataAccess(databaseServer db)
        {
            _connstring = AppSettings.connString(db);
        }

        public DataAccess(string connectionString)
        {
            _connstring = connectionString;
        }

        public DataSet ExecuteDataSet(SqlCommand command)
        {
            return ExecuteDataSet(command, true);
        }

        public DataSet ExecuteDataSet(SqlCommand command, bool isLogError)
        {
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.SelectCommand.Connection = new SqlConnection(_connstring);

            DataSet ds = new DataSet();

            try
            {
                adapter.Fill(ds);
            }
            catch (Exception e)
            {
                if (isLogError)
                {
                    ErrorManager.logErrorSQLCommand(command, SV.ErrorMessages.ExecuteDataSet, e);
                }
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
            return ExecuteDataTable(command, true);
        }

        public DataTable ExecuteDataTable(SqlCommand command, bool isLogError)
        {
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.SelectCommand.Connection = new SqlConnection(_connstring);

            DataTable table = new DataTable();

            try
            {
                adapter.Fill(table);
            }
            catch (Exception e)
            {
                if (isLogError)
                {
                    ErrorManager.logErrorSQLCommand(command, SV.ErrorMessages.ExecuteDataSet, e);
                }
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
            catch (Exception e)
            {
                ErrorManager.logError(SV.ErrorMessages.ExecuteNonQuery, e);
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
            catch (Exception e)
            {
                ErrorManager.logError(SV.ErrorMessages.ExecuteNonQuery, e);
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
