using System;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.MacroEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Text transformation template.
    /// </summary>
    public class TextTransformationTemplate : WebControl, ITemplate
    {
        #region "Variables"

        private readonly string mText;
        private MacroResolver mContextResolver;
        private CMSAbstractTransformation mTransformation;

        #endregion


        #region "Properties"

        /// <summary>
        /// Web part context resolver.
        /// </summary>
        public virtual MacroResolver ContextResolver
        {
            get
            {
                if (mContextResolver == null)
                {
                    // Prepare the resolver
                    mContextResolver = MacroContext.CurrentResolver.CreateChild();
                    mContextResolver.Culture = Thread.CurrentThread.CurrentUICulture.ToString();
                    mContextResolver.OnGetValue += ContextResolver_OnGetValue;
                }

                return mContextResolver;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor, creates the template based on the given text.
        /// </summary>
        /// <param name="text">Transformation text</param>
        public TextTransformationTemplate(string text)
        {
            mText = text;
        }


        /// <summary>
        /// Instantiates the new transformation object.
        /// </summary>
        /// <param name="container">Control container</param>
        public void InstantiateIn(Control container)
        {
            var transformation = new CMSAbstractTransformation();

            // Insert the text as the literal control
            LiteralControl ltl = new LiteralControl(mText);
            ltl.ID = "ltl";
            ltl.EnableViewState = false;
            ltl.DataBinding += ltl_DataBinding;

            transformation.Controls.Add(ltl);

            // Add to the container
            container.Controls.Add(transformation);
        }


        /// <summary>
        /// Fires on the created literal data binding.
        /// </summary>
        protected void ltl_DataBinding(object sender, EventArgs e)
        {
            if (sender is LiteralControl)
            {
                // Resolve the macros in the literal
                LiteralControl ltl = (LiteralControl)sender;

                // Prepare the resolver
                mTransformation = (CMSAbstractTransformation)ltl.NamingContainer;
                ContextResolver.SetAnonymousSourceData(mTransformation.DataItem);

                // Resolve macros
                ltl.Text = ContextResolver.ResolveMacros(ltl.Text);
            }
        }


        /// <summary>
        /// Processes the dynamic values.
        /// </summary>
        /// <param name="name">Data item name</param>
        public object ContextResolver_OnGetValue(string name)
        {
            if (mTransformation == null)
            {
                return null;
            }

            if (String.IsNullOrEmpty(name))
            {
                return null;
            }

            switch (name.ToLowerInvariant())
            {
                case "getdocumenturl":
                    return mTransformation.GetDocumentUrl();

                case "getdocumentlink":
                    return mTransformation.GetDocumentLink();

                case "dataitemindex":
                    return mTransformation.DataItemIndex;

                case "displayindex":
                    return mTransformation.DisplayIndex;

                case "dataitemcount":
                    return mTransformation.DataItemCount;

                case "getdocumenturlforfeed":
                    return mTransformation.GetDocumentUrlForFeed();

                case "object":
                    return mTransformation.Object;

                case "dataitem":
                    return mTransformation.DataItem;
            }

            return null;
        }

        #endregion
    }
}