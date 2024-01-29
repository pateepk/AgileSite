using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a mapping of CMS objects to SalesForce entities.
    /// </summary>
    public sealed class Mapping
    {

        #region "Private members"

        private string mExternalIdentifierAttributeName;
        private string mExternalIdentifierAttributeLabel;
        private List<MappingItem> mItems;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets or sets a name of the SalesForce entity attribute that contains the identifier of the bound CMS object.
        /// </summary>
        public string ExternalIdentifierAttributeName
        {
            get
            {
                return mExternalIdentifierAttributeName;
            }
            set
            {
                mExternalIdentifierAttributeName = value;
            }
        }

        /// <summary>
        /// Gets or sets a label of the SalesForce entity attribute that contains the identifier of the bound CMS object.
        /// </summary>
        public string ExternalIdentifierAttributeLabel
        {
            get
            {
                return mExternalIdentifierAttributeLabel;
            }
            set
            {
                mExternalIdentifierAttributeLabel = value;
            }
        }

        /// <summary>
        /// Gets a collection of mapping items.
        /// </summary>
        public IEnumerable<MappingItem> Items
        {
            get
            {
                return mItems.AsEnumerable();
            }
        }

        #endregion

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the Mapping class.
        /// </summary>
        public Mapping()
        {
            mItems = new List<MappingItem>();
        }

        /// <summary>
        /// Initializes a new instance of the Mapping class using the specified initial values.
        /// </summary>
        /// <param name="externalIdentifierAttributeName">The name of the SalesForce entity attribute that contains the identifier of the bound CMS object.</param>
        /// <param name="externalIdentifierAttributeLabel">The label of the SalesForce entity attribute that contains the identifier of the bound CMS object.</param>
        /// <param name="items">A collection of mapping items.</param>
        public Mapping(string externalIdentifierAttributeName, string externalIdentifierAttributeLabel, IEnumerable<MappingItem> items)
        {
            mExternalIdentifierAttributeName = externalIdentifierAttributeName;
            mExternalIdentifierAttributeLabel = externalIdentifierAttributeLabel;
            HashSet<string> attributeNames = new HashSet<string>();
            foreach (MappingItem item in items)
            {
                if (!attributeNames.Add(item.AttributeName))
                {
                    throw new ArgumentException("[Mapping.Mapping]: Duplicated attribute name.");
                }
            }
            mItems = new List<MappingItem>(items);
        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Adds the specified item to this mapping.
        /// </summary>
        /// <param name="item">The mapping item to add.</param>
        public void Add(MappingItem item)
        {
            if (mItems.Any(x => x.AttributeName == item.AttributeName))
            {
                throw new ArgumentException("[Mapping.Add]: Duplicated attribute name.");
            }
            mItems.Add(item);
        }

        /// <summary>
        /// Searches for a mapping item with the specified entity attribute name, and returns it.
        /// </summary>
        /// <param name="attributeName">The entity attribute name.</param>
        /// <returns>A mapping item with the specified entity attribute name, if found; otherwise, null.</returns>
        public MappingItem GetItem(string attributeName)
        {
            return mItems.SingleOrDefault(x => x.AttributeName == attributeName);
        }

        #endregion

    }

}