using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Default implementation of <see cref="IObjectTypeGraphService"/>.
    /// </summary>
    internal class DefaultObjectTypeGraphService : IObjectTypeGraphService
    {
        // The graph increases the hierarchy level for child/binding nodes if their count exceeds this value
        private const int NODE_LEVEL_INCREASE_THRESHOLD = 11;

        // Trimming threshold for graph node labels
        private const int NODE_LABEL_MAX_LENGTH = 20;

        // Node group name constants
        private const string GROUP_CURRENT_OBJECTTYPE = "current";
        private const string GROUP_PARENTCHILD = "parentchild";
        private const string GROUP_CHILDBINDING = "childbinding";
        private const string GROUP_OTHERBINDING = "otherbinding";
        private const string GROUP_BINDINGBRANCH = "bindingbranch";

        // Graph edge color constants
        private const string COLOR_PARENTCHILD = "#1175ae";
        private const string COLOR_CHILDBINDING = "#a3a2a2";
        private const string COLOR_OTHERBINDING = "#d6d9d6";

        // ID component for binding branch nodes
        private const string BRANCH_ID_MODIFIER = "|branch";        


        /// <summary>
        /// Loads data for a vis.js object type graph.
        /// </summary>
        /// <param name="objectType">The object type for which the graph data is loaded.</param>
        /// <param name="scope">Indicates which types of related object types are loaded for standard objects.</param>
        public GraphData LoadGraphData(string objectType, ObjectTypeGraphScopeEnum scope)
        {
            var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);

            if (typeInfo == null)
            {
                throw new ArgumentException(String.Format("The '{0}' object type was not found.", objectType));
            }

            // Creates the graph builder instance, along with the initial graph node representing the processed object type
            var builder = new ObjectTypeGraphBuilder(objectType, GROUP_CURRENT_OBJECTTYPE)
            {
                LevelOverflowThreshold = NODE_LEVEL_INCREASE_THRESHOLD,
                NodeLabelMaxLength = NODE_LABEL_MAX_LENGTH
            };

            // Loads data for binding object type graphs
            if (typeInfo.IsBinding)
            {
                LoadParentsForBinding(builder, typeInfo);
                LoadRelationsForBinding(builder, typeInfo);

                return builder.Data;
            }

            // Loads data for standard (non-binding) object type graphs
            LoadStandardObjectGraphData(builder, typeInfo, scope);
                     
            return builder.Data;
        }


        private void LoadParentsForBinding(ObjectTypeGraphBuilder builder, ObjectTypeInfo typeInfo)
        {
            // Checks for parent types of the binding that are registered in the 'RegisterAsBindingToObjectTypes' collection (this overrides the main parent)
            var secondaryParentTypes = typeInfo.RegisterAsBindingToObjectTypes;

            // Does not use the 'RegisterAsBindingToObjectTypes' collection if it is explicitly set as empty
            if (secondaryParentTypes != null && secondaryParentTypes.Count > 0)
            {
                // Creates a branch node if there are multiple registered parent types
                if (secondaryParentTypes.Count > 1)
                {
                    var branchNode = builder.AddBranchNode(typeInfo.ObjectType + BRANCH_ID_MODIFIER, GROUP_BINDINGBRANCH, 1);
                    builder.AddEdge(builder.MainNode, branchNode, COLOR_CHILDBINDING, true, "");

                    foreach (var parentType in secondaryParentTypes)
                    {
                        var branchedParent = builder.AddNode(parentType, GROUP_CHILDBINDING, 0);
                        builder.AddEdge(branchNode, branchedParent, COLOR_CHILDBINDING, true);
                    }

                    builder.MainNode.Level = 2;
                }
                else
                {
                    var singleParent = builder.AddNode(secondaryParentTypes.First(), GROUP_CHILDBINDING, 0);
                    builder.AddEdge(builder.MainNode, singleParent, COLOR_CHILDBINDING, true);

                    builder.MainNode.Level = 1;
                }
            }
            else
            {
                string mainParentType = typeInfo.ParentObjectType;
                if (!String.IsNullOrEmpty(mainParentType))
                {
                    var parent = builder.AddNode(mainParentType, GROUP_CHILDBINDING, 0);
                    builder.AddEdge(builder.MainNode, parent, COLOR_CHILDBINDING, true);

                    builder.MainNode.Level = 1;
                }
            }
        }


        private void LoadRelationsForBinding(ObjectTypeGraphBuilder builder, ObjectTypeInfo typeInfo)
        {
            // Builds a list of all object types that are binding dependencies
            var bindings = typeInfo.ObjectDependencies.Where(d => d.DependencyType == ObjectDependencyEnum.Binding).Select(d => d.DependencyObjectType);
            var bindingTypeList = bindings.ToList();
            if (typeInfo.IsSiteBinding)
            {
                bindingTypeList.Add(PredefinedObjectType.SITE);
            }

            foreach (string bindingType in bindingTypeList)
            {
                // Checks for binding dependencies with a null object type
                var bindingTypeInfo = ObjectTypeManager.GetTypeInfo(bindingType);
                if (bindingTypeInfo != null)
                {
                    bool branchedBinding = false;

                    // Branches bindings for object types with multiple TypeInfos, if they are manually registered in the 'RegisterAsOtherBindingToObjectTypes' collection
                    var registeredOtherBindings = typeInfo.RegisterAsOtherBindingToObjectTypes;
                    if ((registeredOtherBindings != null) && (registeredOtherBindings.Count > 0) && registeredOtherBindings.Contains(bindingType))
                    {
                        // Gets a collection of other bindings that have the same class name as the given binding dependency
                        var relevantOtherBindings = new List<string>();
                        foreach (var otherBinding in registeredOtherBindings)
                        {
                            var otherBindingTypeInfo = ObjectTypeManager.GetTypeInfo(otherBinding);
                            if (otherBindingTypeInfo.ObjectClassName.Equals(bindingTypeInfo.ObjectClassName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                relevantOtherBindings.Add(otherBinding);
                            }
                        }

                        // Creates a branch if required, ignores the 'RegisterAsOtherBindingToObjectTypes' collection if there are not multiple TypeInfos for the binding dependency
                        if (relevantOtherBindings.Count > 1)
                        {
                            int multipleBranchOccurrences = builder.Data.Nodes.Count(n => n.Id.Split('_')[0] == bindingType + BRANCH_ID_MODIFIER);

                            var branchNode = builder.AddBranchNode(bindingType + BRANCH_ID_MODIFIER, GROUP_BINDINGBRANCH, builder.MainNode.Level + 1, multipleBranchOccurrences);
                            builder.AddEdge(builder.MainNode, branchNode, COLOR_OTHERBINDING, true, "");

                            foreach (var otherBinding in relevantOtherBindings)
                            {
                                int multipleNodeOccurrences = builder.Data.Nodes.Count(n => n.Id.Split('_')[0] == otherBinding);

                                var branchedBindingNode = builder.AddNode(otherBinding, GROUP_OTHERBINDING, builder.MainNode.Level + 2, multipleNodeOccurrences);
                                builder.AddEdge(branchNode, branchedBindingNode, COLOR_OTHERBINDING, true);
                            }

                            branchedBinding = true;
                        }
                    }

                    if (!branchedBinding)
                    {
                        // Ensures unique IDs for binding graphs containing multiple occurrences of the same object type
                        int multipleNodeOccurrences = builder.Data.Nodes.Count(n => n.Id.Split('_')[0] == bindingType);

                        var bindingNode = builder.AddNode(bindingType, GROUP_OTHERBINDING, builder.MainNode.Level + 1, multipleNodeOccurrences);
                        builder.AddEdge(builder.MainNode, bindingNode, GROUP_OTHERBINDING, true);
                    }
                }
            }
        }


        private void LoadStandardObjectGraphData(ObjectTypeGraphBuilder builder, ObjectTypeInfo typeInfo, ObjectTypeGraphScopeEnum scope)
        {
            // Parent object types
            if (scope.HasFlag(ObjectTypeGraphScopeEnum.ParentObjects))
            {
                LoadParentObjects(builder, typeInfo);
            }

            // Child object types
            if (scope.HasFlag(ObjectTypeGraphScopeEnum.ChildObjects))
            {
                LoadChildObjects(builder, typeInfo);
            }

            // Child bindings
            if (scope.HasFlag(ObjectTypeGraphScopeEnum.Bindings))
            {
                LoadChildBindings(builder, typeInfo);
            }

            // Other related bindings
            if (scope.HasFlag(ObjectTypeGraphScopeEnum.OtherBindings))
            {
                LoadOtherBindings(builder, typeInfo);
            }
        }


        private void LoadParentObjects(ObjectTypeGraphBuilder builder, ObjectTypeInfo typeInfo)
        {
            // Checks for parent types registered in the 'RegisterAsChildToObjectTypes' collection (this overrides the main parent)
            var otherParentTypes = typeInfo.RegisterAsChildToObjectTypes;

            if (otherParentTypes != null)
            {
                foreach (var parentType in otherParentTypes)
                {
                    var otherParentNode = builder.AddNode(parentType, GROUP_PARENTCHILD, 0);
                    builder.AddEdge(builder.MainNode, otherParentNode, COLOR_PARENTCHILD);
                }

                // An empty 'RegisterAsChildToObjectTypes' collection means that the object type does not have a parent, even if set in the main 'ParentObjectType' TypeInfo property
                if (otherParentTypes.Count > 0)
                {
                    builder.MainNode.Level = 1;
                }
            }
            else
            {
                string parentObjectType = typeInfo.ParentObjectType;
                if (!String.IsNullOrEmpty(parentObjectType))
                {
                    var parentNode = builder.AddNode(parentObjectType, GROUP_PARENTCHILD, 0);
                    builder.AddEdge(builder.MainNode, parentNode, COLOR_PARENTCHILD);

                    builder.MainNode.Level = 1;
                }
            }
        }


        private void LoadChildObjects(ObjectTypeGraphBuilder builder, ObjectTypeInfo typeInfo)
        {
            foreach (var childType in typeInfo.ChildObjectTypes)
            {
                var childNode = builder.AddNodeWithLevelOverflow(childType, GROUP_PARENTCHILD, builder.MainNode.Level + 1);
                builder.AddEdge(childNode, builder.MainNode, COLOR_PARENTCHILD);
            }
        }


        private void LoadChildBindings(ObjectTypeGraphBuilder builder, ObjectTypeInfo typeInfo)
        {
            foreach (var bindingType in typeInfo.BindingObjectTypes)
            {
                var bindingNode = builder.AddNodeWithLevelOverflow(bindingType, GROUP_CHILDBINDING, builder.MainNode.Level + 1);
                builder.AddEdge(bindingNode, builder.MainNode, COLOR_CHILDBINDING, true);
            }
        }


        private void LoadOtherBindings(ObjectTypeGraphBuilder builder, ObjectTypeInfo typeInfo)
        {
            foreach (var bindingType in typeInfo.OtherBindingObjectTypes)
            {
                var bindingNode = builder.AddNodeWithLevelOverflow(bindingType, GROUP_OTHERBINDING, builder.MainNode.Level + 1);
                builder.AddEdge(bindingNode, builder.MainNode, COLOR_OTHERBINDING, true);
            }
        }
    }
}
