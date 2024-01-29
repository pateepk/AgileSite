using System;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Ecommerce.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;

[assembly: RegisterCustomClass(nameof(ProductCouponEditExtender), typeof(ProductCouponEditExtender))]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Extender for Product Coupon Discount new/edit page
    /// </summary>
    public class ProductCouponEditExtender : ControlExtender<UIForm>
    {
        private const string BRANDS = "brands";
        private const string PRODUCTS = "products";
        private const string DEPARTMENTS = "departments";
        private const string COLLECTIONS = "collections";
        private const string SECTIONS = "sections";

        private const string APPLICATION_FIELD_NAME = "BuyDepartmentOrProduct";
        private const string EXCLUDE_FIELD_NAME = "Exclude";
        private const string EXCLUDED_BRANDS_FIELD_NAME = "ExcludedBrands";
        private const string EXCLUDED_COLLECTION_FIELD_NAME = "ExcludedCollections";
        private const string EXCLUDED_SECTION_FIELD_NAME = "ExcludedSections";
        private const string PRODUCT_FIELD_NAME = "BuyProductSet";
        private const string DEPARTMENT_FIELD_NAME = "BuyDepartmentSet";
        private const string BRAND_FIELD_NAME = "BuyBrandSet";
        private const string COLLECTION_FIELD_NAME = "BuyCollectionSet";
        private const string SECTION_FIELD_NAME = "BuySectionSet";


        /// <summary>
        /// Returns edited discount info object.
        /// </summary>
        protected MultiBuyDiscountInfo Discount
        {
            get
            {
                return Control.EditedObject as MultiBuyDiscountInfo;
            }
        }


        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            if (Control == null)
            {
                return;
            }

            Control.OnAfterDataLoad += Control_OnAfterDataLoad;
            Control.OnItemValidation += ControlOnOnItemValidation;
            Control.OnAfterSave += Control_OnAfterSave;
        }


        /// <summary>
        /// Check that combination of included and excluded sections leaves at least section included.
        /// </summary>
        private void ControlOnOnItemValidation(object sender, ref string errorMessage)
        {
            FormEngineUserControl ctrl = sender as FormEngineUserControl;
            if ((ctrl != null) && (ctrl.FieldInfo.Name == SECTION_FIELD_NAME))
            {
                if (GetFieldValue(APPLICATION_FIELD_NAME) != SECTIONS)
                {
                    return;
                }

                var includedSectionsSelection = GetIds(SECTION_FIELD_NAME).ToArray();
                var excludedSectionsSelection = GetIds(EXCLUDED_SECTION_FIELD_NAME).ToArray();

                // No sections are set as included nor excluded. Validation is handled in UI.
                if ((!includedSectionsSelection.Any()) && (!excludedSectionsSelection.Any()))
                {
                    return;
                }

                // If section is set as both included and excluded, it is saved to database only as excluded. There is possibility that discount will have only excluded sections set, with empty included sections.
                if (!includedSectionsSelection.Except(excludedSectionsSelection).Any())
                {
                    errorMessage = ResHelper.GetString("com.multibuydiscount.section.includeequalsexclude");
                }
            }
        }


        private void Control_OnAfterDataLoad(object sender, EventArgs e)
        {
            SetControlData();

            Control.FieldControls[APPLICATION_FIELD_NAME].Changed += (s, a) =>
            {
                SetExcludedBrandsData();
                SetExcludedCollectionsData();
                SetExcludedSectionsData();
            };
        }


        private void Control_OnAfterSave(object sender, EventArgs e)
        {
            SaveApplication();
        }


        /// <summary>
        /// Sets the discount data into form controls
        /// </summary>
        private void SetControlData()
        {
            if (Discount == null || RequestHelper.IsPostBack())
            {
                return;
            }

            SetProductData();
            SetDepartmentData();
            SetIncludedBrandData();
            SetIncludedCollectionData();
            SetIncludedSectionData();
            SetExcludedBrandsData(true);
            SetExcludedCollectionsData(true);
            SetExcludedSectionsData(true);
        }


        /// <summary>
        /// Sets product related data from discount to form
        /// </summary>
        private void SetProductData()
        {
            if (!Discount.UseProducts)
            {
                return;
            }

            Control.FieldControls[APPLICATION_FIELD_NAME].Value = PRODUCTS;
            Control.FieldControls[PRODUCT_FIELD_NAME].Value = TextHelper.Join(";", Discount.MultiBuyDiscountProducts.Select(i => i.SKUID.ToString()));
        }


        /// <summary>
        /// Sets department related data from discount to form
        /// </summary>
        private void SetDepartmentData()
        {
            if (!Discount.UseDepartments)
            {
                return;
            }

            Control.FieldControls[APPLICATION_FIELD_NAME].Value = DEPARTMENTS;
            Control.FieldControls[DEPARTMENT_FIELD_NAME].Value = TextHelper.Join(";", Discount.MultiBuyDiscountDepartments.Select(i => i.DepartmentID.ToString()));
            Control.FieldControls[EXCLUDE_FIELD_NAME].ReloadControl();
        }


        /// <summary>
        /// Sets brand related data from discount to form
        /// </summary>
        private void SetIncludedBrandData()
        {
            if (!Discount.MultiBuyDiscountIncludedBrands.Any())
            {
                return;
            }

            Control.FieldControls[APPLICATION_FIELD_NAME].Value = BRANDS;
            Control.FieldControls[BRAND_FIELD_NAME].Value = TextHelper.Join(";", Discount.MultiBuyDiscountIncludedBrands.Select(i => i.BrandID.ToString()));
            Control.FieldControls[EXCLUDE_FIELD_NAME].ReloadControl();
        }


        /// <summary>
        /// Sets brand related data from discount to form
        /// </summary>
        /// <param name="reloadSelector">if true loads data from discount to selector</param>
        private void SetExcludedBrandsData(bool reloadSelector = false)
        {
            if (!Discount.MultiBuyDiscountExcludedBrands.Any())
            {
                return;
            }

            Control.FieldControls[EXCLUDE_FIELD_NAME].Value = BRANDS;
            Control.FieldControls[EXCLUDE_FIELD_NAME].ReloadControl();

            if (reloadSelector)
            {
                Control.FieldControls[EXCLUDED_BRANDS_FIELD_NAME].Value = TextHelper.Join(";", Discount.MultiBuyDiscountExcludedBrands.Select(i => i.BrandID.ToString()));
            }
        }


        /// <summary>
        /// Sets collection related data from discount to form
        /// </summary>
        private void SetIncludedCollectionData()
        {
            if (!Discount.MultiBuyDiscountIncludedCollections.Any())
            {
                return;
            }

            Control.FieldControls[APPLICATION_FIELD_NAME].Value = COLLECTIONS;
            Control.FieldControls[COLLECTION_FIELD_NAME].Value = TextHelper.Join(";", Discount.MultiBuyDiscountIncludedCollections.Select(i => i.CollectionID.ToString()));
            Control.FieldControls[EXCLUDE_FIELD_NAME].ReloadControl();
        }


        /// <summary>
        /// Sets collection related data from discount to form
        /// </summary>
        /// <param name="reloadSelector">if true loads data from discount to selector</param>
        private void SetExcludedCollectionsData(bool reloadSelector = false)
        {
            if (!Discount.MultiBuyDiscountExcludedCollections.Any())
            {
                return;
            }

            Control.FieldControls[EXCLUDE_FIELD_NAME].Value = COLLECTIONS;
            Control.FieldControls[EXCLUDE_FIELD_NAME].ReloadControl();

            if (reloadSelector)
            {
                Control.FieldControls[EXCLUDED_COLLECTION_FIELD_NAME].Value = TextHelper.Join(";", Discount.MultiBuyDiscountExcludedCollections.Select(i => i.CollectionID.ToString()));
            }
        }


        /// <summary>
        /// Sets section related data from discount to form
        /// </summary>
        private void SetIncludedSectionData()
        {
            if (!Discount.MultiBuyDiscountIncludedSections.Any())
            {
                return;
            }

            Control.FieldControls[APPLICATION_FIELD_NAME].Value = SECTIONS;
            Control.FieldControls[SECTION_FIELD_NAME].Value = TextHelper.Join(";", Discount.MultiBuyDiscountIncludedSections.Select(i => i.NodeID.ToString()));
            Control.FieldControls[EXCLUDE_FIELD_NAME].ReloadControl();
        }


        /// <summary>
        /// Sets section related data from discount to form
        /// </summary>
        /// <param name="reloadSelector">if true loads data from discount to selector</param>
        private void SetExcludedSectionsData(bool reloadSelector = false)
        {
            if (!Discount.MultiBuyDiscountExcludedSections.Any())
            {
                return;
            }

            Control.FieldControls[EXCLUDE_FIELD_NAME].Value = SECTIONS;
            Control.FieldControls[EXCLUDE_FIELD_NAME].ReloadControl();

            if (reloadSelector)
            {
                Control.FieldControls[EXCLUDED_SECTION_FIELD_NAME].Value = TextHelper.Join(";", Discount.MultiBuyDiscountExcludedSections.Select(i => i.NodeID.ToString()));
            }
        }


        /// <summary>
        /// Saves or removes application related data to database
        /// </summary>
        private void SaveApplication()
        {
            if (Discount == null)
            {
                return;
            }

            HandleExcludeFieldChange();
            HandleIncludeFieldChange();
        }

        private void HandleIncludeFieldChange()
        {
            var currentIncludeApplication = GetFieldValue(APPLICATION_FIELD_NAME);
            var isIncludeApplicationChanged = IsApplicationChanged(currentIncludeApplication);

            switch (currentIncludeApplication)
            {
                case PRODUCTS:

                    if (isIncludeApplicationChanged)
                    {
                        RemoveDepartments();
                        RemoveIncludedBrands();
                        RemoveExcludedBrands();
                        RemoveIncludedCollections();
                        RemoveExcludedCollections();
                        RemoveIncludedSections();
                        RemoveExcludedSections();
                    }

                    SaveProducts();

                    ClearField(DEPARTMENT_FIELD_NAME);
                    ClearField(BRAND_FIELD_NAME);
                    ClearField(COLLECTION_FIELD_NAME);
                    ClearField(SECTION_FIELD_NAME);

                    break;

                case DEPARTMENTS:

                    if (isIncludeApplicationChanged)
                    {
                        RemoveProducts();
                        RemoveIncludedBrands();
                        RemoveIncludedCollections();
                        RemoveIncludedSections();
                    }

                    SaveDepartments();
                    SaveExcludedBrands();
                    SaveExcludedCollections();
                    SaveExcludedSections();

                    ClearField(PRODUCT_FIELD_NAME);
                    ClearField(BRAND_FIELD_NAME);
                    ClearField(COLLECTION_FIELD_NAME);
                    ClearField(SECTION_FIELD_NAME);

                    break;

                case BRANDS:

                    if (isIncludeApplicationChanged)
                    {
                        RemoveProducts();
                        RemoveDepartments();
                        RemoveExcludedBrands();
                        RemoveIncludedCollections();
                        RemoveIncludedSections();
                    }

                    SaveIncludedBrands();
                    SaveExcludedCollections();
                    SaveExcludedSections();

                    ClearField(PRODUCT_FIELD_NAME);
                    ClearField(DEPARTMENT_FIELD_NAME);
                    ClearField(COLLECTION_FIELD_NAME);
                    ClearField(SECTION_FIELD_NAME);

                    break;

                case COLLECTIONS:

                    if (isIncludeApplicationChanged)
                    {
                        RemoveProducts();
                        RemoveDepartments();
                        RemoveIncludedBrands();
                        RemoveExcludedCollections();
                        RemoveIncludedSections();
                    }

                    SaveIncludedCollections();
                    SaveExcludedBrands();
                    SaveExcludedSections();

                    ClearField(PRODUCT_FIELD_NAME);
                    ClearField(DEPARTMENT_FIELD_NAME);
                    ClearField(BRAND_FIELD_NAME);
                    ClearField(SECTION_FIELD_NAME);

                    break;

                case SECTIONS:

                    if (isIncludeApplicationChanged)
                    {
                        RemoveProducts();
                        RemoveDepartments();
                        RemoveIncludedBrands();
                        RemoveIncludedCollections();
                    }

                    SaveIncludedSections();
                    SaveExcludedBrands();
                    SaveExcludedCollections();
                    SaveExcludedSections();

                    ClearField(PRODUCT_FIELD_NAME);
                    ClearField(DEPARTMENT_FIELD_NAME);
                    ClearField(BRAND_FIELD_NAME);
                    ClearField(COLLECTION_FIELD_NAME);

                    break;
            }
        }


        private void HandleExcludeFieldChange()
        {
            var currentExclude = GetFieldValue(EXCLUDE_FIELD_NAME);
            var isExclusionChanged = IsExclusionChanged(currentExclude);

            switch (currentExclude)
            {
                case BRANDS:

                    if (isExclusionChanged)
                    {
                        RemoveExcludedCollections();
                        RemoveExcludedSections();
                        ClearField(EXCLUDED_COLLECTION_FIELD_NAME);
                        ClearField(EXCLUDED_SECTION_FIELD_NAME);
                    }

                    break;

                case COLLECTIONS:

                    if (isExclusionChanged)
                    {
                        RemoveExcludedBrands();
                        RemoveExcludedSections();
                        ClearField(EXCLUDED_BRANDS_FIELD_NAME);
                        ClearField(EXCLUDED_SECTION_FIELD_NAME);
                    }

                    break;

                case SECTIONS:

                    if (isExclusionChanged)
                    {
                        RemoveExcludedBrands();
                        RemoveExcludedCollections();
                        ClearField(EXCLUDED_BRANDS_FIELD_NAME);
                        ClearField(EXCLUDED_COLLECTION_FIELD_NAME);
                    }

                    break;

                default:

                    if (isExclusionChanged)
                    {
                        RemoveExcludedBrands();
                        RemoveExcludedCollections();
                        RemoveExcludedSections();
                        ClearField(EXCLUDED_BRANDS_FIELD_NAME);
                        ClearField(EXCLUDED_COLLECTION_FIELD_NAME);
                        ClearField(EXCLUDED_SECTION_FIELD_NAME);
                    }

                    break;
            }
        }


        private void SaveProducts()
        {
            var selectedIds = GetIds(PRODUCT_FIELD_NAME).ToArray();
            var existingIds = Discount.MultiBuyDiscountProducts.Select(i => i.SKUID).ToArray();

            foreach (var id in selectedIds.Except(existingIds))
            {
                MultiBuyDiscountSKUInfoProvider.AddMultiBuyDiscountToProduct(Discount.MultiBuyDiscountID, id);
            }

            foreach (var id in existingIds.Except(selectedIds))
            {
                MultiBuyDiscountSKUInfoProvider.RemoveMultiBuyDiscountFromProduct(Discount.MultiBuyDiscountID, id);
            }
        }


        private void SaveDepartments()
        {
            var selectedIds = GetIds(DEPARTMENT_FIELD_NAME).ToArray();
            var existingIds = Discount.MultiBuyDiscountDepartments.Select(i => i.DepartmentID).ToArray();

            foreach (var id in selectedIds.Except(existingIds))
            {
                MultiBuyDiscountDepartmentInfoProvider.AddMultiBuyDiscountToDepartment(Discount.MultiBuyDiscountID, id);
            }

            foreach (var id in existingIds.Except(selectedIds))
            {
                MultiBuyDiscountDepartmentInfoProvider.RemoveMultiBuyDiscountFromDepartment(Discount.MultiBuyDiscountID, id);
            }
        }


        private void SaveIncludedBrands()
        {
            var selectedIds = GetIds(BRAND_FIELD_NAME).ToArray();
            var existingIds = Discount.MultiBuyDiscountIncludedBrands.Select(i => i.BrandID).ToArray();

            foreach (var id in selectedIds.Except(existingIds))
            {
                MultiBuyDiscountBrandInfoProvider.AddMultiBuyDiscountToBrand(Discount.MultiBuyDiscountID, id, true);
            }

            foreach (var id in existingIds.Except(selectedIds))
            {
                MultiBuyDiscountBrandInfoProvider.RemoveMultiBuyDiscountFromBrand(Discount.MultiBuyDiscountID, id);
            }
        }


        private void SaveExcludedBrands()
        {
            var selectedIds = GetIds(EXCLUDED_BRANDS_FIELD_NAME).ToArray();
            var existingIds = Discount.MultiBuyDiscountExcludedBrands.Select(i => i.BrandID).ToArray();

            foreach (var id in selectedIds.Except(existingIds))
            {
                MultiBuyDiscountBrandInfoProvider.AddMultiBuyDiscountToBrand(Discount.MultiBuyDiscountID, id, false);
            }

            foreach (var id in existingIds.Except(selectedIds))
            {
                MultiBuyDiscountBrandInfoProvider.RemoveMultiBuyDiscountFromBrand(Discount.MultiBuyDiscountID, id);
            }
        }


        private void SaveIncludedCollections()
        {
            var selectedIds = GetIds(COLLECTION_FIELD_NAME).ToArray();
            var existingIds = Discount.MultiBuyDiscountIncludedCollections.Select(i => i.CollectionID).ToArray();

            foreach (var id in selectedIds.Except(existingIds))
            {
                MultiBuyDiscountCollectionInfoProvider.AddMultiBuyDiscountToCollection(Discount.MultiBuyDiscountID, id, true);
            }

            foreach (var id in existingIds.Except(selectedIds))
            {
                MultiBuyDiscountCollectionInfoProvider.RemoveMultiBuyDiscountFromCollection(Discount.MultiBuyDiscountID, id);
            }
        }


        private void SaveExcludedCollections()
        {
            var selectedIds = GetIds(EXCLUDED_COLLECTION_FIELD_NAME).ToArray();
            var existingIds = Discount.MultiBuyDiscountExcludedCollections.Select(i => i.CollectionID).ToArray();

            foreach (var id in selectedIds.Except(existingIds))
            {
                MultiBuyDiscountCollectionInfoProvider.AddMultiBuyDiscountToCollection(Discount.MultiBuyDiscountID, id, false);
            }

            foreach (var id in existingIds.Except(selectedIds))
            {
                MultiBuyDiscountCollectionInfoProvider.RemoveMultiBuyDiscountFromCollection(Discount.MultiBuyDiscountID, id);
            }
        }


        private void SaveIncludedSections()
        {
            var selectedIds = GetIds(SECTION_FIELD_NAME).ToArray();
            var existingIds = Discount.MultiBuyDiscountIncludedSections.Select(i => i.NodeID).ToArray();

            foreach (var id in selectedIds.Except(existingIds))
            {
                MultiBuyDiscountTreeInfoProvider.AddMultiBuyDiscountToTree(Discount.MultiBuyDiscountID, id, true);
            }

            foreach (var id in existingIds.Except(selectedIds))
            {
                MultiBuyDiscountTreeInfoProvider.RemoveMultiBuyDiscountFromTree(Discount.MultiBuyDiscountID, id);
            }
        }


        private void SaveExcludedSections()
        {
            var selectedIds = GetIds(EXCLUDED_SECTION_FIELD_NAME).ToArray();
            var existingIds = Discount.MultiBuyDiscountExcludedSections.Select(i => i.NodeID).ToArray();

            foreach (var id in selectedIds.Except(existingIds))
            {
                MultiBuyDiscountTreeInfoProvider.AddMultiBuyDiscountToTree(Discount.MultiBuyDiscountID, id, false);
            }

            foreach (var id in existingIds.Except(selectedIds))
            {
                MultiBuyDiscountTreeInfoProvider.RemoveMultiBuyDiscountFromTree(Discount.MultiBuyDiscountID, id);
            }
        }


        private void RemoveProducts()
        {
            foreach (var id in Discount.MultiBuyDiscountProducts.Select(i => i.SKUID))
            {
                MultiBuyDiscountSKUInfoProvider.RemoveMultiBuyDiscountFromProduct(Discount.MultiBuyDiscountID, id);
            }
        }


        private void RemoveDepartments()
        {
            foreach (var id in Discount.MultiBuyDiscountDepartments.Select(i => i.DepartmentID))
            {
                MultiBuyDiscountDepartmentInfoProvider.RemoveMultiBuyDiscountFromDepartment(Discount.MultiBuyDiscountID, id);
            }
        }


        private void RemoveIncludedBrands()
        {
            foreach (var id in Discount.MultiBuyDiscountIncludedBrands.Select(i => i.BrandID))
            {
                MultiBuyDiscountBrandInfoProvider.RemoveMultiBuyDiscountFromBrand(Discount.MultiBuyDiscountID, id);
            }
        }


        private void RemoveExcludedBrands()
        {
            foreach (var id in Discount.MultiBuyDiscountExcludedBrands.Select(i => i.BrandID))
            {
                MultiBuyDiscountBrandInfoProvider.RemoveMultiBuyDiscountFromBrand(Discount.MultiBuyDiscountID, id);
            }
        }


        private void RemoveIncludedCollections()
        {
            foreach (var id in Discount.MultiBuyDiscountIncludedCollections.Select(i => i.CollectionID))
            {
                MultiBuyDiscountCollectionInfoProvider.RemoveMultiBuyDiscountFromCollection(Discount.MultiBuyDiscountID, id);
            }
        }


        private void RemoveExcludedCollections()
        {
            foreach (var id in Discount.MultiBuyDiscountExcludedCollections.Select(i => i.CollectionID))
            {
                MultiBuyDiscountCollectionInfoProvider.RemoveMultiBuyDiscountFromCollection(Discount.MultiBuyDiscountID, id);
            }
        }


        private void RemoveIncludedSections()
        {
            foreach (var id in Discount.MultiBuyDiscountIncludedSections.Select(i => i.NodeID))
            {
                MultiBuyDiscountTreeInfoProvider.RemoveMultiBuyDiscountFromTree(Discount.MultiBuyDiscountID, id);
            }
        }


        private void RemoveExcludedSections()
        {
            foreach (var id in Discount.MultiBuyDiscountExcludedSections.Select(i => i.NodeID))
            {
                MultiBuyDiscountTreeInfoProvider.RemoveMultiBuyDiscountFromTree(Discount.MultiBuyDiscountID, id);
            }
        }


        private IEnumerable<int> GetIds(string fieldName)
        {
            var stringIds = GetFieldValue(fieldName).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var ids = ValidationHelper.GetIntegers(stringIds.OfType<object>().ToArray(), 0);

            return ids;
        }


        /// <summary>
        /// Returns true when exlude value was changed during discount edit.
        /// </summary>
        private bool IsExclusionChanged(string currentExclude)
        {
            if (currentExclude == BRANDS && Discount.MultiBuyDiscountExcludedBrands.Any())
            {
                return false;
            }

            if (currentExclude == COLLECTIONS && Discount.MultiBuyDiscountExcludedCollections.Any())
            {
                return false;
            }

            if (currentExclude == SECTIONS && Discount.MultiBuyDiscountExcludedSections.Any())
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns true when applies to value was changed during discount edit.
        /// </summary>
        private bool IsApplicationChanged(string currentApplication)
        {
            if (currentApplication == PRODUCTS && Discount.UseProducts)
            {
                return false;
            }

            if (currentApplication == DEPARTMENTS && Discount.UseDepartments)
            {
                return false;
            }

            if (currentApplication == BRANDS && Discount.MultiBuyDiscountIncludedBrands.Any())
            {
                return false;
            }

            if (currentApplication == COLLECTIONS && Discount.MultiBuyDiscountIncludedCollections.Any())
            {
                return false;
            }

            if (currentApplication == SECTIONS && Discount.MultiBuyDiscountIncludedSections.Any())
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns value for given field name as string.
        /// </summary>
        protected string GetFieldValue(string fieldName)
        {
            return Control.FieldControls[fieldName].Value.ToString(string.Empty);
        }


        /// <summary>
        /// Set form control value of given <paramref name="fieldName"/> to null.
        /// </summary>
        protected void ClearField(string fieldName)
        {
            Control.FieldControls[fieldName].Value = null;
        }
    }
}