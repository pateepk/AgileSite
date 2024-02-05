using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTA_custom_egivings_Get : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string EgivingsID = "EgivingsID";
			public const string siteid = "siteid";
			public const string Name = "Name";
			public const string Description = "Description";
			public const string OTorRecurring = "OTorRecurring";
			public const string MinDonation = "MinDonation";
			public const string CC_ACH = "CC_ACH";
		}

		public DRspTA_custom_egivings_Get(DataSet ds)
		{
			base.setData(ds);
		}

		public int EgivingsID(int index) 
		{
			return base.getValueInteger(index, Columns.EgivingsID);
		}

		public int siteid(int index) 
		{
			return base.getValueInteger(index, Columns.siteid);
		}

		public string Name(int index) 
		{
			return base.getValue(index, Columns.Name);
		}

		public string Description(int index) 
		{
			return base.getValue(index, Columns.Description);
		}

		public int OTorRecurring(int index) 
		{
			return base.getValueInteger(index, Columns.OTorRecurring);
		}

		public string MinDonation(int index) 
		{
			return base.getValue(index, Columns.MinDonation);
		}

		public string CC_ACH(int index) 
		{
			return base.getValue(index, Columns.CC_ACH);
		}

	}
}
