using CMS.DataEngine;

namespace CMS.Tests
{
    /// <summary>
    /// Interface for automated tests fake of the info
    /// </summary>
    public interface IInfoFake<TInfo>
        where TInfo : BaseInfo, IInfo, new()
    {
        /// <summary>
        /// Faked class structure info
        /// </summary>
        ClassStructureInfo ClassStructureInfo
        {
            get;
        }
    }
}
