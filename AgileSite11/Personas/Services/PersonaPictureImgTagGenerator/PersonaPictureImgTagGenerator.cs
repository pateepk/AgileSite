using System;
using System.Linq;
using System.Text;

using CMS.Helpers;

namespace CMS.Personas
{
    /// <summary>
    /// Class capable of generating HTML img tags displaying persona picture.
    /// </summary>
    public sealed class PersonaPictureImgTagGenerator : IPersonaPictureImgTagGenerator
    {
        private readonly IPersonaPictureUrlCreator mPictureUrlCreator = PersonasFactory.GetPersonaPictureUrlCreator();


        /// <summary>
        /// Generates img tag which displays persona picture with a specified size.
        /// </summary>
        /// <param name="persona">This persona's picture will be displayed</param>
        /// <param name="imageSideSizeInPx">Size of the picture</param>
        /// <exception cref="ArgumentNullException"><paramref name="persona"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="imageSideSizeInPx"/> isn't greater than zero</exception>
        /// <returns>HTML which displays persona picture</returns>
        public string GenerateImgTag(PersonaInfo persona, int imageSideSizeInPx)
        {
            if (persona == null)
            {
                throw new ArgumentNullException("persona");
            }
            if (imageSideSizeInPx <= 0)
            {
                throw new ArgumentException("size of the persona picture must be greater than zero", "imageSideSizeInPx");
            }

            string imageUrl = mPictureUrlCreator.CreatePersonaPictureUrl(persona, imageSideSizeInPx);

            if (imageUrl == null)
            {
                return null;
            }

            string encodedPersonaDisplayName = HTMLHelper.HTMLEncode(persona.PersonaDisplayName);

            return string.Format("<img src=\"{0}\" alt=\"{1}\" style=\"width: {2}px; height: {2}px;\" />", imageUrl, encodedPersonaDisplayName, imageSideSizeInPx);
        }
    }
}