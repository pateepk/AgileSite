using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.SharePoint
{
    /// <summary>
    /// Encapsulates information regarding connection parameters for SharePoint server.
    /// Any connection data item can be retrieved via <see cref="GetSharePointConnectionDataItem"/>.
    /// For access to frequently used items getter properties are available.
    /// </summary>
    /// <seealso cref="SharePointConnectionSiteUrl"/>
    /// <seealso cref="SharePointConnectionSharePointVersion"/>
    /// <seealso cref="SharePointConnectionAuthMode"/>
    /// <seealso cref="SharePointConnectionUserName"/>
    /// <seealso cref="SharePointConnectionDomain"/>
    /// <seealso cref="SharePointConnectionPassword"/>
    public class SharePointConnectionData
    {
        #region "Variables"

        private readonly Dictionary<string, object> mConnectionData;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets SharePointConnectionName.
        /// </summary>
        public string SharePointConnectionName
        {
            get
            {
                return GetSharePointConnectionDataItem("SharePointConnectionName") as string;
            }
            set
            {
                SetSharePointConnectionDataItem("SharePointConnectionName", value);
            }
        }


        /// <summary>
        /// Gets or sets SharePointConnectionSiteUrl.
        /// </summary>
        public string SharePointConnectionSiteUrl
        {
            get
            {
                return GetSharePointConnectionDataItem("SharePointConnectionSiteUrl") as string;
            }
            set
            {
                SetSharePointConnectionDataItem("SharePointConnectionSiteUrl", value);
            }
        }


        /// <summary>
        /// Gets or sets SharePointConnectionSharePointVersion.
        /// </summary>
        /// <seealso cref="SharePointVersion"/>
        public string SharePointConnectionSharePointVersion
        {
            get
            {
                return GetSharePointConnectionDataItem("SharePointConnectionSharePointVersion") as string;
            }
            set
            {
                SetSharePointConnectionDataItem("SharePointConnectionSharePointVersion", value);
            }
        }


        /// <summary>
        /// Gets or sets SharePointConnectionAuthMode.
        /// </summary>
        /// <seealso cref="SharePointAuthMode"/>
        public string SharePointConnectionAuthMode
        {
            get
            {
                return GetSharePointConnectionDataItem("SharePointConnectionAuthMode") as string;
            }
            set
            {
                SetSharePointConnectionDataItem("SharePointConnectionAuthMode", value);
            }
        }


        /// <summary>
        /// Gets or sets SharePointConnectionUserName.
        /// </summary>
        public string SharePointConnectionUserName
        {
            get
            {
                return GetSharePointConnectionDataItem("SharePointConnectionUserName") as string;
            }
            set
            {
                SetSharePointConnectionDataItem("SharePointConnectionUserName", value);
            }
        }


        /// <summary>
        /// Gets or sets SharePointConnectionDomain.
        /// </summary>
        public string SharePointConnectionDomain
        {
            get
            {
                return GetSharePointConnectionDataItem("SharePointConnectionDomain") as string;
            }
            set
            {
                SetSharePointConnectionDataItem("SharePointConnectionDomain", value);
            }
        }


        /// <summary>
        /// Gets or sets SharePointConnectionPassword.
        /// </summary>
        public string SharePointConnectionPassword
        {
            get
            {
                return GetSharePointConnectionDataItem("SharePointConnectionPassword") as string;
            }
            set
            {
                SetSharePointConnectionDataItem("SharePointConnectionPassword", value);
            }
        }


        /// <summary>
        /// Gets or sets SharePoint connection item by name.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <returns>Item value, or null if not present</returns>
        /// <seealso cref="GetSharePointConnectionDataItem"/>
        /// <seealso cref="SetSharePointConnectionDataItem"/>
        public object this[string name]
        {
            get
            {
                return GetSharePointConnectionDataItem(name);
            }
            set
            {
                SetSharePointConnectionDataItem(name, value);
            }
        }


        /// <summary>
        /// Gets list of connection item names.
        /// </summary>
        public List<string> SharePointConnectionDataItemNames
        {
            get
            {
                return mConnectionData.Keys.ToList();
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new empty SharePoint connection data.
        /// </summary>
        public SharePointConnectionData()
        {
            mConnectionData = new Dictionary<string, object>();
        }


        /// <summary>
        /// Creates new SharePoint connection data filled with connectionData.
        /// </summary>
        /// <param name="connectionData">Connection data</param>
        public SharePointConnectionData(Dictionary<string, object> connectionData)
        {
            mConnectionData = connectionData;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets connection data item for given name.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <returns>Item value, or null if not present</returns>
        public object GetSharePointConnectionDataItem(string name)
        {
            if (mConnectionData.ContainsKey(name))
            {
                return mConnectionData[name];
            }
            return null;
        }


        /// <summary>
        /// Sets connection data item for given name.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <param name="value">Item value</param>
        public void SetSharePointConnectionDataItem(string name, object value)
        {
            mConnectionData[name] = value;
        }


        /// <summary>
        /// Returns clone of SharePoint connection data.
        /// </summary>
        /// <returns>Clone of SharePoint connection data.</returns>
        public SharePointConnectionData Clone()
        {
            SharePointConnectionData clone = new SharePointConnectionData();
            foreach (KeyValuePair<string, object> connectionDataItem in mConnectionData)
            {
                clone[connectionDataItem.Key] = connectionDataItem.Value;
            }

            return clone;
        }

        #endregion
    }
}
