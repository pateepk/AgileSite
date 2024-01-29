using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// View model for campaign assets.
    /// </summary>
    public class CampaignAssetViewModel
    {
        private Dictionary<string, object> mAdditionalProperties;
        
        /// <summary>
        /// Asset type.
        /// </summary>
        public string Type
        {
            get;
            set;
        }


        /// <summary>
        /// Asset ID.
        /// </summary>
        public int ID
        {
            get;
            set;
        }


        /// <summary>
        /// Asset info ID.
        /// </summary>
        public int AssetID
        {
            get;
            set;
        }


        /// <summary>
        /// Asset name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Direct asset link.
        /// </summary>
        public string Link
        {
            get;
            set;
        }


        /// <summary>
        /// ID of campaign this asset belongs to. Used when creating or updating asset.
        /// </summary>
        public int CampaignID
        {
            get;
            set;
        }


        /// <summary>
        /// Contains properties required by specific types of campaign assets.
        /// </summary>
        public Dictionary<string, object> AdditionalProperties
        {
            get
            {
                return mAdditionalProperties ?? (mAdditionalProperties = new Dictionary<string, object>());
            }
            set
            {
                mAdditionalProperties = value;
            }
        }
    }
}
