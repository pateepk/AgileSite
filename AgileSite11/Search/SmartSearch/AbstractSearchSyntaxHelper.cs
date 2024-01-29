using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Search
{
    /// <summary>
    /// Helper methods for search condition syntax
    /// </summary>
    [StaticContract(typeof(ISearchSyntaxHelper), IsLocal = true)]
    public abstract class AbstractSearchSyntaxHelper : ISearchSyntaxHelper
    {
        #region "Variables"

        /// <summary>
        /// Regular expression for field name.
        /// </summary>
        private Regex mFieldNameSyntaxRegex;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the regular expression for field name.
        /// </summary>
        protected Regex FieldNameSyntaxRegex
        {
            get
            {
                return mFieldNameSyntaxRegex ?? (mFieldNameSyntaxRegex = RegexHelper.GetRegex(@"(?<FieldName>\S+?[:])\S"));
            }
        }

        #endregion


        #region "Syntax specific methods"

        /// <summary>
        /// Returns true if the given search condition is empty
        /// </summary>
        /// <param name="condition">Search condition to check</param>
        public virtual bool IsEmptyCondition(string condition)
        {
            return string.IsNullOrEmpty(condition) || (condition.Trim() == ":");
        }
        

        /// <summary>
        /// Adds the given search condition to the existing condition
        /// </summary>
        /// <param name="original">Original condition</param>
        /// <param name="add">Condition to add</param>
        public virtual string AddSearchCondition(string original, string add)
        {
            return String.Format("{0} {1}", original, add).Trim();
        }


        /// <summary>
        /// Gets the range expression
        /// </summary>
        /// <param name="from">From value</param>
        /// <param name="to">To value. If not specified, the range covers only the from value.</param>
        public virtual string GetRange(object from, object to = null)
        {
            // Start of the range must be something
            if (from == null)
            {
                return null;
            }

            // Range with single value
            if (to == null)
            {
                to = from;
            }

            var range = String.Format("[{0} TO {1}]", from, to);
            return range;
        }


        /// <summary>
        /// Gets the search condition for the given field
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="val">Field value</param>
        /// <param name="valueMatch">Defines if the condition is required. True means required, false means all except, null means default operator (typically optional)</param>
        public virtual string GetFieldCondition(string fieldName, object val, bool? valueMatch = true)
        {
            string value = SearchValueConverter.ConvertToString(val);

            if (String.IsNullOrEmpty(fieldName) || String.IsNullOrEmpty(value))
            {
                return null;
            }

            var condition = String.Format("{0}:{1}", fieldName, value);
            if (valueMatch == null)
            {
                return condition;
            }
            else if (valueMatch.Value)
            {
                return GetRequiredCondition(condition);
            }
            else
            {
                return GetNotCondition(condition);
            }
        }


        /// <summary>
        /// Gets the exact phrase condition from the given phrase
        /// </summary>
        /// <param name="phrase">Phrase to convert</param>
        public virtual string GetExactPhraseCondition(string phrase)
        {
            if (String.IsNullOrEmpty(phrase))
            {
                return phrase;
            }


            return "\"" + phrase + "\"";
        }


        /// <summary>
        /// Gets a required condition from the given condition
        /// </summary>
        /// <param name="condition">Condition to convert to required</param>
        public virtual string GetRequiredCondition(string condition)
        {
            if (String.IsNullOrEmpty(condition))
            {
                return condition;
            }

            return "+" + condition;
        }


        /// <summary>
        /// Gets a not (except) condition from the given condition
        /// </summary>
        /// <param name="condition">Condition to convert to required</param>
        public virtual string GetNotCondition(string condition)
        {
            if (String.IsNullOrEmpty(condition))
            {
                return condition;
            }

            return "-" + condition;
        }


        /// <summary>
        /// Groups the expressions
        /// </summary>
        /// <param name="expressions">Inner group expressions</param>
        public virtual string GetGroup(params string[] expressions)
        {
            var result = String.Join(" ", expressions.Where(s => !String.IsNullOrEmpty(s)));

            return (!String.IsNullOrEmpty(result.Trim())) ? "(" + result + ")" : null;
        }


        /// <summary>
        /// Returns condition for search filter row
        /// </summary>
        /// <param name="row">Filter row</param>
        /// <param name="value">Filter row value</param>
        public virtual string GetFilterCondition(string row, string value)
        {
            if (String.IsNullOrEmpty(row))
            {
                return null;
            }

            if (String.IsNullOrEmpty(value))
            {
                return row;
            }

            if (!row.Contains("{0}"))
            {
                row += "{0}";
            }

            return String.Format(row, ":" + GetGroup(value));
        }

        
        /// <summary>
        /// Expands given search expression with synonyms. If the data base of synonyms for given language is not found, searchExpression is returned without any modifications.
        /// </summary>
        /// <param name="searchExpression">Search expression which should be expanded with synonyms</param>
        /// <param name="culture">Language code of the search expression (if null, en-us is used)</param>
        public virtual string ExpandWithSynonyms(string searchExpression, string culture)
        {
            // No expansion by default - not supported
            return searchExpression;
        }


        /// <summary>
        /// Adds ~ signs to each term to force fuzzy search.
        /// </summary>
        /// <param name="searchExpression">Search expression to transform</param>
        public virtual string TransformToFuzzySearch(string searchExpression)
        {
            // No transformation by default - Not supported
            return searchExpression;
        }


        /// <summary>
        /// Escapes the key words to be searched
        /// </summary>
        /// <param name="keywords">Keywords</param>
        public virtual string EscapeKeyWords(string keywords)
        {
            // No escape by default - Not supported
            return keywords;
        }


        /// <summary>
        /// Splits the given list of key words separated by space
        /// </summary>
        /// <param name="keyWords">Key words</param>
        protected virtual IEnumerable<string> SplitKeyWords(string keyWords)
        {
            string[] splittedWords = keyWords.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return splittedWords;
        }

        #endregion

        
        #region "Other methods"

        /// <summary>
        /// Gets the field condition for a range of values
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="from">From value</param>
        /// <param name="to">To value</param>
        /// <param name="valueMatch">Defines if the condition is required. True means required, false means all except, null means default operator (typically optional)</param>
        public string GetFieldCondition(string fieldName, object from, object to, bool? valueMatch = true)
        {
            var range = GetRange(from, to);

            return GetFieldCondition(fieldName, range, valueMatch);
        }
        
        
        /// <summary>
        /// Returns string with combined search index conditions.
        /// </summary>
        /// <param name="keyWords">Search keywords, separated by space</param>
        /// <param name="searchCondition">Search condition</param>
        public string CombineSearchCondition(string keyWords, SearchCondition searchCondition)
        {
            if (searchCondition.SearchMode != SearchModeEnum.AnyWord)
            {
                searchCondition.SearchOptions = SearchOptionsEnum.NoneSearch;
            }

            var result = GetKeyWordsCondition(keyWords, searchCondition);

            // Add class name condition
            if (searchCondition.DocumentCondition != null)
            {
                result = AddSearchCondition(result, GetClassNameCondition(searchCondition.DocumentCondition.ClassNames));
            }

            // Add culture condition
            result = AddSearchCondition(result, GetCultureCondition(searchCondition.DocumentCondition));

            // Gets the extra search conditions
            result = AddSearchCondition(result, GetExtraConditions(searchCondition.ExtraConditions));

            // Finalize the condition
            result = FinalizeSearchCondition(result);

            return result;
        }
        
        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns modified keywords string based on searchOptions.
        /// </summary>
        /// <param name="keywords">String with keyword</param>
        /// <param name="searchOptions">Search options specifies encoding</param>
        public virtual string ProcessSearchKeywords(string keywords, SearchOptionsEnum searchOptions)
        {
            if (String.IsNullOrEmpty(keywords))
            {
                return keywords;
            }

            // Switch escaping by search options
            switch (searchOptions)
            {
                // Full search options
                case SearchOptionsEnum.FullSearch:
                    // No escaping return unchanged
                    return keywords;

                // Basic search options
                case SearchOptionsEnum.BasicSearch:
                    return SearchHelper.BasicSearchReplacer.Replace(keywords, "\\$0");

                // None search  options                       
                case SearchOptionsEnum.NoneSearch:
                    return EscapeKeyWords(keywords);
            }

            return null;
        }


        /// <summary>
        /// Converts all field names to lower case in regex result. 
        /// </summary>
        /// <param name="match">Regular expression match</param>
        protected virtual string FieldNameEvaluator(Match match)
        {
            string fieldName = match.Groups["FieldName"].Value;
            return match.Value.Replace(fieldName, fieldName.ToLowerCSafe());
        }


        /// <summary>
        /// Finalizes the search query to make sure that its syntax is consistent and valid
        /// </summary>
        /// <param name="condition">Condition to finalize</param>
        protected virtual string FinalizeSearchCondition(string condition)
        {
            // Convert all field names to lowercase
            condition = FieldNameSyntaxRegex.Replace(condition, FieldNameEvaluator);
            
            return condition;
        }


        /// <summary>
        /// Gets the extra conditions based on the given expression
        /// </summary>
        /// <param name="extraConditions">Extra conditions</param>
        protected virtual string GetExtraConditions(string extraConditions)
        {
            // Process extra condition
            extraConditions = SearchValueConverter.ReplaceNumbers(extraConditions);

            if (SearchHelper.RemoveDiacriticsForIndexField)
            {
                extraConditions = TextHelper.RemoveDiacritics(extraConditions);
            }

            return extraConditions;
        }


        /// <summary>
        /// Gets the search condition for the given key words
        /// </summary>
        /// <param name="keyWords">Key words</param>
        /// <param name="searchCondition">Search condition</param>
        protected virtual string GetKeyWordsCondition(string keyWords, SearchCondition searchCondition)
        {
            if (String.IsNullOrEmpty(keyWords))
            {
                return keyWords;
            }

            // Process key words
            keyWords = SearchValueConverter.ReplaceNumbers(keyWords);

            if (keyWords != null)
            {
                if (!String.IsNullOrEmpty(keyWords.Trim()))
                {
                    keyWords = ProcessSearchKeywords(keyWords, searchCondition.SearchOptions);

                    switch (searchCondition.SearchMode)
                    {
                        // All words
                        case SearchModeEnum.AllWords:
                            // Split keywords by spaces
                            {
                                var words = SplitKeyWords(keyWords);
                                keyWords = null;

                                foreach (var word in words)
                                {
                                    keyWords = AddSearchCondition(keyWords, GetRequiredCondition(word));
                                }
                            }
                            break;

                        // Exact phrase
                        case SearchModeEnum.ExactPhrase:
                            keyWords = GetExactPhraseCondition(keyWords);
                            break;

                        // Exact phrase
                        case SearchModeEnum.AnyWordOrSynonyms:
                            {
                                // Ensure culture
                                string culture = null;
                                if (searchCondition.DocumentCondition != null)
                                {
                                    culture = searchCondition.DocumentCondition.Culture;
                                }

                                keyWords = ExpandWithSynonyms(keyWords, culture);
                            }
                            break;

                        // Any word
                        case SearchModeEnum.AnyWord:
                            if (searchCondition.FuzzySearch)
                            {
                                keyWords = TransformToFuzzySearch(keyWords);
                            }
                            break;
                    }

                    keyWords = GetRequiredCondition(GetGroup(keyWords));
                }
            }

            return keyWords;
        }


        /// <summary>
        /// Gets the search condition for the given culture
        /// </summary>
        /// <param name="documentCondition">Document condition</param>
        protected virtual string GetCultureCondition(DocumentSearchCondition documentCondition)
        {
            string cultureCondition = null;

            if (documentCondition != null)
            {
                var defaultCulture = documentCondition.DefaultCulture;
                var culture = documentCondition.Culture;

                culture = ValidationHelper.GetString(culture, String.Empty).ToLowerCSafe();
                defaultCulture = ValidationHelper.GetString(defaultCulture, String.Empty).ToLowerCSafe();

                // Add culture conditions
                if (!String.IsNullOrEmpty(culture) && (culture != "##all##"))
                {
                    string values;

                    if (documentCondition.CombineWithDefaultCulture && (culture != defaultCulture))
                    {
                        values = GetGroup(GetRange(culture), GetRange(defaultCulture), GetRange(SearchHelper.INVARIANT_FIELD_VALUE));
                    }
                    else
                    {
                        values = GetGroup(GetRange(culture), GetRange(SearchHelper.INVARIANT_FIELD_VALUE));
                    }

                    cultureCondition = GetFieldCondition(SearchFieldsConstants.CULTURE, values);
                }
            }

            return cultureCondition;
        }


        /// <summary>
        /// Gets the search condition for a given list of class names
        /// </summary>
        /// <param name="classNames">List of class names separated by semicolon</param>
        protected virtual string GetClassNameCondition(string classNames)
        {
            // Add optional classname condition
            string classCondition = null;

            if (!String.IsNullOrEmpty(classNames))
            {
                string[] classes = classNames.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                // Must exists at least one class
                if (classes.Length > 0)
                {
                    // More classnames specified
                    if (classes.Length > 1)
                    {
                        string classesList = String.Empty;

                        // Create list of classnames
                        foreach (string oneclass in classes)
                        {
                            // Check whether current class is specified
                            if (!String.IsNullOrEmpty(oneclass.Trim()))
                            {
                                var className = oneclass.Trim().ToLowerCSafe();
                                classesList += " " + GetRange(className);
                            }
                        }

                        classCondition = GetFieldCondition("classname", GetGroup(classesList));
                    }
                    // Only one classname
                    else
                    {
                        // Check whether class is specified
                        if (!String.IsNullOrEmpty(classes[0].Trim()))
                        {
                            classCondition = GetFieldCondition("classname", GetRange(classes[0].Trim().ToLowerCSafe()));
                        }
                    }
                }
            }

            return classCondition;
        }

        #endregion
    }
}
