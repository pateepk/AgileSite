using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Converts list of fields and list of values to stream of <see cref="ContactImportData"/>.
    /// </summary>
    internal static class ImportDataConvertor
    {
        /// <summary>
        /// Converts list of fields and list of values to stream of <see cref="ContactImportData"/>. <paramref name="onErrorAction"/> is called for each failed conversion.
        /// </summary>
        public static IEnumerable<ContactImportData> ConvertToImportData(IList<string> fields, IList<List<string>> fieldsValues, Action<List<string>, Exception> onErrorAction = null)
        {
            if (fields == null)
            {
                throw new ArgumentNullException("fields");
            }

            if (fieldsValues == null)
            {
                throw new ArgumentNullException("fieldsValues");
            }

            foreach (var record in fieldsValues)
            {
                ContactImportData importData = null;
                try
                {
                    importData = new ContactImportData(fields, record);
                }
                catch (Exception e)
                {
                    if (onErrorAction != null)
                    {
                        onErrorAction(record, e);    
                    }
                }

                if (importData != null)
                {
                    yield return importData;
                }
            }
        }
    }
}