using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace ssch.tools
{

    public class TableReaderBase
    {
        protected DataTable _derror;
        protected DataTable _dt;

        private class Columns
        {
            public const string ErrorNumber = "ErrorNumber";
		    public const string ErrorCode ="ErrorCode";
		    public const string ErrorMessage = "ErrorMessage";
        }

        public void setData(DataSet ds)
        {
            if (ds != null)
            {
                int lt = ds.Tables.Count;
                // last table is error message
                if (lt > 0)
                {
                    _derror = ds.Tables[lt - 1];
                }
                else
                {
                    // ErrorManager.logMessage(SV.ErrorMessages.TablesInDataSet);
                }

                if (lt > 1)
                {
                    // above that table is the table of return
                    _dt = ds.Tables[lt - 2];
                }
                else
                {
                    // ErrorManager.logMessage(SV.ErrorMessages.TablesInDataSet);
                }
            }
        }

        public int Count
        {
            get
            {
                return _dt.Rows.Count;
            }
        }

        protected string getValue(int index, string cs)
        {
            string s = string.Empty;
            if ((index > -1) && (index < _dt.Rows.Count))
            {
                try
                {
                    s = _dt.Rows[index][cs].ToString();
                    if (s == null)
                    {
                        s = string.Empty;
                    }
                }
                catch
                {
                    s = string.Empty;
                }
            }
            return s;
        }

        protected int getValueInteger(int index, string cs)
        {
            int r = 0;
            int.TryParse(getValue(index, cs), out r);
            return r;
        }

        protected Int64 getValueInteger64(int index, string cs)
        {
            Int64 r = 0;
            Int64.TryParse(getValue(index, cs), out r);
            return r;
        }

        protected double getValueDouble(int index, string cs)
        {
            double r = 0;
            double.TryParse(getValue(index, cs), out r);
            return r;
        }

        protected DateTime getValueDate(int index, string cs)
        {
            DateTime d = DateTime.MinValue;
            DateTime.TryParse(getValue(index, cs), out d);
            return d;
        }

        protected bool getValueBool(int index, string cs)
        {
            bool b = false;
            bool.TryParse(getValue(index, cs), out b);
            return b;
        }

        protected byte getValueByte(int index, string cs)
        {
            byte b = 0;
            byte.TryParse(getValue(index, cs), out b);
            return b;
        }

        protected decimal getValueDecimal(int index, string cs)
        {
            decimal d = 0;
            decimal.TryParse(getValue(index, cs), out d);
            return d;
        }

        public int ErrorNumber
        {
            get
            {
                int r = 0;
                int.TryParse(_derror.Rows[0][Columns.ErrorNumber].ToString(), out r);
                return r;
            }
        }

        public int ErrorCode
        {
            get
            {
                int r = 0;
                int.TryParse(_derror.Rows[0][Columns.ErrorCode].ToString(), out r);
                return r;
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _derror.Rows[0][Columns.ErrorMessage].ToString();
            }
        }

    }

}
