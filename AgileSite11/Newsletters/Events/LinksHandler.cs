using System;
using System.Linq;
using System.Text;

using CMS.Base;


namespace CMS.Newsletters
{
    /// <summary>
    /// Handler raised when system link in email campaign is clicked and tracked, e.g. for logging email opening and email link tracking.
    /// </summary>
    public class LinksHandler : SimpleHandler<LinksHandler, LinksEventArgs>
    {
    }
}
