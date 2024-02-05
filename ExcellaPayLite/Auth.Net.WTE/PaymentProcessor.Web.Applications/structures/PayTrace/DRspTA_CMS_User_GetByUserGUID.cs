using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTA_CMS_User_GetByUserGUID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string UserID = "UserID";
			public const string UserName = "UserName";
			public const string FirstName = "FirstName";
			public const string MiddleName = "MiddleName";
			public const string LastName = "LastName";
			public const string FullName = "FullName";
			public const string Email = "Email";
			public const string UserPassword = "UserPassword";
			public const string PreferredCultureCode = "PreferredCultureCode";
			public const string PreferredUICultureCode = "PreferredUICultureCode";
			public const string UserEnabled = "UserEnabled";
			public const string UserIsEditor = "UserIsEditor";
			public const string UserIsGlobalAdministrator = "UserIsGlobalAdministrator";
			public const string UserIsExternal = "UserIsExternal";
			public const string UserPasswordFormat = "UserPasswordFormat";
			public const string UserCreated = "UserCreated";
			public const string LastLogon = "LastLogon";
			public const string UserStartingAliasPath = "UserStartingAliasPath";
			public const string UserGUID = "UserGUID";
			public const string UserLastModified = "UserLastModified";
			public const string UserLastLogonInfo = "UserLastLogonInfo";
			public const string UserIsHidden = "UserIsHidden";
			public const string UserVisibility = "UserVisibility";
			public const string UserIsDomain = "UserIsDomain";
			public const string UserHasAllowedCultures = "UserHasAllowedCultures";
			public const string UserSiteManagerDisabled = "UserSiteManagerDisabled";
			public const string UserTokenID = "UserTokenID";
			public const string UserMFRequired = "UserMFRequired";
			public const string UserTokenIteration = "UserTokenIteration";
		}

		public DRspTA_CMS_User_GetByUserGUID(DataSet ds)
		{
			base.setData(ds);
		}

		public int UserID(int index) 
		{
			return base.getValueInteger(index, Columns.UserID);
		}

		public string UserName(int index) 
		{
			return base.getValue(index, Columns.UserName);
		}

		public string FirstName(int index) 
		{
			return base.getValue(index, Columns.FirstName);
		}

		public string MiddleName(int index) 
		{
			return base.getValue(index, Columns.MiddleName);
		}

		public string LastName(int index) 
		{
			return base.getValue(index, Columns.LastName);
		}

		public string FullName(int index) 
		{
			return base.getValue(index, Columns.FullName);
		}

		public string Email(int index) 
		{
			return base.getValue(index, Columns.Email);
		}

		public string UserPassword(int index) 
		{
			return base.getValue(index, Columns.UserPassword);
		}

		public string PreferredCultureCode(int index) 
		{
			return base.getValue(index, Columns.PreferredCultureCode);
		}

		public string PreferredUICultureCode(int index) 
		{
			return base.getValue(index, Columns.PreferredUICultureCode);
		}

		public bool UserEnabled(int index) 
		{
			return base.getValueBool(index, Columns.UserEnabled);
		}

		public bool UserIsEditor(int index) 
		{
			return base.getValueBool(index, Columns.UserIsEditor);
		}

		public bool UserIsGlobalAdministrator(int index) 
		{
			return base.getValueBool(index, Columns.UserIsGlobalAdministrator);
		}

		public bool UserIsExternal(int index) 
		{
			return base.getValueBool(index, Columns.UserIsExternal);
		}

		public string UserPasswordFormat(int index) 
		{
			return base.getValue(index, Columns.UserPasswordFormat);
		}

		public DateTime UserCreated(int index) 
		{
			return base.getValueDate(index, Columns.UserCreated);
		}

		public DateTime LastLogon(int index) 
		{
			return base.getValueDate(index, Columns.LastLogon);
		}

		public string UserStartingAliasPath(int index) 
		{
			return base.getValue(index, Columns.UserStartingAliasPath);
		}

		public SqlGuid UserGUID(int index) 
		{
			return base.getSqlGuid(index, Columns.UserGUID);
		}

		public DateTime UserLastModified(int index) 
		{
			return base.getValueDate(index, Columns.UserLastModified);
		}

		public string UserLastLogonInfo(int index) 
		{
			return base.getValue(index, Columns.UserLastLogonInfo);
		}

		public bool UserIsHidden(int index) 
		{
			return base.getValueBool(index, Columns.UserIsHidden);
		}

		public string UserVisibility(int index) 
		{
			return base.getValue(index, Columns.UserVisibility);
		}

		public bool UserIsDomain(int index) 
		{
			return base.getValueBool(index, Columns.UserIsDomain);
		}

		public bool UserHasAllowedCultures(int index) 
		{
			return base.getValueBool(index, Columns.UserHasAllowedCultures);
		}

		public bool UserSiteManagerDisabled(int index) 
		{
			return base.getValueBool(index, Columns.UserSiteManagerDisabled);
		}

        public SqlGuid UserTokenID(int index) 
		{
			return base.getSqlGuid(index, Columns.UserTokenID);
		}

		public bool UserMFRequired(int index) 
		{
			return base.getValueBool(index, Columns.UserMFRequired);
		}

		public int UserTokenIteration(int index) 
		{
			return base.getValueInteger(index, Columns.UserTokenIteration);
		}

	}
}
