using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Defines SQL query format used in <see cref="CMSQueryProvider{TObject}"/> for selected string methods.
    /// </summary>
    internal interface IStringMetadata : IMetadata
    {
        /// <summary>
        /// Returns true if the given string 
        /// </summary>
        /// <param name="prefix">Prefix to check</param>
        [SqlRepresentation("{0} LIKE {1} + '%'")]
        bool StartsWith(string prefix);


        /// <summary>
        /// Returns true if the given string ends with the given suffix
        /// </summary>
        /// <param name="suffix">Suffix to check</param>
        [SqlRepresentation("{0} LIKE '%' + {1}")]
        bool EndsWith(string suffix);


        /// <summary>
        /// Returns true if the given string contains the given substring
        /// </summary>
        /// <param name="substring">Substring to check</param>
        [SqlRepresentation("{0} LIKE '%' + {1} + '%'")]
        bool Contains(string substring);
    }
}