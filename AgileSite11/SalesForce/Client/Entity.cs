using System;
using System.Collections.Generic;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a SalesForce object.
    /// </summary>
    public sealed class Entity
    {

        #region "Private members"

        private EntityModel mEntityModel;
        private Dictionary<string, EntityAttribute> mEntityAttributes;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets the entity model.
        /// </summary>
        public EntityModel Model
        {
            get
            {
                return mEntityModel;
            }
        }

        /// <summary>
        /// Gets a collection of entity attributes.
        /// </summary>
        public IEnumerable<EntityAttribute> Attributes
        {
            get
            {
                return mEntityAttributes.Values;
            }
        }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        public string Id
        {
            get
            {
                EntityAttribute attribute = null;
                if (!mEntityAttributes.TryGetValue("Id", out attribute))
                {
                    return null;
                }

                return (string)attribute.Value;
            }
            set
            {
                SetAttributeValue("Id", value);
            }
        }

        /// <summary>
        /// Gets or sets the specified attribute value.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>The specified attribute value.</returns>
        public object this[string attributeName]
        {
            get
            {
                return GetAttributeValue<object>(attributeName);
            }
            set
            {
                SetAttributeValue(attributeName, value);
            }
        }

        #endregion

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the Entity class using the specified model.
        /// </summary>
        /// <param name="entityModel">The entity model.</param>
        public Entity(EntityModel entityModel)
        {
            mEntityModel = entityModel;
            mEntityAttributes = new Dictionary<string, EntityAttribute>();
        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Gets the specified attribute value.
        /// </summary>
        /// <typeparam name="T">The attribute value type.</typeparam>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>The specified attribute value.</returns>
        public T GetAttributeValue<T>(string attributeName)
        {
            EntityAttribute attribute = null;
            if (!mEntityAttributes.TryGetValue(attributeName, out attribute))
            {
                throw new ArgumentException("[Entity.GetAttributeValue]: Invalid attribute name.");
            }

            return (T)attribute.Value;
        }

        #endregion

        #region "Private methods"

        private void SetAttributeValue(string attributeName, object value)
        {
            EntityAttribute attribute = null;
            if (mEntityAttributes.TryGetValue(attributeName, out attribute))
            {
                attribute.Value = value;
            }
            else
            {
                EntityAttributeModel attributeModel = mEntityModel.GetAttributeModel(attributeName);
                if (attributeModel == null)
                {
                    throw new ArgumentException("[Entity.SetAttributeValue]: Invalid attribute name.");
                }
                attribute = new EntityAttribute(attributeModel);
                attribute.Value = value;
                mEntityAttributes.Add(attributeName, attribute);
            }
        }

        #endregion

    }

}