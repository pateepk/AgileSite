using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.Core;
using CMS.Search;
using CMS.Search.Lucene3;

using Lucene.Net.Documents;

[assembly: RegisterImplementation(typeof(ILuceneSearchDocument), typeof(LuceneSearchDocument), Priority = CMS.Core.RegistrationPriority.SystemDefault, Lifestyle = Lifestyle.Transient)]

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Represents Lucene search document
    /// </summary>
    /// <remarks>This class is strongly tied with the current Lucene.Net library (version 3.0.3). We do not recommend using the class except for special cases.</remarks>
    public class LuceneSearchDocument : ILuceneSearchDocument
    {
        #region "Properties"

        /// <summary>
        /// Underlying document
        /// </summary>
        public Document Document
        {
            get;
            protected set;
        }
        
        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public LuceneSearchDocument()
            : this(null)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="doc">Base document</param>
        public LuceneSearchDocument(Document doc)
        {
            Document = doc ?? new Document();
        }


        /// <summary>
        /// Add field do the Lucene document.
        /// </summary>
        /// <param name="name">Name of new field</param>
        /// <param name="value">Value of field</param>
        /// <param name="store">Should be value stored</param>
        /// <param name="tokenize">Should be value tokenized</param>
        public void AddGeneralField(string name, object value, bool store, bool tokenize)
        {
            SearchHelper.AddGeneralField(this, name, value, store, tokenize);
        }


        /// <summary>
        /// Adds the given field to the document
        /// </summary>
        /// <param name="name">Field name</param>
        /// <param name="value">Field value</param>
        /// <param name="store">If true, the field value is stored</param>
        /// <param name="tokenize">If true, the field value is tokenized</param>
        public void Add(string name, string value, bool store = true, bool tokenize = false)
        {
            // Set field storing
            Field.Store storeSetting = (store ? Field.Store.YES : Field.Store.NO);

            // Set field tokenization
            Field.Index tokenSetting = (tokenize ? Field.Index.ANALYZED : Field.Index.NOT_ANALYZED);


            //Add default _type field
            Field field = new Field(name, value, storeSetting, tokenSetting);

            Document.Add(field);
        }


        /// <summary>
        /// Gets the value of specified field
        /// </summary>
        /// <param name="name">Field name</param>
        public string Get(string name)
        {
            return Document.Get(name);
        }


        /// <summary>
        /// Removes field with the given name
        /// </summary>
        /// <param name="name">Field name</param>
        public void RemoveField(string name)
        {
            Document.RemoveField(name);
        }

        #endregion
    }
}
