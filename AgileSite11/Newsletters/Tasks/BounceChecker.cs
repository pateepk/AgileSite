using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Newsletters.Extensions;
using CMS.Scheduler;
using CMS.Base;
using CMS.ContactManagement;
using CMS.SiteProvider;

using OpenPop.Mime;
using OpenPop.Pop3;

namespace CMS.Newsletters
{
    /// <summary>
    /// Task for checking bounced newsletter e-mails.
    /// </summary>
    public class BounceChecker : ITask
    {
        #region "Events"

        /// <summary>
        /// Occurs when a character set name could not be mapped to the encoding.
        /// </summary>
        /// <param name="characterSet">Character set name</param>
        public delegate Encoding FallbackDecoderHandler(string characterSet);


        /// <summary>
        /// Occurs when a character set from email content type header could not be mapped to the encoding.
        /// This allows to map given character set name to the encoding manually.
        /// </summary>
        public static event FallbackDecoderHandler OnFallbackDecoderEvent;

        #endregion


        #region "Constants"

        internal const string IssueIDField = "X-CMS-IssueID";


        internal const string SubscriberIDField = "X-CMS-SubscriberID";


        internal const string ContactIDField = "X-CMS-ContactID";


        internal const string RegexSuffix = @":[ ]*?(\d+)";

        #endregion


        #region "Fields"

        private static Regex mIssueIdRegex;


        private static Regex mSubscriberIdRegex;


        private static Regex mContactIdRegex;


        private string mServerName;


        private int mPortNumber;


        private string mUsername;


        private string mPassword;


        private string mEmailAddress;


        private bool mUseSsl;


        private string mAuthenticationMethod;


        private bool mBlockGlobally;


        private Dictionary<int, int> issueBounces;


        private int mSiteId;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets a regex that can be used to parse email header fields to search for an issue ID.
        /// </summary>
        private static Regex IssueIDRegex
        {
            get
            {
                return mIssueIdRegex ?? (mIssueIdRegex = RegexHelper.GetRegex(IssueIDField + RegexSuffix));
            }
        }


        /// <summary>
        /// Gets a regex that can be used to parse email header fields to search for an subscriber ID.
        /// </summary>
        private static Regex SubscriberIdRegex
        {
            get
            {
                return mSubscriberIdRegex ?? (mSubscriberIdRegex = RegexHelper.GetRegex(SubscriberIDField + RegexSuffix));
            }
        }


        /// <summary>
        /// Gets a regex that can be used to parse email header fields to search for an contact ID.
        /// </summary>
        private static Regex ContactIDRegex
        {
            get
            {
                return mContactIdRegex ?? (mContactIdRegex = RegexHelper.GetRegex(ContactIDField + RegexSuffix));
            }
        }

        #endregion


        #region "ITask Members"

