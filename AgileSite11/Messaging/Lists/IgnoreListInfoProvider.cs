using System;
using System.Data;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.Messaging
{
    using TypedDataSet = InfoDataSet<IgnoreListInfo>;

    /// <summary>
    /// Class providing IgnoreListInfo management.
    /// </summary>
    public class IgnoreListInfoProvider : AbstractInfoProvider<IgnoreListInfo, IgnoreListInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns the IgnoreListInfo structure for the specified users.
        /// </summary>
        /// <param name="ownerId">ID of ignorelist owner</param>
        /// <param name="contactId">ID of ignored user</param>
        public static IgnoreListInfo GetIgnoreListInfo(int ownerId, int contactId)
        {
            return ProviderObject.GetIgnoreListInfoInternal(ownerId, contactId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified ignoreList.
        /// </summary>
        /// <param name="ignore">IgnoreList to set</param>
        public static void SetIgnoreListInfo(IgnoreListInfo ignore)
        {
            // Check license for messaging
            LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Messaging);

            ProviderObject.SetIgnoreListInfoInternal(ignore);
        }


        /// <summary>
        /// Adds user to owner's ignorelist.
        /// </summary>
        /// <param name="ownerId">ID of ignorelist owner</param>
        /// <param name="contactId">ID of ignored user</param>
        public static void AddToIgnoreList(int ownerId, int contactId)
        {
            ProviderObject.AddToIgnoreListInternal(ownerId, contactId);
        }


        /// <summary>
        /// Deletes specified ignoreList.
        /// </summary>
        /// <param name="ignore">IgnoreList object</param>
        public static void DeleteIgnoreListInfo(IgnoreListInfo ignore)
        {
            ProviderObject.DeleteInfo(ignore);
        }


        /// <summary>
        /// Deletes specified ignoreList.
        /// </summary>
        /// <param name="ownerId">ID of ignorelist owner</param>
        /// <param name="contactId">ID of ignored user</param>
        public static void RemoveFromIgnoreList(int ownerId, int contactId)
        {
            ProviderObject.RemoveFromIgnoreListInternal(ownerId, contactId);
        }


        /// <summary>
        /// Returns ignorelist based on conditions.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<IgnoreListInfo> instead.")]
        public static TypedDataSet GetIgnoreList(string where, string orderBy)
        {
            return ProviderObject.GetIgnoreListInternal(where, orderBy);
        }


        /// <summary>
        /// Returns all users from user's ignorelist.
        /// </summary>
        /// <param name="ownerId">User ID</param>
        public static TypedDataSet GetIgnoreList(int ownerId)
        {
            return ProviderObject.GetIgnoreListInternal(ownerId, null, null, -1, null);
        }


        /// <summary>
        /// Returns all users from user's ignorelist.
        /// </summary>
        /// <param name="ownerId">User ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Select only top N columns</param>
        /// <param name="columns">Specifies columns to be selected</param>
        public static TypedDataSet GetIgnoreList(int ownerId, string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetIgnoreListInternal(ownerId, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns true if given user is in owner's ignorelist.
        /// </summary>
        /// <param name="ownerId">ID of ignorelist owner</param>
        /// <param name="contactId">ID of user to check</param>
        public static bool IsInIgnoreList(int ownerId, int contactId)
        {
            return ProviderObject.IsInIgnoreListInternal(ownerId, contactId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the IgnoreListInfo structure for the specified ignoreList.
        /// </summary>
        /// <param name="ownerId">ID of ignorelist owner</param>
        /// <param name="contactId">ID of ignored user</param>
        protected virtual IgnoreListInfo GetIgnoreListInfoInternal(int ownerId, int contactId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@IgnoreListUserID", ownerId);
            parameters.Add("@IgnoreListIgnoredUserID", contactId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("messaging.ignorelist.select", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new IgnoreListInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Sets (updates or inserts) specified ignoreList.
        /// </summary>
        /// <param name="ignore">IgnoreList to set</param>
        protected virtual void SetIgnoreListInfoInternal(IgnoreListInfo ignore)
        {
            if (ignore != null)
            {
                // Check IDs
                if ((ignore.IgnoreListUserID <= 0) || (ignore.IgnoreListIgnoredUserID <= 0))
                {
                    throw new Exception("[IgnoreListInfoProvider.SetIgnoreListInfo]: Object IDs not set.");
                }

                // Get existing
                IgnoreListInfo existing = GetIgnoreListInfoInternal(ignore.IgnoreListUserID, ignore.IgnoreListIgnoredUserID);
                if (existing != null)
                {
                    ignore.Generalized.UpdateData();
                }
                else
                {
                    ignore.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[IgnoreListInfoProvider.SetIgnoreListInfo]: No IgnoreListInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(IgnoreListInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);
            }
        }


        /// <summary>
        /// Adds user to owner's ignorelist.
        /// </summary>
        /// <param name="ownerId">ID of ignorelist owner</param>
        /// <param name="contactId">ID of ignored user</param>
        protected virtual void AddToIgnoreListInternal(int ownerId, int contactId)
        {
            // Add user to the ignore list
            if (!IsInIgnoreListInternal(ownerId, contactId))
            {
                IgnoreListInfo ili = new IgnoreListInfo();
                ili.IgnoreListUserID = ownerId;
                ili.IgnoreListIgnoredUserID = contactId;
                SetIgnoreListInfoInternal(ili);
            }

            // Remove user from contact list 
            ContactListInfoProvider.RemoveFromContactList(ownerId, contactId);
        }


        /// <summary>
        /// Deletes specified ignoreList.
        /// </summary>
        /// <param name="ownerId">ID of ignorelist owner</param>
        /// <param name="contactId">ID of ignored user</param>
        protected virtual void RemoveFromIgnoreListInternal(int ownerId, int contactId)
        {
            IgnoreListInfo infoObj = GetIgnoreListInfoInternal(ownerId, contactId);
            DeleteInfo(infoObj);
        }


        /// <summary>
        /// Returns all users from user's ignorelist.
        /// </summary>
        /// <param name="ownerId">User ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Select only top N columns</param>
        /// <param name="columns">Specifies columns to be selected</param>
        protected virtual TypedDataSet GetIgnoreListInternal(int ownerId, string where, string orderBy, int topN, string columns)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@UserID ", ownerId);
            parameters.EnsureDataSet<IgnoreListInfo>();

            return ConnectionHelper.ExecuteQuery("messaging.ignorelist.selectignorelist", parameters, where, orderBy, topN, columns).As<IgnoreListInfo>();
        }


        /// <summary>
        /// Returns ignorelist based on conditions.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<IgnoreListInfo> instead.")]
        protected virtual TypedDataSet GetIgnoreListInternal(string where, string orderBy)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.EnsureDataSet<IgnoreListInfo>();

            return ConnectionHelper.ExecuteQuery("messaging.ignorelist.selectall", parameters, where, orderBy).As<IgnoreListInfo>();
        }


        /// <summary>
        /// Returns true if given user is in owner's ignorelist.
        /// </summary>
        /// <param name="ownerId">ID of ignorelist owner</param>
        /// <param name="contactId">ID of user to check</param>
        protected virtual bool IsInIgnoreListInternal(int ownerId, int contactId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@IgnoreListIgnoredUserID", contactId);
            parameters.Add("@IgnoreListUserID", ownerId);

            DataSet result = ConnectionHelper.ExecuteQuery("messaging.ignorelist.select", parameters);

            return (!DataHelper.DataSourceIsEmpty(result));
        }

        #endregion
    }
}