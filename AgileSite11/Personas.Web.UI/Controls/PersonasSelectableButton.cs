using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using CMS.Core;
using CMS.Base.Web.UI;

namespace CMS.Personas.Web.UI
{
    /// <summary>
    /// Nice looking dropdown with personas.
    /// </summary>
    [ToolboxItem(false)]
    internal class PersonasSelectableButton : CMSSelectableToggleButton
    {
        private const int PICTURE_SIDE_SIZE = 36;
        private const string NO_PERSONA_ACTION_NAME = "##nopersona##";


        /// <summary>
        /// Personas in this dropdown for further use.
        /// </summary>
        private Dictionary<string, PersonaInfo> mPersonas;


        private readonly IPersonaPictureUrlCreator mPersonaPictureUrlCreator = PersonasFactory.GetPersonaPictureUrlCreator();


        /// <summary>
        /// Currently selected persona. Becomes available on the PreLoad phase.
        /// </summary>
        public PersonaInfo SelectedPersona
        {
            get
            {
                if (mPersonas == null)
                {
                    return null;
                }

                PersonaInfo persona;
                if (mPersonas.TryGetValue(SelectedActionName, out persona))
                {
                    return persona;
                }

                return null;
            }
            set
            {
                SelectedActionName = value == null ? NO_PERSONA_ACTION_NAME : value.PersonaName;
            }
        }


        /// <summary>
        /// Initializes control with personas. Specified personas will be displayed in the dropdown.
        /// </summary>
        /// <param name="personas">Personas to display</param>
        internal void SetPersonas(List<PersonaInfo> personas)
        {
            mPersonas = personas.ToDictionary(p => p.PersonaName, p => p);

            Actions.Add(new CMSButtonAction
            {
                Name = NO_PERSONA_ACTION_NAME,
                Text = CoreServices.Localization.GetString("personas.nopersona"),
            });

            foreach (var persona in personas)
            {
                Actions.Add(new CMSButtonAction
                {
                    Name = persona.PersonaName,
                    Text = persona.PersonaDisplayName,
                    ToolTip = persona.PersonaDescription,
                });
            }
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.AddCssClass("personas-selectable-button");
        }


        /// <summary>
        /// Adds required attributes to the button.
        /// </summary>
        /// <param name="firstAction">Action</param>
        /// <returns>Opening button</returns>
        protected override WebControl InitOpeningButtonControls(CMSButtonAction firstAction)
        {
            var openingButton = base.InitOpeningButtonControls(firstAction);

            SetControlImage(openingButton, firstAction.Name, "persona-main-button");

            return openingButton;
        }


        /// <summary>
        /// Adds required attributes to the hyperlink.
        /// </summary>
        /// <param name="action">Action</param>
        /// <returns>Action hyperlink</returns>
        protected override HyperLink CreateListItemHyperLink(CMSButtonAction action)
        {
            var hyperlink = base.CreateListItemHyperLink(action);

            SetControlImage(hyperlink, action.Name, "persona-dropdown-item");

            return hyperlink;
        }


        /// <summary>
        /// Adds required attributes to the button.
        /// </summary>
        /// <param name="onlyAction">Action</param>
        /// <returns>Opening button</returns>
        protected override CMSButton CreateButtonForOneAction(CMSButtonAction onlyAction)
        {
            var button = base.CreateButtonForOneAction(onlyAction);

            SetControlImage(button, onlyAction.Name, "persona-main-button");

            return button;
        }


        /// <summary>
        /// Tries to get url of the persona picture with the specified name (<paramref name="actionName"/>). 
        /// If image exists, sets specified css class to the control and adds picture as a background-image.
        /// </summary>
        /// <param name="control">Control to add attributes to</param>
        /// <param name="actionName">Persona code name</param>
        /// <param name="cssClass">Css class to add if picture exists</param>
        private void SetControlImage(WebControl control, string actionName, string cssClass)
        {
            string personaPictureUrl = GetPersonaPictureUrl(actionName);
            if (personaPictureUrl != null)
            {
                control.AddCssClass(cssClass);
                control.Style.Add("background-image", "url('" + personaPictureUrl + "')");
            }
        }


        /// <summary>
        /// Gets url of the picture of the persona specified by its name.
        /// </summary>
        /// <param name="personaName">Name of the persona</param>
        /// <returns>Url of th persona picture</returns>
        private string GetPersonaPictureUrl(string personaName)
        {
            if (mPersonas == null)
            {
                return null;
            }

            if (personaName == NO_PERSONA_ACTION_NAME)
            {
                return mPersonaPictureUrlCreator.CreateDefaultPersonaPictureUrl(PICTURE_SIDE_SIZE);
            }

            PersonaInfo persona;
            if (mPersonas.TryGetValue(personaName, out persona))
            {
                return mPersonaPictureUrlCreator.CreatePersonaPictureUrl(persona, PICTURE_SIDE_SIZE);
            }

            return mPersonaPictureUrlCreator.CreateDefaultPersonaPictureUrl(PICTURE_SIDE_SIZE);
        }
    }
}