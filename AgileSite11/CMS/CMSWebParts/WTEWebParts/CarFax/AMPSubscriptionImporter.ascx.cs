using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;

using CMS.Helpers;
using CMS.PortalEngine;
using CMS.PortalEngine.Web.UI;
using CMS.Membership;

namespace CMSApp.CMSWebParts.WTEWebParts.CarFax
{
    public partial class AMPSubscriptionImporter : CMSAbstractWebPart
    {
        #region enum

        /// <summary>
        /// Subscriber list type
        /// </summary>
        public enum SubscriberDataType
        {
            /// <summary>
            /// Imported data
            /// </summary>
            Imported = 0,

            /// <summary>
            /// Existing
            /// </summary>
            Existing = 1,

            /// <summary>
            /// Opt out
            /// </summary>
            OptOut = 2,

            /// <summary>
            /// Assigned to newsletter list
            /// </summary>
            NewsLetterList = 3
        }

        #endregion enum

        #region classes

        /// <summary>
        /// Base data handler
        /// </summary>
        public class WTEDataHandler
        {
            #region methods

            /// <summary>
            /// Get string value from a data column
            /// </summary>
            /// <param name="p_row"></param>
            /// <param name="p_columnName"></param>
            /// <param name="p_defaultValue"></param>
            /// <param name="p_IsValid"></param>
            /// <param name="p_message"></param>
            /// <returns></returns>
            public string GetDataColumnString(DataRow p_row, string p_columnName, string p_defaultValue, out bool p_IsValid, out string p_message)
            {
                string value = String.Empty;
                p_message = String.Empty;
                p_IsValid = true;

                Object data = null;

                try
                {
                    data = p_row[p_columnName];
                }
                catch (Exception ex)
                {
                    // do nothing
                    p_IsValid = false;
                    p_message = ex.Message;
                }

                // process the data
                if (data != null)
                {
                    value = data.ToString();
                }
                else
                {
                    value = p_defaultValue;
                    p_IsValid = false;
                    if (!String.IsNullOrWhiteSpace(p_message))
                    {
                        p_message = "Column is NULL";
                    }
                }

                return value;
            }

            /// <summary>
            /// Get int value from a data column
            /// </summary>
            /// <param name="p_row"></param>
            /// <param name="p_columnName"></param>
            /// <param name="p_defaultValue"></param>
            /// <param name="p_IsValid"></param>
            /// <param name="p_message"></param>
            /// <returns></returns>
            public int GetDataColumnInt(DataRow p_row, string p_columnName, int p_defaultValue, out bool p_IsValid, out string p_message)
            {
                int value = 0;
                p_message = String.Empty;
                p_IsValid = true;

                Object data = null;

                try
                {
                    data = p_row[p_columnName];
                }
                catch (Exception ex)
                {
                    // do nothing
                    p_IsValid = false;
                    p_message = ex.Message;
                }

                // process the data
                if (data != null)
                {
                    if (!int.TryParse(data.ToString(), out value))
                    {
                        p_IsValid = false;
                        value = p_defaultValue;
                        p_message = "Wrong data type";
                    }
                }
                else
                {
                    value = p_defaultValue;
                    p_IsValid = false;
                    if (!String.IsNullOrWhiteSpace(p_message))
                    {
                        p_message = "Column is NULL";
                    }
                }

                return value;
            }

            #endregion methods
        }

        /// <summary>
        /// Import statistic helper
        /// </summary>
        public class ImportStatisticData
        {
            #region members

            private int _totalRead = 0;
            private int _badRecords = 0;
            private int _totalImported = 0;

            private int _existingSubscribers = 0;
            private int _optOutSubscribers = 0;
            private int _duplicatedSubscribers = 0;

            private int _totalAssignedToNewsletterList = 0;
            private int _inSelectedNewsletterList = 0;
            private int _needAssignedToNewsletterList = 0;

            private bool _showImportCount = false;
            private bool _showFullStastic = false;

            #endregion members

            #region properties

            /// <summary>
            /// Total number of records imported
            /// </summary>
            public int TotalRead
            {
                get
                {
                    return _totalRead;
                }
                set
                {
                    _totalRead = value;
                }
            }

            /// <summary>
            /// Bad records
            /// </summary>
            public int BadRecords
            {
                get
                {
                    return _badRecords;
                }
                set
                {
                    _badRecords = value;
                }
            }

            /// <summary>
            /// Total imported (added to the DB)
            /// </summary>
            public int TotalImported
            {
                get
                {
                    return _totalImported;
                }
                set
                {
                    _totalImported = value;
                }
            }

            /// <summary>
            /// Existing records
            /// </summary>
            public int ExistingSubscribers
            {
                get
                {
                    return _existingSubscribers;
                }
                set
                {
                    _existingSubscribers = value;
                }
            }

            /// <summary>
            /// duplicated records
            /// </summary>
            public int DuplicatedSubscribers
            {
                get
                {
                    return _duplicatedSubscribers;
                }
                set
                {
                    _duplicatedSubscribers = value;
                }
            }

            /// <summary>
            /// Opted out recrods
            /// </summary>
            public int OptOutSubscribers
            {
                get
                {
                    return _optOutSubscribers;
                }
                set
                {
                    _optOutSubscribers = value;
                }
            }

            /// <summary>
            /// Total subscriber assigned to selected newletter list
            /// </summary>
            public int TotalAssignedToNewsletterList
            {
                get
                {
                    return _totalAssignedToNewsletterList;
                }
                set
                {
                    _totalAssignedToNewsletterList = value;
                }
            }

            /// <summary>
            /// Total subscribers already in the selected list
            /// </summary>
            public int InSelectedNewsletterList
            {
                get
                {
                    return _inSelectedNewsletterList;
                }
                set
                {
                    _inSelectedNewsletterList = value;
                }
            }

            /// <summary>
            /// Total number of subscribers needs to be assigned to the list
            /// </summary>
            public int NeedAssignedToNewsletterList
            {
                get
                {
                    return _needAssignedToNewsletterList;
                }
                set
                {
                    _needAssignedToNewsletterList = value;
                }
            }

            /// <summary>
            /// Show full static
            /// </summary>
            public bool ShowFullStatistic
            {
                get
                {
                    return _showFullStastic;
                }
                set
                {
                    _showFullStastic = value;
                }
            }

            /// <summary>
            /// Show import count
            /// </summary>
            public bool ShowImportCount
            {
                get
                {
                    return _showImportCount;
                }
                set
                {
                    _showImportCount = value;
                }
            }

            #endregion properties

            #region constructor

