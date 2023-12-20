using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

using Version = Lucene.Net.Util.Version;

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Represent a search analyzer for the Lucene.Net search index
    /// </summary>
    internal class LuceneSearchAnalyzer : ISearchAnalyzer
    {
        private static readonly StringSafeDictionary<Analyzer> mIndexAnalyzers = new StringSafeDictionary<Analyzer>();


        /// <summary>
        /// Search analyzer
        /// </summary>
        public Analyzer Analyzer
        {
            get;
            protected set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sii">Search index info</param>
        /// <param name="isSearch">If true, the analyzer is meant for searching</param>
        public LuceneSearchAnalyzer(SearchIndexInfo sii, bool isSearch)
        {
            Analyzer = isSearch ? CreateSearchAnalyzer(sii) : CreateIndexingAnalyzer(sii);
        }


        /// <summary>
        /// Creates analyzer that will be used for searching over multiple indexes
        /// </summary>
        /// <param name="indexes">Index infos</param>
        public LuceneSearchAnalyzer(params SearchIndexInfo[] indexes)
        {
            Analyzer = CreateSearchAnalyzer(indexes);
        }


        /// <summary>
        /// Returns hash table key for given set of indexes
        /// </summary>
        /// <param name="indexes">Index infos</param>
        /// <returns></returns>
        protected string GetSearchAnalyzerKey(params SearchIndexInfo[] indexes)
        {
            return indexes.Aggregate("", (key, index) => key + index.IndexCodeName + ";").TrimEnd(';');
        }


        /// <summary>
        /// Returns analyzer to be used for searching over given indexes (not indexing)
        /// </summary>
        /// <param name="indexes">Search indexes</param>
        protected Analyzer CreateSearchAnalyzer(params SearchIndexInfo[] indexes)
        {
            var orderedIndexes = indexes.OrderBy(index => index.IndexCodeName).ToArray();
            var key = GetSearchAnalyzerKey(orderedIndexes);

            var analyzer = mIndexAnalyzers[key];
            if (analyzer == null)
            {
                analyzer = CreatePerFieldAnalyzer(true, orderedIndexes);
                mIndexAnalyzers[key] = analyzer;
            }

            return analyzer;
        }


        /// <summary>
        /// Returns analyzer to be used for indexing (not searching)
        /// </summary>
        /// <param name="index">Search index</param>
        protected Analyzer CreateIndexingAnalyzer(SearchIndexInfo index)
        {
            return CreatePerFieldAnalyzer(false, index);
        }


        /// <summary>
        /// Creates PerFieldAnalyzer for given indexes. This analyzer should be used for searching (not indexing)
        /// </summary>
        /// <param name="isSearch">If true, the analyzer is meant for searching (not indexing)</param>
        /// <param name="indexes">Index infos</param>
        protected virtual Analyzer CreatePerFieldAnalyzer(bool isSearch, params SearchIndexInfo[] indexes)
        {
            if (indexes.Length == 0)
            {
                throw new ArgumentException("[SearchAnalyzers.CreateSearchAnalyzer]:Indexes must contain at least one index.", "indexes");
            }

            var perFieldAnalyzer = new PerFieldAnalyzerWrapper(CreateAnalyzer(indexes[0].IndexAnalyzerType, indexes[0], isSearch));

            foreach (var index in indexes)
            {
                // Dictionary used to cache analyzers by it's type created for this index
                var analyzers = new SafeDictionary<SearchAnalyzerTypeEnum, Analyzer>();

                // When creating per field analyzer for only one index, we don't need to include default index analyzer
                var fieldAnalyzers = GetFieldAnalyzers(index, isSearch);

                foreach (var fieldName in fieldAnalyzers.TypedKeys)
                {
                    // Get field analyzers
                    var analyzerType = fieldAnalyzers[fieldName];
                    var fieldAnalyzer = analyzers[analyzerType] ?? (analyzers[analyzerType] = CreateAnalyzer(analyzerType, index, isSearch));

                    // Create the field analyzer
                    perFieldAnalyzer.AddAnalyzer(fieldName, fieldAnalyzer);
                    perFieldAnalyzer.AddAnalyzer(fieldName.ToLowerInvariant(), fieldAnalyzer);
                }
            }

            return perFieldAnalyzer;
        }


        /// <summary>
        /// Gets the dictionary of analyzers to be used by specific field. Dictionary [field name -> AnalyzerTypeEnum]
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="isSearch">If true, the analyzer is meant for searching (not indexing)</param>
        protected virtual SafeDictionary<string, SearchAnalyzerTypeEnum> GetFieldAnalyzers(SearchIndexInfo index, bool isSearch)
        {
            var fieldAnalyzers = new SafeDictionary<string, SearchAnalyzerTypeEnum>();

            var indexer = SearchIndexers.GetIndexer(index.IndexType);
            if (indexer != null)
            {
                // Set whitespace analyzer for non tokenized fields
                foreach (var field in indexer.GetSearchFields(index).Items)
                {
                    var defaultAnalyzer = index.IndexAnalyzerType;

                    // Apply white space analyzers to non-tokenized fields.
                    // White space analyzer will prevent application of the default index analyzer to non-tokenized fields when parsing search query.
                    if (!field.GetFlag(SearchSettings.TOKENIZED))
                    {
                        if (isSearch)
                        {
                            fieldAnalyzers.Add(field.FieldName, SearchAnalyzerTypeEnum.WhiteSpaceAnalyzer);
                        }

                        continue;
                    }

                    // Insert default analyzer only when searching. One search query can be used to search over more indexes with different default analyzers.
                    if (field.Analyzer == null)
                    {
                        if (isSearch)
                        {
                            fieldAnalyzers.Add(field.FieldName, defaultAnalyzer);
                        }

                        continue;
                    }

                    fieldAnalyzers.Add(field.FieldName, field.Analyzer.Value);
                }
            }

            return fieldAnalyzers;
        }


        /// <summary>
        /// Creates an analyzer of the given type
        /// </summary>
        /// <param name="analyzerType">Analyzer type</param>
        /// <param name="sii">Search index info</param>
        /// <param name="isSearch">If true, the analyzer is meant for searching</param>
        protected Analyzer CreateAnalyzer(SearchAnalyzerTypeEnum analyzerType, SearchIndexInfo sii, bool isSearch)
        {
            Analyzer analyzer;

            switch (analyzerType)
            {
                    // Stop analyzer
                case SearchAnalyzerTypeEnum.StopAnalyzer:
                    analyzer = GetStopWordAnalyzer(sii);
                    break;

                    // Simple analyzer
                case SearchAnalyzerTypeEnum.SimpleAnalyzer:
                    analyzer = new SimpleAnalyzer();
                    break;

                    // White space analyzer
                case SearchAnalyzerTypeEnum.WhiteSpaceAnalyzer:
                    analyzer = new WhitespaceAnalyzer();
                    break;

                    // Keyword analyzer
                case SearchAnalyzerTypeEnum.KeywordAnalyzer:
                    analyzer = new KeywordAnalyzer();
                    break;

                    // Custom analyzer
                case SearchAnalyzerTypeEnum.CustomAnalyzer:
                    analyzer = SearchHelper.LoadCustomAnalyzer<Analyzer>(sii.CustomAnalyzerAssemblyName, sii.CustomAnalyzerClassName);
                    break;

                    // Subset analyzer
                case SearchAnalyzerTypeEnum.SubsetAnalyzer:
                    analyzer = new SubSetAnalyzer(isSearch, false, 1);
                    break;

                    // StratsWith analyzer
                case SearchAnalyzerTypeEnum.StartsWithanalyzer:
                    analyzer = new SubSetAnalyzer(isSearch, true, 1);
                    break;

                    // Whitespace with stemming analyzer
                case SearchAnalyzerTypeEnum.WhitespaceWithStemmingAnalyzer:
                    analyzer = new StemmingAnalyzer(new WhitespaceAnalyzer());
                    break;

                    // Simple with stemming analyzer
                case SearchAnalyzerTypeEnum.SimpleWithStemmingAnalyzer:
                    analyzer = new StemmingAnalyzer(new SimpleAnalyzer());
                    break;

                    // Stop with stemming analyzer
                case SearchAnalyzerTypeEnum.StopWithStemmingAnalyzer:
                    analyzer = new StemmingAnalyzer(GetStopWordAnalyzer(sii));
                    break;

                    // Standard analyzer by default
                default:
                    analyzer = GetSandardAnalyzer(sii);
                    break;
            }

            return analyzer;
        }


        /// <summary>
        /// Returns correct stop word analyzer.
        /// </summary>
        /// <param name="sii">Search index info</param>
        private Analyzer GetStopWordAnalyzer(SearchIndexInfo sii)
        {
            var stopWords = SearchHelper.GetStopWordsFileInfo(sii.StopWordsFile);
            if (stopWords == null)
            {
                return new StopAnalyzer(Version.LUCENE_21);
            }
            else
            {
                using (var stopWordsReader = stopWords.OpenText())
                {
                    return new StopAnalyzer(Version.LUCENE_21, stopWordsReader);
                }
            }
        }


        /// <summary>
        /// Returns correct standard analyzer.
        /// </summary>
        /// <param name="sii">Search index info</param>
        private Analyzer GetSandardAnalyzer(SearchIndexInfo sii)
        {
            var stopWords = SearchHelper.GetStopWordsFileInfo(sii.StopWordsFile);
            if (stopWords == null)
            {
                return new StandardAnalyzer(Version.LUCENE_21);
            }
            else
            {
                return new StandardAnalyzer(Version.LUCENE_21, stopWords.SystemInfo);
            }
        }


        /// <summary>
        /// Initializes handlers for clearing analyzers table.
        /// </summary>
        internal static void Init()
        {
            SearchIndexInfo.TYPEINFO.Events.Update.After += (sender, args) => SyncClearAnalyzersTable();
            DataClassInfo.TYPEINFO.Events.Update.After += (sender, args) => SyncClearAnalyzersTable();
            SearchIndexInfo.TYPEINFO.Events.Delete.After += (sender, args) => SyncClearAnalyzersTable();
            DataClassInfo.TYPEINFO.Events.Delete.After += (sender, args) => SyncClearAnalyzersTable();

            WebFarmHelper.RegisterTask<ClearSearchAnalyzersWebFarmTask>(true);
        }


        /// <summary>
        /// Clears analyzers table.
        /// </summary>
        internal static void ClearAnalyzersTable()
        {
            mIndexAnalyzers.Clear();
        }


        /// <summary>
        /// Clears analyzers table.
        /// </summary>
        private static void SyncClearAnalyzersTable()
        {
            ClearAnalyzersTable();
            WebFarmHelper.CreateTask(new ClearSearchAnalyzersWebFarmTask());
        }
    }
}
