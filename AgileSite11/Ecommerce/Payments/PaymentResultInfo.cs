using System;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Xml;

using CMS.EventLog;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class representing payment result.
    /// </summary>
    [Serializable]
    public class PaymentResultInfo : ISerializable
    {
        #region "Variables"

        [NonSerialized]
        private XmlDocument xmlDoc;

        private const string HEADER_DATE = "{$PaymentGateway.Result.Date$}";
        private const string HEADER_PAYMENTMETHOD = "{$PaymentGateway.Result.PaymentMethod$}";
        private const string HEADER_ISCOMPLETED = "{$PaymentGateway.Result.IsCompleted$}";
        private const string HEADER_ISAUTHORIZED = "{$PaymentGateway.Result.IsAuthorized$}";
        private const string HEADER_ISFAILED = "{$PaymentGateway.Result.IsFailed$}";
        private const string HEADER_STATUS = "{$PaymentGateway.Result.Status$}";
        private const string HEADER_TRANSACTIONID = "{$PaymentGateway.Result.TransactionID$}";
        private const string HEADER_AUTHORIZATION_ID = "{$PaymentGateway.Result.AuthorizationId$}";
        private const string HEADER_DESCRIPTION = "{$PaymentGateway.Result.Description$}";
        private const string HEADER_APPROVAL_URL = "{$PaymentGateway.Result.Approval.Url$}";
        private const string EVENT_SOURCE = "PAYMENTRESULT";

        private const string AUTHORIZED_ITEM_NAME = "authorized";
        private const string AUTHORIZATION_ID_ITEM_NAME = "authorizationid";
        private const string FAILED_ITEM_NAME = "failed";


        /// <summary>
        /// Root node of the payment result xml definition.
        /// </summary>
        private XmlNode RootNode
        {
            get
            {
                if (xmlDoc != null)
                {
                    return xmlDoc.DocumentElement ?? xmlDoc.AppendChild(xmlDoc.CreateElement("result"));
                }

                return null;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Payment date.
        /// </summary>
        public DateTime PaymentDate
        {
            get
            {
                PaymentResultItemInfo itemObj = EnsurePaymentResultItemInfo("date", HEADER_DATE);
                if (itemObj != null)
                {
                    return ValidationHelper.GetDateTimeSystem(itemObj.Value, DateTimeHelper.ZERO_TIME);
                }
                return DateTimeHelper.ZERO_TIME;
            }
            set
            {
                PaymentResultItemInfo itemObj = EnsurePaymentResultItemInfo("date", HEADER_DATE);
                if (itemObj != null)
                {
                    // Set date/time value in en-us culture format
                    itemObj.Value = value.ToString(CultureHelper.EnglishCulture.DateTimeFormat);

                    SetPaymentResultItemInfo(itemObj);
                }
            }
        }


        /// <summary>
        /// Indicates whether the payment is successfully completed.
        /// </summary>
        public bool PaymentIsCompleted
        {
            get
            {
                var item = EnsurePaymentResultItemInfo("completed", HEADER_ISCOMPLETED);

                return (item != null) && ValidationHelper.GetBoolean(item.Value, false);
            }
            set
            {
                var item = EnsurePaymentResultItemInfo("completed", HEADER_ISCOMPLETED);
                if (item != null)
                {
                    if (ValidationHelper.GetBoolean(value, false))
                    {
                        item.Value = "1";
                        item.Text = "{$PaymentGateway.Result.PaymentCompleted$}";
                    }
                    else
                    {
                        item.Reset();
                    }

                    SetPaymentResultItemInfo(item);
                }
            }
        }


        /// <summary>
        /// Indicates whether the payment failed.
        /// </summary>
        public bool PaymentIsFailed
        {
            get
            {
                var item = EnsurePaymentResultItemInfo(FAILED_ITEM_NAME, HEADER_ISFAILED);

                return (item != null) && ValidationHelper.GetBoolean(item.Value, false);
            }
            set
            {
                var item = EnsurePaymentResultItemInfo(FAILED_ITEM_NAME, HEADER_ISFAILED);

                if (ValidationHelper.GetBoolean(value, false))
                {
                    item.Value = "1";
                    item.Text = "{$PaymentGateway.Result.PaymentFailed$}";
                }
                else
                {
                    item.Reset();
                }

                SetPaymentResultItemInfo(item);
            }
        }


        /// <summary>
        /// Indicates whether the payment is already authorized and capture is possible.
        /// </summary>
        public bool PaymentIsAuthorized
        {
            get
            {
                var item = EnsurePaymentResultItemInfo(AUTHORIZED_ITEM_NAME, HEADER_ISAUTHORIZED);

                return (item != null) && ValidationHelper.GetBoolean(item.Value, false);
            }
            set
            {
                var item = EnsurePaymentResultItemInfo(AUTHORIZED_ITEM_NAME, HEADER_ISAUTHORIZED);

                if (ValidationHelper.GetBoolean(value, false))
                {
                    item.Value = "1";
                    item.Text = "{$paymentgateway.result.paymentauthorized$}";
                }
                else
                {
                    item.Reset();
                }

                SetPaymentResultItemInfo(item);
            }
        }


        /// <summary>
        /// Payment result description.
        /// </summary>
        public string PaymentDescription
        {
            get
            {
                PaymentResultItemInfo itemObj = EnsurePaymentResultItemInfo("description", HEADER_DESCRIPTION);
                if (itemObj != null)
                {
                    return itemObj.Value;
                }

                return "";
            }
            set
            {
                PaymentResultItemInfo itemObj = EnsurePaymentResultItemInfo("description", HEADER_DESCRIPTION);
                if (itemObj != null)
                {
                    itemObj.Value = ValidationHelper.GetString(value, "");
                    SetPaymentResultItemInfo(itemObj);
                }
            }
        }


        /// <summary>
        /// Payment result transaction ID.
        /// </summary>
        public string PaymentTransactionID
        {
            get
            {
                PaymentResultItemInfo itemObj = EnsurePaymentResultItemInfo("transactionid", HEADER_TRANSACTIONID);
                if (itemObj != null)
                {
                    return itemObj.Value;
                }

                return "";
            }
            set
            {
                PaymentResultItemInfo itemObj = EnsurePaymentResultItemInfo("transactionid", HEADER_TRANSACTIONID);
                if (itemObj != null)
                {
                    itemObj.Value = ValidationHelper.GetString(value, "");
                    SetPaymentResultItemInfo(itemObj);
                }
            }
        }


        /// <summary>
        /// Gets or sets the payment authorization ID.
        /// </summary>
        public string PaymentAuthorizationID
        {
            get
            {
                var item = EnsurePaymentResultItemInfo(AUTHORIZATION_ID_ITEM_NAME, HEADER_AUTHORIZATION_ID);
                return item != null ? item.Value : "";
            }
            set
            {
                var item = EnsurePaymentResultItemInfo(AUTHORIZATION_ID_ITEM_NAME, HEADER_AUTHORIZATION_ID);
                if (item != null)
                {
                    item.Value = ValidationHelper.GetString(value, "");
                    SetPaymentResultItemInfo(item);
                }
            }
        }


        /// <summary>
        /// Payment method name.
        /// </summary>
        public string PaymentMethodName
        {
            get
            {
                PaymentResultItemInfo itemObj = EnsurePaymentResultItemInfo("method", HEADER_PAYMENTMETHOD);
                if (itemObj != null)
                {
                    return itemObj.Text;
                }

                return "";
            }
            set
            {
                PaymentResultItemInfo itemObj = EnsurePaymentResultItemInfo("method", HEADER_PAYMENTMETHOD);
                if (itemObj != null)
                {
                    itemObj.Text = ValidationHelper.GetString(value, "");
                    SetPaymentResultItemInfo(itemObj);
                }
            }
        }


        /// <summary>
        /// Payment method ID.
        /// </summary>
        public int PaymentMethodID
        {
            get
            {
                PaymentResultItemInfo itemObj = EnsurePaymentResultItemInfo("method", HEADER_PAYMENTMETHOD);
                if (itemObj != null)
                {
                    return ValidationHelper.GetInteger(itemObj.Value, 0);
                }

                return 0;
            }
            set
            {
                PaymentResultItemInfo itemObj = EnsurePaymentResultItemInfo("method", HEADER_PAYMENTMETHOD);
                if (itemObj != null)
                {
                    itemObj.Value = ValidationHelper.GetString(value, "");
                    SetPaymentResultItemInfo(itemObj);
                }
            }
        }


        /// <summary>
        /// Payment status value.
        /// </summary>
        public string PaymentStatusValue
        {
            get
            {
                PaymentResultItemInfo itemObj = EnsurePaymentResultItemInfo("status", HEADER_STATUS);
                if (itemObj != null)
                {
                    return itemObj.Value;
                }

                return "";
            }
            set
            {
                PaymentResultItemInfo itemObj = EnsurePaymentResultItemInfo("status", HEADER_STATUS);
                if (itemObj != null)
                {
                    itemObj.Value = ValidationHelper.GetString(value, "");
                    SetPaymentResultItemInfo(itemObj);
                }
            }
        }


        /// <summary>
        /// Payment status display name.
        /// </summary>
        public string PaymentStatusName
        {
            get
            {
                PaymentResultItemInfo itemObj = EnsurePaymentResultItemInfo("status", HEADER_STATUS);
                if (itemObj != null)
                {
                    return itemObj.Text;
                }

                return "";
            }
            set
            {
                PaymentResultItemInfo itemObj = EnsurePaymentResultItemInfo("status", HEADER_STATUS);
                if (itemObj != null)
                {
                    itemObj.Text = ValidationHelper.GetString(value, "");
                    SetPaymentResultItemInfo(itemObj);
                }
            }
        }


        /// <summary>
        /// Gets or sets the url with allowing to approve payment.
        /// </summary>
        public string PaymentApprovalUrl
        {
            get
            {
                var item = EnsurePaymentResultItemInfo("approvalUrl", HEADER_APPROVAL_URL);
                return item.Value;
            }
            set
            {
                var item = EnsurePaymentResultItemInfo("approvalUrl", HEADER_APPROVAL_URL);
                item.Value = value;

                SetPaymentResultItemInfo(item);
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns payment result item object.
        /// </summary>
        /// <param name="itemName">Item name</param>
        public PaymentResultItemInfo GetPaymentResultItemInfo(string itemName)
        {
            XmlNode node = RootNode?.SelectSingleNode("item[@name='" + itemName + "']");
            if (node != null)
            {
                return new PaymentResultItemInfo(node);
            }

            return null;
        }


        /// <summary>
        /// Updates/Inserts payment result item data in/into payment result.
        /// </summary>
        /// <param name="itemObj">Payment result item data object</param>
        public void SetPaymentResultItemInfo(PaymentResultItemInfo itemObj)
        {
            if (itemObj != null)
            {
                // Create base xml document when necessary
                if (xmlDoc == null)
                {
                    xmlDoc = new XmlDocument();
                }

                // Get payment result item node
                XmlNode itemNode = RootNode.SelectSingleNode("item[@name='" + itemObj.Name + "']");
                if (itemNode == null)
                {
                    // Add new result item node
                    itemNode = xmlDoc.CreateElement("item");
                    RootNode.AppendChild(itemNode);
                }

                // Update node attributes
                string[,] attribs = new string[4, 2];
                attribs[0, 0] = "name";
                attribs[0, 1] = itemObj.Name;
                attribs[1, 0] = "header";
                attribs[1, 1] = itemObj.Header;
                attribs[2, 0] = "value";
                attribs[2, 1] = itemObj.Value;
                attribs[3, 0] = "text";
                attribs[3, 1] = itemObj.Text;
                XmlHelper.SetXmlNodeAttributes(itemNode, attribs, false);
            }

        }


        /// <summary>
        /// Returns payment result XML string.
        /// </summary>
        public string GetPaymentResultXml()
        {
            if (RootNode?.SelectNodes("item")?.Count > 0)
            {
                return xmlDoc.InnerXml;
            }

            return "";
        }


        /// <summary>
        /// Returns formatted payment result string which is visible to the user.
        /// </summary>
        public virtual string GetFormattedPaymentResultString()
        {
            if (RootNode == null)
            {
                return "";
            }

            var strBuilder = new StringBuilder();

            PaymentResultItemInfo descriptionObj = null;
            foreach (XmlNode itemNode in RootNode.SelectNodes("item"))
            {
                var itemObj = new PaymentResultItemInfo(itemNode);
                if (!string.Equals(itemObj.Name, "description", StringComparison.OrdinalIgnoreCase))
                {
                    // Build result string
                    var text = string.IsNullOrWhiteSpace(itemObj.Text) ? itemObj.Value : itemObj.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        text = ResHelper.LocalizeString(itemObj.Header) + " " + ResHelper.LocalizeString(text);
                        strBuilder.Append(HTMLHelper.HTMLEncode(text), "<br />");
                    }
                }
                else
                {
                    // Remember description
                    descriptionObj = itemObj;
                }
            }

            // Add description as the last item
            if (descriptionObj != null)
            {
                string text = (string.IsNullOrWhiteSpace(descriptionObj.Text)) ? descriptionObj.Value : descriptionObj.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    text = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(text));
                    var header = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(descriptionObj.Header));
                    strBuilder.Append(header, "<br />", text, "<br />");
                }
            }

            return strBuilder.ToString();
        }


        /// <summary>
        /// Loads payment result xml.
        /// </summary>
        /// <param name="xml">Payment result XML to load</param>
        public void LoadPaymentResultXml(string xml)
        {
            xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(xml);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(EVENT_SOURCE, "LoadPaymentResultXml", ex);
            }
        }


        /// <summary>
        /// Tries to find specified payment result item (if it is not found it is created and initialized with item name and item header) and returns it.
        /// </summary>
        /// <param name="itemName">Payment result item name to get</param>
        /// <param name="itemHeader">Payment result item header text which is set to the new item when requested item is not found</param>
        public PaymentResultItemInfo EnsurePaymentResultItemInfo(string itemName, string itemHeader)
        {
            // Try to find requested item
            PaymentResultItemInfo item = GetPaymentResultItemInfo(itemName);

            // If not found
            if (item == null)
            {
                // Create and initialize new item
                item = new PaymentResultItemInfo
                {
                    Header = itemHeader,
                    Name = itemName,
                    Value = "",
                    Text = ""
                };

                // Save new item
                SetPaymentResultItemInfo(item);
            }

            return item;
        }


        /// <summary>
        /// Gets object data.
        /// </summary>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("xmlString", GetPaymentResultXml());
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates PaymentResultInfo object.
        /// </summary>
        /// <param name="xml">Payment result in XML format</param>
        public PaymentResultInfo(string xml)
        {
            LoadPaymentResultXml(xml);
        }


        /// <summary>
        /// Creates base PaymentResultInfo object - following fields are created in payment result XML definition, they are not initialized: date, payment method, payment is completed, payment status, transaction id, description.
        /// </summary>
        public PaymentResultInfo()
        {
            // Create base payment result xml definition
            EnsurePaymentResultItemInfo("date", HEADER_DATE);
            EnsurePaymentResultItemInfo("method", HEADER_PAYMENTMETHOD);
            EnsurePaymentResultItemInfo("status", HEADER_STATUS);
            EnsurePaymentResultItemInfo("transactionid", HEADER_TRANSACTIONID);
            EnsurePaymentResultItemInfo("description", HEADER_DESCRIPTION);
        }


        /// <summary>
        /// Constructor - Creates a new PaymentResultInfo object from serialized data.
        /// </summary>
        /// <param name="info">Serialization data</param>
        /// <param name="context">Context</param>
        public PaymentResultInfo(SerializationInfo info, StreamingContext context)
        {
            LoadPaymentResultXml(info.GetString("xmlString"));
        }

        #endregion
    }
}
