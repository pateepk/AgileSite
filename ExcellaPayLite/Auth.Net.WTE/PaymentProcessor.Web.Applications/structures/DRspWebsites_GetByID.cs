namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspWebsites_GetByID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string WebsiteName = "WebsiteName";
			public const string IsUpgrading = "IsUpgrading";
			public const string UpgradingByUserID = "UpgradingByUserID";
            public const string UserFullName = "UserFullName";
			public const string LastRestart = "LastRestart";
		}

		public DRspWebsites_GetByID(DataSet ds)
		{
			base.setData(ds);
		}

		public string WebsiteName
		{
            get
            {
                return base.getValue(0, Columns.WebsiteName);
            }
		}

		public bool IsUpgrading
		{
            get
            {
                return base.getValueBool(0, Columns.IsUpgrading);
            }
		}

		public int UpgradingByUserID 
		{
            get
            {
                return base.getValueInteger(0, Columns.UpgradingByUserID);
            }
		}

        public string UserFullName
        {
            get
            {
                return base.getValue(0, Columns.UserFullName);
            }
        }

        public DateTime LastRestart
        {
            get
            {
                return base.getValueDate(0, Columns.LastRestart);
            }
        }

	}
}
