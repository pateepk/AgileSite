using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Modules;
using CMS.Personas.Web.UI;
using CMS.SiteProvider;
using CMS.UIControls;

[assembly: RegisterExtender(typeof (DocumentListMassActionsExtender))]

namespace CMS.Personas.Web.UI
{
    /// <summary>
    /// Extender of the mass actions on the document listing page. Adds to persona related actions: 
    /// mass tagging and mass untagging.
    /// </summary>
    internal sealed class DocumentListMassActionsExtender : Extender<IExtensibleMassActions>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public DocumentListMassActionsExtender()
            : base("DocumentListMassActions")
        {
        }


        /// <summary>
        /// Adds two mass actions to the MassAction control.
        /// </summary>
        /// <param name="instance">The MassActions control to add persona related actions to</param>
        protected override void Initialize(IExtensibleMassActions instance)
        {
            if (!LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.Personas) ||
                !ResourceSiteInfoProvider.IsResourceOnSite(ModuleName.PERSONAS, SiteContext.CurrentSiteName))
            {
                return;
            }

            instance.AddMassActions(
                new MassActionItem
                {
                    CodeName = "TagDocumentsWithPersonas",
                    DisplayNameResourceString = "personas.massaction.tag",
                    ActionType = MassActionTypeEnum.OpenModal,
                    CreateUrl = (scope, selectedItems, additionalParameters) =>
                        BuildDialogUrl(MultipleDocumentsActionTypeEnum.Tag,
                            scope,
                            selectedItems,
                            (DocumentListMassActionsParameters)additionalParameters),
                },
                new MassActionItem
                {
                    CodeName = "UntagPersonasFromDocuments",
                    DisplayNameResourceString = "personas.massaction.untag",
                    ActionType = MassActionTypeEnum.OpenModal,
                    CreateUrl = (scope, selectedItems, additionalParameters) =>
                        BuildDialogUrl(MultipleDocumentsActionTypeEnum.Untag,
                            scope,
                            selectedItems,
                            (DocumentListMassActionsParameters)additionalParameters),
                });
        }


        /// <summary>
        /// Creates URL of the dialog which allows user to choose which personas should be assigned to or 
        /// removed from documents. Stores additional parameters to the
        /// session (via WindowHelper) as well.
        /// </summary>
        /// <param name="massActionType">Tag/Untag documents</param>
        /// <param name="massActionScope">All documents according to filter values or only selected ones</param>
        /// <param name="selectedNodeIDs">Specifies NodeIDs which should be tagged/untagged if only selected 
        /// documents are chosen in <paramref name="massActionScope"/> parameter</param>
        /// <param name="additionalParameters">Additional parameters allowing to filter documents when 'All 
        /// document' scope is selected</param>
        /// <exception cref="ArgumentNullException"><paramref name="additionalParameters"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="massActionScope"/> is set to 'selected documents' 
        /// and <paramref name="selectedNodeIDs"/> is null or empty</exception>
        /// <returns>URL of the dialog which should be opened to finish action</returns>
        private string BuildDialogUrl(MultipleDocumentsActionTypeEnum massActionType,
                                      MassActionScopeEnum massActionScope,
                                      List<int> selectedNodeIDs,
                                      DocumentListMassActionsParameters additionalParameters)
        {
            if (additionalParameters == null)
            {
                throw new ArgumentNullException("additionalParameters");
            }
            if (massActionScope == MassActionScopeEnum.SelectedItems)
            {
                if ((selectedNodeIDs == null) || !selectedNodeIDs.Any())
                {
                    throw new ArgumentException("[DocumentListMassActionsExtender.BuildDialogUrl]: selectedNodeIDs cannot be null or empty when action should be performed only on selected documents");
                }
            }

            string identifier = StoreParametersToWindowHelper(massActionType,
                massActionScope,
                selectedNodeIDs,
                additionalParameters);

            string url = URLHelper.ResolveUrl("~/CMSModules/Personas/Dialogs/DocumentsMassTagging.aspx");

            url = URLHelper.AddParameterToUrl(url, "params", identifier);
            url = URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(url));

            return url;
        }


        private static string StoreParametersToWindowHelper(MultipleDocumentsActionTypeEnum massActionType,
                                                            MassActionScopeEnum massActionScope,
                                                            List<int> selectedNodeIDs,
                                                            DocumentListMassActionsParameters additionalParameters)
        {
            var parameters = ConstructDialogParameters(massActionType,
                massActionScope,
                selectedNodeIDs,
                additionalParameters);

            string identifier = Guid.NewGuid().ToString();

            WindowHelper.Add(identifier, parameters);
            return identifier;
        }


        private static Hashtable ConstructDialogParameters(MultipleDocumentsActionTypeEnum massActionType,
                                                           MassActionScopeEnum massActionScope,
                                                           List<int> selectedNodeIDs,
                                                           DocumentListMassActionsParameters additionalParameters)
        {
            var parameters = new Hashtable();

            parameters["PersonasMassAction.MassActionType"] = massActionType;
            parameters["PersonasMassAction.MassActionScope"] = massActionScope;
            if (massActionScope == MassActionScopeEnum.SelectedItems)
            {
                parameters["PersonasMassAction.SelectedNodeIDs"] = selectedNodeIDs;
            }
            else
            {
                // Parameters what should be considered as 'all documents'. Those parameters' 
                // purpose is to be able to filter document to the ones which are displayed on the list.
                parameters["PersonasMassAction.CurrentWhereCondition"] = additionalParameters.CurrentWhereCondition;
                parameters["PersonasMassAction.NodeAliasPath"] = additionalParameters.Node.NodeAliasPath;
                parameters["PersonasMassAction.NodeLevel"] = additionalParameters.Node.NodeLevel;
                parameters["PersonasMassAction.ClassID"] = additionalParameters.ClassID;
                parameters["PersonasMassAction.AllLevels"] = additionalParameters.ShowAllLevels;
            }

            // SelectionDialog parameters
            parameters["IsMassAction"] = true;
            parameters["SiteId"] = SiteContext.CurrentSiteID;
            parameters["ObjectType"] = "personas.persona";
            parameters["AllowNone"] = true;
            parameters["ReturnColumnName"] = "PersonaID";
            parameters["SelectionMode"] = SelectionModeEnum.Multiple;

            return parameters;
        }
    }
}