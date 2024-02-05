using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides enumerator for pages to be stored within continuous integration.
    /// </summary>
    internal class GetDocumentsEnumerator : IEnumerable<BaseInfo>
    {
        #region "Properties"

        /// <summary>
        /// Object type to be enumerated
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition for the objects
        /// </summary>
        public IWhereCondition WhereCondition
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="where">Where condition for the objects</param>
        public GetDocumentsEnumerator(string objectType, IWhereCondition where = null)
        {
            ObjectType = objectType;
            WhereCondition = where;
        }


        /// <summary>
        /// Gets the enumerator to enumerate all pages
        /// </summary>
        public IEnumerator<BaseInfo> GetEnumerator()
        {
            // Get all pages per type
            var tree = new TreeProvider();
            var classNames = DocumentTypeHelper.GetClassNames(WhereCondition);

            foreach (var className in classNames)
            {
                // Get published data
                var query = tree.SelectNodes(className)
                                .All()
                                .Where(WhereCondition);

                foreach (var doc in query)
                {
                    yield return doc;
                }
            }
        }


        /// <summary>
        /// Gets the enumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}