using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.DataProtection
{
    /// <summary>
    /// Defines contract for collecting identities of a data subject.
    /// </summary>
    public interface IIdentityCollector
    {
        /// <summary>
        /// Collects all the identities satisfying given <paramref name="dataSubjectIdentifiersFilter"/> and appends them to the list of <paramref name="identities"/>.
        /// </summary>
        /// <remarks>
        /// Contents of the <paramref name="identities"/> list may be modified in any way.
        /// Duplicate entries do not need to be appended to the <paramref name="identities"/>list. 
        /// </remarks>
        /// <param name="dataSubjectIdentifiersFilter">Key value collection containing data subject's information that identifies it.</param>
        /// <param name="identities">List of already collected identities.</param>
        void Collect(IDictionary<string, object> dataSubjectIdentifiersFilter, List<BaseInfo> identities); 
    }
}
