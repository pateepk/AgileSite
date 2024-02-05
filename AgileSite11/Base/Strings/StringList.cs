using System;

namespace CMS.Base
{
    /// <summary>
    /// Represents a list of strings separated by semicolon
    /// </summary>
    public class StringList
    {
        #region "Properties"

        private string mStringRepresentation;


        /// <summary>
        /// Items that take part in the list
        /// </summary>
        public object[] Items
        {
            get;
            protected set;
        }


        /// <summary>
        /// List separator
        /// </summary>
        public string Separator
        {
            get;
            protected set;
        }


        /// <summary>
        /// String representation of the list
        /// </summary>
        protected string StringRepresentation
        {
            get
            {
                if (Items == null)
                {
                    return null;
                }

                return mStringRepresentation ?? (mStringRepresentation = String.Join(Separator, Items));
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">List items</param>
        public StringList(params object[] items)
        {
            Separator = ";";
            Items = items;
        }


        /// <summary>
        /// Sets the separator to the list
        /// </summary>
        /// <param name="separator">Separator to use</param>
        public StringList WithSeparator(string separator)
        {
            Separator = separator;

            return this;
        }


        /// <summary>
        /// Returns the string representation of the list
        /// </summary>
        public override string ToString()
        {
            return StringRepresentation;
        }


        /// <summary>
        /// Implicit operator to convert list of strings to string
        /// </summary>
        /// <param name="list">List of values</param>
        public static implicit operator string(StringList list)
        {
            return list.ToString();
        }

        #endregion
    }
}
