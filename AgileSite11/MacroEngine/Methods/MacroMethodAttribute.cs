using System;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Adds action to the page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class MacroMethodAttribute : Attribute
    {
        #region "Variables"

        private Type mType = typeof(object);

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets a return type of the method.
        /// </summary>
        public Type Type
        {
            get
            {
                return mType;
            }
            set
            {
                mType = value;
            }
        }


        /// <summary>
        /// Name of the macro method.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Comment of the macro method.
        /// </summary>
        public string Comment
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a code snippet which is used in AutoCompletion when TAB is pressed (for determining the cursor position use pipe).
        /// </summary>
        public string Snippet
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the minimal number of parameters needed by the method.
        /// </summary>
        public int MinimumParameters
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the method won't be visible in IntelliSense (but will be normally executed when called).
        /// </summary>
        public bool IsHidden
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the list of special parameters needed to be supplied by resolver.
        /// </summary>
        public string[] SpecialParameters
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MacroMethodAttribute()
        {
        }


        /// <summary>
        /// Creates new MacroMethodAttribute.
        /// </summary>
        /// <param name="type">Method return type</param>
        /// <param name="comment">Comment of the method</param>
        /// <param name="minimumParameters">minimal number of parameters needed by the method</param>
        public MacroMethodAttribute(Type type, string comment, int minimumParameters)
        {
            Type = type;
            Comment = comment;
            MinimumParameters = minimumParameters;
        }

        #endregion
    }
}