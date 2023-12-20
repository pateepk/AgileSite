using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Settings for title of the page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class TitleAttribute : AbstractAttribute, ICMSFunctionalAttribute
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
        /// <param name="resourceString">Resource string for the title</param>    
        public TitleAttribute(string resourceString)
        {
            HelpName = "helpTopic";
            ResourceString = resourceString;
        }


        /// <summary>
        /// Default constructor.
        /// </summary>
        public TitleAttribute()
            : this(null)
        {
            // Do not merge constructor overloads until .NET 4.0 is supported
            // There is an error in .NET compiler: http://connect.microsoft.com/VisualStudio/feedback/details/574497/optional-parameter-of-type-string-in-a-attribute-constructor-cannot-be-null
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

                page.SetTitle(GetText(ResourceString, Text));
                page.SetHelp(HelpTopic, HelpName);
            }
        }

        #endregion
    }
}