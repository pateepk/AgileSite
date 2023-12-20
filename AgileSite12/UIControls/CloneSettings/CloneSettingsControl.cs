using System.Collections;

using CMS.DataEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for clone settings controls.
    /// </summary>
    public class CloneSettingsControl : CMSUserControl
    {
        /// <summary>
        /// Gets or sets BaseInfo object to be cloned.
        /// </summary>
        public BaseInfo InfoToClone
        {
            get { return GetValue("InfoToClone") as BaseInfo; }
            set { SetValue("InfoToClone", value); }
        }


        /// <summary>
        /// Gets custom parameters
        /// </summary>
        public virtual Hashtable CustomParameters
        {
            get;
        }


        /// <summary>
        /// Gets the list of excluded child types (separated with semicolon).
        /// </summary>
        public virtual string ExcludedChildTypes
        {
            get;
        }


        /// <summary>
        /// Gets the list of excluded binding types (separated with semicolon).
        /// </summary>
        public virtual string ExcludedBindingTypes
        {
            get;
        }


        /// <summary>
        /// Gets the list of excluded other binding types (separated with semicolon).
        /// </summary>
        public virtual string ExcludedOtherBindingTypes
        {
            get;
        }


        /// <summary>
        /// Indicates if the control should be displayed. (Use for control which only needs to specify excluded types without any special settings).
        /// </summary>
        public virtual bool DisplayControl
        {
            get { return true; }
        }


        /// <summary>
        /// Indicates if the control should hide display name field even if display name is defined.
        /// </summary>
        public virtual bool HideDisplayName { get; }


        /// <summary>
        /// Indicates if the control should hide code name field even if code name is defined.
        /// </summary>
        public virtual bool HideCodeName
        {
            get { return false; }
        }


        /// <summary>
        /// Indicates if the codename should be validated.
        /// </summary>
        public virtual bool ValidateCodeName
        {
            get { return true; }
        }


        /// <summary>
        /// Defines a custom close refresh script.
        /// </summary>
        public virtual string CloseScript
        {
            get;
        }


        /// <summary>
        /// Returns true if custom settings are valid against given clone setting.
        /// </summary>
        /// <param name="settings">Clone settings</param>
        public virtual bool IsValid(CloneSettings settings)
        {
            return true;
        }
    }
}