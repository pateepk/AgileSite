using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace WTE
{
    public class WTEDB
    {
        private Dictionary<string, Parameter> m_sqlParameters = new Dictionary<string, Parameter>();

        public string ConnectionString { get; set; }

        public string SQLCommand { get; set; }

        public CommandType SQLCommandType { get; set; }

        /// <summary>
        /// Connection timeout in seconds, default is 30 seconds
        /// </summary>
        public int CommandTimeout { get; set; }

        public Dictionary<string, Parameter> SQLParameters
        {
            get
            {
                return m_sqlParameters;
            }
            set
            {
                m_sqlParameters = value;
            }
        }

        public class TransactionData
        {
            public SqlCommand Command { get; set; }
            public SqlTransaction Transaction { get; set; }

            public TransactionData(SqlCommand command, SqlTransaction transaction)
            {
                Command = command;
                Transaction = transaction;
            }
        }

        public TransactionData MyTransactionData { get; set; }

        public void BeginTransaction()
        {
            if (MyTransactionData != null)
            {
                throw new Exception("WTEDB: previous transaction is still in progress. commit or rollback transaction.");
            }
            SqlConnection connection = new SqlConnection(this.ConnectionString);
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            SqlCommand command = new SqlCommand();
            command.CommandTimeout = this.CommandTimeout;
            command.Connection = connection;
            command.Transaction = transaction;
            MyTransactionData = new TransactionData(command, transaction);
        }

        public void CommitTransaction()
        {
            if (MyTransactionData != null)
            {
                MyTransactionData.Transaction.Commit();
                MyTransactionData.Transaction.Connection.Close();
                MyTransactionData = null;
            }
            else
            {
                throw new Exception("WTEDB: no transaction in progress. call begintransaction first.");
            }
        }

        public void RollBackTransaction()
        {
            if (MyTransactionData != null)
            {
                MyTransactionData.Transaction.Rollback();
                MyTransactionData.Transaction.Connection.Close();
                MyTransactionData = null;
            }
            else
            {
                throw new Exception("WTEDB: no transaction in progress. call begintransaction first.");
            }
        }

        public WTEDB(String connectionString)
            : this(connectionString, CommandType.Text, "")
        {
        }

        public WTEDB(String connectionString, CommandType commandType, String sqlCommand)
        {
            SetCommand(commandType, sqlCommand);
            CommandTimeout = 30;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("connectionString can not be null or empty");
            }
            ConnectionString = connectionString; //ConfigurationManager.ConnectionStrings["SQLPortalConnectionString"].ConnectionString;
        }

        /// <summary>
        /// use an existing external transaction
        /// </summary>
        /// <param name="transaction"></param>
        public WTEDB(SqlTransaction transaction)
            : this(transaction, CommandType.Text, string.Empty)
        {
        }

        /// <summary>
        /// use an existing external transaction
        /// </summary>
        /// <param name="transaction"></param>
        public WTEDB(SqlTransaction transaction, CommandType commandType, String sqlCommand)
        {
            SqlCommand command = new SqlCommand();
            command.CommandTimeout = this.CommandTimeout;
            SetCommand(commandType, sqlCommand);
            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            MyTransactionData = new TransactionData(command, transaction);
        }

        public void SetCommand(CommandType commandType, String sqlCommand)
        {
            SQLCommandType = commandType;
            SQLCommand = sqlCommand;
        }

        public void AddParameter(string key, object value)
        {
            AddParameter(key, value, ParameterDirection.Input);
        }

        public void AddParameter(string key, object value, ParameterDirection direction)
        {
            SQLParameters.Add(key, new Parameter(value, direction));
        }

        public void ClearParameters()
        {
            SQLParameters.Clear();
        }

        public Parameter GetParameter(String key)
        {
            return SQLParameters[key];
        }

        public int ExecuteNonQuery()
        {
            try
            {
                using (SqlConnection MyConnection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand MyCommand = new SqlCommand(SQLCommand, MyConnection))
                    {
                        SqlCommand command = MyCommand;

                        //setup transaction if using
                        if (MyTransactionData != null)
                        {
                            command = MyTransactionData.Command;
                        }
                        else
                        {
                            command.CommandTimeout = this.CommandTimeout;
                            MyConnection.Open();
                        }

                        command.CommandText = this.SQLCommand;
                        command.CommandType = this.SQLCommandType;
                        command.Parameters.Clear();

                        if (SQLParameters.Count > 0)
                        {
                            foreach (string key in SQLParameters.Keys)
                            {
                                Parameter parameter = SQLParameters[key];
                                command.Parameters.AddWithValue("@" + key, parameter.Value);
                                command.Parameters["@" + key].Direction = parameter.Direction;
                            }
                        }

                        int ret = command.ExecuteNonQuery();

                        //update any output parameters
                        if (SQLParameters.Count > 0)
                        {
                            string[] keys = new string[SQLParameters.Keys.Count];
                            SQLParameters.Keys.CopyTo(keys, 0);
                            foreach (string key in keys)
                            {
                                Parameter parameter = SQLParameters[key];
                                if (parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Output)
                                {
                                    object o = command.Parameters["@" + key].Value;
                                    if (o != DBNull.Value)
                                    {
                                        parameter.Value = o;
                                    }
                                    else
                                    {
                                        parameter.Value = null;
                                    }
                                    SQLParameters[key] = parameter;
                                }
                            }
                        }

                        return ret;
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw (ex);
            }
        }

        public DataSet GetDataSet()
        {
            try
            {
                using (SqlConnection MyConnection = new SqlConnection(ConnectionString))
                {
                    using (SqlDataAdapter MyDataAdapter = new SqlDataAdapter(SQLCommand, MyConnection))
                    {
                        SqlConnection connection = MyConnection;
                        SqlDataAdapter dataAdapter = MyDataAdapter;

                        //setup transaction connection if using
                        if (MyTransactionData != null)
                        {
                            dataAdapter = new SqlDataAdapter();
                            dataAdapter.SelectCommand = MyTransactionData.Command;
                        }

                        dataAdapter.SelectCommand.CommandTimeout = this.CommandTimeout;
                        dataAdapter.SelectCommand.CommandText = this.SQLCommand;
                        dataAdapter.SelectCommand.CommandType = SQLCommandType;
                        dataAdapter.SelectCommand.Parameters.Clear();

                        if (SQLParameters.Count > 0)
                        {
                            foreach (string key in SQLParameters.Keys)
                            {
                                Parameter parameter = SQLParameters[key];
                                dataAdapter.SelectCommand.Parameters.AddWithValue("@" + key, parameter.Value);
                                dataAdapter.SelectCommand.Parameters["@" + key].Direction = parameter.Direction;
                            }
                        }

                        DataSet myDataSet = new DataSet();
                        dataAdapter.Fill(myDataSet);

                        //update any output parameter
                        if (SQLParameters.Count > 0)
                        {
                            string[] keys = new string[SQLParameters.Keys.Count];
                            SQLParameters.Keys.CopyTo(keys, 0);
                            foreach (string key in keys)
                            {
                                Parameter parameter = SQLParameters[key];
                                if (parameter.Direction == ParameterDirection.InputOutput ||
                                    parameter.Direction == ParameterDirection.Output)
                                {
                                    object o = MyDataAdapter.SelectCommand.Parameters["@" + key].Value;
                                    if (o != DBNull.Value)
                                    {
                                        parameter.Value = o;
                                    }
                                    else
                                    {
                                        parameter.Value = null;
                                    }
                                    SQLParameters[key] = parameter;
                                }
                            }
                        }

                        return myDataSet;
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw (ex);
            }
        }

        public DataTable GetDataTable()
        {
            DataTable ret = null;
            DataSet myDataSet = GetDataSet();

            if (myDataSet.Tables.Count > 0)
            {
                ret = myDataSet.Tables[0];
            }

            return ret;
        }

        public DataRow GetDataRow()
        {
            DataRow ret = null;
            DataTable dt = GetDataTable();

            if (dt != null && dt.Rows.Count > 0)
            {
                ret = dt.Rows[0];
            }

            return ret;
        }

        public object ExecuteScalar()
        {
            object ret = null;
            DataRow dr = GetDataRow();

            if (dr != null)
            {
                ret = dr.ItemArray[0];
            }

            return ret;
        }

        private void LogException(Exception ex)
        {
            //add logging here
        }

        public class Parameter
        {
            public object Value { get; set; }
            public ParameterDirection Direction { get; set; }

            public Parameter(object value)
                : this(value, ParameterDirection.Input)
            {
            }

            public Parameter(object value, ParameterDirection direction)
            {
                Value = value;
                Direction = direction;
            }
        }
    }
}