using CMS;
using CMS.Personas;

[assembly: RegisterImplementation(typeof(IPersonaPictureUrlCreator), typeof(PersonaPictureUrlCreator), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Personas
{
    /// <summary>
    /// Creates urls of the persona images.
    /// </summary>
    public interface IPersonaPictureUrlCreator
    {
        /// <summary>
        /// Gets URL of the the image which should be used when persona has no picture selected or when picture of contact belonging to no persona should be displayed.
        /// </summary>
        /// <param name="imageSideSizeInPx">Maximum size of one side of the image. Persona pictures are usually squares</param>
        /// <returns>Url of the default persona picture</returns>
        string CreateDefaultPersonaPictureUrl(int imageSideSizeInPx);


        /// <summary>
        /// Gets url of the specified persona's picture.
        /// </summary>
        /// <param name="persona">Url to the image of this persona will be created</param>
        /// <param name="imageSideSizeInPx">Maximum size of pictures side</param>
        /// <returns>Url leading to the persona's image or to the default image if persona does not have picture set</returns>
        string CreatePersonaPictureUrl(PersonaInfo persona, int imageSideSizeInPx);
    }
}