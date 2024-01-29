namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for InfoProvider class which uses full name as info identifier. 
    /// </summary>
    public interface IFullNameInfoProvider
    {
        /// <summary>
        /// Creates new caching collection for objects addressed by full name
        /// </summary>
        ProviderInfoDictionary<string> GetFullNameDictionary();


        /// <summary>
        /// Gets the where condition that searches the object based on the given full name.
        /// </summary>
        /// <param name="fullName">Object full name</param>
        string GetFullNameWhereCondition(string fullName);
    }
}