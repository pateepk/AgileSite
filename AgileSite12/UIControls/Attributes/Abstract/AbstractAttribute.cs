using System;
using System.Web.UI;

using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Abstract attribute class.
    /// </summary>
    public abstract class AbstractAttribute : Attribute, ICMSAttribute
    {
        #region "Properties"

        /// <summary>
        /// If True, attribute is applied only when edited object is defined.
        /// </summary>        
        public bool ExistingObject
        {
            get;
            set;
        }

        /// <summary>
        /// If True, attribute is applied only when edited object is not defined.
        /// </summary>        
        public bool NewObject
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the attribute contains macros
        /// </summary>
        public bool ContainsMacro
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the text for the element.
        /// </summary>
        /// <param name="resourceString">Resource string</param>
        /// <param name="text">Text, has higher priority</param>
        protected string GetText(string resourceString, string text)
        {
            string result = null;

            if (!String.IsNullOrEmpty(text))
            {
                // Resolve the macros if necessary
                if (MacroProcessor.ContainsMacro(text))
                {
                    ContainsMacro = true;

                    text = Resolve(text);
                }

                result = text;
            }
            if (String.IsNullOrEmpty(result) && (resourceString != null))
            {
                result = ResHelper.GetString(resourceString);
            }

            return result;
        }


        /// <summary>
        /// Transforms the URL to the correct format.
        /// </summary>
        /// <param name="page">Processing page</param>
        /// <param name="url">URL</param>
        protected string GetUrl(Page page, string url)
        {
            if (url != null)
            {
                // Resolve the macros if necessary
                if (MacroProcessor.ContainsMacro(url))
                {
                    ContainsMacro = true;

                    url = Resolve(url);
                }

                return page.ResolveUrl(url);
            }

            return null;
        }


        /// <summary>
        /// Resolves the give text.
        /// </summary>
        /// <param name="text">Text to resolve</param>
        protected string Resolve(string text)
        {
            // Encode the values
            var ctx = new MacroSettings()
            {
                EncodeResolvedValues = true,

                // We cannot check security in macros inserted via Attributes (no user to check against, could be any salt)
                CheckSecurity = false
            };

            return MacroResolver.Resolve(text, ctx);
        }


        /// <summary>
        /// Returns True if attribute should be applied. 
        /// Evaluation is based on the edited object existence and ExistingObject and NewObject properties.
        /// </summary>        
        protected bool CheckEditedObject()
        {
            bool objectExists = (UIContext.Current.EditedObject != null);

            if ((!ExistingObject && !NewObject) ||
                (objectExists && ExistingObject) ||
                (!objectExists && NewObject))
            {
                return true;
            }

            return false;
        }
        
        #endregion
    }
}