namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Data;
    using System.Data.SqlTypes;

    public class TableReaderBase
    {
        protected DataTable _derror;
        protected DataTable _dt;
        protected DataRow[] _sortdr;
        protected DataSet _returnset;
        protected bool _isSorted = false;

        private class Columns
        {
            public const string ErrorNumber = "ErrorNumber";
            public const string ErrorCode = "ErrorCode";
            public const string ErrorMessage = "ErrorMessage";
        }

        public void sort(string ColumnName, string SortDirection)
        {
            _sortdr = _dt.Select(String.Empty, string.Format("{0} {1}", ColumnName, SortDirection));
            _isSorted = true;
        }

        public void setData(DataTable tbData, DataTable tbError)
        {
            if (tbData != null)
            {
                _dt = tbData;
            }

            if (tbError != null)
            {
                _derror = tbError;
            }
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
                _returnset = ds;
            }
        }

        public DataTable DataSource
        {
            get
            {
                return _dt; // return main table for datasource
            }
        }

        public DataSet ReturnSet
        {
            get
            {
                return _returnset; // return main table for dataset (all the tables)
            }
        }

        public int Count
        {
            get
            {
                return _dt == null ? 0 : _dt.Rows.Count;
            }
        }

        public void deleteRow(int index)
        {
            if (_isSorted)
            {
                _sortdr[index].Delete();
            }
            else
            {
                _dt.Rows[index].Delete();
            }
        }

        public string getValue(int index, string cs)
        {
            string s = string.Empty;
            if ((index > -1) && (index < _dt.Rows.Count))
            {
                try
                {
                    // if it is sorted, use datarow array
                    if (_isSorted)
                    {
                        s = _sortdr[index][cs].ToString();
                    }
                    else
                    {
                        s = _dt.Rows[index][cs].ToString();
                    }
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

        protected Guid getValueGuid(int index, string cs)
        {
            Guid g;
            Guid.TryParse(getValue(index, cs), out g);
            return g;
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

        protected long getValueLong(int index, string cs)
        {
            long r = 0;
            long.TryParse(getValue(index, cs), out r);
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

        protected SqlGuid getSqlGuid(int index, string cs)
        {
            Guid g = Guid.Empty;
            Guid.TryParse(getValue(index, cs), out g);
            return g;
        }

        protected decimal getValueDecimal(int index, string cs)
        {
            decimal d = 0;
            decimal.TryParse(getValue(index, cs), out d);
            return d;
        }

        protected XmlDocument getXmlDocument(int index, string cs)
        {
            XmlDocument xdoc = new XmlDocument();
            string sv = getValue(index, cs);
            if (!String.IsNullOrEmpty(sv))
            {
                xdoc.LoadXml("<" + SV.Common.RootTag + ">" + sv + "</" + SV.Common.RootTag + ">");
            }
            return xdoc;
        }

        protected XDocument getXDocument(int index, string cs)
        {
            XDocument xdoc = new XDocument();
            string sv = getValue(index, cs);
            if (!String.IsNullOrEmpty(sv))
            {
                try
                {
                    xdoc = XDocument.Parse(sv);
                }
                catch
                {
                    xdoc = new XDocument();
                }
            }
            return xdoc;
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

        public bool isError
        {
            get
            {
                int r = 0;
                int.TryParse(_derror.Rows[0][Columns.ErrorCode].ToString(), out r);
                return (r > 0);
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
