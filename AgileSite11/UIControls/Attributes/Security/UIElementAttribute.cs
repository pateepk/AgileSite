using System;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Initializes page with license and security checks. Performs redirect if security check fails. Works only with pages inherited from <see cref="CMSPage"/> class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UIElementAttribute : AbstractAttribute, ICMSSecurityAttribute
    {
        #region "Variables"

        bool mValidateDialogHash = true;
        bool mCheckPermissions = true;

        #endregion
        

        #region "Properties"

        /// <summary>
        /// Resource name
        /// </summary>
        public string ResourceName
        {
            get;
            set;
        }


        /// <summary>
        /// UI Element name
        /// </summary>
        public string ElementName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether hash for dialogs should be validated (use only for legacy pages)
        /// </summary>
        public bool ValidateDialogHash 
        {
            get
            {
                return mValidateDialogHash;
            }
            set
            {
                mValidateDialogHash = value;
            }
        }


        /// <summary>
        /// Indicates whether permission should be checked (incl. UI personalization, Permissions, Resource on site)
        /// </summary>
        public bool CheckPermissions
        {
            get
            {
                return mCheckPermissions;
            }
            set
            {
                mCheckPermissions = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="elementName">Element name</param>
        /// <param name="resourceName">Resource name</param>
        public UIElementAttribute(string resourceName, string elementName)
        {
            ResourceName = resourceName;
            ElementName = elementName;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="elementName">Element name</param>
        /// <param name="resourceName">Resource name</param>
        /// <param name="validateDialogHash">Indicates whether hash for dialogs should be validated (Disable for legacy pages only in special cases)</param>
        /// <param name="checkPermissions">Indicates whether permission should be checked (incl. UI personalization, Permissions, Resource on site)</param>
        public UIElementAttribute(string resourceName, string elementName, bool validateDialogHash, bool checkPermissions)
            :this(resourceName, elementName)
        {
            ValidateDialogHash = validateDialogHash;
            CheckPermissions = checkPermissions;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes page with license and security checks. Performs redirect if security check fails.
        /// </summary>
        /// <param name="page">Page from which is check performed</param>
        public void Check(CMSPage page)
        {
            // Set UI element context
            var ctx = UIContext.Current;

            ctx.ElementName = ElementName;
            ctx.ResourceName = ResourceName;
            
            // Init page with UI element
            CMSUIPage.InitPage(page, ctx, ValidateDialogHash, CheckPermissions);
        }

        #endregion
    }
}