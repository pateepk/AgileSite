using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Data query source which gets the data for specific object type
    /// </summary>
    public class ObjectSource<TObject> : ObjectSourceBase<ObjectSource<TObject>>
        where TObject : BaseInfo, new()
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectSource()
            : this(null)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectType">Object type</param>
        public ObjectSource(string objectType)
            : base(null)
        {
            if (!string.IsNullOrEmpty(objectType))
            {
                // Use specific object type
                ObjectType = objectType;
            }
            else
            {
                InitFromType<TObject>();
            }
        }
        
        #endregion
    }
}
