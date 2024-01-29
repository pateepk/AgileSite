namespace CMS.Base
{
    /// <summary>
    /// Objects containing the related data reference.
    /// </summary>
    public interface IRelatedData
    {
        /// <summary>
        /// Custom data connected to the object.
        /// </summary>
        object RelatedData
        {
            get;
            set;
        }
    }
}