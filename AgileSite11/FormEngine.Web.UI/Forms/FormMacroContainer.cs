using System;
using System.Linq;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.MacroEngine;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Container to wrap the form structure
    /// </summary>
    public class FormMacroContainer<FormType> : ReadOnlyMacroObjectWrapper<FormType>
        where FormType : BasicForm
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="form">Wrapped form</param>
        public FormMacroContainer(FormType form)
            : base(form)
        {
        }


        /// <summary>
        /// Constructor for virtual resolvers
        /// </summary>
        public FormMacroContainer()
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers columns
        /// </summary>
        protected override void RegisterColumns()
        {
            base.RegisterColumns();

            RegisterColumn("Data", f => f?.Data);
            RegisterColumn<FormModeEnum>("FormMode", f => f?.Mode ?? FormModeEnum.Insert);
        }


        /// <summary>
        /// Registers properties
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty<StringSafeDictionary<FormControlMacroContainer>>("Fields", f =>
            {
                if (f == null)
                {
                    return null;
                }

                var fieldControls = f.FieldControls;
                var fields = new StringSafeDictionary<FormControlMacroContainer>();
                if (fieldControls != null)
                {
                    foreach (string name in fieldControls.TypedKeys)
                    {
                        fields[name] = new FormControlMacroContainer(fieldControls[name]);
                    }
                }

                return new SafeDictionaryContainer<FormControlMacroContainer>(fields);
            });
            
            RegisterProperty<StringSafeDictionary<FormCategoryInfo>>("Categories", f =>
            {
                if (f == null)
                {
                    return null;
                }

                StringSafeDictionary<FormCategoryInfo> categories = new StringSafeDictionary<FormCategoryInfo>();

                var formCategories = f.FormInformation.ItemsList.OfType<FormCategoryInfo>().Where(c => !String.IsNullOrEmpty(c.CategoryName));

                foreach (var categoryInfo in formCategories)
                {
                    categories[categoryInfo.CategoryName] = categoryInfo;
                }

                return new SafeDictionaryContainer<FormCategoryInfo>(categories);
            });
        }

        #endregion
    }
}