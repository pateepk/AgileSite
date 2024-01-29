using System.Collections;
using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Wrapper to make the given object purely enumerable
    /// </summary>
    public class EnumerableWrapper<TObject> : IEnumerable<TObject>
    {
        #region "Properties"

        /// <summary>
        /// Source data
        /// </summary>
        public IEnumerable Source 
        { 
            get; 
            protected set; 
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Source data</param>
        public EnumerableWrapper(IEnumerable source)
        {
            Source = source;
        }

        #endregion


        #region "IEnumerable members"

        /// <summary>
        /// Gets the enumerator
        /// </summary>
        IEnumerator<TObject> IEnumerable<TObject>.GetEnumerator()
        {
            foreach (var obj in Source)
            {
                yield return (TObject)obj;
            }
        }


        /// <summary>
        /// Gets the enumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var obj in Source)
            {
                yield return obj;
            }
        }

        #endregion
    }
}
