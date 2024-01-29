using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for object transformation
    /// </summary>
    public abstract class ObjectTransformationBase : PlaceHolder, IDataItemContainer
    {
        #region "Variables"

        private bool encodeOutput = true;
        private MacroResolver mContextResolver;
        private string mObjectType;

        /// <summary>
        /// Flag whether the data was already loaded or not.
        /// </summary>
        private bool mDataLoaded;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the object was already requested
        /// </summary>
        protected bool ObjectRequested
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates whether empty info should be used for objects limited by license.
        /// Use this property to true if object values need not to be correct and the purpose of the displayed object is object itself.
        /// </summary>
        public bool UseEmptyInfoForObjectLimitedByLicense
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the component is allowed to get the objects one by one directly from provider if not found registered. Default false.
        /// </summary>
        public bool DirectIfNotCached
        {
            get;
            set;
        }
        

        /// <summary>
        /// Object type.
        /// </summary>
        public string ObjectType
        {
            get
            {
                return mObjectType;
            }
            set
            {
                mObjectType = value;

                RegisterObject();
            }
        }


        /// <summary>
        /// Transformation - Column name (e.g. "ResourceName"), Internal UniGrid transformation (e.g. "#yesno") or a macro (e.g. "{% FirstName %} {% LastName %}").
        /// </summary>
        public string Transformation
        {
            get;
            set;
        }


        /// <summary>
        /// Transformation name.
        /// Supports the following formats of transformation name:
        ///     [Some text with macros] - Inline transformation
        ///     cms.user.sometransformation - Transformation full name from database
        ///     ~/SomePath/SomeControl.ascx - Path to transformation user control
        /// </summary>
        public string TransformationName
        {
            get;
            set;
        }
        

        /// <summary>
        /// Transformation used in case the object was not found
        /// </summary>
        public string NoDataTransformation
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the output localizes the string macros.
        /// </summary>
        public bool LocalizeStrings
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the output is encoded. Default value is true.
        /// </summary>
        public bool EncodeOutput
        {
            get
            {
                return encodeOutput;
            }
            set
            {
                encodeOutput = value;
            }
        }
        

        /// <summary>
        /// Macro resolver to use
        /// </summary>
        public MacroResolver ContextResolver
        {
            get
            {
                return mContextResolver ?? (mContextResolver = MacroResolver.GetInstance().CreateChild());
            }
            set
            {
                mContextResolver = value;
            }
        }


        /// <summary>
        /// Resolved text
        /// </summary>
        private string Text
        {
            get;
            set;
        }


        /// <summary>
        /// Data item for the transformation template
        /// </summary>
        public object DataItem
        {
            get;
            protected set;
        }


        /// <summary>
        /// Data item index
        /// </summary>
        public int DataItemIndex
        {
            get
            {
                return 0;
            }
        }


        /// <summary>
        /// Display index
        /// </summary>
        public int DisplayIndex
        {
            get
            {
                return 0;
            }
        }

        #endregion


        #region "Abstract methods"

        /// <summary>
        /// Gets the default parameter for the transformation
        /// </summary>
        protected abstract object GetDefaultParameter();

        /// <summary>
        /// Register required object.
        /// </summary>
        protected abstract void RegisterObject();


        /// <summary>
        /// Gets the object used by the transformation
        /// </summary>
        protected abstract IDataContainer GetObject();

        #endregion


        #region "Methods"


        /// <summary>
        /// Constructor.
        /// </summary>
        protected ObjectTransformationBase()
        {
            LocalizeStrings = true;
            NoDataTransformation = String.Empty;
        }
        

        /// <summary>
        /// Renders the control.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (!String.IsNullOrEmpty(TransformationName))
            {
                // Load and data bind the template if available
                DataItem = GetObject();

                ITemplate template = null;
                if (DataItem == null)
                {
                    if (!String.IsNullOrEmpty(NoDataTransformation))
                    {
                        template = new TextTransformationTemplate(NoDataTransformation);
                    }
                }
                else
                {
                    template = TransformationHelper.LoadTransformation(this, TransformationName);
                }

                if (template != null)
                {
                    template.InstantiateIn(this);
                    DataBind();
                }
            }
            else
            {
                // Load with transformation text
                LoadText(false);

                if (Text != null)
                {
                    writer.Write(Text);
                }
            }

            base.Render(writer);
        }


        /// <summary>
        /// Loads the transformation data.
        /// </summary>
        private void LoadText(bool forceReload)
        {
            if (mDataLoaded && !forceReload)
            {
                return;
            }

            mDataLoaded = true;

            Text = ResolveTransformationText();
        }


        /// <summary>
        /// Gets the result of the transformation.
        /// </summary>
        private string ResolveTransformationText()
        {
            // Get object and translate
            var obj = GetObject();

            string transformation = (obj != null) ? Transformation : NoDataTransformation;

            var parameter = GetDefaultParameter();

            if ((obj == null) || MacroProcessor.ContainsMacro(transformation))
            {
                // Resolve macros
                if (obj != null)
                {
                    ContextResolver.SetAnonymousSourceData(obj);
                    ContextResolver.SetNamedSourceData(TypeHelper.GetNiceName(ObjectType), obj);
                    ContextResolver.SetNamedSourceData("Object", obj);
                }

                parameter = ContextResolver.ResolveMacros(transformation);
            }
            else
            {
                // Apply transformation if specified
                int hashIndex = transformation.IndexOf('#');
                if (hashIndex >= 0)
                {
                    // Handle as column name with internal transformation
                    string colName = transformation.Substring(0, hashIndex).Trim();

                    parameter = obj.GetValue(colName);

                    // Try to find the transformation
                    string tr = transformation.Substring(hashIndex).Trim();

                    UniGridTransformations.Global.ExecuteTransformation(this, tr, ref parameter);
                }
                else
                {
                    // Handle as column name
                    parameter = obj.GetValue(transformation.Trim());
                }
            }

            // Localize
            string text = ValidationHelper.GetString(parameter, String.Empty);
            if (LocalizeStrings)
            {
                text = ResHelper.LocalizeString(text);
            }

            return EncodeOutput ? HTMLHelper.HTMLEncode(text) : text;
        }


        /// <summary>
        /// Overrides base method and returns transformed result as a text.
        /// </summary>
        public override string ToString()
        {
            if (!String.IsNullOrEmpty(TransformationName))
            {
                // Templated transformations not supported
                return "";
            }

            LoadText(false);

            return Text;
        }

        #endregion
    }
}
