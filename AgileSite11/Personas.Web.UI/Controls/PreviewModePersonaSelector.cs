using System;
using System.ComponentModel;
using System.Linq;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Personas.Web.UI
{
    /// <summary>
    /// Displays selector of site's personas and saves the selected one to the <see cref="IPreviewPersonaStorage"/>. 
    /// This control is used on the page preview mode.
    /// </summary>
    [ToolboxItem(false)]
    internal class PreviewModePersonasSelector : CMSPlaceHolder
    {
        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var personas = PersonaInfoProvider.GetPersonas().ToList();

            // Show nothing if there is no persona available
            if (!personas.Any())
            {
                Visible = false;
                return;
            }

            var btnSelectablePersonas = new PersonasSelectableButton();

            btnSelectablePersonas.DropDownItemsAlignment = CMSMultiButtonDropDownItemsAlignment.Right;
            btnSelectablePersonas.SetPersonas(personas);

            var previewPersonaStorage = PersonasFactory.GetPreviewPersonaStorage();

            btnSelectablePersonas.SelectedPersona = previewPersonaStorage.GetPreviewPersona();
            btnSelectablePersonas.Click += (o, args) =>
            {
                previewPersonaStorage.StorePreviewPersona(btnSelectablePersonas.SelectedPersona);

                // Selected persona is now stored in the session. Page must be refreshed to reflect the new 
                // value in the session.
                URLHelper.RefreshCurrentPage();
            };

            Controls.Add(btnSelectablePersonas);
        }
    }
}