            /// <summary>
            /// Constructor
            /// </summary>
            public ImportStatisticData()
            {
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="p_subscriberList"></param>
            /// <param name="p_showFullStat"></param>
            /// <param name="p_showImportCount"></param>
            public ImportStatisticData(List<SubscriberDataRow> p_subscriberList, bool p_showFullStat, bool p_showImportCount)
            {
                _showFullStastic = p_showFullStat;
                _showImportCount = p_showImportCount;

                if (p_subscriberList != null)
                {
                    _totalRead = p_subscriberList.Count;

                    List<SubscriberDataRow> optouts = p_subscriberList.FindAll(delegate(SubscriberDataRow s)
                    {
                        return s.IsOptOut;
                    });

                    _optOutSubscribers = optouts.Count;

                    List<SubscriberDataRow> existing = p_subscriberList.FindAll(delegate(SubscriberDataRow s)
                    {
                        return s.Existing;
                    });

                    _existingSubscribers = existing.Count;

                    List<SubscriberDataRow> invalid = p_subscriberList.FindAll(delegate(SubscriberDataRow s)
                    {
                        return s.IsBadDataRow;
                    });

                    _badRecords = invalid.Count;

                    List<SubscriberDataRow> duplicated = p_subscriberList.FindAll(delegate(SubscriberDataRow s)
                    {
                        return s.HasDuplicate;
                    });

                    _duplicatedSubscribers = duplicated.Count;

                    List<SubscriberDataRow> assigned = p_subscriberList.FindAll(delegate(SubscriberDataRow s)
                    {
                        return s.InNewsletterList;
                    });

                    _inSelectedNewsletterList = assigned.Count;

                    List<SubscriberDataRow> needassigned = p_subscriberList.FindAll(delegate(SubscriberDataRow s)
                    {
                        return !s.InNewsletterList
                            && !s.HasDuplicate
                            && !s.IsBadDataRow
                            && !s.IsOptOut;
                    });

                    _needAssignedToNewsletterList = needassigned.Count;

                    List<SubscriberDataRow> inserted = p_subscriberList.FindAll(delegate(SubscriberDataRow s)
                    {
                        return !s.HasDuplicate
                            && !s.Existing
                            && !s.IsBadDataRow
                            && !s.IsOptOut;
                    });

                    _totalImported = inserted.Count;
                    _totalAssignedToNewsletterList = _needAssignedToNewsletterList + _totalImported;
                }
            }

            #endregion constructor

            #region methods

            /// <summary>
            /// To string method
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                string info = String.Empty;

                if (TotalRead > 0)
                {
                    info += "Subscribers successfully imported. Note: Subscribers are automatically de-duplicated and checked for being in a valid format.";
                }
                else
                {
                    info += "No valid subscribers record to import.";
                }

                if (ShowImportCount)
                {
                    info += "</br>";
                    info += TotalImported.ToString() + " New Subscribers Imported. (" + TotalRead.ToString() + " Processed) </br>";
                    //info += TotalAssignedToNewsletterList.ToString() + " Subscribers On Newsletter List.</br>";
                    info += String.Format("Bad Rows (invalid) : {0}<br/>", BadRecords);
                    info += String.Format("Duplicated Rows    : {0}<br/>", DuplicatedSubscribers);
                }

                if (ShowFullStatistic)
                {
                    info += String.Format("Row Processed      : {0}<br/>", TotalRead);
                    info += String.Format("Bad Rows (invalid) : {0}<br/>", BadRecords);
                    info += String.Format("Duplicated Rows    : {0}<br/>", DuplicatedSubscribers);

                    info += String.Format("Existing Subscribers  : {0}<br/>", ExistingSubscribers);
                    info += String.Format("Opt out Subscribers   : {0}<br/>", OptOutSubscribers);

                    info += String.Format("Total New Subscribers : {0}<br/>", TotalImported);
                    info += String.Format("Already in selected Newsletter   : {0}<br/>", InSelectedNewsletterList);
                    info += String.Format("Need to be assigned to newsletter: {0}<br/>", NeedAssignedToNewsletterList);
                    info += String.Format("Total Assigned to newsletter     : {0}<br/>", TotalAssignedToNewsletterList);
                }

                return info;
            }

            #endregion methods
        }

        /// <summary>
        /// Subscriber data row data
        /// </summary>
        public class SubscriberDataRow : WTEDataHandler
        {
            #region members

            private int _subscriberId = 0;
            private string _name = String.Empty;
            private string _emailAddress = String.Empty;
            private string _firstName = String.Empty;
            private string _lastName = String.Empty;
            private string _systemNote = String.Empty;
            private int _siteId = 0;
            private int _newsletterListId = 0;

            private bool _existing = false;
            private bool _InNewsletterList = false;
            private bool _isBadDataRow = false;
            private bool _isOptOut = false;
            private bool _hasDuplicate = false;
            private List<String> _vins = new List<string>();

            #endregion members

            #region properties

            /// <summary>
            /// Existing subscriber ID
            /// </summary>
            public int SubscriberId
            {
                get
                {
                    return _subscriberId;
                }
                set
                {
                    _subscriberId = value;
                }
            }

            /// <summary>
            /// Email address
            /// </summary>
            public string EmailAddress
            {
                get
                {
                    return _emailAddress;
                }
                set
                {
                    _emailAddress = value;
                }
            }

            /// <summary>
            /// Name?
            /// </summary>
            public string Name
            {
                get
                {
                    return _name;
                }
                set
                {
                    _name = value;
                }
            }

            /// <summary>
            /// First Name
            /// </summary>
            public string FirstName
            {
                get
                {
                    return _firstName;
                }
                set
                {
                    _firstName = value;
                }
            }

            /// <summary>
            /// Last Name
            /// </summary>
            public string LastName
            {
                get
                {
                    return _lastName;
                }
                set
                {
                    _lastName = value;
                }
            }

            /// <summary>
            /// System note (Invalid items etc)
            /// </summary>
            public string SystemNote
            {
                get
                {
                    return _systemNote;
                }
                set
                {
                    _systemNote = value;
                }
            }

            /// <summary>
            /// The dealer's site ID in AMP
            /// </summary>
            public int SiteId
            {
                get
                {
                    return _siteId;
                }
                set
                {
                    _siteId = value;
                }
            }

            /// <summary>
            /// News letter list to create/assign
            /// </summary>
            public int NewsLetterListId
            {
                get
                {
                    return _newsletterListId;
                }
                set
                {
                    _newsletterListId = value;
                }
            }

            /// <summary>
            /// Flag indicated a duplicate record
            /// </summary>
            public bool Existing
            {
                get
                {
                    return _existing;
                }
                set
                {
                    _existing = value;
                }
            }

            /// <summary>
            /// Subscriber already assigned to the selecte news letter list
            /// </summary>
            public bool InNewsletterList
            {
                get
                {
                    return _InNewsletterList;
                }
                set
                {
                    _InNewsletterList = value;
                }
            }

            /// <summary>
            /// Flag indicated a duplicate record
            /// </summary>
            public bool HasDuplicate
            {
                get
                {
                    return _hasDuplicate;
                }
                set
                {
                    _hasDuplicate = value;
                }
            }

            /// <summary>
            /// Flag indicated bad data
            /// </summary>
            public bool IsBadDataRow
            {
                get
                {
                    return _isBadDataRow;
                }
                set
                {
                    _isBadDataRow = value;
                }
            }

            /// <summary>
            /// Is Opt Out
            /// </summary>
            public bool IsOptOut
            {
                get
                {
                    return _isOptOut;
                }
                set
                {
                    _isOptOut = value;
                }
            }

            /// <summary>
            /// VIN
            /// </summary>
            public List<string> VINs
            {
                get
                {
                    return _vins;
                }
                set
                {
                    _vins = value;
                }
            }

            #endregion properties

            #region constructor

