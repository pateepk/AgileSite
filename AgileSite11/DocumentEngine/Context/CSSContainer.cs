using System.Collections.Generic;

using CMS.DataEngine;
using CMS.PortalEngine;
using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// CSS Container
    /// </summary>
    public class CSSContainer : INameIndexable<ObjectProperty>, IHierarchicalObject
    {
        #region "Variables"

        private List<string> mProperties;
        private CSSWrapper mStylesheets;
        private CSSWrapper mContainers;
        private CSSWrapper mLayouts;
        private CSSWrapper mTemplates;
        private CSSWrapper mWebParts;
        private CSSWrapper mTransformations;

        #endregion


        #region "Properties"

        /// <summary>
        /// CSS Stylesheets
        /// </summary>
        public CSSWrapper Stylesheets
        {
            get
            {
                return mStylesheets ?? (mStylesheets = new CSSWrapper(CMSDataContext.Current.GlobalObjects[CssStylesheetInfo.OBJECT_TYPE], "StylesheetText"));
            }
        }


        /// <summary>
        /// Web part containers
        /// </summary>
        public CSSWrapper Containers
        {
            get
            {
                return mContainers ?? (mContainers = new CSSWrapper(CMSDataContext.Current.GlobalObjects[WebPartContainerInfo.OBJECT_TYPE], "ContainerCSS"));
            }
        }


        /// <summary>
        /// Page layouts
        /// </summary>
        public CSSWrapper Layouts
        {
            get
            {
                return mLayouts ?? (mLayouts = new CSSWrapper(CMSDataContext.Current.GlobalObjects[LayoutInfo.OBJECT_TYPE], "LayoutCSS"));
            }
        }


        /// <summary>
        /// Page templates
        /// </summary>
        public CSSWrapper Templates
        {
            get
            {
                return mTemplates ?? (mTemplates = new CSSWrapper(CMSDataContext.Current.GlobalObjects[PageTemplateInfo.OBJECT_TYPE], "PageTemplateCSS"));
            }
        }


        /// <summary>
        /// Web parts
        /// </summary>
        public CSSWrapper WebParts
        {
            get
            {
                return mWebParts ?? (mWebParts = new CSSWrapper(CMSDataContext.Current.GlobalObjects[WebPartInfo.OBJECT_TYPE], "WebPartCSS"));
            }
        }


        /// <summary>
        /// Transformations
        /// </summary>
        public CSSWrapper Transformations
        {
            get
            {
                return mTransformations ?? (mTransformations = new CSSWrapper(CMSDataContext.Current.Transformations, "TransformationCSS"));
            }
        }

        #endregion


        #region "IHierarchicalObject Members"

        /// <summary>
        /// Properties of the object available through GetProperty.
        /// </summary>
        public List<string> Properties
        {
            get
            {
                if (mProperties == null)
                {
                    // Build the list of properties
                    mProperties = new List<string>();

                    mProperties.Add("Stylesheets");
                    mProperties.Add("Transformations");
                    mProperties.Add("Containers");
                    mProperties.Add("Layouts");
                    mProperties.Add("Templates");
                    mProperties.Add("WebParts");

                    mProperties.Sort();
                }

                return mProperties;
            }
        }


        /// <summary>
        /// Returns property with given name (either object or property value).
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetProperty(string columnName)
        {
            object value;
            TryGetProperty(columnName, out value);

            return value;
        }


        /// <summary>
        /// Returns property with given name (either object or property value).
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public bool TryGetProperty(string columnName, out object value)
        {
            switch (columnName.ToLowerInvariant())
            {
                case "stylesheets":
                    value = Stylesheets;
                    return true;

                case "transformations":
                    value = Transformations;
                    return true;

                case "containers":
                    value = Containers;
                    return true;

                case "layouts":
                    value = Layouts;
                    return true;

                case "templates":
                    value = Templates;
                    return true;

                case "webparts":
                    value = WebParts;
                    return true;

            }

            value = this[columnName];
            if (value != null)
            {
                return true;
            }

            return false;
        }

        #endregion


        #region "INameIndexable<ObjectProperty> Members"

        /// <summary>
        /// Returns the CSS content of the given CSS stylesheet (by code name)
        /// </summary>
        /// <param name="name">Stylesheet name</param>
        public ObjectProperty this[string name]
        {
            get
            {
                return Stylesheets[name];
            }
            set
            {
                Stylesheets[name] = value;
            }
        }

        #endregion


        #region "INameIndexable Members"

        /// <summary>
        /// Returns the object registered by the specific name.
        /// </summary>
        /// <param name="name">Object name (indexer)</param>
        object INameIndexable.this[string name]
        {
            get
            {
                return this[name];
            }
        }

        #endregion
    }
}