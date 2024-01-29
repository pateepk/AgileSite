using System;

using CMS.DataEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Defines the edited object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ParentObjectAttribute : AbstractAttribute, ICMSFunctionalAttribute
    {
        #region "Properties"

        /// <summary>
        /// Object type.
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Query parameter to get the object ID.
        /// </summary>
        public string IDQueryParameter
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ParentObjectAttribute()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="idQueryParameter">Name of the parameter in query string containing the object ID</param>
        public ParentObjectAttribute(string objectType, string idQueryParameter)
        {
            ObjectType = objectType;
            IDQueryParameter = idQueryParameter;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the edited object parent.
        /// </summary>
        public BaseInfo GetObject()
        {
            if (!String.IsNullOrEmpty(ObjectType) && !String.IsNullOrEmpty(IDQueryParameter))
            {
                // Get the ID
                int id = QueryHelper.GetInteger(IDQueryParameter, 0);
                if (id > 0)
                {
                    return ProviderHelper.GetInfoById(ObjectType, id);
                }
            }

            return null;
        }


        /// <summary>
        /// Applies the attribute data to the page (object).
        /// </summary>
        /// <param name="sender">Sender object</param>
        public void Apply(object sender)
        {
            if (!String.IsNullOrEmpty(ObjectType) && !String.IsNullOrEmpty(IDQueryParameter) && QueryHelper.Contains(IDQueryParameter))
            {
                // Get the ID
                int id = QueryHelper.GetInteger(IDQueryParameter, 0);
                if (id > 0)
                {
                    UIContext.Current.EditedObjectParent = ProviderHelper.GetInfoById(ObjectType, id);
                }
            }
        }

        #endregion
    }
}