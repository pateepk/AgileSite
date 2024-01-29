using System;

using CMS.ContactManagement;
using CMS.DataEngine;

namespace CMS.SalesForce
{
    internal class SalesForceHandlers
    {
        /// <summary>
        /// Init handlers
        /// </summary>
        public static void Init()
        {
            SettingsKeyInfo.TYPEINFO.Events.Update.After += SettingsKeyUpdate_After;
            ContactInfo.TYPEINFO.Events.Update.Before += ContactUpdate_Before;
        }

        #region "Private methods"

        private static void SettingsKeyUpdate_After(object sender, ObjectEventArgs e)
        {
            SettingsKeyInfo setting = e.Object as SettingsKeyInfo;
            if ((setting != null) && (setting.KeyName == "CMSSalesForceLeadReplicationMapping"))
            {
                SettingsKeyInfoProvider.SetGlobalValue("CMSSalesForceLeadReplicationMappingDateTime", DateTime.Now.ToString("s"));
            }
        }


        private static void ContactUpdate_Before(object sender, ObjectEventArgs e)
        {
            ContactInfo contact = e.Object as ContactInfo;
            if (!DetectChange(contact, "ContactSalesForceLeadID", "ContactSalesForceLeadReplicationSuspensionDateTime", "ContactSalesForceLeadReplicationDisabled", "ContactSalesForceLeadReplicationDateTime"))
            {
                contact.SetValue("ContactSalesForceLeadReplicationSuspensionDateTime", null);
            }
        }


        private static bool DetectChange(ContactInfo contact, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                object current = contact.GetValue(columnName);
                object original = contact.GetOriginalValue(columnName);
                if (current != original)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
