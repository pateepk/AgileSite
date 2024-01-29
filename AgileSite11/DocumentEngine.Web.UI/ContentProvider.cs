using System;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using CMS.DataEngine;
using CMS.IO;
using CMS.Localization;
using CMS.PortalEngine;
using CMS.Base;
using CMS.Membership;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Renders content using tree data and stylesheets.
    /// </summary>
    public class ContentProvider
    {
        private TreeProvider mTreeProvider;


        /// <summary>
        /// Tree provider instance used for accessing data. If no TreeProvider instance is assigned, a new one is created.
        /// </summary>
        public TreeProvider TreeProvider
        {
            get
            {
                return mTreeProvider ?? (mTreeProvider = new TreeProvider());
            }
            set
            {
                mTreeProvider = value;
            }
        }


        /// <summary>
        /// Returns node content in XML format without any transformation.
        /// </summary>
        /// <param name="path">Node path</param>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Culture code</param>
        public string GetSingleNodeContent(string siteName, string path, string culture)
        {
            var node = TreeProvider.SelectSingleNode(siteName, path, culture);
            if (node == null)
            {
                return String.Empty;
            }

            return node.GetDataSet().GetXml();
        }


        /// <summary>
        /// Returns node content after using specified transformation.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="path">Node path</param>
        /// <param name="culture">Culture code</param>
        /// <param name="combineWithDefCulture">Combine with default culture</param>
        /// <param name="className">Class name (can be empty, but it's faster if you specify it)</param>
        /// <param name="transformationName">Transformation name in format application.class.transformation</param>
        /// <param name="selectOnlyPublished">Indicates if only published documents should be returned. Default value is false and all documents are returned</param>
        /// <param name="checkPermissions">If true, the method will return only those nodes that can be read by current user</param>
        public string GetSingleNodeContent(string siteName, string path, string culture, bool combineWithDefCulture, string className, string transformationName = null, bool selectOnlyPublished = false, bool checkPermissions = false)
        {
            // Get the document
            TreeNode node = TreeProvider.SelectSingleNode(siteName, path, culture, combineWithDefCulture, className, selectOnlyPublished);
            if (node == null)
            {
                return String.Empty;
            }

            // Prepare the node DataSet
            var data = node.GetDataSet();
            data.Tables[0].TableName = node.NodeClassName.ToLowerCSafe();

            // Check permissions
            if (checkPermissions)
            {
                UserInfo userInfo = UserInfoProvider.GetUserInfo(MembershipContext.AuthenticatedUser.UserID);
                data = TreeSecurityProvider.FilterDataSetByPermissions(data, NodePermissionsEnum.Read, userInfo);
            }

            // Prepare the transformation name
            if (String.IsNullOrEmpty(transformationName))
            {
                transformationName = node.NodeClassName + ".default";
            }
            else if (transformationName.IndexOfCSafe(".") < 0)
            {
                transformationName = node.NodeClassName + "." + transformationName;
            }

            return Transform(data.GetXml(), transformationName);
        }


        /// <summary>
        /// Returns content of nodes in XML format without any transformation.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="path">Node path</param>
        /// <param name="culture">Culture code</param>
        /// <param name="combineWithDefCulture">Combine with default culture</param>
        public string GetNodesContent(string siteName, string path, string culture, bool combineWithDefCulture)
        {
            return TreeProvider.SelectNodes(siteName, path, culture, combineWithDefCulture).GetXml();
        }


        /// <summary>
        /// Returns content of nodes in XML format without any transformation.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="path">Node path</param>
        /// <param name="culture">Culture code</param>
        /// <param name="combineWithDefCulture">Combine with default culture</param>
        /// <param name="classNames">Classnames list (separated by the semicolon)</param>
        public string GetNodesContent(string siteName, string path, string culture, bool combineWithDefCulture, string classNames)
        {
            return TreeProvider.SelectNodes(siteName, path, culture, combineWithDefCulture, classNames).GetXml();
        }


        /// <summary>
        /// Returns content of nodes after using specified transformation.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="path">Node path</param>
        /// <param name="culture">Culture code</param>
        /// <param name="combineWithDefCulture">Combine with default culture</param>
        /// <param name="classNames">Classnames list (separated by the semicolon)</param>
        /// <param name="transformationName">Transformation name in format application.class.transformation</param>
        public string GetNodesContent(string siteName, string path, string culture, bool combineWithDefCulture, string classNames, string transformationName)
        {
            return Transform(TreeProvider.SelectNodes(siteName, path, culture, combineWithDefCulture, classNames).GetXml(), transformationName);
        }


        /// <summary>
        /// Returns content of nodes after using specified transformation.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="path">Node path</param>
        /// <param name="culture">Culture code</param>
        /// <param name="combineWithDefCulture">Combine with default culture</param>
        /// <param name="classNames">Classnames list (separated by the semicolon)</param>
        /// <param name="transformationName">Transformation name in format application.class.transformation</param>
        /// <param name="where">Where condition</param>
        /// <remarks>The default sorting is ascending by node name.</remarks>
        public string GetNodesContent(string siteName, string path, string culture, bool combineWithDefCulture, string classNames, string transformationName, string where)
        {
            return Transform(TreeProvider.SelectNodes(siteName, path, culture, combineWithDefCulture, classNames, where).GetXml(), transformationName);
        }


        /// <summary>
        /// Returns content of nodes after using specified transformation.
        /// </summary>
        /// <param name="path">Node path</param>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Culture code</param>
        /// <param name="combineWithDefCulture">Combine with default culture</param>
        /// <param name="classNames">Classnames list (separated by the semicolon)</param>
        /// <param name="transformationName">Transformation name in format application.class.transformation</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">ORDER BY SQL expressions (without the ORDERBY keyword)</param>
        /// <remarks>The default sorting is ascending by node name.</remarks>
        public string GetNodesContent(string siteName, string path, string culture, bool combineWithDefCulture, string classNames, string transformationName, string where, string orderBy)
        {
            return Transform(TreeProvider.SelectNodes(siteName, path, culture, combineWithDefCulture, classNames, where, orderBy).GetXml(), transformationName);
        }


        /// <summary>
        /// Returns content of nodes after using specified transformation.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="path">Node path</param>
        /// <param name="culture">Culture code</param>
        /// <param name="combineWithDefCulture">Combine with default culture</param>
        /// <param name="classNames">Classnames list (separated by the semicolon)</param>
        /// <param name="transformationName">Transformation name in format application.class.transformation</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">ORDER BY SQL expressions (without the ORDERBY keyword)</param>        
        /// <param name="maxRelativeLevel">Maximum relative node level to be returned. Value 1 returns only the node itself. Use -1 for unlimited recurrence</param>    
        /// <remarks>The default sorting is ascending by node name.</remarks>
        public string GetNodesContent(string siteName, string path, string culture, bool combineWithDefCulture, string classNames, string transformationName, string where, string orderBy, int maxRelativeLevel)
        {
            return Transform(TreeProvider.SelectNodes(siteName, path, culture, combineWithDefCulture, classNames, where, orderBy, maxRelativeLevel).GetXml(), transformationName);
        }


        /// <summary>
        /// Returns content of nodes after using specified transformation.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="path">Node path</param>
        /// <param name="culture">Culture code</param>
        /// <param name="combineWithDefCulture">Combine with default culture</param>
        /// <param name="classNames">Classnames list (separated by the semicolon)</param>
        /// <param name="transformationName">Transformation name in format application.class.transformation</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">ORDER BY SQL expressions (without the ORDERBY keyword)</param>        
        /// <param name="maxRelativeLevel">Maximum relative node level to be returned. Value 1 returns only the node itself. Use -1 for unlimited recurrence</param>    
        /// <param name="selectOnlyPublished">Indicates if only published documents should be returned. Default value is false and all documents are returned</param>
        /// <remarks>The default sorting is ascending by node name.</remarks>
        public string GetNodesContent(string siteName, string path, string culture, bool combineWithDefCulture, string classNames, string transformationName, string where, string orderBy, int maxRelativeLevel, bool selectOnlyPublished)
        {
            return Transform(TreeProvider.SelectNodes(siteName, path, culture, combineWithDefCulture, classNames, where, orderBy, maxRelativeLevel, selectOnlyPublished).GetXml(), transformationName);
        }
        

        /// <summary>
        /// Returns content of nodes after using specified transformation.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="path">Node path</param>
        /// <param name="culture">Culture code</param>
        /// <param name="combineWithDefCulture">Combine with default culture</param>
        /// <param name="classNames">Classnames list (separated by the semicolon)</param>
        /// <param name="transformationName">Transformation name in format application.class.transformation</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">ORDER BY SQL expressions (without the ORDERBY keyword)</param>        
        /// <param name="maxRelativeLevel">Maximum relative node level to be returned. Value 1 returns only the node itself. Use -1 for unlimited recurrence</param>    
        /// <param name="selectOnlyPublished">Indicates if only published documents should be returned. Default value is false and all documents are returned</param>
        /// <param name="checkPermissions">If true, the method will return only those nodes that can be read by current user</param>
        /// <param name="topN">Select top N rows</param>
        /// <remarks>The default sorting is ascending by node name.</remarks>
        public string GetNodesContent(string siteName, string path, string culture, bool combineWithDefCulture, string classNames, string transformationName, string where, string orderBy, int maxRelativeLevel, bool selectOnlyPublished, bool checkPermissions, int topN = 0)
        {
            DataSet resultDS = TreeProvider.SelectNodes(siteName, path, culture, combineWithDefCulture, classNames, where, orderBy, maxRelativeLevel, selectOnlyPublished, topN);

            // check permissions
            if (checkPermissions)
            {
                UserInfo userInfo = UserInfoProvider.GetUserInfo(MembershipContext.AuthenticatedUser.UserID);
                resultDS = TreeSecurityProvider.FilterDataSetByPermissions(resultDS, NodePermissionsEnum.Read, userInfo);
            }

            return Transform(resultDS.GetXml(), transformationName);
        }


        /// <summary>
        /// Returns content of nodes after using specified transformation.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="path">Node path</param>
        /// <param name="culture">Culture code</param>
        /// <param name="combineWithDefCulture">Combine with default culture</param>
        /// <param name="classNames">Classnames list (separated by the semicolon)</param>
        /// <param name="transformationName">Transformation name in format application.class.transformation</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">ORDER BY SQL expressions (without the ORDERBY keyword</param>        
        /// <param name="maxRelativeLevel">Maximum relative node level to be returned. Value 1 returns only the node itself. Use -1 for unlimited recurrence</param>    
        /// <param name="selectOnlyPublished">Indicates if only published documents should be returned. Default value is false and all documents are returned</param>
        /// <param name="checkPermissions">If true, the method will return only those nodes that can be read by current user</param>
        /// <param name="relationshipWithNodeGuid">Select nodes with given relationship with given node</param>
        /// <param name="relationshipName">Name of the relationship</param>
        /// <param name="relatedNodeIsOnTheLeftSide">If true, the returned nodes are on the right side of the relationship</param>
        /// <param name="topN">Select top N rows</param>
        /// <remarks>The default sorting is ascending by node name.</remarks>
        public string GetNodesContent(string siteName, string path, string culture, bool combineWithDefCulture, string classNames, string transformationName, string where, string orderBy, int maxRelativeLevel, bool selectOnlyPublished, bool checkPermissions, Guid relationshipWithNodeGuid, string relationshipName, bool relatedNodeIsOnTheLeftSide, int topN = 0)
        {
            string whereCondition;

            if (relatedNodeIsOnTheLeftSide)
            {
                whereCondition = " NodeID IN (SELECT RightNodeID FROM CMS_Relationship LEFT JOIN CMS_RelationshipName ON CMS_RelationshipName.RelationshipNameID = CMS_Relationship.RelationshipNameID WHERE CMS_RelationshipName.RelationshipName = N'" + SqlHelper.EscapeQuotes(relationshipName) + "' AND LeftNodeID = " + relationshipWithNodeGuid + ") ";
            }
            else
            {
                whereCondition = " NodeID IN (SELECT LeftNodeID FROM CMS_Relationship LEFT JOIN CMS_RelationshipName ON CMS_RelationshipName.RelationshipNameID = CMS_Relationship.RelationshipNameID WHERE CMS_RelationshipName.RelationshipName = N'" + SqlHelper.EscapeQuotes(relationshipName) + "' AND RightNodeID = " + relationshipWithNodeGuid + ") ";
            }

            return GetNodesContent(siteName, path, culture, combineWithDefCulture, classNames, transformationName, whereCondition, orderBy, maxRelativeLevel, selectOnlyPublished, checkPermissions, topN);
        }
        
        
        /// <summary>
        /// Returns data selected by query after using specified transformation.
        /// </summary>
        /// <param name="queryName">Query name in format application.class.query</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="transformationName">Transformation name in format application.class.transformation</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">ORDER BY SQL expressions (without the ORDERBY keyword</param>        
        public string GetNodesContentByQuery(string queryName, QueryDataParameters parameters, string transformationName = null, string where = null, string orderBy = null)
        {
            DataSet ds = ConnectionHelper.ExecuteQuery(queryName, parameters, where, orderBy);
            if (ds != null)
            {
                string xml = ds.GetXml();
                if (String.IsNullOrEmpty(transformationName))
                {
                    return xml;
                }

                return Transform(xml, transformationName);
            }

            return String.Empty;
        }


        /// <summary>
        /// Transforms XML document using provided XSL style sheet.
        /// </summary>
        /// <param name="xml">XML source text</param>
        /// <param name="transformationName">Transformation name in format application.class.transformation</param>
        public string Transform(string xml, string transformationName)
        {
            if (string.IsNullOrEmpty(xml) || (string.IsNullOrEmpty(transformationName)))
            {
                return String.Empty;
            }

            // Load the XSLT transformation
            TransformationInfo ti = TransformationInfoProvider.GetLocalizedTransformation(transformationName, LocalizationContext.PreferredCultureCode);
            if (ti == null)
            {
                return "<span class=\"TransformationError\">Transformation '" + transformationName + "' not found.</span>";
            }

            if (ti.TransformationType == TransformationTypeEnum.Xslt)
            {
                XmlDocument xmlDoc = new XmlDocument();
                StringWriter resultSW = new StringWriter();
                XslCompiledTransform xslTransform = new XslCompiledTransform();

                // Load the transformation content
                string content = ti.TransformationCode;

                // Apply the XSLT transformation
                StringReader stream = new StringReader(content);
                XmlTextReader xmlReader = new XmlTextReader(stream);
                xmlReader.XmlResolver = null;
                xmlDoc.LoadXml(xml);
                XPathNavigator xslDoc = xmlDoc.CreateNavigator();
                xslTransform.Load(xmlReader);
                xslTransform.Transform(xslDoc, null, resultSW);

                return resultSW.ToString();
            }

            throw new Exception("The transformation " + ti.TransformationFullName + " is not Xsl transformation!");
        }
    }
}