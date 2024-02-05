using System.ComponentModel;

using CMS.Base;

namespace CMS.DataEngine.Internal
{
    /// <summary>
    /// Specifies data object that can be serialized to <see cref="IDataContainer"/> and used e.g. in bulk insert.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IDataTransferObject
    {
        /// <summary>
        /// Fills given <paramref name="dataContainer"/> with values from current object.
        /// </summary>
        /// <param name="dataContainer">Datarow to be filled</param>
        void FillDataContainer(IDataContainer dataContainer);
    }
}