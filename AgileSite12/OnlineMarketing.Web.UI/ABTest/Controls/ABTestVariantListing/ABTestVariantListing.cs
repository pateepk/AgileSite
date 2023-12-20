using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Class representing a web component used to display and edit A/B tests variants.
    /// </summary>
    internal class ABTestVariantListing : WebControl
    {
        /// <summary>
        /// List of items to be rendered in the list.
        /// </summary>
        public IList<ABTestVariantListItem> ListItems { get; set; }


        /// <summary>
        /// Gets or sets identifier of an item that should be selected in the listing.
        /// </summary>
        /// <seealso cref="ABTestVariantListItem.Key"/>
        public string SelectedItemIdentifier { get; set; }


        /// <summary>
        /// Gets or sets the Javascript function to be called when an item is selected. 
        /// The function receives an object as a parameter containing data related to the selected item.
        /// </summary>
        public string SelectItemCallback { get; set; }


        /// <summary>
        /// Gets or sets the Javascript function to be called when an item is edited. 
        /// The function receives an object as a parameter containing data related to the edited item.
        /// </summary>
        /// <remarks>
        /// Name of a function with given signature: 'function(editedItem, newName)' is expected here.
        /// </remarks>
        public string EditItemCallback { get; set; }


        /// <summary>
        /// Gets or sets the Javascript function to be called when an item is removed. 
        /// The function receives an object as a parameter containing data related to the removed item.
        /// </summary>
        /// <remarks>
        /// Name of a function with given signature: 'function(itemToRemove)' is expected here.
        /// </remarks>
        public string RemoveItemCallback { get; set; }


        /// <summary>
        /// Maximum length of the variant name.
        /// </summary>
        public int? MaximumNameLength { get; set; }


        /// <summary>
        /// Gets or sets the title of the edit action icon.
        /// </summary>
        public string EditActionIconTitle { get; set; } = ResHelper.GetString("abtesting.renamevariant");


        /// <summary>
        /// Gets or sets the title of the remove action icon.
        /// </summary>
        public string RemoveActionIconTitle { get; set; } = ResHelper.GetString("abtesting.removevariant");

        
        /// <summary>
        /// Gets or sets the title displayed on disabled actions.
        /// </summary>
        public string ActionDisabledTitle { get; set; } = ResHelper.GetString("abtest.popupdialog.cannotmodifyabtest");


        /// <summary>
        /// Gets or sets the remove action confirmation message.
        /// </summary>
        public string RemoveConfirmationMessage { get; set; } = ResHelper.GetString("abtesting.removevariant.confirmation");                


        /// <summary>
        /// Creates a new instance of <see cref="ABTestVariantListing"/> class.
        /// </summary>
        public ABTestVariantListing()
            : base("kentico-abtest-variant-listing")
        {
        }


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!String.IsNullOrEmpty(SelectedItemIdentifier))
            {
                Attributes.Add("active-item-identifier", SelectedItemIdentifier);
            }
            if (MaximumNameLength.HasValue) 
            {
                Attributes.Add("maximum-name-length", MaximumNameLength.ToString());
            }
            
            ScriptHelper.RegisterWebComponentsScript(Page);
            InitializeJs();
        }


        private void InitializeJs()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                StringEscapeHandling = StringEscapeHandling.EscapeHtml,
            };

            var items = JsonConvert.SerializeObject(ListItems, serializerSettings);

            var itemSelectedScript = String.IsNullOrEmpty(SelectItemCallback) ? null : @"
list.selectVariant = function(variant) {
    var wrappedItem = { id: variant.key };
" + SelectItemCallback + @"(wrappedItem);
};";

            var itemEditedScript = String.IsNullOrEmpty(EditItemCallback) ? null : @"
list.renameVariant = function(variant) {
    var wrappedItem = { id: variant.key };
" + EditItemCallback + @"(wrappedItem, variant.name); 
};";

            var itemRemovedScript = String.IsNullOrEmpty(RemoveItemCallback) ? null : @"
list.removeVariant = function(variantKey) {
    var wrappedItem = { id: variantKey };
" + RemoveItemCallback + @"(wrappedItem);
};";

            ScriptHelper.RegisterStartupScript(this, typeof(string), Guid.NewGuid().ToString(), @"
(function(document) {
var list = document.getElementById('" + ClientID + @"');

list.editActionIconTitle = " + ScriptHelper.GetString(EditActionIconTitle) + @";
list.removeActionIconTitle = " + ScriptHelper.GetString(RemoveActionIconTitle) + @";
list.actionDisabledTitle = " + ScriptHelper.GetString(ActionDisabledTitle) + @";
list.removeConfirmationMessage = " + ScriptHelper.GetString(RemoveConfirmationMessage) + @";

list.variants = " + items + @";
"+ itemSelectedScript + @"
"+ itemEditedScript + @"
"+ itemRemovedScript + @"
})(document);", true);
        }
    }
}
