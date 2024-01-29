using System;
using System.Web.UI;

namespace CMS.UIControls.UniGridConfig
{
    /// <summary>
    /// Represents <see cref="UniGrid"/> mass action configuration.
    /// </summary>
    [ParseChildren(true)]
    public class MassAction
    {
        /// <summary>
        /// Unique name of mass action.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Caption for mass action in drop-down.
        /// </summary>
        public string Caption
        {
            get;
            set;
        }


        /// <summary>
        /// Type of interaction when performing the mass action.
        /// </summary>
        public MassActionTypeEnum Behavior
        {
            get;
            set;
        }


        /// <summary>
        /// URL to a page that executes the mass action.
        /// </summary>
        public String Url
        {
            get;
            set;
        }


        /// <summary>
        /// Returns a <see cref="string"/> which represents the object instance.
        /// </summary>
        public override string ToString()
        {
            return String.Format(
                "Name: {1}{0}Caption: {2}{0}Behavior: {3}{0}Url: {4}{0}",
                Environment.NewLine,
                Name,
                Caption,
                Behavior,
                Url);
        }
    }
}
