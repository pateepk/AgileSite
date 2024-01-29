using System;
using System.Web.UI;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Common interface for custom user controls used in content (in-line user controls).
    /// </summary>
    public abstract class InlineUserControl : AbstractUserControl, ISimpleDataContainer
    {
        #region "Variables"

        // If true, the ViewState has been already tracked.
        private bool mViewStateTracked = false;
        
        private StringSafeDictionary<object> mProperties = new StringSafeDictionary<object>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Control properties.
        /// </summary>
        protected StringSafeDictionary<object> Properties
        {
            get
            {
                return mProperties;
            }
            set
            {
                mProperties = value;
            }
        }

        
        /// <summary>
        /// Control parameter.
        /// </summary>
        public virtual string Parameter
        {
            get
            {
                return Convert.ToString(ViewState["Parameter"]);
            }
            set
            {
                ViewState["Parameter"] = value;
            }
        }


        /// <summary>
        /// ViewState - overridden for the Master page ViewState fix.
        /// </summary>
        protected override StateBag ViewState
        {
            get
            {
                // Track ViewState in first access
                if (!mViewStateTracked)
                {
                    TrackViewState();
                }

                return base.ViewState;
            }
        }


        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                SetValue(columnName, value);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Method that is called when the control content is loaded.
        /// </summary>
        public virtual void OnContentLoaded()
        {
        }


        /// <summary>
        /// Tracks the view state.
        /// </summary>
        protected override void TrackViewState()
        {
            base.TrackViewState();
            mViewStateTracked = true;
        }


        /// <summary>
        /// Returns true if the value of the given property is set.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public virtual bool HasValue(string propertyName)
        {
            return (Properties[propertyName] != null);
        }


        /// <summary>
        /// Returns the value of the given property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public virtual object GetValue(string propertyName)
        {
            return Properties[propertyName];
        }


        /// <summary>
        /// Returns the value of the given property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="defaultValue">Default value</param>
        public virtual ReturnType GetValue<ReturnType>(string propertyName, ReturnType defaultValue)
        {
            return ValidationHelper.GetValue(GetValue(propertyName), defaultValue);
        }


        /// <summary>
        /// Sets the property value of the control, setting the value affects only local property value.
        /// </summary>
        /// <param name="propertyName">Property name to set</param>
        /// <param name="value">New property value</param>
        public virtual bool SetValue(string propertyName, object value)
        {
            Properties[propertyName] = value;
            return true;
        }

        #endregion
    }
}