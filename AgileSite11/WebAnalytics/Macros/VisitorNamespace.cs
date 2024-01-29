using System;

using CMS.MacroEngine;
using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Wrapper class to provide Visitor namespace in the MacroEngine.
    /// </summary>
    [Extension(typeof(VisitorMethods))]
    public class VisitorNamespace : MacroNamespace<VisitorNamespace>
    {
    }
}