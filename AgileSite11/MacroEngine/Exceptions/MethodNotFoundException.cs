namespace CMS.MacroEngine
{
    /// <summary>
    /// Base for the exceptions thrown when method which does not exist was tried to be executed.
    /// </summary>
    public class MethodNotFoundException : MacroException
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="originalExpression">The whole expression which was being parsed when this error occured.</param>
        public MethodNotFoundException(string originalExpression) :
            base(originalExpression)
        {
        }

        #endregion
    }
}
