using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Thrown when identifier was required and was not translated
    /// </summary>
    public class IDNotTranslatedException : Exception
    {
        /// <summary>
        /// Class name of not translated object.
        /// </summary>
        public string ClassName
        {
            get;
            private set;
        }


        /// <summary>
        /// Code name of not translated object.
        /// </summary>
        public string CodeName
        {
            get;
            private set;
        }


        /// <summary>
        /// Column name of not translated object.
        /// </summary>
        public string ColumnName
        {
            get;
            private set;
        }


        /// <summary>
        /// Identifier of not translated object.
        /// </summary>
        public int ID
        {
            get;
            private set;
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="columnName">Column name of not translated object</param>
        /// <param name="codeName">Code name of not translated object</param>
        /// <param name="className">Class name of not translated object</param>
        /// <param name="id">Identifier of not translated object</param>
        public IDNotTranslatedException(string columnName, string codeName, string className, int id)
            : base(GetErrorMessage(columnName, codeName, className, id))
        {
            ClassName = className;
            ID = id;
            ColumnName = columnName;
            CodeName = codeName;
        }


        /// <summary>
        /// Builds exception error message.
        /// </summary>
        /// <param name="columnName">Column name of not translated object</param>
        /// <param name="codeName">Code name of not translated object</param>
        /// <param name="className">Class name of not translated object</param>
        /// <param name="id">Identifier of not translated object</param>
        /// <returns>Error message based on given parameters</returns>
        private static string GetErrorMessage(string columnName, string codeName, string className, int id)
        {
            string toReturn = "Required column '" + columnName + "' was not translated.";

            if (!string.IsNullOrEmpty(codeName))
            {
                toReturn += " Object '" + codeName + "' of '" + className + "' type was not found.";
            }
            else
            {
                toReturn += " Translation record for object of '" + className + "' type with identifier '" + id + "' type was not found.";
            }
            return toReturn;
        }
    }
}