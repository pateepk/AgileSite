using System;
using System.Web.UI;

using CMS.Helpers;
using CMS.IO;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Definition of the control. Pass in either string as control path or a Type as control type.
    /// </summary>
    public class ControlDefinition
    {
        #region "Properties"

        /// <summary>
        /// Control path
        /// </summary>
        public string Path
        {
            get;
            protected set;
        }


        /// <summary>
        /// Control type
        /// </summary>
        public Type Type
        {
            get;
            protected set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        protected ControlDefinition()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Control path</param>
        public ControlDefinition(string path)
        {
            Path = path;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Control type</param>
        public ControlDefinition(Type type)
        {
            Type = type;
        }


        /// <summary>
        /// Loads the given control
        /// </summary>
        /// <param name="page">Parent page</param>
        public Control Load(Page page)
        {
            if (Path != null)
            {
                VirtualPathLog.Log(Path);

                return page.LoadControl(Path);
            }
            else
            {
                return (Control)Activator.CreateInstance(Type);
            }
        }

        #endregion


        #region "Operators"

        /// <summary>
        /// Implicit conversion from string to control definition
        /// </summary>
        /// <param name="path">Control path</param>
        public static implicit operator ControlDefinition(string path)
        {
            return new ControlDefinition(path);
        }


        /// <summary>
        /// Implicit conversion from type to control definition
        /// </summary>
        /// <param name="type">Control type</param>
        public static implicit operator ControlDefinition(Type type)
        {
            return new ControlDefinition(type);
        }

        #endregion
    }
}