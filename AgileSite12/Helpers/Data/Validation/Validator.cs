using System;

namespace CMS.Helpers
{
    /// <summary>
    /// Provides fluent interface for validation of user input.
    /// </summary>
    public class Validator
    {
        #region "Variables"

        /// <summary>
        /// A value indicating whether the validation was successful.
        /// </summary>
        protected bool mIsValid = true;


        /// <summary>
        /// The last validation error message.
        /// </summary>
        protected string mResult = "";

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets the last validation error message.
        /// </summary>
        public string Result
        {
            get
            {
                return mResult;
            }
        }


        /// <summary>
        /// Gets a value indicating whether the validation was successful.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return mIsValid;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Validates source control for emptiness.
        /// </summary>
        /// <param name="source">Control to validate</param>
        /// <param name="errorMessage">It is returned when control is empty</param>
        public Validator NotEmpty(object source, string errorMessage)
        {
            if (mIsValid)
            {
                if (source == null || source.ToString().Length == 0 || source is DBNull)
                {
                    Fail(errorMessage);
                }
            }

            return this;
        }


        /// <summary>
        /// Validates source control for null value.
        /// </summary>
        /// <param name="source">Control to validate</param>
        /// <param name="errorMessage">It is returned when control is null</param>
        public Validator NotNull(object source, string errorMessage)
        {
            if (mIsValid)
            {
                if (source == null || source is DBNull)
                {
                    Fail(errorMessage);
                }
            }

            return this;
        }


        /// <summary>
        /// Determines whether source control's value is integer.
        /// </summary>
        /// <param name="source">Control to validate</param>
        /// <param name="errorMessage">It is returned when control's value is not integer</param>
        public Validator IsInteger(object source, string errorMessage)
        {
            if (mIsValid)
            {
                if (!ValidationHelper.IsInteger(source))
                {
                    Fail(errorMessage);
                }
            }

            return this;
        }


        /// <summary>
        /// Determines whether source control's value is positive number.
        /// </summary>
        /// <param name="source">Control to validate</param>
        /// <param name="errorMessage">It is returned when control's value is not integer</param>
        /// <param name="allowEmpty">If true, an empty value is allowed</param>
        public Validator IsPositiveNumber(object source, string errorMessage, bool allowEmpty = false)
        {
            if (mIsValid)
            {
                // If empty and allow empty, accept
                if (allowEmpty && (ValidationHelper.GetString(source, "") == ""))
                {
                    return this;
                }
                // Check for being positive number
                if (!ValidationHelper.IsPositiveNumber(source))
                {
                    Fail(errorMessage);
                }
            }

            return this;
        }


        /// <summary>
        /// Determines whether source control's value is double.
        /// </summary>
        /// <param name="source">Control to validate</param>
        /// <param name="errorMessage">It is returned when control's value is not double</param>
        public Validator IsDouble(object source, string errorMessage)
        {
            if (mIsValid)
            {
                if (!ValidationHelper.IsDouble(source))
                {
                    Fail(errorMessage);
                }
            }

            return this;
        }


        /// <summary>
        /// Determines whether source control's value is decimal.
        /// </summary>
        /// <param name="source">Control to validate</param>
        /// <param name="errorMessage">It is returned when control's value is not decimal</param>
        public Validator IsDecimal(object source, string errorMessage)
        {
            if (mIsValid)
            {
                if (!ValidationHelper.IsDecimal(source))
                {
                    Fail(errorMessage);
                }
            }

            return this;
        }


        /// <summary>
        /// Determines whether source control's value is boolean.
        /// </summary>
        /// <param name="source">Control to validate</param>
        /// <param name="errorMessage">It is returned when control's value is not boolean</param>
        public Validator IsBoolean(object source, string errorMessage)
        {
            if (mIsValid)
            {
                if (!ValidationHelper.IsBoolean(source))
                {
                    Fail(errorMessage);
                }
            }

            return this;
        }


