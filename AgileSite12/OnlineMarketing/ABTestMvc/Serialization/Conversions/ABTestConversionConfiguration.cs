using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Represents conversion configuration for an A/B test which contains list of <see cref="ABTestConversion"/> instances.
    /// </summary>
    [Serializable]
    public class ABTestConversionConfiguration
    {
        private const string CONVERSIONS_NODE = "conversions";


        /// <summary>
        /// A/B Test to which this Conversion Configuration belongs
        /// </summary>
        private readonly ABTestInfo mABTest;


        /// <summary>
        /// List of conversion settings for an A/B test.
        /// </summary>
        private readonly List<ABTestConversion> mABTestConversions;


        /// <summary>
        /// Iterator for conversions in current configuration.
        /// </summary>
        public IEnumerable<ABTestConversion> ABTestConversions
        {
            get
            {
                if (mABTestConversions == null || !mABTestConversions.Any())
                {
                    yield break;
                }

                foreach (var conversion in mABTestConversions)
                {
                    yield return conversion;
                }
            }
        }


        /// <summary>
        /// Gets conversions count in current configuration.
        /// </summary>
        public int ConversionsCount
        {
            get
            {
                return mABTestConversions.Count;
            }
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ABTestConversionConfiguration"/> class.
        /// </summary>
        /// <param name="abTest">Parent A/B Test object.
        /// If null value is provided, then this object has to be manually serialized into <see cref="ABTestInfo"/> backing DB field.</param>
        /// <param name="conversionConfig">Serialized <see cref="ABTestConversionConfiguration"/> object.</param>
        public ABTestConversionConfiguration(ABTestInfo abTest, string conversionConfig)
        {
            mABTest = abTest;
            mABTestConversions = new List<ABTestConversion>();
            
            if (String.IsNullOrWhiteSpace(conversionConfig))
            {
                return;
            }

            var xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.LoadXml(conversionConfig);
            }
            catch(XmlException ex)
            {
                // Re-throw exception
                throw new ArgumentException("Parameter is not in XML format.", nameof(conversionConfig), ex);
            }

            mABTestConversions.AddRange(xmlDocument.SelectSingleNode(CONVERSIONS_NODE).ChildNodes.Cast<XmlNode>().Select(node => new ABTestConversion(node)));
        }


        /// <summary>
        /// Serializes current configuration into parent A/B Test object database field.
        /// </summary>
        private void SetABTestInfoABTestConversions()
        {
            if (mABTest != null)
            {
                mABTest.SetValue("ABTestConversions", Serialize());
            }
        }


        /// <summary>
        /// Returns conversion configuration serialized into XML format string.
        /// </summary>
        public string Serialize()
        {
            if (ConversionsCount == 0)
            {
                return null;
            }

            var xmlDocument = new XmlDocument();
            var root = xmlDocument.CreateElement(CONVERSIONS_NODE);
            xmlDocument.AppendChild(root);

            mABTestConversions.ForEach(conversion => root.AppendChild(conversion.GetXMLNode(xmlDocument)));

            return xmlDocument.InnerXml;
        }


        /// <summary>
        /// Adds new conversion to current configuration.
        /// </summary>
        /// <param name="conversion">Conversion to be added.</param>
        public void AddConversion(ABTestConversion conversion)
        {
            mABTestConversions.Add(conversion);

            SetABTestInfoABTestConversions();
        }


        /// <summary>
        /// Gets conversion from current configuration.
        /// </summary>
        /// <param name="index">The zero-based index of the conversion to retrieve.</param>
        public ABTestConversion GetConversionAt(int index)
        {
            return mABTestConversions[index];
        }


        /// <summary>
        /// Updates conversion in current configuration.
        /// </summary>
        /// <param name="index">The zero-based index of the conversion to update.</param>
        /// <param name="conversion">Conversion to be set at <paramref name="index"/>.</param>
        public void SetConversionAt(int index, ABTestConversion conversion)
        {
            mABTestConversions[index] = conversion;

            SetABTestInfoABTestConversions();
        }


        /// <summary>
        /// Removes conversion from current configuration.
        /// </summary>
        /// <param name="index">The zero-based index of the conversion to remove.</param>
        public void RemoveConversionAt(int index)
        {
            mABTestConversions.RemoveAt(index);

            SetABTestInfoABTestConversions();
        }


        /// <summary>
        /// Returns true, if conversion specified by <paramref name="conversionName"/> is defined in the configuration and provides this conversion through the output parameter <paramref name="conversion"/> if exists.
        /// </summary>
        /// <param name="conversionName">Conversion name.</param>
        /// <param name="conversion">Output parameter for conversion.</param>
        public bool TryGetConversion(string conversionName, out ABTestConversion conversion)
        {
            conversion = mABTestConversions.FirstOrDefault(c => String.Equals(conversionName, c.ConversionName, StringComparison.OrdinalIgnoreCase));
            
            return (conversion != null);
        }


        /// <summary>
        /// Returns true, if conversion specified by <paramref name="conversionOriginalName"/> is defined in the configuration and provides this conversion through the output parameter <paramref name="conversion"/> if exists.
        /// </summary>
        /// <param name="conversionOriginalName">Original name of the conversion (<see cref="ABTestConversion.ConversionOriginalName"/>) to be retrieved..</param>
        /// <param name="conversion">Output parameter for conversion.</param>
        public bool TryGetConversionByOriginalName(string conversionOriginalName, out ABTestConversion conversion)
        {
            conversion = mABTestConversions.FirstOrDefault(c => String.Equals(conversionOriginalName, c.ConversionOriginalName, StringComparison.OrdinalIgnoreCase));

            return (conversion != null);
        }
    }
}
