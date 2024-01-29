using System;
using System.Linq;
using System.Text;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Tries to fix common error in email.
    /// </summary>
    internal static class ImportDataEmailFixer
    {
        /// <summary>
        /// This method trims spaces in ContactEmail column.
        /// </summary>
        public static ContactImportData TrimSpaces(ContactImportData record)
        {
            record.Email = record.Email.Trim();
            return record;
        }
    }
}
