using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Represents return value of special commands in macro engine (break, continue, etc.).
    /// </summary>
    internal class MacroCommand
    {
        #region "Variables"

        /// <summary>
        /// Name of the command.
        /// </summary>
        protected string mName = null;

        /// <summary>
        /// Value of the command (needed for return statement)
        /// </summary>
        protected object mValue = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Name of the command.
        /// </summary>
        public string Name
        {
            get
            {
                if (mName != null)
                {
                    return mName.ToLowerCSafe();
                }
                return null;
            }
            set
            {
                mName = value;
            }
        }


        /// <summary>
        /// Value of the command (needed for return statement)
        /// </summary>
        public object Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor - creates a macro command.
        /// </summary>
        /// <param name="name">Name of the command</param>
        /// <param name="value">Value of the command</param>
        public MacroCommand(string name, object value)
        {
            if (name != null)
            {
                Name = name.ToLowerCSafe();
            }
            mValue = value;
        }

        #endregion


        /// <summary>
        /// If value is not null, returns value.ToString(), otherwise returns name of the command.
        /// </summary>
        public override string ToString()
        {
            if (mValue != null)
            {
                return mValue.ToString();
            }
            return mName;
        }
    }
}