using System.Collections.Generic;

using CMS.DataEngine;


namespace CMS.DataProtection
{
    /// <summary>
    /// Defines contract for erasing personal data of a data subject.
    /// </summary>
    /// <seealso cref="PersonalDataEraserRegister"/>
    public interface IPersonalDataEraser
    {
        /// <summary>
        /// Erases personal data of data subject's <paramref name="identities"/> filtered by <paramref name="configuration"/>.
        /// </summary>
        /// <param name="identities">Collection of identities representing a data subject.</param>
        /// <param name="configuration">Defines which personal data will be erased.</param>
        void Erase(IEnumerable<BaseInfo> identities, IDictionary<string, object> configuration);
    }
}
