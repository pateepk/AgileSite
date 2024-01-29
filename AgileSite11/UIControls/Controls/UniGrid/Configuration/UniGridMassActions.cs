using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml;
using System.Linq;
using System.Web.UI;
using System.Xml.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;

namespace CMS.UIControls.UniGridConfig
{
    /// <summary>
    /// Represents <see cref="UniGrid"/> mass action's definitions.
    /// </summary>
    [ParseChildren(typeof(MassAction), DefaultProperty = "MassActionDefinitions", ChildrenAsProperties = true)]
    public class UniGridMassActions : AbstractConfiguration
    {
        private readonly List<MassActionItem> massActions;


        /// <summary>
        /// Mass actions rendered by <see cref="UniGrid"/>.
        /// </summary>
        internal IEnumerable<MassActionItem> MassActions
        {
            get
            {
                return massActions.AsReadOnly();
            }
        }


        /// <summary>
        /// Mass actions defined by user in markup or XML.
        /// </summary>
        public Collection<MassAction> MassActionDefinitions
        {
            get;

            // Setter must be private in order to prevent .NET Framework overwriting this property 
            // when creating the control from markup.
            private set;
        }


        /// <summary>
        /// Creates a new instance of <see cref="UniGridMassActions"/> with no 
        /// <see cref="MassActionDefinitions"/> and <see cref="MassActions"/>.
        /// </summary>
        public UniGridMassActions()
        {
            var collection = new ObservableCollection<MassAction>();
            collection.CollectionChanged += OnMassActionDefinitionChanged;
            MassActionDefinitions = collection;

            massActions = new List<MassActionItem>();
        }


        /// <summary>
        /// Creates a new instance of <see cref="UniGridMassActions"/> 
        /// with <see cref="MassActionDefinitions"/> from <paramref name="element"/>,
        /// but no <see cref="MassActions"/>.
        /// </summary>
        /// <param name="element">Mass actions XML element</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="element"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when "behavior" attribute of any "massaction" element provide in <paramref name="element"/> is not valid <see cref="MassActionTypeEnum"/> value.</exception>
        public UniGridMassActions(XElement element)
            : this()
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            // Load the child nodes
            IEnumerable<MassAction> massActionContent = element.Descendants()
                .Where(e => e.NodeType == XmlNodeType.Element)
                .Select(childElement =>
                {
                    string actionName = childElement.GetAttributeStringValue("name");
                    string caption = childElement.GetAttributeStringValue("caption");

                    string actionTypeName = childElement.GetAttributeValue("behavior", String.Empty);

                    return new MassAction
                    {
                        Name = actionName,
                        Behavior = GetActionType(actionTypeName),
                        Caption = caption,
                        Url = childElement.GetAttributeStringValue("url")
                    };
                });

            foreach (var massAction in massActionContent)
            {
                MassActionDefinitions.Add(massAction);
            }
        }


