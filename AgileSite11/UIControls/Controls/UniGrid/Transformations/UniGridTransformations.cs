using System;

using CMS.Base;
using CMS.Helpers;

namespace CMS.UIControls
{
    using TransformationFunc = Func<object, object>;

    /// <summary>
    /// UniGrid built-in transformations
    /// </summary>
    public class UniGridTransformations : StringSafeDictionary<TransformationFunc>
    {
        #region "Variables"

        /// <summary>
        /// Global transformations
        /// </summary>
        private static UniGridTransformations mGlobal;

        /// <summary>
        /// Locking object
        /// </summary>
        private static readonly object lockObject = new object();

        /// <summary>
        /// Transformation prefix
        /// </summary>
        public const string TRANSFORM_PREFIX = "#transform:";

        #endregion


        #region "Properties"

        /// <summary>
        /// Global transformations
        /// </summary>
        public static UniGridTransformations Global
        {
            get
            {
                return LockHelper.Ensure(ref mGlobal, GetGlobalTransformations, lockObject);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the global set of UniGrid transformations
        /// </summary>
        private static UniGridTransformations GetGlobalTransformations()
        {
            // Register the global transformations
            UniGridTransformations tr = new UniGridTransformations();

            tr.RegisterTransformation("#yesno", UniGridFunctions.ColoredSpanYesNo);
            tr.RegisterTransformation("#yesnocolorless", UniGridFunctions.ColorLessSpanYesNo);
            tr.RegisterTransformation("#yes", UniGridFunctions.ColoredSpanYes);
            tr.RegisterTransformation("#isnullyesno", UniGridFunctions.ColoredSpanIsNullYesNo);

            tr.RegisterTransformation("#colorbar", UniGridFunctions.ColorBar);

            tr.RegisterTransformation("#sitename", UniGridFunctions.SiteName);
            tr.RegisterTransformation("#sitenameorglobal", UniGridFunctions.SiteNameOrGlobal);

            tr.RegisterTransformation("#countryname", UniGridFunctions.CountryName);

            tr.RegisterTransformation("#cultureshortname", UniGridFunctions.CultureShortName);
            tr.RegisterTransformation("#culturename", UniGridFunctions.CultureName);
            tr.RegisterTransformation("#culturenamewithflag", UniGridFunctions.CultureNameWithFlag);

            tr.RegisterTransformation("#username", UniGridFunctions.UserName);
            tr.RegisterTransformation("#formattedusername", UniGridFunctions.FormattedUserName);

            tr.RegisterTransformation("#htmlencode", UniGridFunctions.HTMLEncode);
            tr.RegisterTransformation("#url", UniGridFunctions.Link);
            tr.RegisterTransformation("#filesize", UniGridFunctions.FileSize);
            tr.RegisterTransformation("#mailto", UniGridFunctions.MailTo);
            tr.RegisterTransformation("#classname", UniGridFunctions.ClassName);

            tr.RegisterTransformation("#userdatetimegmt", UniGridFunctions.UserDateTimeGMT);
            tr.RegisterTransformation("#userdatetime", UniGridFunctions.UserDateTime);
            tr.RegisterTransformation("#usertime", UniGridFunctions.UserTime);
            tr.RegisterTransformation("#usertimezonename", UniGridFunctions.UserTimeZoneName);

            tr.RegisterTransformation("#date", UniGridFunctions.Date);
            tr.RegisterTransformation("#time", UniGridFunctions.Time);

            tr.RegisterTransformation("#objecttypename", UniGridFunctions.ObjectTypeName);

            tr.RegisterTransformation("#published", UniGridFunctions.IsPublished);

            return tr;
        }


        /// <summary>
        /// Registers the given transformation
        /// </summary>
        /// <param name="name">Transformation name</param>
        /// <param name="func">Transformation function</param>
        public void RegisterTransformation(string name, TransformationFunc func)
        {
            this[name] = func;
        }


        /// <summary>
        /// Executes the given transformation, returns true if the transformation was found and executed
        /// </summary>
        /// <param name="sender">Sender component</param>
        /// <param name="name">Transformation name</param>
        /// <param name="parameter">Transformation parameter which is transformed to the result</param>
        public bool ExecuteTransformation(object sender, string name, ref object parameter)
        {
            string defaultValue = null;

            // Extract default value if present
            int sepIndex = name.IndexOf('|');
            if (sepIndex > 0)
            {
                defaultValue = name.Substring(sepIndex + 1);
                defaultValue = ResHelper.LocalizeString(defaultValue);

                name = name.Substring(0, sepIndex);
            }

            // Try to find the transformation
            TransformationFunc tr = this[name];
            if (tr != null)
            {
                parameter = tr(parameter);

                // Set default value if available
                if ((defaultValue != null) &&
                    ((parameter == null) || (parameter == DBNull.Value) || ((parameter is string) && String.IsNullOrEmpty((string)parameter)))
                    )
                {
                    parameter = defaultValue;
                }

                return true;
            }

            return false;
        }

        #endregion
    }
}
