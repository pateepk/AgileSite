namespace CMS.Base
{
    /// <summary>
    /// Class event attributes
    /// </summary>
    public class ClassEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Class name
        /// </summary>
        public string ClassName
        {
            get;
            set;
        }


        /// <summary>
        /// Assembly name
        /// </summary>
        public string AssemblyName
        {
            get;
            set;
        }


        /// <summary>
        /// Returning the object of the given class
        /// </summary>
        public object Object
        {
            get;
            set;
        }
    }
}