        /// <summary>
        /// Synchronizes <see cref="MassActionDefinitions"/> with <see cref="MassActions"/>.
        /// </summary>
        /// <remarks>Reacts only on first item in the collection.</remarks>
        private void OnMassActionDefinitionChanged(object sender, NotifyCollectionChangedEventArgs collectionChangedtArgs)
        {
            switch (collectionChangedtArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newItem = collectionChangedtArgs.NewItems.Cast<MassAction>().Select(GetMassActionItem).First();
                    massActions.Add(newItem);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    massActions.RemoveAt(collectionChangedtArgs.OldStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    massActions.Clear();
                    break;

                case NotifyCollectionChangedAction.Move:
                    var movedAction = massActions[collectionChangedtArgs.OldStartingIndex];
                    massActions.RemoveAt(collectionChangedtArgs.OldStartingIndex);
                    massActions.Insert(collectionChangedtArgs.NewStartingIndex, movedAction);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    var newAction = collectionChangedtArgs.NewItems.Cast<MassAction>().First();
                    massActions[collectionChangedtArgs.OldStartingIndex] = GetMassActionItem(newAction);
                    break;

                default:
                    ThrowUnsupportedCollectionAction(collectionChangedtArgs.Action);
                    return;
            }
        }


        /// <summary>
        /// Throws exception when not supported action occurs.
        /// </summary>
        private static void ThrowUnsupportedCollectionAction(NotifyCollectionChangedAction action)
        {
            var supportedActions = new
            {
                NotifyCollectionChangedAction.Add,
                NotifyCollectionChangedAction.Remove,
                NotifyCollectionChangedAction.Reset,
                NotifyCollectionChangedAction.Replace,
                NotifyCollectionChangedAction.Move
            };

            throw new NotSupportedException(string.Format("Action '{0}' is not supported. Supported actions are: {1}",
                action,
                string.Join(", ", supportedActions)));
        }


        /// <summary>
        /// Converts <see cref="MassAction"/> to <see cref="MassActionItem"/>. 
        /// </summary>
        private MassActionItem GetMassActionItem(MassAction actionDefinition)
        {
            return new MassActionItem
            {
                DisplayNameResourceString = LocalizationHelper.GetResourceName(actionDefinition.Caption),
                CodeName = actionDefinition.Name,
                ActionType = actionDefinition.Behavior,
                CreateUrl = GetUrlDelegate(actionDefinition.Url, actionDefinition.Name, actionDefinition.Behavior)
            };
        }


        /// <summary>
        /// Returns <see cref="CreateUrlDelegate"/> that generates URL when user submits mass action with provided <paramref name="actionName"/>.
        /// </summary>
        /// <param name="redirectUrl">URL specified by user.</param>
        /// <param name="actionName">Name of custom or default mass action.</param>
        /// <param name="behavior">Type of interaction when performing the mass action.</param>
        private CreateUrlDelegate GetUrlDelegate(string redirectUrl, string actionName, MassActionTypeEnum behavior)
        {
            redirectUrl = GetDefaultRedirectUrlByActionName(actionName) ?? redirectUrl;
            if (String.IsNullOrEmpty(redirectUrl))
            {
                throw new NotSupportedException("Combination of URL and name is not supported. Provide returnUrl or supported actionName.");
            }

            return (scope, selectedIDs, parameters) => GetTargetUrl(redirectUrl, behavior, scope, selectedIDs, parameters as MassActionParameters);
        }


        /// <summary>
        /// Returns URL for predefined mass actions, <c>null</c> otherwise.
        /// </summary>
        private static string GetDefaultRedirectUrlByActionName(string actionName)
        {
            switch (actionName)
            {
                case "#delete":
                    return "~/CMSAdminControls/UI/UniGrid/Pages/MassDelete.aspx";

                default:
                    return null;
            }
        }

        /// <summary>
        /// Stores mass action parameters in session and returns mass action target URL.
        /// </summary>
        /// <param name="redirectUrl">Target mass action URL without parameters.</param>
        /// <param name="generalParameters">General parameters from UniGrid.</param>
        /// <param name="behavior">Type of interaction when performing the mass action.</param>
        /// <param name="scope">Determines whether action should be performed only on selected items or on all items which satisfies filter condition.</param>
        /// <param name="selectedIDs">Info identifiers selected by user.</param>
        private string GetTargetUrl(string redirectUrl, MassActionTypeEnum behavior, MassActionScopeEnum scope, IEnumerable<int> selectedIDs, MassActionParameters generalParameters)
        {
            if (generalParameters == null)
            {
                throw new ArgumentException("Given parameters has to be of type MassActionParameters.", "generalParameters");
            }

            string key = StoreCompleteParameters(generalParameters, behavior, scope, selectedIDs);
            return GetRedirectUrlWithParameters(redirectUrl, key);
        }


        /// <summary>
        /// Returns key to the new instance of <see cref="MassActionParameters"/> that is stored in <see cref="WindowHelper"/>.
        /// The stored instance contains both <paramref name="generalParameters"/> and selected identifiers of infos.
        /// </summary>
        /// <remarks>Method is internal only for test purposes.</remarks>
        internal static string StoreCompleteParameters(MassActionParameters generalParameters, MassActionTypeEnum behavior, MassActionScopeEnum actionScope, IEnumerable<int> selectedIDs)
        {
            if (actionScope == MassActionScopeEnum.AllItems)
            {
                selectedIDs = GetAllIDs(generalParameters);
            }

            var parameters = new MassActionParameters(selectedIDs, behavior, generalParameters);
            string key = Guid.NewGuid().ToString();

            // pass URL and selected items through session
            WindowHelper.Add(key, parameters);
            return key;
        }


        /// <summary>
        /// Returns mass action target URL with <paramref name="key"/> to the <see cref="MassActionParameters"/> stored in the session.
        /// </summary>
        /// <remarks>Method is internal only for test purposes.</remarks>
        internal string GetRedirectUrlWithParameters(string redirectUrl, string key)
        {
            redirectUrl = URLHelper.AddParameterToUrl(redirectUrl, "parameters", key);
            redirectUrl = URLHelper.AddParameterToUrl(redirectUrl, "hash", QueryHelper.GetHash(URLHelper.GetQuery(redirectUrl)));

            return ResolveUrl(redirectUrl);
        }


        /// <summary>
        /// Returns all info identifiers that satisfies the condition specified in <see cref="UniGrid"/> filter.
        /// </summary>
        private static IEnumerable<int> GetAllIDs(MassActionParameters generalParameters)
        {
            var objectQuery = new ObjectQuery(generalParameters.ObjectType);
            string idColumn = objectQuery.TypeInfo.IDColumn;
            string whereCondition = generalParameters.WhereCondition;

            return objectQuery
                .Column(idColumn)
                .Where(whereCondition)
                .GetListResult<int>()
                .ToList();
        }


        /// <summary>
        /// Returns behavior type from XML node.
        /// </summary>
        /// <remarks>Method is marked as internal for testing purposes.</remarks>
        internal static MassActionTypeEnum GetActionType(string actionTypeName)
        {
            
            try
            {
                return (MassActionTypeEnum)Enum.Parse(typeof(MassActionTypeEnum), actionTypeName, ignoreCase: true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Invalid action type attribute.", ex);
            }
        }
    }
}