        /// <summary>
        /// Determines whether source control's value is valid email.
        /// </summary>
        /// <param name="source">Control to validate</param>
        /// <param name="errorMessage">It is returned when control's value is not valid email</param>
        /// <param name="checkLength">if <c>true</c> checks whether email length is no longer than <see cref="ValidationHelper.SINGLE_EMAIL_LENGTH"/></param>
        public Validator IsEmail(object source, string errorMessage, bool checkLength = false)
        {
            if (mIsValid)
            {
                if (!ValidationHelper.IsEmail(source, checkLength))
                {
                    Fail(errorMessage);
                }
            }

            return this;
        }


        /// <summary>
        /// Determines whether source control's value matches file name.
        /// </summary>
        /// <param name="source">Control to validate</param>
        /// <param name="errorMessage">It is returned when control's value is not file name</param>
        public Validator IsFileName(object source, string errorMessage)
        {
            if (mIsValid)
            {
                if (!ValidationHelper.IsFileName(source))
                {
                    Fail(errorMessage);
                }
            }

            return this;
        }


        /// <summary>
        /// Determines whether source control's value matches folder name.
        /// </summary>
        /// <param name="source">Control to validate</param>
        /// <param name="errorMessage">It is returned when control's value is not folder name</param>
        public Validator IsFolderName(object source, string errorMessage)
        {
            if (mIsValid)
            {
                if (!ValidationHelper.IsFolderName(source))
                {
                    Fail(errorMessage);
                }
            }

            return this;
        }


        /// <summary>
        /// Determines whether source control's value is identifier.
        /// </summary>
        /// <param name="source">Control to validate</param>
        /// <param name="errorMessage">It is returned when control's value is not identifier</param>
        public Validator IsIdentifier(object source, string errorMessage)
        {
            if (mIsValid)
            {
                if (!ValidationHelper.IsIdentifier(source))
                {
                    Fail(errorMessage);
                }
            }

            return this;
        }


        /// <summary>
        /// Determines whether source control's value is code name.
        /// </summary>
        /// <param name="source">Control to validate</param>
        /// <param name="errorMessage">It is returned when control's value is not code name</param>
        /// <param name="useUnicode">If true, unicode letters are allowed in codename</param>
        public Validator IsCodeName(object source, string errorMessage, bool useUnicode = false)
        {
            if (mIsValid)
            {
                if (!ValidationHelper.IsCodeName(source, useUnicode))
                {
                    Fail(errorMessage);
                }
            }

            return this;
        }


        /// <summary>
        /// Determines whether source control's value is user name.
        /// </summary>
        /// <param name="source">Control to validate</param>
        /// <param name="errorMessage">It is returned when control's value is not user name</param>
        public Validator IsUserName(object source, string errorMessage)
        {
            if (mIsValid)
            {
                if (!ValidationHelper.IsUserName(source))
                {
                    Fail(errorMessage);
                }
            }

            return this;
        }


        /// <summary>
        /// Determines whether source control's value matches the regular expression.
        /// </summary>
        /// <param name="source">Control to validate</param>
        /// <param name="regExp">Regular expression</param>
        /// <param name="errorMessage">It is returned when control's value doesn't match the regular expression</param>
        public Validator IsRegularExp(object source, string regExp, string errorMessage)
        {
            if (mIsValid)
            {
                if (!ValidationHelper.IsRegularExp(source, regExp))
                {
                    Fail(errorMessage);
                }
            }

            return this;
        }


        /// <summary>
        /// Validates whether the specified value meets the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of value to validate.</typeparam>
        /// <param name="source">A value to validate.</param>
        /// <param name="condition">A condition to match.</param>
        /// <param name="errorMessage">The error message to report when validation fails.</param>
        public Validator MatchesCondition<T>(T source, Func<T, bool> condition, string errorMessage)
        {
            if (mIsValid)
            {
                if (!condition(source))
                {
                    Fail(errorMessage);
                }
            }

            return this;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Fails validation with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message to report.</param>
        private void Fail(string errorMessage)
        {
            mResult = errorMessage;
            mIsValid = false;
        }

        #endregion
    }
}