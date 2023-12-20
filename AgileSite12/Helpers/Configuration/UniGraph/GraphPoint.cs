using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Helpers.UniGraphConfig
{
    /// <summary>
    /// Represents coordinates of a point in a two-dimensional space.
    /// </summary>
    [DataContract(Name = "GraphPoint", Namespace = "CMS.Helpers.UniGraphConfig")]
    public class GraphPoint
    {
        /// <summary>
        /// Gets or sets the x-coordinate.
        /// </summary>
        [DataMember]
        public int X
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the y-coordinate.
        /// </summary>
        [DataMember]
        public int Y
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="GraphPoint"/> class with coordinates set to zero.
        /// </summary>
        public GraphPoint()
        {

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="GraphPoint"/> class from given coordinates.
        /// </summary>
        /// <param name="x">Position on the x-axis.</param>
        /// <param name="y">Position on the y-axis.</param>
        public GraphPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
