using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace CMS.Personas.Web.UI
{
    /// <summary>
    /// Control which renders table containing pictures of personas passed in constructor. Each table row contains 5 pictures.
    /// </summary>
    internal class PersonaPicturesInGrid : Control
    {
        private readonly List<PersonaInfo> mPersonas;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="personas">Personas whose pictures will be rendered</param>
        internal PersonaPicturesInGrid(List<PersonaInfo> personas)
        {
            if (personas == null)
            {
                throw new ArgumentNullException("personas");
            }

            mPersonas = personas;
        }


        /// <summary>
        /// Renders HTML table containing persona pictures or nothing if there is no persona.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives 
        /// the server control content. </param>
        protected override void Render(HtmlTextWriter writer)
        {
            // Render nothing if no persona is passed
            if (mPersonas.Count == 0)
            {
                return;
            }

            writer.AddAttribute("class", "persona-pictures-in-grid");
            writer.RenderBeginTag("table");

            var personaPictureTagBuilder = PersonasFactory.GetPersonaPictureImgTagGenerator();

            for (int i = 0; i < mPersonas.Count; i += 5)
            {
                // Take up to 5 personas which will be rendered to the current row
                var batch = mPersonas.Skip(i).Take(5);

                writer.RenderBeginTag("tr");
                foreach (var personaInfo in batch)
                {
                    writer.AddAttribute("title", personaInfo.PersonaDisplayName);
                    writer.RenderBeginTag("td");

                    string imgTag = personaPictureTagBuilder.GenerateImgTag(personaInfo, 19);
                    writer.Write(imgTag);

                    writer.RenderEndTag(); // closes td
                }
                writer.RenderEndTag(); // closes tr
            }
            writer.RenderEndTag(); // closes table
        }
    }
}