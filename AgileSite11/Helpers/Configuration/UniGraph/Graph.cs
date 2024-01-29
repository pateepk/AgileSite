using System.Collections.Generic;

using CMS.Core;

namespace CMS.Helpers.UniGraphConfig
{
    /// <summary>
    /// Configuration class for JSON serialization.
    /// </summary>
    public class Graph
    {
        #region "Constants"

        /// <summary>
        /// Default prefix for resource strings in graph context.
        /// </summary>
        private const string RESOURCES_PREFIX = "graph";


        /// <summary>
        /// Default prefix for resource strings in service context.
        /// </summary>
        private const string SERVICE_RESOURCES_PREFIX = "graphservice";

        #endregion


        #region "Variables"

        /// <summary>
        /// Converted nodes.
        /// </summary>
        private List<Node> mNodes = null;


        /// <summary>
        /// Converted connections.
        /// </summary>
        private List<Connection> mConnections = null;


        /// <summary>
        /// Addresses used in JS.
        /// </summary>
        private Dictionary<string, string> mAddresses = null;


        /// <summary>
        /// Prefix used in resource strings in graph context.
        /// </summary>
        private string mResourcesPrefix = RESOURCES_PREFIX;


        /// <summary>
        /// Prefix used in resource strings in service context.
        /// </summary>
        private string mServiceResourcesPrefix = SERVICE_RESOURCES_PREFIX;


        /// <summary>
        /// Resource strings used in JS service.
        /// </summary>
        private Dictionary<string, string> mServiceResourceStrings = null;


        /// <summary>
        /// Resource strings used in graph JS.
        /// </summary>
        private Dictionary<string, string> mGraphResourceStrings = null;


        /// <summary>
        /// Contains addresses to all needed JS files.
        /// </summary>
        private List<string> mJsFiles = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Addresses used in JS.
        /// </summary>
        public List<string> JsFiles
        {
            get
            {
                if (mJsFiles == null)
                {
                    mJsFiles = GetDefaultJsFiles();
                }
                return mJsFiles;
            }
            set
            {
                mJsFiles = value;
            }
        }


        /// <summary>
        /// Addresses used in JS.
        /// </summary>
        public Dictionary<string, string> Addresses
        {
            get
            {
                if (mAddresses == null)
                {
                    mAddresses = GetDefaultAddresses();
                }
                return mAddresses;
            }
            set
            {
                mAddresses = value;
            }
        }


        /// <summary>
        /// Prefix used in resource strings in graph context.
        /// </summary>
        public string ResourcesPrefix
        {
            get
            {
                return mResourcesPrefix;
            }
            set
            {
                mResourcesPrefix = ValidationHelper.GetString(value, RESOURCES_PREFIX);
            }
        }


        /// <summary>
        /// Prefix used in resource strings in service context.
        /// </summary>
        public string ServiceResourcesPrefix
        {
            get
            {
                return mServiceResourcesPrefix;
            }
            set
            {
                mServiceResourcesPrefix = ValidationHelper.GetString(value, SERVICE_RESOURCES_PREFIX);
            }
        }


        /// <summary>
        /// Resource strings used in graph JS.
        /// </summary>
        public Dictionary<string, string> GraphResourceStrings
        {
            get
            {
                if (mGraphResourceStrings == null)
                {
                    mGraphResourceStrings = GetDefaultGraphResources();
                }
                return mGraphResourceStrings;
            }
        }


        /// <summary>
        /// Resource strings used in graph JS.
        /// </summary>
        public Dictionary<string, string> ServiceResourceStrings
        {
            get
            {
                if (mServiceResourceStrings == null)
                {
                    mServiceResourceStrings = GetDefaultServiceResources();
                }
                return mServiceResourceStrings;
            }
        }


        /// <summary>
        /// ID of graph (not printed to HTML)
        /// </summary>
        public string ID
        {
            get;
            set;
        }


        /// <summary>
        /// Nodes in graph.
        /// </summary>
        public virtual List<Node> Nodes
        {
            get
            {
                if (mNodes == null)
                {
                    mNodes = GetDefaultNodes();
                }
                return mNodes;
            }
            set
            {
                mNodes = value;
            }
        }


