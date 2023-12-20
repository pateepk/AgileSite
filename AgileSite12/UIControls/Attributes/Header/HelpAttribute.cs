using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Settings for title of the page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class HelpAttribute : AbstractAttribute, ICMSFunctionalAttribute
    {
        #region "Properties"

        /// <summary>
        /// Help topic for the title.
        /// </summary>
        public string HelpTopic
        {
            get;
            set;
        }


        /// <summary>
        /// Help name to identify the help within the javascript.
        /// </summary>
        public string HelpName
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public HelpAttribute()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="helpTopic">Help topic</param>
        public HelpAttribute(string helpTopic)
        {
            HelpTopic = helpTopic;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="helpTopic">Help topic</param>
        /// <param name="helpName">Name of the help for javascript purposes</param>
        public HelpAttribute(string helpTopic, string helpName)
        {
            HelpTopic = helpTopic;
            HelpName = helpName;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Applies the attribute data to the page (object).
        /// </summary>
        /// <param name="sender">Sender object</param>
        public void Apply(object sender)
        {
            // Do not apply the attribute 
            if (!CheckEditedObject())
            {
                return;
            }

            if (sender is CMSPage)
            {
                // Let the page check the license
                CMSPage page = (CMSPage)sender;

                page.SetHelp(HelpTopic, HelpName);
            }
        }

        #endregion
    }
}