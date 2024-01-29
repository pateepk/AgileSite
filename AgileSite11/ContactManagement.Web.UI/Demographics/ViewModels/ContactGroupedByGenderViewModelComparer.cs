using System.Collections.Generic;

using CMS.ContactManagement.Web.UI.Internal;
using CMS.Membership;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Orders given collection of <see cref="ContactsGroupedByAgeViewModel"/> int the  following manner: male, female, unknown
    /// </summary>
    internal class ContactGroupedByGenderViewModelComparer : IComparer<ContactsGroupedByGenderViewModel>
    {
        public int Compare(ContactsGroupedByGenderViewModel x, ContactsGroupedByGenderViewModel y)
        {
            switch (x.Gender)
            {
                case UserGenderEnum.Male:
                case UserGenderEnum.Unknown:
                    return GetIntegerOrder(x.Gender);
            }

            return -GetIntegerOrder(y.Gender);
        }


        private int GetIntegerOrder(UserGenderEnum gender)
        {
            switch (gender)
            {
                case UserGenderEnum.Male:
                    return -1;
                case UserGenderEnum.Unknown:
                    return 1;
            }

            return 0;
        }
    }
}