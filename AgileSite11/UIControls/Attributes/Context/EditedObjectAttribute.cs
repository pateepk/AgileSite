using System;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Defines the edited object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EditedObjectAttribute : AbstractAttribute, ICMSFunctionalAttribute
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


        /// <summary>
        /// URL of frameset to redirect if necessary to display more tabs than general tab only (Versioning, Object relationships, etc.).
        /// </summary>
        public string FrameSetUrl
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditedObjectAttribute()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="idQueryParameter">Name of the parameter in query string containing the object ID</param>
        public EditedObjectAttribute(string objectType, string idQueryParameter)
        {
            ObjectType = objectType;
            IDQueryParameter = idQueryParameter;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="idQueryParameter">Name of the parameter in query string containing the object ID</param>
        /// <param name="frameSetUrl">URL of frameset to redirect if necessary to display more tabs than general tab only (Versioning, Object relationships, etc.)</param>
        public EditedObjectAttribute(string objectType, string idQueryParameter, string frameSetUrl)
        {
            ObjectType = objectType;
            IDQueryParameter = idQueryParameter;
            FrameSetUrl = frameSetUrl;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the edited object.
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
            if (sender is CMSPage)
            {
                // Let the page check the license
                CMSPage page = (CMSPage)sender;

                if (!String.IsNullOrEmpty(ObjectType) && !String.IsNullOrEmpty(IDQueryParameter) && QueryHelper.Contains(IDQueryParameter))
                {
                    // Get the ID
                    int id = QueryHelper.GetInteger(IDQueryParameter, 0);
                    if (id > 0)
                    {
                        page.SetEditedObject(ProviderHelper.GetInfoById(ObjectType, id), FrameSetUrl);
                    }
                }
            }
            else
            {
                throw new Exception("[EditedObjectAttribute.Apply]: The attribute [EditedObject] is only valid on CMSPage class.");
            }
        }

        #endregion
    }
}