using System;

using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Displays the information on the page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InformationAttribute : AbstractSecurityAttribute, ICMSFunctionalAttribute
    {
        #region "Properties"

        /// <summary>
        /// Text for the title, has higher priority than the resource string.
        /// </summary>
        public string Text
        {
            get;
            set;
        }


        /// <summary>
        /// Resource string for the title.
        /// </summary>
        public string ResourceString
        {
            get;
            set;
        }


        /// <summary>
        /// If set, the information text is displayed only if the given setting is true, otherwise, the false text is displayed.
        /// </summary>
        public string CheckSettings
        {
            get;
            set;
        }


        /// <summary>
        /// Text for the information in case the check doesn't succeed, has higher priority than the resource string.
        /// </summary>
        public string FalseText
        {
            get;
            set;
        }


        /// <summary>
        /// Resource string for the information in case the check doesn't succeed.
        /// </summary>
        public string FalseResourceString
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public InformationAttribute()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resourceString">Resource string for the information</param>
        public InformationAttribute(string resourceString)
        {
            ResourceString = resourceString;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the text is allowed to be shown.
        /// </summary>
        public bool ShowText()
        {
            // Check if the text can be displayed
            bool showText = CheckSecurity();
            if (!String.IsNullOrEmpty(CheckSettings) && !SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + "." + CheckSettings))
            {
                showText = false;
            }

            return showText;
        }


        /// <summary>
        /// Applies the attribute data to the page (object).
        /// </summary>
        /// <param name="sender">Sender object</param>
        public virtual void Apply(object sender)
        {
            if (sender is CMSPage)
            {
                // Let the page check the license
                CMSPage page = (CMSPage)sender;

                // Set the appropriate information to the page
                if (ShowText())
                {
                    page.ShowInformation(GetText(ResourceString, Text));
                }
                else
                {
                    page.ShowInformation(GetText(FalseResourceString, FalseText));
                }
            }
            else
            {
                throw new Exception("[TabAttribute.Apply]: The attribute [Information] is only valid on CMSPage class.");
            }
        }

        #endregion
    }
}