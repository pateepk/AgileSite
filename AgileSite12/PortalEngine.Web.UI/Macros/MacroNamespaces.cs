using System.Collections.Generic;

using CMS.Base;
using CMS.MacroEngine;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Wrapper class to provide path namespace in the MacroEngine.
    /// </summary>
    [Extension(typeof(PathMacroMethods))]
    internal class PathNamespace : MacroNamespace<PathNamespace>
    {

    }


    /// <summary>
    /// Wrapper class to provide web part namespace in the MacroEngine.
    /// </summary>
    [Extension(typeof(WebPartMacroMethods))]
    internal class WebPartNamespace : MacroNamespace<WebPartNamespace>
    {

    }


    /// <summary>
    /// Wrapper class to provide web part zone namespace in the MacroEngine.
    /// </summary>
    [Extension(typeof(WebPartZoneMacroMethods))]
    internal class WebPartZoneNamespace : MacroNamespace<WebPartZoneNamespace>
    {

    }


    /// <summary>
    /// Document wizard class used for macro engine
    /// </summary>
    internal class DocumentWizard :  AbstractHierarchicalObject<DocumentWizard>
    {

        #region "Properties"

        /// <summary>
        /// Current steps
        /// </summary>
        public StepList Steps
        {
            get
            {
                if (Manager != null)
                {
                    return new StepList(Manager.Steps);
                }

                return new StepList();
            }
        }


        /// <summary>
        /// Current step
        /// </summary>
        public DocumentWizardStep CurrentStep
        {
            get
            {
                if (Manager != null)
                {
                    return Manager.CurrentStep;
                }

                return new DocumentWizardStep();
            }
        }


        /// <summary>
        /// Last step
        /// </summary>
        public DocumentWizardStep LastStep
        {
            get
            {
                if (Manager != null)
                {
                    return Manager.LastStep;
                }

                return new DocumentWizardStep();
            }
        }


        /// <summary>
        /// First step
        /// </summary>
        public DocumentWizardStep FirstStep
        {
            get
            {
                if (Manager != null)
                {
                    return Manager.FirstStep;
                }

                return new DocumentWizardStep();
            }
        }


        /// <summary>
        /// Next step
        /// </summary>
        public DocumentWizardStep NextStep
        {
            get
            {
                if (Manager != null)
                {
                    DocumentWizardStep step = Manager.CurrentStep;
                    if ((step != null) && ((step.StepIndex + 1) < Manager.Steps.Count))
                    {
                        return Manager.Steps[step.StepIndex + 1];
                    }
                }

                return new DocumentWizardStep();
            }
        }


        /// <summary>
        /// Previous step
        /// </summary>
        public DocumentWizardStep PreviousStep
        {
            get
            {
                if (Manager != null)
                {
                    DocumentWizardStep step = Manager.CurrentStep;
                    if ((step != null) && (step.StepIndex > 0))
                    {
                        return Manager.Steps[step.StepIndex - 1];
                    }
                }

                return new DocumentWizardStep();
            }
        }


        /// <summary>
        /// Document wizard manager
        /// </summary>
        private IDocumentWizardManager Manager
        {
            get
            {
                CMSPortalManager man = PortalContext.CurrentPageManager as CMSPortalManager;
                if (man != null)
                {
                    return man.DocumentWizardManager;
                }
                return null;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Register properties
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty<DocumentWizardStep>("CurrentStep", m => m.CurrentStep);
            
            RegisterProperty<DocumentWizardStep>("FirstStep", m => m.FirstStep);
            RegisterProperty<DocumentWizardStep>("LastStep", m => m.LastStep);

            RegisterProperty<DocumentWizardStep>("NextStep", m => m.NextStep);
            RegisterProperty<DocumentWizardStep>("PreviousStep", m => m.PreviousStep);

            RegisterProperty<List<DocumentWizardStep>>("Steps", m => m.Steps);
        }


        /// <summary>
        /// Returns value of property.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <param name="notNull">If true, the property attempts to return non-null values, at least it returns the empty object of the correct type</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetProperty(string columnName, out object value, bool notNull)
        {
            value = null;
            
            // Load virtual list for not-existing steps
            if (notNull && columnName.EqualsCSafe("Steps", true))
            {
                value = Steps;
                return true;
            }

            return base.TryGetProperty(columnName, out value, notNull);
        }

        #endregion
    }


    /// <summary>
    /// Wizard steps collection used for macro engine
    /// </summary>
    internal class StepList :  AbstractHierarchicalObject<StepList>, IIndexable
    {
        #region "Variables"

        private List<DocumentWizardStep> mList = null;
        private bool virtualMode = false;

        #endregion


        #region "Properties"

        /// <summary>
        /// Step collection
        /// </summary>
        private List<DocumentWizardStep> List
        {
            get
            {
                return mList;
            }
            set
            {
                mList = value;
            }
        }


        /// <summary>
        /// Gets the item for specified index
        /// </summary>
        /// <param name="index">Index</param>
        public object this[int index]
        {
            get
            {
                if (virtualMode)
                {
                    return new DocumentWizardStep();
                }

                return List[index];
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public StepList()
        {
            List = new List<DocumentWizardStep>();
            virtualMode = true;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="collection">Original collection</param>
        public StepList(List<DocumentWizardStep> collection)
        {
            List = collection;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Register properties
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            RegisterProperty("Count", m => m.List.Count); 
            base.RegisterProperties();
        }


        /// <summary>
        /// Register columns
        /// </summary>
        protected override void RegisterColumns()
        {
            RegisterColumn("Count", m => m.List.Count);
            base.RegisterColumns();
        }

        #endregion

    }
}

