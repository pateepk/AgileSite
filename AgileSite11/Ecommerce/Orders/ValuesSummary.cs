using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using CMS.IO;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class representing values summary (discounts, taxes, other payments) in Invoice, Email template and Shopping cart.
    /// </summary>
    [Serializable]
    [XmlRoot("Summary")]
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class ValuesSummary : IEnumerable<SummaryItem>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly List<SummaryItem> mSummaryItems = new List<SummaryItem>();

        
        /// <summary>
        /// Count of the summary list.
        /// </summary>
        public int Count => mSummaryItems.Count;


        /// <summary>
        /// Creates a new instance de-serialized from the <paramref name="xmlSummary"/>.
        /// </summary>
        /// <param name="xmlSummary">XML summary definition</param>
        public ValuesSummary(string xmlSummary)
        {
            var summaryObj = GetSummaryFromXml(xmlSummary);

            if (summaryObj != null)
            {
                mSummaryItems = summaryObj.ToList();
            }
        }


        /// <summary>
        /// Creates a new instance of summary with the specified <paramref name="items"/>.
        /// </summary>
        /// <param name="items"><see cref="SummaryItem"/> items.</param>
        public ValuesSummary(IEnumerable<SummaryItem> items)
        {
            if (items != null)
            {
                mSummaryItems = items.ToList();
            }
        }


        /// <summary>
        /// Creates a new instance of summary.
        /// </summary>
        public ValuesSummary()
        {
            // Needed for XML deserialization
        }


        /// <summary>
        /// Removes all items from the summary.
        /// </summary>
        public void Clear()
        {
            mSummaryItems.Clear();
        }


        /// <summary>
        /// Returns an enumerator.
        /// </summary>
        public IEnumerator<SummaryItem> GetEnumerator()
        {
            return mSummaryItems.GetEnumerator();
        }


        /// <summary>
        /// Adds the new item to the summary.
        /// </summary>
        /// <param name="item"><see cref="SummaryItem"/> item.</param>
        public void Add(SummaryItem item)
        {
            // Method is needed for XML serialization.
            mSummaryItems.Add(item);
        }


        /// <summary>
        /// Sums the <paramref name="value"/> with the item specified by <paramref name="name"/>. Creates a new summary item when not found.
        /// </summary>
        /// <param name="name">Name of the summary item.</param>
        /// <param name="value">Value of the summary item.</param>
        public void Sum(string name, decimal value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var existing = GetExisting(name);
            if (existing == null)
            {
                Add(new SummaryItem(name, value));
            }
            else
            {
                existing.Value += value;
            }
        }


        /// <summary>
        /// Adds the set of <paramref name="items"/> to the summary.
        /// </summary>
        /// <param name="items">Items to be summed into the summary.</param>
        public void Merge(IEnumerable<SummaryItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            foreach (var item in items)
            {
                Sum(item.Name, item.Value);
            }
        }


        private SummaryItem GetExisting(string name)
        {
            return mSummaryItems.FirstOrDefault(i => i.Name.Equals(name, StringComparison.InvariantCulture));
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        internal string GetSummaryXml()
        {
            var xmlSerializer = new XmlSerializer(GetType());
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true
            };
            var emptyNamepsaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            using (var stream = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stream, settings))
            {
                xmlSerializer.Serialize(xmlWriter, this, emptyNamepsaces);
                return stream.ToString();
            }
        }


        internal ValuesSummary GetSummaryFromXml(string xmlSummary)
        {
            if (string.IsNullOrEmpty(xmlSummary))
            {
                return null;
            }

            try
            {
                var stringReader = new StringReader(xmlSummary);
                var serializer = new XmlSerializer(GetType());
                return serializer.Deserialize(stringReader) as ValuesSummary;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
