namespace CMS.Helpers
{
    /// <summary>
    /// ObjectDataSet interface
    /// </summary>
    public interface IReadOnlyFlag
    {
        /// <summary>
        /// If true, the dataset is read-only (not allowed to modify, must be cloned)
        /// </summary>
        bool IsReadOnly
        {
            get;
            set;
        }
    }
}