        /// <summary>
        /// Connections in graph.
        /// </summary>
        public virtual List<Connection> Connections
        {
            get
            {
                if (mConnections == null)
                {
                    mConnections = GetDefaultConnections();
                }
                return mConnections;
            }
            set
            {
                mConnections = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns default list of connections.
        /// </summary>
        /// <returns>List of connections.</returns>
        protected virtual List<Connection> GetDefaultConnections()
        {
            return new List<Connection>();
        }


        /// <summary>
        /// Returns default list of nodes.
        /// </summary>
        /// <returns>List of nodes</returns>
        protected virtual List<Node> GetDefaultNodes()
        {
            return new List<Node>();
        }


        /// <summary>
        /// Returns default dictionary of addresses.
        /// </summary>
        /// <returns>Dictionary of addresses</returns>
        protected virtual Dictionary<string, string> GetDefaultAddresses()
        {
            return new Dictionary<string, string>
            {
                {"ImagesUrl", URLHelper.ResolveUrl(AdministrationUrlHelper.GetImageUrl("/Design/Controls/UniGraph"))},
                {"ApplicationUrl", URLHelper.GetApplicationUrl()}
            };
        }

                
        /// <summary>
        /// Returns default dictionary of graph resource strings.
        /// </summary>
        /// <returns>Dictionary of resource strings</returns>
        protected virtual Dictionary<string, string> GetDefaultGraphResources()
        {
            return new Dictionary<string, string>
            {
                {"EditNodeTooltip", GetString("editNodeTooltip")},
                {"DeleteNodeTooltip", GetString("deleteNodeTooltip")},
                {"EditCaseTooltip", GetString("editCaseTooltip")},
                {"DeleteCaseTooltip", GetString("deleteCaseTooltip")},
                {"AddCaseTooltip", GetString("addCaseTooltip")},
                {"AddChoiceTooltip", GetString("addChoiceTooltip")},
                {"EditChoiceTooltip", GetString("editChoiceTooltip")},
                {"DeleteChoiceTooltip", GetString("deleteChoiceTooltip")},
                {"SourcepointStandardTooltip", GetString("sourcepointStandardTooltip")},
                {"SourcepointSwitchCaseTooltip", GetString("sourcepointSwitchCaseTooltip")},
                {"SourcepointSwitchChoiceTooltip", GetString("sourcepointSwitchChoiceTooltip")},
                {"SourcepointSwitchDefaultTooltip", GetString("sourcepointSwitchDefaultTooltip")},
                {"SourcepointTimeoutTooltip", GetString("sourcepointTimeoutTooltip")},
                {"ReattachHelperTooltip", GetString("reattachHelperTooltip")},
                {"TimeoutIconTooltip", GetString("timeoutIconTooltip")},

                {"AutomaticConnectionType", GetString("automaticConnectionType")},
                {"ManualConnectionType", GetString("manualConnectionType")},

                {"CaseDeleteConfirmation", GetServicesString("casedeleteconfirmation")},
                {"MinCaseSourcePointCountError", GetServicesString("toofewsourcepoints")},
                {"MaxCaseSourcePointCountError", GetServicesString("toomanysourcepoints")},
                {"ChoiceDeleteConfirmation", GetServicesString("choicedeleteconfirmation")},
                {"MinChoiceSourcePointCountError", GetServicesString("choicetoofewsourcepoints")},
                {"MaxChoiceSourcePointCountError", GetServicesString("choicetoomanysourcepoints")},
            };
        }


        /// <summary>
        /// Gets the localized string
        /// </summary>
        /// <param name="key">String key</param>
        private string GetString(string key)
        {
            return CoreServices.Localization.GetString(ResourcesPrefix + "." + key, null);
        }


        /// <summary>
        /// Gets the localized string for services
        /// </summary>
        /// <param name="key">String key</param>
        private string GetServicesString(string key)
        {
            return CoreServices.Localization.GetString(ServiceResourcesPrefix + "." + key, null);
        }

        
        /// <summary>
        /// Returns default dictionary of service resource strings.
        /// </summary>
        /// <returns>Dictionary of resource strings</returns>
        protected virtual Dictionary<string, string> GetDefaultServiceResources()
        {
            return new Dictionary<string, string>
            {
                {"CriticalError", GetServicesString("criticalerror")},
                {"NodeDeleteConfirmation", GetServicesString("nodedeleteconfirmation")},
                {"ConnectionDeleteConfirmation", GetServicesString("connectiondeleteconfirmation")},
                {"NondeletableNode", GetServicesString("nondeletablenode")},
                {"NoItemSelected", GetServicesString("noitemselected")}
            };
        }


        /// <summary>
        /// Return default list of JS files needed in graph.
        /// </summary>
        /// <returns>List of JS files to be registered</returns>
        protected virtual List<string> GetDefaultJsFiles()
        {
            return new List<string>
            {
                {"~/CMSModules/Workflows/Controls/Scripts/jquery.jsPlumb-1.3.3-all.js"},
                {"~/CMSModules/Workflows/Controls/Scripts/Graph.js"},
                {"~/CMSModules/Workflows/Controls/Scripts/Handlers.js"},
                {"~/CMSModules/Workflows/Controls/Scripts/AbstractNode.js"},

                {"~/CMSModules/Workflows/Controls/Scripts/StandardNode.js"},
                {"~/CMSModules/Workflows/Controls/Scripts/ActionNode.js"},
                {"~/CMSModules/Workflows/Controls/Scripts/ConditionNode.js"},
                {"~/CMSModules/Workflows/Controls/Scripts/MultichoiceNode.js"},
                {"~/CMSModules/Workflows/Controls/Scripts/UserchoiceNode.js"}
            };
        }

        #endregion
    }
}
