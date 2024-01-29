using System;

using CMS.Base.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Adds action to the page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ScriptAttribute : AbstractAttribute, ICMSFunctionalAttribute
    {
        #region "Properties"

        /// <summary>
        /// List of files to be registered.
        /// </summary>
        public string[] Files
        {
            get;
            set;
        }


        /// <summary>
        /// If true, JQuery is registered.
        /// </summary>
        public bool JQuery
        {
            get;
            set;
        }


        /// <summary>
        /// Registers the tooltip script.
        /// </summary>
        public bool Tooltip
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ScriptAttribute()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="files">Files to register</param>
        public ScriptAttribute(params string[] files)
        {
            Files = files;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Applies the attribute data to the page (object).
        /// </summary>
        /// <param name="sender">Sender object</param>
        public void Apply(object sender)
        {
            if (sender is CMSPage)
            {
                CMSPage page = (CMSPage)sender;

                if (JQuery)
                {
                    ScriptHelper.RegisterJQuery(page);
                }

                // Register Tooltip script
                if (Tooltip)
                {
                    ScriptHelper.RegisterTooltip(page);
                }

                // Register all files
                if (Files != null)
                {
                    foreach (string file in Files)
                    {
                        ScriptHelper.RegisterScriptFile(page, file);
                    }
                }
            }
            else
            {
                throw new Exception("[TabAttribute.Apply]: The attribute [Script] is only valid on CMSPage class.");
            }
        }

        #endregion
    }
}
