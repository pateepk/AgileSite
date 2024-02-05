using System;
using System.Linq;
using System.Text;


namespace CMS.EmailEngine
{
    /// <summary>
    /// Item in email template type register.
    /// </summary>
    /// <remarks>
    /// Instances of this class are immutable.
    /// </remarks>
    public class EmailTemplateTypeRegisterItem
    {
        /// <summary>
        /// Code name of the register item.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }


        /// <summary>
        /// Resource string for display name of the register item.
        /// </summary>
        public string DisplayNameResourceString
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates if email template type is global only.
        /// </summary>
        public bool IsGlobalOnly
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates a new register item.
        /// </summary>
        /// <param name="name">Code name of the item.</param>
        /// <param name="displayNameResourceString">Display name resource string.</param>
        public EmailTemplateTypeRegisterItem(string name, string displayNameResourceString)
            : this(name, displayNameResourceString, false)
        {
        }


        /// <summary>
        /// Creates a new register item.
        /// </summary>
        /// <param name="name">Code name of the item.</param>
        /// <param name="displayNameResourceString">Display name resource string.</param>
        /// <param name="isGlobalOnly">Indicates if email template type is global only.</param>
        public EmailTemplateTypeRegisterItem(string name, string displayNameResourceString, bool isGlobalOnly)
        {
            Name = name;
            DisplayNameResourceString = displayNameResourceString;
            IsGlobalOnly = isGlobalOnly;
        }
    }
}