            /// <summary>
            /// Create a note Item
            /// </summary>
            /// <param name="p_systemNote"></param>
            public SubscriberDataRow(string p_systemNote)
            {
                _systemNote = p_systemNote;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="p_email"></param>
            /// <param name="p_first"></param>
            /// <param name="p_last"></param>
            public SubscriberDataRow(string p_email, string p_first, string p_last)
            {
                _emailAddress = p_email;
                _firstName = p_first;
                _lastName = p_last;
            }

            /// <summary>
            /// constructor using data row.
            /// </summary>
            /// <param name="p_subscriberData"></param>
            public SubscriberDataRow(DataRow p_subscriberData)
                : this(p_subscriberData, SubscriberDataType.Imported, 0, 0)
            {
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="p_subscriberData"></param>
            /// <param name="p_type"></param>
            public SubscriberDataRow(DataRow p_subscriberData, SubscriberDataType p_type)
                : this(p_subscriberData, p_type, 0, 0)
            {
            }

            /// <summary>
            /// Export by type
            /// </summary>
            /// <param name="p_subscriberData"></param>
            /// <param name="p_type"></param>
            public SubscriberDataRow(DataRow p_subscriberData, SubscriberDataType p_type, int p_siteID, int p_newsletterlistID)
            {
                if (p_subscriberData != null)
                {
                    switch (p_type)
                    {
                        case SubscriberDataType.OptOut:
                            {
                                string errorMessage = String.Empty;
                                bool isValid = true;
                                _siteId = GetDataColumnInt(p_subscriberData, "SiteID", p_siteID, out isValid, out errorMessage);
                                _newsletterListId = GetDataColumnInt(p_subscriberData, "NewsLetterListID", p_newsletterlistID, out isValid, out errorMessage);
                                _subscriberId = GetDataColumnInt(p_subscriberData, "SubscriberID", 0, out isValid, out errorMessage);
                                _firstName = GetDataColumnString(p_subscriberData, "FirstName", String.Empty, out isValid, out errorMessage);
                                _lastName = GetDataColumnString(p_subscriberData, "LastName", String.Empty, out isValid, out errorMessage);
                                _name = GetDataColumnString(p_subscriberData, "Name", String.Empty, out isValid, out errorMessage);
                                _emailAddress = GetDataColumnString(p_subscriberData, "Email", String.Empty, out isValid, out errorMessage);
                            }
                            break;
                        case SubscriberDataType.NewsLetterList:
                        case SubscriberDataType.Existing:
                            {
                                string errorMessage = String.Empty;
                                bool isValid = true;
                                _siteId = GetDataColumnInt(p_subscriberData, "SiteID", p_siteID, out isValid, out errorMessage);
                                _newsletterListId = GetDataColumnInt(p_subscriberData, "NewsLetterListID", p_newsletterlistID, out isValid, out errorMessage);
                                _subscriberId = GetDataColumnInt(p_subscriberData, "SubscriberID", 0, out isValid, out errorMessage);
                                _firstName = GetDataColumnString(p_subscriberData, "FirstName", String.Empty, out isValid, out errorMessage);
                                _lastName = GetDataColumnString(p_subscriberData, "LastName", String.Empty, out isValid, out errorMessage);
                                _name = GetDataColumnString(p_subscriberData, "Name", String.Empty, out isValid, out errorMessage);
                                _emailAddress = GetDataColumnString(p_subscriberData, "Email", String.Empty, out isValid, out errorMessage);
                                SetVINData(p_subscriberData, out isValid, out errorMessage);
                            }
                            break;

                        case SubscriberDataType.Imported:
                        default:
                            {
                                string errorMessage = String.Empty;
                                bool isValid = true;
                                _siteId = GetDataColumnInt(p_subscriberData, "SiteID", p_siteID, out isValid, out errorMessage);
                                _newsletterListId = GetDataColumnInt(p_subscriberData, "NewsLetterListID", p_newsletterlistID, out isValid, out errorMessage);
                                _subscriberId = GetDataColumnInt(p_subscriberData, "SubscriberID", 0, out isValid, out errorMessage);
                                _firstName = GetDataColumnString(p_subscriberData, "first", String.Empty, out isValid, out errorMessage);
                                _lastName = GetDataColumnString(p_subscriberData, "last", String.Empty, out isValid, out errorMessage);

                                //_name = GetDataColumnString(p_subscriberData, "Name", String.Empty, out isValid, out errorMessage);
                                _emailAddress = GetDataColumnString(p_subscriberData, "Email", String.Empty, out isValid, out errorMessage);
                                SetVINData(p_subscriberData, out isValid, out errorMessage);
                            }
                            break;
                    }

                    // validation
                    if (String.IsNullOrWhiteSpace(EmailAddress))
                    {
                        _isBadDataRow = true;
                        _systemNote = "Invalid Email";
                    }
                }
            }

            #endregion constructor

            #region methods

            public static string _columnTemplate = "| {0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8} | {9} | {10} | {11} | {12} |<br/>";

            /// <summary>
            /// To string method
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                string vinString = GetVINsString();
                return String.Format(_columnTemplate, SubscriberId, SiteId, NewsLetterListId,
                    !String.IsNullOrWhiteSpace(EmailAddress) ? EmailAddress : "N/A"
                    , !String.IsNullOrWhiteSpace(Name) ? Name : "N/A"
                    , !String.IsNullOrWhiteSpace(FirstName) ? FirstName : "N/A"
                    , !String.IsNullOrWhiteSpace(LastName) ? LastName : "N/A"
                    , !String.IsNullOrWhiteSpace(vinString) ? vinString : "N/A"
                    , IsOptOut.ToString()
                    , Existing.ToString(), HasDuplicate.ToString()
                    , IsBadDataRow.ToString()
                    , !String.IsNullOrWhiteSpace(SystemNote) ? SystemNote : "N/A");
            }

            /// <summary>
            /// Get Column Header
            /// </summary>
            /// <returns></returns>
            public static string GetColumnHeader()
            {
                return String.Format(_columnTemplate, "SiteID", "NewsLetterListID", "SubscriberID", "Email"
                    , "Name", "FirstName", "LastName", "VIN(s)", "OptOut"
                    , "Existing", "Duplicated", "Invalid", "Notes");
            }

            /// <summary>
            /// Get VINs as a string
            /// </summary>
            public string GetVINsString()
            {
                string ret = String.Empty;
                if (_vins != null)
                {
                    foreach (string vin in _vins)
                    {
                        if (!String.IsNullOrWhiteSpace(ret))
                        {
                            ret += ", ";
                        }
                        if (!String.IsNullOrWhiteSpace(vin))
                        {
                            ret += vin;
                        }
                    }
                }
                return ret;
            }

            /// <summary>
            /// Set VIN data
            /// </summary>
            /// <param name="p_subscriberData"></param>
            /// <returns></returns>
            protected void SetVINData(DataRow p_subscriberData, out bool p_IsValid, out string p_message)
            {
                string value = String.Empty;
                p_message = String.Empty;
                p_IsValid = true;

                // Check for 8 VIN number
                for (int i = 1; i <= 8; i++)
                {
                    string tempColName = String.Format("VIN{0}", i);
                    string tempVin = GetDataColumnString(p_subscriberData, tempColName, String.Empty, out p_IsValid, out p_message);
                    if (!String.IsNullOrWhiteSpace(tempVin))
                    {
                        _vins.Add(tempVin);
                    }
                }
            }

            #endregion methods
        }

        /// <summary>
        /// NewsletterList data row
        /// </summary>
        public class NewsLetterListDataRow : WTEDataHandler
        {
            #region member

            private int _siteID = 0;
            private int _NewsLetterListID = 0;
            private string _Name = String.Empty;
            private string _DealerName = String.Empty;
            private string _DealerEmail = String.Empty;
            private int _subscriberCount = 0;

            #endregion member

            #region properties

            /// <summary>
            /// The site ID
            /// </summary>
            public int SiteID
            {
                get
                {
                    return _siteID;
                }
                set
                {
                    _siteID = value;
                }
            }

            /// <summary>
            /// The news letter list ID
            /// </summary>
            public int NewsLetterListID
            {
                get
                {
                    return _NewsLetterListID;
                }
                set
                {
                    _NewsLetterListID = value;
                }
            }

            /// <summary>
            /// The news letter list name
            /// </summary>
            public string Name
            {
                get
                {
                    return _Name;
                }
                set
                {
                    _Name = value;
                }
            }

            /// <summary>
            /// The dealer's name
            /// </summary>
            public string DealerName
            {
                get
                {
                    return _DealerName;
                }
                set
                {
                    _DealerName = value;
                }
            }

            /// <summary>
            /// The dealer's name
            /// </summary>
            public string DealerEmail
            {
                get
                {
                    /// note this should be UserName + @CARFAXMARKETING.COM
                    return _DealerEmail;
                }
                set
                {
                    _DealerEmail = value;
                }
            }

