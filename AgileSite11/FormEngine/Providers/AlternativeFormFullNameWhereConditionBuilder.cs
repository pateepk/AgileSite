using System;

using CMS.DataEngine;

namespace CMS.FormEngine
{
    /// <summary>
    /// Builds where condition to get alternative form based on its full name.
    /// </summary>
    internal class AlternativeFormFullNameWhereConditionBuilder
    {
        private string FullName
        {
            get;
            set;
        }


        /// <summary>
        /// Creates instance of <see cref="AlternativeFormFullNameWhereConditionBuilder"/> class.
        /// </summary>
        /// <param name="fullName">Full name to use for building the where condition.</param>
        public AlternativeFormFullNameWhereConditionBuilder(string fullName)
        {
            FullName = fullName;
        }


        /// <summary>
        /// Builds the full name where condition for the given full name of the alternative form.
        /// </summary>
        public IWhereCondition Build()
        {
            if (string.IsNullOrEmpty(FullName))
            {
                return new WhereCondition();
            }

            var delimiterIndex = GetDelimiterIndex();
            if (delimiterIndex < 0)
            {
                return new WhereCondition();
            }

            var classId = GetClassId(delimiterIndex);
            var formName = GetFormName(delimiterIndex);

            return GetWhereCondition(classId, formName);
        }


        private static IWhereCondition GetWhereCondition(int classId, string formName)
        {
            return new WhereCondition()
                .WhereEquals("FormClassID", classId)
                .WhereEquals("FormName", formName);
        }


        private string GetFormName(int delimiterIndex)
        {
            return FullName.Substring(delimiterIndex + 1);
        }


        private int GetClassId(int delimiterIndex)
        {
            var className = FullName.Substring(0, delimiterIndex);
            var classInfo = DataClassInfoProvider.GetDataClassInfo(className);

            return classInfo == null ? 0 : classInfo.ClassID;
        }


        private int GetDelimiterIndex()
        {
            const string FULLNAME_DELIMITER = ".";

            // First '.' in class code name which is always in format '<namespace>.<name>'
            int dotIndex = FullName.IndexOf(FULLNAME_DELIMITER, StringComparison.Ordinal);
            if (dotIndex < 0)
            {
                return dotIndex;
            }

            // Second '.' is the delimiter between class name and form name
            return FullName.IndexOf(FULLNAME_DELIMITER, dotIndex + 1, StringComparison.Ordinal);
        }
    }
}
