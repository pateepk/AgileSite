using CMS.Reporting;

namespace APIExamples
{
    /// <summary>
    /// Holds reporting API examples.
    /// </summary>
    /// <pageTitle>Reporting</pageTitle>
    internal class Reporting
    {
        /// <summary>
        /// Holds report category API examples.
        /// </summary>
        /// <groupHeading>Report categories</groupHeading>
        private class ReportCategories
        {
            /// <heading>Creating a report category</heading>
            private void CreateReportCategory()
            {
                // Creates a new report category object
                ReportCategoryInfo newCategory = new ReportCategoryInfo();

                // Sets the report category properties
                newCategory.CategoryDisplayName = "New category";
                newCategory.CategoryCodeName = "NewCategory";

                // Saves the report category to the database
                ReportCategoryInfoProvider.SetReportCategoryInfo(newCategory);
            }


            /// <heading>Updating a report category</heading>
            private void GetAndUpdateReportCategory()
            {
                // Gets the report category
                ReportCategoryInfo updateCategory = ReportCategoryInfoProvider.GetReportCategoryInfo("NewCategory");
                if (updateCategory != null)
                {
                    // Updates the report category properties
                    updateCategory.CategoryDisplayName = updateCategory.CategoryDisplayName.ToLower();

                    // Saves the updated report category to the database
                    ReportCategoryInfoProvider.SetReportCategoryInfo(updateCategory);
                }
            }


            /// <heading>Updating multiple report categories</heading>
            private void GetAndBulkUpdateReportCategories()
            {
                // Gets the report categories whose code name starts with 'New'
                var categories = ReportCategoryInfoProvider.GetCategories().WhereStartsWith("CategoryCodeName", "New");

                // Loops through individual report categories
                foreach (ReportCategoryInfo category in categories)
                {
                    // Updates the report category properties
                    category.CategoryDisplayName = category.CategoryDisplayName.ToUpper();

                    // Saves the updated report category to the database
                    ReportCategoryInfoProvider.SetReportCategoryInfo(category);
                }
            }


            /// <heading>Deleting a report category</heading>
            private void DeleteReportCategory()
            {
                // Gets the report category
                ReportCategoryInfo deleteCategory = ReportCategoryInfoProvider.GetReportCategoryInfo("NewCategory");

                if (deleteCategory != null)
                {
                    // Deletes the report category
                    ReportCategoryInfoProvider.DeleteReportCategoryInfo(deleteCategory);
                }
            }
        }


        /// <summary>
        /// Holds report API examples.
        /// </summary>
        /// <groupHeading>Reports</groupHeading>
        private class Reports
        {
            /// <heading>Creating a report</heading>
            private void CreateReport()
            {
                // Gets a parent category for the report
                ReportCategoryInfo category = ReportCategoryInfoProvider.GetReportCategoryInfo("NewCategory");
                if (category != null)
                {
                    // Creates a new report object
                    ReportInfo newReport = new ReportInfo();

                    // Sets the report properties
                    newReport.ReportDisplayName = "New report";
                    newReport.ReportName = "NewReport";
                    newReport.ReportCategoryID = category.CategoryID;
                    newReport.ReportAccess = ReportAccessEnum.All;
                    newReport.ReportLayout = "";
                    newReport.ReportParameters = "";

                    // Saves the report to the database
                    ReportInfoProvider.SetReportInfo(newReport);
                }
            }


            /// <heading>Updating a report</heading>
            private void GetAndUpdateReport()
            {
                // Gets the report
                ReportInfo updateReport = ReportInfoProvider.GetReportInfo("NewReport");
                if (updateReport != null)
                {
                    // Updates the report properties
                    updateReport.ReportDisplayName = updateReport.ReportDisplayName.ToLower();

                    // Saves the modified report to the database
                    ReportInfoProvider.SetReportInfo(updateReport);
                }
            }


            /// <heading>Updating multiple reports</heading>
            private void GetAndBulkUpdateReports()
            {
                // Gets a report category
                ReportCategoryInfo category = ReportCategoryInfoProvider.GetReportCategoryInfo("NewCategory");

                // Gets all reports within the specified category
                var reports = ReportInfoProvider.GetReports().WhereEquals("ReportCategoryID", category.CategoryID);
                
                // Loops through individual reports
                foreach (ReportInfo report in reports)
                {
                    // Updates the report properties
                    report.ReportDisplayName = report.ReportDisplayName.ToUpper();

                    // Saves the modified report to the database
                    ReportInfoProvider.SetReportInfo(report);
                }
            }
            

            /// <heading>Deleting a report</heading>
            private void DeleteReport()
            {
                // Gets the report
                ReportInfo deleteReport = ReportInfoProvider.GetReportInfo("NewReport");

                if (deleteReport != null)
                {
                    // Deletes the report
                    ReportInfoProvider.DeleteReportInfo(deleteReport);
                }
            }
        }


