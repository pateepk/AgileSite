using CMS.Base;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// Handler for checking the current number of feature-related objects that are limited by license limitations.
    /// </summary>
    internal class ObjectCountCheckHandler : SimpleHandler<ObjectCountCheckHandler, ObjectCountCheckEventArgs>
    {
    }
}