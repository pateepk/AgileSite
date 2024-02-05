using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Info object interface.
    /// </summary>
    public class GeneralizedInfo : BaseInfo.GeneralizedInfoWrapper
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainObj">Main object</param>
        /// <param name="dummy">Dummy object to separate the protected constructor</param>
        protected GeneralizedInfo(BaseInfo mainObj, object dummy)
            : base(mainObj)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainObj">Main object</param>
        internal GeneralizedInfo(BaseInfo mainObj)
            : base(mainObj)
        {
        }        
    }


    /// <summary>
    /// Event which should load the data to the object when fired.
    /// </summary>
    /// <param name="infoObj">Object to load</param>
    public delegate void LoadDataEventHandler(BaseInfo infoObj);
}