        /// <summary>
        /// Holds report component API examples.
        /// </summary>
        /// <groupHeading>Report components (graphs, tables, values)</groupHeading>
        private class ReportComponents
        {
            /// <heading>Creating report components</heading>
            private void CreateReportComponents()
            {
                // Gets the report
                ReportInfo report = ReportInfoProvider.GetReportInfo("NewReport");
                if (report != null)
                {
                    // Creates a new report graph object
                    ReportGraphInfo newGraph = new ReportGraphInfo
                    {
                        // Sets the graph properties
                        GraphDisplayName = "New graph",
                        GraphName = "NewGraph",
                        GraphQuery = "SELECT TOP 10 DocumentName, DocumentID FROM CMS_Document",
                        GraphReportID = report.ReportID,
                        GraphQueryIsStoredProcedure = false,
                        GraphType = "bar"
                    };                    

                    // Saves the report graph to the database
                    ReportGraphInfoProvider.SetReportGraphInfo(newGraph);

                    // Creates a new report table object
                    ReportTableInfo newTable = new ReportTableInfo
                    {
                        // Sets the table properties
                        TableDisplayName = "New table",
                        TableName = "NewTable",
                        TableQuery = "SELECT TOP 10 DocumentName, DocumentID FROM CMS_Document",
                        TableReportID = report.ReportID,
                        TableQueryIsStoredProcedure = false
                    };
                    
                    // Saves the report table to the database
                    ReportTableInfoProvider.SetReportTableInfo(newTable);

                    // Creates a new report value object
                    ReportValueInfo newValue = new ReportValueInfo
                    {
                        // Sets the report value properties
                        ValueDisplayName = "New value",
                        ValueName = "NewValue",
                        ValueQuery = "SELECT COUNT(DocumentName) FROM CMS_Document",
                        ValueQueryIsStoredProcedure = false,
                        ValueReportID = report.ReportID
                    };
                   
                    // Saves the report value to the database
                    ReportValueInfoProvider.SetReportValueInfo(newValue);
                }
            }


            /// <heading>Updating a report component</heading>
            private void GetAndUpdateReportComponent()
            {
                // Gets a report graph
                ReportGraphInfo updateGraph = ReportGraphInfoProvider.GetReportGraphInfo("NewGraph");
                if (updateGraph != null)
                {
                    // Updates the graph properties
                    updateGraph.GraphDisplayName = updateGraph.GraphDisplayName.ToLower();

                    // Saves the updated report graph to the database
                    ReportGraphInfoProvider.SetReportGraphInfo(updateGraph);
                }
            }


            /// <heading>Updating multiple report components</heading>
            private void GetAndBulkUpdateReportComponents()
            {
                // Gets a report
                ReportInfo report = ReportInfoProvider.GetReportInfo("NewReport");                               

                // Gets all graphs defined for the specified report
                var graphs = ReportGraphInfoProvider.GetReportGraphs().WhereEquals("GraphReportID", report.ReportID);
                
                // Loops through individual report graphs
                foreach (ReportGraphInfo graph in graphs)
                {
                    // Updates the graph properties
                    graph.GraphDisplayName = graph.GraphDisplayName.ToUpper();

                    // Saves the updated report graph to the database
                    ReportGraphInfoProvider.SetReportGraphInfo(graph);
                }
            }


            /// <heading>Adding components into the layout of a report</heading>
            private void InsertElementsToLayout()
            {
                // Gets the report
                ReportInfo report = ReportInfoProvider.GetReportInfo("NewReport");
                if (report != null)
                {
                    // Gets a report graph
                    ReportGraphInfo graph = ReportGraphInfoProvider.GetReportGraphInfo("NewGraph");
                    if (graph != null)
                    {
                        // Inserts the graph into the report's layout
                        report.ReportLayout += "<br/>%%control:Report" + ReportItemType.Graph + "?" + report.ReportName + "." + graph.GraphName + "%%<br/>";
                    }

                    // Gets a report table
                    ReportTableInfo table = ReportTableInfoProvider.GetReportTableInfo("NewTable");
                    if (table != null)
                    {
                        // Inserts the table into the report's layout
                        report.ReportLayout += "<br/>%%control:Report" + ReportItemType.Table + "?" + report.ReportName + "." + table.TableName + "%%<br/>";
                    }

                    // Gets a report value
                    ReportValueInfo value = ReportValueInfoProvider.GetReportValueInfo("NewValue");
                    if (value != null)
                    {
                        // Inserts the value into the report's layout
                        report.ReportLayout += "<br/>%%control:Report" + ReportItemType.Value + "?" + report.ReportName + "." + value.ValueName + "%%<br/>";
                    }

                    // Saves the modified report to the database
                    ReportInfoProvider.SetReportInfo(report);
                }
            }


            /// <heading>Deleting a report component</heading>
            private void DeleteReportComponent()
            {
                // Gets a report graph
                ReportGraphInfo deleteGraph = ReportGraphInfoProvider.GetReportGraphInfo("NewGraph");

                if (deleteGraph != null)
                {
                    // Deletes the report graph
                    ReportGraphInfoProvider.DeleteReportGraphInfo(deleteGraph);
                }
            }
        }
    }
}
