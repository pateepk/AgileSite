﻿using System.Collections.Generic;

using CMS.DataEngine;
using CMS.DataProtection;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Sample implementation of <see cref="IPersonalDataCollector"/> interface for collecting contact's personal data.
    /// </summary>
    internal class SampleContactDataCollector : IPersonalDataCollector
    {
        /// <summary>
        /// Collects personal data based on given <paramref name="identities"/>.
        /// </summary>
        /// <param name="identities">Collection of identities representing a data subject.</param>
        /// <param name="outputFormat">Defines an output format for the result.</param>
        /// <returns><see cref="PersonalDataCollectorResult"/> containing personal data.</returns>
        public PersonalDataCollectorResult Collect(IEnumerable<BaseInfo> identities, string outputFormat)
        {
            using (var writer = CreateWriter(outputFormat))
            {
                var dataCollector = new SampleContactDataCollectorCore(writer);
                return new PersonalDataCollectorResult
                {
                    Text = dataCollector.CollectData(identities)
                };
            }
        }


        private IPersonalDataWriter CreateWriter(string outputFormat)
        {
            switch (outputFormat.ToLowerInvariant())
            {
                case PersonalDataFormat.MACHINE_READABLE:
                    return new XmlPersonalDataWriter();

                case PersonalDataFormat.HUMAN_READABLE:
                default:
                    return new HumanReadablePersonalDataWriter();
            }
        }
    }
}
