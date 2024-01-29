using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CMS.DataEngine
{
    /// <summary>
    /// Contains helper methods for index fields.
    /// </summary>
    public class SearchFieldsHelper
    {
        private static SearchFieldsHelper instance;


        /// <summary>
        /// An event raised upon <see cref="IsContentField"/> execution. Based on the <see cref="IsContentFieldEventArgs.Result"/> the field is added to content fields.
        /// By default, all fields having flag named <see cref="SearchSettings.CONTENT"/> set to true are considered content fields.
        /// </summary>
        public IsContentFieldHandler IncludeContentField = new IsContentFieldHandler { Name = nameof(SearchFieldsHelper) + "." + nameof(IncludeContentField) };


        /// <summary>
        /// An event raised upon <see cref="IsIndexField"/> execution. Based on the <see cref="IsIndexFieldEventArgs.Result"/> the field is added to index fields.
        /// By default, all fields having flag named <see cref="SearchSettings.SEARCHABLE"/> set to true are added.
        /// </summary>
        public IsIndexFieldHandler IncludeIndexField = new IsIndexFieldHandler { Name = nameof(SearchFieldsHelper) + "." + nameof(IncludeIndexField) };


        /// <summary>
        /// Gets the <see cref="SearchFieldsHelper"/> instance.
        /// </summary>
        public static SearchFieldsHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    Interlocked.CompareExchange(ref instance, new SearchFieldsHelper(), null);
                }
                return instance;
            }
        }
        

        /// <summary>
        /// Initializes a new <see cref="SearchFieldsHelper"/>.
        /// </summary>
        private SearchFieldsHelper()
        {
        }


        /// <summary>
        /// Returns true if field represented by <paramref name="searchSettings"/> is to be included in given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index for which to detect whether field is an index field. Pass null to detect if field is an index field in any index type.</param>
        /// <param name="searchSettings">Search setting representing tested field.</param>
        /// <returns>True if field is an index field, false otherwise.</returns>
        public bool IsIndexField(ISearchIndexInfo index, SearchSettingsInfo searchSettings)
        {
            var eventArgs = new IsIndexFieldEventArgs
            {
                Result = false,
                Index = index,
                SearchSettings = searchSettings
            };

            using (IncludeIndexField.StartEvent(eventArgs))
            {
                return eventArgs.Result;
            }
        }


        /// <summary>
        /// Returns true if value of field represented by <paramref name="searchSettings"/> is to be included in a designated content column in given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index for which to detect whether field is a content field. Pass null to detect if field is an index field in any index type.</param>
        /// <param name="searchSettings">Search setting representing tested field.</param>
        /// <returns>True if field is an index field, false otherwise.</returns>
        public bool IsContentField(ISearchIndexInfo index, SearchSettingsInfo searchSettings)
        {
            var eventArgs = new IsContentFieldEventArgs
            {
                Result = false,
                Index = index,
                SearchSettings = searchSettings
            };

            using (IncludeContentField.StartEvent(eventArgs))
            {
                return eventArgs.Result;
            }
        }
    }
}
