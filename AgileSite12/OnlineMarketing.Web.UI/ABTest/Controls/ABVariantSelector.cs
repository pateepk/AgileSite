using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;
using CMS.UIControls;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// A multi button that allows creating new A/B variants and switching between them.
    /// </summary>
    internal class ABVariantSelector : CMSWebControl, ICallbackEventHandler
    {
        private const string ADD_NEW_VARIANT_ACTION_NAME = "##addnewvariant##";
        private const string CHANGE_VARIANT_ACTION_NAME = "##changevariant##";
        private const string RENAME_VARIANT_ACTION_NAME = "##renamevariant##";
        private const string REMOVE_VARIANT_ACTION_NAME = "##removevariant##";

        // Contains data to be sent back to client after successful callback action
        private readonly CallbackResult callbackResult = new CallbackResult();

        // Contains a GUID of a variant retrieved from the cookie
        private readonly Guid? variantGuid;

        // Child controls
        private readonly PopUpWindow abTestPopupContainer = new PopUpWindow();
        private readonly ABTestVariantListing popUpListing = new ABTestVariantListing();
        private readonly CMSButton variantSelectorButton = new CMSButton
        {
            Text = ResHelper.GetString("abtesting.createabvariant"),
            ButtonStyle = ButtonStyle.Default
        };
        private readonly SubmitButton addVariantButton = new SubmitButton()
        {
            Text = ResHelper.GetString("abtesting.addnewvariant"),
            IsPrimaryButton = true
        };

        private readonly string testUnderWorkflowTooltip = ResHelper.GetString("abtest.popupdialog.cannotmodifyabtest");
        private readonly string permissionsRequiredTooltip = ResHelper.GetString("abtesting.permission.manage.tooltip");
        private readonly Guid addNewVariantButtonId = Guid.NewGuid();
        private readonly Guid manageABTestButtonId = Guid.NewGuid();
        private readonly Guid promoteWinnerInfoMessageId = Guid.NewGuid();

        private readonly IABTestManager testManager = Service.Resolve<IABTestManager>();
        private readonly bool isPreviewMode;


        /// <summary>
        /// Manager responsible for saving current page.
        /// </summary>
        private ICMSDocumentManager DocumentManager => ((CMSPage)Page).DocumentManager;


        /// <summary>
        /// A/B test for current page.
        /// </summary>
        private ABTestInfo CurrentABTest { get; }


        /// <summary>
        /// Gets the variant for currently edited page.
        /// </summary>
        private IABTestVariant CurrentVariant => ABTestVariants.GetVariant(variantGuid) ?? ABTestVariants.Original;


        /// <summary>
        /// Contains A/B test variants for current node in order in which they are to be displayed.
        /// </summary>
        private VariantsCollection ABTestVariants { get; } = new VariantsCollection();


        /// <summary>
        /// Indicates whether user has 'Read' permission for A/B test module.
        /// </summary>
        private bool UserHasReadPermission { get; } = MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(ModuleName.ABTEST, "Read");


        private bool UserHasABTestManagePermission { get; } = MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(ModuleName.ABTEST, "Manage");


        private bool UserHasContentModifyPermission { get; } = MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(ModuleName.CONTENT, "Modify");


        /// <summary>
        /// Indicates whether user has 'Manage' permission for A/B test module and 'Modify' for Content module.
        /// </summary>
        private bool UserHasPermissions
        {
            get
            {
                return UserHasABTestManagePermission && UserHasContentModifyPermission;
            }
        }


        /// <summary>
        /// Indicates whether user is authorized to interact with the selector.
        /// </summary>
        private bool IsCurrentUserAuthorized { get; }


        /// <summary>
        /// Creates a new instance of <see cref="ABVariantSelector"/> for the current page.
        /// </summary>
        /// <exception cref="InvalidOperationException">If it is not possible to recognize the currently edited page.</exception>
        public ABVariantSelector(bool isPreviewMode)
        {
            if (DocumentContext.EditedDocument == null || DocumentContext.EditedDocument.DocumentID == 0)
            {
                throw new InvalidOperationException("Edited page is unknown. Make sure the control is used in Pages application on Content Only site.");
            }

            this.isPreviewMode = isPreviewMode;

            IsCurrentUserAuthorized = IsUserAuthorized();
            if (!IsCurrentUserAuthorized)
            {
                return;
            }

            variantGuid = ABTestUIVariantHelper.GetPersistentVariantIdentifier(DocumentContext.EditedDocument.DocumentID);

            CurrentABTest = testManager.GetABTestWithoutWinner(DocumentContext.EditedDocument);
            if (CurrentABTest == null)
            {
                ABTestUIVariantHelper.RemovePersistentVariantIdentifier(DocumentContext.EditedDocument.DocumentID);
            }
        }


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            LoadABTestVariants();

            Visible = IsCurrentUserAuthorized;

            // If preview mode and there are no variants to switch between, then hide the control
            if (isPreviewMode && ABTestVariants.Count == 0)
            {
                Visible = false;
            }

            if (!Visible)
            {
                // Clear persisted cookie, so that original is displayed to user
                ABTestUIVariantHelper.RemovePersistentVariantIdentifier(DocumentContext.EditedDocument.DocumentID);
                return;
            }

            AddVariantSelectorButton();

            AddPopUpContainer();

            AddHiddenFieldToStoreCurrentVariant();
        }


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!Visible)
            {
                return;
            }

            var message = TempData.GetMessage(DocumentContext.EditedDocument.DocumentID);
            if (!String.IsNullOrEmpty(message))
            {
                ((CMSPage)Page).ShowConfirmation(message);
            }

            ShowPromoteWinnerInfoMessage();

            InitializeVariantSelectorButton();

            InitializePopUpContainer();

            InitializeClientScripts();
        }

        /// <summary>
        /// Shows info message to prompt the user to select the winning A/B variant after the A/B test has finished.
        /// </summary>
        private void ShowPromoteWinnerInfoMessage()
        {
            if (CurrentABTest != null
                && ABTestStatusEvaluator.GetStatus(CurrentABTest) == ABTestStatusEnum.Finished
                && !isPreviewMode
                && CurrentABTest.ABTestWinnerGUID == Guid.Empty
                && UserHasPermissions)
            {
                string testFinishedMessage = DocumentManager.AllowSave
                    ? ResHelper.GetStringFormat("abtesting.pages.finishedtest.promotewinnerpromptmessage", promoteWinnerInfoMessageId)
                    : ResHelper.GetString("abtesting.pages.finishedtest.promotewinnerpromptmessage.warning");

                ((CMSPage)Page).ShowInformation(testFinishedMessage);
            }
        }


        private bool IsUserAuthorized()
        {
            return ModuleEntryManager.IsModuleLoaded(ModuleName.ONLINEMARKETING)
                                   && SiteContext.CurrentSite.SiteIsContentOnly
                                   && LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.ABTesting)
                                   && ABTestInfoProvider.ABTestingEnabled(SiteContext.CurrentSiteName)
                                   && ResourceSiteInfoProvider.IsResourceOnSite(ModuleName.ABTEST, SiteContext.CurrentSiteName)
                                   && UserHasReadPermission
                                   && !DocumentContext.EditedDocument.IsFolder();
        }


        /// <summary>
        /// Creates and adds hidden field to a page to persist variant identifier to be used for saving Page builder's configuration.
        /// </summary>
        /// <seealso cref="IABTestVariantIdentifierProvider.GetVariantIdentifier"/>
        private void AddHiddenFieldToStoreCurrentVariant()
        {
            if (CurrentABTest == null || CurrentVariant == null)
            {
                return;
            }

            var hdnCurrentVariantIdentifier = new HtmlGenericControl("input");
            hdnCurrentVariantIdentifier.Attributes.Add("type", "hidden");
            hdnCurrentVariantIdentifier.Attributes.Add("name", ABTestVariantIdentifierProvider.VARIANT_IDENTIFIER_FORM_FIELD_NAME);
            hdnCurrentVariantIdentifier.Attributes.Add("value", CurrentVariant.Guid.ToString());

            Controls.Add(hdnCurrentVariantIdentifier);
        }


        private void AddVariantSelectorButton()
        {
            this.AddCssClass("ab-test-variant-selector-wrapper btn-dropdown");

            Controls.Add(variantSelectorButton);
        }


        private void InitializeVariantSelectorButton()
        {
            if (ABTestVariants.Count > 0)
            {
                variantSelectorButton.Text = CurrentVariant.Name + CMSButton.ICON_PLACEMENT_MACRO;
                variantSelectorButton.IconCssClass = "caret";
            }
            else
            {
                if (!isPreviewMode)
                {
                    if (!UserHasPermissions)
                    {
                        variantSelectorButton.ToolTip = permissionsRequiredTooltip;
                        variantSelectorButton.Enabled = false;
                    }
                    else if (!DocumentManager.AllowSave)
                    {
                        variantSelectorButton.ToolTip = testUnderWorkflowTooltip;
                        variantSelectorButton.Enabled = false;
                    }
                    else
                    {
                        variantSelectorButton.ToolTip = ResHelper.GetString("abtesting.createabvariant.tooltip");
                        variantSelectorButton.Enabled = true;
                    }
                }
            }
        }


        /// <summary>
        /// Orders variants so that the original variant is the last in the list all the other variants are ordered alphabetically.
        /// </summary>
        private IOrderedEnumerable<IABTestVariant> GetOrderedVariants()
        {
            return ABTestVariants.Select(v => v.Value).OrderBy(x => x.IsOriginal).ThenBy(x => x.Name);
        }


        private void InitializeClientScripts()
        {
            ScriptHelper.RegisterModule(this, "CMS.OnlineMarketing/ABVariantSelector", new
            {
                buttonModeOnly = ABTestVariants.Count == 0,

                variantSelectorButtonId = variantSelectorButton.ClientID,
                abTestPopupContainerId = abTestPopupContainer.ClientID,
                abTestVariantListingId = popUpListing.ClientID,
                addNewVariantButtonId = addNewVariantButtonId,

                manageABTestButtonId = manageABTestButtonId,
                manageABTestUrl = GetEditABTestUrl(),

                promoteWinnerInfoMessageId = promoteWinnerInfoMessageId,

                currentVariantGuid = CurrentVariant?.Guid,

                addNewVariantAction = ADD_NEW_VARIANT_ACTION_NAME,
                changeVariantAction = CHANGE_VARIANT_ACTION_NAME,
                renameVariantAction = RENAME_VARIANT_ACTION_NAME,
                removeVariantAction = REMOVE_VARIANT_ACTION_NAME
            });

            string cbReference = Page.ClientScript.GetCallbackEventReference(this, "arg", "variantSelectorReceiveResult", "context", "variantSelectorError", true);
            string callbackScript = "function variantSelectorCallback(arg, context)" + "{ " + cbReference + ";};";
            string clientFunction = @"function variantSelectorReceiveResult(callbackResult, context) {
                window.ABVariantSelectorModule.abVariantSelectorCallbackReceived(callbackResult);
                window.RefreshTree();
            };

            function variantSelectorError(errorMessage) {
                alert('" + ResHelper.GetString("abtesting.abvariantselectorerror") + @"');
                window.SelectNode('" + DocumentContext.EditedDocument.NodeID + @"');
                return false;
            };";

            ScriptHelper.RegisterStartupScript(this, typeof(string), ClientID, ScriptHelper.GetScript(callbackScript + clientFunction));
        }


        private string GetEditABTestUrl()
        {
            if (CurrentABTest == null)
            {
                return String.Empty;
            }

            string manageABTestUrl = UIContextHelper.GetElementUrl(ModuleName.ABTEST, "Detail", true, CurrentABTest.ABTestID);

            manageABTestUrl = URLHelper.AddParameterToUrl(manageABTestUrl, "dialog", "1");
            manageABTestUrl = ApplicationUrlHelper.AppendDialogHash(manageABTestUrl);

            return manageABTestUrl;
        }


        private void AddPopUpContainer()
        {
            abTestPopupContainer.Visible = true;
            abTestPopupContainer.HeaderText = ResHelper.GetString("abtest.popupdialog.header");
            abTestPopupContainer.ContentHeaderText = ResHelper.GetString("abtest.popupdialog.contentheader");
            abTestPopupContainer.ColorTheme = PopUpWindowColorTheme.Section;

            abTestPopupContainer.AddCssClass("ab-test-pop-up-dialog");
            abTestPopupContainer.Position = PopUpWindowPosition.Left;
            abTestPopupContainer.DisplayTriangle = true;

            if (!isPreviewMode)
            {
                if (CurrentABTest != null)
                {
                    var manageButton = new SubmitButton
                    {
                        Text = ResHelper.GetString("abtesting.manageabtest"),
                        Identifier = manageABTestButtonId,
                        IsPrimaryButton = false
                    };

                    abTestPopupContainer.FooterControls.Add(manageButton);
                }

                addVariantButton.Identifier = addNewVariantButtonId;

                abTestPopupContainer.FooterControls.Add(addVariantButton);
            }

            popUpListing.SelectItemCallback = "window.ABVariantSelectorModule.variantChosen";
            popUpListing.EditItemCallback = "window.ABVariantSelectorModule.renameVariant";
            popUpListing.RemoveItemCallback = "window.ABVariantSelectorModule.removeVariant";
            popUpListing.MaximumNameLength = ABTestManager.MAXIMUM_VARIANT_NAME_LENGTH;
            if (!UserHasPermissions)
            {
                popUpListing.ActionDisabledTitle = permissionsRequiredTooltip;
            }
            popUpListing.SelectedItemIdentifier = CurrentVariant == null ? String.Empty : CurrentVariant.Guid.ToString();

            abTestPopupContainer.ContentControl = popUpListing;

            Controls.Add(abTestPopupContainer);
        }


        private void InitializePopUpContainer()
        {
            if (!isPreviewMode)
            {
                addVariantButton.Disabled = !UserHasPermissions || !DocumentManager.AllowSave;
                addVariantButton.Tooltip = !UserHasPermissions ? permissionsRequiredTooltip :
                    DocumentManager.AllowSave ? String.Empty : testUnderWorkflowTooltip;
            }

            popUpListing.ListItems = GetOrderedVariants().Select(v =>
            {
                var editActionState = isPreviewMode ? ABTestVariantListItemActionState.Hidden :
                    DocumentManager.AllowSave && UserHasPermissions ? ABTestVariantListItemActionState.Enabled : ABTestVariantListItemActionState.Disabled;

                var removeActionState = v.IsOriginal || isPreviewMode ? ABTestVariantListItemActionState.Hidden :
                    DocumentManager.AllowSave && UserHasPermissions ? ABTestVariantListItemActionState.Enabled : ABTestVariantListItemActionState.Disabled;

                return new ABTestVariantListItem
                {
                    Key = v.Guid.ToString(),
                    Name = v.Name,
                    EditActionState = editActionState,
                    RemoveActionState = removeActionState
                };
            }).ToList();
        }


        /// <summary>
        /// Handles a callback event from the client.
        /// </summary>
        public void RaiseCallbackEvent(string eventArgument)
        {
            try
            {
                var parameters = JsonConvert.DeserializeObject<CallbackParameters>(eventArgument);
                if (parameters == null || String.IsNullOrEmpty(parameters.ActionName))
                {
                    throw new InvalidOperationException("No parameters passed.");
                }

                switch (parameters.ActionName)
                {
                    case ADD_NEW_VARIANT_ACTION_NAME:
                        {
                            AddNewVariant();
                            return;
                        }
                    case CHANGE_VARIANT_ACTION_NAME:
                        {
                            ChangeVariant(parameters);
                            return;
                        }
                    case RENAME_VARIANT_ACTION_NAME:
                        {
                            RenameVariant(parameters);
                            return;
                        }
                    case REMOVE_VARIANT_ACTION_NAME:
                        {
                            RemoveVariant(parameters);
                            return;
                        }
                    default:
                        {
                            throw new InvalidOperationException($"Unknown action '{parameters.ActionName}' passed.");
                        }
                }
            }
            catch (Exception e)
            {
                ABTestUIVariantHelper.RemovePersistentVariantIdentifier(DocumentContext.EditedDocument.DocumentID);
                EventLogProvider.LogException("ABVariantSelector", "CALLBACKERROR", e, additionalMessage: $"Error happened for A/B test in the page with id '{DocumentContext.EditedDocument.DocumentID}'.");
                throw;
            }
        }


        /// <summary>
        /// Returns a result to the client after processing the client's callback.
        /// </summary>
        public string GetCallbackResult()
        {
            return JsonConvert.SerializeObject(callbackResult, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }


        private void AddNewVariant()
        {
            VerifyPermissions();

            var abTestExists = CurrentABTest != null;
            if (!abTestExists)
            {
                TempData.StoreMessage(DocumentContext.EditedDocument.DocumentID, ResHelper.GetString("abtesting.newabtestwithvariantscreated"));
                testManager.CreateABTest(DocumentContext.EditedDocument);
            }

            var newVariant = testManager.AddVariant(DocumentContext.EditedDocument, CurrentVariant?.Guid);
            SaveDocument();

            ABTestUIVariantHelper.SetPersistentVariantIdentifier(DocumentContext.EditedDocument.DocumentID, newVariant.Guid);

            if (abTestExists)
            {
                var message = String.Format(ResHelper.GetString("abtesting.newabvariantcreated"), newVariant.Name);
                TempData.StoreMessage(DocumentContext.EditedDocument.DocumentID, message);
            }
            callbackResult.NodeId = DocumentContext.EditedDocument.NodeID;
        }


        private void ChangeVariant(CallbackParameters parameters)
        {
            if (!UserHasReadPermission)
            {
                throw new PermissionCheckException(ModuleName.ABTEST, "Read", SiteContext.CurrentSiteName);
            }

            if (parameters.VariantIdentifier == null)
            {
                throw new InvalidOperationException("Variant identifier is not a GUID.");
            }

            if (CurrentABTest == null)
            {
                throw new InvalidOperationException($"A/B test is not created for a page with id '{DocumentContext.EditedDocument.DocumentID}'.");
            }

            callbackResult.NodeId = DocumentContext.EditedDocument.NodeID;
            ABTestUIVariantHelper.SetPersistentVariantIdentifier(DocumentContext.EditedDocument.DocumentID, parameters.VariantIdentifier.Value);
        }


        private void RenameVariant(CallbackParameters parameters)
        {
            VerifyPermissions();

            if (parameters.VariantIdentifier == null)
            {
                throw new InvalidOperationException("Variant identifier is not a GUID.");
            }

            if (String.IsNullOrEmpty(parameters.Name))
            {
                throw new InvalidOperationException("New display name for the variant cannot be null or empty.");
            }

            if (CurrentABTest == null)
            {
                throw new InvalidOperationException($"A/B test is not created for a page with id '{DocumentContext.EditedDocument.DocumentID}'.");
            }

            var refreshNeeded = IsRefreshNeeded();

            testManager.RenameVariant(DocumentContext.EditedDocument, parameters.VariantIdentifier.Value, parameters.Name);

            SaveDocument();

            if (refreshNeeded)
            {
                callbackResult.NodeId = DocumentContext.EditedDocument.NodeID;
            }
        }


        private void RemoveVariant(CallbackParameters parameters)
        {
            VerifyPermissions();

            if (parameters.VariantIdentifier == null)
            {
                throw new InvalidOperationException("Variant identifier is missing.");
            }

            if (CurrentABTest == null)
            {
                throw new InvalidOperationException($"A/B test is not created for a page with id '{DocumentContext.EditedDocument.DocumentID}'.");
            }

            var deleteSelectedVariant = CurrentVariant?.Guid == parameters.VariantIdentifier;

            testManager.RemoveVariant(DocumentContext.EditedDocument, parameters.VariantIdentifier.Value);

            var message = String.Format(ResHelper.GetString("abtesting.abvariantremoved"), ABTestVariants.GetVariant(parameters.VariantIdentifier).Name);

            var refreshNeeded = IsRefreshNeeded();

            SaveDocument();

            if (refreshNeeded || deleteSelectedVariant)
            {
                if (deleteSelectedVariant)
                {
                    ABTestUIVariantHelper.RemovePersistentVariantIdentifier(DocumentContext.EditedDocument.DocumentID);
                }
                callbackResult.NodeId = DocumentContext.EditedDocument.NodeID;
                TempData.StoreMessage(DocumentContext.EditedDocument.DocumentID, message);
            }
            else
            {
                callbackResult.SuccessMessage = message;
            }
        }


        /// <summary>
        /// Verifies that current user has 'Manage' permission for A/B test module and 'Modify' permission for Content module.
        /// Throws <see cref="PermissionCheckException"/> if user lacks the permissions.
        /// </summary>
        private void VerifyPermissions()
        {
            if (!UserHasABTestManagePermission)
            {
                throw new PermissionCheckException(ModuleName.ABTEST, "Manage", SiteContext.CurrentSiteName);
            }

            if (!UserHasContentModifyPermission)
            {
                throw new PermissionCheckException(ModuleName.CONTENT, "Modify", SiteContext.CurrentSiteName);
            }
        }


        /// <summary>
        /// Saves the edited document.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when document cannot be saved.</exception>
        private void SaveDocument()
        {
            VerifyPermissions();

            if (!DocumentManager.AllowSave)
            {
                throw new InvalidOperationException("Page cannot be saved.");
            }

            DocumentManager.SaveDocument();
        }


        /// <summary>
        /// Loads existing A/B test variants into class property.
        /// Should be called only once in page lifecycle from <see cref="OnInit"/> method.
        /// </summary>
        private void LoadABTestVariants()
        {
            if (CurrentABTest == null)
            {
                return;
            }

            ABTestVariants.AddRange(testManager.GetVariants(DocumentContext.EditedDocument));
            if (CurrentVariant != null)
            {
                ABTestUIVariantHelper.SetPersistentVariantIdentifier(DocumentContext.EditedDocument.DocumentID, CurrentVariant.Guid);
            }
        }


        private bool IsRefreshNeeded()
        {
            if (DocumentManager.AutoCheck)
            {
                var step = DocumentManager.Step;
                if (step != null && (step.StepIsArchived || step.StepIsPublished))
                {
                    return true;
                }
            }

            return false;
        }


        private class CallbackResult
        {
            /// <summary>
            /// Success message to be displayed after successfully processing the callback action.
            /// Keep null or empty not to display any success message.
            /// </summary>
            public string SuccessMessage { get; set; }


            /// <summary>
            /// Gets or sets id of the node which should be selected after refreshing all the iframes in the Pages application.
            /// If equals to zero then no refresh is performed.
            /// </summary>
            public int NodeId { get; set; }
        }


        private class CallbackParameters
        {
            /// <summary>
            /// Name of the action that should be raised when callback is received.
            /// </summary>
            public string ActionName { get; set; }


            /// <summary>
            /// Success message to be displayed after successfuly processing the callback action.
            /// Keep null or empty to not display any success message.
            /// </summary>
            public Guid? VariantIdentifier { get; set; }


            /// <summary>
            /// Returns new variant name.
            /// </summary>
            public string Name { get; set; }
        }


        private class VariantsCollection : IEnumerable<KeyValuePair<Guid, IABTestVariant>>
        {
            private readonly Dictionary<Guid, IABTestVariant> dictionary;


            /// <summary>
            /// Gets the variant representing original page.
            /// Returns null if no variants are defined for current page.
            /// </summary>
            public IABTestVariant Original { get; private set; }


            /// <summary>
            /// Gets the number of variants.
            /// </summary>
            public int Count => dictionary.Count;


            /// <summary>
            /// Initializes a new instance of <see cref="VariantsCollection"/>.
            /// </summary>
            public VariantsCollection(IEnumerable<IABTestVariant> variants = null)
            {
                dictionary = new Dictionary<Guid, IABTestVariant>();
                AddRange(variants);
            }


            /// <summary>
            /// Returns a variant for given <paramref name="key"/>.
            /// If <paramref name="key"/> not found then return null.
            /// </summary>
            public IABTestVariant GetVariant(Guid? key)
            {
                return dictionary.TryGetValue(key.GetValueOrDefault(), out IABTestVariant variant) ? variant : null;
            }



            /// <summary>
            /// Adds variants to the collection.
            /// </summary>
            public void AddRange(IEnumerable<IABTestVariant> variants)
            {
                foreach (var variant in variants ?? Enumerable.Empty<IABTestVariant>())
                {
                    if (variant.IsOriginal)
                    {
                        Original = variant;
                    }

                    dictionary.Add(variant.Guid, variant);
                }
            }


            public IEnumerator<KeyValuePair<Guid, IABTestVariant>> GetEnumerator()
            {
                return dictionary.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }



        /// <summary>
        /// A temporary data storage to store data required in a subsequent request.
        /// </summary>
        private static class TempData
        {
            private const string MESSAGE_KEY = nameof(ABVariantSelector) + "_message";


            /// <summary>
            /// Stores a message to be displayed in a subsequent request.
            /// </summary>
            /// <param name="pageId">Identifier for the page in which the message is to be displayed.</param>
            /// <param name="message">The message to be displayed.</param>
            public static void StoreMessage(int pageId, string message)
            {
                SessionHelper.SetValue($"{MESSAGE_KEY}_{pageId}", message);
            }



            /// <summary>
            /// Gets a stored message for given <paramref name="pageId"/>.
            /// </summary>
            /// <param name="pageId">Identifier for the page in which the message is to be displayed.</param>
            public static string GetMessage(int pageId)
            {
                var value = SessionHelper.GetValue($"{MESSAGE_KEY}_{pageId}");
                SessionHelper.Remove($"{MESSAGE_KEY}_{pageId}");

                return value as string;
            }
        }
    }
}
