using System;
using System.Linq;
using System.Text;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.UIControls;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Inline edit for discount priority.
    /// </summary>
    public class PriorityInlineEdit : InlineEditingTextBox
    {
        /// <summary>
        /// Unigrid to reload after priority changed.
        /// </summary>
        public UniGrid Unigrid
        {
            get;
            set;
        }


        /// <summary>
        /// Edited object.
        /// </summary>
        public IPrioritizable PrioritizableObject
        {
            get;
            set;
        }


        /// <summary>
        /// Initialize value of textbox.
        /// </summary>
        protected override void OnLoad(EventArgs ev)
        {
            base.OnLoad(ev);

            if (PrioritizableObject != null)
            {
                Text = PrioritizableObject.Priority.ToString();
            }
        }


        /// <summary>
        /// Sets priority according discount type.
        /// </summary>
        protected override void OnUpdate(EventArgs e)
        {
            base.OnUpdate(e);

            if (PrioritizableObject == null)
            {
                return;
            }

            // Get new priority
            var priority = ValidationHelper.GetDouble(Text, -1);

            // Do not update same priority
            if (PrioritizableObject.Priority == priority)
            {
                return;
            }

            // Update priority
            var error = PrioritizableObject.TryUpdatePriority(priority);

            // Show error
            if (!String.IsNullOrEmpty(error))
            {
                ShowError(error);
                return;
            }

            // Reload unigrid
            Unigrid?.ReloadData();
        }


        /// <summary>
        /// Shows error in textbox and unigrid.
        /// </summary>
        private void ShowError(string text)
        {
            ErrorText = text;
            Unigrid?.ShowError(text);
        }
    }
}
