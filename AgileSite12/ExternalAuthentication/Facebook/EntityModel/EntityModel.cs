using System.Collections.Generic;
using System.Linq;

namespace CMS.ExternalAuthentication.Facebook
{
    /// <summary>
    /// Represents a model of a Facebook API entity.
    /// </summary>
    public sealed class EntityModel
    {
        #region "Variables"

        /// <summary>
        /// The dictionary that holds pairs of entity attribute name and the corresponding attribute model.
        /// </summary>
        private Dictionary<string, EntityAttributeModel> mItems;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the array of attribute models.
        /// </summary>
        public EntityAttributeModel[] Items
        {
            get
            {
                return mItems.Values.ToArray();
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the EntityModel class.
        /// </summary>
        internal EntityModel()
        {
            mItems = new Dictionary<string, EntityAttributeModel>();
        }


        /// <summary>
        /// Initializes a new instance of the EntityModel class with the collection of attribute models.
        /// </summary>
        /// <param name="source">The collection of attribute models.</param>
        internal EntityModel(IEnumerable<EntityAttributeModel> source) : this()
        {
            foreach (EntityAttributeModel attributeInfo in source)
            {
                mItems.Add(attributeInfo.Name, attributeInfo);
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Searches for an attribute model with the specified name, and returns it.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <returns>The attribute model with the specified name, if found; otherwise, null.</returns>
        public EntityAttributeModel GetAttributeModel(string name)
        {
            if (mItems.ContainsKey(name))
            {
                return mItems[name];
            }

            return null;
        }

        #endregion
    }

}