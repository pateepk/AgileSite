using System;
using System.Linq;

using CMS.Helpers;
using CMS.DocumentEngine.Internal;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Loads the data for <see cref="TreeNode"/> components.
    /// </summary>
    /// <remarks>
    /// Loads missing components of <see cref="TreeNode"/> from database based on current properties. 
    /// Especcialy for linked pages ensures <see cref="TreeNode.CultureData"/> and <see cref="TreeNode.CoupledData"/> components for Continuous Integration restoration.</remarks>
    internal class ContinuousIntegrationComponentsDataLoader : ComponentsDataLoader
    {
        /// <summary>
        /// Creates instance of <see cref="ContinuousIntegrationComponentsDataLoader"/> instance.
        /// </summary>
        /// <param name="document">Document instance.</param>
        public ContinuousIntegrationComponentsDataLoader(TreeNode document) :
            base(document)
        {
        }


        /// <summary>
        /// Loads the culture data of given document.
        /// </summary>
        public override DocumentCultureDataInfo LoadCultureData()
        {
            int nodeId = Document.OriginalNodeID;
            if (nodeId <= 0)
            {
                return base.LoadCultureData();
            }

            var cultureColumn = DocumentQueryColumnBuilder.GetCulturePriorityColumn("DocumentCulture", new[]
            {
                CultureHelper.GetDefaultCultureCode(Document.NodeSiteName)
            });

            var cultureData = DocumentCultureDataInfoProvider.GetDocumentCultures()
                                                .TopN(1)
                                                .WhereEquals("DocumentNodeID", nodeId)
                                                .OrderByDescending(cultureColumn.ToString())
                                                .OrderByAscending("DocumentCulture")
                                                .FirstOrDefault();

            if (cultureData == null)
            {
                throw new InvalidOperationException("Culture data cannot be loaded from the current instance state.");
            }

            return cultureData;
        }


        /// <summary>
        /// Loads coupled data for given document.
        /// </summary>
        public override DocumentFieldsInfo LoadCoupledData()
        {
            if (!Document.IsCoupled)
            {
                return null;
            }

            int documentForeignKeyValue = Document.DocumentForeignKeyValue;
            if (documentForeignKeyValue <= 0)
            {
                return base.LoadCoupledData();
            }

            var coupledData = DocumentFieldsInfoProvider.GetDocumentFieldsInfo(documentForeignKeyValue, Document.DataClassInfo.ClassName);
            if (coupledData == null)
            {
                throw new InvalidOperationException("Coupled data cannot be loaded from the current instance state.");
            }

            return coupledData;
        }
    }
}