        /// <summary>
        /// Executes the task given in a task info.
        /// </summary>
        /// <param name="task">Container with task information</param>
        /// <returns>Textual description of task run's failure if any.</returns>
        public string Execute(TaskInfo task)
        {
            mSiteId = ValidationHelper.GetInteger(task.TaskSiteID, 0);
            if (mSiteId == 0)
            {
                return "Site ID is missing or malformed.";
            }

            SiteInfo site = SiteInfoProvider.GetSiteInfo(mSiteId);
            if (site == null)
            {
                return "A site with the specified ID does not exist.";
            }

            string siteName = site.SiteName;
            if (!NewsletterHelper.MonitorBouncedEmails(siteName))
            {
                return "Monitoring is not enabled for this site.";
            }

            // Get connection settings and test connection
            mServerName = SettingsKeyInfoProvider.GetValue(siteName + ".CMSPOP3ServerName");
            mPortNumber = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSPOP3ServerPort");
            mUsername = SettingsKeyInfoProvider.GetValue(siteName + ".CMSPOP3UserName");
            mPassword = EncryptionHelper.DecryptData(SettingsKeyInfoProvider.GetValue(siteName + ".CMSPOP3Password"));
            mEmailAddress = NewsletterHelper.BouncedEmailAddress(siteName);
            mUseSsl = SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSPOP3UseSSL");
            mAuthenticationMethod = SettingsKeyInfoProvider.GetValue(siteName + ".CMSPOP3AuthenticationMethod");
            mBlockGlobally = SettingsKeyInfoProvider.GetBoolValue("CMSBlockSubscribersGlobally");

            // Check basic connection parameters
            if (string.IsNullOrEmpty(mServerName) || (mPortNumber == 0) || string.IsNullOrEmpty(mUsername) || string.IsNullOrEmpty(mPassword))
            {
                EventLogProvider.LogWarning("Bounce checker", "Settings check", null, mSiteId, "Some of the settings for bounce checker are missing. Make sure that settings at 'On-line marketing -> Email marketing -> POP3 settings' are filled correctly.");
                return "Settings for bounce checker are incomplete.";
            }

            return ProcessAllEmails();
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Connects to the predefined mailbox, retrieves and checks all messages.
        /// </summary>
        /// <returns>Textual description of task run's failure if any.</returns>
        private string ProcessAllEmails()
        {
            // Initialize issue bounce counter
            issueBounces = new Dictionary<int, int>();

            using (Pop3Client pop3Client = new Pop3Client())
            {
                try
                {
                    // Connect; end on error, log exception and do not retry
                    pop3Client.Connect(mServerName, mPortNumber, mUseSsl);

                    // Authenticate using only the selected authentication method, end on error, log exception, do not try different methods
                    AuthenticationMethod authMethod = GetAuthenticationMethod();
                    pop3Client.Authenticate(mUsername, mPassword, authMethod);

                    // Set custom fallback decoder method which is called if a character set name could not be mapped to the encoding
                    OpenPop.Mime.Decode.EncodingFinder.FallbackDecoder = CustomFallbackDecoder;

                    int messageCount = pop3Client.GetMessageCount();
                    for (int i = messageCount; i > 0; i--)
                    {
                        Message message = pop3Client.GetSafeMessage(i);
                        if (ProcessEmail(message))
                        {
                            pop3Client.DeleteMessage(i);
                        }
                    }

                    pop3Client.Disconnect();
                }
                catch (Exception pce)
                {
                    // Typically catches Pop3ClientException and SecurityException
                    EventLogProvider.LogException("Newsletter", "BouncedEmails", pce);
                    return pce.Message;
                }
            }

            // Update issues' bounces in DB
            if (issueBounces.Count > 0)
            {
                foreach (int issueId in issueBounces.Keys)
                {
                    var issue = IssueInfoProvider.GetIssueInfo(issueId);
                    if (issue != null)
                    {
                        issue.IssueBounces += issueBounces[issueId];
                        IssueInfoProvider.SetIssueInfo(issue);
                    }
                }
            }

            // Task executed successfully
            return null;
        }


        /// <summary>
        /// Gets the authentication method.
        /// </summary>
        /// <returns>Value from an enumeration that represents the chosen authentication method</returns>
        /// <exception cref="ArgumentException">Thrown when a chosen authentication method is unsupported or malformed</exception>
        private AuthenticationMethod GetAuthenticationMethod()
        {
            switch (mAuthenticationMethod.ToUpperCSafe())
            {
                case "USERNAME":
                    return AuthenticationMethod.UsernameAndPassword;

                case "APOP":
                    return AuthenticationMethod.Apop;

                case "CRAM-MD5":
                    return AuthenticationMethod.CramMd5;

                case "AUTO":
                    return AuthenticationMethod.Auto;

                default:
                    throw new ArgumentException("Unsupported authentication method");
            }
        }


        /// <summary>
        /// Occurs when a character set name could not be mapped to the encoding.
        /// Calls OnFallbackDecoderEvent event if defined.
        /// </summary>
        /// <param name="characterSet">Character set name</param>
        /// <returns>Encoding</returns>
        private static Encoding CustomFallbackDecoder(string characterSet)
        {
            if (OnFallbackDecoderEvent != null)
            {
                return OnFallbackDecoderEvent(characterSet);
            }

            // No encoding could be found. This will throw an exception.
            return null;
        }


        /// <summary>
        /// Checks a given e-mail message for known header fields.
        /// If the message is recognized as bounced e-mail, then it is processed and counted.
        /// </summary>
        /// <param name="message">E-mail message</param>
        /// <returns>true, if message is a recognized bounced e-mail, otherwise false</returns>
        private bool ProcessEmail(Message message)
        {
            if (message == null)
            {
                return false;
            }

            // Issue and subscriber fields are mandatory, contact field is optional
            int issueId = 0, subscriberId = 0, contactId = 0;

            // Ty to get IDs from header
            if (message.Headers != null && (message.Headers.UnknownHeaders.Count > 0))
            {
                // UnknownHeaders may contain custom headers
                NameValueCollection headers = message.Headers.UnknownHeaders;

                issueId = TryGetHeaderValue(IssueIDField, headers);
                if (issueId > 0)
                {
                    subscriberId = TryGetHeaderValue(SubscriberIDField, headers);
                    contactId = TryGetHeaderValue(ContactIDField, headers);
                }
            }

            if (issueId == 0)
            {
                // Try parse message text parts
                List<MessagePart> textParts = message.FindAllTextVersions();
                foreach (MessagePart textPart in textParts)
                {
                    subscriberId = 0;
                    contactId = 0;

                    string body = textPart.GetBodyAsText();
                    if (TryParseField(IssueIDRegex, body, out issueId))
                    {
                        TryParseField(SubscriberIdRegex, body, out subscriberId);
                        TryParseField(ContactIDRegex, body, out contactId);
                        break;
                    }
                }
            }

            if (issueId == 0)
            {
                // Try parse raw message
                string asciiString = Encoding.ASCII.GetString(message.RawMessage);
                if (TryParseField(IssueIDRegex, asciiString, out issueId))
                {
                    TryParseField(SubscriberIdRegex, asciiString, out subscriberId);
                    TryParseField(ContactIDRegex, asciiString, out contactId);
                }
            }

            if (issueId > 0)
            {
                // Add bounces
                AddBouncedEmail(issueId, subscriberId, contactId);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Attempts to find a known field in unknown header collection.
        /// </summary>
        /// <param name="headerField">Field code</param>
        /// <param name="headers">Header collection</param>
        /// <returns>Integer value from specified header or 0 if not found</returns>
        private static int TryGetHeaderValue(string headerField, NameValueCollection headers)
        {
            int result = 0;

            string value = headers[headerField];
            if (!string.IsNullOrEmpty(value))
            {
                result = ValidationHelper.GetInteger(value.Replace("\0", "").Trim(), 0);
            }

            return result;
        }



        /// <summary>
        /// Attempts to find a known field using regex using try-parse pattern.
        /// </summary>
        /// <param name="regex">Regular expression used to look for the field</param>
        /// <param name="input">Input text</param>
        /// <param name="fieldID">Extracted ID from the field on success</param>
        /// <returns>true, if parsing was successful, otherwise false</returns>
        private static bool TryParseField(Regex regex, string input, out int fieldID)
        {
            Match match = regex.Match(input);

            fieldID = match.Success ? ValidationHelper.GetInteger(match.Groups[1].Value, 0) : 0;

            return match.Success;
        }


        /// <summary>
        /// Adds a bounce to all respective bounced email counters.
        /// </summary>
        /// <param name="issueId">Issue ID</param>
        /// <param name="subscriberId">Subscriber ID</param>
        /// <param name="contactId">Contact ID</param>
        private void AddBouncedEmail(int issueId, int subscriberId, int contactId)
        {
            AddIssueBounce(issueId);

            if (!mBlockGlobally)
            {
                // Add bounces depending on the IDs in the bounced e-mail
                AddSubscriberBounce(subscriberId);
                AddContactBounce(contactId);
            }
            else
            {
                // Add bounces depending on the e-mail address in the bounced e-mail
                string bouncedAddress = RetrieveBouncedAddress(subscriberId, contactId);
                if (!string.IsNullOrEmpty(bouncedAddress))
                {
                    AddEmailBounceGlobal(bouncedAddress);
                }
            }
        }


        /// <summary>
        /// Retrieves a bounced address of an intended recipient of the newsletter e-mail.
        /// </summary>
        /// <param name="subscriberId">Subscriber ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <returns>E-mail address where the newsletter could not be delivered</returns>
        private static string RetrieveBouncedAddress(int subscriberId, int contactId)
        {
            if (subscriberId > 0)
            {
                // Get email of a subscriber
                SubscriberInfo subscriber = SubscriberInfoProvider.GetSubscriberInfo(subscriberId);
                if ((subscriber != null) && (subscriber.SubscriberType == PredefinedObjectType.CONTACT))
                {
                    // Get the contact ID
                    contactId = subscriber.SubscriberRelatedID;
                }
            }

            // Get email of a contact
            if (contactId > 0)
            {
                if (LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.FullContactManagement))
                {
                    ContactInfo contact = ContactInfoProvider.GetContactInfo(contactId);
                    return contact != null ? contact.ContactEmail : null;
                }
            }

            return null;
        }


        /// <summary>
        /// Increments the number of bounces for every subscriber with a specified e-mail address across all sites.
        /// </summary>
        /// <param name="email">E-mail address to filter by</param>
        private void AddEmailBounceGlobal(string email)
        {
            // Increment number of bounces for subscribers
            var subscribers = SubscriberInfoProvider.GetSubscribers().WhereEquals("SubscriberEmail", email);

            foreach (var subscriber in subscribers)
            {
                AddSubscriberBounce(subscriber);
            }

            // Increment number of bounces for contacts, if contact management present
            if (LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.FullContactManagement))
            {
                AddContactsBounces(email);
            }
        }


        /// <summary>
        /// Adds bounce to all contacts with specified e-mail across all sites.
        /// </summary>
        /// <param name="email">E-mail of contacts to add bounce to</param>
        private void AddContactsBounces(string email)
        {
            ContactInfoProvider.AddContactBounceByEmail(email);

            // Increment number of bounces for contact subscribers
            // Get contact IDs of contacts with specified email
            var contactIds = ContactInfoProvider.GetContacts()
                .WhereEquals("ContactEmail", email)
                .Columns("ContactID")
                .GetListResult<int>();

            foreach (var contactId in contactIds)
            {
                // Get contact subscriber based on related ID and site ID
                var subscriber = SubscriberInfoProvider.GetSubscriberInfo(PredefinedObjectType.CONTACT, contactId, mSiteId);
                if (subscriber != null)
                {
                    // Increment number of bounces for contact subscriber
                    AddSubscriberBounce(subscriber);
                }
            }
        }


        /// <summary>
        /// Increments number of bounces for the specified issue by one.
        /// </summary>
        /// <param name="issueId">Issue ID</param>
        private void AddIssueBounce(int issueId)
        {
            if (!issueBounces.ContainsKey(issueId))
            {
                issueBounces.Add(issueId, 0);
            }

            // Increment number of bounces in temporary dictionary
            issueBounces[issueId]++;
        }


        /// <summary>
        /// Increments number of bounces for the specified subscriber by one.
        /// </summary>
        /// <param name="subscriberId">Subscriber ID</param>
        private static void AddSubscriberBounce(int subscriberId)
        {
            if (subscriberId > 0)
            {
                SubscriberInfo subscriber = SubscriberInfoProvider.GetSubscriberInfo(subscriberId);
                AddSubscriberBounce(subscriber);
            }
        }


        /// <summary>
        /// Increments number of bounces for the specified subscriber by one.
        /// </summary>
        /// <param name="subscriber">Subscriber object</param>
        private static void AddSubscriberBounce(SubscriberInfo subscriber)
        {
            if (subscriber != null)
            {
                subscriber.SubscriberBounces++;
                SubscriberInfoProvider.SetSubscriberInfo(subscriber);
            }
        }


        /// <summary>
        /// Increments number of bounces by one for specified contact.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        private static void AddContactBounce(int contactId)
        {
            if (contactId > 0)
            {
                ContactInfoProvider.AddContactBounce(contactId);
            }
        }

        #endregion
    }
}