            /// <summary>
            /// Subscriber count
            /// </summary>
            public int SubscriberCount
            {
                get
                {
                    return _subscriberCount;
                }
                set
                {
                    _subscriberCount = value;
                }
            }

            /// <summary>
            /// Display name with count
            /// </summary>
            public string DisplayName
            {
                get
                {
                    return String.Format("{0} ({1})", Name, SubscriberCount);
                }
            }

            #endregion properties

            #region constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="p_siteID"></param>
            /// <param name="p_newsLetterListID"></param>
            /// <param name="p_name"></param>
            public NewsLetterListDataRow(int p_siteID, int p_newsLetterListID, string p_name)
            {
                _Name = p_name;
                _NewsLetterListID = p_newsLetterListID;
                _siteID = p_siteID;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="p_newsLetterListRow"></param>
            public NewsLetterListDataRow(DataRow p_newsLetterListRow)
            {
                if (p_newsLetterListRow != null)
                {
                    bool isvalid;
                    string message;
                    _Name = GetDataColumnString(p_newsLetterListRow, "Name", String.Empty, out isvalid, out message);
                    _NewsLetterListID = GetDataColumnInt(p_newsLetterListRow, "NewsLetterListID", 0, out isvalid, out message);
                    _siteID = GetDataColumnInt(p_newsLetterListRow, "SiteID", 0, out isvalid, out message);
                    _subscriberCount = GetDataColumnInt(p_newsLetterListRow, "SubscriberCounts", 0, out isvalid, out message);
                }
            }

            #endregion constructor
        }

        #endregion classes

        #region properties

        #region custom data fields from CMS

        /// <summary>
        /// Show full export details
        /// </summary>
        protected bool ShowFullImportDetails
        {
            get
            {
                return GetBoolObjectValue("ShowFullImportDetails", true);
            }
        }

        /// <summary>
        /// Hide if no template selected property from the CMS
        /// </summary>
        protected bool HideIfNoTemplateSelected
        {
            get
            {
                return GetBoolObjectValue("HideIfNoSelectedTemplate", true);
            }
        }

        /// <summary>
        /// The redirect url property from CMS
        /// </summary>
        protected string RedirectUrlTemplate
        {
            get
            {
                string redirectTemplate = GetStringObjectValue("RedirectPageUrlTemplate", "/Send-Email.aspx?template={0}");

                if (!redirectTemplate.Contains("{0}"))
                {
                    redirectTemplate = "/Send-Email.aspx?template={0}";
                }

                return redirectTemplate;
            }
        }

        /// <summary>
        /// The query string to access the selected id
        /// </summary>
        protected string TemplateIDQueryStringKey
        {
            get
            {
                string temp = GetStringObjectValue("TemplateIDQueryStringKey", "template");
                return temp;
            }
        }

        /// <summary>
        /// Show import statistic
        /// </summary>
        protected bool ShowFullImportStatistic
        {
            get
            {
                return GetBoolObjectValue("ShowFullImportStatistic", false);
            }
        }

        /// <summary>
        /// Show Import Count
        /// </summary>
        protected bool ShowImportCount
        {
            get
            {
                return GetBoolObjectValue("ShowImportCount", false);
            }
        }

        /// <summary>
        /// Allow adding to existing newsletter list
        /// </summary>
        protected bool AllowInsertToNewsletterList
        {
            get
            {
                return GetBoolObjectValue("AllowInsertToNewsletterList", false);
            }
        }

        #endregion custom data fields from CMS

        #region query string

        /// <summary>
        /// Check to see if the user selected a template
        /// </summary>
        protected bool HasTemplateSelected
        {
            get
            {
                return Request.Params[TemplateIDQueryStringKey] != null;
            }
        }

        /// <summary>
        /// The selected template ID from the Query string
        /// </summary>
        protected int SelectedTemplateID
        {
            get
            {
                int id = 0;

                if (HasTemplateSelected)
                {
                    int.TryParse(SelectedTemplateIDString, out id);
                }

                return id;
            }
        }

        /// <summary>
        /// The selected template ID from the Query string
        /// </summary>
        protected string SelectedTemplateIDString
        {
            get
            {
                if (HasTemplateSelected)
                {
                    return Request.Params[TemplateIDQueryStringKey].ToString();
                }
                return String.Empty;
            }
        }

        #endregion query string

        #region site properties

        /// <summary>
        /// The AMP site id
        /// </summary>
        protected int AMPSiteID
        {
            get
            {
                CurrentUserInfo info = MembershipContext.AuthenticatedUser;
                string GUID = info.UserGUID.ToString();
                string UserName = info.UserName.ToString();
                string userAmpNum = info.GetStringValue("UserAmpNum", String.Empty);
                string dealerName = info.GetStringValue("UserDealer", String.Empty);

                int siteId = 0;
                int.TryParse(userAmpNum, out siteId);

                if (siteId == 0)
                {
                    //bool output = true;

                    // see if it's there when we refresh
                    MembershipContext.Clear();
                    info = MembershipContext.AuthenticatedUser;
                    userAmpNum = info.GetStringValue("UserAmpNum", String.Empty);
                    int.TryParse(userAmpNum, out siteId);
                }

                if (siteId == 0)
                {
                    siteId = GetDealerAMPSiteIDByUserName(dealerName);
                }

                if (siteId == 0)
                {
                    siteId = GetDealerAMPSiteIDByUserName(UserName);
                }

                if (siteId <= 0)
                {
                    siteId = 1;
                }

                return siteId;
            }
        }

        #endregion site properties

        #region other properties

        /// <summary>
        /// The selected newsletter ID
        /// </summary>
        public int SelectedNewsLetterID
        {
            get
            {
                int id = 0;

                if (ViewState["WTEuploaderSelectedNewsLetterListID"] != null)
                {
                    int.TryParse(ViewState["WTEuploaderSelectedNewsLetterListID"].ToString(), out id);
                }

                return id;
            }
            set
            {
                ViewState["WTEuploaderSelectedNewsLetterListID"] = value;
            }
        }

        #endregion other properties

        #endregion properties

        #region Page Events

        /// <summary>
        /// The page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            string param = String.Empty;

            if (!HideIfNoTemplateSelected || HasTemplateSelected)
            {
                divUploaderMain.Visible = true;
                btnContinue.Visible = false;
                if (!IsPostBack)
                {
                    BindNewsLetterListDropDown(AMPSiteID);
                }
            }
            else
            {
                // must have a param to be an upload page
                divUploaderMain.Visible = false;
                btnContinue.Visible = false;
            }
        }

        #endregion Page Events

        #region data binding

        /// <summary>
        /// Bind the drop down
        /// </summary>
        /// <param name="p_siteId"></param>
        protected void BindNewsLetterListDropDown(int p_siteId)
        {
            List<NewsLetterListDataRow> newsLetterList = GetExistingNewsletterList(p_siteId);

            ddlSelectNewsLetter.DataSource = newsLetterList;
            ddlSelectNewsLetter.DataTextField = "DisplayName";
            ddlSelectNewsLetter.DataValueField = "NewsLetterListID";
            ddlSelectNewsLetter.DataBind();
            ddlSelectNewsLetter.Items.Insert(0, new ListItem(" - Create New List -", "0"));
            ddlSelectNewsLetter.SelectedValue = "0";
        }

        #endregion data binding

        #region general events

