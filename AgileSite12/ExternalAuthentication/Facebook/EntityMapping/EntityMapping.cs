using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.FormEngine;

namespace CMS.ExternalAuthentication.Facebook
{

    /// <summary>
    /// Represents a mapping between a CMS object and a Facebook API entity.
    /// </summary>
    public sealed class EntityMapping
    {
        #region "Variables"

        /// <summary>
        /// The dictionary that holds pairs of a form field name and related entity mapping item.
        /// </summary>
        private Dictionary<string, EntityMappingItem> mItems;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the array of entity mapping items.
        /// </summary>
        public IEnumerable<EntityMappingItem> Items
        {
            get
            {
                return mItems.Values.ToArray();
            }
        }


        /// <summary>
        /// Gets a value that indicates whether this mapping contains items.
        /// </summary>
        public bool HasItems
        {
            get
            {
                return mItems.Count > 0;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the EntityMapping class.
        /// </summary>
        public EntityMapping()
        {
            mItems = new Dictionary<string, EntityMappingItem>();
        }


        /// <summary>
        /// Initializes a new instance of the EntityMapping class with the collection of entity mapping items.
        /// </summary>
        /// <param name="source">The collection of entity mapping items.</param>
        internal EntityMapping(IEnumerable<EntityMappingItem> source) : this()
        {
            foreach (EntityMappingItem item in source)
            {
                if (mItems.ContainsKey(item.FieldName))
                {
                    throw new Exception("[EntityMapping.EntityMapping]: Duplicated field name");
                }
                mItems.Add(item.FieldName, item);
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Adds the specified entity mapping item to this mapping.
        /// </summary>
        /// <param name="attributeModel">The entity attribute name.</param>
        /// <param name="fieldInfo">The form field name.</param>
        /// <param name="scope">The form field scope.</param>
        public void Add(EntityAttributeModel attributeModel, FormFieldInfo fieldInfo, string scope = null)
        {
            string fieldName = fieldInfo.Name;
            if (!string.IsNullOrEmpty(scope))
            {
                fieldName = String.Format("{0}.{1}", scope, fieldName);
            }
            if (mItems.ContainsKey(fieldName))
            {
                throw new Exception(String.Format("[EntityMapping.Add]: Duplicated field name ({0})", fieldName));
            }
            EntityMappingItem item = new EntityMappingItem(attributeModel.Name, fieldName);
            mItems.Add(fieldName, item);
        }


        /// <summary>
        /// Creates a new entity mapping for the specified scope, and returns it.
        /// </summary>
        /// <param name="scope">The form field scope.</param>
        /// <returns>A new entity mapping for the specified scope.</returns>
        public EntityMapping GetFromScope(string scope)
        {
            IEnumerable<EntityMappingItem> items = null;
            if (String.IsNullOrEmpty(scope))
            {
                items = mItems.Values.Where(x => !x.FieldName.Contains('.'));
            }
            else
            {
                string prefix = scope + '.';
                items = mItems.Values.Where(x => x.FieldName.StartsWithCSafe(prefix)).Select(x => new EntityMappingItem(x.AttributeName, x.FieldName.Substring(prefix.Length)));
            }

            return new EntityMapping(items);
        }


        /// <summary>
        /// Retrieves a collection of Facebook permission scope names required to obtain required values of Facebook API object, and returns it.
        /// </summary>
        /// <param name="model">The model of the Facebook API object.</param>
        /// <returns>A collection of Facebook permission scope names required to obtain required values of Facebook API object.</returns>
        public string[] GetFacebookPermissionScopeNames(EntityModel model)
        {
            HashSet<string> scopeNames = new HashSet<string>();
            foreach (EntityMappingItem item in mItems.Values)
            {
                EntityAttributeModel attributeModel = model.GetAttributeModel(item.AttributeName);
                if (attributeModel != null && !String.IsNullOrEmpty(attributeModel.FacebookPermissionScopeName))
                {
                    scopeNames.Add(attributeModel.FacebookPermissionScopeName);
                }
            }

            return scopeNames.ToArray();
        }

        #endregion

    }

}