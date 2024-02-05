using System.Data;

namespace CMS.DataEngine
{
    /// <summary>
    /// Empty info object collection
    /// </summary>
    public class EmptyCollection<InfoType> : InfoObjectCollection<InfoType>
        where InfoType : BaseInfo, new()
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EmptyCollection()
            : base(new DataSet())
        {
        }
    }
}
