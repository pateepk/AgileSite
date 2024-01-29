using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace ssch.tools
{
    public class DRspRecipient_GetByID : TableReaderBase
    {
        public class Columns
        {
            public const string RecipientID = "RecipientID";
            public const string MID = "MID";
            public const string FirstName = "FirstName";
            public const string LastName = "LastName";
            public const string RespiteHourID = "RespiteHourID";
            public const string EligibilityID = "EligibilityID";
            public const string DocumentID = "DocumentID";
            public const string IsInactive = "IsInactive";
        }

        public DRspRecipient_GetByID(DataSet ds)
        {
            base.setData(ds);
        }

        public int RecipientID
        {
            get
            {
                return base.getValueInteger(0, Columns.RecipientID);
            }
        }

        public string MID
        {
            get
            {
                return base.getValue(0, Columns.MID);
            }
        }

        public string FirstName
        {
            get
            {
                return base.getValue(0, Columns.FirstName);
            }
        }

        public string LastName
        {
            get
            {
                return base.getValue(0, Columns.LastName);
            }
        }

        public int RespiteHourID
        {
            get
            {
                return base.getValueInteger(0, Columns.RespiteHourID);
            }
        }

        public string EligibilityID
        {
            get
            {
                return base.getValue(0, Columns.EligibilityID);
            }
        }

        public Int64 DocumentID
        {
            get
            {
                return base.getValueInteger64(0, Columns.DocumentID);
            }
        }

        public bool IsInactive
        {
            get
            {
                return base.getValueBool(0, Columns.IsInactive);
            }
        }

    }
}
