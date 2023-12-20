using System;

using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Form control for bundle inventory selection.
    /// </summary>
    public class SelectBundleInventoryType : FormControl
    {
        #region "Public properties"

        /// <summary>
        /// Gets or sets the bundle inventory type.
        /// </summary>
        public BundleInventoryTypeEnum BundleInventoryType
        {
            get
            {
                string type = ValidationHelper.GetString(Value, String.Empty);
                return type.ToEnum<BundleInventoryTypeEnum>();
            }
            set
            {
                Value = value.ToStringRepresentation();
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public SelectBundleInventoryType()
        {
            FormControlName = "BundleInventoryTypeSelector";
        }
    }
}