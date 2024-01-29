using System;
using System.Linq;
using System.Text;
using System.Web.UI;

using CMS.Base;
using CMS.DataEngine;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Base checkBox grid item.
    /// </summary>
    public abstract class ImportExportBase : CMSUserControl
    {
        #region "Private variables"

        private StringBuilder sbAvailable = null;

        #endregion


        #region "Event handlers"

        /// <summary>
        /// Occurs when a button (All, None, ...) is pressed.
        /// </summary>
        public event EventHandler<EventArgs> ButtonPressed;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Current object type.
        /// </summary>
        public string ObjectType
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ObjectType"], ObjectHelper.GROUP_OBJECTS);
            }
            set
            {
                ViewState["ObjectType"] = value;
            }
        }


        /// <summary>
        /// True if site object.
        /// </summary>
        public bool SiteObject
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["SiteObject"], false);
            }
            set
            {
                ViewState["SiteObject"] = value;
            }
        }

        #endregion


        #region "Protected properties"

        /// <summary>
        /// Filter current object type.
        /// </summary>
        protected string FilterCurrentObjectType
        {
            get
            {
                return ValidationHelper.GetString(ViewState["FilterCurrentObjectType"], ObjectHelper.GROUP_OBJECTS);
            }
            set
            {
                ViewState["FilterCurrentObjectType"] = value;
            }
        }


        /// <summary>
        /// Filter current where condition.
        /// </summary>
        protected string FilterCurrentWhereCondition
        {
            get
            {
                return ValidationHelper.GetString(ViewState["FilterCurrentWhereCondition"], ObjectHelper.GROUP_OBJECTS);
            }
            set
            {
                ViewState["FilterCurrentWhereCondition"] = value;
            }
        }


        /// <summary>
        /// Available items
        /// </summary>
        protected StringBuilder AvailableItems
        {
            get
            {
                return sbAvailable ?? (sbAvailable = new StringBuilder());
            }
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Returns the name for the checkBox.
        /// </summary>
        /// <param name="codeName">CodeName.</param>
        /// <param name="prepos">Prefix for name.</param>
        /// <returns>CheckBox name.</returns>
        protected string GetCheckBoxName(object codeName, string prepos)
        {
            return prepos + ValidationHelper.GetIdentifier(codeName).ToLowerCSafe().Replace("'", "\'");
        }


        /// <summary>
        /// Returns CMSCheckBox with specified ID and name.
        /// </summary>
        /// <param name="codeNameColumnName">CodeName</param>
        /// <param name="prepos">Prefix for name.</param>
        /// <returns>CMSCheckBox</returns>
        protected CMSCheckBox GetCheckBox(string codeNameColumnName, string prepos)
        {
            var check = new CMSCheckBox
            {
                ID = GetCheckBoxId(codeNameColumnName, prepos),
                ClientIDMode = ClientIDMode.Static
            };
            check.Attributes.Add("name", GetCheckBoxName(codeNameColumnName, prepos));
            return check;
        }


        /// <summary>
        /// Returns the ID for the checkBox.
        /// </summary>
        /// <param name="prepos">Prefix for name.</param>
        /// <param name="codeName">Object code name</param>
        protected string GetCheckBoxId(object codeName, string prepos)
        {
            return prepos + ValidationHelper.GetIdentifier(codeName).ToLowerCSafe().Replace("'", "\'");
        }


        /// <summary>
        /// Add item to AvailableItems
        /// </summary>
        /// <param name="item">CheckBox codename</param>
        protected void AddAvailableItem(string item)
        {
            var sb = AvailableItems;
            if (sb.Length > 0)
            {
                sbAvailable.Append(";");
            }
            sbAvailable.Append(item);
        }


        /// <summary>
        /// Raises ButtonPressed event.
        /// </summary>
        protected void RaiseButtonPressed(object sender, EventArgs e)
        {
            if (ButtonPressed != null)
            {
                ButtonPressed(sender, e);
            }
        }

        #endregion
    }
}
