using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;

using CMS.Helpers;
using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing checkout process definition management.
    /// </summary>
    public class CheckoutProcessInfo
    {
        /// <summary>
        /// This constants says that index of the step is not known.
        /// </summary>
        public const int STEP_INDEX_NOT_KNOWN = -1;

        private XmlDocument xml = null;

        /// <summary>
        /// Root node of the checkout process xml definition.
        /// </summary>
        private XmlNode RootNode
        {
            get
            {
                return xml?.DocumentElement;
            }
        }


        #region "XML methods"

        /// <summary>
        /// Updates/inserts checkout step node in/into xml definition.
        /// </summary>
        /// <param name="stepObj">Checkout step data object with new step information</param>
        public void SetCheckoutProcessStepNode(CheckoutProcessStepInfo stepObj)
        {
            if ((RootNode == null) || (stepObj == null))
            {
                return;
            }
            bool isInsert = false;

            // Try to find checkout step node by its name
            XmlNode stepNode = GetCheckoutProcessStepNode(stepObj.Name);

            // Node doesnt exist -> insert
            if (stepNode == null)
            {
                stepNode = xml.CreateElement("step");
                isInsert = true;
            }

            // Initialize step node attributes
            string[,] attributes = new string[8, 2];
            attributes[0, 0] = "name";
            attributes[0, 1] = stepObj.Name;
            attributes[1, 0] = "caption";
            attributes[1, 1] = stepObj.Caption;
            attributes[2, 0] = "icon";
            attributes[2, 1] = stepObj.Icon;
            attributes[3, 0] = "path";
            attributes[3, 1] = stepObj.ControlPath;
            attributes[4, 0] = "livesite";
            attributes[4, 1] = ValidationHelper.GetString(stepObj.ShowOnLiveSite, "false").ToLowerCSafe();
            attributes[5, 0] = "cmsdeskorder";
            attributes[5, 1] = ValidationHelper.GetString(stepObj.ShowInCMSDeskOrder, "false").ToLowerCSafe();
            attributes[6, 0] = "cmsdeskorderitems";
            attributes[6, 1] = ValidationHelper.GetString(stepObj.ShowInCMSDeskOrderItems, "false").ToLowerCSafe();
            attributes[7, 0] = "cmsdeskcustomer";
            attributes[7, 1] = ValidationHelper.GetString(stepObj.ShowInCMSDeskCustomer, "false").ToLowerCSafe();

            // Update node attributes 
            XmlHelper.SetXmlNodeAttributes(stepNode, attributes, false);

            if (isInsert)
            {
                // Add new node to the end of the collection
                RootNode.AppendChild(stepNode);
            }
        }


        /// <summary>
        /// Updates/inserts checkout step node in/into xml definition.
        /// </summary>
        /// <param name="stepObj">Checkout step data object with new step information</param>
        /// <param name="originalName">Name of the node to be replaced</param>
        public void ReplaceCheckoutProcessStepNode(CheckoutProcessStepInfo stepObj, string originalName)
        {
            if ((RootNode == null) || (stepObj == null))
            {
                return;
            }

            // Create new step node
            XmlNode stepNode = xml.CreateElement("step");

            // Initialize step node attributes
            string[,] attributes = new string[8, 2];
            attributes[0, 0] = "name";
            attributes[0, 1] = stepObj.Name;
            attributes[1, 0] = "caption";
            attributes[1, 1] = stepObj.Caption;
            attributes[2, 0] = "icon";
            attributes[2, 1] = stepObj.Icon;
            attributes[3, 0] = "path";
            attributes[3, 1] = stepObj.ControlPath;
            attributes[4, 0] = "livesite";
            attributes[4, 1] = ValidationHelper.GetString(stepObj.ShowOnLiveSite, "false").ToLowerCSafe();
            attributes[5, 0] = "cmsdeskorder";
            attributes[5, 1] = ValidationHelper.GetString(stepObj.ShowInCMSDeskOrder, "false").ToLowerCSafe();
            attributes[6, 0] = "cmsdeskorderitems";
            attributes[6, 1] = ValidationHelper.GetString(stepObj.ShowInCMSDeskOrderItems, "false").ToLowerCSafe();
            attributes[7, 0] = "cmsdeskcustomer";
            attributes[7, 1] = ValidationHelper.GetString(stepObj.ShowInCMSDeskCustomer, "false").ToLowerCSafe();

            // Update node attributes 
            XmlHelper.SetXmlNodeAttributes(stepNode, attributes, false);

            bool replace = false;
            XmlNode originalStepNode = null;

            // Replace existing node with the new one
            if (!String.IsNullOrEmpty(originalName))
            {
                // Get original step node
                originalStepNode = GetCheckoutProcessStepNode(originalName);
                if (originalStepNode != null)
                {
                    replace = true;
                }
            }

            if (replace)
            {
                // Replace existing node with the new one
                RootNode.InsertAfter(stepNode, originalStepNode);
                RootNode.RemoveChild(originalStepNode);
            }
            else
            {
                // Add new node to the end of the collection
                // this.RootNode.AppendChild(stepNode);
            }
        }


        /// <summary>
        /// Removes checkout step from the XML.
        /// </summary>
        public void RemoveCheckoutProcessStepNode(string stepName)
        {
            if (RootNode != null)
            {
                XmlNode stepNode = GetCheckoutProcessStepNode(stepName);
                if (stepNode != null)
                {
                    RootNode.RemoveChild(stepNode);
                }
            }
        }


        /// <summary>
        /// Moves specified step up.
        /// </summary>
        /// <param name="stepName">Name of the step to move</param>
        public void MoveCheckoutProcessStepNodeUp(string stepName)
        {
            if (RootNode != null)
            {
                XmlNode actualNode = GetCheckoutProcessStepNode(stepName);
                XmlNode previousNode = actualNode.PreviousSibling;
                XmlNode tempNode = actualNode.Clone();

                if ((actualNode != null) && (previousNode != null))
                {
                    // Remove actual node
                    RootNode.RemoveChild(actualNode);

                    // Insert temp node before the previous node
                    RootNode.InsertBefore(tempNode, previousNode);
                }
            }
        }


        /// <summary>
        /// Moves specified step down.
        /// </summary>
        /// <param name="stepName">Name of the step to move</param>
        public void MoveCheckoutProcessStepNodeDown(string stepName)
        {
            if (RootNode != null)
            {
                // Get the node
                XmlNode actualNode = GetCheckoutProcessStepNode(stepName);
                XmlNode nextNode = actualNode.NextSibling;
                XmlNode tempNode = actualNode.Clone();

                if ((actualNode != null) && (nextNode != null))
                {
                    // remove actual node
                    RootNode.RemoveChild(actualNode);

                    //insert temp node after the nextNode
                    RootNode.InsertAfter(tempNode, nextNode);
                }
            }
        }


        /// <summary>
        /// Returns checkout step node according to the specified step name.
        /// </summary>
        /// <param name="stepName">Checkout step name</param>
        private XmlNode GetCheckoutProcessStepNode(string stepName)
        {
            return RootNode?.SelectSingleNode("step[@name='" + stepName + "']");
        }

        #endregion


        #region "CheckoutProcessStepInfo methods"

        ///// <summary>
        ///// Returns index of the checkout process step
        ///// </summary>
        ///// <param name="stepName">Checkout process step code name</param>
        ////public int GetCheckoutProcessStepIndex(string stepName)
        //{
        //    if (this.RootNode != null)
        //    {
        //        for (int i = 0; i < this.RootNode.ChildNodes.Count; i++)
        //        {
        //            XmlNode node = GetCheckoutProcessStepNode(stepName);
        //            if (node != null)
        //            {
        //                return i;
        //            }
        //        }

        //    }
        //}


        /// <summary>
        /// Returns CheckoutProcessStepInfo object created from the xml node of the specified name attribute.
        /// </summary>
        /// <param name="stepName">Step name</param>
        public CheckoutProcessStepInfo GetCheckoutProcessStepInfo(string stepName)
        {
            if (RootNode != null)
            {
                XmlNode node = GetCheckoutProcessStepNode(stepName);
                if (node != null)
                {
                    return new CheckoutProcessStepInfo(node);
                }
            }
            return null;
        }


        /// <summary>
        /// Returns CheckoutProcessStepInfo object created from the xml node of the specified order(index).
        /// </summary>
        /// <param name="index">Index of the step to be returned</param>
        public CheckoutProcessStepInfo GetCheckoutProcessStepInfo(int index)
        {
            if ((RootNode != null) && (RootNode.ChildNodes.Count > index))
            {
                if (RootNode.ChildNodes[index].Name.ToLowerCSafe() == "step")
                {
                    return new CheckoutProcessStepInfo(RootNode.ChildNodes[index]);
                }
            }
            return null;
        }


        /// <summary>
        /// Returns list of steps for the specified type of checkout process.
        /// </summary>
        /// <param name="process">Checkout process type</param>
        public List<CheckoutProcessStepInfo> GetCheckoutProcessSteps(CheckoutProcessEnum process)
        {
            List<CheckoutProcessStepInfo> result = new List<CheckoutProcessStepInfo>();

            XmlNodeList list = GetCheckoutProcessStepNodes(process);
            if (list != null)
            {
                foreach (XmlNode node in list)
                {
                    result.Add(new CheckoutProcessStepInfo(node));

                    // Set zero based index of the currently added step
                    int index = result.Count - 1;
                    result[index].StepIndex = index;
                }
            }
            return result;
        }

        #endregion


        #region "Common methods"

        /// <summary>
        /// Loads checkout process xml definition.
        /// </summary>
        /// <param name="definition">Checkout process xml definition</param>
        public void LoadXmlDefinition(string definition)
        {
            if (String.IsNullOrEmpty(definition))
            {
                definition = "<checkout></checkout>";
            }

            try
            {
                xml = new XmlDocument();
                xml.LoadXml(definition);
            }
            catch
            {
            }
        }


        /// <summary>
        /// Returns checkout process xml form definition.
        /// </summary>
        public string GetXmlDefinition()
        {
            return (xml != null) ? xml.InnerXml : "";
        }


        /// <summary>
        /// Returns DataTable with steps for specified checkout process.
        /// </summary>
        /// <param name="process">Checkout process type</param>
        public DataTable GetDataTableFromXmlDefinition(CheckoutProcessEnum process)
        {
            if (RootNode != null)
            {
                DataTable dtResult = CreateCheckoutProcessTable();

                XmlNodeList stepNodes = GetCheckoutProcessStepNodes(process);
                if (stepNodes != null)
                {
                    foreach (XmlNode stepNode in stepNodes)
                    {
                        DataRow row = CreateCheckoutProcessRow(dtResult, new CheckoutProcessStepInfo(stepNode));
                        dtResult.Rows.Add(row);
                    }
                }

                return dtResult;
            }
            return null;
        }


        /// <summary>
        /// Creates table row with checkout step information.
        /// </summary>
        /// <param name="checkoutProcessTable">Checkout step table</param>
        /// <param name="stepObj">Checkout step information</param>
        private DataRow CreateCheckoutProcessRow(DataTable checkoutProcessTable, CheckoutProcessStepInfo stepObj)
        {
            DataRow row = checkoutProcessTable.NewRow();

            if (stepObj != null)
            {
                DataHelper.SetDataRowValue(row, "Name", stepObj.Name);
                DataHelper.SetDataRowValue(row, "Caption", stepObj.Caption);
                DataHelper.SetDataRowValue(row, "Path", stepObj.ControlPath);
                DataHelper.SetDataRowValue(row, "Icon", stepObj.Icon);
                DataHelper.SetDataRowValue(row, "LiveSite", stepObj.ShowOnLiveSite);
                DataHelper.SetDataRowValue(row, "CMSDeskCustomer", stepObj.ShowInCMSDeskCustomer);
                DataHelper.SetDataRowValue(row, "CMSDeskOrder", stepObj.ShowInCMSDeskOrder);
                DataHelper.SetDataRowValue(row, "CMSDeskOrderItems", stepObj.ShowInCMSDeskOrderItems);
            }

            return row;
        }


        /// <summary>
        /// Returns the empty checkout process table.
        /// </summary>
        private DataTable CreateCheckoutProcessTable()
        {
            DataTable dt = new DataTable();

            // Add specified columns to the table
            dt.Columns.Add(new DataColumn("Name", typeof(string)));
            dt.Columns.Add(new DataColumn("Caption", typeof(string)));
            dt.Columns.Add(new DataColumn("Path", typeof(string)));
            dt.Columns.Add(new DataColumn("Icon", typeof(string)));
            dt.Columns.Add(new DataColumn("LiveSite", typeof(bool)));
            dt.Columns.Add(new DataColumn("CMSDeskCustomer", typeof(bool)));
            dt.Columns.Add(new DataColumn("CMSDeskOrder", typeof(bool)));
            dt.Columns.Add(new DataColumn("CMSDeskOrderItems", typeof(bool)));

            return dt;
        }


        /// <summary>
        /// Returns checkout step nodes of the specified checkout process.
        /// </summary>
        /// <param name="process">Checkout process type</param>
        private XmlNodeList GetCheckoutProcessStepNodes(CheckoutProcessEnum process)
        {
            if (RootNode != null)
            {
                string attributeName = "";

                switch (process)
                {
                    case CheckoutProcessEnum.LiveSite:
                        attributeName = "livesite";
                        break;

                    case CheckoutProcessEnum.CMSDeskOrder:
                        attributeName = "cmsdeskorder";
                        break;

                    case CheckoutProcessEnum.CMSDeskOrderItems:
                        attributeName = "cmsdeskorderitems";
                        break;

                    case CheckoutProcessEnum.CMSDeskCustomer:
                        attributeName = "cmsdeskcustomer";
                        break;

                    default:
                        return RootNode.SelectNodes("step");
                }
                return RootNode.SelectNodes("step[@" + attributeName + "='true']");
            }

            return null;
        }


        /// <summary>
        /// Constructor - creates empty checkout process object.
        /// </summary>
        public CheckoutProcessInfo()
        {
        }


        /// <summary>
        /// Constructor - loads checkout process xml definition.
        /// </summary>
        /// <param name="definition">Checkout process xml definition</param>
        public CheckoutProcessInfo(string definition)
        {
            LoadXmlDefinition(definition);
        }

        #endregion
    }
}