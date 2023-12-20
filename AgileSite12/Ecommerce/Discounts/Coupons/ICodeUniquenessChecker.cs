namespace CMS.Ecommerce
{
    /// <summary>
    /// Ensures discount code uniqueness in entire application.
    /// </summary>
    public interface ICodeUniquenessChecker
    {
        /// <summary>
        /// Returns true if given code is unique, false otherwise.
        /// </summary>
        bool IsUnique(string code);
    }
}