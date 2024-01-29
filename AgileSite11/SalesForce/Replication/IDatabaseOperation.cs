using System.Collections.Generic;

using CMS.ContactManagement;
using CMS.OnlineMarketing;

namespace CMS.SalesForce
{
    
    internal interface IDatabaseOperation
    {

        void Execute(string entityId, Dictionary<int, ContactInfo> contacts);

    }

}