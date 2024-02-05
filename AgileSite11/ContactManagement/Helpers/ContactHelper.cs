using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class used for merging, splitting and deleting contacts.
    /// </summary>
    public static class ContactHelper
    {
        #region "Constants"

        /// <summary>
        /// Prefix for anonymous contacts.
        /// </summary>
        public const string ANONYMOUS = "Anonymous - ";


        /// <summary>
        /// Anonymous contact last name date format.
        /// </summary>
        public const string ANONYMOUS_CONTACT_LASTNAME_DATE_PATTERN = "yyyy-MM-dd HH:mm:ss.fff";


        /// <summary>
        /// Replaced left bracket.
        /// </summary>
        public const string REPLACED_BRACKET_LEFT = "\x00\x00";

        /// <summary>
        /// Replaced right bracket.
        /// </summary>
        public const string REPLACED_BRACKET_RIGHT = "\x01\x01";

        #endregion


        #region "Public general methods"

        /// <summary>
        /// Returns ContactListInfos for specified parameters.
        /// </summary>
        /// <param name="parameters">Query parameters</param>
        /// <param name="where">SQL WHERE condition</param>
        /// <param name="orderBy">SQL ORDER BY parameters</param>
        /// <param name="topN">SQL TOP N parameters</param>
        /// <param name="columns">Selected columns</param>
        /// <returns>Returns DataSet</returns>
        public static DataSet GetContactListInfos(QueryDataParameters parameters, string where, string orderBy, int topN, string columns)
        {
            // Check license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.SimpleContactManagement);
            }

            return ConnectionHelper.ExecuteQuery("om.contact.selectfromview", parameters, @where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns email domain name.
        /// </summary>
        public static string GetEmailDomain(string email)
        {
            string domainName = null;

            if ((!String.IsNullOrEmpty(email)) && (ValidationHelper.IsEmail(email)))
            {
                domainName = email.Substring(email.IndexOfCSafe("@") + 1);
            }

            return domainName;
        }

        #endregion


        #region "Remove dependencies methods"

        /// <summary>
        /// Removes customer dependencies in all contact management objects.
        /// </summary>
        /// <param name="customerID">Customer being deleted</param>
        public static void RemoveCustomer(int customerID)
        {
            var parameters = new QueryDataParameters();
            parameters.Add("@ID", customerID);
            ConnectionHelper.ExecuteQuery("om.contact.removecustomer", parameters);
        }

        #endregion
    }
}