using System;
using System.Collections.Generic;
using System.Text;

using CMS.Base;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Represents a structure of of boolean expressions.
    /// </summary>
    [Serializable]
    public class MacroDesignerTree
    {
        #region "Variables"

        private List<MacroDesignerTree> mChildGroups = null;
        private MacroDesignerTree mParentGroup = null;
        private int mPosition = 0;
        private string mLeftExpression = "";
        private string mRightExpression = "";
        private string mOperator = "";
        private string mGroupOperator = "&&";

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the child groups.
        /// </summary>
        public List<MacroDesignerTree> ChildGroups
        {
            get
            {
                if (mChildGroups == null)
                {
                    mChildGroups = new List<MacroDesignerTree>();
                }
                return mChildGroups;
            }
            set
            {
                mChildGroups = value;
            }
        }


        /// <summary>
        /// Gets or sets the parent group.
        /// </summary>
        public MacroDesignerTree ParentGroup
        {
            get
            {
                return mParentGroup;
            }
            set
            {
                mParentGroup = value;
            }
        }


        /// <summary>
        /// Gets or sets the relative position of the group within the parent group.
        /// </summary>
        public int Position
        {
            get
            {
                return mPosition;
            }
            set
            {
                mPosition = value;
            }
        }


        /// <summary>
        /// Gets or sets left part of the boolean expression.
        /// </summary>
        public string LeftExpression
        {
            get
            {
                return mLeftExpression;
            }
            set
            {
                mLeftExpression = value;
            }
        }


        /// <summary>
        /// Gets or sets right part of the boolean expression.
        /// </summary>
        public string RightExpression
        {
            get
            {
                return mRightExpression;
            }
            set
            {
                mRightExpression = value;
            }
        }


        /// <summary>
        /// Gets or sets operator of the group.
        /// </summary>
        public string GroupOperator
        {
            get
            {
                return mGroupOperator;
            }
            set
            {
                mGroupOperator = value;
            }
        }


        /// <summary>
        /// Gets or sets operator of the boolean expression.
        /// </summary>
        public string Operator
        {
            get
            {
                return mOperator;
            }
            set
            {
                mOperator = value;
            }
        }

        /// <summary>
        /// Returns IDPath of the group.
        /// </summary>
        public string IDPath
        {
            get
            {
                if (ParentGroup == null)
                {
                    return "";
                }
                else
                {
                    return (string.IsNullOrEmpty(ParentGroup.IDPath) ? "" : ParentGroup.IDPath + ".") + Position;
                }
            }
        }


        /// <summary>
        /// Returns level of the group.
        /// </summary>
        public int Level
        {
            get
            {
                if (ParentGroup == null)
                {
                    return 0;
                }
                else
                {
                    if (IsLeaf)
                    {
                        return ParentGroup.Level;
                    }
                    else
                    {
                        return ParentGroup.Level + 1;
                    }
                }
            }
        }


        /// <summary>
        /// Returns true if group is a leaf (expression).
        /// </summary>
        public bool IsLeaf
        {
            get
            {
                return !(string.IsNullOrEmpty(LeftExpression) && string.IsNullOrEmpty(RightExpression) && string.IsNullOrEmpty(Operator));
            }
        }


        /// <summary>
        /// Returns the condition represented by the current tree node.
        /// </summary>
        public string Condition
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                bool isFirst = true;
                foreach (MacroDesignerTree item in ChildGroups)
                {
                    if (item.IsLeaf)
                    {
                        if (!isFirst)
                        {
                            sb.Append(" " + item.GroupOperator + " ");
                        }

                        // Append leaf expression
                        if (item.Operator == "--")
                        {
                            sb.Append(item.LeftExpression);
                        }
                        else if (item.Operator == "!--")
                        {
                            sb.Append("!" + item.LeftExpression);
                        }
                        else
                        {
                            if (item.Operator.EndsWithCSafe("contains") || item.Operator.EndsWithCSafe("startswith") || item.Operator.EndsWithCSafe("endswith"))
                            {
                                bool negate = item.Operator.StartsWithCSafe("!");
                                sb.Append((negate ? "!" : "") + item.LeftExpression + "." + (negate ? item.Operator.Substring(1) : item.Operator) + "(" + item.RightExpression + ")");
                            }
                            else
                            {
                                sb.Append(item.LeftExpression + " " + item.Operator + " " + item.RightExpression);
                            }
                        }

                        isFirst = false;
                    }
                    else
                    {
                        // Add only non-empty groups
                        string condition = item.Condition;
                        if (!string.IsNullOrEmpty(condition))
                        {
                            // Add operator only if its not the first child in the current group
                            if (!isFirst)
                            {
                                sb.Append(" " + item.GroupOperator + " ");
                            }
                            sb.Append("(", condition, ")");
                            isFirst = false;
                        }
                    }
                }
                return sb.ToString();
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new group (wrapper)
        /// </summary>
        public MacroDesignerTree()
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Builds designer tree object from given MacroExpression.
        /// </summary>
        /// <param name="expression">Parsed expression</param>
        public static MacroDesignerTree BuildTreeFromExpression(MacroExpression expression)
        {
            MacroDesignerTree tree = BuildTreeFromExpressionInternal(expression, false);
            if ((tree != null) && (tree.IsLeaf))
            {
                // If the build tree is only a leaf expression, we need to wrap it into the top-most group
                MacroDesignerTree root = new MacroDesignerTree();
                root.AddGroup(tree);

                return root;
            }
            return tree;
        }


        /// <summary>
        /// Builds designer tree object from given MacroExpression.
        /// </summary>
        /// <param name="expression">Parsed expression</param>
        /// <param name="negate">Negates processed operator</param>
        private static MacroDesignerTree BuildTreeFromExpressionInternal(MacroExpression expression, bool negate)
        {
            MacroDesignerTree retval = new MacroDesignerTree();

            if (expression != null)
            {
                bool processed = false;
                if (expression.Type == ExpressionType.SubExpression)
                {
                    if ((expression.Children != null) && (expression.Children.Count == 1))
                    {
                        retval = BuildTreeFromExpressionInternal((MacroExpression)expression.Children[0], negate);
                        processed = true;
                    }
                }
                else if (expression.Type == ExpressionType.MethodCall)
                {
                    // Method call means either creation of two groups (for && or || operators) or new boolean expression
                    if ((expression.Name == "&&") || (expression.Name == "||"))
                    {
                        if (expression.Children != null)
                        {
                            foreach (MacroExpression expr in expression.Children)
                            {
                                MacroDesignerTree child = BuildTreeFromExpressionInternal(expr, negate);
                                child.GroupOperator = expression.Name;
                                retval.AddGroup(child);
                            }
                        }
                        processed = true;
                    }
                    else
                    {
                        // New leaf expression with more structure maybe
                        if (IsSupportedOperator(expression.Name))
                        {
                            if (expression.Children != null)
                            {
                                if (expression.Name == "!")
                                {
                                    // Special case for negation
                                    MacroDesignerTree child = BuildTreeFromExpressionInternal(((MacroExpression)expression.Children[0]), !negate);
                                    retval.AddGroup(child);
                                }
                                else
                                {
                                    if (expression.Children.Count == 2)
                                    {
                                        retval.LeftExpression = ((MacroExpression)expression.Children[0]).ToString();
                                        retval.RightExpression = ((MacroExpression)expression.Children[1]).ToString();
                                        retval.Operator = (negate ? NegateOperator(expression.Name) : expression.Name);
                                    }
                                }
                            }
                            processed = true;
                        }
                    }
                }

                if (!processed)
                {
                    // Leaf expression
                    retval.LeftExpression = expression.ToString();
                    retval.RightExpression = "";
                    retval.Operator = (negate ? "!--" : "--");
                }
            }

            return retval;
        }

        /// <summary>
        /// Nagates given operator.
        /// </summary>
        /// <param name="op">Operator to negate</param>
        private static string NegateOperator(string op)
        {
            string loLower = op.ToLowerCSafe();
            switch (loLower)
            {
                case "==":
                    return "!=";

                case "!=":
                    return "==";

                case "<":
                    return ">=";

                case ">":
                    return "<=";

                case "<=":
                    return ">";

                case ">=":
                    return "<";

                case "contains":
                case "endswith":
                case "startswith":
                    return "!" + loLower;

                default:
                    return op;
            }
        }


        /// <summary>
        /// Returns concatenation of subset of the elements.
        /// </summary>
        /// <param name="elements">List of elements</param>
        /// <param name="start">Starting index (inclusive)</param>
        /// <param name="end">Ending index (inclusive)</param>
        private static string GetElementsString(List<MacroElement> elements, int start, int end)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = start; i <= end; i++)
            {
                sb.Append(elements[i].ToString());
            }
            return sb.ToString();
        }


        /// <summary>
        /// Returns true, if given operator is supported in the boolean expression.
        /// </summary>
        /// <param name="op">Operator name (symbol)</param>
        private static bool IsSupportedOperator(string op)
        {
            switch (op.ToLowerCSafe())
            {
                case "!":
                case "==":
                case "!=":
                case ">":
                case "<":
                case ">=":
                case "<=":
                case "contains":
                case "endswith":
                case "startswith":
                    return true;
                default:
                    return false;
            }
        }


        /// <summary>
        /// Adds new group into the structure.
        /// </summary>
        public MacroDesignerTree AddGroup()
        {
            return AddGroup(null);
        }


        /// <summary>
        /// Adds given group as a new child.
        /// </summary>
        /// <param name="position">Relative position within the children collection</param>
        public void RemoveGroup(int position)
        {
            if ((position >= 0) && (ChildGroups.Count > position))
            {
                ChildGroups.RemoveAt(position);
            }
            ResetPositions();
        }


        /// <summary>
        /// Adds given group as a new child.
        /// </summary>
        public MacroDesignerTree AddGroup(MacroDesignerTree group)
        {
            return AddGroup(group, -1);
        }


        /// <summary>
        /// Adds given group as a new child.
        /// </summary>
        /// <param name="group">Group to add</param>
        /// <param name="position">Position within the parent group</param>
        public MacroDesignerTree AddGroup(MacroDesignerTree group, int position)
        {
            MacroDesignerTree groupToAdd = group;
            if (groupToAdd == null)
            {
                groupToAdd = new MacroDesignerTree();
            }

            if (!IsLeaf)
            {
                groupToAdd.ParentGroup = this;
                if ((position < 0) || (position >= ChildGroups.Count))
                {
                    groupToAdd.Position = ChildGroups.Count;
                    ChildGroups.Add(groupToAdd);
                }
                else
                {
                    groupToAdd.Position = position;
                    ChildGroups.Insert(position, groupToAdd);
                    ResetPositions();
                }

                if (groupToAdd.Position > 0)
                {
                    // Inherit the last used operator when adding new group
                    groupToAdd.GroupOperator = ChildGroups[groupToAdd.Position - 1].GroupOperator;
                }

                return groupToAdd;
            }
            else
            {
                throw new NotSupportedException("Cannot add a group into the expression group.");
            }
        }


        /// <summary>
        /// Adds an expression (leaf in the tree structure) as a new child.
        /// </summary>
        public MacroDesignerTree AddExpression()
        {
            if (!IsLeaf)
            {
                MacroDesignerTree group = new MacroDesignerTree();
                group.Operator = "&&";
                group.ParentGroup = this;
                group.Position = ChildGroups.Count;
                ChildGroups.Add(group);

                return group;
            }
            else
            {
                throw new NotSupportedException("Cannot add a group into the expression group.");
            }
        }


        /// <summary>
        /// Moves the group to given location.
        /// </summary>
        /// <param name="sourcePath">Position path of the source</param>
        /// <param name="targetPath">Position path of the target</param>
        /// <param name="targetPos">Position within the target</param>
        public void MoveGroup(string sourcePath, string targetPath, int targetPos)
        {
            string[] source = sourcePath.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            string[] target = targetPath.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            MacroDesignerTree srcGroup = this;
            foreach (string posStr in source)
            {
                int pos = ValidationHelper.GetInteger(posStr, 0);
                if (srcGroup.ChildGroups.Count > pos)
                {
                    srcGroup = srcGroup.ChildGroups[pos];
                }
                else
                {
                    throw new Exception("[MacroDesigner.MoveGroup]: Incorrect source path.");
                }
            }

            MacroDesignerTree targetGroup = this;
            foreach (string posStr in target)
            {
                int pos = ValidationHelper.GetInteger(posStr, 0);
                if (targetGroup.ChildGroups.Count > pos)
                {
                    targetGroup = targetGroup.ChildGroups[pos];
                }
                else
                {
                    throw new Exception("[MacroDesigner.MoveGroup]: Incorrect target path.");
                }
            }

            // Move the group
            MacroDesignerTree srcParent = srcGroup.ParentGroup;
            srcParent.ChildGroups.Remove(srcGroup);
            targetGroup.AddGroup(srcGroup, targetPos);

            // Reset the positions
            srcParent.ResetPositions();
            targetGroup.ResetPositions();
        }


        /// <summary>
        /// Sets correct positions according to current state.
        /// </summary>
        public void ResetPositions()
        {
            int i = 0;
            foreach (MacroDesignerTree child in ChildGroups)
            {
                child.Position = i++;
                child.ResetPositions();
            }
        }

        #endregion
    }
}