        /// <summary>
        /// Upload button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnUploadClicked(object sender, EventArgs e)
        {
            // blank out status message
            System.Threading.Thread.Sleep(3000);
            litMessage.Text = "processing...";
            try
            {
                rfvFileName.Enabled = true;
                rfvFileName.Validate();

                int siteID = AMPSiteID;
                SelectedNewsLetterID = GetImportNewsLetterListID(siteID);

                if (rfvFileName.IsValid)
                {
                    if (SelectedNewsLetterID >= 0)
                    {
                        string uploadFilePath = UploadFile(String.Empty);
                        if (!String.IsNullOrWhiteSpace(uploadFilePath))
                        {
                            List<DataTable> fileDataTables = ReadExcel(uploadFilePath, true);

                            // existing subscribers on the Subscriber list
                            List<SubscriberDataRow> existingSubs = GetExistingSubscribers(siteID);

                            // opt out subscribers
                            List<SubscriberDataRow> optOutSubs = GetOptOutList(siteID);

                            // Selected news letter subscribers
                            List<SubscriberDataRow> nLSubs = GetNewsletterListSubscriber(SelectedNewsLetterID);

                            List<SubscriberDataRow> importedData = GetImportedData(fileDataTables, existingSubs, optOutSubs, nLSubs);
                            InsertSubscribers(importedData, SelectedNewsLetterID, siteID);
                            ShowStatus(importedData);
                        }
                        else
                        {
                            // no file selected, error message already set.
                            ShowContinueButton(false);
                        }
                    }
                }
                else
                {
                    // we have an error
                    // error message already set
                    ShowContinueButton(false);
                }
            }
            catch (Exception ex)
            {
                litMessage.Text = "An error had occurred please try again: " + ex.Message;

                //btnContinue.Visible = false;
                ShowContinueButton(false);
            }
        }

        /// <summary>
        /// Continue button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnContinueClicked(object sender, EventArgs e)
        {
            string redirectToURL = String.Format(RedirectUrlTemplate, SelectedTemplateIDString);
            int selectedID = SelectedNewsLetterID;

            if (selectedID <= 0)
            {
                // pull it from the selection
                int.TryParse(ddlSelectNewsLetter.SelectedValue, out selectedID);
            }

            redirectToURL += "?list=" + selectedID;

            URLHelper.Redirect(ResolveUrl(redirectToURL));
        }

        /// <summary>
        /// On selected index changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnSelectNewsLetterIndexChanged(object sender, EventArgs e)
        {
            if (ddlSelectNewsLetter.SelectedValue == "0")
            {
                divNewsletterName.Visible = true;
                divImportUpload.Visible = true;
            }
            else
            {
                divNewsletterName.Visible = false;

                if (AllowInsertToNewsletterList)
                {
                    divImportUpload.Visible = true;
                }
                else
                {
                    divImportUpload.Visible = false;
                }
            }

            ShowContinueButton(false);
        }

        #endregion general events

        #region subscriber mapping

        /// <summary>
        /// Show/Hide Continue button with a check for existing newsletter list
        /// </summary>
        /// <param name="p_show"></param>
        protected void ShowContinueButton(bool p_show)
        {
            if (ddlSelectNewsLetter.SelectedValue == "0")
            {
                btnContinue.Visible = p_show;
                rfvFileName.Enabled = true;
            }
            else
            {
                // we can always continue, if an existing newsletter is selected.
                btnContinue.Visible = true;
                if (!p_show)
                {
                    rfvFileName.Enabled = true;
                }
                else
                {
                    rfvFileName.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Get Subscriber list as string
        /// </summary>
        /// <param name="p_subscribers"></param>
        /// <param name="p_newsletterlistid"></param>
        /// <param name="p_siteid"></param>
        /// <returns></returns>
        protected void ShowStatus(List<SubscriberDataRow> p_subscribers)
        {
            ImportStatisticData stat = new ImportStatisticData(p_subscribers, ShowFullImportStatistic, ShowImportCount);
            litMessage.Text = stat.ToString();

            if (ShowFullImportDetails && stat.TotalRead > 0)
            {
                litMessage.Text += SubscriberDataRow.GetColumnHeader();
                foreach (SubscriberDataRow row in p_subscribers)
                {
                    litMessage.Text += row.ToString();
                }
            }

            if (stat.TotalRead == 0)
            {
                // we had an error
                ShowContinueButton(false);
            }
            else
            {
                // at least one item was imported show continue button, hide the upload
                divImportUpload.Visible = false;
                divSelectNewsLetterList.Visible = false;
                ShowContinueButton(true);
            }
        }

        /// <summary>
        /// Get Subscriber data
        /// </summary>
        /// <param name="p_importedDataTables"></param>
        /// <param name="p_existing"></param>
        /// <param name="p_optOut"></param>
        /// <param name="p_inNewsletterList"></param>
        /// <returns></returns>
        protected List<SubscriberDataRow> GetImportedData(List<DataTable> p_importedDataTables, List<SubscriberDataRow> p_existing, List<SubscriberDataRow> p_optOut, List<SubscriberDataRow> p_inNewsletterList)
        {
            List<SubscriberDataRow> importedData = new List<SubscriberDataRow>();

            if (p_importedDataTables != null)
            {
                foreach (DataTable fdt in p_importedDataTables)
                {
                    if (fdt != null)
                    {
                        if (fdt.Rows.Count > 0)
                        {
                            foreach (DataRow row in fdt.Rows)
                            {
                                SubscriberDataRow subInfo = null;

                                try
                                {
                                    subInfo = new SubscriberDataRow(row);
                                }
                                catch (Exception)
                                {
                                    // bad data ignore it
                                    subInfo = new SubscriberDataRow("Invalid format");
                                    subInfo.IsBadDataRow = true;
                                }

                                if (subInfo != null)
                                {
                                    if (!subInfo.IsBadDataRow)
                                    {
                                        SubscriberDataRow optout = p_optOut.Find(delegate(SubscriberDataRow s)
                                        {
                                            return s.EmailAddress.ToLower() == subInfo.EmailAddress.ToLower();
                                        });

                                        if (optout != null)
                                        {
                                            subInfo.IsOptOut = true;
                                        }

                                        SubscriberDataRow existInImport = importedData.Find(delegate(SubscriberDataRow s)
                                        {
                                            return s.EmailAddress.ToLower() == subInfo.EmailAddress.ToLower();
                                        });

                                        if (existInImport != null)
                                        {
                                            subInfo.HasDuplicate = true;
                                        }

                                        SubscriberDataRow existInSubscriberList = p_existing.Find(delegate(SubscriberDataRow s)
                                        {
                                            return s.EmailAddress.ToLower() == subInfo.EmailAddress.ToLower();
                                        });

                                        if (existInSubscriberList != null)
                                        {
                                            subInfo.SubscriberId = existInSubscriberList.SubscriberId;
                                            subInfo.Existing = true;
                                        }

                                        SubscriberDataRow existsInNewsLetterList = p_inNewsletterList.Find(delegate(SubscriberDataRow s)
                                        {
                                            return s.EmailAddress.ToLower() == subInfo.EmailAddress.ToLower();
                                        });

                                        if (existsInNewsLetterList != null)
                                        {
                                            subInfo.InNewsletterList = true;
                                        }
                                    }

                                    importedData.Add(subInfo);
                                }
                            }
                        }
                    }
                }
            }
            return importedData;
        }

        /// <summary>
        /// Get subscriber data from data set
        /// </summary>
        /// <param name="p_subscriberData"></param>
        /// <param name="p_dataType"></param>
        /// <returns></returns>
        protected List<SubscriberDataRow> GetSubscribersData(DataSet p_subscriberData, SubscriberDataType p_dataType)
        {
            List<SubscriberDataRow> subscriberData = new List<SubscriberDataRow>();

            if (p_subscriberData != null)
            {
                if (p_subscriberData.Tables[0] != null)
                {
                    if (p_subscriberData.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in p_subscriberData.Tables[0].Rows)
                        {
                            SubscriberDataRow currentSub = null;

                            try
                            {
                                currentSub = new SubscriberDataRow(row, p_dataType);
                            }
                            catch (Exception)
                            {
                                // ignore for now
                            }

                            if (currentSub != null)
                            {
                                subscriberData.Add(currentSub);
                            }
                        }
                    }
                }
            }

            return subscriberData;
        }

        #endregion subscriber mapping

        #region Read csv, xlsx, .xls

        /// <summary>
        /// Get ODBCConnectionString
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="firstRowContainsHeaders"></param>
        /// <returns></returns>
        protected string GetODBCConnectionString(string filename, bool firstRowContainsHeaders)
        {
            string connectionString = "Provider={0};Data Source={1};Extended Properties=\"{2};HDR={3};IMEX=1;Empty Text Mode=NullAsEmpty\"";
            string dataSource = String.Empty;
            string lowerFileName = filename.ToLower();
            string provider = String.Empty;
            string properties = String.Empty;
            if (lowerFileName.Contains(".xlsx"))
            {
                dataSource = filename;
                provider = "Microsoft.ACE.OLEDB.12.0";
                properties = "Excel 12.0 Xml";
            }
            else if (lowerFileName.Contains(".xls"))
            {
                dataSource = filename;

                //provider = "Microsoft.Jet.OLEDB.4.0";
                provider = "Microsoft.ACE.OLEDB.12.0";
                properties = "Excel 8.0";
            }
            else if (lowerFileName.Contains(".csv") || lowerFileName.Contains(".txt"))
            {
                dataSource = System.IO.Path.GetDirectoryName(filename);

                //provider = "Microsoft.Jet.OLEDB.4.0";
                provider = "Microsoft.ACE.OLEDB.12.0";
                properties = "Text;FMT = Delimited";
            }

            if (!String.IsNullOrWhiteSpace(provider))
            {
                connectionString = String.Format(connectionString, provider, dataSource.Replace("'", "''"), properties, firstRowContainsHeaders ? "Yes" : "No");
            }
            else
            {
                // unsupported
            }

            return connectionString;
        }

        /// <summary>
        /// Read Excel Files
        /// </summary>
        /// <param name="filename"></param>
        protected List<DataTable> ReadExcel(string filename, bool firstRowContainsHeaders)
        {
            string connStr = GetODBCConnectionString(filename, firstRowContainsHeaders);
            OleDbConnection conn = null;
            OleDbCommand cmd = null;
            OleDbDataReader dr = null;

            List<DataTable> dts = new List<DataTable>();
            try
            {
                conn = new System.Data.OleDb.OleDbConnection(connStr);
                conn.Open();

                DataTable sheets = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new Object[] { null, null, null, "Table" });
                DataTable views = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new Object[] { null, null, null, "Views" });

                List<string> sheetIds = new List<string>();

                if (filename.Contains(".csv"))
                {
                    sheetIds.Add(System.IO.Path.GetFileName(filename));
                }
                else
                {
                    if (sheets != null && sheets.Rows.Count > 0)
                    {
                        foreach (DataRow sheet in sheets.Rows)
                        {
                            string sheetName = sheet.ItemArray[2].ToString();
                            sheetIds.Add(sheetName);
                        }
                    }
                }

                foreach (string sheetId in sheetIds)
                {
                    string query = @"Select * From [" + sheetId + "]";
                    cmd = new System.Data.OleDb.OleDbCommand(query, conn);
                    dr = cmd.ExecuteReader();
                    DataTable sheetDataTable = new DataTable();
                    sheetDataTable.Load(dr);

                    if (sheetDataTable != null)
                    {
                        dts.Add(sheetDataTable);
                    }
                }

                dr.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                if (dr != null)
                {
                    dr.Close();
                }
                if (conn != null)
                {
                    conn.Close();
                }
                throw ex;
            }

            return dts;
        }

