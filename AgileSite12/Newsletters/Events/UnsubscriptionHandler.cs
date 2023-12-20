using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.Newsletters
{
    /// <summary>
    /// Handler for the event when subscriber email is being unsubscribed.
    /// </summary>
    public class UnsubscriptionHandler : SimpleHandler<UnsubscriptionHandler, UnsubscriptionEventArgs>
    {
    }
}
