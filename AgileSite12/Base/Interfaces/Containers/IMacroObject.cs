
namespace CMS.Base
{
    /// <summary>
    /// Interface for the objects which are used in macro processing. Defines their default resolving behavior.
    /// </summary>
    public interface IMacroObject
    {
        /// <summary>
        /// Returns the default text representation in the macros (this is called when the expression is resolved to its final value and should be converted to string).
        /// </summary>
        string ToMacroString();

        /// <summary>
        /// Returns the object which represents current object in the macro engine. 
        /// Whenever the object implementing IMacroObject interface is used within macro engine this method is called its result is used instead.
        /// </summary>
        /// <remarks>Return this if object doesn't have special representation for macro engine.</remarks>
        object MacroRepresentation();
    }
}