        #endregion Read csv, xlsx, .xls

        #region data access

        #region connection string helper

        /// <summary>
        /// Get AMP connection string
        /// </summary>
        /// <returns></returns>
        protected string GetAMPConnectionString()
        {
            return GetAMPConnectionString("STARLORD", "AMP10_Carfax", "sa", "FrodoFrodo!!");
        }

        /// <summary>
        /// Get connections tring
        /// </summary>
        /// <param name="p_datasource"></param>
        /// <param name="p_catalog"></param>
        /// <param name="p_userid"></param>
        /// <param name="p_password"></param>
        /// <returns></returns>
        protected string GetAMPConnectionString(string p_datasource, string p_catalog, string p_userid, string p_password)
        {
            string datasource = p_datasource;
            string catalog = p_catalog;
            string userid = p_userid;
            string password = p_password;
            return String.Format("Data Source={0};Initial Catalog={1};user id={2};password={3}", datasource, catalog, userid, password);
        }

        #endregion connection string helper

        #region subscribers handling

        /// <summary>
        /// Insert subsubscribers
        /// </summary>
        /// <param name="p_subscriberData"></param>
        /// <returns></returns>
        protected bool InsertSubscribers(List<SubscriberDataRow> p_subscriberData, int p_newsletterListId, int p_siteId)
        {
            if (p_subscriberData != null && p_subscriberData.Count > 0)
            {
                List<SubscriberDataRow> insertList = p_subscriberData.FindAll(delegate(SubscriberDataRow s)
                {
                    return !s.HasDuplicate
                        && !s.Existing
                        && !s.IsBadDataRow
                        && !s.IsOptOut;
                });

                List<SubscriberDataRow> needAssignedToList = p_subscriberData.FindAll(delegate(SubscriberDataRow s)
                {
                    return !s.InNewsletterList;
                });

                foreach (SubscriberDataRow ins in insertList)
                {
                    try
                    {
                        InsertSubscriber(ins, p_newsletterListId, p_siteId);
                    }
                    catch (Exception ex)
                    {
                        // for now.
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }

                foreach (SubscriberDataRow assign in needAssignedToList)
                {
                    try
                    {
                        AddSubscriberToNewsLetterList(assign, p_newsletterListId);
                    }
                    catch (Exception ex)
                    {
                        // for now.
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Insert a single subscriber
        /// </summary>
        /// <param name="p_subscriberData"></param>
        /// <param name="p_newsletterListId"></param>
        /// <param name="p_siteId"></param>
        /// <returns></returns>
        protected bool InsertSubscriber(SubscriberDataRow p_subscriberData, int p_newsletterListId, int p_siteId)
        {
            bool added = false;

            string connectionString = GetAMPConnectionString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //SqlTransaction transaction = null;
                try
                {
                    //transaction = connection.BeginTransaction();

                    string strCmd = String.Empty;
                    strCmd = "sproc_cust_InsertCarFaxSubscriber";

                    #region call store proc

                    SqlCommand command = new SqlCommand(strCmd, connection);

                    // Mark the command as store procedure
                    command.CommandType = CommandType.StoredProcedure;

                    //command.Transaction = transaction;

                    command.Parameters.AddWithValue("@SiteID", p_siteId);
                    command.Parameters.AddWithValue("@NewsLetterListID", p_newsletterListId);
                    command.Parameters.AddWithValue("@Email", p_subscriberData.EmailAddress);
                    command.Parameters.AddWithValue("@Name", p_subscriberData.Name);
                    command.Parameters.AddWithValue("@FirstName", p_subscriberData.FirstName);
                    command.Parameters.AddWithValue("@LastName", p_subscriberData.LastName);

                    int vinIndex = 1;

                    if (p_subscriberData.VINs != null)
                    {
                        foreach (string vin in p_subscriberData.VINs)
                        {
                            if (!String.IsNullOrWhiteSpace(vin))
                            {
                                string paramName = String.Format("@VIN{0}", vinIndex);
                                command.Parameters.AddWithValue(paramName, vin);
                                vinIndex++;
                            }
                        }
                    }

                    // Execute Query
                    command.ExecuteNonQuery();

                    added = true;

                    #endregion call store proc

                    // Release the resources
                    connection.Close();

                    //transaction.Commit();
                }
                catch (SqlException)
                {
                    // Rollback the transaction, if error occurred.
                    //transaction.Rollback();
                    throw;
                }
                finally
                {
                    // Close connection
                    connection.Close();
                }
            }

            return added;
        }

        /// <summary>
        /// Get Opt out list
        /// </summary>
        /// <param name="p_siteId"></param>
        protected List<SubscriberDataRow> GetOptOutList(int p_siteId)
        {
            List<SubscriberDataRow> optOutSubscribers = new List<SubscriberDataRow>();

            string connectionString = GetAMPConnectionString();

            // Create instance of connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create instance of command object
                SqlDataAdapter adapter = new SqlDataAdapter("sproc_cust_GetCarfaxOptOutSubscribersBySiteID", connection);

                // Mark the command as store procedure
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                adapter.SelectCommand.Parameters.AddWithValue("@SiteID", p_siteId);

                // Create and fill the dataset
                DataSet dataSet = new DataSet();
                connection.Open();
                adapter.Fill(dataSet);

                // Release the resources
                adapter.Dispose();
                connection.Close();

                // convert data to subscriber data rows
                optOutSubscribers = GetSubscribersData(dataSet, SubscriberDataType.OptOut);
            }

            return optOutSubscribers;
        }

        /// <summary>
        /// Get Existing subscribers
        /// </summary>
        /// <param name="p_siteId"></param>
        /// <returns></returns>
        protected List<SubscriberDataRow> GetExistingSubscribers(int p_siteId)
        {
            List<SubscriberDataRow> existingSubscriber = new List<SubscriberDataRow>();

            string connectionString = GetAMPConnectionString();

            // Create instance of connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create instance of command object
                SqlDataAdapter adapter = new SqlDataAdapter("sproc_cust_GetCarfaxExistingSubscribersBySiteID", connection);

                // Mark the command as store procedure
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                adapter.SelectCommand.Parameters.AddWithValue("@SiteID", p_siteId);

                // Create and fill the dataset
                DataSet dataSet = new DataSet();
                connection.Open();
                adapter.Fill(dataSet);

                // Release the resources
                adapter.Dispose();
                connection.Close();

                // convert data to subscriber data rows
                existingSubscriber = GetSubscribersData(dataSet, SubscriberDataType.Existing);
            }

            return existingSubscriber;
        }

        #endregion subscribers handling

        #region newsletterlist

        /// <summary>
        /// Get newsletter list to map the imported user to
        /// </summary>
        protected int GetImportNewsLetterListID(int p_siteID)
        {
            int selectedID = 0;
            int.TryParse(ddlSelectNewsLetter.SelectedValue, out selectedID);

            if (selectedID <= 0)
            {
                string name = txtNewsLetterlistName.Text.Trim();

                // we need to create a new list
                NewsLetterListDataRow data = new NewsLetterListDataRow(p_siteID, 0, name);
                CurrentUserInfo info = MembershipContext.AuthenticatedUser;
                string GUID = info.UserGUID.ToString();
                string UserName = info.UserName.ToString();
                string userAmpNum = info.GetStringValue("UserAmpNum", String.Empty);
                string dealerName = info.GetStringValue("UserDealer", String.Empty);
                data.DealerName = UserName;
                data.DealerEmail = UserName + "@CARFAXMARKETING.COM";
                try
                {
                    selectedID = InsertNewsletterList(data);
                }
                catch (Exception ex)
                {
                    litMessage.Text = "Unabled to create new letter list: " + ex.Message;
                }
            }

            return selectedID;
        }

        /// <summary>
        /// Get existing news letter list
        /// </summary>
        /// <param name="p_siteId"></param>
        /// <returns></returns>
        protected List<NewsLetterListDataRow> GetExistingNewsletterList(int p_siteId)
        {
            List<NewsLetterListDataRow> newsletterlists = new List<NewsLetterListDataRow>();

            string connectionString = GetAMPConnectionString();

            // Create instance of connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create instance of command object
                SqlDataAdapter adapter = new SqlDataAdapter("sproc_cust_GetCarFaxNewsLetterListBySite", connection);

                // Mark the command as store procedure
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                adapter.SelectCommand.Parameters.AddWithValue("@SiteID", p_siteId);

                // Create and fill the dataset
                DataSet dataSet = new DataSet();
                connection.Open();
                adapter.Fill(dataSet);

                // Release the resources
                adapter.Dispose();
                connection.Close();

                if (dataSet != null)
                {
                    if (dataSet.Tables[0] != null)
                    {
                        if (dataSet.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in dataSet.Tables[0].Rows)
                            {
                                if (row != null)
                                {
                                    NewsLetterListDataRow newsletter = new NewsLetterListDataRow(row);
                                    if (newsletter != null)
                                    {
                                        newsletterlists.Add(newsletter);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return newsletterlists;
        }

        /// <summary>
        /// Returns news letter list ID
        /// </summary>
        /// <param name="p_newsletter"></param>
        /// <returns></returns>
        protected int InsertNewsletterList(NewsLetterListDataRow p_newsletter)
        {
            int newletterListID = 0;
            string connectionString = GetAMPConnectionString();

            // Create instance of connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create instance of command object
                SqlDataAdapter adapter = new SqlDataAdapter("sproc_cust_InsertCarFaxNewsletterList", connection);

                // Mark the command as store procedure
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                adapter.SelectCommand.Parameters.AddWithValue("@SiteID", p_newsletter.SiteID);
                adapter.SelectCommand.Parameters.AddWithValue("@Name", p_newsletter.Name);
                adapter.SelectCommand.Parameters.AddWithValue("@DealerName", p_newsletter.DealerName);
                adapter.SelectCommand.Parameters.AddWithValue("@DealerEmail", p_newsletter.DealerEmail);

                // Create and fill the dataset
                DataSet dataSet = new DataSet();
                connection.Open();
                adapter.Fill(dataSet);

                // Release the resources
                adapter.Dispose();
                connection.Close();

                if (dataSet != null)
                {
                    if (dataSet.Tables[0] != null)
                    {
                        if (dataSet.Tables[0].Rows.Count > 0)
                        {
                            DataRow row = dataSet.Tables[0].Rows[0];
                            if (row != null)
                            {
                                newletterListID = Convert.ToInt32(row["NewsLetterListID"]);
                            }
                        }
                    }
                }
            }

            return newletterListID;
        }

        /// <summary>
        /// Get current news letter list subscribers
        /// </summary>
        /// <param name="p_newsletterListID"></param>
        /// <returns></returns>
        protected List<SubscriberDataRow> GetNewsletterListSubscriber(int p_newsletterListID)
        {
            List<SubscriberDataRow> optOutSubscribers = new List<SubscriberDataRow>();

            string connectionString = GetAMPConnectionString();

            // Create instance of connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create instance of command object
                SqlDataAdapter adapter = new SqlDataAdapter("sproc_cust_GetCarFaxSubscribersByNewsLetterID", connection);

                // Mark the command as store procedure
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                adapter.SelectCommand.Parameters.AddWithValue("@NewsLetterListID", p_newsletterListID);

                // Create and fill the dataset
                DataSet dataSet = new DataSet();
                connection.Open();
                adapter.Fill(dataSet);

                // Release the resources
                adapter.Dispose();
                connection.Close();

                // convert data to subscriber data rows
                optOutSubscribers = GetSubscribersData(dataSet, SubscriberDataType.NewsLetterList);
            }

            return optOutSubscribers;
        }


        /// <summary>
        /// Add a subscriber to news letter list.
        /// </summary>
        /// <param name="p_subscriberData"></param>
        /// <param name="p_newsletterlistId"></param>
        /// <returns></returns>
        protected bool AddSubscriberToNewsLetterList(SubscriberDataRow p_subscriberData, int p_newsletterlistId)
        {
            bool added = false;
            string connectionString = GetAMPConnectionString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //SqlTransaction transaction = null;
                try
                {
                    //transaction = connection.BeginTransaction();

                    string strCmd = String.Empty;
                    strCmd = "sproc_cust_InsertCarFaxAddSubscriberToList";

                    #region call store proc

                    SqlCommand command = new SqlCommand(strCmd, connection);

                    // Mark the command as store procedure
                    command.CommandType = CommandType.StoredProcedure;

                    //command.Transaction = transaction;

                    command.Parameters.AddWithValue("@SubscriberID", p_subscriberData.SubscriberId);
                    command.Parameters.AddWithValue("@NewsLetterListID", p_newsletterlistId);

                    int vinIndex = 1;

                    if (p_subscriberData.VINs != null)
                    {
                        foreach (string vin in p_subscriberData.VINs)
                        {
                            if (!String.IsNullOrWhiteSpace(vin))
                            {
                                string paramName = String.Format("@VIN{0}", vinIndex);
                                command.Parameters.AddWithValue(paramName, vin);
                                vinIndex++;
                            }
                        }
                    }

                    // Execute Query
                    command.ExecuteNonQuery();

                    added = true;

                    #endregion call store proc

                    // Release the resources
                    connection.Close();

                    //transaction.Commit();
                }
                catch (SqlException)
                {
                    // Rollback the transaction, if error occurred.
                    //transaction.Rollback();
                    throw;
                }
                finally
                {
                    // Close connection
                    connection.Close();
                }
            }

            return added;
        }

        #endregion newsletterlist

        #region other helpers

        /// <summary>
        /// Get CarFax AMP site ID by dealer's name
        /// </summary>
        /// <param name="p_userName"></param>
        /// <returns></returns>
        protected int GetDealerAMPSiteIDByUserName(string p_userName)
        {
            int siteId = 0;
            string connectionString = GetAMPConnectionString();

            // Create instance of connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create instance of command object
                SqlDataAdapter adapter = new SqlDataAdapter("sproc_cust_GetCarfaxSiteIDByAgileSiteUserName", connection);

                // Mark the command as store procedure
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                adapter.SelectCommand.Parameters.AddWithValue("@UserName", p_userName);

                // Create and fill the dataset
                DataSet dataSet = new DataSet();
                connection.Open();
                adapter.Fill(dataSet);

                // Release the resources
                adapter.Dispose();
                connection.Close();

                if (dataSet != null)
                {
                    if (dataSet.Tables[0] != null)
                    {
                        if (dataSet.Tables[0].Rows.Count > 0)
                        {
                            DataRow row = dataSet.Tables[0].Rows[0];
                            if (row != null)
                            {
                                siteId = Convert.ToInt32(row["SiteID"]);
                            }
                        }
                    }
                }
            }

            return siteId;
        }

        /// <summary>
        /// Get CarFax AMP site ID by dealer's name
        /// </summary>
        /// <param name="p_dealerName"></param>
        /// <returns></returns>
        protected int GetDealerAMPSiteIDByDealerName(string p_dealerName)
        {
            int siteId = 0;
            string connectionString = GetAMPConnectionString();

            // Create instance of connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create instance of command object
                SqlDataAdapter adapter = new SqlDataAdapter("sproc_cust_GetCarfaxSiteIDByDealerName", connection);

                // Mark the command as store procedure
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                adapter.SelectCommand.Parameters.AddWithValue("@DealerName", p_dealerName);

                // Create and fill the dataset
                DataSet dataSet = new DataSet();
                connection.Open();
                adapter.Fill(dataSet);

                // Release the resources
                adapter.Dispose();
                connection.Close();

                if (dataSet != null)
                {
                    if (dataSet.Tables[0] != null)
                    {
                        if (dataSet.Tables[0].Rows.Count > 0)
                        {
                            DataRow row = dataSet.Tables[0].Rows[0];
                            if (row != null)
                            {
                                siteId = Convert.ToInt32(row["SiteID"]);
                            }
                        }
                    }
                }
            }

            return siteId;
        }

        #endregion other helpers

        #endregion data access

        #region file handling

        /// <summary>
        /// Upload file
        /// </summary>
        /// <returns></returns>
        protected string UploadFile(string p_uploadDirPath)
        {
            // Clear out the error message.
            litMessage.Text = String.Empty;

            // can we do this dynamically?
            string uploadDirPath = "~/sites/carfax/forms/carfax/";

            if (!String.IsNullOrWhiteSpace(p_uploadDirPath))
            {
                uploadDirPath = p_uploadDirPath;
            }

            CurrentUserInfo info = MembershipContext.AuthenticatedUser;
            HttpContext.Current.Server.ScriptTimeout = 90000;
            string GUID = info.UserGUID.ToString();
            string UserName = info.UserName.ToString();
            string userAmpNum = info.GetStringValue("UserAmpNum", String.Empty);
            string dealerName = info.GetStringValue("UserDealer", String.Empty);

            if (String.IsNullOrWhiteSpace(ExcelUpload.PostedFile.FileName))
            {
                litMessage.Text = "Please select a file.";

                // missing file name
                return String.Empty;
            }

            string filename = Path.GetFileNameWithoutExtension(ExcelUpload.PostedFile.FileName);
            string extension = Path.GetExtension(ExcelUpload.PostedFile.FileName);
            string sessionId = Session.SessionID.ToString();

            if (extension.ToLower().Contains(".txt"))
            {
                // change it to csv
                extension = ".csv";
            }

            //string FullName = uploadDirPath + filename + "_" + sessionId + "_" + GUID  + extension;
            string FullName = uploadDirPath + filename + "_" + sessionId + extension;

            byte[] fileData = new byte[ExcelUpload.PostedFile.InputStream.Length];
            ExcelUpload.PostedFile.InputStream.Read(fileData, 0, (int)ExcelUpload.PostedFile.InputStream.Length);
            string physicalPath = GetPhysicalPath(FullName);

            // Save out the file
            File.WriteAllBytes(physicalPath, fileData);

            // return the physical path
            return physicalPath;
        }

        /// <summary>
        /// Get physical file path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        protected string GetPhysicalPath(string filePath)
        {
            string physicalPath = String.Empty;

            if (!String.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    physicalPath = Server.MapPath(filePath);
                }
                catch (Exception)
                {
                    physicalPath = HttpRuntime.AppDomainAppPath + filePath.Replace("~", string.Empty).Replace('/', '\\');
                }
            }

            return physicalPath;
        }

        #endregion file handling

        #region helpers

        /// <summary>
        /// Get string property value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_defaultValue"></param>
        /// <returns></returns>
        protected string GetStringObjectValue(string p_key, string p_defaultValue)
        {
            string value = p_defaultValue;

            object obj = GetValue(p_key);

            if (obj != null)
            {
                value = obj.ToString();
            }

            if (String.IsNullOrWhiteSpace(value))
            {
                value = p_defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Get Bool property value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_defaultValue"></param>
        /// <returns></returns>
        protected bool GetBoolObjectValue(string p_key, bool p_defaultValue)
        {
            bool value = p_defaultValue;
            object obj = GetValue(p_key);
            if (obj != null)
            {
                if (!bool.TryParse(obj.ToString(), out value))
                {
                    value = p_defaultValue;
                }
            }
            return value;
        }

        #endregion helpers
    }
}