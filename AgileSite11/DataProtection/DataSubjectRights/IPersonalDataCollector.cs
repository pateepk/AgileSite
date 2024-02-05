using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.DataProtection
{
    /// <summary>
    /// Defines contract for collecting personal data of a data subject.
    /// </summary>
    /// <seealso cref="PersonalDataCollectorRegister"/>
    public interface IPersonalDataCollector
    {
        /// <summary>
        /// Collects personal data based on given <paramref name="identities"/>.
        /// </summary>
        /// <param name="identities">Collection of identities representing a data subject.</param>
        /// <param name="outputFormat">Defines an output format for the result.</param>
        /// <returns><see cref="PersonalDataCollectorResult"/> containing personal data.</returns>
        PersonalDataCollectorResult Collect(IEnumerable<BaseInfo> identities, string outputFormat);
    }
}