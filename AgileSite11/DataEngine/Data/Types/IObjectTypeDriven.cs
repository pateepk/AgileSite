namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for object type driven controls
    /// </summary>
    public interface IObjectTypeDriven
    {
        /// <summary>
        /// Type of the selected objects.
        /// </summary>
        string ObjectType
        {
            get;
        }
    }
}
