using CMS.Base;

namespace CMS.Modules
{
    /// <summary>
    /// Handler for event raised when NuSpec manifest for an installable module NuGet package is being built.
    /// </summary>
    /// <seealso cref="NuSpecBuilder"/>
    public class BuildNuSpecManifestHandler : AdvancedHandler<BuildNuSpecManifestHandler, BuildNuSpecManifestEventArgs>
    {
    }
}
