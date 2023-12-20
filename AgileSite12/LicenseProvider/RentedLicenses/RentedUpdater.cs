using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider.RentedLicenseWebService;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// Class representing update logic for rented licenses.
    /// </summary>
    public class RentedUpdater
    {
        #region "Variables"

        // How long before expiration of a license key will be new one requested
        private const int EXPIRATION_RESERVE = 7;

        /// <summary>
        /// Event log source for rented license updates
        /// </summary>
        public const string LOG_SOURCE = "RentedLicenseUpdater";

        /// <summary>
        /// Event log code for rented license updates
        /// </summary>
        public const string EVENT_CODE = "LICENSEUPDATE";

        private Regex mRentedKeyCodeRegEx;
        private string mServiceUrl;

        #endregion


        #region "Properties"

        /// <summary>
        /// Regular expression for rented key code
        /// </summary>
        private Regex RentedKeyCodeRegEx
        {
            get
            {
                if (mRentedKeyCodeRegEx == null)
                {
                    mRentedKeyCodeRegEx = RegexHelper.GetRegex("^RL-[A-F0-9]{8}-.+$", RegexOptions.Compiled);
                }

                return mRentedKeyCodeRegEx;
            }
        }


        /// <summary>
        /// Gets or sets rented license webservice url
        /// </summary>
        protected string ServiceUrl
        {
            get
            {
                return mServiceUrl;
            }
            set
            {
                mServiceUrl = value;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Rented updater constructor
        /// </summary>
        public RentedUpdater()
        {
        }


        /// <summary>
        /// Constructor which can set rented license webservice url.
        /// If null default url is used.
        /// </summary>
        /// <param name="serviceUrl">Service URL to be set</param>
        public RentedUpdater(string serviceUrl)
        {
            ServiceUrl = serviceUrl;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Renews license keys for domains specified by rented keys.
        /// </summary>
        /// <param name="rentedKeys">Rented key codes</param>        
        /// <returns>Information message</returns>
        public string RenewLicenseKeys(string[] rentedKeys)
        {
            if (rentedKeys == null)
            {
                throw new ArgumentException();
            }

            // Get domains corresponding to rented keys
            string[] domains = GetDomains(rentedKeys);

            Dictionary<string, DataRow> licenses = GetCurrentLicensesWithExpiration(domains);


            #region "Get rented keys which need renewal"

            List<string> renewalKeys = new List<string>();

            // Loop trough all rented key codes and check if license for it should be renewed/retrieved
            for (int i = 0; i < rentedKeys.Length; i++)
            {
                bool renew = true;

                DataRow licenseRow;
                licenses.TryGetValue(domains[i], out licenseRow);

                // Check if license key for domain already exists
                if (licenseRow != null)
                {

                    string licenseExpiration = ValidationHelper.GetString(licenseRow["LicenseExpiration"], string.Empty);
                    // Check for perpetual license
                    if (licenseExpiration == LicenseKeyInfoProvider.TIME_UNLIMITED_LICENSE_CODENAME)
                    {
                        throw new InvalidOperationException(String.Format(CoreServices.Localization.GetString("RentedLicense.PerpetualExists"), domains[i]));
                    }
                    else
                    {
                        DateTime expiration = ParseDateTime(licenseExpiration);

                        if (expiration > DateTime.Now.AddDays(EXPIRATION_RESERVE))
                        {
                            // Is within limit, does not need to be renewed.
                            renew = false;
                        }
                    }
                }
                else
                {
                    // New rented license
                }


                if (renew)
                {
                    renewalKeys.Add(rentedKeys[i]);
                }
            }

            #endregion


            string eventType = "I";
            string additionalInfo = null;
            string returnMessage = null;

            if (rentedKeys.Length > 0)
            {
                if (renewalKeys.Count == 0)
                {
                    returnMessage = CoreServices.Localization.GetString("RentedLicense.NoLicenseNeeded");
                }

                RentedLicenseService service = new RentedLicenseService();

                if (!String.IsNullOrEmpty(ServiceUrl))
                {
                    service.Url = ServiceUrl;
                }


                RentedLicenseResponse response = service.GetLicenseKeys(rentedKeys, renewalKeys.ToArray(), GetSystemVersion());
                DomainLicense[] newLicenseKeys = response.Licenses;

                eventType = response.Status;

                if (response.Status == "E")
                {
                    returnMessage = "Error: " + response.Message;
                }
                else
                {
                    // If there is something wrong, generate log
                    if (response.Status != "I")
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(response.Message);

                        if (newLicenseKeys != null)
                        {
                            foreach (DomainLicense lic in newLicenseKeys)
                            {
                                if (lic.Status != "I")
                                {
                                    sb.AppendLine(lic.Message);
                                }
                            }
                        }
                        additionalInfo = sb.ToString();
                    }

                    // Update keys
                    int updatedCount = UpdateLicenseKeys(newLicenseKeys);

                    returnMessage += String.Format(CoreServices.Localization.GetString("RentedLicense.LicenseUpdated"), updatedCount);
                    if (!String.IsNullOrEmpty(additionalInfo))
                    {
                        returnMessage += " " + CoreServices.Localization.GetString("RentedLicense.SomeProblem");
                    }
                }
            }

            // Log info
            EventLogProvider.LogEvent(eventType, LOG_SOURCE, EVENT_CODE, returnMessage + "\n" + additionalInfo);

            return returnMessage;
        }


        /// <summary>
        /// Creates array of domain from rented key codes.
        /// Domain is the suffix of rented key code.
        /// </summary>
        /// <param name="rentedKeys">Array with rented key codes</param>        
        private string[] GetDomains(string[] rentedKeys)
        {
            if (rentedKeys == null)
            {
                return null;
            }

            string[] domains = new string[rentedKeys.Length];

            for (int i = 0; i < rentedKeys.Length; i++)
            {
                // Check if key code is valid
                if (!RentedKeyCodeRegEx.IsMatch(rentedKeys[i]))
                {
                    throw new InvalidOperationException(String.Format(CoreServices.Localization.GetString("RentedLicense.InvalidKeyCode"), rentedKeys[i]));
                }

                domains[i] = rentedKeys[i].Substring(12);
            }

            // Check for duplicit domain names
            CheckForDuplicitDomains(domains);

            return domains;
        }


        /// <summary>
        /// Checks if array of domains contains two domains with same domain name, which is invalid.        
        /// </summary>
        /// <exception cref="InvalidOperationException">If such duplicity exists exception is thrown.</exception>
        /// <param name="domains">Array of domain names</param>
        private void CheckForDuplicitDomains(string[] domains)
        {
            int i;
            for (i = 0; i < domains.Length - 1; i++)
            {
                for (int j = i + 1; j < domains.Length; j++)
                {
                    if (domains[i] == domains[j])
                    {
                        throw new InvalidOperationException(CoreServices.Localization.GetString("RentedLicense.MoreLicenses"));
                    }
                }
            }
        }


        /// <summary>
        /// Returns the DateTime representation of a US short date string.
        /// </summary>
        /// <param name="value">Value to convert.</param>   
        private static DateTime ParseDateTime(string value)
        {
            // License key expiration are stored in US short date format, e.g. 3/27/2014
            return DateTime.ParseExact(value, LicenseKeyInfo.LICENSE_EXPIRATION_DATE_FORMAT, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Updates license keys - deletes old ones and adds new.        
        /// </summary>
        /// <param name="domains">Array of domain licenses</param>
        /// <returns>Number of updated keys.</returns>
        private int UpdateLicenseKeys(IEnumerable<DomainLicense> domains)
        {
            int updatedCount = 0;
            if (domains != null)
            {
                foreach (DomainLicense license in domains)
                {
                    if (!String.IsNullOrEmpty(license.LicenseKey))
                    {
                        updatedCount++;

                        // Use transaction for each delete-add
                        using (var tr = new CMSTransactionScope())
                        {
                            // Delete old license key
                            var lki = LicenseKeyInfoProvider.GetLicenseKeyInfo(license.Domain);

                            LicenseKeyInfoProvider.DeleteLicenseKeyInfo(lki);

                            // Insert new license key
                            var newKey = new LicenseKeyInfo();
                            newKey.LoadLicense(license.LicenseKey, "");

                            LicenseKeyInfoProvider.SetLicenseKeyInfo(newKey);

                            tr.Commit();
                        }
                    }
                }

                ModuleManager.ClearHashtables();
            }

            return updatedCount;
        }


        /// <summary>
        /// Gets licenses and their expiration for specified domains.
        /// </summary>
        /// <param name="domains">Array of domains.</param>
        /// <returns>Dictionary for fast search, domain as key, DataRow as value</returns>
        private Dictionary<string, DataRow> GetCurrentLicensesWithExpiration(IEnumerable<string> domains)
        {
            // Limit license keys to those under rented license
            string where = SqlHelper.GetWhereCondition("LicenseDomain", domains);
            DataSet ds = ConnectionHelper.ExecuteQuery("cms.licensekey.selectall", null, where, null, 0, "LicenseDomain, LicenseKeyID, LicenseExpiration");

            // Make dictionary for searching by domain name
            Dictionary<string, DataRow> licenses = new Dictionary<string, DataRow>();
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    licenses.Add(dr["LicenseDomain"] as string, dr);
                }
            }

            return licenses;
        }


        /// <summary>
        /// Returns CMS version as int, to be able to use it for generating the license.
        /// Based on CMSVersion.NUMBER
        /// </summary>        
        private int GetSystemVersion()
        {
            return CMSVersion.Version.Major;
        }

        #endregion
    }
}