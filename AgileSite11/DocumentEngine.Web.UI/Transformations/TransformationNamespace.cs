using CMS.MacroEngine;
using CMS.Base;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Wrapper class to provide Transformation namespace in the MacroEngine.
    /// </summary>
    [Extension(typeof(TransformationMethods))]
    public class TransformationNamespace : MacroNamespace<TransformationNamespace>
    {
    }
}