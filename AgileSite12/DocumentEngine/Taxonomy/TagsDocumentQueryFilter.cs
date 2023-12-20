using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Represents the state of external filter data and provides methods to apply filter on <see cref="IDocumentQuery" /> objects.
    /// </summary>
    internal class TagsDocumentQueryFilter : IDocumentQueryFilter
    {
        private readonly string mTagName;
        private readonly string mTagGroupName;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tagName">Name of tag which filtered pages have to be assigned to.</param>
        /// <param name="tagGroupName">Restricts the scope of a tag provided by <see cref="mTagName"/> argument.</param>
        public TagsDocumentQueryFilter(string tagName, string tagGroupName)
        {
            mTagName = tagName;
            mTagGroupName = tagGroupName;
        }


        /// <summary>
        /// Returns <see cref="IWhereCondition"/> object based on current Tags filter inner state.
        /// </summary>
        /// <param name="properties">Query properties representing the query inner state.</param>
        public IWhereCondition GetWhereCondition(DocumentQueryProperties properties)
        {
            return DocumentTagInfoProvider.GetDocumentTagWhereCondition(mTagName, mTagGroupName);
        }
    }
}
